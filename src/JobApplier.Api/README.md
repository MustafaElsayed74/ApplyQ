# API Layer Responsibilities

The Api project serves as the presentation layer and HTTP entry point.

## Contents

### Controllers/
- **BaseController.cs**: Abstract base with shared logic (user extraction, etc.)
- **HealthController.cs**: Public health check endpoint

### Extensions/
- **AuthenticationExtensions.cs**: JWT setup
- **SwaggerExtensions.cs**: OpenAPI documentation
- **DependencyInjectionExtensions.cs**: Service registration

### Middleware/
- **GlobalExceptionHandlingMiddleware.cs**: Catches unhandled exceptions, returns 500 errors

### Configuration
- **appsettings.Development.json**: Local development secrets
- **appsettings.Production.json**: Production template (uses env vars)
- **Program.cs**: Application startup and middleware pipeline

## Notes
- Controllers should be thin: receive request → call service → return response
- Authentication required by default (see [Authorize] on BaseController)
- Health endpoint is public for load balancers
