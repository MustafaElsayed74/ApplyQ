# CV Upload Functionality

## Overview

The CV Upload feature allows users to upload PDF and DOCX curriculum vitae files. The system extracts raw text from the documents and sends the extracted content to OpenAI for structured parsing, storing both the original file and parsed data.

## Architecture

### Domain Layer (CV Entity)
```csharp
public class CV : Entity
{
    public Guid UserId { get; private set; }
    public string FileName { get; private set; }
    public string FileType { get; private set; }      // "pdf" or "docx"
    public string FilePath { get; private set; }      // Secure storage path
    public long FileSizeBytes { get; private set; }
    public string ExtractedText { get; private set; }
    public string ParsedDataJson { get; private set; }
    public bool IsParsed { get; private set; }
    public DateTime? ParsedAt { get; private set; }
    public string FileChecksum { get; private set; }   // SHA256 for deduplication
}
```

### Application Layer

**Services:**
- `CVService` - Main orchestration service for CV upload, retrieval, and deletion
- `IFileStorageService` - Interface for secure file storage
- `ITextExtractionService` - Interface for PDF/DOCX text extraction
- `IOpenAICVParsingService` - Interface for OpenAI CV parsing

**DTOs:**
- `CVUploadRequest` - File upload request
- `CVUploadResponse` - Upload response with CV metadata
- `CVDetailsResponse` - Full CV details with extracted and parsed data

### Infrastructure Layer

**File Handling:**
- `FileStorageService` - Stores files in `{AppData}/JobApplier/CVs/{UserId}/`
- `TextExtractionService` - Placeholder for PDF/DOCX text extraction

**AI Integration:**
- `OpenAICVParsingService` - Skeleton for OpenAI API integration

**Persistence:**
- `CVRepository` - Data access for CV entities
- Database configured with CV table and indexes

## API Endpoints

### Upload CV
```http
POST /api/cvs/upload
Authorization: Bearer {access_token}
Content-Type: multipart/form-data

Body:
  file: (binary PDF or DOCX file, max 10 MB)

Response (200):
{
  "cvId": "uuid",
  "fileName": "resume.pdf",
  "fileType": "pdf",
  "fileSizeBytes": 245678,
  "isParsed": false,
  "createdAt": "2026-01-05T20:00:00Z",
  "message": "CV uploaded successfully. Parsing in progress..."
}
```

### Get CV Details
```http
GET /api/cvs/{cvId}
Authorization: Bearer {access_token}

Response (200):
{
  "cvId": "uuid",
  "fileName": "resume.pdf",
  "fileType": "pdf",
  "fileSizeBytes": 245678,
  "extractedText": "John Doe...",
  "parsedDataJson": "{...}",
  "isParsed": true,
  "parsedAt": "2026-01-05T20:01:30Z",
  "createdAt": "2026-01-05T20:00:00Z",
  "updatedAt": "2026-01-05T20:01:30Z"
}
```

### Get User's CVs
```http
GET /api/cvs
Authorization: Bearer {access_token}

Response (200):
[
  { ...CVDetailsResponse },
  { ...CVDetailsResponse }
]
```

### Delete CV
```http
DELETE /api/cvs/{cvId}
Authorization: Bearer {access_token}

Response (204 No Content)
```

## File Storage

Files are stored securely in:
- **Windows**: `%APPDATA%\JobApplier\CVs\{UserId}\{timestamp}_{randomId}.{extension}`
- **Linux/Mac**: `~/.config/JobApplier/CVs/{UserId}/{timestamp}_{randomId}.{extension}`

**Security Features:**
- Original filenames are not preserved (prevents path traversal)
- Files organized by user ID
- SHA256 checksum for deduplication
- Maximum file size: 10 MB
- Allowed types: `.pdf`, `.docx`

## Text Extraction

### TODO: PDF Text Extraction
Implement using one of:
- **iTextSharp** (AGPL/commercial)
- **PdfSharpCore** (open source)
- **Aspose.Pdf** (commercial)
- **Spire.Pdf** (commercial)
- **PdfPig** (open source - recommended)

```csharp
// Example with PdfPig
using (var document = PdfDocument.Open(filePath))
{
    var text = new StringBuilder();
    foreach (var page in document.GetPages())
    {
        text.Append(page.Text);
    }
    return text.ToString();
}
```

### TODO: DOCX Text Extraction
Implement using:
- **DocumentFormat.OpenXml** (official Microsoft SDK - recommended)
- **DocX** (.NET library)
- **Aspose.Words** (commercial)

```csharp
// Example with DocumentFormat.OpenXml
using (var wordDoc = WordprocessingDocument.Open(filePath, false))
{
    var body = wordDoc.MainDocumentPart.Document.Body;
    return body.InnerText;
}
```

## OpenAI Integration

### TODO: Configure API Key
Set the OpenAI API key in one of:

**1. Environment Variable:**
```bash
set OPENAI_API_KEY=sk-...
# or
export OPENAI_API_KEY=sk-...
```

**2. appsettings.json:**
```json
{
  "OpenAI": {
    "ApiKey": "sk-...",
    "Model": "gpt-4-turbo",
    "Temperature": 0.7
  }
}
```

