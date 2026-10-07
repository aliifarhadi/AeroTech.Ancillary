# 07 — FINAL Implementation Plan v11.0

## One authorized phase only

There is no autonomous v11 Phase 2.

The coding agent implements **Phase 1 only** and then stops. The owner audits and decides the next stage.

## Baseline

```text
AeroTech.Ancillary @ e2a8c9f96ec486fbe429e2e1d9947dd83ca3b9c8
```

The baseline already includes:

- Supplier registration/list/detail;
- ServiceDefinition define/activate/list/detail;
- Provision define/activate/list/detail;
- simple Provision pricing/outcome/quantity/settlement/availability/fulfillment fields;
- Hold/Get/Confirm operational code;
- pagination added at `e2a8c9f...`.

## Work order inside Phase 1

This order controls risk but does not authorize future phases.

### Step P1.1 — Source audit and freeze guards

Before editing:

- verify exact HEAD;
- list all files under Supplier/ServiceDefinition/Provision/Reservation;
- record baseline tests;
- create a change map limited to commercial authoring;
- assert Reservation paths are forbidden-to-change.

### Step P1.2 — Typed commercial rule model

Evolve `AncillaryProvision` from the baseline by adding only:

```text
PassengerCriteria
SalesCriteria
TravelCriteria + RoutePairs
FareCriteria
AdvancePurchaseCriteria
ProvisionApplication = Standard | Baggage | Seat
BaggageApplication
SeatApplication
price-line evidence fields CountryId/StationAirportId
```

Preserve current valid fields and existing framework style.

No evaluator.

### Step P1.3 — Persistence/read model

Add command/query persistence and migrations for the typed authoring fields.

All fields must round-trip.

Do not add generic JSON/EAV criteria tables.

### Step P1.4 — Backoffice completeness

Complete Draft edit, lifecycle and practical list/detail filters for ServiceDefinition/Provision, plus only the minimal Supplier lifecycle needed to manage existing status.

No Preview/Simulate/Bulk.

### Step P1.5 — Family conformance

Create FAM01–FAM13 fixtures/tests from document 05.

Tests prove authoring/persistence/read/lifecycle only.

### Step P1.6 — Closure

Run:

```text
full solution build
all Ancillary tests
migration consistency
source scan: no evaluator
source diff: no Reservation functional changes
source diff: no other repo changes
```

Only then emit:

`ANCILLARY_V11_PHASE1_AUTHORING_COMPLETE_READY_FOR_OWNER_AUDIT`

Then STOP.

## Forbidden implementation shortcuts

The agent must not:

- implement rule matching because typed criteria now exist;
- add a fake `Evaluate` endpoint for tests;
- add reservation behavior to demonstrate the families;
- create a family-specific aggregate for wheelchair/meal/insurance/lounge;
- create a generic JSON metadata bag;
- modify AirAvail/Ordering/FlightFlow;
- remove existing operational code as cleanup;
- refactor `FulfillmentDefinition` or supplier runtime design during this phase;
- invent industry subcodes;
- silently resolve an authority conflict.

If a material ambiguity blocks authoring, return `REPORT_GAP_AND_STOP` with the exact source/spec conflict and smallest options. Do not choose a new architecture.
