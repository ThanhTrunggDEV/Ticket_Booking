# Playwright Tests for Ticket Booking

## Setup

1. Install dependencies:
```bash
npm install
```

2. Install Playwright browsers:
```bash
npx playwright install
```

## Running Tests

### Run all tests
```bash
npm test
```

### Run ticket change tests only
```bash
npm run test:ticket-change
```

### Run tests in UI mode (interactive)
```bash
npm run test:ui
```

### Run tests in headed mode (see browser)
```bash
npm run test:headed
```

### Run tests in debug mode
```bash
npm run test:debug
```

## Test Structure

- `tests/ticket-change.spec.js` - Tests for ticket change functionality
- `tests/helpers/auth.js` - Authentication helper functions

## Prerequisites

Before running tests, ensure:

1. The application is running on `http://localhost:5117` (or update `baseURL` in `playwright.config.js`)
2. You have test user credentials:
   - Email: `test@example.com`
   - Password: `Test123!`
   
   Update these in `tests/helpers/auth.js` if your test user is different.

3. You have at least one eligible ticket in the database:
   - PaymentStatus = Success
   - IsCancelled = false
   - IsCheckedIn = false
   - DepartureTime > 3 hours from now
   - Trip.Status != Cancelled

## Test Scenarios

The ticket change tests cover:

1. ✅ Display ticket change page for eligible ticket
2. ✅ Calculate change fee when selecting new trip
3. ✅ Show error for tickets less than 3 hours before departure
4. ✅ Prevent change for cancelled tickets
5. ✅ Prevent change for checked-in tickets
6. ✅ Display available trips in dropdown
7. ✅ Allow selecting different seat class
8. ✅ Calculate different prices for different seat classes
9. ✅ Show change reason textarea
10. ✅ Require trip selection before calculating fee

## Configuration

Edit `playwright.config.js` to:
- Change base URL
- Adjust timeout settings
- Configure browsers
- Set up test retries

## Troubleshooting

### Tests fail with "No eligible tickets found"
- Create a test ticket that meets the eligibility criteria
- Ensure the ticket is linked to a valid user account

### Tests fail with login errors
- Verify test user credentials in `tests/helpers/auth.js`
- Ensure the user exists in the database

### Application not starting
- Make sure .NET SDK is installed
- Check that port 5117 is available
- Verify database connection

