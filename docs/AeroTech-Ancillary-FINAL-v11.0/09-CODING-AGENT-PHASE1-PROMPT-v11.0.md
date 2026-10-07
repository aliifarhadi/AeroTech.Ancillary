# CODING AGENT PROMPT — AeroTech Ancillary v11.0 — PHASE 1 ONLY

You are implementing the owner-authorized **Phase 1 commercial authoring** in the existing `AeroTech.Ancillary` repository.

## 0. Absolute source baseline

Expected functional baseline:

```text
Repository: aliifarhadi/AeroTech.Ancillary
Commit:     e2a8c9f96ec486fbe429e2e1d9947dd83ca3b9c8
```

Before editing, print/verify HEAD and compare to this SHA.

If HEAD differs by functional code not explicitly authorized by the owner, do not overwrite it. Return `REPORT_GAP_AND_STOP` with commit/file diff.

## 1. Read all v11 files first

Read in this order:

```text
00-FINAL-Authority-and-Source-Freeze-v11.0.md
02-FINAL-Ancillary-Phase1-Domain-Master-v11.0.md
03-FINAL-Backoffice-Authoring-Contracts-v11.0.md
04-FINAL-Pricing-and-Eligibility-Rule-Authoring-v11.0.md
05-FINAL-Family-Definition-Catalog-v11.0.md
06-FINAL-Scenario-Conformance-v11.0.md
07-FINAL-Implementation-Plan-v11.0.md
08-FINAL-Out-of-Scope-and-Gap-Register-v11.0.md
11-FINAL-Phase1-Exit-Criteria-v11.0.md
01-FINAL-Industry-Benchmark-and-Phase1-Stress-Test-v11.0.md
```

v11 supersedes v10 and earlier packs wherever they conflict.

Do not implement from memory of v10.

## 2. Owner boundary — non-negotiable

Implement only:

> complete ancillary definition + typed sale conditions + fixed filed pricing + Backoffice management.

Do not decide or implement what comes after Phase 1.

### Other repositories — read only

Do not modify:

```text
AeroTech.AirAvail
AeroTech.Ordering.Final
AeroTech.FlightFlow
Aerotech.AirPrice
AeroTech.AeroCore
any other repository
```

## 3. Frozen operational paths

Phase 1 must not functionally modify files under the existing reservation/hold/confirm implementation, including paths matching:

```text
**/AncillaryReservationAggregate/**
**/RestApi/**/AncillaryReservationAggregate/**
```

Do not add Cancel/Release/Issue/Split/History or partial servicing.

Do not delete the baseline Hold/Get/Confirm code.

If a commercial-model change appears to require modifying Reservation code, stop and report the conflict instead of changing it.

## 4. Forbidden evaluator code

Do NOT create any equivalent of:

```text
AncillaryEvaluation/
IAncillaryCommercialEvaluator
AncillaryCommercialEvaluator
ProvisionCriteriaMatcher
EvaluationContext
FlightContext
FareContext for runtime matching
EvaluateAncillariesService
Service/v1/AncillaryEvaluations
Backoffice Simulate
```

Typed authoring criteria are data, not an invitation to implement shopping.

## 5. Required Phase 1 domain work

Evolve the existing `AncillaryProvision` cleanly; do not create a parallel v11 aggregate.

Add typed authoring blocks:

```text
PassengerCriteria
  PassengerTypeCodes[]  // use AeroTech.Messages.AirPrice.Enums.PassengerTypeCode

SalesCriteria
  PointOfSaleIds[]
  CustomerIds[]
  CustomerTypes[]  // use AeroTech.Messages.Core.Enums.CustomerType

TravelCriteria
  OriginAirportIds[]
  DestinationAirportIds[]
  ViaAirportIds[]
  RoutePairs[] { OriginAirportId, DestinationAirportId, Direction }
  TravelFrom / TravelTo
  DaysOfWeek[]
  TimeFrom / TimeTo
  MarketingAirlineIds[]
  OperatingAirlineIds[]
  FlightNumbers[]
  FlightIds[]
  AircraftIds[]

FareCriteria
  AirFareIds[]
  AirFareTypes[]  // use AeroTech.Messages.AirPrice.Enums.AirFareType
  FareFamilyIds[]
  FareBasisCodes[]
  CabinClassIds[]
  RbdIds[]

AdvancePurchaseCriteria?
  Period
  Unit = AeroTech.Messages.AirPrice.Enums.TimeUnit (reuse existing enum; do not create a duplicate)

ProvisionApplication
  Standard
  Baggage
  Seat
```

