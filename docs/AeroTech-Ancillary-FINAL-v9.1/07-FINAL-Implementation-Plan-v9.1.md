# 07 — FINAL Implementation Plan v9.1

**Status:** execution authority.  
**Supersedes:** the three-phase v9.0 execution plan.  
**Principle:** implement **vertical, executable milestones with early cross-service feedback**. Do not complete a large horizontal domain/backoffice layer before proving that one ancillary can actually be shopped, accepted and fulfilled.

This plan does not redesign folders, layers, mediator style, persistence framework or solution architecture. Existing repositories remain architecture authority.

---

# 1. Execution rules

1. Implement **one milestone at a time**.
2. At the end of every milestone, stop and produce the required report/marker for architect audit.
3. Do **not** continue into the next milestone in the same implementation run unless the architect/owner explicitly authorizes it after review.
4. A milestone may be split further if source reality proves it too large. There is no artificial prohibition on an additional checkpoint.
5. A CORE requirement may not be silently deferred merely to close a milestone.
6. If source and Pack conflict materially, use `REPORT_GAP_AND_STOP` with evidence.
7. Every milestone must leave the repositories buildable and tests green for the scope already implemented.
8. No milestone may introduce dead placeholder fields for FUTURE criteria.

The plan currently defines **six milestones**. Six is an execution plan, not a domain invariant.

---

# Milestone 1 — Walking Ancillary: first real end-to-end sale

## Goal

Prove the architecture with the smallest useful ancillary before implementing the full authoring/reference/rule surface.

A simple airline/local ancillary must flow:

```text
Ancillary authoring
-> published catalog
-> AirAvail pre-order AncillaryOffers / Details
-> Ordering accepted ancillary service
-> Ordering reservation orchestration
-> Ancillary Hold
-> Ancillary Confirm
```

This milestone must produce an ancillary that is actually shoppable and commit-able.

## M1.1 Ancillary minimum commercial domain

Implement the minimum v9.1 model required by the slice:

```text
Supplier
AncillaryServiceDefinition
AncillaryProvision
AncillaryReservation
```

Do not implement `AncillaryStockPool` yet unless the selected walking service genuinely needs quota. The root remains in the final model and is implemented in M4.

Walking Supplier:

```text
FulfillmentKind = Local
```

Required Supplier fields already use the final v9.1 shape, including typed fulfillment routing.

Walking ServiceDefinition:

- one normal optional service from an **already source-backed/test-backed classification fixture**; do not invent an industrial subcode merely to make M1 easy;
- no paid seat;
- no baggage-specific semantics;
- M1 stops before accountable document issuance. Prefer an existing no-document fixture if source already has one; if not, an existing verified service definition may carry document metadata but M1 does not implement/pretend document issuance;
- immutable published identity by numeric `Id`.

Walking Provision:

- no optional commercial criteria are required to match;
- `StandardApplication`;
- `FixedAmount` only;
- platform money convention;
- no quota.

## M1.2 Minimal authoring only

Implement only the commands/endpoints needed to create and activate the walking data:

```text
Supplier: register/detail
ServiceDefinition: define/detail/activate
Provision: define/detail/activate
```

Do **not** implement in M1:

```text
PricingMatrix Preview
Bulk
Publish orchestration
full reference selector API
IndustryServiceSubCode production dataset
seat-map authoring
all applicability criteria
```

Use only the reference data genuinely required by the walking fixture.

## M1.3 Published catalog

Produce the minimal published read projection consumed through the existing AirAvail ancillary catalog/cache path.

It must carry at least:

```text
SupplierId
ServiceDefinitionId
ProvisionId
name/classification needed for display
filed currency and FixedAmount lines
Document / Booking / Settlement metadata required by current Details shape
FulfillmentProviderKey
```

## M1.4 AirAvail pre-order shopping

Use the existing AirAvail evaluator path. Do not create an evaluator in Ancillary.

Implement enough for:

```http
POST {surface}/v1/AncillaryOffers
POST {surface}/v1/AncillaryOffers/Details
```

for the walking service in Offer context.

