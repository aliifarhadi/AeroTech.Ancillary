# 05 — FINAL Family Definition Catalog v11.0

This catalog is a **conformance fixture guide**, not a new domain hierarchy.

## FAM01 — Extra / Prepaid Baggage

Model:

```text
ServiceDefinition
Application = Baggage
Quantity Unit = Piece or Kilogram
```

Must prove authoring of:

- piece package (e.g. one additional piece);
- weight package (e.g. 5/10/20/23 kg fixed package);
- route/flight/date/PTC/fare/POS restrictions;
- Paid, Free and NotAvailable outcome variants;
- `0CC` industry semantics only where current reference supports it.

Do not implement excess-baggage percentage/per-kg formulas unless separately authorized.

## FAM02 — Sports / Special Baggage

Represent different equipment as separate ServiceDefinitions/SKUs when their commercial meaning differs.

Use Baggage application only for real baggage descriptors; otherwise Standard is acceptable.

## FAM03 — Wheelchair / Special Assistance

Use Standard application + BookingDefinition.

Fixtures may use established SSR booking codes such as WCHR/WCHS/WCHC, but must not invent an ATPCO industry sub-code that is absent from the authoritative reference dataset.

Prove Free and paid configurations can be authored.

## FAM04 — Meal

Use Standard application.

Different meal products are separate ServiceDefinitions where they are independently saleable.

Prove:

- ADT/CHD price differentiation via separate Provisions;
- flight/date/cabin/fare-family restrictions;
- booking metadata can be represented.

## FAM05 — Travel Insurance

Use Standard application.

Different insurance plans are separate ServiceDefinitions.

Phase 1 models the airline commercial offer only:

```text
plan commercial identity
supplier
sales/travel applicability
PTC/POS rules
fixed filed price
document/booking metadata as configured
```

Do not invent underwriting, medical-limit, policy-activation or claims schemas without a real supplier contract.

## FAM06 — Paid Seat Selection

Use Seat application.

Prove:

- aircraft-specific rules;
- exact seat number list and/or seat characteristic codes;
- cabin/RBD/fare-family/PTC/POS/date pricing;
- fixed prices.

Do not store live seat availability/blocked/occupied state and do not modify FlightFlow.

## FAM07 — Airport Lounge

Use Standard application.

Use industry `0BX` only where its current read-only reference semantics fit.

Prove multiple Suppliers may each define lounge access with different prices for the same airport/route/market.

## FAM08 — Priority Boarding

Standard application. Prove Paid/Free/NotAvailable by fare family/cabin/PTC/POS.

## FAM09 — Fast Track

Standard application. Prove airport/route/date/POS-based authoring.

## FAM10 — Wi-Fi

Standard application. Prove aircraft/flight/cabin/fare-family pricing.

## FAM11 — Pet Service

Standard application + booking metadata where required. Prove sector/journey and fixed-price authoring.

## FAM12 — Meet & Assist / CIP

Standard application. Prove airport/route/PTC/POS/date pricing and multi-supplier definitions.

## FAM13 — UMNR / Special Handling Style Service

Standard application + booking metadata.

Use available PTC and commercial dimensions. Do not add age fields in Phase 1 simply to make the fixture richer.

## Required family proof rule

For every FAM01–FAM13:

```text
Create Draft
Read Detail
Appear in Paginated Backoffice
Persist relevant typed fields
Update Draft and prove the edited fields round-trip before activation
Activate
Read Active state
```

No family is required to Hold/Confirm/Cancel in Phase 1.
