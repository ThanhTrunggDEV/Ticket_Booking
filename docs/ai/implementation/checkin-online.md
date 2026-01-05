---
phase: implementation
title: Online Check-In - Implementation Guide
description: Technical implementation notes and code guidelines for online check-in
---

# Implementation Guide: Online Check-In Feature

## Development Setup
**How do we get started?**

### Prerequisites and Dependencies
- ASP.NET Core 8.0 (existing)
- Entity Framework Core (existing)
- SQLite (existing)
- PDF generation library: QuestPDF (recommended) or iTextSharp
  - Install via: `dotnet add package QuestPDF`
- Email service (existing MailService)

### Environment Setup Steps
1. Install PDF generation library via NuGet
2. Review existing Ticket and Trip models
3. Review existing MailService implementation
4. Set up file storage directory for boarding passes (e.g., `wwwroot/boarding-passes/`)

### Configuration Needed
- Add boarding pass storage path to `appsettings.json`
- Configure email template for boarding pass delivery
- Set check-in window (24-48 hours) - can be configurable

## Code Structure
**How is the code organized?**

### Directory Structure
```
Ticket_Booking/
├── Models/
│   └── DomainModels/
│       └── Ticket.cs (updated)
├── Models/
│   └── ViewModels/
│       └── SeatMapViewModel.cs (new)
│       └── CheckInViewModel.cs (new)
├── Services/
│   ├── SeatMapService.cs (new)
│   ├── SeatSelectionService.cs (new)
│   └── BoardingPassService.cs (new)
├── Interfaces/
│   ├── ISeatMapService.cs (new)
│   ├── ISeatSelectionService.cs (new)
│   └── IBoardingPassService.cs (new)
├── Controllers/
│   └── CheckInController.cs (new)
├── Repositories/
│   └── TicketRepository.cs (extended)
├── Views/
│   └── CheckIn/
│       ├── Index.cshtml (new)
│       ├── SeatMap.cshtml (new)
│       ├── Confirmation.cshtml (new)
│       └── MyTickets.cshtml (new)
└── Data/
    └── AppDbContext.cs (updated)
```

### Module Organization
- **Models**: Domain entities (Ticket updates) and ViewModels
- **Services**: Business logic (seat map, seat selection, boarding pass)
- **Interfaces**: Service contracts
- **Repositories**: Data access layer extensions
- **Controllers**: Request handling
- **Views**: UI presentation

### Naming Conventions
- Services: `{Feature}Service` (e.g., `SeatMapService`)
- Interfaces: `I{Feature}Service` (e.g., `ISeatMapService`)
- ViewModels: `{Feature}ViewModel` (e.g., `SeatMapViewModel`)
- Controllers: `{Feature}Controller` (e.g., `CheckInController`)

## Implementation Notes
**Key technical details to remember:**

### Core Features

#### Feature 1: Check-In Eligibility Validation
**Implementation Approach:**
- Check payment status (must be confirmed)
- Check check-in window (24-48 hours before departure)
- Check if already checked in
- Check flight status (not cancelled)

**Code Pattern:**
```csharp
public async Task<bool> IsEligibleForCheckInAsync(int ticketId)
{
    var ticket = await _ticketRepository.GetByIdAsync(ticketId);
    if (ticket == null) return false;
    
    // Check payment
    if (ticket.PaymentStatus != PaymentStatus.Confirmed) return false;
    
    // Check if already checked in
    if (ticket.IsCheckedIn) return false;
    
    // Check check-in window (24-48 hours before departure)
    var now = DateTime.UtcNow;
    var departureTime = ticket.Trip.DepartureTime;
    var hoursUntilDeparture = (departureTime - now).TotalHours;
    
    if (hoursUntilDeparture < 2 || hoursUntilDeparture > 48) return false;
    
    // Check flight status
    if (ticket.Trip.Status != TripStatus.Scheduled) return false;
    
    return true;
}
```

#### Feature 2: Seat Map Generation
**Implementation Approach:**
- Generate seat map from Trip seat configuration
- Calculate available seats (total seats - booked seats)
- Return seat map data structure with row/column layout
- Support different seat classes (Economy, Business, First Class)

**Code Pattern:**
```csharp
public SeatMapViewModel GetSeatMap(int tripId, SeatClass seatClass)
{
    var trip = _tripRepository.GetByIdAsync(tripId).Result;
    var totalSeats = GetTotalSeatsForClass(trip, seatClass);
    var bookedSeats = GetBookedSeats(tripId, seatClass);
    var availableSeats = totalSeats.Except(bookedSeats).ToList();
    
    return new SeatMapViewModel
    {
        TripId = tripId,
        SeatClass = seatClass,
        TotalSeats = totalSeats,
        AvailableSeats = availableSeats,
        BookedSeats = bookedSeats,
        Rows = CalculateRows(totalSeats.Count),
        SeatsPerRow = CalculateSeatsPerRow(seatClass)
    };
}
```

#### Feature 3: Seat Selection
**Implementation Approach:**
- Validate seat availability (check if not already taken)
- Use database transaction to prevent race conditions
- Update ticket with new seat number
- Return success/error

