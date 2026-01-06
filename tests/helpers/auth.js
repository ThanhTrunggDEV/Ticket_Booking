/**
 * Authentication helper functions for Playwright tests
 */

/**
 * Login as a user
 * @param {import('@playwright/test').Page} page
 * @param {string} email
 * @param {string} password
 */
async function login(page, email, password) {
  await page.goto('/Login');
  await page.fill('input[name="email"]', email);
  await page.fill('input[name="password"]', password);
  await page.click('button[type="submit"]');
  // Wait for navigation after login (User role redirects to /User/Index)
  await page.waitForURL(/\/User/, { timeout: 10000 });
}

/**
 * Login as test user (you may need to adjust credentials)
 * @param {import('@playwright/test').Page} page
 */
async function loginAsTestUser(page) {
  // Default test user - adjust these credentials based on your test data
  await login(page, 'test@example.com', 'Test123!');
}

module.exports = { login, loginAsTestUser };