**3. User Secrets (development):**
```bash
dotnet user-secrets set "OpenAI:ApiKey" "sk-..."
```

### TODO: Implement OpenAI Parsing
Use OpenAI SDK to send extracted CV text to GPT-4 with system prompt:

```csharp
// Pseudo-code
var client = new OpenAIClient(apiKey);
var response = await client.ChatCompletion.CreateAsync(new ChatCompletionCreateRequest
{
    Model = "gpt-4-turbo",
    Messages = new List<ChatMessage>
    {
        new ChatMessage { Role = "system", Content = "You are an expert CV parser. Extract structured data from CVs and return valid JSON." },
        new ChatMessage { Role = "user", Content = $"Parse this CV:\n\n{extractedText}" }
    },
    ResponseFormat = new { type = "json_object" }
});
```

### Expected JSON Structure
```json
{
  "personalInfo": {
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "+1-234-567-8900",
    "location": "New York, NY",
    "summary": "..."
  },
  "experiences": [
    {
      "company": "Tech Corp",
      "position": "Senior Engineer",
      "startDate": "2022-01",
      "endDate": "2025-01",
      "description": "...",
      "responsibilities": ["Responsibility 1", "Responsibility 2"]
    }
  ],
  "education": [
    {
      "institution": "University Name",
      "degree": "Bachelor of Science",
      "fieldOfStudy": "Computer Science",
      "graduationYear": "2020"
    }
  ],
  "skills": [
    {
      "category": "Programming Languages",
      "items": ["C#", ".NET", "Python"]
    }
  ],
  "certifications": [
    {
      "name": "AWS Solutions Architect",
      "issuer": "Amazon Web Services",
      "date": "2024-06"
    }
  ],
  "languages": [
    {
      "language": "English",
      "proficiency": "Native"
    },
    {
      "language": "Spanish",
      "proficiency": "Fluent"
    }
  ]
}
```

## Workflow

1. **Upload**: User uploads CV file via `/api/cvs/upload`
2. **Validation**: File type and size validated
3. **Storage**: File saved to secure directory
4. **Deduplication**: SHA256 checksum checked for duplicates
5. **Text Extraction**: Text extracted from PDF/DOCX
6. **Parsing**: Background task sends to OpenAI (if configured)
7. **Storage**: Parsed JSON stored in database
8. **Access**: User can retrieve full CV details including parsed data

## Error Handling

**400 Bad Request:**
- File is required
- File size exceeds 10 MB
- File type not supported (must be .pdf or .docx)

**401 Unauthorized:**
- Missing or invalid authentication token

**404 Not Found:**
- CV not found or unauthorized access attempt

**500 Internal Server Error:**
- File storage error
- Database error
- OpenAI API error

## Security Considerations

✅ **File Storage:**
- Files saved outside web root
- Original filenames not preserved
- User-based directory isolation
- Secure temporary file handling

✅ **Database:**
- Cascade delete (orphaned files cleaned on user deletion)
- User ownership enforced
- Audit trail (createdAt, updatedAt)

✅ **API:**
- Authentication required (all endpoints)
- Authorization enforced (users can only access own CVs)
- File size limits enforced
- Input validation on all parameters

⚠️ **TODO:**
- Implement virus scanning on uploaded files
- Add rate limiting on uploads
- Implement file encryption at rest
- Add audit logging for file access

## Performance Optimization

**Indexes:**
- UserId (find user's CVs quickly)
- UserId + FileChecksum (deduplication check)
- IsParsed (find pending parsing jobs)

**Considerations:**
- Text extraction runs synchronously during upload
- OpenAI parsing happens asynchronously in background
- Large files (>5MB) may benefit from chunked upload
- Database connection pooling configured

## Testing

### Manual Testing
```bash
# Register user
POST /api/auth/register
{ "email": "test@example.com", "password": "TestPass123!" }

# Login
POST /api/auth/login
{ "email": "test@example.com", "password": "TestPass123!" }

# Upload CV
POST /api/cvs/upload
Authorization: Bearer {accessToken}
Content-Type: multipart/form-data
file=@resume.pdf

# Get CV details
GET /api/cvs/{cvId}
Authorization: Bearer {accessToken}

# Delete CV
DELETE /api/cvs/{cvId}
Authorization: Bearer {accessToken}
```

### Integration Tests (TODO)
- Upload various file types
- Duplicate file detection
- File size validation
- Text extraction accuracy
- OpenAI parsing integration
- Authorization enforcement

## Future Enhancements

1. **Cover Letter Generation** - Use parsed CV data for auto-generating cover letters
2. **Resume Parsing** - Additional parsing engines (LinkedIn, Indeed, etc.)
3. **Resume Scoring** - Rate CV quality and relevance to jobs
4. **Resume Templates** - Generate formatted resume from parsed data
5. **Version Control** - Track CV version history
6. **Collaboration** - Share CVs with career advisors
7. **Bulk Upload** - Upload multiple CVs at once
8. **Background Jobs** - Use Hangfire for asynchronous parsing
9. **Resume Analytics** - Track which CVs generate most interest
10. **Multi-language Support** - Parse CVs in different languages

---

**Status**: ✅ Core implementation complete, TODOs left for text extraction and OpenAI integration
