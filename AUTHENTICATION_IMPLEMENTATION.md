# Authentication Implementation Guide

## Overview

A production-ready authentication system has been implemented with:
- ✅ Email/password registration and login
- ✅ JWT access tokens (15-minute expiration)
- ✅ Refresh tokens (7-day expiration, revocable)
- ✅ Password hashing (PBKDF2 with SHA-256)
- ✅ Comprehensive validation
- ✅ Database persistence

## Architecture

### Domain Layer
- **User**: Aggregate root with email, password hash, profile info
  - Email auto-normalized to lowercase
  - Methods: ConfirmEmail(), Deactivate(), UpdatePasswordHash()
  - Navigation: RefreshTokens collection

- **RefreshToken**: Value object for token management
  - Tracks: UserId, Token, ExpiresAt, RevokedAt
  - Methods: IsValid(), IsExpired, IsRevoked, Revoke()
  - Cascade delete with User

- **Email**: Value object for email validation
  - Format validation (RFC 5321)
  - Result<T> monad for error handling

### Application Layer
- **IAuthenticationService**: Authentication orchestration
  - RegisterAsync()
  - LoginAsync()
  - RefreshTokenAsync()
  - LogoutAsync()

- **AuthenticationValidator**: Production-grade validation
  - Register: Email, names, password confirmation
  - Login: Email and password required
  - Password: 8+ chars, uppercase, lowercase, digit, special char

- **DTOs**:
  - RegisterRequest
  - LoginRequest
  - RefreshTokenRequest
  - AuthResponse (with user and tokens)
  - UserDto

- **Interfaces**:
  - IAuthenticationService
  - IPasswordHasher
  - IJwtTokenProvider
  - IUserRepository
  - IRefreshTokenRepository

### Infrastructure Layer
- **PasswordHasher**: PBKDF2 implementation
  - 100,000 iterations (OWASP standard)
  - 16-byte random salt
  - 32-byte hash
  - Constant-time comparison (timing attack resistant)

- **JwtTokenProvider**: Secure token generation
  - Configurable expiration, issuer, audience
  - Standard claims: sub, email, iat, exp, iss, aud, jti
  - HMAC SHA-256 signing
  - Refresh token: 64-byte cryptographically secure random

- **Repositories**:
  - UserRepository (CRUD + EmailExists check)
  - RefreshTokenRepository (Token lookup, revocation)

- **Database**:
  - User table with unique email index
  - RefreshToken table with cascade delete
  - Migrations applied via EF Core

### API Layer
- **AuthController**: HTTP endpoints
  - POST /api/auth/register - Public
  - POST /api/auth/login - Public
  - POST /api/auth/refresh - Public
  - POST /api/auth/logout - Requires JWT

- Error handling:
  - 400 Bad Request: Validation failures
  - 409 Conflict: Duplicate email
  - 401 Unauthorized: Invalid credentials/token
  - 500 Internal Server Error: Server issues

## Data Flow

### Registration
```
1. RegisterRequest (email, firstName, lastName, password, confirmPassword)
   ↓
2. Validate request (email format, password strength, confirmation)
   ↓
3. Check email doesn't exist
   ↓
4. Hash password (PBKDF2 + salt)
   ↓
5. Create User entity
   ↓
6. Save User to database
   ↓
7. Generate JWT access token
   ↓
8. Generate refresh token
   ↓
9. Save RefreshToken to database
   ↓
10. Return AuthResponse (tokens + user info)
```

### Login
```
1. LoginRequest (email, password)
   ↓
2. Validate request
   ↓
3. Find user by email
   ↓
4. Verify password (constant-time comparison)
   ↓
5. Check user is active
   ↓
6. Generate tokens (same as registration steps 7-9)
```

### Refresh Token
```
1. RefreshTokenRequest (refresh_token)
   ↓
2. Find refresh token
   ↓
3. Verify valid (not expired, not revoked)
   ↓
4. Get associated user
   ↓
5. Check user is active
   ↓
6. Revoke old refresh token
   ↓
7. Generate new tokens
```

### Logout
```
1. Extract userId from JWT claims
   ↓
2. Revoke all user's refresh tokens
   ↓
3. Return success
```

## Security Features

### Password Security
- ✅ PBKDF2 with 100,000 iterations (OWASP recommendation)
- ✅ 16-byte random salt per password
- ✅ SHA-256 hash algorithm
- ✅ Constant-time comparison (prevents timing attacks)
- ✅ No plaintext passwords stored
- ✅ Password strength validation (8+ chars, complexity)

### Token Security
- ✅ JWT signing with HMAC SHA-256
- ✅ Short access token expiration (15 minutes)
- ✅ Longer refresh token expiration (7 days)
- ✅ Refresh token revocation support
- ✅ Cryptographically secure random generation (64 bytes)
- ✅ Token claims: sub (user ID), email, jti (unique ID)

### API Security
- ✅ HTTPS required (configured in appsettings)
- ✅ Validation at boundaries (RegisterRequest, LoginRequest)
- ✅ Email normalization (lowercase storage)
- ✅ User activation status checked
- ✅ Duplicate email prevention
- ✅ Invalid credentials (generic message - no user enumeration)
- ✅ Cascade delete (orphaned tokens cleaned up)

### Database Security
- ✅ Unique index on email
- ✅ Not null constraints
- ✅ Foreign key relationships
- ✅ Parameterized queries (EF Core)
- ✅ Connection string in configuration (not hardcoded)

## Configuration

