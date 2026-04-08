using AfneyGym.Domain.Entities;

namespace AfneyGym.Domain.Interfaces;

public interface ILessonService
{
    Task<List<Lesson>> GetAllWithTrainersAsync();
    Task<Lesson?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(Lesson lesson);
    Task<bool> UpdateAsync(Lesson lesson);
    Task<bool> DeleteAsync(Guid id);
}