# 00 — FINAL Authority & Source Freeze v9.1

**Status:** `GO_FOR_IMPLEMENTATION`

**Supersedes:** Ancillary v9.0 and all earlier packs.

This document is the highest authority in the v9.1 pack. The coding agent must not infer business behavior that is not explicitly frozen here or in the Domain/Contract documents referenced below.

## 1. Start point

Ancillary MUST be implemented from the reviewed bootstrap:

```text
843cf7ccda45a1e16b1bd537172346fcdd76ae35
```

Current Ancillary donor/reference commit:

```text
7e26946b4547a495e46f360fdee5053cfeb51e07
```

is **donor only**. It may be consulted for already-working patterns such as idempotency, concurrency, cache refresh, read-model synchronization, reference-data synchronization, Supplier CRUD shape and service contract conventions. It is not wholesale domain authority.

## 2. Pinned source baselines

```text
AeroTech.Ancillary bootstrap      843cf7ccda45a1e16b1bd537172346fcdd76ae35
AeroTech.Ancillary donor          7e26946b4547a495e46f360fdee5053cfeb51e07
AeroTech.AirAvail                 8c6f5d18e17caf9eb47dfd039c20df11f76840f9
Aerotech.AirPrice                 1b41f08e9a22d3c27b807d5ea275120f0f7273ad
Aerotech.FlightFlow               d2180b2e4c07789abde75e712e870ece8d2c776b
AeroTech.Ordering.Final           cd50a2a5fd18a372f32f2d4efac08dee0510f6ba
AeroTech.AeroCore                 7304af6a44754077b6ec170490915027835f8ef7
```

The coding agent must inspect the actual source at these commits before changing a cross-service contract.

## 3. Corrected final business boundary

`AeroTech.Ancillary` is an **ancillary marketplace/catalog and supplier-fulfillment service**, not a catalog restricted to services physically supplied by the airline itself.

It may contain multiple suppliers that sell the same broad service type or the same industry sub-code under one airline retailing context.

Examples:

```text
Lounge access -> airline lounge supplier + third-party lounge supplier
Meal          -> airline catering supplier + contracted caterer
Fast track    -> airport supplier A + supplier B
Baggage-like optional service -> airline-owned or contracted supplier where applicable
```

The supplier identity is part of the sold-service provenance and can affect Ancillary's internal reserve/confirm/validate/cancel/fulfillment behavior.

This does **not** collapse unrelated bounded contexts. Complex standalone hotel/car/general travel inventory remains in its own product context where its own domain semantics require it. A third-party supplier is allowed inside Ancillary when the sold product is modeled as an Ancillary service under the contracts in this pack.

## 4. Final aggregate authority

Exactly these Ancillary aggregate roots are required:

```text
Supplier
AncillaryServiceDefinition
AncillaryProvision
AncillaryReservation
AncillaryStockPool
```

Do not create one aggregate per Meal/Lounge/Baggage/Seat/Pet/WiFi.

Do not restore the donor's `AncillaryProduct` / `AncillaryPriceRule` model as the target model.

`ServiceSubCode` is **not** a mutable aggregate root in v9.1. Industry sub-code semantics are validated against a read-only industry reference dataset as defined in the Domain Master.

## 5. Identity rule — internal references use IDs

The previous v8 runtime use of:

```text
ServiceDefinitionRef + Version
```

as an internal reference is removed.

Final rule:

```text
ServiceDefinitionId long
ProvisionId         long
SupplierId          long
OrderServiceId      long
TravellerId         long
FlightId            long
```

are the identifiers used between AeroTech services and inside persisted relationships whenever the canonical entity already has an ID.

`ServiceDefinitionRef` and `Version` remain useful human/business metadata for Backoffice, audit and display, but they are **not foreign keys and not runtime lookup keys**.

Historical stability comes from immutable published rows + persisted IDs + Order snapshots; not from reconstructing a composite business key.

## 6. Supplier authority

The current donor already proves a real `Supplier` concept exists in the codebase. v9.1 keeps it as a required target concept and adds **typed fulfillment routing**.

Frozen Supplier state:

```text
Id                       long
OwnerAirlineId           int
Name                     string(100)
FulfillmentKind          SupplierFulfillmentKind
FulfillmentProviderKey   string(50)?
Status                   Active | Retired
CreatedAt                DateTimeOffset
RetiredAt                DateTimeOffset?
```

```text
SupplierFulfillmentKind
  Local
  External
```

Rules:

