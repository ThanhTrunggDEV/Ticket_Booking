---
phase: requirements
title: Requirements & Problem Understanding
description: Clarify the problem space, gather requirements, and define success criteria
---

# Requirements & Problem Understanding

## Problem Statement
**What problem are we solving?**

- The Ticket Booking application currently displays all prices in USD only, which creates barriers for Vietnamese users who are more familiar with VND (Vietnamese Dong)
- Users need to manually calculate VND equivalents, which is inconvenient and error-prone
- Admin and Partner users managing revenue and pricing also need to view financial data in both currencies for better decision-making
- Current situation: All prices (flight tickets, booking totals, payments, revenue) are hardcoded in USD
- Workaround: Users must use external currency converters or calculate manually

## Goals & Objectives
**What do we want to achieve?**

- Primary goals:
  - Enable users to view all prices in their selected currency (VND or USD)
  - Provide near real-time currency conversion using live exchange rates with caching
  - Allow users to select their preferred currency (VND or USD)
  - Persist currency preference across sessions (similar to language preference)
  - Display prices in selected currency throughout the application (flight search, booking, payment, dashboard)
  - Show clear indication that converted prices are for display purposes and payment is processed in USD
- Secondary goals:
  - Support future expansion to additional currencies if needed
  - Provide accurate exchange rates from reliable external API
  - Cache exchange rates to reduce API calls and improve performance (refresh hourly)
  - Show both currencies side-by-side for transparency (optional enhancement)
  - Display exchange rate information to users (e.g., "1 USD = 24,500 VND") for transparency
- Non-goals (what's explicitly out of scope):
  - Automatic currency detection based on user location
  - Historical exchange rate tracking
  - Currency conversion for user-generated content (reviews, comments)
  - Multi-currency payment processing (payment will still be processed in USD, only display changes)
  - Admin interface for manual exchange rate override (initially)

## User Stories & Use Cases
**How will users interact with the solution?**

- As a Vietnamese passenger, I want to view flight prices in VND so that I can better understand the cost in my local currency
- As an international passenger, I want to view prices in USD so that I can compare with other booking platforms
- As a user, I want my currency preference to be remembered so that I don't have to switch currencies every time I visit
- As a user, I want to switch currencies from any page so that I can change currency context without navigating away
- As a user, I want to see a clear indication that displayed prices are converted for convenience and actual payment is in USD
- As an admin, I want to view revenue and financial reports in both VND and USD so that I can make informed business decisions
- As a partner, I want to view my earnings and trip prices in both currencies so that I can manage pricing effectively
- Key workflows:
  1. User selects currency preference (VND or USD) from currency switcher
  2. Application fetches current exchange rate from external API (or uses cached rate)
  3. All prices are converted and displayed in selected currency
  4. Currency preference is saved (cookie/session)
  5. On next visit, application loads in preferred currency
  6. Exchange rate is refreshed periodically (e.g., every hour) to ensure accuracy
- Edge cases to consider:
  - First-time visitor (no currency preference set) - default to USD (or VND if language is Vietnamese)
  - Exchange rate API unavailable - fallback to cached rate; if cache expired, use default rate from configuration
  - Invalid exchange rate response - use fallback rate and log error
  - Currency switching during active booking process - prices update but booking data remains valid (no reset)
  - Currency switching on pages with validation errors - prices update, form data preserved
  - Network timeout when fetching exchange rate - use cached rate or fallback
  - Exchange rate significantly different from cached rate - refresh cache and update prices
  - User switches currency multiple times rapidly - prevent API spam, use cache

## Success Criteria
**How will we know when we're done?**

- Users can switch between VND and USD from any page
- Currency preference persists across browser sessions
- All price displays (flight prices, booking totals, payment amounts, revenue) show in selected currency
- Exchange rates are updated near real-time with hourly caching (balance between accuracy and performance)
- Currency conversion works without page reload (or with minimal reload)
- No broken price displays or calculation errors visible
- Performance impact is minimal (<200ms for currency conversion, <50ms with cache)
- Exchange rate API integration is reliable with proper error handling
- Fallback mechanism works when API is unavailable (cached rate → default rate)
- Exchange rates are accurate to at least 2 decimal places
- Prices are rounded appropriately (VND: no decimals, USD: 2 decimals)
- Users see clear indication that converted prices are approximate and payment is in USD
- Currency conversion calculations are mathematically correct (no rounding errors)

## Constraints & Assumptions
**What limitations do we need to work within?**

- Technical constraints:
  - Must work with existing ASP.NET Core MVC architecture
  - Should integrate with existing localization infrastructure
  - Must be compatible with existing session/cookie management
  - Exchange rate API must be free or have reasonable pricing
  - API rate limits must be considered for caching strategy
- Business constraints:
  - Payment processing will remain in USD (only display changes)
  - Exchange rates are for display purposes only
  - Simple implementation preferred (no complex currency management systems)
  - Only VND and USD initially
- Time/budget constraints:
  - Keep implementation simple and straightforward
  - Use free tier of exchange rate API if possible
- Assumptions we're making:
  - Exchange rate API will be available and reliable (with fallback mechanisms)
  - Users will see disclaimer that converted prices are approximate and payment is in USD
  - Actual payment will still be processed in USD (no multi-currency payment processing)
  - Exchange rate fluctuations are acceptable for display purposes
  - Default currency is USD for all users (or VND if user's language is Vietnamese)
  - Exchange rates cached for 1 hour provide acceptable accuracy vs real-time
  - API rate limits (typically 1,500 requests/month free tier) are sufficient with caching

## Questions & Open Items
**What do we still need to clarify?**

- Which exchange rate API to use? (Options: ExchangeRate-API, Fixer.io, CurrencyAPI, etc.)
  - **Recommendation**: ExchangeRate-API (free tier: 1,500 requests/month, no API key required)
- How often should exchange rates be refreshed? (Every request, hourly, daily?)
  - **Recommendation**: Hourly refresh with caching (balance between accuracy and API limits)
- Should we show both currencies side-by-side or only selected currency?
  - **Recommendation**: Only selected currency initially (can add side-by-side as enhancement)
- Should currency preference be stored in user profile (database) or just cookie/session?
  - **Recommendation**: Cookie/session initially (similar to language preference), can add database storage later
- What should be the default currency for Vietnamese users vs international users?
  - **Recommendation**: USD for all users initially, or VND if user's language is Vietnamese (can be configurable)
- Should we display exchange rate information to users (e.g., "1 USD = 24,500 VND")?
  - **Recommendation**: Optional enhancement - show in tooltip or small text near prices
- How to handle exchange rate API failures gracefully?
  - **Recommendation**: Use cached rate if available, otherwise use default rate from configuration, log error
- Should we cache exchange rates in database or memory?
  - **Recommendation**: Memory cache (IMemoryCache) initially, can upgrade to distributed cache (Redis) if needed
- What is the acceptable accuracy for exchange rates? (e.g., 4 decimal places)
  - **Recommendation**: 2-4 decimal places for calculation, display rounded appropriately

