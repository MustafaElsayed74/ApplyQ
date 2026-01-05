namespace JobApplier.Application.Interfaces;

/// <summary>
/// Interface for repository operations on JobDescription entities.
/// </summary>
public interface IJobDescriptionRepository
{
    /// <summary>
    /// Get a job description by ID.
    /// </summary>
    Task<Domain.Entities.JobDescription?> GetByIdAsync(Guid jobDescriptionId);

    /// <summary>
    /// Get all job descriptions for a specific user.
    /// </summary>
    Task<List<Domain.Entities.JobDescription>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Add a new job description to the repository.
    /// </summary>
    Task AddAsync(Domain.Entities.JobDescription jobDescription);

    /// <summary>
    /// Update an existing job description.
    /// </summary>
    Task UpdateAsync(Domain.Entities.JobDescription jobDescription);

    /// <summary>
    /// Delete a job description.
    /// </summary>
    Task DeleteAsync(Domain.Entities.JobDescription jobDescription);

    /// <summary>
    /// Persist changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
