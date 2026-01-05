# Architecture Diagrams & Visual Reference

## 1. Clean Architecture Layers

```
┌────────────────────────────────────────────────────┐
│                   PRESENTATION                      │
│         JobApplier.Api (Controllers, DTOs)         │
│  - HTTP endpoints                                   │
│  - Request/Response handling                        │
│  - JWT authentication middleware                    │
│  - Swagger documentation                            │
└────────────────────────────────────────────────────┘
                         ↓
┌────────────────────────────────────────────────────┐
│                   APPLICATION                       │
│     JobApplier.Application (Services, DTOs)        │
│  - Business logic                                   │
│  - Orchestration of domain & infrastructure         │
│  - Application exceptions                           │
│  - Validation rules                                 │
└────────────────────────────────────────────────────┘
                         ↓
┌────────────────────────────────────────────────────┐
│                     DOMAIN                          │
│     JobApplier.Domain (Entities, ValueObjects)     │
│  - Core business rules (NO EXTERNAL DEPENDENCIES)  │
│  - Entities & ValueObjects                          │
│  - Domain events                                    │
│  - Can be used independently                        │
└────────────────────────────────────────────────────┘
                         ↓
┌────────────────────────────────────────────────────┐
│                INFRASTRUCTURE                       │
│  JobApplier.Infrastructure (Database, APIs)        │
│  - EF Core DbContext                                │
│  - Repositories                                     │
│  - External service clients (OpenAI, OCR)          │
│  - File processing                                  │
│  - Configuration providers                          │
└────────────────────────────────────────────────────┘
```

## 2. Dependency Flow (Unidirectional)

```
                    ┌─────────┐
                    │   Api   │
                    └────┬────┘
                         │
                 ┌───────┴───────┐
                 ↓               ↓
            ┌──────────┐  ┌──────────────┐
            │  Application  │  │ Infrastructure │
            └──────────┘  └──────────────┘
                 ↓               ↓
                 └───────┬───────┘
                         ↓
                    ┌─────────┐
                    │ Domain  │
                    └─────────┘
                         ↓
                   ┌───────────┐
                   │  Shared   │
                   │ Utilities │
                   └───────────┘

✓ Api depends on Application & Infrastructure
✓ Application depends on Domain only
✓ Infrastructure depends on Domain only
✓ Domain depends on nothing
✓ All depend on Shared (bottom-most)
```

## 3. Project Structure Tree

```
JobApplier.sln
│
├── 📁 src/
│   │
│   ├── 📦 JobApplier.Api/          (Presentation Layer)
│   │   ├── 📁 Controllers/
│   │   │   ├── BaseController.cs
│   │   │   └── HealthController.cs
│   │   ├── 📁 Extensions/
│   │   │   ├── AuthenticationExtensions.cs
│   │   │   ├── SwaggerExtensions.cs
│   │   │   └── DependencyInjectionExtensions.cs
│   │   ├── 📁 Middleware/
│   │   │   └── GlobalExceptionHandlingMiddleware.cs
│   │   ├── Program.cs               (Startup)
│   │   ├── appsettings.*.json       (Configuration)
│   │   └── JobApplier.Api.csproj
│   │
│   ├── 📦 JobApplier.Application/   (Business Logic)
│   │   ├── 📁 Services/             (TODO)
│   │   ├── 📁 DTOs/                 (TODO)
│   │   ├── 📁 Interfaces/           (TODO)
│   │   ├── 📁 Exceptions/
│   │   │   └── ApplicationException.cs
│   │   ├── Extensions/
│   │   │   └── DependencyInjection.cs
│   │   └── JobApplier.Application.csproj
│   │
│   ├── 📦 JobApplier.Domain/        (Business Rules)
│   │   ├── 📁 Entities/
│   │   │   ├── Entity.cs
│   │   │   └── User.cs
│   │   ├── 📁 ValueObjects/         (TODO)
│   │   └── JobApplier.Domain.csproj
│   │
│   └── 📦 JobApplier.Infrastructure/ (Technical Impl)
│       ├── 📁 Persistence/
│       │   ├── ApplicationDbContext.cs
│       │   └── 📁 Repositories/     (TODO)
│       ├── 📁 ExternalServices/     (TODO)
│       ├── 📁 FileHandling/         (TODO)
│       ├── Extensions/
│       │   └── DependencyInjection.cs
│       └── JobApplier.Infrastructure.csproj
│
├── 📁 tests/                        (Unit & Integration Tests)
│
├── 📄 Configuration
│   ├── .gitignore
│   ├── .editorconfig
│
└── 📄 Documentation
    ├── INDEX.md
    ├── README.md
    ├── QUICKSTART.md
    ├── IMPLEMENTATION.md
    ├── SECURITY_CONFIG.md
    ├── CHECKLIST.md
    ├── SETUP_COMPLETE.md
    ├── FINAL_STATUS.md
    └── FILE_MANIFEST.md
```

