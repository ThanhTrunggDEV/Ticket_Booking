---
phase: testing
title: PNR Booking Code - Testing Strategy
description: Testing approach and test cases for PNR feature
---

# Testing Strategy: PNR Booking Code Feature

## Test Coverage Goals
**What level of testing do we aim for?**

- **Unit test coverage target**: 100% of new/changed code
  - PNR Helper: 100%
  - Repository extensions: 100%
  - Controller logic: 100%

- **Integration test scope**: Critical paths + error handling
  - Booking flow with PNR generation
  - PNR lookup flow
  - Database operations

- **End-to-end test scenarios**: Key user journeys
  - Complete booking with PNR
  - PNR lookup by user
  - Admin search by PNR

- **Alignment with requirements**: All acceptance criteria covered

## Unit Tests
**What individual components need testing?**

### Component: PNR Helper (`Helpers/PNRHelper.cs`)

#### Test Case 1: GeneratePNR Returns Valid Format
- **Description**: Verify PNR generation returns 6-character alphanumeric string
- **Covers**: Basic generation logic
- **Assertions**:
  - PNR length is exactly 6
  - Contains only valid characters (A-Z, 0-9, excluding 0, O, 1, I, L)
  - Is uppercase

#### Test Case 2: GeneratePNR Excludes Confusing Characters
- **Description**: Verify generated PNRs don't contain 0, O, 1, I, L
- **Covers**: Character set validation
- **Assertions**:
  - No '0' in PNR
  - No 'O' in PNR
  - No '1' in PNR
  - No 'I' in PNR
  - No 'L' in PNR

#### Test Case 3: GeneratePNR Produces Different Values
- **Description**: Verify multiple calls produce different PNRs (high probability)
- **Covers**: Randomness
- **Assertions**:
  - Multiple calls produce different values (test 100 times)
  - At least 90% uniqueness (statistical test)

#### Test Case 4: GenerateUniquePNRAsync Handles Collisions
- **Description**: Verify retry mechanism works when collision occurs
- **Covers**: Uniqueness validation and retry logic
- **Setup**: Mock repository to return collision then success
- **Assertions**:
  - Retries when collision detected
  - Returns unique PNR after retry
  - Throws exception after max retries

#### Test Case 5: GenerateUniquePNRAsync Returns Unique PNR
- **Description**: Verify uniqueness check works correctly
- **Covers**: Repository integration
- **Setup**: Mock repository with existing PNRs
- **Assertions**:
  - Checks repository for existence
  - Returns PNR not in repository

#### Test Case 6: IsValidPNRFormat Validates Correctly
- **Description**: Verify PNR format validation
- **Covers**: Format validation logic
- **Test Cases**:
  - Valid PNR: "A1B2C3" → true
  - Too short: "A1B2C" → false
  - Too long: "A1B2C3D" → false
  - Invalid chars: "A1B2C@" → false
  - Null/empty: null, "" → false
  - Case insensitive: "a1b2c3" → true (normalized)

### Component: Ticket Repository Extensions (`Repositories/TicketRepository.cs`)

#### Test Case 1: GetByPNRAsync Returns Correct Ticket
- **Description**: Verify PNR lookup returns correct ticket
- **Covers**: Basic lookup functionality
- **Setup**: Seed database with ticket having known PNR
- **Assertions**:
  - Returns ticket with matching PNR
  - Includes related entities (Trip, User)
  - Returns null for non-existent PNR

#### Test Case 2: GetByPNRAsync Is Case Insensitive
- **Description**: Verify case-insensitive lookup
- **Covers**: Case handling
- **Setup**: Ticket with PNR "ABC123"
- **Assertions**:
  - "abc123" finds ticket
  - "ABC123" finds ticket
  - "AbC123" finds ticket

#### Test Case 3: GetByPNRAndEmailAsync Validates Email
- **Description**: Verify email validation in lookup
- **Covers**: Email matching
- **Setup**: Ticket with PNR and user email
- **Assertions**:
  - Correct PNR + email → returns ticket
  - Correct PNR + wrong email → returns null
  - Wrong PNR + correct email → returns null

