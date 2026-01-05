# ⚡ Quick Start Guide - 5 Minutes to Running API

## 1️⃣ Prerequisites Check (1 minute)

Ensure you have:
- ✅ .NET 8 SDK installed
- ✅ SQL Server or LocalDB
- ✅ Code editor (VS Code, Visual Studio 2022)

```powershell
dotnet --version  # Should be 8.0.x
```

## 2️⃣ Clone/Open Project (30 seconds)

Navigate to workspace:
```powershell
cd d:\Job applier
```

## 3️⃣ Restore & Build (2 minutes)

```powershell
# Restore NuGet packages
dotnet restore

# Build solution
dotnet build
```

Expected output:
```
JobApplier.Domain succeeded
JobApplier.Application succeeded
JobApplier.Infrastructure succeeded
JobApplier.Api succeeded
Build succeeded
```

## 4️⃣ Configure Secrets (1 minute)

Edit: `src/JobApplier.Api/appsettings.Development.json`

Update JWT secret (must be 32+ characters):
```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-minimum-32-characters-long-here"
  }
}
```

**Note**: This file is in `.gitignore` - won't be committed

## 5️⃣ Run the API (30 seconds)

```powershell
dotnet run --project src/JobApplier.Api
```

Expected output:
```
[INFO] Starting JobApplier API
[INFO] Now listening on: http://localhost:5000
[INFO] Application started. Press Ctrl+C to shut down.
```

## ✅ Verify It Works

### Option A: Browser
1. Open: http://localhost:5000/swagger
2. See: Swagger UI with health endpoint
3. Try: Click "Try it out" on `/api/health` endpoint
4. See: Response `{ "status": "healthy", ... }`

### Option B: PowerShell
```powershell
Invoke-WebRequest -Uri "http://localhost:5000/api/health" -UseBasicParsing | Select-Object -Property Status, Content

# Expected output: Status 200
```

### Option C: cURL
```bash
curl http://localhost:5000/api/health
```

## 🎉 Success!

You now have a running ASP.NET Core backend with:
- ✅ Clean Architecture (4 layers)
- ✅ JWT Authentication setup
- ✅ Swagger documentation
- ✅ Global error handling
- ✅ Structured logging

## 📚 Next: Read Documentation

All in order of importance:

1. **[README.md](README.md)** (5 min) - Full setup guide
2. **[IMPLEMENTATION.md](IMPLEMENTATION.md)** (10 min) - What was built
3. **[SECURITY_CONFIG.md](SECURITY_CONFIG.md)** (5 min) - JWT & secrets
4. **[CHECKLIST.md](CHECKLIST.md)** (5 min) - Verification

## 🔧 Common Commands

```powershell
# Start development server (with auto-reload)
dotnet watch --project src/JobApplier.Api run

# Build only
dotnet build

# Create database migration (TODO)
dotnet ef migrations add InitialCreate -p src/JobApplier.Infrastructure -s src/JobApplier.Api

# Format code
dotnet format
```

## 🚨 Troubleshooting

### Port 5000 Already in Use
```powershell
# Stop other processes using port 5000
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

### Database Connection Error
- Update connection string in `appsettings.Development.json`
- Or create LocalDB: `(localdb)\mssqllocaldb`

### Build Fails
```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### JWT Secret Warning
- Edit `appsettings.Development.json`
- Add strong secret (32+ characters)

## 📋 Architecture at a Glance

```
┌─────────────────────────────────────┐
│         API Layer (Controllers)      │ ← HTTP requests
├─────────────────────────────────────┤
│    Application Layer (Services)      │ ← Business logic
├─────────────────────────────────────┤
│      Domain Layer (Entities)         │ ← Core rules (no deps)
├─────────────────────────────────────┤
│ Infrastructure (Database, APIs)      │ ← External services
└─────────────────────────────────────┘
         ↓ (via Dependency Injection)
       Configuration & Secrets
```

## 🔐 Security Notes

- ✅ JWT secrets stored locally only
- ✅ Production secrets via environment variables
- ✅ .gitignore prevents secret commits
- ✅ All endpoints protected by default
- ✅ Health check is public

## 🎯 Your First Task

After running the API successfully:

1. **Read** the README.md file
2. **Explore** the Swagger UI at http://localhost:5000/swagger
3. **Review** the project structure in VS Code
4. **Check** the TODO comments in code
5. **Plan** your first feature (e.g., User entity)

## 💡 Tips

- Use Swagger UI to test endpoints
- Press Ctrl+Shift+D to open debug console
- Use `dotnet watch run` for auto-reload during development
- Check `appsettings.Development.json` for local config
- All business logic goes in `Application` layer
- All database access goes in `Infrastructure` layer

---

**You're all set! Happy coding! 🚀**

Questions? Check the documentation files in the root directory.
