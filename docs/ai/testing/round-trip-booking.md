---
phase: testing
title: Round-Trip Booking - Testing Strategy
description: Define testing approach, test cases, and quality assurance
---

# Testing Strategy: Round-Trip Booking & Price Management

## Test Coverage Goals
**What level of testing do we aim for?**

- Unit test coverage target: 100% of new/changed code
- Integration test scope: Critical booking paths + price calculation + error handling
- End-to-end test scenarios: Complete round-trip booking flow, partner price management
- Alignment with requirements/design acceptance criteria: All acceptance criteria must be testable

## Unit Tests
**What individual components need testing?**

### PriceCalculatorService

- [ ] Test case 1: CalculateOneWayPrice returns correct price for Economy class
- [ ] Test case 2: CalculateOneWayPrice returns correct price for Business class
- [ ] Test case 3: CalculateOneWayPrice returns correct price for FirstClass class
- [ ] Test case 4: CalculateRoundTripPrice calculates correct subtotal
- [ ] Test case 5: CalculateRoundTripPrice applies discount correctly (10% discount)
- [ ] Test case 6: CalculateRoundTripPrice applies discount correctly (15% discount)
- [ ] Test case 7: CalculateRoundTripPrice calculates savings amount correctly
- [ ] Test case 8: CalculateRoundTripPrice handles 0% discount (no discount)
- [ ] Test case 9: CalculateRoundTripPrice handles 50% discount (maximum)
- [ ] Test case 10: CalculateRoundTripPrice with different seat classes (Economy + Business)
- [ ] Test case 11: CalculateRoundTripPrice with same seat class for both legs
- [ ] Additional coverage: Edge cases with very high/low prices

### PricingService

- [ ] Test case 1: GetRoundTripDiscount returns trip discount when set
- [ ] Test case 2: GetRoundTripDiscount returns default (0%) when not set
- [ ] Test case 3: UpdateRoundTripDiscountAsync updates discount successfully
- [ ] Test case 4: UpdateRoundTripDiscountAsync validates discount range (rejects < 0)
- [ ] Test case 5: UpdateRoundTripDiscountAsync validates discount range (rejects > 50)
- [ ] Test case 6: UpdateRoundTripDiscountAsync updates PriceLastUpdated timestamp
- [ ] Test case 7: UpdateRouteDiscountAsync updates all trips in route
- [ ] Test case 8: UpdateRouteDiscountAsync only updates trips for specified route
- [ ] Test case 9: UpdateRoundTripDiscountAsync returns false for non-existent trip
- [ ] Additional coverage: Bulk update with invalid data

### TicketRepository (Extended Methods)

- [ ] Test case 1: GetTicketsByBookingGroupId returns linked tickets
- [ ] Test case 2: GetTicketsByBookingGroupId returns empty for non-existent group
- [ ] Test case 3: GetRoundTripTicketsByUser returns only round-trip tickets
- [ ] Test case 4: GetRoundTripTicketsByUser excludes one-way tickets
- [ ] Additional coverage: Query performance with indexes

### Booking Service / UserController

- [ ] Test case 1: CreateOneWayBooking creates single ticket with Type=OneWay
- [ ] Test case 2: CreateRoundTripBooking creates two linked tickets
- [ ] Test case 3: CreateRoundTripBooking sets BookingGroupId correctly
- [ ] Test case 4: CreateRoundTripBooking links tickets bidirectionally
- [ ] Test case 5: CreateRoundTripBooking applies discount to total price
- [ ] Test case 6: CreateRoundTripBooking rolls back on failure (transaction)
- [ ] Test case 7: CreateRoundTripBooking sets correct Type for both tickets
- [ ] Additional coverage: Concurrent booking scenarios

## Integration Tests
**How do we test component interactions?**

- [ ] Integration scenario 1: Complete round-trip booking flow
  - User selects round-trip → Selects outbound → Selects return → 
  - Price calculated → Booking created → Tickets linked → Confirmation sent
  
- [ ] Integration scenario 2: Price calculation with discount from trip
  - Trip has 15% discount → Price calculator retrieves discount → 
  - Applies to total → Returns correct breakdown
  
- [ ] Integration scenario 3: Partner updates discount and booking reflects change
  - Partner sets 20% discount → New booking uses updated discount → 
  - Price calculation uses new discount
  
- [ ] Integration scenario 4: Round-trip booking failure and rollback
  - Create outbound ticket succeeds → Create return ticket fails → 
  - Transaction rolls back → No tickets created
  
- [ ] Integration scenario 5: Different seat classes in round-trip
  - User selects Economy for outbound → Business for return → 
  - Price calculated correctly for each → Discount applied to total

