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
            .OrderByDescending(l => l.StartTime)
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
}