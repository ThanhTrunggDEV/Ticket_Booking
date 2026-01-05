---
phase: implementation
title: Round-Trip Booking - Implementation Guide
description: Technical implementation notes and code guidelines for round-trip booking
---

# Implementation Guide: Round-Trip Booking & Price Management

## Development Setup
**How do we get started?**

### Prerequisites and Dependencies
- ASP.NET Core 8.0 (existing)
- Entity Framework Core (existing)
- SQLite (existing)
- No new external dependencies

### Environment Setup Steps
1. Review existing Trip and Ticket models
2. Review existing booking flow in UserController
3. Review existing partner management in PartnerController
4. Set up test data with various pricing scenarios

### Configuration Needed
- No new configuration required
- Discount validation rules (0-50%) are hardcoded in service

## Code Structure
**How is the code organized?**

### Directory Structure
```
Ticket_Booking/
├── Enums/
│   └── TicketType.cs (new)
├── Models/
│   └── DomainModels/
│       ├── Ticket.cs (updated)
│       └── Trip.cs (updated)
│   └── ViewModels/
│       └── BookingViewModel.cs (updated)
├── Services/
│   ├── PriceCalculatorService.cs (new)
│   └── PricingService.cs (new)
├── Interfaces/
│   ├── IPriceCalculatorService.cs (new)
│   └── IPricingService.cs (new)
├── Controllers/
│   ├── UserController.cs (updated)
│   └── PartnerController.cs (updated)
├── Repositories/
│   ├── TicketRepository.cs (extended)
│   └── TripRepository.cs (extended)
├── Views/
│   ├── User/
│   │   ├── BookTrip.cshtml (updated)
│   │   └── MyBooking.cshtml (updated)
│   └── Partner/
│       └── TripsManagement.cshtml (updated)
└── Migrations/
    └── [timestamp]_AddRoundTripFields.cs (new)
```

### Module Organization
- **Enums**: TicketType enum
- **Models**: Domain models and ViewModels
- **Services**: Business logic (pricing, calculation)
- **Interfaces**: Service contracts
- **Controllers**: Request handling
- **Views**: UI presentation

### Naming Conventions
- Services: `{Feature}Service` (e.g., `PriceCalculatorService`)
- Interfaces: `I{Feature}Service` (e.g., `IPriceCalculatorService`)
- ViewModels: `{Feature}ViewModel` (e.g., `BookingViewModel`)
- Enums: PascalCase (e.g., `TicketType`)

## Implementation Notes
**Key technical details to remember:**

### Core Features

#### Feature 1: Ticket Type Selection
**Implementation Approach:**
- Add TicketType enum with OneWay and RoundTrip
- Add Type property to Ticket model
- Default existing tickets to OneWay
- Add ticket type selector to booking form

**Code Pattern:**
```csharp
public enum TicketType
{
    OneWay = 0,
    RoundTrip = 1
}

// In Ticket model
public TicketType Type { get; set; } = TicketType.OneWay;
```

#### Feature 2: Round-Trip Price Calculation
**Implementation Approach:**
- Get base prices for outbound and return trips
- Calculate subtotal
- Get discount percentage from trip or route
- Apply discount to subtotal
- Calculate savings vs two one-way tickets

**Code Pattern:**
```csharp
public RoundTripPriceBreakdown CalculateRoundTripPrice(
    Trip outboundTrip, 
    Trip returnTrip, 
    SeatClass seatClass)
{
    var outboundPrice = GetPriceForSeatClass(outboundTrip, seatClass);
    var returnPrice = GetPriceForSeatClass(returnTrip, seatClass);
    var subtotal = outboundPrice + returnPrice;
    
    var discountPercent = GetDiscountPercent(outboundTrip, returnTrip);
    var discountAmount = subtotal * (discountPercent / 100m);
    var totalPrice = subtotal - discountAmount;
    
    // Calculate savings vs two one-way tickets
    var twoOneWayTotal = outboundPrice + returnPrice;
    var savingsAmount = twoOneWayTotal - totalPrice;
    
    return new RoundTripPriceBreakdown
    {
        OutboundPrice = outboundPrice,
        ReturnPrice = returnPrice,
        Subtotal = subtotal,
        DiscountPercent = discountPercent,
        DiscountAmount = discountAmount,
        TotalPrice = totalPrice,
        SavingsAmount = savingsAmount
    };
}
```

#### Feature 3: Linked Ticket Creation
**Implementation Approach:**
- Generate unique BookingGroupId
- Create outbound ticket first
- Create return ticket with link to outbound
- Update outbound ticket with link to return
- Use database transaction for atomicity

