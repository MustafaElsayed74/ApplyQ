namespace JobApplier.Infrastructure.Persistence.Repositories;

using JobApplier.Application.Interfaces;
using JobApplier.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// CV repository implementation
/// </summary>
public sealed class CVRepository : ICVRepository
{
    private readonly ApplicationDbContext _context;

    public CVRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<CV?> GetByIdAsync(Guid cvId, CancellationToken cancellationToken = default)
    {
        return await _context.CVs
            .AsNoTracking()
            .FirstOrDefaultAsync(cv => cv.Id == cvId, cancellationToken);
    }

    public async Task<List<CV>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.CVs
            .AsNoTracking()
            .Where(cv => cv.UserId == userId)
            .OrderByDescending(cv => cv.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CV?> GetByChecksumAsync(
        Guid userId,
        string checksum,
        CancellationToken cancellationToken = default)
    {
        return await _context.CVs
            .AsNoTracking()
            .FirstOrDefaultAsync(cv => cv.UserId == userId && cv.FileChecksum == checksum, cancellationToken);
    }

    public async Task AddAsync(CV cv, CancellationToken cancellationToken = default)
    {
        await _context.CVs.AddAsync(cv, cancellationToken);
    }

    public async Task UpdateAsync(CV cv, CancellationToken cancellationToken = default)
    {
        _context.CVs.Update(cv);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid cvId, CancellationToken cancellationToken = default)
    {
        var cv = await _context.CVs.FirstOrDefaultAsync(cv => cv.Id == cvId, cancellationToken);
        if (cv != null)
        {
            _context.CVs.Remove(cv);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