## 4. Request Flow Diagram

```
HTTP Request
     │
     ↓
┌─────────────────────────────────────────┐
│  Swagger/API Client                     │
│  (GET /api/health)                      │
└─────────────────────────────────────────┘
     │
     ↓
┌─────────────────────────────────────────┐
│  Api Layer - Middleware Pipeline        │
│  ├─ HTTPS Redirection                   │
│  ├─ CORS                                │
│  ├─ Authentication (JWT Bearer)         │
│  ├─ Authorization ([Authorize])         │
│  └─ Global Exception Handling           │
└─────────────────────────────────────────┘
     │
     ↓
┌─────────────────────────────────────────┐
│  Controller                             │
│  (HealthController.Get())               │
│  ├─ Validate request                    │
│  ├─ Call application service            │
│  └─ Return response                     │
└─────────────────────────────────────────┘
     │
     ↓
┌─────────────────────────────────────────┐
│  Application Layer - Service            │
│  (TODO: Business logic orchestration)   │
└─────────────────────────────────────────┘
     │
     ↓
┌─────────────────────────────────────────┐
│  Domain Layer - Business Rules          │
│  (Validate entities)                    │
└─────────────────────────────────────────┘
     │
     ↓
┌─────────────────────────────────────────┐
│  Infrastructure - Data/Services         │
│  ├─ Database (EF Core)                  │
│  └─ External APIs (OpenAI, OCR)         │
└─────────────────────────────────────────┘
     │
     ↓
      HTTP Response (JSON)
```

## 5. Dependency Injection Container

```
┌─────────────────────────────────────────┐
│    Program.cs (DI Configuration)        │
├─────────────────────────────────────────┤
│                                          │
│  services.AddJwtAuthentication()        │
│  │                                      │
│  └─→ JwtBearer validation              │
│      TokenValidationParameters         │
│      IssuerSigningKey                  │
│      Validate lifetime, issuer, aud.   │
│                                          │
│  services.AddSwaggerDocumentation()    │
│  │                                      │
│  └─→ OpenAPI schema generation         │
│      JWT security scheme                │
│      Swagger UI configuration           │
│                                          │
│  services.AddApplication()             │
│  │                                      │
│  └─→ ResumeService (TODO)              │
│      CoverLetterService (TODO)         │
│      DocumentProcessingService (TODO)  │
│                                          │
│  services.AddInfrastructure()          │
│  │                                      │
│  ├─→ DbContext                         │
│  ├─→ Repositories (TODO)               │
│  ├─→ OpenAiService (TODO)              │
│  └─→ OcrService (TODO)                 │
│                                          │
└─────────────────────────────────────────┘
         ↓ (Injected into)
┌─────────────────────────────────────────┐
│  Controllers & Services                 │
│  (Receive dependencies via constructor) │
└─────────────────────────────────────────┘
```

## 6. Authentication Flow

```
Client                                   Server
  │                                         │
  ├─ POST /api/auth/login ──────────────→ │
  │   { email, password }                  │
  │                                         │
  │                                  Program.cs
  │                                  JwtTokenProvider
  │                                  │
  │                                  ├─ Validate credentials
  │                                  ├─ Create claims
  │                                  ├─ Sign with secret key
  │                                  └─ Return JWT
  │                                         │
  │ ←─────────── 200 OK ──────────────────│
  │   { token, refreshToken }              │
  │                                         │
  ├─ GET /api/resumes ──────────────────→ │
  │   Authorization: Bearer <JWT>          │
  │                                         │
  │                                  Middleware
  │                                  │
  │                                  ├─ Extract token
  │                                  ├─ Validate signature
  │                                  ├─ Check expiration
  │                                  └─ Extract claims
  │                                         │
  │ ←──────── 200 OK ──────────────────── │
  │   [ { resume1 }, { resume2 } ]         │
  │                                         │
  │ [Token Expired]                        │
  │                                         │
  ├─ POST /api/auth/refresh ────────────→ │
  │   { refreshToken }                     │
  │                                         │
  │ ←──────── 200 OK ──────────────────── │
  │   { token (new) }                      │
  │                                         │
  └─────────────────────────────────────────
```

## 7. Configuration Hierarchy

