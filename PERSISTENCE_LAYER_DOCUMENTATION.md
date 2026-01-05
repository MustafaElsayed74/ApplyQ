# Entity Framework Core Persistence Layer Documentation

## Overview

The persistence layer implements a clean, well-structured Entity Framework Core setup with proper relationships, soft deletes, indexing, and query filtering.

## Entity Relationships

### User (Root Aggregate)
```
User (1)
├── RefreshTokens (Many)
├── CVs (Many)
├── JobDescriptions (Many)
└── CoverLetters (Many)
```

**Cascade Delete**: ✅ Enabled
- When a user is deleted, all related CVs, JobDescriptions, and CoverLetters are deleted
- RefreshTokens are also cascade deleted

### CV (Aggregate Root)
```
CV (1)
├── User (1)
└── CoverLetters (Many)
```

**Cascade Delete**: ✅ Enabled
- When a CV is deleted, all related CoverLetters are deleted

### JobDescription (Aggregate Root)
```
JobDescription (1)
├── User (1)
└── CoverLetters (Many)
```

**Cascade Delete**: ✅ Enabled
- When a JobDescription is deleted, all related CoverLetters are deleted

### CoverLetter (Aggregate Root)
```
CoverLetter (Many-to-1)
├── User (1)
├── CV (1)
└── JobDescription (1)
```

**Cascade Delete**: ✅ Enabled
- Deletion cascades from User, CV, and JobDescription

## Database Schema

### Users Table
```sql
CREATE TABLE Users (
    Id GUID PRIMARY KEY,
    Email VARCHAR(254) UNIQUE NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    PasswordHash VARCHAR(MAX) NOT NULL,
    IsActive BIT DEFAULT 1,
    EmailConfirmedAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL,
    DeletedAt DATETIME NULL
);
```

**Indexes:**
- Unique index on `Email`
- Soft delete filter: `WHERE DeletedAt IS NULL`

