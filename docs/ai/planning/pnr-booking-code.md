---
phase: planning
title: PNR Booking Code - Project Planning
description: Task breakdown and implementation plan for PNR feature
---

# Project Planning: PNR Booking Code Feature

## Milestones
**What are the major checkpoints?**

- [ ] Milestone 1: Database & Model Setup
  - PNR column added to database
  - Ticket model updated
  - Migration created and applied

- [ ] Milestone 2: PNR Generation Core
  - PNR helper created
  - Uniqueness validation implemented
  - Integration with booking flow

- [ ] Milestone 3: PNR Lookup Functionality
  - PNR lookup controller created
  - Lookup views implemented
  - Email validation added

- [ ] Milestone 4: UI Integration
  - PNR displayed in ticket views
  - PNR included in emails
  - Admin/Partner search by PNR

- [ ] Milestone 5: Testing & Documentation
  - Unit tests written
  - Integration tests completed
  - Manual testing verified

## Task Breakdown
**What specific work needs to be done?**

### Phase 1: Foundation (Database & Models)

#### Task 1.1: Update Ticket Model
- [ ] Add `PNR` property to `Ticket` class
  - Type: `string?` (nullable)
  - Max length: 6 characters
  - Location: `Models/DomainModels/Ticket.cs`
- **Estimate**: 15 minutes
- **Dependencies**: None

#### Task 1.2: Create Database Migration
- [ ] Add PNR column to Tickets table
  - Column: `PNR TEXT NULL`
  - Create unique index: `IX_Tickets_PNR` (case-insensitive)
  - Update `AppDbContext.OnModelCreating`
- **Estimate**: 30 minutes
- **Dependencies**: Task 1.1

#### Task 1.3: Apply Migration
- [ ] Run migration to update database
- [ ] Verify column and index created correctly
- **Estimate**: 10 minutes
- **Dependencies**: Task 1.2

### Phase 2: Core Features (PNR Generation)

#### Task 2.1: Create PNR Helper Interface
- [ ] Create `IPNRHelper` interface
  - `GeneratePNR()`: Generate random PNR
  - `GenerateUniquePNRAsync()`: Generate with uniqueness check
  - `IsValidPNRFormat()`: Validate format
- **Estimate**: 20 minutes
- **Dependencies**: None

#### Task 2.2: Implement PNR Helper
- [ ] Create `PNRHelper` class implementing `IPNRHelper`
  - Character set: A-Z, 0-9 (exclude 0, O, 1, I, L)
  - Random generation algorithm
  - Uniqueness check via repository
  - Retry mechanism for collisions
- **Estimate**: 1 hour
- **Dependencies**: Task 2.1, Task 1.2

#### Task 2.3: Extend Ticket Repository
- [ ] Add `GetByPNRAsync(string pnr)` method
- [ ] Add `GetByPNRAndEmailAsync(string pnr, string email)` method
- [ ] Add `PNRExistsAsync(string pnr)` method
- [ ] Implement case-insensitive comparison
- **Estimate**: 45 minutes
- **Dependencies**: Task 1.2

#### Task 2.4: Register PNR Helper in DI
- [ ] Add `IPNRHelper` and `PNRHelper` to service collection
- [ ] Update `Program.cs`
- **Estimate**: 10 minutes
- **Dependencies**: Task 2.2

#### Task 2.5: Integrate PNR Generation in Booking Flow
- [ ] Update `UserController.ConfirmBooking` method
  - Inject `IPNRHelper`
  - Generate PNR before creating ticket
  - Assign PNR to ticket entity
- [ ] Handle PNR generation errors gracefully
- **Estimate**: 30 minutes
- **Dependencies**: Task 2.2, Task 2.4

### Phase 3: PNR Lookup Feature

#### Task 3.1: Create PNR Controller
- [ ] Create `PNRController` class
  - `GET Lookup()`: Display lookup form
  - `POST Lookup()`: Process lookup request
- [ ] Implement email + PNR validation
- [ ] Handle invalid PNR/email combinations
- **Estimate**: 1 hour
- **Dependencies**: Task 2.3

#### Task 3.2: Create PNR Lookup Views
- [ ] Create `Views/PNR/Lookup.cshtml`
  - Form with PNR and Email fields
  - Display ticket details after lookup
  - Error messages for invalid lookups
- [ ] Add validation messages
- **Estimate**: 1 hour
- **Dependencies**: Task 3.1

#### Task 3.3: Add PNR Lookup Route
- [ ] Add route configuration for PNR controller
- [ ] Test route accessibility
- **Estimate**: 10 minutes
- **Dependencies**: Task 3.1

### Phase 4: UI Integration

#### Task 4.1: Update Ticket Detail View
- [ ] Update `Views/User/Ticket.cshtml`
  - Display PNR code prominently
  - Show PNR label and value
- **Estimate**: 20 minutes
- **Dependencies**: Task 1.1

#### Task 4.2: Update My Booking View
- [ ] Update `Views/User/MyBooking.cshtml`
  - Add PNR column to booking list
  - Display PNR for each ticket
- **Estimate**: 30 minutes
- **Dependencies**: Task 1.1

#### Task 4.3: Update Admin Views
- [ ] Update admin ticket management views
  - Display PNR in ticket lists
  - Add PNR search functionality
- **Estimate**: 45 minutes
- **Dependencies**: Task 2.3

