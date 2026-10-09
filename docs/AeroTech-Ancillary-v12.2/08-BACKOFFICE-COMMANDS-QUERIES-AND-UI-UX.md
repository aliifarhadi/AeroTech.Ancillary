# 08 — Backoffice contract: nine specialist editors, one reusable set of application services

## 1. Practical authoring UX
The airline user selects one of nine **family templates**, then variant A01–A24. Common tabs: `General`, `Product-specific`, `Eligibility & POS`, `Pricing`, `Capacity/Availability`, `Review & Publish`. For every variant the specialized tab includes **only relevant variant fields** and informative validation, not a 100-field generic form.

| Editor | Variants | Form-specific widgets | Reuse |
|---|---|---|---|
| Baggage | A01–A06 | Piece/Weight concept, kg/dim controls, bracket, equipment kind, purchase limits | Shared header/POS/fare/route/pricing |
| Seat | A07–A09 | Cabin/characteristics, exit row, extra-seat adjacency | FlightFlow delegated inventory control (configuration) |
| Upgrade | A10 | From/To cabin, reprice vs quote source, ticket exchange | shared provider reference and quote policy |
| Meal | A11/A12 | SSR meal code or menu SKU, catering cutoff, mutual exclusivity | Free or Filed pricing template |
| Pet | A13/A14 | Mode, species, max weight+dimensions, age, document checklist | Supplier/verified FlightCount |
| Assisted Travel | A15–A19 | Conditional subform: Wheelchair / Assistance / Medical / Bassinet / UMNR, never mixed | booking approval and provider |
| Airport | A20–A22 | Airport/terminal/facility/direction/service hours/guest/package | Supplier/verified Slot |
| Priority | A23 | Boarding/check-in zone and included fare | Entitlement without false physical pool |
| Connectivity | A24 | Plan, duration/data/devices/eligible aircraft/stage | Provider check only |

## 2. Commands and DTOs (preserve existing routes where possible)
Use already implemented Backoffice naming and controllers. Expose the following semantic DTOs, composed of typed nested parts. This is a **contract shape**, not a demand for gratuitous endpoint duplication.

```csharp
record ServiceDefinitionDraftInput(
    int OwnerAirlineId, long SupplierId, string StableRef,
    string ServiceTypeCode, string ServiceSubCode, string GroupCode,
    AncillaryProfile Profile, string VariantCode,
    string CommercialName, string? Description,
    PricingUnit PricingUnit, ServiceDateBasis ServiceDateBasis,
    BookingDefinitionInput Booking, DocumentDefinitionInput Document, DocumentRouting DocumentRouting,
    OneOfNineTypedSpecification Specification,
    DateOnly? SalesEffectiveFrom, DateOnly? SalesDiscontinueOn);

record ProvisionDraftInput(
    long ServiceDefinitionId, long PointOfSaleId, int Sequence,
    PurchaseStage PurchaseStage, ServiceCoverageScope CoverageScope,
    CommercialOutcomeInput Outcome, PriceOrigin PriceOrigin,
    QuantityRuleInput AcceptedQuantity,
    CommonEligibilityRulesInput SharedRules,
    TypedProfileRuleInput? FamilyRule,
    CustomerSelectionRequirementsInput SelectionRequirements,
    AvailabilityDefinitionInput Availability,
    FulfillmentDefinitionInput Fulfillment);

record PricingRateInput(
    int CurrencyId, PassengerTypeCode? Ptc, int? AgeFrom, int? AgeTo,
    MoneyInput BasePrice, IReadOnlyList<PriceComponentInput> Components);

record InventoryPolicyDraftInput(
    int OwnerAirlineId, string ServiceDefinitionRef,
    InventoryAuthority Authority, LocalInventoryPattern? Pattern,
    string? ProviderKey, FlightCountConsumptionInput? Count,
    FlightWeightConsumptionInput? Weight, AirportSlotConsumptionInput? Slot,
    IReadOnlyList<PassengerUsageLimitInput> UsageLimits);
```

`OneOfNineTypedSpecification` denotes a **closed typed DTO discriminator**: implementation may use 9 explicitly named request DTOs or a validated discriminated request union with controlled JSON `kind`; DO NOT implement a runtime arbitrary `OneOf` dependency or a `Dictionary<string,object>`. The current framework structure governs controllers/handlers. A `VariantCode` determines the only legal spec schema. Unknown member fields must be rejected where they could alter behavior, not ignored as supposed constraints.

