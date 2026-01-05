# Deployment Guide

## Overview

This guide covers deploying the Job Applier API to production environments.

## Prerequisites

- .NET 8.0 SDK
- MySQL 8.0+
- OpenAI API key (for cover letter generation)
- Domain name (optional, for HTTPS)
- Hosting platform (Azure, AWS, DigitalOcean, etc.)

## Local Development Setup

### 1. Clone Repository

```bash
git clone <repository-url>
cd "d:\Job applier"
```

### 2. Install Dependencies

```bash
# Restore NuGet packages
dotnet restore

# Install EF Core tools if not already installed
dotnet tool install --global dotnet-ef
```

### 3. Configure Database

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=job_applier_dev;User=root;Password=your_password;"
  },
  "Jwt": {
    "Key": "your-very-long-secret-key-at-least-32-characters",
    "Issuer": "JobApplierAPI",
    "Audience": "JobApplierUsers"
  },
  "OpenAI": {
    "ApiKey": "sk-your-test-key",
    "CoverLetterModel": "gpt-4-turbo"
  }
}
```

### 4. Apply Database Migrations

```bash
# Navigate to API project
cd src/API

# Apply all migrations
dotnet ef database update

# Or apply specific migration
dotnet ef database update AddCoverLetterFunctionality
```

### 5. Run Application

```bash
cd src/API
dotnet run

# Application will be available at http://localhost:5000
```

## Docker Deployment

### Build Docker Image

```bash
# Create Dockerfile in project root
docker build -t job-applier-api:latest .

# Tag for registry
docker tag job-applier-api:latest your-registry/job-applier-api:1.0.0

# Push to registry
docker push your-registry/job-applier-api:1.0.0
```

### Dockerfile Template

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/API/API.csproj", "src/API/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/API/API.csproj"

# Copy all files
COPY . .

# Build
RUN dotnet build "src/API/API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "src/API/API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "API.dll"]
```

### Docker Compose (Local Development)

```yaml
# docker-compose.yml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: rootpass
      MYSQL_DATABASE: job_applier_dev
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql

  api:
    build: .
    environment:
      ConnectionStrings__DefaultConnection: "Server=mysql;Port=3306;Database=job_applier_dev;User=root;Password=rootpass;"
      Jwt__Key: "your-very-long-secret-key-at-least-32-characters"
      OpenAI__ApiKey: "sk-your-key"
    ports:
      - "5000:80"
    depends_on:
      - mysql
    volumes:
      - ./uploads:/app/uploads

volumes:
  mysql_data:
```

Run with:
```bash
docker-compose up -d
```

## Azure Deployment

### 1. Create Resources

```bash
# Login to Azure
az login

# Create resource group
az group create --name job-applier-rg --location eastus

# Create App Service Plan
az appservice plan create \
  --name job-applier-plan \
  --resource-group job-applier-rg \
  --sku B2 --is-linux

# Create Web App
az webapp create \
  --resource-group job-applier-rg \
  --plan job-applier-plan \
  --name job-applier-api \
  --runtime "DOTNET|8.0"

# Create MySQL Database
az mysql flexible-server create \
  --resource-group job-applier-rg \
  --name job-applier-db \
  --admin-user adminuser \
  --admin-password YourSecurePassword123! \
  --sku-name Standard_B2s
```

### 2. Configure Application Settings

```bash
az webapp config appsettings set \
  --resource-group job-applier-rg \
  --name job-applier-api \
  --settings \
    WEBSITES_PORT=80 \
    ConnectionStrings__DefaultConnection="Server=job-applier-db.mysql.database.azure.com;Port=3306;Database=job_applier;User=adminuser;Password=YourSecurePassword123;" \
    Jwt__Key="your-very-long-secret-key-at-least-32-characters" \
    OpenAI__ApiKey="sk-your-key" \
    ASPNETCORE_ENVIRONMENT="Production"
```

### 3. Deploy Application

```bash
# Publish locally
dotnet publish -c Release -o ./publish

# Create deployment package
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy to Azure
az webapp deployment source config-zip \
  --resource-group job-applier-rg \
  --name job-applier-api \
  --src deploy.zip
```

### 4. Configure SSL/TLS

```bash
# Add custom domain
az webapp config hostname add \
  --resource-group job-applier-rg \
  --webapp-name job-applier-api \
  --hostname api.yourdomain.com

# Add SSL certificate
az webapp config ssl bind \
  --resource-group job-applier-rg \
  --name job-applier-api \
  --certificate-thumbprint your-cert-thumbprint
```

