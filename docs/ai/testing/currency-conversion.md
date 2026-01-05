---
phase: testing
title: Testing Strategy
description: Define testing approach, test cases, and quality assurance
---

# Testing Strategy

## Test Coverage Goals
**What level of testing do we aim for?**

- Unit test coverage target: 100% of new/changed code (CurrencyService, CurrencyController, helpers)
- Integration test scope: Currency switching flow, API integration, cache behavior, price display across views
- End-to-end test scenarios: User switches currency, views prices, completes booking with currency conversion
- Alignment with requirements/design acceptance criteria: All success criteria must be testable and verified

## Unit Tests
**What individual components need testing?**

### CurrencyService
- [ ] Test GetExchangeRateAsync - successful API call returns correct rate
- [ ] Test GetExchangeRateAsync - cache hit returns cached rate
- [ ] Test GetExchangeRateAsync - API failure uses fallback rate
- [ ] Test GetExchangeRateAsync - cache miss calls API and caches result
- [ ] Test ConvertAmountAsync - converts USD to VND correctly
- [ ] Test ConvertAmountAsync - converts VND to USD correctly
- [ ] Test ConvertAmountAsync - handles zero amounts
- [ ] Test ConvertAmountAsync - handles negative amounts (edge case)
- [ ] Test FormatPriceAsync - formats VND with correct symbol and formatting
- [ ] Test FormatPriceAsync - formats USD with correct symbol and formatting
- [ ] Test FormatPriceAsync - handles large amounts correctly
- [ ] Additional coverage: Cache expiration, error handling, logging

### CurrencyController
- [ ] Test SetCurrency - valid currency (VND) sets cookie correctly
- [ ] Test SetCurrency - valid currency (USD) sets cookie correctly
- [ ] Test SetCurrency - invalid currency defaults to USD
- [ ] Test SetCurrency - redirects to returnUrl if valid
- [ ] Test SetCurrency - redirects to referrer if returnUrl invalid
- [ ] Test SetCurrency - redirects to default page if both invalid
- [ ] Test SetCurrency - prevents open redirect attacks
- [ ] Additional coverage: Cookie options, security validation

### ExchangeRateApiClient
- [ ] Test GetExchangeRateAsync - successful API response parsing
- [ ] Test GetExchangeRateAsync - handles API errors (404, 500, etc.)
- [ ] Test GetExchangeRateAsync - handles network timeout
- [ ] Test GetExchangeRateAsync - handles invalid JSON response
- [ ] Test GetExchangeRateAsync - handles missing exchange rate in response
- [ ] Additional coverage: Retry logic, authentication (if required)

### Price Display Helpers
- [ ] Test DisplayPrice - formats VND correctly
- [ ] Test DisplayPrice - formats USD correctly
- [ ] Test DisplayPrice - handles null or zero amounts
- [ ] Test DisplayPrice - uses currency service for conversion
- [ ] Additional coverage: Edge cases, error handling

## Integration Tests
**How do we test component interactions?**

- [ ] Integration scenario 1: Currency switching flow
  - User clicks currency switcher
  - Cookie is set
  - Page reloads with new currency
  - Prices are displayed in new currency
- [ ] Integration scenario 2: API integration with caching
  - First request fetches from API and caches
  - Second request uses cache
  - Cache expiration triggers API refresh
- [ ] Integration scenario 3: API failure fallback
  - API unavailable
  - System uses cached rate
  - If cache expired, uses fallback rate
  - User experience continues normally
- [ ] Integration scenario 4: Price display across views
  - Flight search shows prices in selected currency
  - Booking page shows prices in selected currency
  - Payment confirmation shows prices in selected currency
  - Dashboard shows revenue in selected currency
- [ ] Integration scenario 5: Currency preference persistence
  - User selects currency
  - User navigates to different pages
  - Currency preference is maintained
  - User closes browser and returns
  - Currency preference is still set

## End-to-End Tests
**What user flows need validation?**

- [ ] User flow 1: Currency switching during flight search
  - User searches for flights
  - User switches from USD to VND
  - Flight prices update to VND
  - User selects flight and proceeds to booking
- [ ] User flow 2: Currency switching during booking
  - User is on booking page
  - User switches currency
  - Booking prices update
  - User completes booking
- [ ] User flow 3: Admin viewing revenue in different currencies
  - Admin logs in
  - Admin views dashboard in USD
  - Admin switches to VND
  - Revenue displays in VND
- [ ] User flow 4: Partner managing trips with currency conversion
  - Partner views trip management
  - Partner switches currency
  - Trip prices display in selected currency
- [ ] Critical path testing: Complete booking flow with currency conversion
- [ ] Regression of adjacent features: Language switching still works, existing price displays still work

## Test Data
**What data do we use for testing?**

- Test fixtures and mocks:
  - Mock HttpClient for API responses
  - Mock IMemoryCache for cache testing
  - Sample exchange rates (e.g., 1 USD = 24,500 VND)
  - Test prices (various amounts: $100, $1,000, $10,000)
- Seed data requirements:
  - Sample trips with prices
  - Sample bookings with totals
  - Sample revenue data
- Test database setup:
  - No database changes required (currency preference in cookie)

## Test Reporting & Coverage
**How do we verify and communicate test results?**

- Coverage commands: `dotnet test --collect:"XPlat Code Coverage"`
- Coverage thresholds: 100% for CurrencyService, CurrencyController, helpers
- Coverage gaps: Document any gaps with rationale
- Manual testing outcomes: Document in testing checklist
- Test reports: Generate and review coverage reports

## Manual Testing
**What requires human validation?**

- UI/UX testing checklist:
  - [ ] Currency switcher is visible and accessible
  - [ ] Currency switcher shows current currency
  - [ ] Prices update immediately when currency changes
  - [ ] Currency symbols display correctly (₫, $)
  - [ ] Number formatting is correct (commas, decimals)
  - [ ] Prices are readable and not truncated
  - [ ] Currency switcher works on all pages
  - [ ] Currency preference persists across sessions
- Browser/device compatibility:
  - [ ] Test on Chrome, Firefox, Edge, Safari
  - [ ] Test on mobile devices
  - [ ] Test cookie behavior across browsers
- Smoke tests after deployment:
  - [ ] Currency switching works in production
  - [ ] API integration works in production
  - [ ] Prices display correctly
  - [ ] No performance degradation

## Performance Testing
**How do we validate performance?**

- Load testing scenarios:
  - Multiple users switching currencies simultaneously
  - High traffic on pages with price displays
  - API rate limit handling
- Stress testing approach:
  - Simulate API failures
  - Test cache behavior under load
  - Test fallback mechanisms
- Performance benchmarks:
  - Currency conversion: <50ms
  - Currency switching: <100ms
  - Page load with currency conversion: <200ms additional time

## Bug Tracking
**How do we manage issues?**

- Issue tracking process:
  - Log all test failures
  - Document edge cases found during testing
  - Track API integration issues
- Bug severity levels:
  - Critical: Currency conversion incorrect, API failures break application
  - High: Currency switcher not working, prices not updating
  - Medium: Formatting issues, minor display problems
  - Low: UI polish, edge cases
- Regression testing strategy:
  - Run full test suite before each release
  - Test currency conversion with existing features
  - Verify language switching still works
  - Verify existing price displays still work