**Code Pattern:**
```csharp
public async Task<bool> AssignSeatAsync(int ticketId, string seatNumber)
{
    var ticket = await _ticketRepository.GetByIdAsync(ticketId);
    if (ticket == null) return false;
    
    // Validate seat availability
    if (!await IsSeatAvailableAsync(ticket.TripId, seatNumber, ticket.SeatClass))
        return false;
    
    // Use transaction to prevent race conditions
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // Double-check availability within transaction
        if (await IsSeatTakenAsync(ticket.TripId, seatNumber))
        {
            await transaction.RollbackAsync();
            return false;
        }
        
        ticket.SeatNumber = seatNumber;
        await _ticketRepository.UpdateAsync(ticket);
        await _ticketRepository.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }
    catch
    {
        await transaction.RollbackAsync();
        return false;
    }
}
```

#### Feature 4: Boarding Pass Generation
**Implementation Approach:**
- Use QuestPDF library to generate PDF
- Include all required information (passenger, flight, seat, gate, PNR, etc.)
- Save PDF to storage directory
- Return file path/URL

**Code Pattern:**
```csharp
public async Task<string> GenerateBoardingPassAsync(Ticket ticket)
{
    var fileName = $"boarding-pass-{ticket.PNR}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
    var filePath = Path.Combine(_boardingPassStoragePath, fileName);
    
    var document = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.Content()
                .Column(column =>
                {
                    column.Item().Text($"BOARDING PASS").FontSize(20).Bold();
                    column.Item().Text($"PNR: {ticket.PNR}").FontSize(14);
                    column.Item().Text($"Passenger: {ticket.User.FullName}").FontSize(12);
                    column.Item().Text($"Flight: {ticket.Trip.FromCity} → {ticket.Trip.ToCity}").FontSize(12);
                    column.Item().Text($"Seat: {ticket.SeatNumber}").FontSize(12);
                    column.Item().Text($"Gate: A04").FontSize(12);
                    column.Item().Text($"Boarding: {ticket.Trip.DepartureTime.AddMinutes(-45):HH:mm}").FontSize(12);
                    // Add QR code if available
                    if (!string.IsNullOrEmpty(ticket.QrCode))
                    {
                        column.Item().Image(ticket.QrCode).FitArea();
                    }
                });
        });
    });
    
    document.GeneratePdf(filePath);
    return filePath;
}
```

### Patterns & Best Practices

#### Design Patterns Used
- **Service Pattern**: Business logic separated into services
- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Services injected via constructor
- **Transaction Pattern**: Database transactions for atomic operations

#### Code Style Guidelines
- Use async/await for all database operations
- Use nullable reference types appropriately
- Validate inputs before processing
- Handle errors gracefully with try-catch
- Use meaningful variable names
- Add XML documentation comments

#### Common Utilities/Helpers
- `SeatMapService`: Seat map generation logic
- `SeatSelectionService`: Seat assignment logic
- `BoardingPassService`: PDF generation logic
- DateTime helpers for check-in window calculation

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
- QuestPDF library for PDF generation
- MailService for email delivery (existing)

## Error Handling
**How do we handle failures?**

### Error Handling Strategy
- **Check-In Failure**: 
  - Log error
  - Show user-friendly error message
  - Preserve form state
  - Allow retry

- **Seat Selection Failure**:
  - If seat taken: Show error, refresh seat map
  - If validation fails: Show specific error message
  - If database error: Log and show generic error

- **Boarding Pass Generation Failure**:
  - Log error
  - Still complete check-in (boarding pass can be regenerated)
  - Show warning to user
  - Provide manual download option

### Logging Approach
- Log check-in attempts (success/failure)
- Log seat selection conflicts
- Log boarding pass generation errors
- Use ILogger for structured logging

### Retry/Fallback Mechanisms
- Seat selection: Retry with different seat if first choice taken
- Boarding pass generation: Retry once, then provide manual option
- Email delivery: Use existing MailService retry logic

## Performance Considerations
**How do we keep it fast?**

### Optimization Strategies
- Cache seat map data (if trip hasn't changed)
- Optimize database queries (use Include() efficiently)
- Generate boarding pass asynchronously if possible
- Use database indexes on IsCheckedIn and CheckInTime

### Caching Approach
- Cache seat map for a trip (invalidate when seat assigned)
- Consider in-memory cache for frequently accessed trips

### Query Optimization
- Use `Include()` efficiently for related data
- Index IsCheckedIn column for fast queries
- Limit related data loading to necessary fields

### Resource Management
- Dispose PDF generation resources properly
- Clean up old boarding pass files periodically
- Dispose database contexts properly

## Security Notes
**What security measures are in place?**

### Authentication/Authorization
- Check-in via PNR: Validate PNR + Email (existing PNR lookup)
- Check-in via logged-in: Validate user owns ticket
- Prevent unauthorized check-in attempts

### Input Validation
- Validate ticket ID (must be integer, must exist)
- Validate seat number (format, availability)
- Sanitize inputs to prevent injection
- EF Core parameterized queries prevent SQL injection

### Data Encryption
- Boarding pass PDFs stored securely (not publicly accessible)
- Use secure file paths
- Validate file access permissions

### Secrets Management
- No secrets required for check-in feature
- Boarding pass storage path in configuration


