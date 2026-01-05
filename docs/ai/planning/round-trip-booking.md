---
phase: planning
title: Round-Trip Booking - Project Planning
description: Task breakdown and implementation plan for round-trip booking feature
---

# Project Planning: Round-Trip Booking & Price Management

## Milestones
**What are the major checkpoints?**

- [ ] Milestone 1: Database Schema & Models (Foundation)
- [ ] Milestone 2: Pricing Services & Calculation (Core Logic)
- [ ] Milestone 3: Booking Flow Enhancement (User Experience)
- [ ] Milestone 4: Partner Price Management (Admin Features)
- [ ] Milestone 5: Testing & Documentation (Quality Assurance)

## Task Breakdown
**What specific work needs to be done?**

### Phase 1: Foundation - Database & Models
- [ ] Task 1.1: Create TicketType enum
  - Create `Enums/TicketType.cs` with OneWay and RoundTrip values
  - Estimated: 15 minutes

- [ ] Task 1.2: Update Ticket model
  - Add `Type` property (TicketType enum)
  - Add `OutboundTicketId` (nullable int)
  - Add `ReturnTicketId` (nullable int)
  - Add `BookingGroupId` (nullable int)
  - Add navigation properties
  - Estimated: 30 minutes

- [ ] Task 1.3: Update Trip model
  - Add `RoundTripDiscountPercent` (decimal, default 0)
  - Add `PriceLastUpdated` (nullable DateTime)
  - Estimated: 20 minutes

- [ ] Task 1.4: Create database migration
  - Add new columns to Tickets table
  - Add new columns to Trips table
  - Add indexes for performance
  - Add foreign key constraints for ticket linking
  - Estimated: 45 minutes

- [ ] Task 1.5: Update AppDbContext configuration
  - Configure new Ticket properties
  - Configure new Trip properties
  - Set up relationships and constraints
  - Estimated: 30 minutes

### Phase 2: Core Services - Pricing & Calculation
- [ ] Task 2.1: Create IPriceCalculatorService interface
  - Define CalculateOneWayPrice method
  - Define CalculateRoundTripPrice method
  - Define RoundTripPriceBreakdown class
  - Estimated: 30 minutes

- [ ] Task 2.2: Implement PriceCalculatorService
  - Implement one-way price calculation
  - Implement round-trip price calculation with discount
  - Calculate savings amount
  - Handle different seat classes
  - Estimated: 2 hours

- [ ] Task 2.3: Create IPricingService interface
  - Define GetRoundTripDiscount method
  - Define UpdateRoundTripDiscountAsync method
  - Define UpdateRouteDiscountAsync method
  - Estimated: 30 minutes

- [ ] Task 2.4: Implement PricingService
  - Get discount from Trip or default
  - Update discount for single trip
  - Bulk update discount for route
  - Validate discount range (0-50%)
  - Estimated: 2 hours

- [ ] Task 2.5: Register services in Program.cs
  - Register IPriceCalculatorService
  - Register IPricingService
  - Estimated: 10 minutes

### Phase 3: Booking Flow - User Experience
- [ ] Task 3.1: Create BookingViewModel enhancements
  - Add TicketType property
  - Add ReturnTrip property
  - Add pricing breakdown properties
  - Estimated: 30 minutes

- [ ] Task 3.2: Update UserController BookTrip (GET)
  - Add ticket type parameter
  - Show/hide return date picker based on type
  - Load return flights when round-trip selected
  - Estimated: 1.5 hours

- [ ] Task 3.3: Update UserController BookTrip (POST)
  - Handle one-way booking (existing logic)
  - Handle round-trip booking (new logic)
  - Create booking group
  - Create linked tickets
  - Calculate and apply discount
  - Estimated: 3 hours

- [ ] Task 3.4: Update BookTrip view
  - Add ticket type selector (radio buttons)
  - Add conditional return date picker
  - Add return flight selection UI
  - Display price breakdown
  - Show savings highlight
  - Estimated: 3 hours

- [ ] Task 3.5: Update booking confirmation
  - Show both tickets for round-trip
  - Display price breakdown
  - Show savings amount
  - Estimated: 1 hour

- [ ] Task 3.6: Update MyBookings view
  - Group round-trip tickets together
  - Show booking type indicator
  - Display total price for round-trip
  - Estimated: 1.5 hours

### Phase 4: Partner Management - Price Administration
- [ ] Task 4.1: Update PartnerController
  - Add UpdateTripPricing endpoint
  - Add UpdateRouteDiscount endpoint
  - Add GetRoutePricing endpoint
  - Estimated: 2 hours

- [ ] Task 4.2: Update TripsManagement view
  - Add RoundTripDiscount column
  - Add edit pricing button/modal
  - Show discount percentage
  - Estimated: 2 hours

- [ ] Task 4.3: Create pricing edit form/modal
  - Form for updating prices
  - Form for updating discount
  - Validation and error handling
  - Estimated: 2 hours

