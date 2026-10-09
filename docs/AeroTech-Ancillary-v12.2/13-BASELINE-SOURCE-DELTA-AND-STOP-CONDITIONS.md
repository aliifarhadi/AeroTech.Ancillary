# 13 — Code-to-v12.2 delta manifest (do not recode what already exists)

**Inspected branch** `feat/ancillary-v12.1-phase2-stock`, SHA `e23ba293b2578362005840967c3c07aa77dbd174`, 2026-10-09. Source repo `aliifarhadi/AeroTech.Ancillary`. The source has real `src/`, `tests/`, `Contracts/`, `samples/`, `reports/`; no other repository is part of this change. If HEAD differs, re-run path/line audit. Source exact paths below are canonical as reviewed; Source/Action means proposed v12.2 delta, not claim code already changed.

| Existing source / observed fact | Keep | v12.2 exact necessary delta |
|---|---|---|
| `src/AeroTech.Ancillary.Domain/AncillaryServiceDefinitionAggregate/AncillaryServiceDefinition.cs` — one AR with codes, CommercialName, Description, Booking/Document, PricingUnit, ServiceDateBasis, lifecycle | all header/version/lifecycle | `Profile` + `VariantCode`, one matching typed owned spec, validators and revision copying |
| `src/AeroTech.Ancillary.Domain/AncillaryProvisionAggregate/AncillaryProvision.cs` — ten optional Rule Entities and shared VO | all ten groups, no AR split | typed per-family rules selectively, `PriceOrigin`, positive accepted quantity vs UI display 0, exact POS and selection requirement |
| `Contracts/.../Enums/ProvisionApplicationType.cs` Standard=1,Baggage=2,Seat=3 | preserve ordinals | do not demand that 9 profile enum replace this existing compatibility enum; protect matching in publish validator |
| `Contracts/.../Enums/ServiceCoverageScope.cs` Sector=1,Portion=2,Journey=3,Order=4 | preserve | **Portion** is exact code; do not add contradictory `Bound=2` type. Bound reference is a contextual P3 value with coverage mapping to Portion |
| `Contracts/.../Enums/PurchaseStage.cs` PreOrder=1,PostTicketed=2,Both=3,LegacyUnspecified=4 | preserve old 1–4 | v12.2 OnBoard=5 if needed to author A24; explicit invalidity for old LegacyUnspecified during publication |
| `Contracts/.../Enums/PricingUnit.cs` PerPassenger=1,PerRoom=2,PerItem=3,PerVehicle=4,PerSeat=5,PerPiece=6,PerKilogram=7 | preserve | map per meal/pet/package to existing units (do not add unsupported PerMeal/PerPet enum values without necessity) |
| `Contracts/.../Enums/BaggageChargeKind.cs` ExtraPiece=1,WeightPackage=2,Overweight=3,Oversize=4,SpecialEquipment=5 | preserve | A05 CabinBag is Variant not new numeric BaggageChargeKind by default; distinct Spec.Kind and compatibility |
| `src/AeroTech.Ancillary.Domain/AncillaryProvisionAggregate/ValueObjects/QuantityRule.cs` Min>=1 Max>=Min, Unit Each/Piece/Kilogram | preserve | add Bound usage limit and future selection widget 0 skip, no Min=0 hack |
| `src/AeroTech.Ancillary.Domain/AncillaryProvisionAggregate/Entities/ProvisionBaggageApplicationRule.cs` FreePieces,First/Last ExcessPiece,Weight/Unit,ChargeKind,AllowanceConcept,Travel/Purchase application | preserve | exact BaggageSpec field expansion (dimensions, tiers), avoid editable duplicate ChargeKind mismatch |
| `src/AeroTech.Ancillary.Domain/AncillaryProvisionAggregate/Entities/ProvisionSeatApplicationRule.cs` seat numbers/characteristics | preserve | validators for extra-seat safety + no local seating stock |
| `src/AeroTech.Ancillary.Domain/AncillaryPricingAggregate/{AncillaryPricing.cs,Entities/AncillaryPricingRate.cs,Entities/AncillaryPriceComponent.cs,ValueObjects/Money.cs}` | v12.1 Money/rate/component schema | PriceOrigin, tax included/excluded treatment, quote-only type with no fake filed rates |
| `src/AeroTech.Ancillary.Domain/AncillaryInventoryPolicyAggregate/AncillaryInventoryPolicy.cs` plus typed Inventory ARs | authority, typed sources, version/evidence and adjustments | Bound cumulative limit + source truth; preserve recent snapshot fixed selection/current-pointer |
| `src/AeroTech.Ancillary.Query/AncillaryInventoryPolicyAggregate/Queries/GetInventoryConfigurationSnapshot/GetInventoryConfigurationSnapshotService.cs` at previous fixed SHA checks only current version | fixed semantics | maintain when adding typed profile projections; avoid cross-version OR regression |
| `src/AeroTech.Ancillary.Domain/AncillaryReservationAggregate/AncillaryReservation.cs` shallow Held root, current unit `StockPoolId?` | freeze Phase3 implementation | **NO change**; Phase3 architectural spec only, never advertise Hold as actual allocation |
| Existing reports `V121-POSTPUSH-CONFORMANCE-AUDIT.md` and prior completion report | evidence of previous counts (232/250 and 63 HTTP reported) | count/test newly; do not repeat old report as proof current v12.2 |
| `samples/lufthansa-services-configuration.json`, `-services-by-order.json`, `-one-booking-v2-purchase-orders.json` | original research fixtures immutable | attach source-derived tests and traceability. 22 categories / 48 codes, 114 offered services in this example, 6 purchased IBAG rows. |

## Actual source-ref deficiency, not permission to touch other repositories
- `ReferenceData.Currencies.DecimalPlaces` had wrong scale in AirInfo-synchronized data for 168/170 currencies in prior audit. v12.2 must fail closed and record it; do not change `AirInfo`, ReferenceData sync, or introduce a hardcoded list.
- Supplier/FlightFlow/Facility/Flight-Count reference ports were reported unconnected under D11. They remain explicit `SourceUnavailable`; **domain-only test fixtures** may verify correct configuration and negative activation, but are not real integration success.
- `AncillaryReservation.Hold` writes Held but does not reserve actual typed capacity or request Supplier confirmation. Do not extend it here; protect against accidentally exposing as working shopping/reservation.
- `samples/` was added in commit `e23ba293`, not a Lufthansa SDK or private system source. Do not infer exact POST response, authorization, provider inventory data, storage schema or currency minor unit from partial JSON.

## Implementation stop conditions / red flags
1. Agent proposes 24 Aggregate Roots, 24 table hierarchies with TPH/TPT/TPC or `BaseAggregateRoot<Baggage,Pet>`: **STOP — violates ADR-01**.
2. Agent adds generic JSON/EAV dynamic fields/rule evaluation: STOP.
3. Agent changes one of other repositories or AirAvail SQL despite scope: STOP.
4. Agent wants to implement shopping/reservation/payment/EMD in this pass: STOP.
5. Agent marks external unconnected policy as real guaranteed inventory: STOP.
6. Agent invents wrong ISO decimal info in production as temporary fix: STOP.
7. Agent auto-repairs every old sample row from description text or drops data/tables without fresh permission: STOP.
8. Agent cannot express each A01–A24 with persisted typed spec or cannot round-trip through real Backoffice: implementation incomplete; do not close.
9. Agent tests merely instantiate Definition with `VariantCode` and leave all typed fields default/null: invalid test, not proof.
10. Agent cannot map an old enum/supplier reference correctly: record exact source evidence and fail closed; no silent guess.