**Code Pattern:**
```csharp
public async Task<BookingResult> CreateRoundTripBookingAsync(
    int outboundTripId, 
    int returnTripId, 
    int userId, 
    SeatClass seatClass)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        var bookingGroupId = GenerateBookingGroupId();
        
        // Create outbound ticket
        var outboundTicket = new Ticket
        {
            TripId = outboundTripId,
            UserId = userId,
            SeatClass = seatClass,
            Type = TicketType.RoundTrip,
            BookingGroupId = bookingGroupId,
            BookingDate = DateTime.UtcNow,
            PaymentStatus = PaymentStatus.Pending
        };
        await _ticketRepository.AddAsync(outboundTicket);
        
        // Create return ticket
        var returnTicket = new Ticket
        {
            TripId = returnTripId,
            UserId = userId,
            SeatClass = seatClass,
            Type = TicketType.RoundTrip,
            BookingGroupId = bookingGroupId,
            OutboundTicketId = outboundTicket.Id,
            BookingDate = DateTime.UtcNow,
            PaymentStatus = PaymentStatus.Pending
        };
        await _ticketRepository.AddAsync(returnTicket);
        
        // Link tickets
        outboundTicket.ReturnTicketId = returnTicket.Id;
        await _ticketRepository.UpdateAsync(outboundTicket);
        
        await _ticketRepository.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return new BookingResult { Success = true, Tickets = new[] { outboundTicket, returnTicket } };
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

#### Feature 4: Partner Price Management
**Implementation Approach:**
- Add discount field to trip edit form
- Validate discount range (0-50%)
- Update trip pricing including discount
- Support bulk update for routes
- Update PriceLastUpdated timestamp

**Code Pattern:**
```csharp
public async Task<bool> UpdateRoundTripDiscountAsync(int tripId, decimal discountPercent)
{
    if (discountPercent < 0 || discountPercent > 50)
        throw new ArgumentException("Discount must be between 0 and 50 percent.");
    
    var trip = await _tripRepository.GetByIdAsync(tripId);
    if (trip == null)
        return false;
    
    trip.RoundTripDiscountPercent = discountPercent;
    trip.PriceLastUpdated = DateTime.UtcNow;
    
    await _tripRepository.UpdateAsync(trip);
    await _tripRepository.SaveChangesAsync();
    
    return true;
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
- Validate inputs before processing
- Handle errors gracefully with try-catch
- Use meaningful variable names
- Add XML documentation comments
- Use decimal for all monetary values

#### Common Utilities/Helpers
- `PriceCalculatorService`: Price calculation logic
- `PricingService`: Discount management logic
- `GenerateBookingGroupId()`: Generate unique group IDs
- Validation helpers for discount percentages

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
- No new third-party services required

## Error Handling
**How do we handle failures?**

### Error Handling Strategy
- **Price Calculation Failure**: 
  - Log error
  - Show user-friendly error message
  - Fall back to showing individual prices
  - Allow booking without discount

- **Booking Creation Failure**:
  - Use database transaction to rollback
  - Log error
  - Show error message
  - Preserve form state
  - Allow retry

- **Discount Update Failure**:
  - Validate discount range before update
  - Show specific error message
  - Preserve form values
  - Allow correction and retry

### Logging Approach
- Log price calculations (for debugging)
- Log booking creation attempts
- Log discount updates by partners
- Use ILogger for structured logging

### Retry/Fallback Mechanisms
- Booking creation: Transaction rollback on failure
- Price calculation: Fall back to individual prices if discount fails
- Discount update: Validate before attempting update

## Performance Considerations
**How do we keep it fast?**

### Optimization Strategies
- Use database indexes on BookingGroupId and Type
- Optimize queries with Include() for related data
- Calculate prices on-demand (no caching needed)
- Use efficient queries for return flight search

### Caching Approach
- No caching for prices (calculated on-demand)
- Consider caching discount percentages if needed (low priority)

### Query Optimization
- Index BookingGroupId for fast ticket grouping
- Index Type for filtering by ticket type
- Use Include() efficiently for related data
- Limit related data loading to necessary fields

### Resource Management
- Dispose database contexts properly
- Use transactions efficiently
- Avoid N+1 query problems

## Security Notes
**What security measures are in place?**

### Authentication/Authorization
- Only authenticated users can book tickets
- Only partners can update pricing
- Validate user owns tickets before operations

### Input Validation
- Validate discount percentages (0-50%)
- Validate ticket type enum values
- Validate trip IDs exist
- Sanitize inputs to prevent injection
- EF Core parameterized queries prevent SQL injection

### Data Encryption
- No sensitive data requiring encryption
- Prices stored as decimal (standard type)

### Secrets Management
- No secrets required for this feature

