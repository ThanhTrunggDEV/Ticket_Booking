# Design Review: Round-Trip Booking & Price Management

**Review Date:** 2024-12-20  
**Reviewer:** AI Assistant  
**Feature:** Round-Trip Booking  
**Status:** Design Complete - Ready for Implementation

---

## Executive Summary

The design document for round-trip booking is **well-structured and comprehensive**. The architecture is sound, data models are appropriate, and the design decisions are well-reasoned. The document aligns well with requirements and follows best practices.

**Overall Assessment:** ✅ **APPROVED** with minor recommendations

---

## 1. Architecture Overview

### ✅ Mermaid Diagram
**Status:** Present and accurate

The mermaid diagram correctly shows:
- User flow for both one-way and round-trip booking
- Component relationships (Booking Form → Services → Repository → Database)
- Partner price management flow
- Clear separation of concerns

**Recommendation:** Consider adding error handling paths in the diagram (optional enhancement)

### ✅ Key Components
**Status:** Well-defined

All key components are identified:
- Booking Form (enhanced)
- Price Calculator Service (new)
- Pricing Service (new)
- Booking Service (enhanced/new)
- Price Management UI (enhanced)

**Note:** The design mentions "Booking Service" but the current implementation uses UserController directly. Consider clarifying if this will be a new service or enhanced controller method.

### ✅ Technology Stack
**Status:** Appropriate

- ASP.NET Core MVC (existing) ✅
- Entity Framework Core (existing) ✅
- SQLite (existing) ✅
- No new dependencies ✅

**Rationale:** Correct - leverages existing stack, no unnecessary complexity

---

## 2. Data Models & Relationships

### ✅ Trip Model Changes
**Status:** Appropriate

```csharp
public decimal RoundTripDiscountPercent { get; set; }  // 0-50
public DateTime? PriceLastUpdated { get; set; }
```

**Assessment:**
- ✅ Discount stored as decimal (appropriate for percentages)
- ✅ Nullable DateTime for PriceLastUpdated (good for existing trips)
- ✅ Range constraint (0-50) documented

**Recommendation:** Consider adding validation attribute or database constraint for 0-50 range

### ✅ Ticket Model Changes
**Status:** Well-designed

```csharp
public TicketType Type { get; set; }
public int? OutboundTicketId { get; set; }
public int? ReturnTicketId { get; set; }
public int? BookingGroupId { get; set; }
```

**Assessment:**
- ✅ Bidirectional linking (OutboundTicketId + ReturnTicketId) allows easy navigation
- ✅ BookingGroupId enables grouping for display
- ✅ All nullable (appropriate for one-way tickets)
- ✅ Navigation properties included

**Potential Issue:** ⚠️ Self-referencing foreign keys (Ticket → Ticket) need careful EF Core configuration

**Recommendation:** Document EF Core configuration for self-referencing relationships

### ✅ TicketType Enum
**Status:** Appropriate

```csharp
public enum TicketType
{
    OneWay,
    RoundTrip
}
```

**Assessment:** ✅ Simple, clear, extensible

### ⚠️ PricingRule Model
**Status:** Marked as "Optional - for future enhancement"

**Assessment:** ✅ Good to include for future, but not required for MVP

**Recommendation:** Keep as future enhancement, focus on Trip-based discount for MVP

### ✅ BookingViewModel
**Status:** Comprehensive

Includes all necessary properties for price breakdown and display.

---

## 3. Database Schema Changes

### ✅ Schema Design
**Status:** Well-documented

**Columns to add:**
- `Tickets.Type` (INTEGER, default 0, indexed) ✅
- `Tickets.OutboundTicketId` (INTEGER, nullable, FK) ✅
- `Tickets.ReturnTicketId` (INTEGER, nullable, FK) ✅
- `Tickets.BookingGroupId` (INTEGER, nullable) ✅
- `Trips.RoundTripDiscountPercent` (DECIMAL(5,2), default 0) ✅
- `Trips.PriceLastUpdated` (DATETIME, nullable) ✅

**Indexes:**
- `IX_Tickets_BookingGroupId` ✅
- `IX_Tickets_Type` ✅
- `IX_Tickets_OutboundTicketId` ✅
- `IX_Tickets_ReturnTicketId` ✅

**Recommendations:**
1. ⚠️ Add composite index: `IX_Tickets_BookingGroupId_Type` for common query pattern
2. ⚠️ Consider constraint: `CHECK (RoundTripDiscountPercent >= 0 AND RoundTripDiscountPercent <= 50)`
3. ⚠️ Document migration strategy for existing tickets (default Type = OneWay)

