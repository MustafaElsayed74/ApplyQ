# Job Description Submission

## Overview

The Job Description Submission feature allows users to submit job descriptions in two formats:
- **Plain text** - Direct text input
- **Image** - PNG or JPG files with OCR extraction to convert images to readable text

The system stores the raw extracted text and associates it with the user for later analysis and matching against user's CV data.

## Architecture

### Domain Layer (JobDescription Entity)

```csharp
public class JobDescription : Entity
{
    public Guid UserId { get; private set; }
    public string Description { get; private set; }
    public string SourceType { get; private set; }      // "text" or "image"
    public string? SourceImagePath { get; private set; }
    public string? SourceImageFileName { get; private set; }
    public long? SourceImageSizeBytes { get; private set; }
    public bool IsOCRExtracted { get; private set; }
    public string? JobTitle { get; private set; }
    public string? CompanyName { get; private set; }
}
```

**Factory Methods:**
- `CreateFromText()` - Create from plain text submission
- `CreateFromOCR()` - Create from OCR-extracted image text

**Update Methods:**
- `UpdateDescription()` - Update description text
- `UpdateMetadata()` - Update job title and company name

### Application Layer

**DTOs:**
- `JobDescriptionSubmitRequest` - Accepts plain text OR image file
  - Fields: DescriptionText, ImageFile, JobTitle, CompanyName
  - File validation: PNG/JPG only, max 5 MB
  
- `JobDescriptionResponse` - Response with full metadata
  - Fields: JobDescriptionId, Description, SourceType, IsOCRExtracted, JobTitle, CompanyName, SourceImageSizeBytes, CreatedAt, UpdatedAt, Message

**Interfaces:**
- `IJobDescriptionRepository` - CRUD operations for JobDescription entities
- `IOCRExtractionService` - Interface for OCR text extraction from images

**Service:**
- `JobDescriptionService` - Main service orchestrating submission, retrieval, update, and deletion
  - Methods:
    - `SubmitJobDescriptionAsync()` - Accept plain text or image, validate, extract, store
    - `GetJobDescriptionAsync()` - Retrieve specific job description with ownership validation
    - `GetUserJobDescriptionsAsync()` - List all user's job descriptions
    - `DeleteJobDescriptionAsync()` - Delete job description and associated image file
    - `UpdateDescriptionAsync()` - Update description text
    - `UpdateMetadataAsync()` - Update job title and company name
  - Validation:
    - Text or image required (not both)
    - Image file type (PNG/JPG only)
    - Image size (max 5 MB)
  - Text normalization:
    - Whitespace trimming
    - Line ending normalization
    - Excessive blank line removal

### Infrastructure Layer

**Repositories:**
- `JobDescriptionRepository` - Data access for JobDescription entities
  - Location: `Infrastructure/Persistence/Repositories/JobDescriptionRepository.cs`
  - Implements CRUD and read operations with user ownership filtering

**OCR Service:**
- `OCRExtractionService` - Placeholder for OCR implementation
  - Location: `Infrastructure/OCR/OCRExtractionService.cs`
  - Methods:
    - `ExtractTextFromImageAsync()` - Extract text from image (TODO: implement)
    - `GetContentType()` - Get MIME type for image extensions
    - `IsConfigured()` - Check if OCR provider is configured
  - Configuration: Reads from `OCR:Provider` and `OCR:ApiKey` settings
  - Fallback: Returns message if not configured instead of failing

### API Layer

**JobDescriptionsController** - RESTful endpoints
- Route: `/api/job-descriptions`
- Authorization: [Authorize] - All endpoints require authentication

**Endpoints:**

