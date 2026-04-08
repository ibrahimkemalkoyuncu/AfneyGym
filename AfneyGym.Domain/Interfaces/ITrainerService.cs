using AfneyGym.Domain.Entities;

namespace AfneyGym.Domain.Interfaces;

public interface ITrainerService
{
    Task<List<Trainer>> GetAllAsync();
    Task<Trainer?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(Trainer trainer);
    Task<bool> UpdateAsync(Trainer trainer);
    Task<bool> DeleteAsync(Guid id);
}