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
    // TODO: Add DbSet for other entities (Resume, CoverLetter, Document, etc.)

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TODO: Configure entity mappings
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
