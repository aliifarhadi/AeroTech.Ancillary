# 02 — Complete domain graph, identities and lifecycle

```text
Supplier AR (current supplier catalogue)
  AncillaryServiceDefinition AR [owner + stable ServiceDefinitionRef + Version]
    one-of-nine typed Specification VO graph
    DocumentDefinition + BookingDefinition + DocumentRouting
    AncillaryProvision AR [version-scoped Definition ID]
      10 shared rule groups (typed children)
      bounded Pet/AssistedTravel/AirportService specialization
      CommercialOutcome / AvailabilityDefinition / Settlement / Fulfillment / Quantity
      AncillaryPricing AR [one Active revision per Provision]
        AncillaryPricingRate (Money + traveller/currency/age selector)
          AncillaryPriceComponent (Tax / Fee)
  AncillaryInventoryPolicy AR [owner + stable ServiceDefinitionRef; NON-SEAT only]
    Optional PassengerUsageLimit[]
    optional typed Local Count/Weight/Slot source bindings, or Supplier authority
  FlightCountInventory AR [owner + FlightId + resource id; non-seat quotas]
  FlightWeightInventory AR [owner + FlightId + resource id; non-seat quotas]
  AirportSlotInventory AR [owner + facility/time interval; non-seat quotas]

AncillaryReservation AR — EXISTS but frozen / out of scope in P1/P2.
No SeatInventory, SeatMapCache, SeatStock, SeatAvailability, FlightFlow Provider/Policy or SeatHold AR.
```

| AR | Identity / core scalar fields | Child facts | State |
|---|---|---|---|
| `Supplier` | `Id:long`, OwnerAirlineId, FulfillmentKind, FulfillmentProviderKey, Name | existing | existing lifecycle unchanged |
| `AncillaryServiceDefinition` | `Id:long`; `(OwnerAirlineId:int,ServiceDefinitionRef:string(30),Version:int)`; SupplierId, ServiceTypeCode(1), ServiceSubCode(3), SubCodeSource, GroupCode(2), SubGroupCode?, Description1Code?, Description2Code?, Profile, VariantCode(A01..A24), CommercialName(100), Description?(500), PricingUnit, ServiceDateBasis, SalesEffectiveFrom?, SalesDiscontinueOn?, status+timestamps | one of 9 spec graphs, booking, document | Draft editable; Active immutable; Revise new Draft; Suspend/Reactivate/Retire |
| `AncillaryProvision` | Id, ServiceDefinitionId, Sequence, CoverageScope, PurchaseStage, PriceOrigin, QuoteProviderKey?, ApplicationType, Quantity, Outcome, Settlement, Availability, Fulfillment, status+timestamps | all existing 10 rules, typed applicable rules, SalesRestrictions optional POS | Draft editable; Active immutable; Publish/Activate/Suspend/Revise/Retire |
| `AncillaryPricing` | Id, AncillaryProvisionId, Version, PricingUnit, Status, timestamps | Rate(s), components and Money | ≤1 Active version per Provision; immutable Active, revise data faithfully |
| `AncillaryInventoryPolicy` | Id, OwnerAirlineId, ServiceDefinitionRef, ServiceDefinitionId diagnostic current pointer, Authority (`Unlimited|Local|Supplier`), LocalPattern?, ProviderKey?, version/status/timestamps | CountConsumption?, WeightConsumption?, SlotConsumption?, PassengerUsageLimits[] | **Not constructible for Seat**; Draft→Active/Suspended/Retired |
| `FlightCountInventory` | Id, OwnerAirlineId, FlightId, ResourceId, CountUnit, TotalCapacity:int>=0, ClosedForSale, Status/Version/rowversion | append-only count adjustments | nonSeat resource; optimistic concurrency |
| `FlightWeightInventory` | Id, OwnerAirlineId, FlightId, WeightResourceId, CapacityKg:decimal(18,3)>=0, status/version | append-only weight adjustments | nonSeat; **not flight safety weight** |
| `AirportSlotInventory` | Id, OwnerAirlineId, AirportId, FacilityId, StartUtc, EndUtc, CapacityPersons>=0, status/version | append-only slot adjustments | half-open UTC intervals; no overlaps |

### Cross-root invariants
1. Exactly one matching typed Specification per Active Definition, correct Profile/Variant; no untyped JSON/EAV.
2. Stable ref can have versioned revisions; Active changes by Revise, never mutate profile/variant under same stable product identity.
3. Provision references a specific Definition version; Pricing references one Provision; **no physical-stock FK on Seat**.
4. Current nonSeat policy keyed by `(OwnerAirlineId, ServiceDefinitionRef)`, shared across legitimate Definition revisions; its diagnostic ID pointer is not a seat count.
5. `SalesRestrictions.PointsOfSale` null or empty is unrestricted among authorized POS; nonempty means filter. `Sequence` deterministic winner, no ambiguous overlapping active rule.
6. `QuantityRule` MinQuantity>=1 and accepted Max>=Min. UI displaying zero as “not selected” never means sell quantity zero.
7. `ServiceCoverageScope.Portion=2` means relevant portion/bound mapping; don't introduce `Bound` enum alias.
8. Profile-specific children of the wrong family must be rejected; related shared Rule groups must round-trip completely, including `TravelDate.BlackoutPeriods`.
9. Monetary and tax rules remain Pricing-owned; Provider fulfillment declaration is NOT physical-seat authority.
10. No new user selection records in Definition/Provision; runtime buyer selections are future P3 only.
