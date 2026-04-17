using AfneyGym.Domain.Entities;

namespace AfneyGym.Domain.Interfaces;

public interface ILessonService
{
    Task<List<Lesson>> GetAllWithTrainersAsync();
    Task<Lesson?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(Lesson lesson);
    Task<bool> UpdateAsync(Lesson lesson);
    Task<bool> DeleteAsync(Guid id);
    Task<JoinLessonStatus> JoinLessonAsync(Guid lessonId, Guid userId);
    Task<CancelJoinStatus> CancelJoinAsync(Guid lessonId, Guid userId);
    Task<List<LessonAttendee>> GetLessonAttendeesAsync(Guid lessonId);
    Task<bool> MarkAttendanceAsync(Guid lessonAttendeeId, bool isAttended);
    Task<int> GetAvailableSpots(Guid lessonId);
    Task<Lesson?> GetByIdWithAttendeesAsync(Guid id);
}