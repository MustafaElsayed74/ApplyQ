namespace JobApplier.Infrastructure.Persistence;

using JobApplier.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Entity Framework Core database context with soft delete support
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<CV> CVs => Set<CV>();
    public DbSet<JobDescription> JobDescriptions => Set<JobDescription>();
    public DbSet<CoverLetter> CoverLetters => Set<CoverLetter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(254);
            entity.HasIndex(e => e.Email)
                .IsUnique();
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.PasswordHash)
                .IsRequired();
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);
            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.DeletedAt)
                .IsRequired(false);

            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.HasMany(e => e.RefreshTokens)
                .WithOne()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.CVs)
                .WithOne(cv => cv.User)
                .HasForeignKey(cv => cv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.JobDescriptions)
                .WithOne(jd => jd.User)
                .HasForeignKey(jd => jd.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.CoverLetters)
                .WithOne(cl => cl.User)
                .HasForeignKey(cl => cl.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token)
                .IsRequired();
            entity.HasIndex(e => e.Token)
                .IsUnique();
            entity.Property(e => e.UserId)
                .IsRequired();
            entity.Property(e => e.ExpiresAt)
                .IsRequired();
            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAdd();
        });

        // Configure CV entity
        modelBuilder.Entity<CV>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId)
                .IsRequired();
            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.FileType)
                .IsRequired()
                .HasMaxLength(10);
            entity.Property(e => e.FilePath)
                .IsRequired();
            entity.Property(e => e.FileSizeBytes)
                .IsRequired();
            entity.Property(e => e.ExtractedText)
                .IsRequired();
            entity.Property(e => e.ParsedDataJson)
                .HasDefaultValue(string.Empty);
            entity.Property(e => e.IsParsed)
                .HasDefaultValue(false);
            entity.Property(e => e.FileChecksum)
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.DeletedAt)
                .IsRequired(false);

            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Indexes on foreign keys
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_CV_UserId");
            entity.HasIndex(e => new { e.UserId, e.FileChecksum })
                .HasDatabaseName("IX_CV_UserId_FileChecksum");
            entity.HasIndex(e => e.IsParsed)
                .HasDatabaseName("IX_CV_IsParsed");

            // Relationship to User
            entity.HasOne(e => e.User)
                .WithMany(u => u.CVs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship to CoverLetters
            entity.HasMany(e => e.CoverLetters)
                .WithOne(cl => cl.CV)
                .HasForeignKey(cl => cl.CVId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure JobDescription entity
        modelBuilder.Entity<JobDescription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId)
                .IsRequired();
            entity.Property(e => e.Description)
                .IsRequired();
            entity.Property(e => e.SourceType)
                .IsRequired()
                .HasMaxLength(10);
            entity.Property(e => e.SourceImagePath)
                .HasMaxLength(500);
            entity.Property(e => e.SourceImageFileName)
                .HasMaxLength(255);
            entity.Property(e => e.IsOCRExtracted)
                .HasDefaultValue(false);
            entity.Property(e => e.JobTitle)
                .HasMaxLength(255);
            entity.Property(e => e.CompanyName)
                .HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.DeletedAt)
                .IsRequired(false);

            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Indexes on foreign keys
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_JobDescription_UserId");
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_JobDescription_CreatedAt");
            entity.HasIndex(e => e.IsOCRExtracted)
                .HasDatabaseName("IX_JobDescription_IsOCRExtracted");

            // Relationship to User
            entity.HasOne(e => e.User)
                .WithMany(u => u.JobDescriptions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship to CoverLetters
            entity.HasMany(e => e.CoverLetters)
                .WithOne(cl => cl.JobDescription)
                .HasForeignKey(cl => cl.JobDescriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure CoverLetter entity
        modelBuilder.Entity<CoverLetter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId)
                .IsRequired();
            entity.Property(e => e.CVId)
                .IsRequired();
            entity.Property(e => e.JobDescriptionId)
                .IsRequired();
            entity.Property(e => e.GeneratedContent)
                .IsRequired();
            entity.Property(e => e.WordCount)
                .IsRequired();
            entity.Property(e => e.TokensUsed)
                .IsRequired();
            entity.Property(e => e.Model)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Notes)
                .HasMaxLength(1000);
            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.DeletedAt)
                .IsRequired(false);

            // Soft delete query filter
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Indexes on foreign keys
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_CoverLetter_UserId");
            entity.HasIndex(e => e.CVId)
                .HasDatabaseName("IX_CoverLetter_CVId");
            entity.HasIndex(e => e.JobDescriptionId)
                .HasDatabaseName("IX_CoverLetter_JobDescriptionId");
            entity.HasIndex(e => new { e.CVId, e.JobDescriptionId })
                .IsUnique()
                .HasDatabaseName("IX_CoverLetter_CVId_JobDescriptionId_Unique");
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_CoverLetter_CreatedAt");

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany(u => u.CoverLetters)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CV)
                .WithMany(cv => cv.CoverLetters)
                .HasForeignKey(e => e.CVId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.JobDescription)
                .WithMany(jd => jd.CoverLetters)
                .HasForeignKey(e => e.JobDescriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
