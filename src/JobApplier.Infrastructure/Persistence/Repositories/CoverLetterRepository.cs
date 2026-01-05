using JobApplier.Application.Interfaces;
using JobApplier.Domain.Entities;
using JobApplier.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobApplier.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for CoverLetter entity operations.
/// </summary>
public class CoverLetterRepository : ICoverLetterRepository
{
    private readonly ApplicationDbContext _context;

    public CoverLetterRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CoverLetter?> GetByIdAsync(Guid coverLetterId)
    {
        return await _context.CoverLetters
            .AsNoTracking()
            .FirstOrDefaultAsync(cl => cl.Id == coverLetterId);
    }

    public async Task<List<CoverLetter>> GetByUserIdAsync(Guid userId)
    {
        return await _context.CoverLetters
            .AsNoTracking()
            .Where(cl => cl.UserId == userId)
            .OrderByDescending(cl => cl.CreatedAt)
            .ToListAsync();
    }

    public async Task<CoverLetter?> GetByCVAndJobDescriptionAsync(Guid cvId, Guid jobDescriptionId)
    {
        return await _context.CoverLetters
            .AsNoTracking()
            .FirstOrDefaultAsync(cl => cl.CVId == cvId && cl.JobDescriptionId == jobDescriptionId);
    }

    public async Task<List<CoverLetter>> GetByCVIdAsync(Guid cvId)
    {
        return await _context.CoverLetters
            .AsNoTracking()
            .Where(cl => cl.CVId == cvId)
            .OrderByDescending(cl => cl.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<CoverLetter>> GetByJobDescriptionIdAsync(Guid jobDescriptionId)
    {
        return await _context.CoverLetters
            .AsNoTracking()
            .Where(cl => cl.JobDescriptionId == jobDescriptionId)
            .OrderByDescending(cl => cl.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(CoverLetter coverLetter)
    {
        await _context.CoverLetters.AddAsync(coverLetter);
    }

    public async Task UpdateAsync(CoverLetter coverLetter)
    {
        _context.CoverLetters.Update(coverLetter);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(CoverLetter coverLetter)
    {
        _context.CoverLetters.Remove(coverLetter);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
