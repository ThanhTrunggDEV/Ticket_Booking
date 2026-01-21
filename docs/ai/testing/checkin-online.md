---
phase: testing
title: Online Check-In - Testing Strategy
description: Testing approach, test cases, and quality assurance for online check-in
---

# Testing Strategy: Online Check-In Feature

## Test Coverage Goals
**What level of testing do we aim for?**

- Unit test coverage target: 100% of new/changed code (Services, Repository methods)
- Integration test scope: Check-in flow, seat selection, boarding pass generation
- End-to-end test scenarios: Complete check-in process from start to boarding pass receipt
- Alignment with requirements/design acceptance criteria: All acceptance criteria must be testable

## Unit Tests
**What individual components need testing?**

### SeatMapService
- [ ] Test case 1: GetSeatMap returns correct seat map for Economy class
- [ ] Test case 2: GetSeatMap returns correct seat map for Business class
- [ ] Test case 3: GetSeatMap returns correct seat map for First Class
- [ ] Test case 4: GetSeatMap correctly calculates available seats
- [ ] Test case 5: GetSeatMap excludes booked seats from available list
- [ ] Test case 6: IsSeatAvailable returns true for available seat
- [ ] Test case 7: IsSeatAvailable returns false for booked seat
- [ ] Additional coverage: Edge cases (empty trip, all seats booked)

### SeatSelectionService
- [ ] Test case 1: AssignSeatAsync successfully assigns available seat
- [ ] Test case 2: AssignSeatAsync fails when seat already taken
- [ ] Test case 3: AssignSeatAsync fails when ticket not found
- [ ] Test case 4: AssignSeatAsync uses transaction to prevent race conditions
- [ ] Test case 5: ChangeSeatAsync successfully changes seat
- [ ] Test case 6: ChangeSeatAsync fails when new seat not available
- [ ] Test case 7: ValidateSeatAvailability returns correct results
- [ ] Additional coverage: Concurrent seat selection scenarios

### BoardingPassService
- [ ] Test case 1: GenerateBoardingPassAsync creates valid PDF file
- [ ] Test case 2: GenerateBoardingPassAsync includes all required information
- [ ] Test case 3: GenerateBoardingPassAsync handles missing QR code gracefully
- [ ] Test case 4: GenerateBoardingPassAsync saves file to correct location
- [ ] Test case 5: SendBoardingPassEmailAsync sends email with attachment
- [ ] Test case 6: SendBoardingPassEmailAsync handles email failure gracefully
- [ ] Additional coverage: PDF format validation, file size limits

### TicketRepository Extensions
- [ ] Test case 1: IsEligibleForCheckInAsync returns true for eligible ticket
- [ ] Test case 2: IsEligibleForCheckInAsync returns false for unpaid ticket
- [ ] Test case 3: IsEligibleForCheckInAsync returns false outside check-in window
- [ ] Test case 4: IsEligibleForCheckInAsync returns false for already checked-in ticket
- [ ] Test case 5: UpdateCheckInStatusAsync updates status correctly
- [ ] Test case 6: GetEligibleTicketsForCheckInAsync returns correct tickets
- [ ] Additional coverage: Edge cases (cancelled flights, past flights)

### CheckInController
- [ ] Test case 1: Index action displays check-in form for eligible ticket
- [ ] Test case 2: Index action shows error for ineligible ticket
- [ ] Test case 3: SeatMap action displays seat map correctly
- [ ] Test case 4: CheckIn action processes check-in successfully
- [ ] Test case 5: CheckIn action fails for ineligible ticket
- [ ] Test case 6: SelectSeat action assigns seat successfully
- [ ] Test case 7: SelectSeat action fails for unavailable seat
- [ ] Additional coverage: Error handling, validation

## Integration Tests
**How do we test component interactions?**

- [ ] Integration scenario 1: Complete check-in flow (eligibility check → seat selection → check-in → boarding pass)
- [ ] Integration scenario 2: Seat selection with concurrent requests (race condition handling)
- [ ] Integration scenario 3: Check-in with email delivery (end-to-end email flow)
- [ ] Integration scenario 4: Check-in via PNR lookup (public access flow)
- [ ] Integration scenario 5: Check-in via logged-in user (authenticated flow)
- [ ] Integration scenario 6: Failure mode - check-in fails, no partial state saved
- [ ] Integration scenario 7: Failure mode - seat selection fails, ticket unchanged

