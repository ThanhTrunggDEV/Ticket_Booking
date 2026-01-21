---
phase: implementation
title: PNR Booking Code - Implementation Guide
description: Technical implementation notes and code guidelines
---

# Implementation Guide: PNR Booking Code

## Development Setup
**How do we get started?**

### Prerequisites and Dependencies
- ASP.NET Core 8.0 (existing)
- Entity Framework Core (existing)
- SQLite (existing)
- No new NuGet packages required

### Environment Setup Steps
1. Ensure database backup (optional but recommended)
2. Open project in IDE
3. Review existing Ticket model and repository structure
4. Follow implementation order from planning doc

### Configuration Needed
- No new configuration required
- PNR helper will be registered in `Program.cs`

## Code Structure
**How is the code organized?**

### Directory Structure
```
Ticket_Booking/
├── Models/
│   └── DomainModels/
│       └── Ticket.cs (updated)
├── Helpers/
│   └── PNRHelper.cs (new)
├── Interfaces/
│   └── IPNRHelper.cs (new)
├── Repositories/
│   └── TicketRepository.cs (extended)
├── Controllers/
│   ├── UserController.cs (updated)
│   └── PNRController.cs (new)
├── Views/
│   ├── PNR/
│   │   └── Lookup.cshtml (new)
│   ├── User/
│   │   ├── Ticket.cshtml (updated)
│   │   └── MyBooking.cshtml (updated)
│   └── Admin/ (updated)
└── Data/
    └── AppDbContext.cs (updated)
```

### Module Organization
- **Models**: Domain entities (Ticket)
- **Helpers**: Utility classes (PNRHelper)
- **Interfaces**: Service contracts (IPNRHelper)
- **Repositories**: Data access layer (TicketRepository)
- **Controllers**: Request handling (PNRController, UserController)
- **Views**: UI presentation

### Naming Conventions
- PNR codes: Uppercase (e.g., "A1B2C3")
- Methods: PascalCase (e.g., `GeneratePNR`)
- Variables: camelCase (e.g., `pnrCode`)
- Files: Match class names

## Implementation Notes
**Key technical details to remember:**

### Core Features

#### Feature 1: PNR Generation
**Implementation Approach:**
- Use `Random` class for character selection
- Character set: A-Z, 0-9 excluding (0, O, 1, I, L)
- Generate 6 characters
- Convert to uppercase
- Check uniqueness via repository
- Retry up to 5 times if collision occurs

**Code Pattern:**
```csharp
private const string PNR_CHARS = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
private const int PNR_LENGTH = 6;
private const int MAX_RETRIES = 5;

public string GeneratePNR()
{
    var random = new Random();
    var chars = new char[PNR_LENGTH];
    for (int i = 0; i < PNR_LENGTH; i++)
    {
        chars[i] = PNR_CHARS[random.Next(PNR_CHARS.Length)];
    }
    return new string(chars);
}
```

#### Feature 2: PNR Lookup
**Implementation Approach:**
- Case-insensitive comparison
- Email validation (normalize to lowercase)
- Return ticket with related data (Trip, User)
- Handle not found gracefully

**Code Pattern:**
```csharp
public async Task<Ticket?> GetByPNRAndEmailAsync(string pnr, string email)
{
    return await _dbSet
        .Where(t => t.PNR != null && 
                   t.PNR.ToUpper() == pnr.ToUpper() &&
                   t.User.Email.ToLower() == email.ToLower())
        .Include(t => t.Trip)
        .ThenInclude(tr => tr.Company)
        .Include(t => t.User)
        .FirstOrDefaultAsync();
}
```

#### Feature 3: Database Migration
**Implementation Approach:**
- Add PNR column as nullable
- Create unique index (case-insensitive via EF Core)
- Use EF Core migrations

**Migration Code:**
```csharp
// In AppDbContext.OnModelCreating
modelBuilder.Entity<Ticket>(entity =>
{
    entity.Property(e => e.PNR)
        .HasMaxLength(6);
    
    entity.HasIndex(e => e.PNR)
        .IsUnique()
        .HasFilter("[PNR] IS NOT NULL");
});
```