- every `AncillaryServiceDefinition` has exactly one `SupplierId`;
- the same service type/sub-code may exist under several Supplier IDs;
- an Active ServiceDefinition must reference an Active Supplier at activation time;
- retiring a Supplier prevents new commercial offers from its definitions but does not invalidate already sold Orders/reservations;
- **Supplier ID is identity, never behavior configuration**;
- `Local` means fulfillment is handled by Ancillary's local supplier capability; no external connector is selected from the Supplier ID;
- `External` requires a non-empty `FulfillmentProviderKey` that selects a registered Ancillary-side supplier adapter/configuration;
- unknown/unregistered provider keys fail deterministically; do not fall back to a switch on `SupplierId`;
- `FulfillmentProviderKey` is a stable adapter/configuration key only; endpoint URL, credentials, secrets, tokens and provider protocol configuration are not Supplier domain state;
- do not use `ServiceDefinitionRef`, service name or sub-code as a supplier-routing key.

The donor `SupplierDeliveryMethod = Internal` is not copied. v9.1 replaces that single-value limitation with the explicit `SupplierFulfillmentKind` above.

Important naming boundary:

```text
OrderService.FulfillmentProviderKey = "Ancillary"
```

routes Ordering to the Ancillary bounded context. By contrast:

```text
Supplier.FulfillmentProviderKey
```

is **internal to Ancillary** and selects an external supplier adapter only when `FulfillmentKind = External`.

## 7. Industry ServiceSubCode reference rule

For `SubCodeSource = Industry`, Ancillary must validate the code against a read-only industry reference dataset.

The dataset contains, at minimum:

```text
ServiceSubCode
ServiceTypeCode
RFIC where applicable
GroupCode
SubGroupCode?
Description1Code?
Description2Code?
CommercialName / canonical description
Document semantics where the approved source provides it
```

Industry semantics are not typed manually by the airline user and cannot conflict with the reference entry.

The donor's hard-coded two-entry `IndustrySubCodeReference` is useful as test evidence only; **it is not a complete production industry table**. The agent must not pretend that two hard-coded rows are the ATPCO/IATA universe.

If the organization-approved complete dataset is not present in source, implement the reference-table/lookup contract and seed only verified fixtures needed by tests; report the missing production reference-data load as data provisioning, not as a reason to invent codes.

Carrier-defined sub-codes remain authorable under the explicit carrier-defined rules in the Domain Master.

## 8. AirInfo authority

Ancillary never invents canonical Aircraft/Airport/City/Country/Airline/Cabin data.

Existing Ancillary bootstrap already consumes:

```text
v1/Financials/Currencies
v1/Locations/Airlines
v1/Locations/Cities
v1/Locations/Airports
v1/Locations/Countries
```

FlightFlow current source additionally proves AirInfo exposes:

```text
GET v1/Aircrafts
GET v1/Aircrafts/CabinClasses
GET v1/Aircrafts/AirCraftDeckCabins
GET v1/Aircrafts/{aircraftId}/DisplaySeatMap
```

Static aircraft layout and authoring-time seat reference are AirInfo authority.

## 9. AirPrice authority

Canonical fare/sales facts include:

```text
AirFareId
AirFareType
FareFamilyId
FareBasis
CabinClassId
RbdId
PointOfSaleId
```

Do not clone FareFamily/RBD/AirFare/POS master data inside Ancillary.

`FareFamilyId` must be propagated end-to-end into AirAvail ancillary evaluation and Ordering accepted air-service state because text `FareFamily` is not a stable identifier.

## 10. Money and FX rule

Ancillary stores **filed amounts only** in the platform money format.

Final persisted monetary convention for the new Ancillary price rows:

```text
CLR: decimal
SQL: decimal(18,2)
```

This follows the current platform/AirPrice money persistence convention. Rate-of-exchange precision is a separate concern and remains high precision where the platform already uses it (for example Ordering ROE snapshot `decimal(19,9)`).

Ancillary performs no FX conversion.

AirAvail owns conversion from filed currency to selling currency.

Most importantly: Ancillary fulfillment/hold does **not** receive or validate `AcceptedRevenue` in selling currency. Price acceptance is an Ordering/AirAvail commercial concern; Ancillary fulfillment validates fulfillment facts, not converted commercial totals.

## 11. One evaluator only

There is exactly one runtime ancillary eligibility/pricing evaluator:

```text
AirAvail AncillaryOfferEvaluator
```

Ancillary owns authoring, invariants, publication and supplier/stock fulfillment state. It does **not** implement a second context evaluator.

Backoffice simulation uses:

```text
POST Backoffice/v1/AncillaryOffers
```

in AirAvail and therefore executes the same evaluator used by live shopping.

Ancillary `Preview` may perform structural/reference/overlap validation on draft Provision rows, but it must not independently decide a live winning Provision for a traveller/flight/fare context.

## 12. Final shopping direction — no AirAvail <-> Ordering call cycle

There are two sales paths.

### Pre-order

```text
Surface -> AirAvail(Offer context) -> offers/details
```

AirAvail reconstructs context from its own trusted Offer state.

### Post-order

```text
Surface -> Ordering(OrderId)
Ordering builds authoritative Order shopping context
Ordering -> AirAvail(context)
AirAvail -> Ordering result
Ordering -> Surface
```

For Details the direction remains the same:

```text
Surface -> Ordering -> AirAvail Details -> Ordering -> Surface
```

