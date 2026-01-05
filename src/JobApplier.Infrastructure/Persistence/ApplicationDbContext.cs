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
    }
}