Baggage and Seat fields are exactly those in the Domain Master. Do not invent a generic metadata dictionary.

Keep existing `Quantity`, `Outcome`, `Fee`, `PriceLines`, `Settlement`, `Availability`.

Preserve existing baseline `Fulfillment` field untouched semantically; do not build runtime behavior around it in Phase 1.

## 6. Passenger pricing

Do not add AdultPrice/ChildPrice/InfantPrice.

Prove separate Provisions can represent:

```text
ADT -> 25 EUR
CHD -> 15 EUR
INF -> Free or NotAvailable
```

No matcher is required. Only author/persist/read these rows.

## 7. Backoffice completeness

Keep existing route families and extend them rather than creating alternate APIs.

Required:

```text
Supplier: existing create/list/detail + exactly `Retire` (Retired is terminal in Phase 1)
ServiceDefinition: create/list/detail + Draft edit + Activate/Suspend/Reactivate/Retire + required Revise that creates Version+1 as a new Draft numeric Id
Provision: create/list/detail + Draft edit + Activate/Suspend/Reactivate/Retire
```

Practical typed filters only. No generic query DSL.

No Preview/Bulk/Simulate.

## 8. Industry codes

Do not invent ATPCO codes.

Use `SubCodeSource=Industry` only when current `IndustryServiceSubCodeReference` contains the code.

Otherwise use the current CarrierDefined path in fixtures.

Do not create ServiceSubCode CRUD/master aggregate.

## 9. Persistence/read model

Every new typed field must be represented consistently in:

```text
Domain
Application command/input/result
Persistence configuration/migration
Read-model projection
Query model/configuration/migration
Backoffice detail DTO/mapper
Draft update path
```

Prefer typed columns/typed owned/value structures consistent with the repository. Do not persist a generic JSON rules document.

Do not edit historical migrations in a way inconsistent with repository convention. Add normal migrations from the exact baseline and prove no pending model changes.

## 10. Family fixtures

Implement explicit authoring/read/lifecycle fixtures for:

```text
FAM01 Extra/prepaid baggage
FAM02 Sports/special baggage
FAM03 Wheelchair/special assistance
FAM04 Meal
FAM05 Travel insurance
FAM06 Paid seat selection commercial rule
FAM07 Airport lounge
FAM08 Priority boarding
FAM09 Fast track
FAM10 Wi-Fi
FAM11 Pet service
FAM12 Meet & Assist / CIP
FAM13 UMNR/special handling style service
```

These tests must NOT call Hold/Confirm.

Must include the price-stress cases from Scenario Conformance.

## 11. Source audit protections

Before completion, prove with source/diff scans:

```text
no new evaluator namespace/type/service
no functional changes under AncillaryReservationAggregate
no other repository changes
no family-specific aggregate explosion
no age/FF/occurrence/PCC fields
no dynamic/external pricing
no duplicate reference-data master
```

## 12. Ambiguity protocol

If a requirement materially conflicts with the baseline or exact field semantics are not frozen, return:

`REPORT_GAP_AND_STOP`

Include:

```text
exact HEAD SHA
file/path
current behavior
v11 requirement
why conflict is material
smallest 2–3 options
impact of each
```

Do not choose a new architecture yourself.

## 13. Completion report

Report:

```text
commit SHA(s)
files changed grouped by Domain/Application/Persistence/Query/API/Tests
migrations added
typed criteria implemented
Backoffice lifecycle implemented
family scenario -> test map
build command/result
test commands/pass counts
source scan proving no evaluator
source diff proving reservation operational code unchanged
confirmation no other repo changed
remaining gaps
```

Only if every Phase 1 exit criterion passes, emit exactly:

`ANCILLARY_V11_PHASE1_AUTHORING_COMPLETE_READY_FOR_OWNER_AUDIT`

Then STOP.

Do not propose, start or implement Phase 2. The owner alone decides the next step.
