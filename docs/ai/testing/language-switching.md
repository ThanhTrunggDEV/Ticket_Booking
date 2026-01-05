---
phase: testing
title: Testing Strategy
description: Define testing approach, test cases, and quality assurance
---

# Testing Strategy

## Test Coverage Goals
**What level of testing do we aim for?**

- Unit test coverage target: 100% of LanguageController logic
- Integration test scope: Language switching flow, cookie persistence, redirect behavior
- End-to-end test scenarios: User switches language, navigates pages, verifies persistence
- Alignment with requirements: All success criteria must be testable and verified

## Unit Tests
**What individual components need testing?**

### LanguageController
- [ ] Test SetLanguage with valid culture "vi-VN": Sets cookie correctly, redirects properly
- [ ] Test SetLanguage with valid culture "en-US": Sets cookie correctly, redirects properly
- [ ] Test SetLanguage with invalid culture: Defaults to "vi-VN", handles gracefully
- [ ] Test SetLanguage with returnUrl: Redirects to specified URL (if local)
- [ ] Test SetLanguage with external returnUrl: Prevents open redirect, redirects to home
- [ ] Test SetLanguage without returnUrl: Redirects to referrer or home
- [ ] Test cookie expiration: Cookie set with appropriate expiration
- Additional coverage: Edge cases (null parameters, empty strings)

### Resource Files
- [ ] Test all resource keys exist in both vi-VN and en-US files
- [ ] Test resource key retrieval: Returns correct translation for current culture
- [ ] Test missing key fallback: Returns key name if translation missing
- Additional coverage: Verify no duplicate keys, proper encoding

## Integration Tests
**How do we test component interactions?**

- [ ] Integration scenario 1: User switches language, cookie is set, next request uses new culture
- [ ] Integration scenario 2: Language preference persists across multiple requests
- [ ] Integration scenario 3: Language switcher displays current language correctly
- [ ] Integration scenario 4: Views render with correct language based on cookie
- [ ] Integration scenario 5: Language switch during form submission (preserves form state or handles gracefully)
- [ ] Integration scenario 6: Language switch on error page (handles gracefully)

## End-to-End Tests
**What user flows need validation?**

- [ ] User flow 1: New user visits site → Defaults to Vietnamese → Switches to English → All text changes
- [ ] User flow 2: User switches language → Navigates to different pages → Language persists
- [ ] User flow 3: User closes browser → Reopens site → Language preference maintained
- [ ] Critical path testing: Language switching works from all major pages (Login, Booking, Profile, etc.)
- [ ] Regression of adjacent features: Language switching doesn't break existing functionality

## Test Data
**What data do we use for testing?**

- Test fixtures and mocks:
  - Mock HttpContext for controller tests
  - Mock IStringLocalizer for view tests
- Seed data requirements:
  - None (no database involved)
- Test database setup:
  - Not applicable

## Test Reporting & Coverage
**How do we verify and communicate test results?**

- Coverage commands: `dotnet test --collect:"XPlat Code Coverage"`
- Coverage gaps: Document any untested edge cases with rationale
- Manual testing outcomes: Document results in this file
- Test scenarios checklist:
  - [ ] All pages tested in Vietnamese
  - [ ] All pages tested in English
  - [ ] Language switcher tested on all major pages
  - [ ] Cookie persistence verified across sessions
  - [ ] No broken text or missing translations

## Manual Testing
**What requires human validation?**

- UI/UX testing checklist:
  - [ ] Language switcher is visible and accessible
  - [ ] Current language is clearly indicated
  - [ ] Language switch is intuitive (click to switch)
  - [ ] All text changes immediately after switch
  - [ ] No layout breaks with longer/shorter translations
  - [ ] Language switcher works on mobile devices
- Browser/device compatibility:
  - [ ] Chrome (desktop and mobile)
  - [ ] Firefox (desktop and mobile)
  - [ ] Edge (desktop)
  - [ ] Safari (if applicable)
- Smoke tests after deployment:
  - [ ] Language switching works in production
  - [ ] Cookie persistence works in production
  - [ ] No console errors related to localization

## Performance Testing
**How do we validate performance?**

- Load testing scenarios:
  - Language switching under normal load (not critical, but verify no degradation)
- Stress testing approach:
  - Not required for this feature (minimal performance impact)
- Performance benchmarks:
  - Language switch response time: <100ms
  - Page load with localization: <50ms overhead

## Bug Tracking
**How do we manage issues?**

- Issue tracking process:
  - Document any bugs found during testing
  - Track missing translations
  - Track UI/UX issues
- Bug severity levels:
  - Critical: Language switching doesn't work, breaks application
  - High: Missing translations, cookie not persisting
  - Medium: UI issues, minor translation errors
  - Low: Cosmetic issues, edge cases
- Regression testing strategy:
  - Test language switching after any view changes
  - Verify translations after adding new features



