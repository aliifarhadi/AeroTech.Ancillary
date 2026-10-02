# Phase 6 — Pricing depth

**Authority:** `Ancillary-Domain-Master.md` (R5.4).
**Precondition:** Phase 2 closed. It does not depend on Phases 3–5; the Owner may schedule it earlier.
**Business outcome:** a price can carry fees and can differ by cabin, booking class, aircraft, sales channel, agency and country of sale; a product can be free and need no document.

## 1. What is added

| Master section | Added in Phase 6 |
|---|---|
| §5 | `Fee` lines (`Amount > 0`, `Code` required); the `Ancillary` amount may be 0; conditions `CabinClassIds`, `RbdIds`, `AircraftIds`, `TravelAgencyIds`, `PointOfSaleCountryIds`, `Channels` |
| §7 | `salesContext` is used by those conditions. A condition on a value the request did not send does not match. |
| §3 | `None`: allowed for a product all of whose Active rules have a zero `Ancillary` line and no other line; otherwise activating such a rule is `16206` |
| §9 | `Fee`, `None` |

Already fixed:

1. No currency conversion.
2. No formulas and no percentages: every line is a fixed amount per unit.
3. A new condition is a new optional member; existing rules behave exactly as before.
4. No new aggregate, operation or message.

## 2. Not in this phase

Dynamic pricing, bundles, discounts and promotions, loyalty-tier pricing, percentage taxes.
