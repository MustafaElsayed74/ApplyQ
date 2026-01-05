namespace JobApplier.Application.Interfaces;

/// <summary>
/// Interface for repository operations on CoverLetter entities.
/// </summary>
public interface ICoverLetterRepository
{
    /// <summary>
    /// Get a cover letter by ID.
    /// </summary>
    Task<Domain.Entities.CoverLetter?> GetByIdAsync(Guid coverLetterId);

    /// <summary>
    /// Get all cover letters for a specific user.
    /// </summary>
    Task<List<Domain.Entities.CoverLetter>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Get cover letter for a specific CV and job description combination.
    /// </summary>
    Task<Domain.Entities.CoverLetter?> GetByCVAndJobDescriptionAsync(Guid cvId, Guid jobDescriptionId);

    /// <summary>
    /// Get all cover letters for a specific CV.
    /// </summary>
    Task<List<Domain.Entities.CoverLetter>> GetByCVIdAsync(Guid cvId);

    /// <summary>
    /// Get all cover letters for a specific job description.
    /// </summary>
    Task<List<Domain.Entities.CoverLetter>> GetByJobDescriptionIdAsync(Guid jobDescriptionId);

    /// <summary>
    /// Add a new cover letter to the repository.
    /// </summary>
    Task AddAsync(Domain.Entities.CoverLetter coverLetter);

    /// <summary>
    /// Update an existing cover letter.
    /// </summary>
    Task UpdateAsync(Domain.Entities.CoverLetter coverLetter);

    /// <summary>
    /// Delete a cover letter.
    /// </summary>
    Task DeleteAsync(Domain.Entities.CoverLetter coverLetter);

    /// <summary>
    /// Persist changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