---

## 4. API Design & Interfaces

### ✅ IPriceCalculatorService
**Status:** Well-defined

**Methods:**
- `CalculateOneWayPrice(Trip, SeatClass)` ✅
- `CalculateRoundTripPrice(Trip, Trip, SeatClass)` ✅

**Assessment:**
- ✅ Clear method signatures
- ✅ RoundTripPriceBreakdown class is comprehensive
- ✅ Handles different seat classes

**Recommendation:** Consider overload for different seat classes per leg:
```csharp
RoundTripPriceBreakdown CalculateRoundTripPrice(
    Trip outboundTrip, 
    Trip returnTrip, 
    SeatClass outboundSeatClass,
    SeatClass returnSeatClass);
```

### ✅ IPricingService
**Status:** Appropriate

**Methods:**
- `GetRoundTripDiscount(int, string, string)` ✅
- `UpdateRoundTripDiscountAsync(int, decimal)` ✅
- `UpdateRouteDiscountAsync(int, string, string, decimal)` ✅

**Assessment:**
- ✅ Covers all required functionality
- ✅ Async methods where appropriate
- ✅ Validation implied (0-50%)

**Recommendation:** Add validation method or return validation result:
```csharp
ValidationResult ValidateDiscount(decimal discountPercent);
```

### ✅ Controller Endpoints
**Status:** Well-planned

**User Controller:**
- `GET /User/BookTrip?ticketType={OneWay|RoundTrip}` ✅
- `POST /User/BookTrip` (enhanced) ✅

**Partner Controller:**
- `GET /Partner/TripsManagement` (enhanced) ✅
- `POST /Partner/UpdateTripPricing` (new) ✅
- `GET /Partner/RoutePricing` (new) ✅
- `POST /Partner/UpdateRouteDiscount` (new) ✅

**Assessment:** ✅ All endpoints align with requirements

---

## 5. Design Decisions

### ✅ Decision 1: Ticket Linking Strategy
**Status:** Sound decision

- **Choice:** Bidirectional linking with OutboundTicketId/ReturnTicketId
- **Rationale:** Simple, allows easy navigation
- **Assessment:** ✅ Good choice for MVP

**Alternative Considered:** Separate BookingGroup table (rejected - adds complexity)
- **Assessment:** ✅ Correct rejection - adds unnecessary complexity

### ✅ Decision 2: Discount Storage
**Status:** Appropriate for MVP

- **Choice:** Store in Trip table
- **Rationale:** Simple, one discount per trip
- **Assessment:** ✅ Good for MVP

**Future Enhancement:** PricingRules table
- **Assessment:** ✅ Good to plan for future, not needed now

### ✅ Decision 3: Price Calculation Timing
**Status:** Correct

- **Choice:** On-demand calculation
- **Rationale:** Always up-to-date, no cache invalidation
- **Assessment:** ✅ Correct for dynamic pricing

### ✅ Decision 4: Booking Structure
**Status:** Well-reasoned

- **Choice:** One booking group with two separate tickets
- **Rationale:** Maintains ticket independence
- **Assessment:** ✅ Allows separate cancellation, maintains flexibility

### ✅ Decision 5: Discount Application
**Status:** Industry standard

- **Choice:** Apply to total of both tickets
- **Rationale:** Standard practice
- **Assessment:** ✅ Clear and intuitive

### ✅ Decision 6: Different Seat Classes
**Status:** Good flexibility

- **Choice:** Allow different classes per leg
- **Rationale:** More flexibility
- **Assessment:** ✅ Good user experience

**Note:** Implementation needs to handle this in price calculation

---

## 6. Component Breakdown

### ✅ Backend Components
**Status:** All identified

1. PriceCalculatorService (NEW) ✅
2. PricingService (NEW) ✅
3. Booking Service (Enhanced/NEW) ⚠️ **Needs clarification**
4. UserController (Enhanced) ✅
5. PartnerController (Enhanced) ✅

**Issue:** ⚠️ "Booking Service" is mentioned but current code uses UserController directly. Need to clarify:
- Will this be a new service?
- Or will booking logic stay in UserController?

**Recommendation:** Document whether booking logic will be extracted to a service or remain in controller

### ✅ Frontend Components
**Status:** Well-planned

1. Booking Form (Enhanced) ✅
2. Price Display Component ✅
3. Partner Pricing Management (Enhanced) ✅

---

## 7. Non-Functional Requirements

### ✅ Performance Targets
**Status:** Realistic

