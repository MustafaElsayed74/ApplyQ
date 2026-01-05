# Domain Layer Responsibilities

Core business logic with no external dependencies. Contains entities and business rules.

## Contents

### Entities/
- **Entity.cs**: Base class for all domain entities
- **User.cs**: User profile and authentication entity

### ValueObjects/
- **TODO**: Email, Resume content, etc. (immutable objects with value semantics)

## Notes
- Zero external dependencies (no EF, no HTTP, no logging)
- Single responsibility: represent business domain
- Validation rules embedded in entities
- Can be used standalone or in other projects
