---
phase: planning
title: Online Check-In - Project Planning
description: Task breakdown and implementation plan for online check-in feature
---

# Project Planning: Online Check-In Feature

## Milestones
**What are the major checkpoints?**

- [ ] Milestone 1: Database & Models - Check-in fields added, migrations applied
- [ ] Milestone 2: Core Services - Seat map, seat selection, boarding pass services implemented
- [ ] Milestone 3: Check-In Controller - Check-in flow working end-to-end
- [ ] Milestone 4: UI Components - All views created and integrated
- [ ] Milestone 5: Testing & Polish - All tests passing, feature complete

## Task Breakdown
**What specific work needs to be done?**

### Phase 1: Foundation (Database & Models)
- [ ] Task 1.1: Add check-in fields to Ticket model (IsCheckedIn, CheckInTime, BoardingPassUrl)
- [ ] Task 1.2: Create database migration for check-in fields
- [ ] Task 1.3: Apply migration and verify schema changes
- [ ] Task 1.4: Update AppDbContext with new field configurations

### Phase 2: Core Services
- [ ] Task 2.1: Create ISeatMapService interface
- [ ] Task 2.2: Implement SeatMapService (generate seat map from Trip configuration)
- [ ] Task 2.3: Create ISeatSelectionService interface
- [ ] Task 2.4: Implement SeatSelectionService (seat assignment and validation)
- [ ] Task 2.5: Create IBoardingPassService interface
- [ ] Task 2.6: Implement BoardingPassService (PDF generation using library like iTextSharp or QuestPDF)
- [ ] Task 2.7: Register all services in Program.cs dependency injection

### Phase 3: Repository Extensions
- [ ] Task 3.1: Add IsEligibleForCheckInAsync method to TicketRepository
- [ ] Task 3.2: Add UpdateCheckInStatusAsync method to TicketRepository
- [ ] Task 3.3: Add GetEligibleTicketsForCheckInAsync method to TicketRepository
- [ ] Task 3.4: Add seat availability query methods

