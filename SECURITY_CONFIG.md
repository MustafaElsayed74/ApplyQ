# Security & Configuration Guide

## JWT Configuration

### Development Environment

1. **Update** `appsettings.Development.json`:
```json
{
  "Jwt": {
    "SecretKey": "your-dev-secret-key-min-32-characters-long-here",
    "Issuer": "JobApplier",
    "Audience": "JobApplierClient",
    "ExpirationMinutes": 15
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=JobApplierDb;Trusted_Connection=true;"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173"
    ]
  }
}
```

2. **Generate a strong secret** (use OpenSSL or online generator):
```bash
# PowerShell: 32+ random characters
[Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString() + (New-Guid).ToString()))
```

### Production Environment

**NEVER commit secrets.** Use environment variables or Azure Key Vault:

```powershell
# Set as environment variables
$env:Jwt__SecretKey = "production-secret-from-vault"
$env:ConnectionStrings__DefaultConnection = "Server=prod.database.windows.net;..."
```

Or use Azure Key Vault:
```csharp
// TODO: Add in Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

## Authenticated Requests

### Request Format
```http
GET /api/resumes
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Claims
JWT tokens should include:
- `sub`: User ID
- `email`: User email
- `iat`: Issued at (Unix timestamp)
- `exp`: Expiration (Unix timestamp)
- `iss`: Issuer (JobApplier)
- `aud`: Audience (JobApplierClient)

## Testing Authentication

### Using Swagger UI
1. Run the API: `dotnet run -p src/JobApplier.Api`
2. Open `https://localhost:5001/swagger`
3. Click "Authorize" button
4. Paste JWT token from login response
5. Test protected endpoints

### Using Postman/cURL

```bash
# Get token (TODO: create login endpoint)
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password"}'

# Use token for authenticated request
curl -X GET https://localhost:5001/api/resumes \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Key Points

- ✅ Secret key minimum 32 characters
- ✅ Token expiration: 15 minutes (access token)
- ✅ Use refresh tokens for longer sessions (TODO)
- ✅ HTTPS required in production
- ✅ HttpOnly cookies for token storage (TODO)
- ✅ Implement token revocation (logout) (TODO)
- ✅ Add rate limiting to auth endpoints (TODO)
- ✅ Monitor failed login attempts (TODO)

## Database Connection

### LocalDB (Development)
```
Server=(localdb)\mssqllocaldb;Database=JobApplierDb;Trusted_Connection=true;
```

### Azure SQL Database
```
Server=tcp:servername.database.windows.net,1433;Initial Catalog=JobApplierDb;Persist Security Info=False;User ID=username;Password=password;Encrypt=True;Connection Timeout=30;
```

### Entity Framework Migrations

```powershell
# Create migration
dotnet ef migrations add InitialCreate -p src/JobApplier.Infrastructure -s src/JobApplier.Api

# Update database
dotnet ef database update -p src/JobApplier.Infrastructure -s src/JobApplier.Api

# Remove last migration (if needed)
dotnet ef migrations remove -p src/JobApplier.Infrastructure -s src/JobApplier.Api
```

## CORS Configuration

Update allowed origins in `appsettings.json`:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://example.com",
      "https://app.example.com"
    ]
  }
}
```

Then update middleware (TODO):
```csharp
var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
app.UseCors(policy => 
    policy.WithOrigins(allowedOrigins)
          .AllowAnyMethod()
          .AllowAnyHeader());
```

## Environment Variables Reference

| Variable | Example | Purpose |
|----------|---------|---------|
| `ASPNETCORE_ENVIRONMENT` | Production | .NET environment |
| `ASPNETCORE_URLS` | https://0.0.0.0:443 | Listen address |
| `Jwt__SecretKey` | (base64) | JWT signing key |
| `ConnectionStrings__DefaultConnection` | Server=... | DB connection |
| `Cors__AllowedOrigins__0` | https://example.com | CORS origin |