M1 does not need the full future criteria context. It must not invent criteria that are not yet used.

## M1.5 Ordering acceptance + fulfillment

Ordering must:

- accept internal Details with numeric `ServiceDefinitionId`, `ProvisionId`, `SupplierId`;
- snapshot sold commercial evidence;
- create the corresponding ancillary OrderService using current Ordering conventions;
- set `OrderService.FulfillmentProviderKey = "Ancillary"` for this walking service;
- reserve through Ordering's existing provider orchestration;
- call Ancillary Hold/Confirm with ID-based fulfillment facts;
- never send `AcceptedRevenue` or selling currency to Ancillary Hold for validation.

Ancillary must not re-run shopping eligibility at Hold.

## M1 exit proof

Required automated/cross-service proof:

1. create walking Supplier/definition/provision;
2. shop it through AirAvail pre-order;
3. get Details;
4. accept it in Ordering;
5. reserve it;
6. confirm it;
7. repeat idempotent calls safely;
8. verify runtime does not resolve by `ServiceDefinitionRef + Version`;
9. verify no Ancillary FX validation exists.

Marker:

`ANCILLARY_V91_M1_WALKING_SLICE_READY`

**STOP FOR ARCHITECT AUDIT.**

---

# Milestone 2 — Marketplace routing + post-order shopping

## Goal

Prove the two decisions most likely to distort the architecture if delayed:

1. several Suppliers may offer the same ancillary type;
2. post-order shopping starts from Ordering and calls AirAvail one-way.

## M2.1 Typed Supplier routing

Complete Supplier fulfillment routing:

```text
SupplierFulfillmentKind
  Local
  External
```

Rules:

```text
Local
  -> local Ancillary fulfillment capability

External
  -> FulfillmentProviderKey required
  -> key resolves a registered Ancillary supplier adapter
```

Never:

```text
switch (SupplierId)
if SupplierId == ...
SupplierId -> hard-coded endpoint
```

`Supplier.FulfillmentProviderKey` contains no URL/credential/secret.

Implement the provider/adapter abstraction only for the already-frozen operations:

```text
Reserve/Hold
Read/Validate
Confirm
Release
CancelConfirmed
```

Do **not** add a Supplier `Issuance`/`Activation` operation.

A fake/test external adapter may be used for conformance. No invented production third-party protocol is required until a real Supplier exists. The adapter must honor the frozen Hold/Confirm/Read/Validate/Release/Cancel semantics; do not invent extra lifecycle capability flags merely for hypothetical suppliers.

## M2.2 Multiple suppliers

Prove that two Suppliers can publish equivalent broad services/subcodes independently and appear as distinct eligible offers when applicable.

Supplier identity is part of accepted provenance; behavior is selected by typed routing fields, not ID.

## M2.3 Post-order flow

Implement the frozen one-way flow:

```text
Surface
-> Ordering
   -> authorize/read Order
   -> build authoritative AncillaryShoppingContext
   -> AirAvail AncillaryOffers / Details
<- Ordering maps/returns result
```

Forbidden:

```text
AirAvail -> Ordering to fetch order context
```

Post-order validity binds to:

```text
OrderId + CommercialVersion
```

Ordering sends trusted context to AirAvail. AirAvail does not reconstruct it from untrusted surface facts.

## M2 exit proof

- two Supplier identities for the same broad service;
- Local routing works;
- External routing resolves by provider key using a test adapter;
- unknown provider key fails deterministically;
- no SupplierId switch exists;
- post-order offers are produced from Ordering-supplied context;
- stale CommercialVersion is rejected/re-shopped as defined;
- no reverse AirAvail -> Ordering context call exists.

Marker:

`ANCILLARY_V91_M2_MARKETPLACE_POSTORDER_READY`

**STOP FOR ARCHITECT AUDIT.**

---

# Milestone 3 — Commercial rules + complete Backoffice authoring

## Goal

After the end-to-end path is proven, complete the horizontal commercial rule system and practical analyst tooling.

## M3.1 Industry subcode reference