#### Task 4.4: Update Partner Views
- [ ] Update partner trip/ticket views
  - Display PNR for bookings
  - Add PNR filter/search
- **Estimate**: 30 minutes
- **Dependencies**: Task 2.3

#### Task 4.5: Update Email Templates
- [ ] Include PNR in booking confirmation emails
- [ ] Add PNR to email subject or body
- **Estimate**: 30 minutes
- **Dependencies**: Task 2.5

### Phase 5: Testing & Polish

#### Task 5.1: Unit Tests - PNR Helper
- [ ] Test PNR generation format
- [ ] Test uniqueness validation
- [ ] Test retry mechanism
- [ ] Test invalid format validation
- **Estimate**: 1 hour
- **Dependencies**: Task 2.2

#### Task 5.2: Unit Tests - Repository
- [ ] Test `GetByPNRAsync`
- [ ] Test `GetByPNRAndEmailAsync`
- [ ] Test `PNRExistsAsync`
- [ ] Test case-insensitive lookup
- **Estimate**: 45 minutes
- **Dependencies**: Task 2.3

#### Task 5.3: Integration Tests - Booking Flow
- [ ] Test PNR generation during booking
- [ ] Test PNR assignment to ticket
- [ ] Test PNR uniqueness in booking flow
- **Estimate**: 1 hour
- **Dependencies**: Task 2.5

#### Task 5.4: Integration Tests - PNR Lookup
- [ ] Test successful PNR lookup
- [ ] Test invalid PNR lookup
- [ ] Test invalid email lookup
- [ ] Test case-insensitive lookup
- **Estimate**: 45 minutes
- **Dependencies**: Task 3.1

#### Task 5.5: Manual Testing
- [ ] Test complete booking flow with PNR
- [ ] Test PNR lookup functionality
- [ ] Test PNR display in all views
- [ ] Test email with PNR
- [ ] Test admin/partner PNR search
- **Estimate**: 1 hour
- **Dependencies**: All previous tasks

## Dependencies
**What needs to happen in what order?**

### Critical Path
1. Database & Model Setup (Phase 1) → Must complete first
2. PNR Generation Core (Phase 2) → Depends on Phase 1
3. PNR Lookup (Phase 3) → Depends on Phase 2
4. UI Integration (Phase 4) → Can run parallel with Phase 3
5. Testing (Phase 5) → Depends on all previous phases

### Task Dependencies
- Task 1.2 depends on Task 1.1
- Task 1.3 depends on Task 1.2
- Task 2.2 depends on Task 2.1 and Task 1.2
- Task 2.3 depends on Task 1.2
- Task 2.5 depends on Task 2.2 and Task 2.4
- Task 3.1 depends on Task 2.3
- Task 3.2 depends on Task 3.1
- All Phase 4 tasks depend on Task 1.1
- All Phase 5 tasks depend on their respective feature implementations

### External Dependencies
- None (all components are internal)

## Timeline & Estimates
**When will things be done?**

### Phase Estimates
- **Phase 1**: ~55 minutes (Foundation)
- **Phase 2**: ~3 hours (Core Features)
- **Phase 3**: ~2 hours 10 minutes (Lookup)
- **Phase 4**: ~2 hours 5 minutes (UI Integration)
- **Phase 5**: ~4 hours 30 minutes (Testing)

### Total Estimated Time
- **Development**: ~8 hours
- **Testing**: ~4.5 hours
- **Total**: ~12.5 hours

### Suggested Implementation Order
1. Complete Phase 1 (Foundation) - Required first
2. Complete Phase 2 (Core Features) - Required for functionality
3. Complete Phase 3 (Lookup) - Core user feature
4. Complete Phase 4 (UI) - Can be done in parallel with Phase 3
5. Complete Phase 5 (Testing) - Final validation

## Risks & Mitigation
**What could go wrong?**

### Technical Risks

#### Risk 1: PNR Collision (Low Probability)
- **Description**: Generated PNR already exists
- **Impact**: Booking fails or duplicate PNR
- **Mitigation**: 
  - Retry mechanism in PNR helper (up to 5 retries)
  - Database unique constraint prevents duplicates
  - Log collisions for monitoring

#### Risk 2: Performance Impact (Low)
- **Description**: Uniqueness check slows down booking
- **Impact**: Slower booking process
- **Mitigation**:
  - Database index ensures fast lookups
  - Retry mechanism limits attempts
  - Monitor performance metrics

#### Risk 3: Migration Issues (Medium)
- **Description**: Database migration fails on existing data
- **Impact**: System downtime or data loss
- **Mitigation**:
  - Test migration on backup database first
  - PNR column is nullable (backward compatible)
  - Rollback plan prepared

### Resource Risks
- **None identified** - All work can be done by single developer

### Dependency Risks
- **None identified** - No external dependencies

## Resources Needed
**What do we need to succeed?**

### Team Members and Roles
- **Developer**: Implement all code changes
- **Tester**: Manual testing and validation (can be same person)

### Tools and Services
- Visual Studio / VS Code (existing)
- SQLite database (existing)
- Git for version control (existing)

### Infrastructure
- Development environment (existing)
- Test database (can use existing)

### Documentation/Knowledge
- Understanding of Entity Framework migrations
- ASP.NET Core MVC patterns
- SQLite database operations
- C# string manipulation


