---
phase: implementation
title: Implementation Guide
description: Technical implementation notes, patterns, and code guidelines
---

# Implementation Guide

## Development Setup
**How do we get started?**

- Prerequisites and dependencies:
  - ASP.NET Core 8.0 (already in project)
  - No additional NuGet packages required (localization is built-in)
- Environment setup steps:
  - No additional setup needed
- Configuration needed:
  - Update Program.cs for localization services
  - Create Resources folder structure

## Code Structure
**How is the code organized?**

- Directory structure:
  ```
  Controllers/
    LanguageController.cs (new)
  Resources/
    Resources.resx (new)
    Resources.vi.resx (new)
    Resources.en.resx (new)
  Views/
    Shared/
      _LanguageSwitcher.cshtml (new partial view)
  Views/
    _ViewImports.cshtml (update)
  Program.cs (update)
  ```
- Module organization:
  - LanguageController handles language switching logic
  - Resource files contain all translatable strings
  - Language switcher is a reusable partial view
- Naming conventions:
  - Resource keys: PascalCase (e.g., "LoginButton", "WelcomeMessage")
  - Culture codes: "vi-VN" for Vietnamese, "en-US" for English

## Implementation Notes
**Key technical details to remember:**

### Core Features

#### Localization Configuration (Program.cs)
- Add services:
  ```csharp
  builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
  builder.Services.AddControllersWithViews()
      .AddViewLocalization();
  ```
- Configure supported cultures:
  ```csharp
  var supportedCultures = new[] { "vi-VN", "en-US" };
  var localizationOptions = new RequestLocalizationOptions()
      .SetDefaultCulture("vi-VN")
      .AddSupportedCultures(supportedCultures)
      .AddSupportedUICultures(supportedCultures);
  ```
- Add middleware:
  ```csharp
  app.UseRequestLocalization(localizationOptions);
  ```

#### LanguageController
- Validate culture parameter (whitelist approach)
- Set cookie with culture preference
- Implement safe redirect (validate returnUrl)
- Use HttpContext.Response.Cookies.Append for cookie setting

#### Resource Files
- Use Visual Studio resource editor or XML editor
- Key naming: Use descriptive, hierarchical names
- Example structure:
  - Common: "Common.Save", "Common.Cancel", "Common.Delete"
  - Pages: "Login.Title", "Login.Email", "Login.Password"
  - Messages: "Messages.Success", "Messages.Error"

#### Language Switcher Component
- Display current language
- Links to LanguageController.SetLanguage
- Preserve current URL as returnUrl
- Visual indicator of current language

### Patterns & Best Practices
- Always use IStringLocalizer in views: `@Localizer["Key"]`
- Use resource keys consistently across views
- Group related keys with prefixes (e.g., "Login.*", "Booking.*")
- Fallback to key name if translation missing (default behavior)
- Validate culture parameter to prevent injection attacks
- Sanitize returnUrl to prevent open redirect vulnerabilities

## Integration Points
**How do pieces connect?**

- LanguageController integrates with:
  - Cookie management (HttpContext.Response.Cookies)
  - Routing system (returnUrl redirect)
- Views integrate with:
  - IStringLocalizer (injected via _ViewImports)
  - Language switcher partial view
- Middleware integrates with:
  - Request pipeline (reads cookie, sets culture)
  - Thread.CurrentCulture and Thread.CurrentUICulture

## Error Handling
**How do we handle failures?**

- Invalid culture parameter:
  - Default to "vi-VN" if invalid
  - Log warning for invalid culture attempts
- Missing translations:
  - Fallback to resource key (default IStringLocalizer behavior)
  - Log missing keys in development
- Cookie disabled:
  - Fallback to default culture
  - Consider session storage as alternative
- Open redirect prevention:
  - Validate returnUrl is relative or same domain
  - Use Url.IsLocalUrl() helper

## Performance Considerations
**How do we keep it fast?**

- Resource files are compiled into assemblies (fast access)
- Cookie reading is minimal overhead
- No database queries required
- Consider caching resource lookups if needed (usually not necessary)

## Security Notes
**What security measures are in place?**

- Culture parameter validation:
  - Whitelist approach (only allow "vi-VN", "en-US")
  - Reject any other values
- ReturnUrl validation:
  - Use Url.IsLocalUrl() to prevent open redirects
  - Only allow relative URLs or same-domain URLs
- Cookie security:
  - Set HttpOnly flag (if storing sensitive data)
  - Consider SameSite attribute for CSRF protection