Implement `IndustryServiceSubCodeReference` as read-only reference data, not an Aggregate Root.

- Industry codes must resolve to approved semantics.
- CarrierDefined codes follow v9.1 validation.
- donor `0CC`/`0BX` entries are fixtures, not a full production dataset.
- if a licensed production-complete reference dataset is absent, implement the contract/storage/loading mechanism and report the data-provisioning gap; do not invent codes.

## M3.2 CORE commercial criteria

Implement the CORE fields from Domain Master:

```text
PassengerTypeCodes
Sales / PointOfSale references
route pairs / origin / destination / via
travel dates / DOW / time
marketing / operating carrier
FlightNumbers / FlightIds / AircraftIds
AirFareIds / AirFareTypes
FareFamilyIds
FareBasisCodes
CabinClassIds
RbdIds
AdvancePurchase where frozen
QuantityRule
Standard/Baggage/Seat application shapes as required by their later consuming milestones
FixedAmount fee and supported exact application units
Settlement / availability / document / booking metadata
```

Do not add dead Age/Occurrence/FF/keyword/account/tour/tariff/rule fields.

## M3.3 Cross-service canonical context

Complete the source-backed fields required by rules:

- J2/equivalent protected Offer context, including `InfantsWithSeat` and `AllowedPointOfSaleIds`;
- `FareFamilyId` propagation AirPrice -> AirAvail -> Ordering snapshot -> post-order context;
- exact flight/aircraft/cabin/RBD/fare facts used by the evaluator.

## M3.4 Complete Backoffice authoring

Complete v9.1 contracts in `03`:

- Supplier CRUD/lifecycle;
- ServiceDefinition CRUD/lifecycle/revise;
- Provision CRUD/lifecycle/revise;
- Industry subcode lookup;
- required reference selectors from AirInfo/AirPrice/AeroCore;
- PricingMatrix Preview;
- Bulk draft creation;
- atomic Publish;
- production-behavior simulation through **AirAvail `Backoffice/v1/AncillaryOffers`**, not an Ancillary evaluator.

Reference endpoints are implemented when required by the authoring fields above; they are not an M1 prerequisite.

## M3.5 Single evaluator

AirAvail remains the only runtime rule evaluator.

Ancillary Preview may perform static validation such as:

```text
reference existence
shape validation
overlap/shadow diagnostics
date/quantity/money consistency
seat selector structural validation where relevant
```

It must not duplicate runtime eligibility evaluation.

## M3 exit proof

- scenario coverage for all CORE commercial criteria currently reachable by authoritative contexts;
- no duplicate evaluator;
- Preview/Bulk/Publish are practical and atomic where specified;
- no dead future qualifier fields;
- FareFamily matching uses ID, not display text;
- Backoffice simulation and customer shopping use the same AirAvail evaluator.

Marker:

`ANCILLARY_V91_M3_COMMERCIAL_BACKOFFICE_READY`

**STOP FOR ARCHITECT AUDIT.**

---

# Milestone 4 — Baggage + Ancillary quota + accountable EMD

## Goal

Add the first service families whose semantics materially exceed the walking Standard service.

## M4.1 Baggage

Implement frozen baggage application semantics from Domain Master:

- excess-piece ranges;
- optional weight/weight unit where supported;
- prepaid/check-in application values;
- travel application/deference values already frozen;
- ATPCO-aligned baggage charge classification.

Do not implement per-kg/per-5kg runtime calculations until the exact input/rounding contract is available; those remain source-gated as documented.

## M4.2 AncillaryStockPool

Implement quota only for Ancillary-owned limited inventory such as a limited meal allocation.

Never use AncillaryStockPool for:

```text
physical seats
hotel rooms
cars
transfer vehicles
FlightFlow capacity
```

Use numeric `ServiceDefinitionId` relationships.

## M4.3 Ordering accountable EMD

Implement/complete EMD behavior from the Ordering side:

- EMD-A where service/document policy requires association;
- EMD-S where standalone document policy requires it;
- no-document services remain no-document;
- document numbers/stock/coupons/status/void/refund history belong to Ordering.