### Phase 4: Check-In Controller
- [ ] Task 4.1: Create CheckInController with dependencies
- [ ] Task 4.2: Implement GET Index action (check-in form via PNR or logged-in)
- [ ] Task 4.3: Implement GET SeatMap action (display seat map)
- [ ] Task 4.4: Implement POST CheckIn action (process check-in)
- [ ] Task 4.5: Implement POST SelectSeat action (seat selection)
- [ ] Task 4.6: Implement GET MyTickets action (user's eligible tickets)
- [ ] Task 4.7: Add validation and error handling

### Phase 5: UI Components
- [ ] Task 5.1: Create Views/CheckIn/Index.cshtml (check-in form)
- [ ] Task 5.2: Create Views/CheckIn/SeatMap.cshtml (interactive seat map)
- [ ] Task 5.3: Create Views/CheckIn/Confirmation.cshtml (check-in success)
- [ ] Task 5.4: Create Views/CheckIn/MyTickets.cshtml (eligible tickets list)
- [ ] Task 5.5: Add JavaScript for seat map interaction
- [ ] Task 5.6: Add mobile-responsive styling
- [ ] Task 5.7: Integrate with existing navigation/theme

### Phase 6: Integration & Email
- [ ] Task 6.1: Integrate BoardingPassService with MailService
- [ ] Task 6.2: Create boarding pass email template
- [ ] Task 6.3: Add boarding pass storage (local file system or cloud storage)
- [ ] Task 6.4: Add boarding pass download endpoint
- [ ] Task 6.5: Test email delivery with boarding pass attachment

### Phase 7: Localization
- [ ] Task 7.1: Add check-in related localization keys to SharedResource.resx
- [ ] Task 7.2: Add Vietnamese translations to SharedResource.vi.resx
- [ ] Task 7.3: Add English translations to SharedResource.en.resx
- [ ] Task 7.4: Apply Localizer to all views and messages

### Phase 8: Testing
- [ ] Task 8.1: Write unit tests for SeatMapService
- [ ] Task 8.2: Write unit tests for SeatSelectionService
- [ ] Task 8.3: Write unit tests for BoardingPassService
- [ ] Task 8.4: Write integration tests for check-in flow
- [ ] Task 8.5: Write tests for concurrent seat selection
- [ ] Task 8.6: Manual testing of complete check-in flow
- [ ] Task 8.7: Test mobile responsiveness

## Dependencies
**What needs to happen in what order?**

### Task Dependencies
- Phase 1 (Database) must complete before Phase 2 (Services)
- Phase 2 (Services) must complete before Phase 3 (Repository)
- Phase 3 (Repository) must complete before Phase 4 (Controller)
- Phase 4 (Controller) must complete before Phase 5 (UI)
- Phase 2.6 (BoardingPassService) must complete before Phase 6 (Email integration)
- Phase 5 (UI) can partially overlap with Phase 6 (Integration)

### External Dependencies
- PDF generation library (iTextSharp, QuestPDF, or similar) - NuGet package
- Email service already exists (MailService) - no new dependency
- Storage for boarding passes (local file system initially, can upgrade to cloud later)

### Technical Dependencies
- Existing Ticket and Trip models
- Existing authentication/session system
- Existing PNR lookup functionality
- Existing MailService

## Timeline & Estimates
**When will things be done?**

### Estimated Effort
- Phase 1 (Foundation): 2-3 hours
- Phase 2 (Core Services): 6-8 hours
- Phase 3 (Repository): 2-3 hours
- Phase 4 (Controller): 4-5 hours
- Phase 5 (UI): 6-8 hours
- Phase 6 (Integration): 3-4 hours
- Phase 7 (Localization): 2-3 hours
- Phase 8 (Testing): 6-8 hours

**Total Estimated Effort**: 31-42 hours

### Target Timeline
- Week 1: Phases 1-3 (Foundation, Services, Repository)
- Week 2: Phases 4-5 (Controller, UI)
- Week 3: Phases 6-8 (Integration, Localization, Testing)

### Buffer for Unknowns
- PDF library learning curve: +2 hours
- Seat map UI complexity: +2 hours
- Email attachment issues: +1 hour
- Testing edge cases: +2 hours

## Risks & Mitigation
**What could go wrong?**

### Technical Risks
1. **Risk**: PDF generation library complexity
   - **Mitigation**: Choose well-documented library (QuestPDF recommended), start with simple template
   
2. **Risk**: Concurrent seat selection race conditions
   - **Mitigation**: Use database transactions, optimistic locking, validate before assignment
   
3. **Risk**: Seat map generation performance with many seats
   - **Mitigation**: Cache seat map data, optimize queries, consider pagination if needed

4. **Risk**: Boarding pass storage and access
   - **Mitigation**: Use secure file storage, implement proper access control, consider cloud storage for scalability

### Integration Risks
1. **Risk**: Email service may not handle large PDF attachments
   - **Mitigation**: Test with sample PDFs, consider file size limits, provide download link as alternative

2. **Risk**: Check-in window calculation errors (timezone issues)
   - **Mitigation**: Use UTC consistently, test with different timezones, add clear error messages

### User Experience Risks
1. **Risk**: Seat map not intuitive on mobile devices
   - **Mitigation**: Design mobile-first, test on various devices, provide zoom/pan functionality

2. **Risk**: Check-in process too complex
   - **Mitigation**: Simplify flow, provide clear instructions, add progress indicators

## Resources Needed
**What do we need to succeed?**

### Team Members and Roles
- Backend developer: Services, Controller, Repository
- Frontend developer: UI components, JavaScript, styling
- QA: Testing, edge cases, user experience

### Tools and Services
- PDF generation library (NuGet package)
- Text editor/IDE (existing)
- Git for version control (existing)
- Email service (existing MailService)

### Infrastructure
- Development database (SQLite - existing)
- File storage for boarding passes (local initially)
- Email service configuration (existing)

### Documentation/Knowledge
- PDF generation library documentation
- Existing codebase knowledge (Ticket, Trip models)
- Airline check-in process understanding (from research)