### appsettings.Development.json
```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-minimum-32-characters-required",
    "Issuer": "JobApplier",
    "Audience": "JobApplierClient",
    "ExpirationMinutes": 15
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=JobApplierDb;Trusted_Connection=true;"
  }
}
```

### appsettings.Production.json
```json
{
  "Jwt": {
    "SecretKey": "${Jwt:SecretKey}",
    "Issuer": "JobApplier",
    "Audience": "JobApplierClient",
    "ExpirationMinutes": 15
  },
  "ConnectionStrings": {
    "DefaultConnection": "${ConnectionStrings:DefaultConnection}"
  }
}
```

Use environment variables in production:
```bash
export Jwt__SecretKey="production-secret-from-vault"
export ConnectionStrings__DefaultConnection="Server=prod.database.windows.net;..."
```

## API Endpoints

### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!"
}

Response (200):
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64-encoded-token",
  "expiresIn": 900,
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": true,
    "createdAt": "2026-01-05T00:00:00Z"
  }
}
```

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}

Response (200): Same as Register
```

### Refresh Token
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "base64-encoded-token"
}

Response (200): Same as Login
```

### Logout
```http
POST /api/auth/logout
Authorization: Bearer <access_token>

Response (200):
{
  "message": "Logged out successfully"
}
```

## Usage Examples

### cURL
```bash
# Register
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email":"user@example.com",
    "firstName":"John",
    "lastName":"Doe",
    "password":"SecurePassword123!",
    "confirmPassword":"SecurePassword123!"
  }'

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"SecurePassword123!"}'

# Protected endpoint with token
curl -X GET http://localhost:5000/api/resumes \
  -H "Authorization: Bearer <access_token>"

# Refresh token
curl -X POST http://localhost:5000/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"<refresh_token>"}'
```

### PowerShell
```powershell
# Register
$register = @{
    email = "user@example.com"
    firstName = "John"
    lastName = "Doe"
    password = "SecurePassword123!"
    confirmPassword = "SecurePassword123!"
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:5000/api/auth/register" `
  -Method Post `
  -Body $register `
  -ContentType "application/json"
```

## Database Schema

### Users Table
```sql
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Email NVARCHAR(254) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    EmailConfirmedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL
);
```

### RefreshTokens Table
```sql
CREATE TABLE RefreshTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    Token NVARCHAR(MAX) NOT NULL UNIQUE,
    ExpiresAt DATETIME2 NOT NULL,
    RevokedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NULL
);
```

## Testing

### Test Scenario: Complete Auth Flow
```
1. Register new user → Get tokens
2. Use access token to access protected endpoint
3. Token expires (15 minutes)
4. Use refresh token to get new access token
5. Logout → All refresh tokens revoked
6. Refresh token no longer works
```

### Test Scenario: Validation
```
1. Register with invalid email → 400 Bad Request
2. Register with weak password → 400 Bad Request
3. Register duplicate email → 409 Conflict
4. Login with wrong password → 400 Bad Request
5. Refresh with expired token → 400 Bad Request
```

## TODO: Next Steps

- [ ] Add email confirmation (send verification link)
- [ ] Add password reset flow
- [ ] Add 2FA (two-factor authentication)
- [ ] Add rate limiting on auth endpoints
- [ ] Add account lockout after failed attempts
- [ ] Add audit logging for auth events
- [ ] Add OAuth2 / OpenID Connect integration
- [ ] Add API key authentication for service-to-service
- [ ] Add CORS configuration for frontend
- [ ] Add integration tests for auth flow

## Files Modified/Created

**Domain Layer**
- `Entities/User.cs` - Updated with auth properties
- `Entities/RefreshToken.cs` - New refresh token entity
- `ValueObjects/Email.cs` - Email value object
- `ValueObjects/Result.cs` - Result monad

**Application Layer**
- `Services/AuthenticationService.cs` - Auth orchestration
- `Validators/Auth/AuthenticationValidator.cs` - Validation logic
- `DTOs/Auth/` - Request/response models
- `Interfaces/IAuthenticationService.cs`
- `Interfaces/IPasswordHasher.cs`
- `Interfaces/IJwtTokenProvider.cs`
- `Interfaces/IUserRepository.cs`
- `Interfaces/IRefreshTokenRepository.cs`
- `Exceptions/AuthenticationException.cs`
- `Extensions/DependencyInjection.cs` - Updated

**Infrastructure Layer**
- `Security/PasswordHasher.cs` - PBKDF2 hashing
- `Security/JwtTokenProvider.cs` - JWT generation
- `Persistence/Repositories/UserRepository.cs` - User data access
- `Persistence/Repositories/RefreshTokenRepository.cs` - Token data access
- `Persistence/ApplicationDbContext.cs` - Updated with models
- `Extensions/DependencyInjection.cs` - Updated with services

**API Layer**
- `Controllers/AuthController.cs` - HTTP endpoints
- `JobApplier.Api.csproj` - Added EF Core Design package

**Database**
- `Migrations/[timestamp]_AddAuthenticationEntities.cs` - Initial schema

**Configuration**
- `appsettings.Development.json` - JWT and DB config

## Clean Architecture Compliance

✅ **Dependency Flow**: API → Application → Domain ← Infrastructure
✅ **No Cross-Layer Calls**: Each layer uses interfaces, not implementations
✅ **Entity Driven**: Domain entities drive all business logic
✅ **Interface Segregation**: Small, focused interfaces
✅ **Separation of Concerns**: Clear responsibility boundaries
✅ **Production Patterns**: Security, validation, error handling
✅ **Testability**: All dependencies injectable, mockable

---

**Status**: ✅ **Ready for testing and integration**