## AWS Deployment (Elastic Beanstalk)

### 1. Install CLI

```bash
pip install awsebcli --upgrade --user
```

### 2. Initialize Beanstalk

```bash
eb init -p "IIS 10.0" job-applier-api --region us-east-1
```

### 3. Create Environment

```bash
# Create .ebextensions/environment.config
mkdir .ebextensions

cat > .ebextensions/environment.config << EOF
option_settings:
  aws:elasticbeanstalk:application:environment:
    ASPNETCORE_ENVIRONMENT: Production
    ConnectionStrings__DefaultConnection: "Server=your-db.region.rds.amazonaws.com;Port=3306;Database=job_applier;User=admin;Password=YourPassword;"
    Jwt__Key: "your-long-secret-key"
    OpenAI__ApiKey: "sk-your-key"
EOF

# Deploy
eb create production-env
eb deploy
```

### 4. Configure RDS MySQL

```bash
# Create RDS instance
aws rds create-db-instance \
  --db-instance-identifier job-applier-db \
  --db-instance-class db.t3.micro \
  --engine mysql \
  --master-username admin \
  --master-user-password YourSecurePassword123! \
  --allocated-storage 20

# Update connection string
eb setenv ConnectionStrings__DefaultConnection="Server=<RDS-ENDPOINT>;Port=3306;Database=job_applier;User=admin;Password=YourSecurePassword123;"
```

## Environment Variables

### Production Configuration

```bash
# Database
ConnectionStrings__DefaultConnection=Server=<host>;Port=3306;Database=job_applier;User=<user>;Password=<password>;

# JWT
Jwt__Key=<very-long-secret-key-at-least-32-characters>
Jwt__Issuer=JobApplierAPI
Jwt__Audience=JobApplierUsers
Jwt__ExpireMinutes=60

# OpenAI
OpenAI__ApiKey=sk-<your-key>
OpenAI__CoverLetterModel=gpt-4-turbo

# Application
ASPNETCORE_ENVIRONMENT=Production

# Logging
Logging__LogLevel__Default=Information
Logging__LogLevel__Microsoft=Warning

# CORS
AllowedOrigins=https://yourdomain.com

# Rate Limiting (Optional)
RateLimiting__Enabled=true
RateLimiting__RequestsPerMinute=60
```

### Secrets Management

**Azure Key Vault:**
```bash
# Create vault
az keyvault create \
  --resource-group job-applier-rg \
  --name job-applier-vault

# Add secrets
az keyvault secret set \
  --vault-name job-applier-vault \
  --name "OpenAI--ApiKey" \
  --value "sk-your-key"

az keyvault secret set \
  --vault-name job-applier-vault \
  --name "Jwt--Key" \
  --value "your-very-long-secret-key"

# Reference in app settings
# In Azure App Service, add Identity and Key Vault access
```

**AWS Secrets Manager:**
```bash
aws secretsmanager create-secret \
  --name job-applier/openai-key \
  --secret-string "{\"apiKey\":\"sk-your-key\"}"

aws secretsmanager create-secret \
  --name job-applier/jwt-key \
  --secret-string "{\"key\":\"your-long-secret\"}"
```

## Database Migrations

### Applying Migrations in Production

```bash
# Option 1: Automatic on startup (safest)
# Configure in Startup.cs:
if (env.IsProduction())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }
}

# Option 2: Manual via command line
dotnet ef database update --configuration Release

# Option 3: Script-based (for CI/CD)
dotnet ef migrations script --output migration.sql
# Run migration.sql in MySQL
```

### Backup Before Migration

```bash
# MySQL backup
mysqldump -u root -p job_applier > backup_$(date +%Y%m%d_%H%M%S).sql

# On cloud platforms
# Azure: Azure Database for MySQL automatic backups
# AWS: RDS automated backups
```

## CI/CD Pipeline

### GitHub Actions Example

```yaml
# .github/workflows/deploy.yml
name: Deploy to Azure

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0

    - name: Restore
      run: dotnet restore

    - name: Build
      run: dotnet build -c Release --no-restore

    - name: Test
      run: dotnet test -c Release --no-build

    - name: Publish
      run: dotnet publish -c Release -o ./publish

    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: job-applier-api
        publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
        package: ./publish
```

## Monitoring & Logging

### Application Insights (Azure)

```bash
# Add to project
dotnet add package Microsoft.ApplicationInsights.AspNetCore

# Configure in Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### ELK Stack (Self-hosted)

```bash
# Docker Compose for ELK
docker-compose -f elk-docker-compose.yml up -d

