# Infrastructure Layer Responsibilities

Technical implementation: data access, external services, file handling.

## Contents

### Persistence/
- **ApplicationDbContext.cs**: EF Core DbContext
- **Repositories/**: Repository implementations for data access

### ExternalServices/
- **TODO**: OpenAiService, OcrService, etc.

### FileHandling/
- **TODO**: PdfProcessor, DocxProcessor, ImageProcessor

### Extensions/
- **DependencyInjection.cs**: Infrastructure service registration

## Notes
- Implements interfaces defined in Application
- Contains implementation details (EF, external APIs, file I/O)
- Should not be referenced by Domain or Application (only by Api/Infrastructure)
- All secrets/configuration injected via DI
