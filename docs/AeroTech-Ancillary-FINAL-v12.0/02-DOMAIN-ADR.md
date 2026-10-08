# 02 — Domain ADR: three commercial aggregates (Phase 1)

## ADR-12-001 — Topology
```
(existing) Supplier [frozen supporting aggregate]
  1:N AncillaryServiceDefinition [AR #1: identity, classification, product charging unit]
        1:N AncillaryProvision [AR #2: conditions/eligibility, outcome, priority]
              1:N AncillaryPricing [AR #3: filed prices, independent version lifecycle]
                    1:N AncillaryPricingLine [entity: rate selection + monetary component]
```
Each aggregate root has own repository and persistence transaction. Cross-aggregate reference by ID, not by mutable object graph or EF-owned cascading entities. Referential publication invariants involving 2 aggregates are enforced in Application transaction + DB constraint/index; domain methods also check local invariants. `Supplier` stays an existing AR because it already is one in source; **do not refactor it just to meet a semantic count of three new commercial roots**.

## AR #1 `AncillaryServiceDefinition` — same source fields plus one
### Fields (source-preserving)
`Id:long`, `OwnerAirlineId:int`, `SupplierId:long`, `ServiceDefinitionRef:string(30)`, `Version:int`, `ServiceTypeCode:string`, `ServiceSubCode:string(3)`, `SubCodeSource:Industry|CarrierDefined`, `GroupCode:string`, `SubGroupCode:string?`, `Description1Code:string?`, `Description2Code:string?`, `CommercialName:string(100)`, `Description:string(500)?`, `Document:{Type,Rfic,Rfisc}`, `Booking:{Method,SsrCode,SsimCode}`, `SalesEffectiveFrom:DateOnly?`, `SalesDiscontinueOn:DateOnly?`, `Status:Draft|Active|Suspended|Retired`, timestamps. **ADD** `PricingUnit` (closed typed enum, mandatory; fixed across all versions of one commercial identity unless a genuinely different product is created).
### Behaviors
`Define`, `ChangeDraft`, `Activate(activeSupplier)`, `Suspend`, `Reactivate(activeSupplier)`, `Retire`, `Revise(newId,version)` (copies PricingUnit). `SetPricingUnit` only inside ChangeDraft before first activation, never on Active/Suspended; revision must not silently alter a PricingUnit for same `ServiceDefinitionRef`. Keep canonical industry code lookup and carrier-defined path. Verify active owner/supplier identity for all referenced children.
### Invariants
- ID > 0; OwnerAirlineId >0; SupplierId>0; reference/version uniqueness by owner airline and version.
- For `SubCodeSource=Industry`, entry exists in authoritative currently bundled reference; never guess subcode. For `CarrierDefined`, typed code and classification validated as in S1.
- PricingUnit required, one fixed value for one service identity; no mixed PerVehicle and PerPassenger pricing among associated Provisions.
- Draft mutable; Active immutable; Retired terminal; activation requires active Supplier.

