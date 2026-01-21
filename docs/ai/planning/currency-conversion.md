---
phase: planning
title: Project Planning & Task Breakdown
description: Break down work into actionable tasks and estimate timeline
---

# Project Planning & Task Breakdown

## Milestones
**What are the major checkpoints?**

- [ ] Milestone 1: Core Infrastructure - Currency service, API integration, caching
- [ ] Milestone 2: UI Integration - Currency switcher, price display helpers, view updates
- [ ] Milestone 3: Testing & Polish - Unit tests, integration tests, error handling, documentation

## Task Breakdown
**What specific work needs to be done?**

### Phase 1: Foundation & Core Service
- [ ] Task 1.1: Research and select exchange rate API provider
  - Evaluate free options (ExchangeRate-API, Fixer.io free tier, CurrencyAPI)
  - Compare rate limits, accuracy, reliability
  - Select provider and obtain API key if needed
  - Estimate: 1-2 hours

- [ ] Task 1.2: Create CurrencyService interface and implementation
  - Define ICurrencyService interface
  - Implement GetExchangeRateAsync method
  - Implement ConvertAmountAsync method
  - Implement FormatPriceAsync method
  - Add error handling and logging
  - Estimate: 3-4 hours

- [ ] Task 1.3: Implement Exchange Rate API client
  - Create HttpClient wrapper for selected API
  - Implement API request/response models
  - Add authentication handling (if required)
  - Implement retry logic and error handling
  - Add response parsing and validation
  - Estimate: 2-3 hours

- [ ] Task 1.4: Implement caching strategy
  - Configure IMemoryCache or IDistributedCache
  - Implement cache key strategy
  - Set cache expiration (e.g., 1 hour)
  - Add cache refresh logic
  - Handle cache misses and API fallback
  - Estimate: 2-3 hours

- [ ] Task 1.5: Register services in Program.cs
  - Register ICurrencyService with DI container
  - Configure HttpClient for API client
  - Configure cache settings
  - Add API key to configuration (appsettings.json)
  - Estimate: 1 hour

### Phase 2: Currency Switching & Middleware
- [ ] Task 2.1: Create CurrencyController
  - Implement SetCurrency action
  - Add currency validation (whitelist: VND, USD)
  - Set cookie with currency preference
  - Implement safe redirect (prevent open redirect)
  - Add fallback to referrer or default page
  - Estimate: 2 hours

- [ ] Task 2.2: Create currency switcher component
  - Create _CurrencySwitcher.cshtml partial view
  - Display current currency indicator
  - Add links to CurrencyController.SetCurrency
  - Preserve returnUrl
  - Add visual styling
  - Estimate: 2 hours

- [ ] Task 2.3: Integrate currency switcher into layout
  - Add currency switcher to _Layout.cshtml
  - Position near language switcher
  - Ensure visibility for all user types
  - Estimate: 1 hour

- [ ] Task 2.4: Create currency middleware (optional)
  - Or extend existing localization middleware
  - Read currency from cookie
  - Set currency context for request
  - Fallback to default (USD)
  - Estimate: 2 hours

### Phase 3: View Integration & Price Display
- [ ] Task 3.1: Create price display helpers
  - Create HtmlHelper extension for DisplayPrice
  - Create TagHelper for price display
  - Format currency with proper symbols (₫, $)
  - Handle number formatting (commas, decimals)
  - Estimate: 2-3 hours

- [ ] Task 3.2: Update User views with currency conversion
  - Update Views/User/Index.cshtml (flight prices)
  - Update Views/User/BookTrip.cshtml (booking prices)
  - Update Views/User/MyBooking.cshtml (booking history)
  - Update Views/User/Profile.cshtml (if shows prices)
  - Estimate: 3-4 hours

- [ ] Task 3.3: Update Admin views with currency conversion
  - Update Views/Admin/Index.cshtml (revenue, stats)
  - Update Views/Admin/TripManagement.cshtml (trip prices)
  - Update Views/Admin/UserManagement.cshtml (if shows prices)
  - Update Views/Admin/PartnerManagement.cshtml (if shows prices)
  - Estimate: 2-3 hours

- [ ] Task 3.4: Update Partner views with currency conversion
  - Update Views/Partner/Index.cshtml (revenue, bookings)
  - Update Views/Partner/TripsManagement.cshtml (trip prices)
  - Update Views/Partner/CompaniesManagement.cshtml (if shows prices)
  - Estimate: 2-3 hours