1. **POST /submit** - Submit job description
   ```http
   POST /api/job-descriptions/submit
   Authorization: Bearer {accessToken}
   Content-Type: multipart/form-data
   
   Body:
     descriptionText: (optional) "Job description text"
     imageFile: (optional) <binary PNG/JPG file>
     jobTitle: (optional) "Senior Engineer"
     companyName: (optional) "TechCorp"
   
   Response (200):
   {
     "jobDescriptionId": "uuid",
     "description": "Extracted or submitted text...",
     "sourceType": "text|image",
     "isOCRExtracted": false,
     "jobTitle": "Senior Engineer",
     "companyName": "TechCorp",
     "sourceImageSizeBytes": null,
     "createdAt": "2026-01-05T20:00:00Z",
     "updatedAt": null,
     "message": "Job description submitted successfully"
   }
   ```

2. **GET /** - Get user's job descriptions
   ```http
   GET /api/job-descriptions
   Authorization: Bearer {accessToken}
   
   Response (200):
   [
     { ...JobDescriptionResponse },
     { ...JobDescriptionResponse }
   ]
   ```

3. **GET /{jobDescriptionId}** - Get specific job description
   ```http
   GET /api/job-descriptions/{jobDescriptionId}
   Authorization: Bearer {accessToken}
   
   Response (200):
   { ...JobDescriptionResponse with full data }
   ```

4. **PUT /{jobDescriptionId}** - Update description text
   ```http
   PUT /api/job-descriptions/{jobDescriptionId}
   Authorization: Bearer {accessToken}
   Content-Type: application/json
   
   Body:
   {
     "description": "Updated description text..."
   }
   
   Response (200):
   { ...JobDescriptionResponse }
   ```

5. **PATCH /{jobDescriptionId}/metadata** - Update job title and company
   ```http
   PATCH /api/job-descriptions/{jobDescriptionId}/metadata
   Authorization: Bearer {accessToken}
   Content-Type: application/json
   
   Body:
   {
     "jobTitle": "Product Manager",
     "companyName": "TechCorp Inc"
   }
   
   Response (200):
   { ...JobDescriptionResponse }
   ```

6. **DELETE /{jobDescriptionId}** - Delete job description
   ```http
   DELETE /api/job-descriptions/{jobDescriptionId}
   Authorization: Bearer {accessToken}
   
   Response (204 No Content)
   ```

**Error Responses:**
- `400 Bad Request` - Validation errors (missing text/image, file size exceeds limit, unsupported file type)
- `401 Unauthorized` - Missing or invalid authentication token
- `404 Not Found` - Job description not found or unauthorized access attempt
- `500 Internal Server Error` - Server errors

## Database Schema

**JobDescriptions Table:**
- `Id` (GUID, PK) - Unique identifier
- `UserId` (GUID, FK) - Reference to Users table
- `Description` (LONGTEXT) - Full job description text
- `SourceType` (VARCHAR(10)) - "text" or "image"
- `SourceImagePath` (VARCHAR(500)) - Path to stored image file
- `SourceImageFileName` (VARCHAR(255)) - Original image filename
- `SourceImageSizeBytes` (BIGINT, nullable) - File size in bytes
- `IsOCRExtracted` (BIT) - Whether text was extracted via OCR
- `JobTitle` (VARCHAR(255), nullable) - Job position title
- `CompanyName` (VARCHAR(255), nullable) - Company name
- `CreatedAt` (DATETIME) - Submission timestamp
- `UpdatedAt` (DATETIME, nullable) - Last modification timestamp

**Indexes:**
- Primary: `Id`
- Index: `UserId` - For listing user's job descriptions
- Index: `CreatedAt` - For chronological sorting
- Index: `IsOCRExtracted` - For identifying OCR-extracted entries

**Relationships:**
- FK to `Users` with cascade delete
- When user is deleted, all associated job descriptions are deleted

## File Handling

**Image Storage:**
- Temporary storage during processing (if OCR implemented)
- Can be deleted after successful text extraction
- Path: `{AppData}/JobApplier/JobDescriptionImages/`

**Security:**
- File type validation (PNG/JPG only)
- File size validation (5 MB max)
- User-based directory isolation
- Original filenames preserved for reference

## OCR Integration (TODO)

The `OCRExtractionService` is a placeholder that needs implementation. Choose one of:

### 1. **Tesseract (Open Source - Recommended)**
```bash
dotnet add package Tesseract
```
**Installation:** https://github.com/UB-Mannheim/tesseract/wiki

**Configuration:**
```json
{
  "OCR": {
    "Provider": "Tesseract",
    "TessDataPath": "./tessdata"
  }
}
```

**Implementation Example:**
```csharp
using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
{
    using (var img = Pix.LoadFromFile(filePath))
    {
        using (var page = engine.Process(img))
        {
            return page.GetText();
        }
    }
}
```

### 2. **Azure Computer Vision API**
```bash
dotnet add package Azure.AI.Vision.ImageAnalysis
```

**Configuration:**
```json
{
  "OCR": {
    "Provider": "AzureVision",
    "ApiKey": "your-api-key",
    "Endpoint": "https://{region}.cognitiveservices.azure.com"
  }
}
```

### 3. **Google Cloud Vision API**
```bash
dotnet add package Google.Cloud.Vision.V1
```

**Configuration:**
```bash
set GOOGLE_APPLICATION_CREDENTIALS=/path/to/credentials.json
```

### 4. **Microsoft OCR.Space API (Free)**
```bash
dotnet add package RestSharp
```

**Implementation Example:**
```csharp
var client = new RestClient("https://api.ocr.space/parse");
var request = new RestRequest(Method.POST);
request.AddFile("filename", filePath);
var response = await client.ExecuteAsync(request);
```

### Configuration

To enable OCR, set in `appsettings.json`:
```json
{
  "OCR": {
    "Provider": "Tesseract",
    "ApiKey": ""  // Leave empty for Tesseract, required for cloud services
  }
}
```

Or use environment variables:
```bash
set OCR:Provider=Tesseract
set OCR:ApiKey=your-key
```

## Workflow

1. **Submission**
   - User submits via `/api/job-descriptions/submit`
   - Provide either `descriptionText` OR `imageFile` (not both)
   - Optional: Add `jobTitle` and `companyName` for reference

2. **Processing**
   - If text: Normalize and store immediately
   - If image: 
     - Validate file type and size
     - Save to temporary storage
     - Extract text via OCR (if configured)
     - Normalize extracted text
     - Delete temporary image file or store permanently

3. **Storage**
   - Description and metadata stored in database
   - Image file retained if OCR extraction failed
   - User ownership automatically enforced via UserId

4. **Retrieval**
   - User can list all submitted job descriptions
   - Can view specific job description with full details
   - Cannot access others' job descriptions (authorization enforced)

5. **Updates**
   - User can modify description text
   - User can add/update job title and company name
   - UpdatedAt timestamp automatically set

6. **Deletion**
   - Removes database record
   - Deletes associated image file if it exists
   - Graceful error handling if file deletion fails

## Error Handling

**Validation Errors (400):**
- "Either DescriptionText or ImageFile must be provided"
- "Image file is required"
- "Image file cannot be empty"
- "Image file size exceeds maximum allowed size of 5 MB"
- "File type not supported. Only PNG and JPG/JPEG files are allowed"

**Authorization Errors (404):**
- "You are not authorized to access this job description"
- "You are not authorized to delete this job description"
- "You are not authorized to update this job description"
- Note: Returns 404 instead of 403 to avoid revealing entity existence

**Not Found (404):**
- "Job description not found"

**Server Errors (500):**
- "An error occurred while submitting the job description"
- "An error occurred while retrieving the job description"
- "An error occurred while updating the job description"
- "An error occurred while deleting the job description"

## Security Features

✅ **Authentication & Authorization:**
- All endpoints require [Authorize] attribute
- User ID extracted from JWT claims
- Ownership validation on all operations

✅ **File Validation:**
- File type restricted to PNG and JPG only
- File size limited to 5 MB
- Extension validation before processing

✅ **Database:**
- User-based data isolation
- Cascade delete on user deletion
- Foreign key constraints

✅ **Error Handling:**
- Generic 404 for not found/unauthorized (prevents entity discovery)
- Detailed logging without exposing internals
- Graceful degradation if OCR unavailable

⚠️ **TODO Security Enhancements:**
- Rate limiting on submission endpoint
- Virus scanning on uploaded images
- Image encryption at rest
- Audit logging for data access
- GDPR compliance (right to deletion)

## Testing

### Manual Testing with cURL

```bash
# 1. Register and login first
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"TestPass123!"}'

curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"TestPass123!"}'
# Save accessToken from response

# 2. Submit job description as text
curl -X POST http://localhost:5000/api/job-descriptions/submit \
  -H "Authorization: Bearer {accessToken}" \
  -F "descriptionText=Senior Software Engineer position with 5+ years experience required. Stack: C#, .NET, Azure. Benefits include remote work, health insurance, 401k." \
  -F "jobTitle=Senior Engineer" \
  -F "companyName=TechCorp Inc"

# 3. Submit job description from image
curl -X POST http://localhost:5000/api/job-descriptions/submit \
  -H "Authorization: Bearer {accessToken}" \
  -F "imageFile=@job_description.png" \
  -F "jobTitle=Product Manager" \
  -F "companyName=StartupCo"

# 4. Get all job descriptions
curl -X GET http://localhost:5000/api/job-descriptions \
  -H "Authorization: Bearer {accessToken}"

# 5. Get specific job description
curl -X GET http://localhost:5000/api/job-descriptions/{jobDescriptionId} \
  -H "Authorization: Bearer {accessToken}"

# 6. Update description
curl -X PUT http://localhost:5000/api/job-descriptions/{jobDescriptionId} \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{"description":"Updated description text..."}'

# 7. Update metadata
curl -X PATCH http://localhost:5000/api/job-descriptions/{jobDescriptionId}/metadata \
  -H "Authorization: Bearer {accessToken}" \
  -H "Content-Type: application/json" \
  -d '{"jobTitle":"Senior Manager","companyName":"TechCorp"}'

# 8. Delete job description
curl -X DELETE http://localhost:5000/api/job-descriptions/{jobDescriptionId} \
  -H "Authorization: Bearer {accessToken}"
```

### Integration Tests (TODO)
- Text submission validation
- Image submission with OCR extraction
- File size validation (exceed 5 MB limit)
- Unsupported file type rejection
- User authorization enforcement
- CRUD operations
- Orphaned file cleanup

## Performance Considerations

**Database Indexes:**
- `UserId` - Fast user job description lookups
- `CreatedAt` - Efficient chronological sorting
- `IsOCRExtracted` - Identify pending OCR processing

**Optimization:**
- AsNoTracking() for read operations
- Pagination for large result sets (TODO)
- Lazy loading for image files (TODO)
- Compression for stored descriptions (TODO)

**Scalability:**
- User directory isolation for file storage
- Background job processing for OCR (TODO)
- Database connection pooling
- Async/await throughout

## Future Enhancements

1. **OCR Integration** - Choose and implement OCR library
2. **Background Processing** - Move OCR to background job service (Hangfire)
3. **Pagination** - Support pagination for large job description lists
4. **Search & Filter** - Search descriptions by job title, company, date
5. **Versioning** - Track version history of job descriptions
6. **Analytics** - Track which job descriptions are most viewed
7. **Matching** - Compare job descriptions against user's CVs for relevance scoring
8. **Bulk Operations** - Submit multiple job descriptions at once
9. **Export** - Export job descriptions to PDF or Word
10. **Sharing** - Share job descriptions with other users or career advisors

---

**Status**: ✅ Core implementation complete, ready for OCR integration
**Last Updated**: January 5, 2026
