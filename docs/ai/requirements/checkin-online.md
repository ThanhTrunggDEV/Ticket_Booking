---
phase: requirements
title: Online Check-In Feature
description: Allow passengers to check-in online, select seats, and receive boarding passes before arriving at the airport
---

# Requirements: Online Check-In Feature

## Problem Statement
**What problem are we solving?**

- Currently, passengers must check-in at the airport, leading to long queues and waiting times
- Passengers cannot select or change their seats after booking
- No way to receive boarding passes before arriving at the airport
- Passengers have limited control over their travel experience after booking
- Airport check-in counters become congested, especially during peak hours

**Who is affected by this problem?**
- Passengers who have booked tickets and need to check-in
- Airport staff managing check-in counters
- Airlines/partners managing passenger flow

**What is the current situation/workaround?**
- Passengers must arrive at the airport early to check-in at counters
- Seat selection is only available during initial booking
- Boarding passes are only issued at the airport
- No way to modify seat preferences after booking

## Goals & Objectives
**What do we want to achieve?**

### Primary Goals
- Enable passengers to check-in online 24-48 hours before flight departure
- Allow passengers to select or change their seats during online check-in
- Generate and provide digital boarding passes after successful check-in
- Reduce airport check-in counter congestion
- Improve passenger experience and convenience

### Secondary Goals
- Display seat map with available seats during check-in
- Show real-time seat availability
- Allow passengers to view their check-in status
- Support mobile-friendly check-in process
- Integrate with existing PNR lookup system

### Non-goals (what's explicitly out of scope)
- Baggage check-in/payment (handled separately at airport)
- Flight changes or cancellations (handled through existing booking system)
- Check-in for multiple passengers at once (one ticket = one check-in)
- Integration with external airport systems (GDS, departure control systems)
- Automatic check-in (passengers must initiate manually)

## User Stories & Use Cases
**How will users interact with the solution?**

### User Stories
1. **As a passenger**, I want to check-in online using my PNR code and email, so that I can avoid long queues at the airport
2. **As a passenger**, I want to select my preferred seat during online check-in, so that I can choose a window, aisle, or specific seat location
3. **As a passenger**, I want to view available seats on a seat map, so that I can make an informed seat selection
4. **As a passenger**, I want to receive my boarding pass after check-in, so that I can go directly to the gate at the airport
5. **As a passenger**, I want to check-in from my mobile device, so that I can check-in anywhere, anytime
6. **As a passenger**, I want to see my check-in status, so that I know if I've already checked in
7. **As a passenger**, I want to change my seat during check-in, so that I can select a better seat if available

### Key Workflows
1. **Online Check-In Flow**: 
   - Passenger accesses check-in page (via PNR lookup or logged-in account)
   - System validates ticket eligibility (within check-in window, payment confirmed)
   - Passenger views seat map and selects seat
   - System confirms seat availability and assigns seat
   - System generates boarding pass
   - Passenger receives boarding pass (digital download/email)

2. **Seat Selection Flow**:
   - Passenger views interactive seat map
   - Available seats highlighted, occupied seats shown
   - Passenger clicks on desired seat
   - System validates seat availability
   - Seat assigned and confirmed

3. **Boarding Pass Generation**:
   - After successful check-in, generate boarding pass
   - Include all required information:
     - Passenger name (full name as on ticket)
     - Flight details (departure/arrival cities, flight number/plane name)
     - Departure date and time
     - Arrival time (calculated)
     - Seat number and class
     - Gate number (default or assigned)
     - Boarding time (45 minutes before departure)
     - PNR code
     - Booking ID
     - QR code (if available)
     - Airline/company name and logo
   - Provide download option (PDF format)
   - Send email with boarding pass attachment

### Edge Cases to Consider
- Check-in window: 24-48 hours before departure (must enforce time limits)
- Seat already occupied by another passenger (concurrent check-in)
- Ticket payment not confirmed (cannot check-in)
- Flight cancelled or changed (check-in disabled)
- Passenger tries to check-in multiple times (prevent duplicate check-in)
- Check-in after deadline (show error, redirect to airport check-in)
- Seat selection for already assigned seat (allow change if better seat available)
- Mobile vs desktop experience differences

## Success Criteria
**How will we know when we're done?**

### Measurable Outcomes
- Passengers can successfully check-in online within the allowed time window
- Seat selection works correctly with real-time availability
- Boarding passes are generated and delivered successfully
- Check-in process completes in under 2 minutes
- System prevents duplicate check-ins

### Acceptance Criteria
- ✅ Check-in available 24-48 hours before departure time
- ✅ Check-in disabled if payment not confirmed
- ✅ Seat map displays available/occupied seats correctly
- ✅ Seat selection updates in real-time
- ✅ Boarding pass generated with all required information
- ✅ Boarding pass downloadable as PDF
- ✅ Boarding pass sent via email
- ✅ Check-in status visible to passenger
- ✅ Cannot check-in twice for same ticket
- ✅ Mobile-responsive check-in interface
- ✅ Integration with existing PNR lookup system

### Performance Benchmarks
- Check-in process: < 30 seconds (excluding seat selection time)
- Seat map loading: < 2 seconds
- Seat selection confirmation: < 1 second
- Boarding pass generation: < 3 seconds
- Email delivery: < 10 seconds

## Constraints & Assumptions
**What limitations do we need to work within?**

### Technical Constraints
- Must work with existing SQLite database
- Must integrate with existing Ticket and Trip models
- Must use existing authentication/session system
- Must be compatible with current Entity Framework setup
- Must work with existing PNR lookup functionality

### Business Constraints
- Check-in window: 24-48 hours before departure (configurable)
- Check-in deadline: Closes 2 hours before scheduled departure time
- Seat selection only available during check-in window
- Cannot check-in if payment not confirmed
- Cannot check-in for cancelled or changed flights
- Seat changes may have restrictions (e.g., same class only)
- Seat numbering format: {Row}{Letter} (e.g., 12A, 12B, 12C) - standard airline format

### Time/Budget Constraints
- Implementation should be completed within reasonable timeframe
- No external API dependencies required
- Use existing infrastructure and services

### Assumptions We're Making
- Passengers have access to internet and email
- Seat map can be generated from Trip seat configuration
- Seat numbering follows standard format: {Row}{Letter} where Row is 1-50 and Letter is A-F (depending on aircraft type)
- Boarding pass format follows standard airline format (A4 size, printable)
- Email service is already configured and working
- QR code generation is already available (for boarding pass)
- Boarding pass storage: Local file system initially (can upgrade to cloud storage later)
- Gate assignment: Default gate (e.g., A04) or can be assigned by airline (future enhancement)

## Questions & Open Items
**What do we still need to clarify?**

### Resolved
- ✅ Check-in window: 24-48 hours before departure
- ✅ Seat selection: Available during check-in
- ✅ Boarding pass: Digital format (PDF) with email delivery
- ✅ Integration: Use existing PNR lookup for access

### Open Questions
- Should we allow seat changes after initial check-in?
- Should we show seat pricing during selection (if different classes available)?
- Should we support group check-in (multiple tickets together)?
- Should we add check-in reminder notifications (email/SMS)?
- Should we track check-in statistics for analytics?
- Should we allow check-in cancellation (undo check-in)?

