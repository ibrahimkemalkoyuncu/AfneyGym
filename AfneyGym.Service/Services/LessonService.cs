using AfneyGym.Data.Context;
using AfneyGym.Domain.Entities;
using AfneyGym.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AfneyGym.Service.Services;

public class LessonService : ILessonService
{
    private readonly AppDbContext _context;

    public LessonService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Lesson>> GetAllWithTrainersAsync()
    {
        return await _context.Lessons
            .Include(l => l.Trainer)
            .Include(l => l.Attendees)
            .Where(l => !l.IsDeleted)
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

    public async Task<bool> JoinLessonAsync(Guid lessonId, Guid userId)
    {
        // 1. Üyenin Aktif Aboneliği Var mı? (Mühendislik Kısıtlaması)
        var hasActiveSubscription = await _context.Subscriptions
            .AnyAsync(s => s.UserId == userId &&
                           s.Status == SubscriptionStatus.Active &&
                           s.EndDate > DateTime.Now &&
                           !s.IsDeleted);

        if (!hasActiveSubscription) return false; // Abonelik yok, katılım başarısız.   

        // 2. Dersi ve mevcut katılımcıları getir
        var lesson = await _context.Lessons
            .Include(l => l.Attendees)
            .FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted);

        if (lesson == null) return false;

        // 3. Kontenjan Kontrolü
        if (lesson.Attendees.Count >= lesson.Capacity)
            return false;

        // 4. Mükerrer Kayıt Kontrolü
        var alreadyJoined = lesson.Attendees.Any(a => a.UserId == userId);
        if (alreadyJoined) return false;

        // 5. Katılım Kaydı Oluştur
        var attendee = new LessonAttendee
        {
            LessonId = lessonId,
            UserId = userId,
            CreatedAt = DateTime.Now
        };

        await _context.LessonAttendees.AddAsync(attendee);
        await _context.SaveChangesAsync();
        return true;
    }

    
}