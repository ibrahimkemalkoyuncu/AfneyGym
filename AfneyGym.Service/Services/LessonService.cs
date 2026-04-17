using AfneyGym.Data.Context;
using AfneyGym.Domain.Entities;
using AfneyGym.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AfneyGym.Service.Services;

public class LessonService : ILessonService
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public LessonService(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<List<Lesson>> GetAllWithTrainersAsync()
    {
        return await _context.Lessons
            .Include(l => l.Trainer)
            .Include(l => l.Attendees)
            .Where(l => !l.IsDeleted)
            .OrderBy(l => l.StartTime)
            .ToListAsync();
    }

    public async Task<Lesson?> GetByIdAsync(Guid id)
    {
        return await _context.Lessons.Include(l => l.Trainer).FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> CreateAsync(Lesson lesson)
    {
        // MÜHENDİSLİK KONTROLÜ: Eğitmen Çakışma Analizi (Conflict Check)
        // Aynı eğitmenin aynı zaman diliminde başka dersi var mı?
        var isBusy = await _context.Lessons.AnyAsync(l =>
            l.TrainerId == lesson.TrainerId &&
            lesson.StartTime < l.EndTime &&
            l.StartTime < lesson.EndTime);

        if (isBusy) return false; // Eğitmen meşgul, kayıt başarısız.

        await _context.Lessons.AddAsync(lesson);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(Lesson lesson)
    {
        var isBusy = await _context.Lessons.AnyAsync(l =>
            l.Id != lesson.Id &&
            !l.IsDeleted &&
            l.TrainerId == lesson.TrainerId &&
            lesson.StartTime < l.EndTime &&
            l.StartTime < lesson.EndTime);

        if (isBusy) return false;

        _context.Lessons.Update(lesson);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var lesson = await GetByIdAsync(id);
        if (lesson == null) return false;

        _context.Lessons.Remove(lesson);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<JoinLessonStatus> JoinLessonAsync(Guid lessonId, Guid userId)
    {
        // 1. Üyenin Aktif Aboneliği Var mı? (Mühendislik Kısıtlaması)
        var hasActiveSubscription = await _context.Subscriptions
            .AnyAsync(s => s.UserId == userId &&
                           s.Status == SubscriptionStatus.Active &&
                           s.EndDate > DateTime.Now &&
                           !s.IsDeleted);

        if (!hasActiveSubscription) return JoinLessonStatus.NoActiveSubscription;

        // 2. Dersi ve mevcut katılımcıları getir
        var lesson = await _context.Lessons
            .Include(l => l.Attendees)
            .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted);

        if (lesson == null) return JoinLessonStatus.LessonNotFound;

        // 3. Mükerrer Kayıt Kontrolü
        var alreadyJoined = lesson.Attendees.Any(a => a.UserId == userId && !a.IsDeleted);
        if (alreadyJoined) return JoinLessonStatus.AlreadyJoined;

        // 4. Zaman Çakışması Kontrolü
        var existingLessons = await _context.LessonAttendees
            .Include(a => a.Lesson)
            .Where(a => a.UserId == userId
                        && !a.IsDeleted
                        && a.LessonId != lessonId
                        && a.Lesson != null
                        && !a.Lesson.IsDeleted)
            .Select(a => a.Lesson!)
            .ToListAsync();

        var hasTimeConflict = existingLessons.Any(existingLesson =>
            lesson.StartTime < existingLesson.EndTime && lesson.EndTime > existingLesson.StartTime);

        if (hasTimeConflict) return JoinLessonStatus.TimeConflict;

        // 5. Kontenjan Kontrolü
        var activeAttendeeCount = lesson.Attendees.Count(a => !a.IsDeleted);
        if (activeAttendeeCount >= lesson.Capacity)
            return JoinLessonStatus.CapacityFull;

        // 6. Katılım Kaydı Oluştur
        var attendee = new LessonAttendee
        {
            LessonId = lessonId,
            UserId = userId,
            CreatedAt = DateTime.Now
        };

        await _context.LessonAttendees.AddAsync(attendee);
        await _context.SaveChangesAsync();

        await _notificationService.SendToUserAsync(
            userId,
            "Ders Kaydi Basarili",
            $"{lesson.Name} dersine kaydiniz olusturuldu.",
            "lesson");

        return JoinLessonStatus.Success;
    }

    public async Task<CancelJoinStatus> CancelJoinAsync(Guid lessonId, Guid userId)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Attendees)
            .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted);

        if (lesson == null)
            return CancelJoinStatus.LessonNotFound;

        var attendance = lesson.Attendees.FirstOrDefault(a => a.UserId == userId && !a.IsDeleted);
        if (attendance == null)
            return CancelJoinStatus.NotJoined;

        _context.LessonAttendees.Remove(attendance);
        await _context.SaveChangesAsync();

        await _notificationService.SendToUserAsync(
            userId,
            "Ders Kaydi Iptal Edildi",
            $"{lesson.Name} dersi icin kaydiniz iptal edildi.",
            "lesson");

        return CancelJoinStatus.Success;
    }

    public async Task<List<LessonAttendee>> GetLessonAttendeesAsync(Guid lessonId)
    {
        return await _context.LessonAttendees
            .Include(la => la.User)
            .Where(la => la.LessonId == lessonId && !la.IsDeleted)
            .OrderByDescending(la => la.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> MarkAttendanceAsync(Guid lessonAttendeeId, bool isAttended)
    {
        var attendee = await _context.LessonAttendees.FindAsync(lessonAttendeeId);
        if (attendee == null) return false;

        attendee.IsAttended = isAttended;
        attendee.UpdatedAt = DateTime.Now;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> GetAvailableSpots(Guid lessonId)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Attendees)
            .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted);

        if (lesson == null) return 0;
        var activeAttendeeCount = lesson.Attendees.Count(a => !a.IsDeleted);
        return Math.Max(0, lesson.Capacity - activeAttendeeCount);
    }

    public async Task<Lesson?> GetByIdWithAttendeesAsync(Guid id)
    {
        return await _context.Lessons
            .Include(l => l.Trainer)
            .Include(l => l.Attendees)
            .ThenInclude(la => la.User)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }
}