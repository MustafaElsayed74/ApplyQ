using JobApplier.Application.Interfaces;
using JobApplier.Domain.Entities;
using JobApplier.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobApplier.Infrastructure.Repositories;

/// <summary>
/// Repository for JobDescription entity operations.
/// </summary>
public class JobDescriptionRepository : IJobDescriptionRepository
{
    private readonly ApplicationDbContext _context;

    public JobDescriptionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JobDescription?> GetByIdAsync(Guid jobDescriptionId)
    {
        return await _context.JobDescriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(jd => jd.Id == jobDescriptionId);
    }

    public async Task<List<JobDescription>> GetByUserIdAsync(Guid userId)
    {
        return await _context.JobDescriptions
            .AsNoTracking()
            .Where(jd => jd.UserId == userId)
            .OrderByDescending(jd => jd.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(JobDescription jobDescription)
    {
        await _context.JobDescriptions.AddAsync(jobDescription);
    }

    public async Task UpdateAsync(JobDescription jobDescription)
    {
        _context.JobDescriptions.Update(jobDescription);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(JobDescription jobDescription)
    {
        _context.JobDescriptions.Remove(jobDescription);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
