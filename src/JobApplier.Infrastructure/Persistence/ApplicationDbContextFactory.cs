namespace JobApplier.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Design-time factory for EF Core migrations
/// Allows migrations to be created without connecting to the database
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use a dummy connection string for design-time operations
        // The actual connection string will be used at runtime
        var connectionString = "Server=localhost;Port=3306;Database=applyiq_db;User=springstudent;Password=springstudent;";

        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.Parse("8.0.0-mysql"),
            options => options.EnableRetryOnFailure());

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
