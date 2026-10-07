# 00 — FINAL Authority and Source Freeze v11.0

## 1. Absolute authority

This document freezes the owner-approved Phase 1 boundary.

The implementation source baseline is exactly:

```text
Repository: aliifarhadi/AeroTech.Ancillary
Commit:     e2a8c9f96ec486fbe429e2e1d9947dd83ca3b9c8
```

If HEAD differs when implementation starts, the coding agent MUST first compare the functional diff to this SHA. If the difference is more than v11 documentation or owner-approved Phase 1 work, it must stop and report the exact commits/files before changing code.

## 2. Phase 1 scope — only this

Phase 1 completes the **definition and commercial authoring side** of Ancillary:

```text
Supplier identity already present
AncillaryServiceDefinition
AncillaryProvision
classification
booking/document metadata
coverage definition
passenger/sales/travel/fare criteria
advance-purchase criteria
quantity rules
fixed filed prices and price lines
baggage-specific authoring
seat-commercial authoring
Backoffice create/read/list/edit/lifecycle
query/read-model persistence
10–15 representative ancillary-family fixtures/tests
```

The Phase 1 Backoffice must be capable of defining and managing at least:

```text
extra/prepaid baggage
sports/special baggage
wheelchair / special assistance
meals
travel insurance
paid seat selection commercial rules
airport lounge
priority boarding
fast track
Wi-Fi
pet service
meet & assist / CIP
unaccompanied-minor / special handling style service
```

The generic model must support these without one aggregate/table/class hierarchy per family.

## 3. Explicitly NOT Phase 1

The coding agent MUST NOT implement or redesign any of the following:

```text
runtime ancillary matching/evaluation engine in Ancillary
Service/v1/AncillaryEvaluations
AirAvail changes
Ordering changes
FlightFlow changes
Payment changes
supplier integration adapters
supplier network calls
StockPool/quota
Hold redesign
Confirm redesign
Release
Cancel
Issue
Reservation history
Reservation snapshots
partial servicing
Split
document-number / EMD issuance
FX / selling-currency conversion
dynamic pricing
external quote pricing
percentage-of-fare pricing
new generic workflow/orchestration framework
```

## 4. Frozen existing operational code

The baseline already contains operational code under `AncillaryReservationAggregate` and Service Hold/Confirm endpoints. During Phase 1:

- do not delete it;
- do not expand it;
- do not rename it;
- do not add fields to it to support future plans;
- do not use it to justify changes in the commercial model;
- do not add tests that require new reservation behavior.

This is deliberate. The owner will decide its future after Phase 1.

## 5. Important future context — preserve but do not implement now

The owner has stated that when the operational reservation phase is explicitly opened, its Hold/Confirm/Cancel shape should be benchmarked closely against the **actual FlightFlow source**, favoring the simple pattern:

```text
overall Hold reference
+
independent per-unit references
```

so later partial cancel/change/refund can target a passenger/service/flight occurrence without cancelling the whole hold.

Do not implement this in Phase 1. Preserve this decision for the future chat.

## 6. AirAvail boundary — future, owner-controlled

The intended high-level retailing direction remains:

```text
Ancillary = source of authored/published ancillary definitions and filed rules
AirAvail  = later shopping/orchestration layer that determines what is saleable for a concrete flight/passenger/fare context
```

Flydubai's public API is the concrete simplicity benchmark: its `POST /pricing/services` is a shopping API that returns ancillary offers for selected flights with price and availability. v11 therefore does **not** move shopping evaluation into Ancillary.

No AirAvail contract or endpoint is authorized in Phase 1.

## 7. Source hierarchy for Phase 1

When a decision is needed:

1. this v11 owner authority;
2. actual source at `e2a8c9f...`;
3. ATPCO/IATA industry concepts for semantics;
4. Flydubai public APIs as a simplicity/API-shape benchmark;
5. older packs only as historical evidence, never authority.

When a material point is not frozen here or supported by source, the agent must `REPORT_GAP_AND_STOP`; it must not invent a new architecture.
