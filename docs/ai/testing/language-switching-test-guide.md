# Language Switching - Test Guide

## Infrastructure Test Checklist

### ✅ Pre-Test Verification
- [x] Build successful (no errors)
- [x] Resource files created (Resources.resx, Resources.vi.resx, Resources.en.resx)
- [x] LanguageController created
- [x] Language switcher component added to layout
- [x] Sample resource keys added for testing

### Manual Testing Steps

#### 1. Start the Application
```bash
cd "D:\Coding Space\C#\Ticket_Booking"
dotnet run
```

#### 2. Test Language Switcher Visibility
- [ ] Navigate to `/Login` page
- [ ] Verify language switcher (VI | EN) is visible in the navigation bar (top right)
- [ ] Verify current language indicator (highlighted button)

#### 3. Test Language Switching - Vietnamese to English
- [ ] Click on "EN" button in language switcher
- [ ] Verify page redirects back to Login page
- [ ] Verify text changes:
  - "Đăng nhập hành khách" → "Passenger Login"
  - "Địa chỉ email" → "Email Address"
  - "Mật khẩu" → "Password"
  - "Ghi nhớ đăng nhập" → "Remember Me"
  - "Đăng nhập ngay" → "Board Now"
- [ ] Check browser cookies: Should have cookie named `.AspNetCore.Culture` with value `c=en-US|uic=en-US`

#### 4. Test Language Switching - English to Vietnamese
- [ ] Click on "VI" button in language switcher
- [ ] Verify page redirects back to Login page
- [ ] Verify text changes back to Vietnamese
- [ ] Check browser cookies: Should have cookie value `c=vi-VN|uic=vi-VN`

#### 5. Test Cookie Persistence
- [ ] Set language to English
- [ ] Close browser tab/window
- [ ] Reopen browser and navigate to `/Login`
- [ ] Verify page loads in English (cookie persisted)
- [ ] Set language to Vietnamese
- [ ] Refresh page (F5)
- [ ] Verify page stays in Vietnamese

#### 6. Test Language Switcher from Different Pages
- [ ] Navigate to different pages (if logged in)
- [ ] Click language switcher from each page
- [ ] Verify redirects back to the same page
- [ ] Verify language changes correctly

#### 7. Test Edge Cases
- [ ] Try invalid culture parameter: `/Language/SetLanguage?culture=fr-FR`
  - Should default to Vietnamese (vi-VN)
- [ ] Test with returnUrl parameter: `/Language/SetLanguage?culture=en-US&returnUrl=/Login`
  - Should redirect to /Login
- [ ] Test with external returnUrl: `/Language/SetLanguage?culture=en-US&returnUrl=https://evil.com`
  - Should NOT redirect to external URL (security check)

### Expected Results

✅ **Success Criteria:**
- Language switcher visible on all pages
- Language switching works from any page
- Cookie persists across browser sessions
- Text changes immediately after switching
- No broken text or missing translations
- Security: Open redirect prevention works

### Known Limitations (To Be Fixed in Phase 3)

⚠️ **Current State:**
- Only Login page has localized text (sample)
- Other pages still show hardcoded English text
- Validation messages not yet localized
- Some UI elements may not be localized yet

### Next Steps After Testing

If all tests pass:
1. Proceed to Phase 3: Extract and translate all hardcoded strings
2. Add more resource keys for all pages
3. Localize validation messages
4. Complete translation for all views

---

## Quick Test Commands

### Check Cookie Value (Browser DevTools)
```javascript
// In browser console:
document.cookie.split(';').find(c => c.includes('Culture'))
```

### Test Language Endpoint Directly
```
http://localhost:5000/Language/SetLanguage?culture=en-US&returnUrl=/Login
http://localhost:5000/Language/SetLanguage?culture=vi-VN&returnUrl=/Login
```

### Verify Resource Files
- Check `Resources/Resources.vi.resx` has Vietnamese translations
- Check `Resources/Resources.en.resx` has English translations
- Verify keys match between files





