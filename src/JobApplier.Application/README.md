# Application Layer Responsibilities

Business logic layer containing orchestration, DTOs, and service interfaces.

## Contents

### Services/
- **TODO**: ResumeService, CoverLetterService, DocumentProcessingService, etc.

### Interfaces/
- **TODO**: IResumeService, ICoverLetterService, IDocumentProcessingService, etc.

### DTOs/
- **TODO**: Request/response models for API contracts

### Exceptions/
- **ApplicationException.cs**: Base exception for application-specific errors

### Extensions/
- **DependencyInjection.cs**: Application service registration

## Notes
- No direct database access (use repositories from Infrastructure)
- No HTTP concerns (use DTOs for api contracts)
- Orchestrates multiple domain/infrastructure components
- All business validations occur here
