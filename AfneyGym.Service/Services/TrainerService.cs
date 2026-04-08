using AfneyGym.Data.Context;
using AfneyGym.Domain.Entities;
using AfneyGym.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AfneyGym.Service.Services;

public class TrainerService : ITrainerService
{
    private readonly AppDbContext _context;

    public TrainerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Trainer>> GetAllAsync()
    {
        return await _context.Trainers.OrderBy(t => t.FullName).ToListAsync();
    }

    public async Task<Trainer?> GetByIdAsync(Guid id)
    {
        return await _context.Trainers.FindAsync(id);
    }

    public async Task<bool> CreateAsync(Trainer trainer)
    {
        await _context.Trainers.AddAsync(trainer);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(Trainer trainer)
    {
        _context.Trainers.Update(trainer);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var trainer = await GetByIdAsync(id);
        if (trainer == null) return false;

        _context.Trainers.Remove(trainer);
        return await _context.SaveChangesAsync() > 0;
    }
}