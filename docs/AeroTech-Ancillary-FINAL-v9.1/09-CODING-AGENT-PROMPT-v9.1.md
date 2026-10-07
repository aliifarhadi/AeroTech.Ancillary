# 09 — CODING AGENT PROMPT — AeroTech Ancillary v9.1

You are the implementation agent. You write code; you do not redesign the domain.

## 0. Authority

Read these files completely before changing code:

1. `00-FINAL-Authority-and-Source-Freeze-v9.1.md`
2. `01-FINAL-Industry-Benchmark-and-Traceability-v9.1.md`
3. `02-FINAL-Ancillary-Domain-Master-v9.1.md`
4. `03-FINAL-Backoffice-Authoring-Contracts-v9.1.md`
5. `04-FINAL-AirAvail-Retailing-and-ACL-v9.1.md`
6. `05-FINAL-Ordering-Fulfillment-Contracts-v9.1.md`
7. `06-FINAL-Scenario-Conformance-v9.1.md`
8. `07-FINAL-Implementation-Plan-v9.1.md`
9. `08-FINAL-Gap-Register-v9.1.md`

v9.1 supersedes v9.0 and all earlier Ancillary packs when they conflict.

Do not change architecture/layering conventions merely because the Pack uses conceptual terms. Existing repository structure is architecture authority.

## 1. Source pins / donor rule

Use the source pins frozen in `00`.

Ancillary implementation baseline is the pinned bootstrap commit there. The later Ancillary repository is donor/reference only for known-good patterns such as idempotency, concurrency, projections, cache refresh, reference synchronization and Supplier CRUD style.

Do not cherry-pick the rejected Product/PriceRule domain wholesale.

If actual source has moved from a frozen SHA, report the current SHA and diff-relevant conflict before relying on new behavior.

## 2. Mandatory final domain shape

Target Aggregate Roots:

```text
Supplier
AncillaryServiceDefinition
AncillaryProvision
AncillaryReservation
AncillaryStockPool
```

Do not create target roots equivalent to:

```text
AncillaryProduct
AncillaryPriceRule
ServiceSubCode
Meal
Lounge
Baggage
Seat
GenericRule
SupplierVariant
MarketplaceProduct
```

`IndustryServiceSubCodeReference` is read-only reference data, not an Aggregate Root.

## 3. Supplier — absolute v9.1 rule

Supplier is mandatory because Ancillary is a marketplace.

Required routing state:

```text
Id                       long
OwnerAirlineId           int
Name                     string
FulfillmentKind          Local | External
FulfillmentProviderKey   string?   // required for External
Status
...
```

Hard rules:

- `SupplierId` is identity only.
- Never implement `switch (supplier.Id)`, numeric-ID routing, service-name routing or subcode routing.
- `Local` uses local Ancillary fulfillment capability.
- `External` requires `FulfillmentProviderKey` and resolves a registered Ancillary-side supplier adapter.
- Unknown provider key fails deterministically.
- `FulfillmentKind` is routing classification only; do not grow it into supplier-specific lifecycle modes.
- External adapters honor the frozen Hold/Confirm/Read/Validate/Release/Cancel semantics. Add another lifecycle capability only when a concrete supplier contract proves the need.
- URL/credentials/secrets/tokens/provider protocol config are not Supplier domain fields.
- The donor one-value `SupplierDeliveryMethod.Internal` is not the target model.

Do not confuse:

```text
OrderService.FulfillmentProviderKey = "Ancillary"
```

with:

```text
Supplier.FulfillmentProviderKey
```

The first routes Ordering to the Ancillary bounded context; the second routes an External Supplier inside Ancillary.

## 4. Runtime identity — absolute

Internal accepted/fulfillment relationships use numeric IDs:

```text
ServiceDefinitionId
ProvisionId
SupplierId
OrderServiceId
```

`ServiceDefinitionRef + Version` is business/display/audit metadata only. Do not use it as a runtime foreign-key protocol in Hold, StockPool or Ordering snapshot relationships.

## 5. Single evaluator — absolute

```text
Ancillary -> authoring, invariants, publication, supplier/quota fulfillment
AirAvail  -> single runtime ancillary eligibility/pricing evaluator
```

Do not create an Ancillary runtime evaluator.

Backoffice production simulation uses AirAvail `Backoffice/v1/AncillaryOffers` and the same evaluator as real shopping.

Ancillary Preview may validate authoring structure/references/overlaps only.

## 6. Pre-order and post-order shopping

Pre-order:

```text
Surface -> AirAvail
```

Post-order:

```text
Surface -> Ordering -> AirAvail
```

Ordering builds authoritative order context and sends it to AirAvail.

Forbidden:

```text
AirAvail -> Ordering to fetch order context
```

Do not reintroduce the removed reverse context-fetch contract.

## 7. Money / Hold

AirAvail performs retail FX and returns accepted commercial evidence to Ordering.

Ordering snapshots the sale.

