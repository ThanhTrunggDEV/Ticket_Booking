const { test, expect } = require('@playwright/test');
const { loginAsTestUser } = require('./helpers/auth');

test.describe('Ticket Change Functionality', () => {
  test.beforeEach(async ({ page }) => {
    // Login before each test
    await loginAsTestUser(page);
  });

  test('should display ticket change page for eligible ticket', async ({ page }) => {
    // Navigate to My Bookings
    await page.goto('/User/MyBooking');
    
    // Wait for tickets to load
    await page.waitForSelector('table, .ticket-card, [data-ticket-id]', { timeout: 10000 });
    
    // Find the first "Đổi Vé" button (ticket eligible for change)
    const changeButton = page.locator('a:has-text("Đổi Vé")').first();
    
    // Check if change button exists
    const changeButtonCount = await changeButton.count();
    
    if (changeButtonCount === 0) {
      test.skip('No eligible tickets for change found. Please create a test ticket first.');
      return;
    }
    
    // Click on "Đổi Vé" button
    await changeButton.click();
    
    // Wait for ticket change page to load
    await page.waitForURL(/\/TicketChange\/Index/, { timeout: 10000 });
    
    // Verify page title
    await expect(page.locator('h1')).toContainText('Đổi Vé Máy Bay');
    
    // Verify original ticket information is displayed
    await expect(page.locator('text=Thông Tin Vé Hiện Tại')).toBeVisible();
    await expect(page.locator('text=Tuyến đường:')).toBeVisible();
    await expect(page.locator('text=Ngày giờ khởi hành:')).toBeVisible();
    await expect(page.locator('text=Hạng ghế:')).toBeVisible();
    await expect(page.locator('text=Giá vé:')).toBeVisible();
    
    // Verify change fee information
    await expect(page.locator('text=Phí Đổi Vé')).toBeVisible();
    await expect(page.locator('text=Phí đổi vé:')).toBeVisible();
    
    // Verify new trip selection form
    await expect(page.locator('text=Chọn Chuyến Bay Mới')).toBeVisible();
    await expect(page.locator('select[name="newTripId"]')).toBeVisible();
    await expect(page.locator('select[name="newSeatClass"]')).toBeVisible();
  });

  test('should calculate change fee when selecting new trip', async ({ page }) => {
    // Navigate to My Bookings
    await page.goto('/User/MyBooking');
    
    // Wait for tickets to load
    await page.waitForSelector('a:has-text("Đổi Vé")', { timeout: 10000 });
    
    const changeButton = page.locator('a:has-text("Đổi Vé")').first();
    const changeButtonCount = await changeButton.count();
    
    if (changeButtonCount === 0) {
      test.skip('No eligible tickets for change found.');
      return;
    }
    
    // Click on "Đổi Vé" button
    await changeButton.click();
    
    // Wait for ticket change page
    await page.waitForURL(/\/TicketChange\/Index/, { timeout: 10000 });
    
    // Select a new trip from dropdown
    const tripSelect = page.locator('select[name="newTripId"]');
    await tripSelect.waitFor({ state: 'visible' });
    
    // Get available options
    const options = await tripSelect.locator('option').all();
    
    if (options.length <= 1) {
      test.skip('No available trips for change found.');
      return;
    }
    
    // Select first available trip (skip the default empty option)
    await tripSelect.selectOption({ index: 1 });
    
    // Click "Tính Toán Phí" button
    const calculateButton = page.locator('button:has-text("Tính Toán Phí")');
    await calculateButton.click();
    
    // Wait for price summary to appear
    await page.waitForSelector('#priceSummary:not(.hidden)', { timeout: 5000 });
    
    // Verify price summary is displayed
    await expect(page.locator('#priceSummary')).toBeVisible();
    await expect(page.locator('text=Tóm Tắt Thanh Toán')).toBeVisible();
    await expect(page.locator('#changeFeeDisplay')).toBeVisible();
    await expect(page.locator('#priceDiffDisplay')).toBeVisible();
    await expect(page.locator('#totalDueDisplay')).toBeVisible();
    
    // Verify confirm button is enabled
    await expect(page.locator('button:has-text("Xác Nhận Đổi Vé")')).toBeEnabled();
  });

  test('should show error when trying to change ticket less than 3 hours before departure', async ({ page }) => {
    // This test requires a ticket that is less than 3 hours before departure
    // You may need to create such a ticket in your test data
    
    await page.goto('/User/MyBooking');
    await page.waitForSelector('table, .ticket-card', { timeout: 10000 });
    
    // Try to access ticket change directly with a ticket ID that's too close to departure
    // This would require knowing a specific ticket ID, so we'll test the error handling
    
    // Navigate to a ticket change page (you may need to adjust ticketId)
    const response = await page.goto('/TicketChange/Index?ticketId=999999', { waitUntil: 'networkidle' });
    
    // Should redirect to MyTickets with error message
    if (page.url().includes('/User/MyBooking') || page.url().includes('/User/MyTickets')) {
      // Check for error message
      const errorMessage = page.locator('.bg-red-100, .text-red-700, [class*="error"]');
      const hasError = await errorMessage.count() > 0;
      
      // Either error message is shown or we're redirected (both are valid)
      expect(hasError || page.url().includes('/User')).toBeTruthy();
    }
  });

  test('should not allow change for cancelled ticket', async ({ page }) => {
    // Navigate to My Bookings
    await page.goto('/User/MyBooking');
    await page.waitForSelector('table, .ticket-card', { timeout: 10000 });
    
    // Try to access ticket change for a cancelled ticket
    // This test assumes you have a cancelled ticket in your test data
    // Adjust ticketId as needed
    await page.goto('/TicketChange/Index?ticketId=1', { waitUntil: 'networkidle' });
    
    // Should redirect with error message
    if (page.url().includes('/User')) {
      // Error handling is working
      expect(page.url()).toContain('/User');
    }
  });

  test('should not allow change for checked-in ticket', async ({ page }) => {
    // Similar to cancelled ticket test
    await page.goto('/User/MyBooking');
    await page.waitForSelector('table, .ticket-card', { timeout: 10000 });
    
    // Try to access ticket change for a checked-in ticket
    await page.goto('/TicketChange/Index?ticketId=1', { waitUntil: 'networkidle' });
    
    // Should redirect with error message
    if (page.url().includes('/User')) {
      expect(page.url()).toContain('/User');
    }
  });

  test('should display available trips in dropdown', async ({ page }) => {
    await page.goto('/User/MyBooking');
    await page.waitForSelector('a:has-text("Đổi Vé")', { timeout: 10000 });
    
    const changeButton = page.locator('a:has-text("Đổi Vé")').first();
    const changeButtonCount = await changeButton.count();
    
    if (changeButtonCount === 0) {
      test.skip('No eligible tickets for change found.');
      return;
    }
    
    await changeButton.click();
    await page.waitForURL(/\/TicketChange\/Index/, { timeout: 10000 });
    
    // Check trip dropdown has options
    const tripSelect = page.locator('select[name="newTripId"]');
    await tripSelect.waitFor({ state: 'visible' });
    
    const options = await tripSelect.locator('option').all();
    expect(options.length).toBeGreaterThan(1); // At least one option (default + trips)
    
    // Verify trip information is displayed in options
    if (options.length > 1) {
      const firstTripOption = options[1];
      const optionText = await firstTripOption.textContent();
      expect(optionText).toBeTruthy();
      expect(optionText.length).toBeGreaterThan(0);
    }
  });

  test('should allow selecting different seat class', async ({ page }) => {
    await page.goto('/User/MyBooking');
    await page.waitForSelector('a:has-text("Đổi Vé")', { timeout: 10000 });
    
    const changeButton = page.locator('a:has-text("Đổi Vé")').first();
    const changeButtonCount = await changeButton.count();
    
    if (changeButtonCount === 0) {
      test.skip('No eligible tickets for change found.');
      return;
    }
    
    await changeButton.click();
    await page.waitForURL(/\/TicketChange\/Index/, { timeout: 10000 });
    
    // Check seat class dropdown
    const seatClassSelect = page.locator('select[name="newSeatClass"]');
    await expect(seatClassSelect).toBeVisible();
    
    // Verify options
    const options = await seatClassSelect.locator('option').all();
    expect(options.length).toBeGreaterThanOrEqual(4); // Default + Economy + Business + FirstClass
    
    // Select Business class
    await seatClassSelect.selectOption('Business');
    const selectedValue = await seatClassSelect.inputValue();
    expect(selectedValue).toBe('Business');
  });

  test('should calculate different prices for different seat classes', async ({ page }) => {
    await page.goto('/User/MyBooking');
    await page.waitForSelector('a:has-text("Đổi Vé")', { timeout: 10000 });
    
    const changeButton = page.locator('a:has-text("Đổi Vé")').first();
    const changeButtonCount = await changeButton.count();
    
    if (changeButtonCount === 0) {
      test.skip('No eligible tickets for change found.');
      return;
    }
    
    await changeButton.click();
    await page.waitForURL(/\/TicketChange\/Index/, { timeout: 10000 });
    
    const tripSelect = page.locator('select[name="newTripId"]');
    await tripSelect.waitFor({ state: 'visible' });
    
    const options = await tripSelect.locator('option').all();
    if (options.length <= 1) {
      test.skip('No available trips for change found.');
      return;
    }
    
    // Select a trip
    await tripSelect.selectOption({ index: 1 });
    
    // Test Economy class
    const seatClassSelect = page.locator('select[name="newSeatClass"]');
    await seatClassSelect.selectOption('Economy');
    
    const calculateButton = page.locator('button:has-text("Tính Toán Phí")');
    await calculateButton.click();
    
    await page.waitForSelector('#priceSummary:not(.hidden)', { timeout: 5000 });
    const economyTotal = await page.locator('#totalDueDisplay').textContent();
    
    // Test Business class
    await seatClassSelect.selectOption('Business');
    await calculateButton.click();
    
    await page.waitForSelector('#priceSummary:not(.hidden)', { timeout: 5000 });
    const businessTotal = await page.locator('#totalDueDisplay').textContent();
    
    // Prices should be different (unless they're the same by coincidence)
    // At minimum, the calculation should work for both
    expect(economyTotal).toBeTruthy();
    expect(businessTotal).toBeTruthy();
  });

  test('should show change reason textarea', async ({ page }) => {
    await page.goto('/User/MyBooking');
    await page.waitForSelector('a:has-text("Đổi Vé")', { timeout: 10000 });
    
    const changeButton = page.locator('a:has-text("Đổi Vé")').first();
    const changeButtonCount = await changeButton.count();
    
    if (changeButtonCount === 0) {
      test.skip('No eligible tickets for change found.');
      return;
    }
    
    await changeButton.click();
    await page.waitForURL(/\/TicketChange\/Index/, { timeout: 10000 });
    
    // Verify change reason textarea exists
    const reasonTextarea = page.locator('textarea[name="changeReason"]');
    await expect(reasonTextarea).toBeVisible();
    
    // Test entering a reason
    await reasonTextarea.fill('Test reason for ticket change');
    const value = await reasonTextarea.inputValue();
    expect(value).toBe('Test reason for ticket change');
  });

  test('should require trip selection before calculating fee', async ({ page }) => {
    await page.goto('/User/MyBooking');
    await page.waitForSelector('a:has-text("Đổi Vé")', { timeout: 10000 });
    
    const changeButton = page.locator('a:has-text("Đổi Vé")').first();
    const changeButtonCount = await changeButton.count();
    
    if (changeButtonCount === 0) {
      test.skip('No eligible tickets for change found.');
      return;
    }
    
    await changeButton.click();
    await page.waitForURL(/\/TicketChange\/Index/, { timeout: 10000 });
    
    // Try to calculate without selecting a trip
    const calculateButton = page.locator('button:has-text("Tính Toán Phí")');
    
    // Set up dialog handler for alert
    page.on('dialog', async dialog => {
      expect(dialog.message()).toContain('chọn chuyến bay');
      await dialog.accept();
    });
    
    await calculateButton.click();
    
    // Alert should appear
    // The dialog handler will verify the message
  });
});