### Endpoint semantic surface (logical, map to existing route conventions)
- `POST/PUT/GET /backoffice/v1/ancillary-service-definitions` and `/{id}` plus `Activate`, `Suspend`, `Revise`, `Retire`; typed spec included in Define and read details.
- `POST/PUT/GET /backoffice/v1/ancillary-provisions` plus `Publish`, `Suspend`, `Revise`, `Retire`, with single `PointOfSaleId`, typed family rules and selection requirements.
- `POST/PUT/GET /backoffice/v1/ancillary-pricings`, Activate, SwitchActive, with `Rates[]`, Money and `PriceOrigin` coherent; for ExternalQuote do not require a Pricing entity.
- `POST/PUT/GET /backoffice/v1/ancillary-inventory-policies`, Activate/Suspend/Retire and configuration snapshots; same for typed Count/Weight/Slot stocks/adjustments.
- Optional `GET /backoffice/v1/ancillary-authoring-schema/{variantCode}` returns **versioned allowlisted UI field descriptors** generated from fixed type schemas. It is read-only metadata, not a generic data-defining rule engine. If expensive, Backoffice frontend may use generated JSON schema from typed DTOs; server contract is still authoritative.

### Minimal create example: PETC (schematic; enum strings per API convention)
```json
{
  "stableRef":"PET_IN_CABIN", "profile":"Pet", "variantCode":"A13",
  "commercialName":"Pet in cabin", "pricingUnit":"PerItem",
  "specification": {
    "kind":"Pet", "transportMode":"Cabin", "allowedAnimalTypes":["Cat","Dog"],
    "maxCombinedWeightKg":8,
    "carrierDimensionsMaxCm":{"length":55,"width":40,"height":23},
    "minAnimalAgeWeeks":12,
    "requiredDocumentCodes":["ENTRY_RULES_ACK"]
  }
}
```
This is a **domain shape illustration**; not an assertion that the real Lufthansa product has globally the same limits. The Backoffice request must also submit verified Supplier/Booking/Document/industrial-code fields required by the actual endpoint. Numeric enum IDs must match source contract or explicit migration, not ad-hoc strings.

### Minimal Provision authoring for separate POS
```json
{
  "serviceDefinitionId":"<actual-definition-version-id>",
  "pointOfSaleId":"<verified-POS-id>",
  "sequence":10, "purchaseStage":"Both",
  "coverageScope":"Portion", "priceOrigin":"Filed",
  "outcome":"Paid", "acceptedQuantity":{"unit":"Each","min":1,"max":1},
  "availability":{"mustCheckAvailability":true}
}
```
`Portion` is the **actual existing `ServiceCoverageScope=2`**; map to a bound only if coverage semantics of that bound are explicitly supplied and validated. Do not invent a new `Bound=2` enum. `PurchaseStage` current values PreOrder=1/PostTicketed=2/Both=3/LegacyUnspecified=4; add **new** OnBoard value 5 with migration for A24, never renumber old. `PricingUnit` must use existing numeric values PerPassenger=1, PerRoom=2, PerItem=3, PerVehicle=4, PerSeat=5, PerPiece=6, PerKilogram=7.

## 3. Exact release behavior
- Saving Draft permitted with typed syntax valid; Activate/Publish requires full mandatory fields and reference checks. Domain Rule on Root controls state, not client page.
- Editing Active prohibited; `Revise` copies all shared Rules, single family spec/FamilyRule, selections, pricing versions/references subject to provenance, with new Definition version and fresh IDs while retaining stable identity.
- `GetDetails`/paged list can show correct profile-specific summary. Never require clients to supply unrelated group fields or default 0/false values that change meaning.
- Backoffice tenancy/auth from current solution; agents not authorized to bypass JWT/owner scope for smoke convenience.
- `Money` rates, `PriceOrigin`, physical State, LastValidatedVersion distinguish from current Definition and last-ref pointer.

## 4. HTTP contract tests
For all 24 variants: (1) Define expected 200/201; (2) Change Draft; (3) Get type round-trip; (4) Publish; (5) invalid wrong-variant field rejects; (6) clone Version preserves exactly one profile spec. At least 9 authenticated real HTTP smoke happy cases + 9 negative cases, plus full 24x SQL acceptance tests. Verify same request DTO serializes with real JSON long IDs and appropriate enum converters, no accidental undocumented breaking route rename.