## End-to-End Tests
**What user flows need validation?**

- [ ] User flow 1: Book one-way ticket (regression test)
  - Select one-way → Choose flight → Confirm → 
  - Verify single ticket created → Verify Type=OneWay
  
- [ ] User flow 2: Book round-trip ticket
  - Select round-trip → Choose outbound → Choose return → 
  - Review price breakdown → Confirm → 
  - Verify two tickets created → Verify linked → Verify discount applied
  
- [ ] User flow 3: View round-trip booking in My Bookings
  - Complete round-trip booking → Navigate to My Bookings → 
  - Verify booking shown as single entry → Verify both tickets visible
  
- [ ] User flow 4: Partner manages pricing
  - Partner logs in → Navigate to Trips → Edit pricing → 
  - Set discount → Save → Verify discount saved → 
  - Create test booking → Verify new discount applied
  
- [ ] User flow 5: Price comparison display
  - Select round-trip → View price breakdown → 
  - Verify shows: Outbound + Return - Discount = Total → 
  - Verify shows savings amount
  
- [ ] Critical path testing: Round-trip booking with payment
  - Complete booking flow → Proceed to payment → 
  - Complete payment → Verify both tickets confirmed
  
- [ ] Regression of adjacent features: One-way booking still works
  - Test existing one-way booking flow → Verify no breaking changes

## Test Data
**What data do we use for testing?**

### Test Fixtures and Mocks
- Mock trips with various discount percentages (0%, 10%, 15%, 50%)
- Mock trips with different prices for each seat class
- Test users for booking creation
- Test partner user for price management

### Seed Data Requirements
- Trips with round-trip discounts set (0%, 10%, 15%)
- Trips without discounts (default 0%)
- Routes with multiple trips for return flight selection
- Existing one-way bookings (for regression testing)

### Test Database Setup
- Separate test database or in-memory database
- Seed test data before each test suite
- Clean up after tests

## Test Reporting & Coverage
**How do we verify and communicate test results?**

### Coverage Commands
```bash
dotnet test --collect:"XPlat Code Coverage"
dotnet reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage"
```

### Coverage Goals
- PriceCalculatorService: 100%
- PricingService: 100%
- Booking flow methods: 100%
- Controller actions: 90%+ (UI interactions may be lower)

### Coverage Gaps
- Document any files/functions below 100% with rationale
- UI interaction code may have lower coverage (acceptable)

## Manual Testing
**What requires human validation?**

### UI/UX Testing Checklist
- [ ] Ticket type selector is clear and intuitive
- [ ] Return date picker appears/disappears correctly
- [ ] Return flight selection is easy to use
- [ ] Price breakdown is clear and understandable
- [ ] Savings amount is prominently displayed
- [ ] Booking confirmation shows both tickets clearly
- [ ] My Bookings groups round-trip tickets correctly
- [ ] Partner pricing form is user-friendly
- [ ] Error messages are clear and helpful

### Browser/Device Compatibility
- [ ] Test on Chrome, Firefox, Edge, Safari
- [ ] Test on mobile devices (responsive design)
- [ ] Test date picker on different browsers
- [ ] Test form submission on all browsers

### Smoke Tests After Deployment
- [ ] One-way booking still works
- [ ] Round-trip booking creates linked tickets
- [ ] Price calculation is accurate
- [ ] Partner can update discounts
- [ ] My Bookings displays correctly

## Performance Testing
**How do we validate performance?**

### Load Testing Scenarios
- [ ] 100 concurrent round-trip bookings
- [ ] Price calculation under load (< 100ms target)
- [ ] Database queries with indexes perform well

### Stress Testing Approach
- [ ] Test with large number of linked tickets
- [ ] Test price calculation with many trips
- [ ] Test bulk discount update performance

### Performance Benchmarks
- Round-trip price calculation: < 100ms ✅
- Round-trip booking creation: < 3 seconds ✅
- Partner price update: < 1 second ✅
- Flight search with return options: < 2 seconds ✅

## Bug Tracking
**How do we manage issues?**

### Issue Tracking Process
- Log bugs in issue tracker
- Categorize by severity (Critical, High, Medium, Low)
- Assign to developer
- Verify fix with regression test

### Bug Severity Levels
- **Critical**: Booking creation fails, price calculation wrong
- **High**: Discount not applied, tickets not linked
- **Medium**: UI issues, display problems
- **Low**: Minor UI improvements, cosmetic issues

### Regression Testing Strategy
- Run full test suite after bug fixes
- Test related features for side effects
- Verify no breaking changes to existing features