## End-to-End Tests
**What user flows need validation?**

- [ ] User flow 1: Passenger checks in via PNR lookup → selects seat → receives boarding pass
- [ ] User flow 2: Logged-in user views eligible tickets → checks in → receives boarding pass
- [ ] User flow 3: Passenger tries to check-in outside window → sees appropriate error
- [ ] User flow 4: Passenger selects unavailable seat → sees error → selects different seat
- [ ] User flow 5: Passenger checks in successfully → downloads boarding pass → receives email
- [ ] Critical path testing: Complete check-in process under normal conditions
- [ ] Regression of adjacent features: Verify existing booking flow still works

## Test Data
**What data do we use for testing?**

### Test Fixtures and Mocks
- Mock Trip with different seat configurations (Economy, Business, First Class)
- Mock Tickets with various states (paid, unpaid, checked-in, not checked-in)
- Mock Users for authentication testing
- Test PDF files for boarding pass validation

### Seed Data Requirements
- Test trips with different departure times (within/outside check-in window)
- Test tickets with different payment statuses
- Test tickets with different check-in statuses
- Test seats in various states (available, booked, selected)

### Test Database Setup
- Use in-memory database for unit tests
- Use test SQLite database for integration tests
- Clean up test data after each test run

## Test Reporting & Coverage
**How do we verify and communicate test results?**

### Coverage Commands
- Run unit tests: `dotnet test --filter "FullyQualifiedName~CheckIn"`
- Generate coverage report: `dotnet test /p:CollectCoverage=true`
- Coverage threshold: 100% for new services and repository methods

### Coverage Gaps
- Document any files/functions below 100% coverage with rationale
- Controller actions may have lower coverage (integration tested instead)
- View components tested manually

### Test Reports
- Unit test results: Console output + test report files
- Integration test results: Detailed logs with database state
- Manual testing outcomes: Documented in testing checklist

## Manual Testing
**What requires human validation?**

### UI/UX Testing Checklist
- [ ] Check-in form is intuitive and easy to use
- [ ] Seat map is clear and interactive
- [ ] Seat selection provides visual feedback
- [ ] Check-in confirmation page shows all relevant information
- [ ] Boarding pass is readable and contains all required fields
- [ ] Error messages are clear and helpful
- [ ] Mobile responsiveness works on various devices
- [ ] Accessibility: Keyboard navigation, screen reader compatibility

### Browser/Device Compatibility
- [ ] Chrome (desktop and mobile)
- [ ] Firefox (desktop and mobile)
- [ ] Safari (desktop and mobile)
- [ ] Edge (desktop)
- [ ] Test on various screen sizes (mobile, tablet, desktop)

### Smoke Tests After Deployment
- [ ] Check-in process works end-to-end
- [ ] Seat selection works correctly
- [ ] Boarding pass generation works
- [ ] Email delivery works
- [ ] No console errors in browser
- [ ] No server errors in logs

## Performance Testing
**How do we validate performance?**

### Load Testing Scenarios
- [ ] 10 concurrent check-ins on same flight
- [ ] 50 concurrent seat selections
- [ ] 100 boarding pass generations
- [ ] Measure response times under load

### Stress Testing Approach
- [ ] Test with maximum number of seats (all seats booked)
- [ ] Test with many concurrent check-ins
- [ ] Test boarding pass generation with large files
- [ ] Test database performance with many check-ins

### Performance Benchmarks
- Check-in process: < 30 seconds (target met?)
- Seat map loading: < 2 seconds (target met?)
- Seat selection: < 1 second (target met?)
- Boarding pass generation: < 3 seconds (target met?)

## Bug Tracking
**How do we manage issues?**

### Issue Tracking Process
- Document all bugs found during testing
- Categorize by severity (Critical, High, Medium, Low)
- Track resolution status
- Verify fixes with regression tests

### Bug Severity Levels
- **Critical**: Check-in completely broken, data loss
- **High**: Check-in works but with major issues (wrong seat, missing data)
- **Medium**: Minor issues (UI glitches, unclear messages)
- **Low**: Cosmetic issues, minor improvements

### Regression Testing Strategy
- Run full test suite after each bug fix
- Test related features to ensure no side effects
- Re-test fixed bugs to ensure resolution
- Document test results




