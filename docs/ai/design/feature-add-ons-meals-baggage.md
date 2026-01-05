---
phase: design
title: System Design & Architecture
description: Define the technical architecture, components, and data models
---

# System Design & Architecture

## Architecture Overview
**What is the high-level system structure?**

- Add-on selection UI (booking) → UserController booking flow → PriceCalculator (add-on aware) → Payment (VNPay) with total including add-ons → Ticket persistence with TicketAddOn entries → Display in MyBookings/Ticket/Check-in.
- Components: Booking UI, UserController, PriceCalculatorService, CurrencyService (reuse), VNPay client (reuse), AddOnDefinition/Repository, TicketAddOn mapping, Trip/Ticket repos.
- Stack: ASP.NET Core MVC, EF Core (SQLite), VNPay, Tailwind/HTML views with JS for dynamic totals.

`mermaid
graph TD
    UserUI[Booking UI] -->|select seat + add-ons| UserController
    UserController --> PriceCalc[Price Calculator]
    UserController --> AddOnRepo[(AddOnDefinition + TicketAddOn)]
    UserController --> TicketRepo[(Tickets)]
    UserController --> TripRepo[(Trips)]
    PriceCalc --> CurrencySvc[CurrencyService]
    UserController --> VNPay[VNPay Client] --> PaymentRedirect[Payment URL]
    TicketRepo --> MyBookings
    TicketRepo --> TicketView
`

## Data Models
**What data do we need to manage?**

- AddOnDefinition:
  - Id, Type (Meal/Baggage), Code, Name, Description, PriceUSD, SeatClass applicability, Trip applicability (optional), Active flag.
- TicketAddOn:
  - Id, TicketId, AddOnDefinitionId, Quantity, PriceUSD, LegType (Outbound/Return), IsCancelled, CancelledAt.
- Data flow:
  - UI posts selected add-on codes/quantities per leg → controller validates against definitions → price calc sums ticket prices + add-ons → persisted as TicketAddOn per ticket/leg.

## API Design
**How do components communicate?**

- MVC actions:
  - GET User/BookTrip supplies add-on options for outbound (and return if round-trip).
  - POST User/ConfirmBooking accepts selected add-ons per leg.
  - GET User/CancelTicket (existing) cascades cancellation to TicketAddOn.
- Internal services:
  - PriceCalculator: extend to include add-ons in price breakdown.
  - CurrencyService: convert combined total USD→VND for VNPay.
- Auth: existing session-based auth; no new external APIs.

## Component Breakdown
**What are the major building blocks?**

- Frontend: Booking form add-on selectors (meals, baggage) per leg; dynamic price update; display add-ons in MyBookings/Ticket/Check-in.
- Backend:
  - Controllers: UserController booking + cancellation aware of add-ons.
  - Services: PriceCalculatorService extended; CurrencyService reused.
  - Data: AddOnDefinition seeding/config, TicketAddOn persistence, migrations.
- Storage: SQLite tables for AddOnDefinition, TicketAddOn; link to Tickets.
- Third-party: VNPay unchanged.

## Design Decisions
**Why did we choose this approach?**

- Separate definition vs instance (TicketAddOn) for flexibility and reuse across trips/classes.
- Store prices in USD to align with existing ticket pricing; convert once for payment.
- Per-leg add-ons for round-trip keeps cancellation and display simple.
- Server-side validation of codes/prices to prevent tampering.

## Non-Functional Requirements
**How should the system perform?**

- Performance: small datasets; price calc should remain instant (<50ms server-side).
- Scalability: EF queries should be indexed on TicketId/TripId; add-ons volume is low.
- Security: validate add-on codes and prices server-side; never trust client totals.
- Reliability: cancellation should mark TicketAddOn cancelled within transaction; payment failure must not persist add-ons.