## AR #2 `AncillaryProvision` — commercial eligibility, NOT amount
### Root fields
`Id:long`, `ServiceDefinitionId:long` mandatory, `Sequence:int>0`, `Status:Draft|Active|Suspended|Retired`, `SalesEffectiveFrom:DateTimeOffset?`, `SalesDiscontinueAt:DateTimeOffset?`, `CoverageScope:ServiceCoverageScope`, `Quantity:{Unit,MinQuantity,MaxQuantity}`, `Application:Standard|Baggage|Seat` with current typed baggage/seat descriptors, `Outcome:{Disposition:Paid|Free|NotAvailable,DocumentRequired,BookingRequired}`, `AdvancePurchase:{Period,TimeUnit}?`, `Settlement:{ReissueRefund,FormOfRefund,Commissionable,InterlineSettlement}`, `Availability:{MustCheckAvailability}`, `Fulfillment:{FulfillmentProviderKey}` **legacy frozen**, created/activated/suspended/retired timestamps and the typed children enumerated in document 03.
### Behaviors
`DefineDraft`, `ReplaceDraftConditions` (full-graph replacement preserving identities where updated by ID; duplicate-safe), `Add/Remove/Update<Dimension>Row` inside Draft only (the `<Dimension>` denotes explicit typed methods, NOT generic reflective API), `Activate` (application checks parent definition active and required Active Pricing for Paid), `Suspend`, `Reactivate` (same active-price guard), `Retire`. For changing Active conditions, new draft Provision with new ID+priority; never mutate rules already possibly referenced by a booking.
### Rules
- Different populated dimensions combine with **AND**. Multiple values within one dimension combine with **OR**. Excluded dates/times take precedence over allowed ones. Empty dimension means unrestricted.
- For dates: `TravelAllowedWindow` OR (positive), `SeasonalPeriod` OR (positive); when BOTH positive collections exist, require both separately (AND) — avoid hidden union. Blackouts always override. See document 03 for precise composition.
- `Sequence ASC` is later match precedence; unique `(ServiceDefinitionId,Sequence)` for Active rows, with filtered DB index. No evaluator in Phase 1.
- Preserve Baggage and Seat typed application invariants from S1; no live seat occupancy or booking semantics here.
- Paid means active published price is required at Provision activation; Free/NotAvailable have zero active paid Pricing. Creating Draft paid Provision with no price is allowed.

## AR #3 `AncillaryPricing` — price catalog/version
### Fields
`Id:long`, `AncillaryProvisionId:long`, `PricingUnit:enum` **derived from parent ServiceDefinition at creation and validated; never independently configurable**, `CurrencyId:int>0`, `Status:Draft|Active|Suspended|Retired`, `Version:int>=1` monotonic within Provision, `CreatedAt`, `ActivatedAt?`, `SuspendedAt?`, `RetiredAt?`, `PriceLines:IReadOnlyCollection<AncillaryPricingLine>`.
### Behaviors
`DefineDraft`, `ReplaceDraftLines`, `Activate`, `Suspend`, `Reactivate`, `Retire`, `Revise`(new ID/new version), `SwitchActivePricing(provisionId,newPricingId,expectedOldPricingId?)` **Application transactional use case**: deactivates old published version and activates new in one DB transaction. A root must not update other aggregate roots directly.
### Invariants
- Exactly one parent Provision; same pricing unit as owning definition; immutable after activation.
- Unique `(AncillaryProvisionId,Version)` and filtered unique `(AncillaryProvisionId) WHERE Status=Active`.
- Each published Pricing has exactly one positive `Ancillary` base component for every distinct non-overlapping selector band; zero or more tax/fee components attached by SAME selector identity (PTC, age range); currency uniform, no FX.
- Status lifecycle as above; reactivate only if no other Active. Atomic switch required when parent Paid Provision is Active, to prevent a price gap.
- No pricing creation/activation for `NotAvailable` or `Free` Provision; Draft pricing can exist only for future Paid Draft Provision with legal outcome.

## Error behavior / cross-aggregate consistency
- Domain throws existing `ExceptionFactory` style with new stable error codes in Ancillary allocation 16000–16999. Do not leak EF exceptions; map duplicate filtered-index race to domain conflict.
- All commands validate ParentId, owner airline, supplier and status. Avoid cross-aggregate FK navigation; validate through repositories under same write UnitOfWork; SQL unique constraints backstop races.
- Concurrency tokens: rowversion or repository-consistent optimistic strategy on Pricing and Provision; choose only from actual framework pattern. In particular, use atomic guarded swap and DB unique constraint rather than a naive read-then-write.
- Stage 1 publishes data only. No matcher/evaluator, flight API call, provider adapter or AirAvail contract implementation.

## DDD cost control
Typed child entities do not become aggregate roots or repositories. NO rules-expression engine, no multipurpose `ConditionType/Operator/Value` EAV, no dynamic JSON schema, no per-ancillary family aggregates. `PricingUnit` enum is a **minimal closed set** proven in doc 04. Keep existing domain naming/API patterns where they work.