- [ ] Task 3.5: Update payment and booking views
  - Update payment confirmation pages
  - Update ticket views
  - Ensure all price displays use currency conversion
  - Estimate: 2-3 hours

### Phase 4: Error Handling & Edge Cases
- [ ] Task 4.1: Implement fallback mechanisms
  - Default exchange rate if API fails
  - Cached rate fallback
  - Graceful degradation (show USD if conversion fails)
  - Estimate: 2 hours

- [ ] Task 4.2: Add error logging and monitoring
  - Log API failures
  - Log cache misses
  - Add health check for currency service
  - Estimate: 1-2 hours

- [ ] Task 4.3: Handle edge cases
  - Currency switching during active booking
  - Invalid exchange rate responses
  - Network timeouts
  - API rate limit exceeded
  - Estimate: 2-3 hours

### Phase 5: Testing
- [ ] Task 5.1: Unit tests for CurrencyService
  - Test GetExchangeRateAsync (success, cache hit, API failure)
  - Test ConvertAmountAsync
  - Test FormatPriceAsync
  - Test caching logic
  - Estimate: 4-5 hours

- [ ] Task 5.2: Unit tests for CurrencyController
  - Test SetCurrency validation
  - Test cookie setting
  - Test redirect logic
  - Estimate: 2 hours

- [ ] Task 5.3: Integration tests
  - Test currency switching flow
  - Test price display across views
  - Test API integration (with mock)
  - Test cache behavior
  - Estimate: 3-4 hours

- [ ] Task 5.4: Manual testing
  - Test currency switching from all pages
  - Test price display accuracy
  - Test with API unavailable
  - Test currency preference persistence
  - Estimate: 2-3 hours

## Dependencies
**What needs to happen in what order?**

- Task dependencies:
  - Task 1.1 (API selection) → Task 1.3 (API client) → Task 1.2 (CurrencyService)
  - Task 1.4 (Caching) → Task 1.2 (CurrencyService)
  - Task 1.5 (Service registration) → All service tasks
  - Task 2.1 (CurrencyController) → Task 2.2 (Currency switcher)
  - Task 2.2 (Currency switcher) → Task 2.3 (Layout integration)
  - Task 3.1 (Price helpers) → All view update tasks (3.2-3.5)
- External dependencies:
  - Exchange Rate API availability and reliability
  - API key/authentication setup
- Team/resource dependencies:
  - Access to exchange rate API (free tier or paid)
  - Configuration access for API keys

## Timeline & Estimates
**When will things be done?**

- Phase 1 (Foundation): ~10-13 hours
- Phase 2 (Currency Switching): ~7 hours
- Phase 3 (View Integration): ~11-15 hours
- Phase 4 (Error Handling): ~5-7 hours
- Phase 5 (Testing): ~11-14 hours
- **Total estimated effort**: ~44-56 hours (~5-7 working days)
- Buffer for unknowns: +20% (~9-11 hours)
- **Total with buffer**: ~53-67 hours (~6.5-8.5 working days)

## Risks & Mitigation
**What could go wrong?**

- Technical risks:
  - Exchange rate API unavailable or unreliable
    - Mitigation: Implement robust caching, fallback rates, multiple API providers
  - API rate limits exceeded
    - Mitigation: Aggressive caching, rate limiting on our side
  - Performance impact from API calls
    - Mitigation: Caching strategy, async operations, timeout handling
- Resource risks:
  - API provider changes pricing or terms
    - Mitigation: Abstract API client, support multiple providers
- Dependency risks:
  - External API downtime
    - Mitigation: Fallback to cached rates, default rates, graceful degradation

## Resources Needed
**What do we need to succeed?**

- Team members and roles:
  - Backend developer (service implementation, API integration)
  - Frontend developer (view updates, UI components)
  - QA (testing, validation)
- Tools and services:
  - Exchange Rate API account (free tier or paid)
  - Development environment with internet access (for API calls)
  - Testing tools (unit test framework, integration test tools)
- Infrastructure:
  - Caching infrastructure (IMemoryCache or Redis)
  - Configuration management for API keys
- Documentation/knowledge:
  - Exchange Rate API documentation
  - ASP.NET Core caching documentation
  - Existing localization implementation (for reference)





