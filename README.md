# JobApplier - Backend API

AI Resume & Cover Letter Builder - ASP.NET Core Web API (.NET 8)

## Architecture

Clean Architecture with 4 distinct layers:
- **Api**: HTTP entry point, controllers, middleware
- **Application**: Business logic, use cases, service interfaces
- **Domain**: Core business entities, rules, no external dependencies
- **Infrastructure**: Data access, external service integrations

## Project Structure

```
src/
├── JobApplier.Api                  # Web API (Controllers, Middleware, Configuration)
├── JobApplier.Application          # Business logic (Services, DTOs, Interfaces)
├── JobApplier.Domain               # Business entities (User, Resume, etc.)
└── JobApplier.Infrastructure       # Data & external services (DbContext, APIs)
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code
- SQL Server (LocalDB for development)

### Setup

1. **Restore dependencies**
   ```powershell
   dotnet restore
   ```

2. **Configure JWT secrets**
   Edit `appsettings.Development.json`:
   ```json
   {
     "Jwt": {
       "SecretKey": "your-secret-key-here-min-32-chars",
       "Issuer": "JobApplier",
       "Audience": "JobApplierClient"
     }
   }
   ```
   > ⚠️ NEVER commit secrets. Use `User Secrets` or environment variables in production.

3. **Configure database**
   Update connection string in `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=JobApplierDb;Trusted_Connection=true;"
     }
   }
   ```

4. **Apply migrations** (TODO: Create initial migration)
   ```powershell
   dotnet ef database update -p src/JobApplier.Infrastructure
   ```

5. **Run the API**
   ```powershell
   dotnet run -p src/JobApplier.Api
   ```
   
   API will start at `https://localhost:5001`
   Swagger docs at `https://localhost:5001/swagger`

## API Endpoints

### Health Check (Public)
- `GET /api/health` - API health status

### Authentication (TODO)
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh-token`

### Resumes (TODO)
- `GET /api/resumes`
- `POST /api/resumes/upload`
- `PUT /api/resumes/{id}`
- `DELETE /api/resumes/{id}`

### Cover Letters (TODO)
- `POST /api/cover-letters/generate`
- `GET /api/cover-letters/{id}`

## Configuration

### Environment-based settings
- **Development**: `appsettings.Development.json` (verbose logging, CORS relaxed)
- **Production**: `appsettings.Production.json` (secrets from environment variables)

### Key Configuration Sections
- `Jwt`: Authentication token settings
- `ConnectionStrings`: Database connection
- `Cors`: Allowed origins
- `Logging`: Log levels and sinks

## Security

- ✅ JWT Bearer authentication
- ✅ Global exception handling
- ✅ CORS configuration
- TODO: Rate limiting
- TODO: File upload validation & virus scanning
- TODO: OpenAI API key management (Key Vault)
- TODO: Input sanitization
- TODO: Audit logging

## TODO Items

### Immediate
- [ ] Create initial EF Core migration
- [ ] Implement User repository
- [ ] Add strongly-typed configuration classes (JwtSettings, etc.)
- [ ] Setup external service clients (OpenAI, OCR)

### Short-term
- [ ] Implement Resume upload & parsing
- [ ] Implement Cover Letter generation
- [ ] Add file upload security
- [ ] Implement authentication endpoints

### Long-term
- [ ] Implement health checks for dependencies
- [ ] Add distributed tracing (correlation IDs)
- [ ] Setup Application Insights
- [ ] Implement caching strategy
- [ ] Add comprehensive unit & integration tests

## Development Commands

```powershell
# Build solution
dotnet build

# Run tests
dotnet test

# Add migration (Infrastructure project)
dotnet ef migrations add InitialCreate -p src/JobApplier.Infrastructure

# Update database
dotnet ef database update -p src/JobApplier.Infrastructure

# Run API
dotnet run -p src/JobApplier.Api

# Format code
dotnet format
```

## Dependencies

### Core
- `Microsoft.AspNetCore.Authentication.JwtBearer`: JWT authentication
- `Swashbuckle.AspNetCore`: Swagger/OpenAPI documentation
- `Serilog.AspNetCore`: Structured logging
- `Microsoft.EntityFrameworkCore`: ORM
- `Microsoft.EntityFrameworkCore.SqlServer`: SQL Server provider

### TODO
- `OpenAI`: GPT integration
- `Tesseract.Net.Core`: OCR
- `Azure.Storage.Blobs`: File storage
- `FluentValidation`: Input validation
- `MediatR`: Command/Query pattern (optional)

## Notes

- All TODOs in code represent incomplete features or security considerations
- Configuration uses environment-based separation (Development/Production)
- JWT secrets must be configured securely (never hardcoded in production)
- Database migrations are managed via EF Core CLI
# ApplyIQ
