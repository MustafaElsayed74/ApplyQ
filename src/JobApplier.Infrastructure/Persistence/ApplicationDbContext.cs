namespace JobApplier.Infrastructure.Persistence;

using JobApplier.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Entity Framework Core database context
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

            entity.HasMany(e => e.RefreshTokens)
                .WithOne()
                .HasForeignKey(rt => rt.UserId)
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

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.FileChecksum });
            entity.HasIndex(e => e.IsParsed);

            // Relationship to User
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
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

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.IsOCRExtracted);

            // Relationship to User
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
