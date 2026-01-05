---
phase: implementation
title: Implementation Guide
description: Technical implementation notes, patterns, and code guidelines
---

# Implementation Guide

## Development Setup
**How do we get started?**

- Prerequisites and dependencies
- Environment setup steps
- Configuration needed

## Code Structure
**How is the code organized?**

- Directory structure
- Module organization
- Naming conventions

## Implementation Notes
**Key technical details to remember:**

### Core Features
- Add-on capture in `Views/User/BookTrip`: meal (None/Standard/Vegetarian/Special) and baggage (None/15kg/20kg/30kg) radio selectors.
- Add-on pricing handled server-side in `UserController.CalculateAddOnPrice` with fixed USD prices (meal: 0/10/12/15, baggage: 0/20/30/40) and multiplied per leg for round-trip.
- Ticket persistence uses new enum-backed fields on `Ticket` (`MealOption`, `BaggageOption`) plus `AddOnPrice` per leg; `TotalPrice` already stores base fare + add-ons.
- Payment conversion uses `ticket.TotalPrice` (one-way) or base total plus add-ons (round-trip) before VNPay call to ensure the charge matches UI totals.

### Patterns & Best Practices
- Design patterns being used
- Code style guidelines
- Common utilities/helpers

## Integration Points
**How do pieces connect?**

- API integration details
- Database connections
- Third-party service setup

## Error Handling
**How do we handle failures?**

- Error handling strategy
- Logging approach
- Retry/fallback mechanisms

## Performance Considerations
**How do we keep it fast?**

- Optimization strategies
- Caching approach
- Query optimization
- Resource management

## Security Notes
**What security measures are in place?**

- Authentication/authorization
- Input validation
- Data encryption
- Secrets management