#### Test Case 4: GetByPNRAndEmailAsync Is Case Insensitive
- **Description**: Verify case-insensitive email matching
- **Covers**: Email case handling
- **Setup**: Ticket with user email "User@Example.com"
- **Assertions**:
  - "user@example.com" finds ticket
  - "USER@EXAMPLE.COM" finds ticket

#### Test Case 5: PNRExistsAsync Checks Existence
- **Description**: Verify existence check works
- **Covers**: Existence validation
- **Setup**: Seed database with known PNRs
- **Assertions**:
  - Existing PNR → true
  - Non-existent PNR → false
  - Case insensitive check

#### Test Case 6: GetByPNRAsync Handles Null PNR
- **Description**: Verify null PNR handling
- **Covers**: Null safety
- **Assertions**:
  - Null PNR → returns null (doesn't throw)
  - Empty PNR → returns null

### Component: PNR Controller (`Controllers/PNRController.cs`)

#### Test Case 1: Lookup GET Returns View
- **Description**: Verify GET request returns lookup form
- **Covers**: View rendering
- **Assertions**:
  - Returns view result
  - View contains form fields

#### Test Case 2: Lookup POST Validates Input
- **Description**: Verify input validation
- **Covers**: Validation logic
- **Test Cases**:
  - Missing PNR → error
  - Missing email → error
  - Invalid PNR format → error
  - Invalid email format → error

#### Test Case 3: Lookup POST Returns Ticket on Success
- **Description**: Verify successful lookup returns ticket details
- **Covers**: Successful lookup flow
- **Setup**: Mock repository with matching ticket
- **Assertions**:
  - Returns view with ticket data
  - Ticket data is correct

#### Test Case 4: Lookup POST Handles Not Found
- **Description**: Verify error handling for not found
- **Covers**: Error handling
- **Setup**: Mock repository returning null
- **Assertions**:
  - Returns error message
  - Doesn't expose sensitive information

#### Test Case 5: Lookup POST Is Case Insensitive
- **Description**: Verify case-insensitive lookup
- **Covers**: Case handling in controller
- **Assertions**:
  - Lowercase PNR works
  - Mixed case PNR works

## Integration Tests
**How do we test component interactions?**

### Integration Scenario 1: Complete Booking Flow with PNR
- **Description**: Test full booking flow generates and saves PNR
- **Steps**:
  1. User initiates booking
  2. Ticket created with PNR
  3. PNR saved to database
  4. Verify PNR is unique
- **Assertions**:
  - Ticket has PNR assigned
  - PNR is valid format
  - PNR is unique in database
  - Ticket can be retrieved by PNR

### Integration Scenario 2: PNR Lookup Flow
- **Description**: Test complete PNR lookup process
- **Steps**:
  1. Create ticket with known PNR
  2. User submits PNR + email
  3. System validates and returns ticket
- **Assertions**:
  - Lookup succeeds with correct credentials
  - Returns correct ticket data
  - Includes related entities

### Integration Scenario 3: PNR Lookup Failure
- **Description**: Test lookup with invalid credentials
- **Steps**:
  1. Create ticket with PNR
  2. User submits wrong PNR or email
  3. System returns error
- **Assertions**:
  - Returns appropriate error message
  - Doesn't leak information about existing PNRs
  - Doesn't throw exceptions

### Integration Scenario 4: PNR Collision Handling
- **Description**: Test system handles PNR collisions gracefully
- **Steps**:
  1. Manually insert ticket with specific PNR
  2. Attempt to generate PNR (simulate collision)
  3. System retries and generates new PNR
- **Assertions**:
  - Retry mechanism works
  - Eventually generates unique PNR
  - Booking completes successfully

### Integration Scenario 5: Database Migration
- **Description**: Test database migration applies correctly
- **Steps**:
  1. Run migration
  2. Verify column added
  3. Verify index created
  4. Test with existing data
- **Assertions**:
  - Migration succeeds
  - Column exists and is nullable
  - Index created correctly
  - Existing tickets remain functional

## End-to-End Tests
**What user flows need validation?**

### User Flow 1: Complete Booking with PNR
- **Description**: User books ticket and receives PNR
- **Steps**:
  1. User logs in
  2. User selects trip and seat class
  3. User confirms booking
  4. System generates PNR
  5. User sees booking confirmation with PNR
  6. User receives email with PNR
- **Validation**:
  - PNR displayed on confirmation page
  - PNR included in email
  - PNR is valid format
  - User can use PNR for lookup

### User Flow 2: PNR Lookup Without Login
- **Description**: User looks up booking using PNR without logging in
- **Steps**:
  1. User navigates to PNR lookup page
  2. User enters PNR and email
  3. System validates and displays ticket
- **Validation**:
  - Lookup works without authentication
  - Correct ticket displayed
  - Error handling for invalid input

### User Flow 3: Admin Search by PNR
- **Description**: Admin searches for ticket using PNR
- **Steps**:
  1. Admin logs in
  2. Admin navigates to ticket management
  3. Admin searches by PNR
  4. System displays matching ticket
- **Validation**:
  - Search finds ticket
  - Results display correctly
  - Case-insensitive search works

## Test Data
**What data do we use for testing?**

### Test Fixtures and Mocks
- **PNR Helper Tests**: No fixtures needed (pure function)
- **Repository Tests**: 
  - Seed database with test tickets
  - Known PNRs: "ABC123", "XYZ789"
  - Test users with known emails
- **Controller Tests**: Mock repository responses

### Seed Data Requirements
- Test users (different emails)
- Test trips
- Test tickets with known PNRs
- Test tickets with null PNRs (backward compatibility)

### Test Database Setup
- Use in-memory database for unit tests
- Use test SQLite database for integration tests
- Clean database between tests (or use transactions)

## Test Reporting & Coverage
**How do we verify and communicate test results?**

### Coverage Commands
- Run unit tests: `dotnet test --filter "FullyQualifiedName~PNR"`
- Run with coverage: `dotnet test /p:CollectCoverage=true`
- Coverage threshold: 100% for new code

### Coverage Gaps
- Document any code below 100% coverage
- Provide rationale for gaps
- Plan to address gaps if critical

### Manual Testing Outcomes
- Document manual test results
- Include screenshots if applicable
- Note any issues found

## Manual Testing
**What requires human validation?**

### UI/UX Testing Checklist
- [ ] PNR displayed prominently on ticket page
- [ ] PNR format is readable (good font, size)
- [ ] PNR lookup form is intuitive
- [ ] Error messages are clear and helpful
- [ ] PNR in email is clearly labeled
- [ ] Mobile responsiveness (if applicable)

### Browser/Device Compatibility
- [ ] Chrome (latest)
- [ ] Firefox (latest)
- [ ] Edge (latest)
- [ ] Mobile browsers (if applicable)

### Smoke Tests After Deployment
- [ ] Create new booking → verify PNR generated
- [ ] Lookup existing booking → verify works
- [ ] Admin search → verify works
- [ ] Email contains PNR → verify

## Performance Testing
**How do we validate performance?**

### Load Testing Scenarios
- Generate 1000 PNRs → measure time
- Lookup 1000 PNRs → measure time
- Concurrent bookings → verify no collisions

### Performance Benchmarks
- PNR generation: < 10ms (target)
- PNR lookup: < 200ms (target)
- Uniqueness check: < 50ms (target)

## Bug Tracking
**How do we manage issues?**

### Issue Tracking Process
- Document bugs found during testing
- Prioritize by severity
- Track fixes and verification

### Bug Severity Levels
- **Critical**: PNR collision causes data corruption
- **High**: PNR not generated, lookup fails
- **Medium**: UI display issues, minor validation problems
- **Low**: Cosmetic issues, edge cases

### Regression Testing Strategy
- Run full test suite after fixes
- Test related features (booking, tickets)
- Verify backward compatibility maintained