- Round-trip price calculation: < 100ms ✅
- Round-trip booking creation: < 3 seconds ✅
- Partner price update: < 1 second ✅
- Flight search with return options: < 2 seconds ✅

**Assessment:** ✅ Achievable with proper indexing and efficient queries

### ✅ Scalability Considerations
**Status:** Appropriate

- Stateless price calculations ✅
- Database indexes ✅
- No heavy caching ✅

**Assessment:** ✅ Good for current scale

### ✅ Security Requirements
**Status:** Addressed

- Authorization: Only partners can update pricing ✅
- Validation: Discount 0-50% ✅
- Input validation ✅

**Recommendation:** ⚠️ Add rate limiting for price updates (prevent abuse)

### ✅ Reliability/Availability
**Status:** Well-considered

- Atomic booking creation ✅
- Consistent price calculations ✅
- Handle unavailable return flights ✅

**Recommendation:** ⚠️ Document retry strategy for failed bookings

### ✅ Data Integrity
**Status:** Addressed

- Linked tickets validation ✅
- BookingGroupId consistency ✅
- Discount range validation ✅
- Positive prices ✅

---

## 8. Alignment with Requirements

### ✅ Requirements Coverage
**Status:** Excellent alignment

All requirements from `round-trip-booking.md` are addressed:
- ✅ Ticket type selection
- ✅ Round-trip pricing with discount
- ✅ Partner price management
- ✅ Linked bookings
- ✅ Price display

### ✅ User Stories Coverage
**Status:** All covered

All 9 user stories from requirements are supported by the design.

---

## 9. Issues & Recommendations

### ⚠️ Minor Issues

1. **Self-Referencing Foreign Keys**
   - **Issue:** Ticket → Ticket relationships need careful EF Core configuration
   - **Recommendation:** Document EF Core fluent API configuration in implementation guide
   - **Impact:** Medium (implementation complexity)

2. **Booking Service Clarification**
   - **Issue:** Unclear if new service or enhanced controller
   - **Recommendation:** Clarify in design doc - suggest keeping in UserController for MVP
   - **Impact:** Low (implementation decision)

3. **Different Seat Classes Per Leg**
   - **Issue:** Interface doesn't support different classes per leg
   - **Recommendation:** Add overload or update method signature
   - **Impact:** Low (can be added later)

4. **Discount Validation**
   - **Issue:** Validation mentioned but not detailed
   - **Recommendation:** Add ValidationResult or exception details
   - **Impact:** Low (implementation detail)

### ✅ Strengths

1. **Clear Architecture:** Well-structured, easy to understand
2. **Good Data Model:** Appropriate fields, good relationships
3. **Sound Decisions:** All design decisions are well-reasoned
4. **Comprehensive:** Covers all aspects of the feature
5. **Future-Proof:** Includes PricingRules for future enhancement

---

## 10. Missing Sections (Optional Enhancements)

### Consider Adding:

1. **Error Handling Details**
   - Specific error scenarios
   - Error codes/messages
   - Recovery strategies

2. **Migration Strategy**
   - How to handle existing tickets
   - Data migration steps
   - Rollback plan

3. **API Versioning**
   - If API changes affect existing endpoints
   - Backward compatibility strategy

4. **Monitoring & Observability**
   - Metrics to track (booking success rate, discount usage)
   - Logging requirements

---

## 11. Final Recommendations

### Must Address Before Implementation:

1. ✅ **Document EF Core configuration** for self-referencing Ticket relationships
2. ✅ **Clarify Booking Service** - new service or enhanced controller?
3. ✅ **Add discount validation details** - how validation works, error messages
4. ✅ **Document migration strategy** for existing tickets

### Nice to Have:

1. Consider composite index for common queries
2. Add rate limiting for price updates
3. Document retry strategy for failed bookings
4. Add monitoring/metrics requirements

---

## 12. Summary

### ✅ Design Quality: **EXCELLENT**

The design document is:
- ✅ **Comprehensive:** Covers all aspects
- ✅ **Well-structured:** Easy to follow
- ✅ **Aligned with requirements:** All requirements addressed
- ✅ **Sound decisions:** All choices are well-reasoned
- ✅ **Implementation-ready:** Clear enough to start coding

### Approval Status: ✅ **APPROVED**

The design is ready for implementation with minor clarifications recommended above.

### Next Steps:

1. Address minor clarifications (EF Core config, Booking Service)
2. Proceed to implementation phase
3. Reference this design doc during implementation
4. Update design doc if implementation reveals changes needed

---

**End of Review**

