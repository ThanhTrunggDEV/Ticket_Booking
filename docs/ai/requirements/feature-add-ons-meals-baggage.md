---
phase: requirements
title: Requirements & Problem Understanding
description: Clarify the problem space, gather requirements, and define success criteria
---

# Requirements & Problem Understanding

## Problem Statement
**What problem are we solving?**

- Travelers currently cannot purchase add-ons (in-flight meals, extra/checked baggage) during booking, leading to missed ancillary revenue and fragmented user experience.
- Affects end-users (travelers) who need baggage and meal options, partners/airlines wanting upsell revenue, and support teams handling post-booking add-on requests manually.
- Current workaround is offline/phone/email add-on requests or airport upsell, causing inconsistent pricing, longer queues, and poor conversion.

## Goals & Objectives
**What do we want to achieve?**

- Primary goals:
  - Allow users to add meals and baggage during booking checkout for one-way and round-trip tickets.
  - Reflect add-on prices in booking total and payment (VNPay).
  - Persist add-on selections on tickets and show them in MyBookings / Ticket / Check-in views.
- Secondary goals:
  - Allow post-booking add-on purchase (if time allows, optional).
  - Provide partner-facing config for add-on pricing per trip/class (optional, phase 2).
- Non-goals:
  - Loyalty points or coupons for add-ons.
  - Airport/last-minute add-on flows.
  - Dynamic catering inventory management.

## User Stories & Use Cases
**How will users interact with the solution?**

- As a traveler, I want to choose checked baggage allowance (e.g., 15kg/20kg/30kg) when booking so I can avoid airport fees.
- As a traveler, I want to select a meal (standard/vegetarian/special) per passenger/segment so I’m assured of availability onboard.
- As a traveler, I want the total price to include add-ons and see a clear breakdown before payment.
- As a traveler, I want to see my add-on selections in MyBookings and on the ticket/boarding pass for proof.
- As a traveler, I want to cancel a ticket and have add-ons marked as cancelled with the ticket.
- (Optional) As a traveler, I want to add baggage/meals after booking if the flight hasn’t closed check-in.

Key workflows:
- One-way booking: select seat class, add baggage and meal, pay, view ticket with add-ons.
- Round-trip booking: choose add-ons per leg (outbound/return), prices sum into total, reflect on both tickets.
- Cancellation: cancelling outbound in round-trip also cancels its add-ons; return handled per leg.

Edge cases:
- Add-ons unavailable for a class/trip (show disabled).
- Partial availability by leg (return not offering meals).
- Currency conversion (add-ons priced in USD in DB, convert to VND for payment like tickets).
- Payment failure should not create add-ons.
- Check-in blocked? Still allow viewing add-ons; no change to add-on state.

## Success Criteria
**How will we know when we're done?**

- Users can add baggage and meals in the booking UI; selections persist to tickets.
- Totals include add-on prices; VNPay charge matches displayed total.
- Add-ons visible in MyBookings, Ticket view, and (if applicable) Check-in.
- Cancellation marks add-ons as cancelled and restores no seat change needed.
- Round-trip: both legs can have independent add-ons; outbound cancellation cancels outbound add-ons (and return if return is auto-cancelled).
- No regression to existing booking/price flows (x10 bug avoidance).

## Constraints & Assumptions
**What limitations do we need to work within?**

- Technical:
  - Prices stored in USD; payment requires VND conversion via CurrencyService.
  - Existing VNPay integration and booking flows must remain stable.
  - SQLite schema changes via EF Core migrations.
- Business:
  - Fixed add-on price tables per trip/seat class (no dynamic inventory).
  - Meal options are predefined; baggage tiers are predefined weights.
- Time:
  - Phase 1 focuses on booking-time add-ons; post-booking add-on purchase is optional.
- Assumptions:
  - One passenger per booking (existing assumption).
  - Add-ons are per ticket/leg (not shared across passengers).

## Questions & Open Items
**What do we still need to clarify?**

- Should add-ons be editable after booking (within time window)? (scope optional)
- Are there baggage tier limits per class/trip (e.g., max 30kg)?
- Do meals vary by route/company, or global list?
- Should cancellations refund add-on amounts or just mark as cancelled? (current scope: mark cancelled, no refund logic)
- Any caps on add-on quantity (multiple meals not allowed? assume 1 meal per leg).