### Patterns & Best Practices

#### Design Patterns Used
- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Services injected via constructor
- **Helper/Utility Pattern**: PNR generation separated into helper

#### Code Style Guidelines
- Use async/await for database operations
- Use nullable reference types appropriately
- Validate inputs before processing
- Handle errors gracefully with try-catch
- Use meaningful variable names

#### Common Utilities/Helpers
- `PNRHelper`: Centralized PNR generation logic
- String extension methods (if needed) for PNR validation

## Integration Points
**How do pieces connect?**

### API Integration Details
- No external APIs required
- Internal service integration via DI

### Database Connections
- Entity Framework Core handles connections
- SQLite database: `ticket_booking.db`
- Migration updates schema automatically

### Third-Party Service Setup
- None required

## Error Handling
**How do we handle failures?**

### Error Handling Strategy
- **PNR Generation Failure**: 
  - Log error
  - Retry mechanism (up to 5 times)
  - If all retries fail, throw exception with clear message
  - Booking should fail gracefully

- **PNR Lookup Failure**:
  - Invalid PNR: Show user-friendly error message
  - Email mismatch: Show generic "not found" message (security)
  - Database error: Log and show generic error

- **Migration Failure**:
  - Rollback migration
  - Check database state
  - Fix issues and retry

### Logging Approach
- Log PNR generation attempts
- Log PNR collisions (for monitoring)
- Log lookup attempts (optional, for security)
- Use ILogger for structured logging

### Retry/Fallback Mechanisms
- PNR generation: Retry up to 5 times
- No fallback for lookup (fail gracefully)
- Database operations: EF Core handles retries

## Performance Considerations
**How do we keep it fast?**

### Optimization Strategies
- Database index on PNR column ensures fast lookups
- Case-insensitive comparison done in database (efficient)
- PNR generation is in-memory (very fast)

### Caching Approach
- No caching needed for PNR generation (fast enough)
- Consider caching lookup results if needed (future optimization)

### Query Optimization
- Use `Include()` efficiently for related data
- Index ensures O(log n) lookup time
- Limit related data loading to necessary fields

### Resource Management
- Dispose database contexts properly
- No unmanaged resources

## Security Notes
**What security measures are in place?**

### Authentication/Authorization
- PNR lookup: Public (no auth required, but email validation)
- Admin/Partner search: Requires authentication (existing)

### Input Validation
- Validate PNR format (6 alphanumeric characters)
- Validate email format
- Sanitize inputs to prevent injection
- EF Core parameterized queries prevent SQL injection

### Data Encryption
- PNR codes stored in plain text (not sensitive by themselves)
- Email used for validation (already stored)
- No additional encryption needed

### Secrets Management
- No secrets required for PNR feature

## Implementation Checklist

### Phase 1: Foundation
- [ ] Update Ticket model with PNR property
- [ ] Create database migration
- [ ] Apply migration and verify

### Phase 2: Core Features
- [ ] Create IPNRHelper interface
- [ ] Implement PNRHelper class
- [ ] Extend TicketRepository with PNR methods
- [ ] Register PNRHelper in DI container
- [ ] Integrate PNR generation in booking flow

### Phase 3: Lookup
- [ ] Create PNRController
- [ ] Create PNR lookup views
- [ ] Add routes for PNR controller

### Phase 4: UI Integration
- [ ] Update ticket detail view
- [ ] Update my booking view
- [ ] Update admin views
- [ ] Update partner views
- [ ] Update email templates

### Phase 5: Testing
- [ ] Write unit tests for PNR helper
- [ ] Write unit tests for repository
- [ ] Write integration tests
- [ ] Perform manual testing

## Testing Notes
- Test PNR generation uniqueness
- Test case-insensitive lookup
- Test email validation
- Test error scenarios
- Test with existing tickets (null PNR)