# Configure Serilog in app
dotnet add package Serilog.Sinks.Elasticsearch
```

### Log Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "JobApplier.Application": "Information",
      "JobApplier.Infrastructure": "Debug"
    }
  }
}
```

## Health Checks

### Add Health Check Endpoint

```bash
dotnet add package AspNetCore.HealthChecks.MySql
```

```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddMySql(configuration.GetConnectionString("DefaultConnection"));

app.MapHealthChecks("/health");
```

Test:
```bash
curl http://localhost:5000/health
```

## Performance Tuning

### Database Connection Pooling

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=host;Database=job_applier;User=user;Password=pass;MaxPoolSize=100;MinPoolSize=5;"
  }
}
```

### Response Compression

```csharp
// In Program.cs
builder.Services.AddResponseCompression();
app.UseResponseCompression();
```

### Caching

```csharp
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("default", builder =>
        builder.Expire(TimeSpan.FromMinutes(5)));
});

// Use on controller
[OutputCache(PolicyName = "default")]
public async Task<IActionResult> GetCoverLetter(Guid id)
```

## Security Best Practices

✅ **Enforce HTTPS**
```csharp
app.UseHttpsRedirection();
app.UseHsts();
```

✅ **Set Security Headers**
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    await next();
});
```

✅ **Enable CORS Properly**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("production", builder =>
    {
        builder.WithOrigins("https://yourdomain.com")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});
```

✅ **Rate Limiting**
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "fixed", config =>
    {
        config.PermitLimit = 100;
        config.Window = TimeSpan.FromMinutes(1);
    });
});
```

✅ **Secrets Rotation**
- Rotate JWT key quarterly
- Rotate OpenAI API key if compromised
- Update database password periodically

## Troubleshooting

### Application Won't Start

1. **Check database connection**
   ```bash
   mysql -h <host> -u <user> -p
   ```

2. **Verify environment variables**
   ```bash
   echo $ASPNETCORE_ENVIRONMENT
   echo $ConnectionStrings__DefaultConnection
   ```

3. **Check logs**
   ```bash
   dotnet run 2>&1 | tee app.log
   ```

### Database Migration Fails

```bash
# Check migration status
dotnet ef migrations list

# Rollback if needed
dotnet ef database update <previous-migration-name>

# Verify database state
mysql> SHOW TABLES;
mysql> SELECT * FROM __EFMigrationsHistory;
```

### High Memory Usage

- Reduce max pool size
- Enable response compression
- Implement pagination
- Cache frequently accessed data

### Slow Queries

```bash
# Enable MySQL slow query log
SET GLOBAL slow_query_log = 'ON';

# Analyze query performance
EXPLAIN SELECT * FROM CoverLetters WHERE UserId = 'xxx';

# Add indexes if needed
CREATE INDEX idx_user_id ON CoverLetters(UserId);
```

## Scaling

### Horizontal Scaling

1. **Load Balancer Setup**
   - Azure Load Balancer
   - AWS ELB
   - Nginx reverse proxy

2. **Stateless Design**
   - No server-side sessions
   - JWT authentication
   - Redis for distributed cache

3. **Database Replication**
   - Master-replica setup
   - Read replicas for scale-out

### Vertical Scaling

- Increase VM size/tier
- Upgrade database instance
- Increase database memory

## Disaster Recovery

### Backup Strategy

```bash
# Daily automated backups
0 2 * * * mysqldump -u root -p job_applier | gzip > /backups/job_applier_$(date +\%Y\%m\%d).sql.gz

# Or use cloud provider backup services
# Azure: Automated backups with 7-35 day retention
# AWS: Automated backup with configurable retention
```

### Restore Procedure

```bash
# Restore from backup
gunzip < backup_20260105.sql.gz | mysql -u root -p job_applier

# Verify restore
mysql> SELECT COUNT(*) FROM CoverLetters;
```

## Maintenance

### Regular Tasks

- [ ] Monitor application logs
- [ ] Check database disk usage
- [ ] Review failed API requests
- [ ] Test backup restoration
- [ ] Update dependencies
- [ ] Security patches
- [ ] Performance analysis
- [ ] Clean up old files

### Update Process

```bash
# Test in staging first
git pull origin main
dotnet build -c Release
dotnet test

# Apply database migrations if needed
dotnet ef database update

# Deploy to production (via CI/CD or manual)
```

---

**Last Updated**: January 5, 2026
**Version**: 1.0
**Status**: Production Ready
