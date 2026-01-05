---
phase: requirements
title: PNR Booking Code Feature
description: Add Passenger Name Record (PNR) booking code generation and lookup functionality
---

# Requirements: PNR Booking Code Feature

## Problem Statement
**What problem are we solving?**

- Currently, the ticket booking system does not have a PNR (Passenger Name Record) code for tickets
- Users cannot easily reference or look up their bookings using a unique booking code
- Without PNR codes, it's difficult for users to:
  - Check booking status online
  - Reference their booking when contacting support
  - Share booking information securely
  - Track multiple bookings easily

**Who is affected by this problem?**
- End users booking tickets
- Customer support staff who need to look up bookings
- System administrators managing bookings

**What is the current situation/workaround?**
- Users must use ticket ID (numeric) which is not user-friendly
- No standardized booking reference code exists
- QR codes exist but are not suitable for manual reference

## Goals & Objectives
**What do we want to achieve?**

### Primary Goals
- Generate unique PNR codes (6 alphanumeric characters) for each ticket booking
- Enable users to look up their bookings using PNR code
- Display PNR code prominently in booking confirmations and ticket details
- Ensure PNR codes are unique and collision-resistant

### Secondary Goals
- Support PNR lookup by email + PNR combination for additional security
- Display PNR in booking emails/notifications
- Allow admin/partner users to search bookings by PNR

### Non-goals (what's explicitly out of scope)
- Integration with external GDS (Global Distribution System) like Amadeus or Sabre
- PNR modification/cancellation through PNR code (handled through existing ticket management)
- Multi-passenger PNR grouping (single ticket = single PNR)

## User Stories & Use Cases
**How will users interact with the solution?**

### User Stories
1. **As a customer**, I want to receive a PNR code when I book a ticket, so that I can easily reference my booking
2. **As a customer**, I want to look up my booking using my PNR code and email, so that I can check my booking status without logging in
3. **As a customer**, I want to see my PNR code on my ticket confirmation page, so that I can save it for future reference
4. **As a customer**, I want to receive my PNR code via email after booking, so that I have a record of my booking reference
5. **As an admin**, I want to search for bookings by PNR code, so that I can quickly assist customers
6. **As a partner**, I want to see PNR codes for tickets booked on my trips, so that I can track bookings efficiently

### Key Workflows
1. **Booking Flow**: When a ticket is created, automatically generate and assign a unique PNR code
2. **PNR Lookup Flow**: User enters PNR code + email → System validates → Returns booking details
3. **Display Flow**: PNR code shown on ticket details, booking confirmation, and email notifications

### Edge Cases to Consider
- PNR collision (extremely rare but must handle gracefully)
- PNR lookup with invalid/expired email
- PNR lookup for cancelled tickets
- Multiple bookings with same email (should show all matching bookings)
- Case-insensitive PNR lookup

## Success Criteria
**How will we know when we're done?**

### Measurable Outcomes
- Every new ticket booking receives a unique PNR code
- PNR codes are 6 alphanumeric characters (standard format)
- PNR lookup functionality returns correct booking information
- PNR codes are displayed in all relevant UI locations

### Acceptance Criteria
- ✅ PNR code is automatically generated when ticket is created
- ✅ PNR code format: 6 alphanumeric characters (e.g., "A1B2C3", "XYZ789")
- ✅ PNR codes are unique (no duplicates)
- ✅ PNR lookup page accessible without login
- ✅ PNR lookup validates email matches booking
- ✅ PNR displayed on ticket detail page
- ✅ PNR displayed in booking confirmation
- ✅ PNR included in booking email notifications
- ✅ Admin can search bookings by PNR
- ✅ Partner can view PNR for their trip bookings

### Performance Benchmarks
- PNR generation: < 10ms
- PNR lookup: < 200ms (including database query)
- PNR uniqueness check: < 50ms

## Constraints & Assumptions
**What limitations do we need to work within?**

### Technical Constraints
- Must work with existing SQLite database
- Must be compatible with current Entity Framework setup
- Must not break existing booking flow
- PNR format: 6 alphanumeric characters (industry standard)

### Business Constraints
- PNR codes should be human-readable (avoid confusing characters like 0/O, 1/I)
- PNR codes should be case-insensitive for user convenience
- Must maintain backward compatibility (existing tickets without PNR should still work)

### Time/Budget Constraints
- Implementation should be completed within reasonable timeframe
- No external API dependencies required

### Assumptions We're Making
- PNR codes will be generated server-side (not client-side)
- PNR uniqueness will be enforced at database level
- Email validation for PNR lookup provides sufficient security
- Users will primarily use PNR for reference, not for frequent lookups
- SQLite can handle uniqueness constraints efficiently

## Questions & Open Items
**What do we still need to clarify?**

### Resolved
- ✅ PNR format: 6 alphanumeric characters (standard airline format)
- ✅ PNR generation: Server-side, at ticket creation time
- ✅ PNR lookup: Email + PNR combination for security

### Open Questions
- Should PNR codes be included in QR codes?
- Should we add PNR to existing tickets (migration strategy)?
- Should PNR be editable by admins (probably not, for integrity)?
- Should we log PNR lookup attempts for security?