- [ ] Task 4.4: Add bulk discount update
  - UI for selecting multiple trips
  - Bulk update discount for route
  - Confirmation dialog
  - Estimated: 1.5 hours

### Phase 5: Testing & Documentation
- [ ] Task 5.1: Unit tests for PriceCalculatorService
  - Test one-way calculation
  - Test round-trip calculation
  - Test discount application
  - Test different seat classes
  - Estimated: 2 hours

- [ ] Task 5.2: Unit tests for PricingService
  - Test discount retrieval
  - Test discount update
  - Test validation
  - Test bulk update
  - Estimated: 2 hours

- [ ] Task 5.3: Integration tests for booking flow
  - Test one-way booking
  - Test round-trip booking
  - Test price calculation in booking
  - Test ticket linking
  - Estimated: 3 hours

- [ ] Task 5.4: Manual testing checklist
  - Test booking flows
  - Test price displays
  - Test partner price management
  - Test edge cases
  - Estimated: 2 hours

- [ ] Task 5.5: Update implementation documentation
  - Document new services
  - Document new models
  - Document API changes
  - Estimated: 1 hour

## Dependencies
**What needs to happen in what order?**

### Task Dependencies
- Phase 1 (Database & Models) must complete before Phase 2
- Phase 2 (Services) must complete before Phase 3
- Phase 3 (Booking Flow) can partially overlap with Phase 4
- Phase 4 (Partner Management) depends on Phase 2 services
- Phase 5 (Testing) depends on all previous phases

### External Dependencies
- No external APIs required
- Uses existing payment system
- Uses existing email service
- Uses existing authentication/authorization

### Code Dependencies
- Must maintain backward compatibility with existing one-way bookings
- Must work with existing Trip and Ticket repositories
- Must integrate with existing UserController booking flow

## Timeline & Estimates
**When will things be done?**

### Phase 1: Foundation
- Estimated: 2.5 hours
- Critical path: Yes

### Phase 2: Core Services
- Estimated: 5 hours
- Critical path: Yes

### Phase 3: Booking Flow
- Estimated: 11 hours
- Critical path: Yes

### Phase 4: Partner Management
- Estimated: 7.5 hours
- Can be done in parallel with Phase 3

### Phase 5: Testing
- Estimated: 10 hours
- Can overlap with Phase 4

### Total Estimated Effort
- **Sequential (worst case)**: ~36 hours
- **Parallel (best case)**: ~26 hours
- **Realistic (with overlap)**: ~30 hours

### Buffer for Unknowns
- Add 20% buffer: ~6 hours
- **Total with buffer**: ~36 hours (4-5 working days)

## Risks & Mitigation
**What could go wrong?**

### Technical Risks

1. **Risk**: Database migration issues with existing data
   - **Impact**: High
   - **Probability**: Medium
   - **Mitigation**: 
     - Test migration on copy of production data
     - Use nullable columns where possible
     - Provide data migration script if needed

2. **Risk**: Price calculation errors
   - **Impact**: High (financial)
   - **Probability**: Low
   - **Mitigation**:
     - Comprehensive unit tests
     - Manual verification of calculations
     - Code review for pricing logic

3. **Risk**: Performance issues with linked ticket queries
   - **Impact**: Medium
   - **Probability**: Low
   - **Mitigation**:
     - Add proper database indexes
     - Use efficient queries with Include()
     - Monitor query performance

4. **Risk**: Breaking existing one-way bookings
   - **Impact**: High
   - **Probability**: Low
   - **Mitigation**:
     - Maintain backward compatibility
     - Default Type to OneWay for existing tickets
     - Test existing booking flow thoroughly

### Business Risks

1. **Risk**: Partners set incorrect discount percentages
   - **Impact**: Medium (revenue loss)
   - **Probability**: Medium
   - **Mitigation**:
     - Validate discount range (0-50%)
     - Add confirmation dialog for high discounts
     - Add audit trail (future)

2. **Risk**: Users confused by round-trip pricing
   - **Impact**: Low
   - **Probability**: Medium
   - **Mitigation**:
     - Clear price breakdown display
     - Highlight savings amount
     - User testing and feedback

### Dependency Risks

1. **Risk**: Changes to existing booking flow break other features
   - **Impact**: High
   - **Probability**: Medium
   - **Mitigation**:
     - Incremental changes
     - Comprehensive regression testing
     - Code review

## Resources Needed
**What do we need to succeed?**

### Team Members and Roles
- Backend Developer: Implement services and controllers
- Frontend Developer: Update views and UI
- QA Tester: Test booking flows and edge cases
- Product Owner: Review requirements and acceptance criteria

### Tools and Services
- Visual Studio / VS Code
- SQLite database
- Git for version control
- Testing framework (xUnit or NUnit)

### Infrastructure
- Development database
- Test data for various scenarios
- Staging environment for testing

### Documentation/Knowledge
- Existing booking flow documentation
- Database schema documentation
- API documentation
- User stories and acceptance criteria

