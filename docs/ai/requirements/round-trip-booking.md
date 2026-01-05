---
phase: requirements
title: Round-Trip Booking & Price Management
description: Allow users to select ticket types (one-way or round-trip) with different pricing, and enable partners to manage ticket prices
---

# Requirements: Round-Trip Booking & Price Management

## Problem Statement
**What problem are we solving?**

- Currently, users can only book one-way tickets, limiting flexibility for return travel
- No option to book round-trip tickets with potential discounts
- Partners cannot easily manage pricing for different ticket types (one-way vs round-trip)
- No centralized pricing management system for partners to set discounts and promotional pricing
- Users must book two separate one-way tickets for round trips, missing potential savings

**Who is affected by this problem?**
- Passengers who need to book return flights (must book twice, miss discounts)
- Partners who want to offer round-trip discounts to attract customers
- Business travelers who frequently book round trips
- System administrators managing pricing strategies

**What is the current situation/workaround?**
- Users book one-way tickets only
- For return trips, users must make two separate bookings
- No discount mechanism for round-trip bookings
- Partners manually set prices per trip without round-trip pricing options
- No pricing history or discount management

## Goals & Objectives
**What do we want to achieve?**

### Primary Goals
- Enable users to select between one-way and round-trip ticket types during booking
- Implement round-trip pricing with configurable discounts (typically 10-15% off total)
- Allow partners to manage pricing for both one-way and round-trip tickets
- Support linked bookings where outbound and return tickets are associated
- Provide clear pricing display showing savings for round-trip options

### Secondary Goals
- Display price comparison between one-way and round-trip options
- Allow partners to set custom round-trip discount percentages per route
- Support pricing history/audit trail for partner price changes
- Enable flexible return date selection (within reasonable range)
- Show estimated savings when selecting round-trip option

### Non-goals (what's explicitly out of scope)
- Multi-city bookings (only one-way and round-trip)
- Different airlines for outbound and return (must be same company)
- Dynamic pricing based on demand (static pricing with discounts)
- Price negotiation or bidding features
- Corporate/group pricing discounts
- Frequent flyer program integration
- Price guarantee or price matching

## User Stories & Use Cases
**How will users interact with the solution?**

### User Stories

1. **As a passenger**, I want to select "round-trip" when booking, so that I can book both outbound and return flights in one transaction
2. **As a passenger**, I want to see the total price for round-trip tickets, so that I can compare with booking two one-way tickets separately
3. **As a passenger**, I want to see the discount amount for round-trip bookings, so that I know how much I'm saving
4. **As a passenger**, I want to select different return dates, so that I can plan my trip flexibly
5. **As a passenger**, I want to see both outbound and return flight details before confirming, so that I can verify my booking
6. **As a partner**, I want to set round-trip discount percentages for my routes, so that I can attract more customers
7. **As a partner**, I want to manage pricing for one-way and round-trip tickets separately, so that I can optimize revenue
8. **As a partner**, I want to see pricing history, so that I can track price changes over time
9. **As a user**, I want to view my round-trip booking as a single booking with two tickets, so that I can manage it easily

### Key Workflows

1. **Round-Trip Booking Flow:**
   - User searches for flights and selects "Round-Trip" option
   - User selects outbound flight (date, time, seat class)
   - System shows available return flights (same route, reverse direction)
   - User selects return flight (date, time, seat class)
   - System calculates total price with round-trip discount
   - System displays price breakdown (outbound + return - discount = total)
   - User confirms booking
   - System creates one booking with two linked tickets (outbound and return)
   - User receives confirmation with both tickets

2. **One-Way Booking Flow (Enhanced):**
   - User searches for flights and selects "One-Way" option
   - User selects flight (date, time, seat class)
   - System shows one-way pricing
   - User confirms booking
   - System creates booking with single ticket

3. **Partner Price Management Flow:**
   - Partner logs into partner dashboard
   - Partner navigates to trip/route management
   - Partner views current pricing for a route
   - Partner sets one-way prices (Economy, Business, FirstClass)
   - Partner sets round-trip discount percentage (e.g., 10%, 15%)
   - System calculates round-trip prices automatically
   - Partner saves pricing changes
   - System updates prices for future bookings

