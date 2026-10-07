# 08 — FINAL Out-of-Scope and Gap Register v11.0

These are intentionally **not Phase 1 implementation requirements**.

## Operational reservation/fulfillment

Frozen until owner opens that stage:

```text
Hold redesign
Confirm redesign
partial confirm
Release
Cancel
Issue
Split
Reservation history
AcceptedCommercialSnapshot
ProviderReservationRef
ProviderUnitRef behavior
StockPool/quota
supplier adapters
```

Future context to preserve: owner wants the operational shape benchmarked against actual FlightFlow and favors one overall hold reference plus per-unit references for partial servicing.

## AirAvail shopping

Future owner-controlled stage:

```text
read published Ancillary definitions/provisions
apply concrete itinerary/passenger/fare/POS context
return saleable ancillary offers and prices
seat-offer composition
FX/selling currency if required
```

No Ancillary evaluator is a placeholder for this stage.

## Ordering

No Phase 1 contract or change.

## Advanced criteria not authorized

```text
Age bands
DOB-based rules
Frequent-flyer tier
traveller occurrence index
Customer score
keyword rule
PCC
TicketDesignator
AccountCode
TourCode
Tariff/Rule qualifiers
```

## Advanced pricing not authorized

```text
External quote
Dynamic pricing
Mileage pricing
percentage-of-fare formulas
exact per-kg surcharge formulas not fully frozen
FX
```

## Family-specific supplier schemas

Do not invent:

```text
insurance underwriting/policy claims model
SIM/eSIM activation model
lounge QR/voucher protocol
meal caterer protocol
pet document workflow
wheelchair supplier workflow
```

A real contract must prove the need.

## Industry ServiceSubCode dataset

Current source contains only a small read-only subset. Expanding to a complete licensed/reference dataset is a separate data-governance task. Phase 1 fixtures use CarrierDefined when an authoritative industry entry is unavailable.

## Legacy baseline fields that are not future decisions

The source baseline contains operational fields such as:

```text
Supplier.FulfillmentKind
Supplier.FulfillmentProviderKey
Provision.Fulfillment
```

v11 Phase 1 preserves them to avoid unrelated operational refactoring. Their existence does not freeze the future fulfillment architecture. The owner will decide later.