There is still **no generic Supplier Issuance endpoint**.

## M4 exit proof

- extra-baggage scenario;
- quota hold/confirm/release/cancel concurrency;
- EMD-A proof;
- EMD-S proof;
- no-document proof;
- Supplier cannot allocate accountable document numbers.

Marker:

`ANCILLARY_V91_M4_BAGGAGE_QUOTA_EMD_READY`

**STOP FOR ARCHITECT AUDIT.**

---

# Milestone 5 — Paid-seat authoring and runtime

## Goal

Close paid-seat retailing without violating ownership boundaries.

## M5.1 Authoring

Use:

```text
AirInfo aircraft/cabin/static DisplaySeatMap
```

for analyst seat-rule authoring and validation.

Support:

```text
SeatNumbers[]
SeatCharacteristicCodes[]
```

with all frozen validation semantics.

## M5.2 Runtime seat offers

Compose:

```text
AirAvail commercial context/evaluator
+ Ancillary seat Provision
+ FlightFlow live physical seat state
= priced SeatOffers
```

SeatOffers remains a separate flow from generic AncillaryOffers.

## M5.3 Fulfillment

Physical seat fulfillment remains:

```text
Ordering -> FlightFlow
```

not AncillaryReservation.

Prove stale/aircraft-swap behavior and no static-map-as-live-inventory bug.

## M5 exit proof

- exact-seat price rule;
- characteristic-based price rule;
- pre-order SeatOffers;
- post-order SeatOffers;
- unavailable/blocked seat handling;
- aircraft swap/stale offer handling;
- Ordering/FlightFlow seat fulfillment.

Marker:

`ANCILLARY_V91_M5_PAID_SEAT_READY`

**STOP FOR ARCHITECT AUDIT.**

---

# Milestone 6 — Closure, deletion audit and conformance

## Goal

Close the program only after the vertical slices have already been audited independently.

## M6.1 Full scenario catalog

Every `CORE` row in `06-FINAL-Scenario-Conformance-v9.1.md` must map to an automated test or explicit cross-service contract/conformance test.

## M6.2 Concurrency/idempotency/recovery

Verify:

- hold idempotency;
- concurrent stock operations;
- confirm replay safety according to provider capability;
- release/cancel replay;
- stale offer/order commercial version behavior;
- provider evidence persistence.

## M6.3 Negative/deletion audit

Prove absence of obsolete/forbidden concepts:

```text
AncillaryProduct as target commercial root
AncillaryPriceRule as competing model
mutable ServiceSubCode aggregate
runtime ServiceDefinitionRef+Version relationships
SupplierId behavior switches
Ancillary FX validation of sold price
AirAvail -> Ordering order-context fetch
Ancillary runtime evaluator
Age/Occurrence/FF dead fields
Supplier generic Issuance/Activation endpoint
Supplier document authority
seat inventory in Ancillary
```

## M6.4 Final source audit

Record:

- exact commits;
- migrations;
- cross-repository changes;
- test commands/results;
- scenario mapping;
- remaining FUTURE/source gaps only.

Final marker, and only after all required proofs pass:

`ANCILLARY_V91_IMPLEMENTED_READY_FOR_ARCHITECT_AUDIT`

---

# 8. Milestone report contract

At every milestone stop, the coding agent must report:

```text
Milestone
Repository + commit SHA(s)
Files changed
Migrations
Contracts/endpoints changed
Tests added/changed
Exact commands run
Test results
Scenario IDs covered
Known gaps/conflicts
Deletion/negative checks relevant to this milestone
Exit marker
```

A prose claim such as "implemented" is not closure evidence.

---

# 9. Gap protocol

If implementation discovers a material source conflict:

```text
REPORT_GAP_AND_STOP

Repository:
Pinned/current SHA:
File(s):
Current source behavior:
v9.1 requirement:
Why both cannot be satisfied safely:
Smallest decision options:
Recommended option:
```

Do not invent a fourth model, duplicate evaluator, hidden compatibility bridge, or generic future capability to escape the conflict.