4. **Price Display Flow:**
   - User views flight search results
   - System shows one-way price for each flight
   - When round-trip is selected, system shows:
     - Outbound price
     - Return price
     - Discount amount
     - Total round-trip price
     - Savings compared to two one-way tickets

### Edge Cases to Consider
- User selects round-trip but no return flights available → Show error, suggest alternative dates
- Return flight fully booked → Allow booking outbound only or suggest alternative return flights
- Price changes between selecting outbound and return → Lock outbound price, show updated return price
- Partner sets discount higher than 50% → Validate and warn (business rule)
- Round-trip discount results in negative price → Prevent and show error
- User cancels one leg of round-trip → Handle cancellation policy (may affect discount)
- Return date before outbound date → Validate and prevent
- Return date too far in future (e.g., >1 year) → Set reasonable limit
- Different seat classes for outbound and return → Allow but calculate separately
- Partner deletes route with active round-trip bookings → Handle gracefully

## Success Criteria
**How will we know when we're done?**

### Measurable Outcomes
- Users can successfully book round-trip tickets with discount applied
- Round-trip bookings show correct pricing (outbound + return - discount)
- Partners can set and update round-trip discount percentages
- Booking system creates linked tickets for round-trip bookings
- Price calculations are accurate and consistent

### Acceptance Criteria
- ✅ User can select "One-Way" or "Round-Trip" option in booking form
- ✅ Round-trip option shows return date picker
- ✅ System displays available return flights for selected route
- ✅ Round-trip discount is calculated and displayed correctly
- ✅ Total price for round-trip is less than or equal to two one-way tickets
- ✅ One booking contains two linked tickets for round-trip
- ✅ Partner can set round-trip discount percentage (0-50%)
- ✅ Partner can view and edit pricing for both ticket types
- ✅ Price changes are reflected immediately in booking flow
- ✅ Booking confirmation shows both outbound and return ticket details
- ✅ My Bookings page shows round-trip bookings as single entries
- ✅ Price breakdown is clear and understandable

### Performance Benchmarks
- Round-trip flight search: < 2 seconds
- Price calculation: < 100ms
- Booking creation (with 2 tickets): < 3 seconds
- Partner price update: < 1 second

## Constraints & Assumptions
**What limitations do we need to work within?**

### Technical Constraints
- Must work with existing SQLite database
- Must integrate with existing Trip and Ticket models
- Must maintain backward compatibility with existing one-way bookings
- Must use existing Entity Framework setup
- Must work with existing payment system

### Business Constraints
- Round-trip discount range: 0% to 50% (configurable per route)
- Return date must be after outbound date
- Return date must be within 1 year of outbound date
- Both legs must be with the same airline/company
- Round-trip discount applies to total of both tickets
- Minimum discount: 0% (no negative discounts)
- Maximum discount: 50% (business rule to prevent losses)

### Time/Budget Constraints
- Implementation should be completed within reasonable timeframe
- No external API dependencies required
- Use existing infrastructure and services

### Assumptions We're Making
- Round-trip discount is percentage-based (not fixed amount)
- Discount applies to total price of both tickets combined
- Partners will set reasonable discount percentages
- Users understand round-trip vs one-way pricing
- Return flights are available for selected routes
- Same seat class pricing applies to both legs (can be different classes)
- Cancellation of one leg may affect discount (to be defined in cancellation policy)

## Questions & Open Items
**What do we still need to clarify?**

### Resolved
- ✅ Round-trip discount: Percentage-based (10-15% typical)
- ✅ Booking structure: One booking with two linked tickets
- ✅ Price management: Partners can set discount per route
- ✅ Ticket types: Only one-way and round-trip (no multi-city)

### Open Questions
- Should we allow different seat classes for outbound and return in round-trip?
  - **Decision**: Yes, allow different classes but calculate price separately
- What happens if user cancels one leg of round-trip?
  - **Decision**: Handle in cancellation policy (may recalculate price)
- Should round-trip discount apply if booking different seat classes?
  - **Decision**: Yes, discount applies to total regardless of class mix
- Do we need pricing history/audit trail?
  - **Decision**: Yes, for partner price management (future enhancement)
- Should we show "You save $X" prominently?
  - **Decision**: Yes, highlight savings to encourage round-trip bookings