Ancillary Hold receives IDs and fulfillment facts only. It does not receive/validate converted `AcceptedRevenue` to prove AirAvail pricing.

Use platform monetary precision. Do not introduce `decimal(19,4)` for normal money because it looks generally precise. Preserve high precision only for ROE where the existing platform does so.

## 8. ServiceSubCode

For `Industry` source, validate against `IndustryServiceSubCodeReference`.

Do not treat donor two-row fixtures as a complete production dataset.

For `CarrierDefined`, apply the frozen validation and definition semantics. Never guess industrial meaning for an unknown industry code.

## 9. No dead future fields

Do not add inactive placeholders for:

```text
Age
Occurrence
FF status
Keyword/customer score
Ticket designator
Account/tour/tariff/rule qualifiers
ExternalQuote
AI pricing
```

unless a current milestone explicitly has an authoritative source contract for them. They remain Gap Register items.

## 10. Supplier post-confirm Issuance — forbidden in CORE

Do not implement:

```text
IssueSupplierService
POST .../Issuance
SupplierIssueReference as a special new lifecycle
no-op Supplier issuance capability
```

No current real Supplier contract proves a separate post-confirm operation.

If a concrete supplier integration later requires a distinct operation, it must be introduced from that real contract with explicit capability/tests.

This does not affect Ordering's accountable ETKT/EMD issuance authority.

## 11. Seats

Keep the frozen split:

```text
AirInfo    -> static aircraft/cabin/seat reference for authoring
FlightFlow -> live physical seat state/availability/assignment
Ancillary  -> commercial paid-seat rule
AirAvail   -> priced SeatOffers composition
Ordering   -> order/service orchestration
FlightFlow -> physical seat fulfillment
```

Do not create physical seat inventory in Ancillary.

## 12. EMD

Ordering owns:

```text
EMD document stock/number
EMD-A / EMD-S aggregate and coupons
issue/void/refund/exchange state/history
```

Ancillary provides sold service/document semantics but cannot allocate accountable documents.

## 13. Implementation method — ONE MILESTONE PER RUN

This is a hard process rule.

Read `07-FINAL-Implementation-Plan-v9.1.md` and implement only the currently authorized milestone.

Do **not** continue automatically to the next milestone after the exit marker. Stop and return the milestone report for architect audit.

Current sequence:

```text
M1 Walking Ancillary
M2 Marketplace routing + post-order
M3 Commercial rules + full Backoffice
M4 Baggage + quota + EMD
M5 Paid seat
M6 Closure/conformance
```

The architect/owner may split a milestone if source reality shows it is still too large. You may not silently defer CORE requirements or create an invented future phase to hide them.

## 14. Milestone 1 scope — keep it small

M1 must prove one real end-to-end Standard/FixedAmount ancillary:

```text
minimal authoring
-> published catalog
-> AirAvail pre-order offers/details
-> Ordering accepts service
-> Ancillary hold
-> Ancillary confirm
```

Do not pull into M1:

```text
full reference API suite
PricingMatrix Preview/Bulk/Publish
all criteria
StockPool
baggage semantics
EMD
paid seat
External supplier production integration
post-order shopping
```

The domain fields used in M1 must already follow final v9.1 identity/money/Supplier rules; M1 is not throwaway code.

## 15. Tests and conformance

Every implemented CORE scenario must have an automated test or explicit cross-service contract/conformance test.

At final closure, every CORE ID in `06` must be mapped.

Negative tests are first-class, especially:

```text
no SupplierId strategy switch
no composite runtime business key
no FX validation in Hold
no reverse AirAvail -> Ordering order fetch
no second evaluator
no mutable ServiceSubCode aggregate
no Supplier generic Issuance endpoint
no Supplier document authority
no Ancillary seat inventory
```

## 16. Report after every milestone

Return:

```text
Milestone
Repositories and commit SHA(s)
Files changed
Migrations
Contracts/endpoints changed
Tests added/changed
Exact commands run
Test results
Scenario IDs covered
Known gaps/conflicts
Negative/deletion checks
Exit marker
```

Do not claim success from a prose description alone.

## 17. Conflict protocol

If a v9.1 CORE requirement cannot be implemented safely because source contradicts it, stop with:

```text
REPORT_GAP_AND_STOP
Repository:
SHA:
File(s):
Current behavior:
v9.1 requirement:
Conflict:
Smallest options:
Recommended option:
```

Do not guess a domain answer.

## 18. Exit markers

Use exactly:

```text
ANCILLARY_V91_M1_WALKING_SLICE_READY
ANCILLARY_V91_M2_MARKETPLACE_POSTORDER_READY
ANCILLARY_V91_M3_COMMERCIAL_BACKOFFICE_READY
ANCILLARY_V91_M4_BAGGAGE_QUOTA_EMD_READY
ANCILLARY_V91_M5_PAID_SEAT_READY
ANCILLARY_V91_IMPLEMENTED_READY_FOR_ARCHITECT_AUDIT
```

The final marker is allowed only after M6 conformance/closure.