**AirAvail must never call Ordering to obtain Order context.**

This removes the v8 two-way dependency.

AirAvail still owns ancillary offer composition, pricing/FX, the single evaluator and surface-specific ancillary response semantics. Ordering owns the post-order entry point because it owns the trusted Order state.

## 13. Flydubai benchmark — exact conclusions used in v9.1

Verified against the public flydubai OpenAPI document:

```text
https://devportalapi.flydubai.com/devportal/tryout/1.1
```

Relevant real endpoints:

```text
POST /pricing/flightswithfares
POST /order/cart
POST /pricing/services
POST /pricing/seats
POST /cp/summaryPNR
POST /cp/commitPNR
POST /order/payment/processFOP
POST /cp/RetrievePNR
```

Verified behavior used by v9.1:

- ancillary shopping is a pricing/inventory step separated from PNR creation;
- `pricing/services` returns service quotes per segment/leg with service code, category, currency, amount, rule ID, quantities and maximum counts;
- seats are a separate priced-seat-map flow;
- selected ancillary services are committed in `SummaryPNR` using numeric `ServiceID` plus passenger/flight/amount/currency facts;
- RetrievePNR returns the current order including ancillary purchases.

This supports AeroTech's split of shopping from fulfillment and the use of internal IDs rather than composite business keys in runtime commitment.

## 14. Lufthansa benchmark — additive conclusions only

Public/partner Lufthansa documentation confirms:

- static reference data and dynamic offer/seat-map concerns are separate;
- order/service structures include `ValidatingCarrier`;
- optional-service settlement includes methods such as direct settlement, EMD-associated and EMD-standalone;
- document/service data includes `FeeOwner` and provider-facing `Present To/At` concepts;
- service list data carries RFIC/RFISC, group/subgroup, service type and booking instructions.

This is used only to confirm that optional services may have provider/settlement identity beyond "airline supplied by definition". It does not replace ATPCO S5/S7 semantics or the existing AirInfo/FlightFlow seat boundary.

## 15. Cross-context responsibility freeze

| Concern | Final authority |
|---|---|
| Supplier identity and supplier-owned ancillary fulfillment state | Ancillary |
| Optional-service commercial definition | Ancillary |
| Provision/applicability/filed ancillary fee | Ancillary |
| Industry service-sub-code reference validation | Ancillary read-only industry reference dataset |
| Static Aircraft/Cabin/Seat layout reference | AirInfo |
| Fare/FareFamily/RBD/POS master | AirPrice |
| Flight inventory/live physical seat state | FlightFlow |
| Pre-order ancillary shopping | AirAvail |
| Post-order ancillary shopping entry/context | Ordering |
| Post-order offer evaluation/pricing/FX/response composition | AirAvail |
| Single ancillary evaluator | AirAvail |
| Order source of truth | Ordering |
| Generic provider orchestration | Ordering |
| Ancillary supplier/quota/hold state | Ancillary |
| Seat hold/assignment | FlightFlow |
| ETKT/EMD accountable-document issuance | Ordering |

## 16. No architecture/layer redesign

The source already determines projects, layers, mediator/query patterns, persistence conventions, controller conventions, authorization surfaces and dependency style.

The Pack freezes **business/domain/contracts**, not a new solution architecture.

Prohibited:

- introducing a new project/layer because a document contains a new concept;
- renaming existing projects/namespaces for style;
- replacing current mediator/query/cache/reference conventions without a concrete v9.1 requirement;
- adding a generic rules engine/DSL;
- inventing a second pricing engine in Ancillary.

If a frozen v9.1 business contract genuinely cannot be implemented within the existing source structure, report the exact conflict as `REPORT_GAP_AND_STOP` instead of redesigning the architecture.

## 17. Final implementation blockers / execution order

No unresolved CORE domain-design question remains. Execution is deliberately **vertical and feedback-first**.

The implementation milestones are:

```text
M1  walking Standard/FixedAmount ancillary end-to-end
M2  typed Supplier marketplace routing + post-order Ordering -> AirAvail
M3  complete commercial criteria + reference data + Backoffice
M4  baggage + Ancillary quota + accountable EMD
M5  paid seat
M6  closure/conformance/deletion audit
```

The coding agent stops after every milestone for architect audit. Reference selectors, Bulk authoring and advanced service families are introduced with the milestone that consumes them rather than being prerequisites for the first sale.

Cross-cutting blockers that apply from the first commit:

1. internal relationships are ID-based;
2. Supplier behavior is routed by `FulfillmentKind + FulfillmentProviderKey`, never numeric Supplier ID;
3. AirAvail remains the single runtime evaluator;
4. post-order call direction is Ordering -> AirAvail;
5. Ancillary Hold has no selling-currency/FX validation;
6. no dead source-gated fields;
7. no generic Supplier post-confirm Issuance/Activation API without a real supplier contract;
8. accountable EMD authority stays in Ordering.