### RefreshTokens Table
```sql
CREATE TABLE RefreshTokens (
    Id GUID PRIMARY KEY,
    Token VARCHAR(MAX) UNIQUE NOT NULL,
    UserId GUID NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

**Indexes:**
- Unique index on `Token`
- Foreign key index on `UserId`

### CVs Table
```sql
CREATE TABLE CVs (
    Id GUID PRIMARY KEY,
    UserId GUID NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FileType VARCHAR(10) NOT NULL,
    FilePath VARCHAR(MAX) NOT NULL,
    FileSizeBytes BIGINT NOT NULL,
    ExtractedText VARCHAR(MAX) NOT NULL,
    ParsedDataJson VARCHAR(MAX) DEFAULT '',
    IsParsed BIT DEFAULT 0,
    ParsedAt DATETIME NULL,
    FileChecksum VARCHAR(64) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL,
    DeletedAt DATETIME NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

**Indexes:**
- `IX_CV_UserId` - Foreign key lookup
- `IX_CV_UserId_FileChecksum` - Composite for deduplication checks
- `IX_CV_IsParsed` - Filter by parsing status
- Soft delete filter: `WHERE DeletedAt IS NULL`

### JobDescriptions Table
```sql
CREATE TABLE JobDescriptions (
    Id GUID PRIMARY KEY,
    UserId GUID NOT NULL,
    Description VARCHAR(MAX) NOT NULL,
    SourceType VARCHAR(10) NOT NULL,
    SourceImagePath VARCHAR(500) NULL,
    SourceImageFileName VARCHAR(255) NULL,
    SourceImageSizeBytes BIGINT NULL,
    IsOCRExtracted BIT DEFAULT 0,
    JobTitle VARCHAR(255) NULL,
    CompanyName VARCHAR(255) NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL,
    DeletedAt DATETIME NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
```

**Indexes:**
- `IX_JobDescription_UserId` - Foreign key lookup
- `IX_JobDescription_CreatedAt` - Chronological sorting
- `IX_JobDescription_IsOCRExtracted` - Filter by extraction method
- Soft delete filter: `WHERE DeletedAt IS NULL`

### CoverLetters Table
```sql
CREATE TABLE CoverLetters (
    Id GUID PRIMARY KEY,
    UserId GUID NOT NULL,
    CVId GUID NOT NULL,
    JobDescriptionId GUID NOT NULL,
    GeneratedContent VARCHAR(MAX) NOT NULL,
    WordCount INT NOT NULL,
    TokensUsed INT NOT NULL,
    Model VARCHAR(100) NOT NULL,
    Notes VARCHAR(1000) NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL,
    DeletedAt DATETIME NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (CVId) REFERENCES CVs(Id) ON DELETE CASCADE,
    FOREIGN KEY (JobDescriptionId) REFERENCES JobDescriptions(Id) ON DELETE CASCADE
);
```

**Indexes:**
- `IX_CoverLetter_UserId` - Lookup by user
- `IX_CoverLetter_CVId` - Lookup by CV
- `IX_CoverLetter_JobDescriptionId` - Lookup by job description
- `IX_CoverLetter_CVId_JobDescriptionId_Unique` - Prevents duplicate generation
- `IX_CoverLetter_CreatedAt` - Chronological sorting
- Soft delete filter: `WHERE DeletedAt IS NULL`

## Soft Delete Implementation

### Design Pattern
All entities inherit from the `Entity` base class which provides:
- `DeletedAt` property (nullable DateTime)
- `Delete()` method (soft delete)
- `Restore()` method (restore deleted)
- `IsDeleted` property (read-only check)

### Query Filters
Each entity (except RefreshToken) has a global query filter:
```csharp
entity.HasQueryFilter(e => e.DeletedAt == null);
```

This filter automatically excludes soft-deleted entities from all queries.

### How to Use

**Soft Delete:**
```csharp
var user = await context.Users.FindAsync(userId);
user.Delete(); // Sets DeletedAt = DateTime.UtcNow
await context.SaveChangesAsync();
```

**Query Active Only** (automatic):
```csharp
var users = await context.Users.ToListAsync(); // DeletedAt == null only
```

**Query Including Deleted:**
```csharp
var allUsers = await context.Users.IgnoreQueryFilters().ToListAsync();
```

**Restore Deleted:**
```csharp
var user = await context.Users.IgnoreQueryFilters()
    .FirstOrDefaultAsync(u => u.Id == userId);
user.Restore(); // Sets DeletedAt = null
await context.SaveChangesAsync();
```

## Navigation Properties

### User
```csharp
public virtual ICollection<RefreshToken> RefreshTokens { get; }
public virtual ICollection<CV> CVs { get; }
public virtual ICollection<JobDescription> JobDescriptions { get; }
public virtual ICollection<CoverLetter> CoverLetters { get; }
```

### CV
```csharp
public virtual User? User { get; }
public virtual ICollection<CoverLetter> CoverLetters { get; }
```

### JobDescription
```csharp
public virtual User? User { get; }
public virtual ICollection<CoverLetter> CoverLetters { get; }
```

### CoverLetter
```csharp
public virtual User? User { get; }
public virtual CV? CV { get; }
public virtual JobDescription? JobDescription { get; }
```

## Entity Configuration Summary

| Entity | Soft Delete | Cascade | Navigation | Indexes |
|--------|-------------|---------|-----------|---------|
| User | ✅ | Parent | 4 collections | Email (unique) |
| CV | ✅ | Parent | User + CoverLetters | UserId, Composite, IsParsed |
| JobDescription | ✅ | Parent | User + CoverLetters | UserId, CreatedAt, IsOCRExtracted |
| CoverLetter | ✅ | Parent | User, CV, JobDescription | UserId, CVId, JobDescriptionId, Composite (unique) |
| RefreshToken | No | Cascade | None | Token (unique) |

## Performance Considerations

### Index Strategy
- **Foreign Keys**: Indexed for join performance (`UserId`, `CVId`, `JobDescriptionId`)
- **Composite Indexes**: For common filter combinations
  - CV: `(UserId, FileChecksum)` for deduplication checks
  - CoverLetter: `(CVId, JobDescriptionId)` UNIQUE for duplicate prevention
- **Single Filters**: For common queries
  - CV: `IsParsed` to find unparsed CVs
  - JobDescription: `CreatedAt` for chronological sorting
  - CoverLetter: `CreatedAt` for listing recent

### Query Optimization Tips

**Load Related Data:**
```csharp
// Explicit loading
var cv = await context.CVs
    .Include(c => c.CoverLetters)
    .FirstOrDefaultAsync(c => c.Id == cvId);

// Lazy loading (if enabled)
var coverLetters = cv.CoverLetters; // Auto-loaded
```

**Filter Efficiently:**
```csharp
// Use indexed filters
var unparsedCVs = await context.CVs
    .Where(c => c.UserId == userId && !c.IsParsed)
    .ToListAsync();

// Use composite indexes
var duplicate = await context.CoverLetters
    .FirstOrDefaultAsync(cl => cl.CVId == cvId && 
                               cl.JobDescriptionId == jobId);
```

## Migrations

### Current Migrations
1. `20260105182721_InitialCreate` - Base schema
2. `20260105190636_AddCVUploadFunctionality` - CV entity
3. `20260105192537_AddJobDescriptionFunctionality` - JobDescription entity
4. `20260105193008_AddCoverLetterFunctionality` - CoverLetter entity
5. `20260105193943_EnhancePersistenceLayer` - Soft deletes, navigation, improved indexes

### Creating New Migrations
```bash
dotnet ef migrations add {MigrationName} -p src/JobApplier.Infrastructure -s src/JobApplier.Api
```

### Applying Migrations
```bash
dotnet ef database update -p src/JobApplier.Infrastructure -s src/JobApplier.Api
```

### Rolling Back
```bash
# Revert last migration
dotnet ef database update {PreviousMigrationName}

# Remove unapplied migration
dotnet ef migrations remove
```

## DbContext Configuration

### Connection Setup
The `ApplicationDbContext` is configured in the DI container:

```csharp
// Program.cs
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        configuration.GetConnectionString("DefaultConnection"),
        new MariaDbServerVersion(new Version(10, 6, 0))
    )
);
```

### Available DbSets
```csharp
public DbSet<User> Users { get; }
public DbSet<RefreshToken> RefreshTokens { get; }
public DbSet<CV> CVs { get; }
public DbSet<JobDescription> JobDescriptions { get; }
public DbSet<CoverLetter> CoverLetters { get; }
```

## Repository Pattern

Base repository pattern for data access:

```csharp
public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity); // Soft delete
    Task SaveChangesAsync();
}
```

## Transaction Support

For operations spanning multiple entities:

```csharp
using var transaction = await context.Database.BeginTransactionAsync();
try
{
    // Multiple operations
    var cv = new CV(...);
    context.CVs.Add(cv);
    await context.SaveChangesAsync();

    var coverLetter = new CoverLetter(...);
    context.CoverLetters.Add(coverLetter);
    await context.SaveChangesAsync();

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

## Testing

### In-Memory Database
For unit tests, use an in-memory provider:

```csharp
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase("TestDatabase")
    .Options;

using var context = new ApplicationDbContext(options);
// Add seed data and test
```

### Test Example
```csharp
[Fact]
public async Task SoftDelete_ExcludesFromQueries()
{
    var user = new User("test@example.com", "John", "Doe", "hash");
    context.Users.Add(user);
    await context.SaveChangesAsync();

    user.Delete();
    await context.SaveChangesAsync();

    var found = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
    Assert.Null(found);
}
```

## Best Practices

✅ **DO:**
- Use navigation properties to load related data
- Leverage soft deletes for data recovery
- Use indexes for frequently filtered columns
- Test cascade delete behavior
- Use transactions for multi-entity operations

❌ **DON'T:**
- Query with raw SQL and skip EF Core mapping
- Ignore cascade delete configuration
- Create duplicate indexes
- Load entire collections without filtering
- Assume auto-loading without explicit configuration

## Common Queries

### Get User with All Data
```csharp
var user = await context.Users
    .Include(u => u.CVs)
    .Include(u => u.JobDescriptions)
    .Include(u => u.CoverLetters)
    .FirstOrDefaultAsync(u => u.Id == userId);
```

### Find Unparsed CVs
```csharp
var unparsedCVs = await context.CVs
    .Where(c => c.UserId == userId && !c.IsParsed)
    .OrderByDescending(c => c.CreatedAt)
    .ToListAsync();
```

### Check for Duplicate CoverLetter
```csharp
var exists = await context.CoverLetters
    .AnyAsync(cl => cl.CVId == cvId && cl.JobDescriptionId == jobId);
```

### Get Recent CoverLetters
```csharp
var recent = await context.CoverLetters
    .Where(cl => cl.UserId == userId)
    .OrderByDescending(cl => cl.CreatedAt)
    .Take(10)
    .ToListAsync();
```

### Restore Deleted Entity
```csharp
var deletedCv = await context.CVs
    .IgnoreQueryFilters()
    .FirstOrDefaultAsync(c => c.Id == cvId && c.IsDeleted);
    
if (deletedCv != null)
{
    deletedCv.Restore();
    await context.SaveChangesAsync();
}
```

---

**Status**: ✅ Complete  
**Version**: 1.0  
**Last Updated**: January 5, 2026  
**Migrations**: 5 total (all applied)  
**Build Status**: 0 errors, 2 warnings (non-blocking)