```
┌──────────────────────────────────────────┐
│         Application Start                │
└──────────────────────────────────────────┘
          │
          ↓
┌──────────────────────────────────────────┐
│  ASPNETCORE_ENVIRONMENT = Development   │
└──────────────────────────────────────────┘
          │
          ├─ appsettings.json              (Base)
          │  ├─ Logging
          │  └─ AllowedHosts
          │
          ├─ appsettings.Development.json  (Override)
          │  ├─ Jwt.SecretKey
          │  ├─ ConnectionStrings.Default
          │  └─ Cors.AllowedOrigins
          │
          └─ User Secrets / Environment Variables
             ├─ Jwt__SecretKey
             └─ ConnectionStrings__DefaultConnection

Production:
          ├─ appsettings.json              (Base)
          │
          └─ Environment Variables ONLY
             ├─ Jwt__SecretKey (from Key Vault)
             ├─ ConnectionStrings__DefaultConnection
             └─ Cors__AllowedOrigins
```

## 8. Database Schema (Future)

```
┌──────────────────┐       ┌──────────────────┐
│      Users       │       │     Resumes      │
├──────────────────┤       ├──────────────────┤
│ Id (PK)          │       │ Id (PK)          │
│ Email            │◄──────│ UserId (FK)      │
│ FirstName        │       │ Content          │
│ LastName         │       │ FileUrl          │
│ PasswordHash     │       │ CreatedAt        │
│ IsActive         │       │ UpdatedAt        │
│ CreatedAt        │       └──────────────────┘
│ UpdatedAt        │
└──────────────────┘       ┌──────────────────┐
                           │  CoverLetters    │
                           ├──────────────────┤
                           │ Id (PK)          │
                           │ UserId (FK)      │
                           │ Content          │
                           │ JobTitle         │
                           │ Company          │
                           │ CreatedAt        │
                           │ UpdatedAt        │
                           └──────────────────┘
```

## 9. API Endpoint Map (Current & TODO)

```
┌────────────────────────────────────────────────────┐
│              API ENDPOINT STRUCTURE                │
├────────────────────────────────────────────────────┤
│                                                    │
│ Public (✅ Implemented)                           │
│ ├─ GET    /api/health              ← ✅ Works    │
│ ├─ GET    /swagger                 ← ✅ Works    │
│ └─ GET    /swagger/ui               ← ✅ Works    │
│                                                    │
│ Authentication (TODO - Implement)                 │
│ ├─ POST   /api/auth/register        ← Need       │
│ ├─ POST   /api/auth/login           ← Need       │
│ └─ POST   /api/auth/refresh-token   ← Need       │
│                                                    │
│ Protected Endpoints (JWT Required) - TODO         │
│ ├─ Resume Management                             │
│ │  ├─ GET    /api/resumes                        │
│ │  ├─ POST   /api/resumes/upload                 │
│ │  ├─ PUT    /api/resumes/{id}                   │
│ │  └─ DELETE /api/resumes/{id}                   │
│ │                                                 │
│ ├─ CoverLetter Generation                        │
│ │  ├─ POST   /api/cover-letters/generate         │
│ │  └─ GET    /api/cover-letters/{id}             │
│ │                                                 │
│ └─ Documents                                      │
│    ├─ GET    /api/documents                      │
│    └─ DELETE /api/documents/{id}                 │
│                                                    │
└────────────────────────────────────────────────────┘
```

## 10. Technology Stack

```
┌─────────────────────────────────────────────────┐
│          TECHNOLOGY STACK                       │
├─────────────────────────────────────────────────┤
│                                                 │
│  Framework & Runtime                           │
│  └─ .NET 8.0 (Latest LTS)                      │
│     └─ C# 12.0                                 │
│                                                 │
│  Web Framework                                  │
│  └─ ASP.NET Core 8.0                           │
│     ├─ MVC Controllers                         │
│     └─ REST API                                │
│                                                 │
│  Authentication & Security                     │
│  ├─ JWT Bearer Tokens                          │
│  ├─ System.IdentityModel.Tokens.Jwt            │
│  └─ Microsoft.AspNetCore.Authentication        │
│                                                 │
│  Data Access                                    │
│  ├─ Entity Framework Core 8.0                  │
│  └─ SQL Server (SqlServer provider)            │
│                                                 │
│  Documentation                                  │
│  ├─ Swagger/OpenAPI                            │
│  └─ Swashbuckle.AspNetCore                     │
│                                                 │
│  Logging & Monitoring                          │
│  └─ Serilog.AspNetCore                         │
│     └─ Structured logging                      │
│                                                 │
│  Future/Optional                                │
│  ├─ FluentValidation (input validation)        │
│  ├─ OpenAI API (GPT integration)                │
│  ├─ Tesseract (OCR)                            │
│  ├─ Azure Storage (file upload)                │
│  └─ MediatR (CQRS pattern)                     │
│                                                 │
└─────────────────────────────────────────────────┘
```

---

**These diagrams provide a visual reference for understanding the architecture.**

See [IMPLEMENTATION.md](IMPLEMENTATION.md) for detailed descriptions.
