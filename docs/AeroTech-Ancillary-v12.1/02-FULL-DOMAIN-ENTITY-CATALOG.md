# Exact target aggregate/entity/VO catalog (Phase 1+2)

## Aggregate topology (7 commercial + inventory ARs + existing Supplier)
```text
Supplier [unchanged]
AncillaryServiceDefinition [AR] --stable ServiceDefinitionRef--> AncillaryInventoryPolicy [AR]
  BookingDefinition [VO: Method, SsrCode?, SsimCode?, ConfirmationRequirement]
  DocumentDefinition [VO: Type, Rfic?, Rfisc?]
  PricingUnit, ServiceDateBasis, ATPCO-like codes and lifecycle
AncillaryProvision [AR] --ServiceDefinitionId--> AncillaryServiceDefinition
  QuantityRule [VO] / CommercialOutcome [VO] / SettlementDefinition [VO]
  AvailabilityDefinition [VO] / FulfillmentDefinition [VO]
  10 OPTIONAL TYPED RULE ENTITY GROUPS (below)
AncillaryPricing [AR] --ProvisionId--> AncillaryProvision
  AncillaryPricingRate [Entity 1..N]
    BasePrice [Money VO]
    AncillaryPriceComponent [Entity 0..N; Money VO]
AncillaryInventoryPolicy [AR; stable owner+ServiceDefinitionRef identity]
  PassengerUsageLimit [Entity]
  FlightCountConsumption / FlightWeightConsumption / AirportSlotConsumption [typed VOs]
FlightCountInventory [AR] -> FlightCountAdjustment [Entity]
FlightWeightInventory [AR] -> FlightWeightAdjustment [Entity]
AirportSlotInventory [AR] -> AirportSlotAdjustment [Entity]
AncillaryReservation [existing, FROZEN for Phase 3]
```

## AncillaryServiceDefinition AR
Fields (current and retained): `Id:long, OwnerAirlineId:int, SupplierId:long, ServiceDefinitionRef:string(30), Version:int, ServiceTypeCode:string(1), ServiceSubCode:string(3), SubCodeSource:Industry|CarrierDefined, GroupCode:string, SubGroupCode:string?, Description1Code:string?, Description2Code:string?, CommercialName:string(100), Description:string(500)?, PricingUnit:enum, ServiceDateBasis:enum, Document:DocumentDefinition VO, Booking:BookingDefinition VO, SalesEffectiveFrom:DateOnly?, SalesDiscontinueOn:DateOnly?, Status:Draft|Active|Suspended|Retired, CreatedAt/ActivatedAt?/SuspendedAt?/RetiredAt?`. **Delta**: BookingDefinition adds `ConfirmationRequirement` enum (Immediate=1/SubjectToConfirmation=2). No proposed new AR or external provider lifecycle.

## AncillaryProvision AR
`Id:long, ServiceDefinitionId:long, Sequence:int positive, Status, CoverageScope:ServiceCoverageScope, Quantity:QuantityRule(Unit:Each|Piece|Kilogram,MinQuantity:int,MaxQuantity:int), ApplicationType:Standard|Baggage|Seat, Outcome:CommercialOutcome(Paid|Free|NotAvailable,DocumentRequired,BookingRequired), Settlement, Availability(MustCheckAvailability:bool), Fulfillment(FulfillmentProviderKey), PurchaseStage:PreOrder|PostTicketed|Both|LegacyUnspecified, timestamps`. All rules below optional; all modifications via root Draft methods, published immutability preserved.

### Exactly ten typed rule-group entities and owned rows
| Rule-group Entity 0..1 | Scalar fields + child Entities |
|---|---|
| `ProvisionPassengerEligibilityRule` | `ProvisionPassengerType(PassengerTypeCode)`; `ProvisionEligibleAgeBand(AgeFromInclusive,AgeToExclusive?)` |
| `ProvisionSalesRestrictionsRule` | SalesEffectiveFrom:DateTimeOffset?, SalesDiscontinueAt:DateTimeOffset?, `ProvisionPointOfSale(PointOfSaleId:long)`, `ProvisionCustomer(CustomerId:long)`, `ProvisionCustomerType(CustomerType)` |
| `ProvisionGeographyRule` | `ProvisionOriginAirport(AirportId:int)`, `ProvisionDestinationAirport(AirportId:int)`, `ProvisionViaAirport(AirportId:int)`, `ProvisionRoutePair(OriginAirportId:int,DestinationAirportId:int,Direction)`, `ProvisionServiceLocation(LocationType,LocationId:int)`, `ProvisionCoverageCountry(CountryId:int)` |
| `ProvisionFlightApplicationRule` | `ProvisionMarketingAirline(AirlineId:int)`, `ProvisionOperatingAirline(AirlineId:int)`, `ProvisionFlightNumber(FlightNumber:string(16))`, `ProvisionFlight(FlightId:long)`, `ProvisionAircraft(AircraftId:int)` |
| `ProvisionFareApplicationRule` | `ProvisionAirFare(AirFareId:long)`, `ProvisionAirFareType(AirFareType)`, `ProvisionFareFamily(FareFamilyId:long)`, `ProvisionFareBasis(FareBasisCode:string(64))`, `ProvisionCabinClass(CabinClassId:int)`, `ProvisionRbd(RbdId:long)` |
| `ProvisionTravelDateRule` | `ProvisionPermittedTravelPeriod(StartDate:DateOnly,EndDate:DateOnly)`, `ProvisionBlackoutPeriod(StartDate,EndDate)`; intervals inclusive, Blackout overrides |
| `ProvisionDayTimeApplicationRule` | `ProvisionDayTimeWindow(DaysOfWeekMask:byte,StartLocalTime:TimeOnly?,EndLocalTime:TimeOnly?,Effect:Allow|Deny)` |
| `ProvisionAdvancePurchaseRule` | `MinimumPeriod:int >=0`, **`MaximumPeriod:int?`**, `Unit:AirPrice.TimeUnit`, `SameTimeAsTicketed:bool` |
| `ProvisionBaggageApplicationRule` | current FreePieces?,FirstExcessPiece?,LastExcessPiece?,Weight?,WeightUnit,TravelApplication?,PurchaseApplication,RuleDeference? + **ChargeKind enum, AllowanceConcept? enum** |
| `ProvisionSeatApplicationRule` | `ProvisionSeatNumber(SeatNumber:string(16))`, `ProvisionSeatCharacteristic(CharacteristicCode:string(25))` — commercial filters, NOT live seat allocation |

Every Rule Entity/row has `Id:long` and typed FK to parent (RuleGroup owns rows; Provision owns RuleGroup). Retain existing row fields/enum numeric values and constraints. No independent repositories per Rule or per ancillary product.

## Pricing AR and children — authoritative replacement of legacy flat Lines
| Type | Fields | Cardinality |
|---|---|---|
| `AncillaryPricing` AR | `Id:long, AncillaryProvisionId:long, Version:int, PricingUnit:enum (inherited immutable product unit), Status, CreatedAt, ActivatedAt?, SuspendedAt?, RetiredAt?` | one Provision -> many revisions, at most 1 Active |
| `AncillaryPricingRate` Entity | `Id:long, AncillaryPricingId:long, PassengerTypeCode?, AgeFromInclusive?, AgeToExclusive?, BasePrice:Money` | 1..N per Pricing; unique normalized (CurrencyId,PTC,AgeBand) |
| `AncillaryPriceComponent` Entity | `Id:long, RateId:long, Category:Tax|Fee, Code:string(10), Name:string(100)?, CountryId:int?, StationAirportId:int?, Amount:Money, FeeApplicationUnit:enum? (required on newly published Fee, null on Tax), TaxIncludedInSource:bool? (Tax only; informational)` | 0..N per rate; unique code/type/country/station/feeUnit within rate |
| `Money` VO | `Amount:decimal(19,6), CurrencyId:int` | same currency enforced across base/components of one Rate |

**REMOVE authoritative `CurrencyId` and `FeeApplicationUnit` from Pricing root after verified migration**, old columns retained physically during expand/verify then cleanup in contract phase. `AncillaryPricingLine` replaced by Rate/Component with exact mapping and stable historical price snapshots. No `PointOfSaleId` on Rate.

## Inventory Policy AR and children
`AncillaryInventoryPolicy`: Id:long, OwnerAirlineId:int, ServiceDefinitionRef:string(30) stable key, ServiceDefinitionId:long **non-authoritative latest validated source reference / migration compatibility only**, Authority:Unlimited|Local|Supplier|FlightFlow, LocalPattern?:FlightCount|FlightWeight|FlightCountPlusWeight|AirportSlot|DailyCount|RoomNight|AssignedAsset (last three unactivatable), ProviderKey?:string(50), CountConsumption?:VO(ResourceId:long,CountPerAcceptedUnit:int,CountUnit:enum), WeightConsumption?:VO(WeightResourceId:long,ConsumptionMode:FixedKgPerAcceptedUnit|AcceptedWeightKg,FixedKgPerUnit?:decimal(18,3)), SlotConsumption?:VO(FacilityId:long,OccupancyMinutes:int,PeoplePerAcceptedUnit:int), PassengerUsageLimits[Entity: Id,InventoryPolicyId,LimitScope:PerOrder|PerFlightOccurrence|PerServiceDate,MaxUnits:int,CountingFamilyCode:string(30)], Status:Draft|Active|Suspended|Retired, Version:long, timestamps, rowversion.`

Do not bind physical quotas to ephemeral ServiceDefinitionId; policy resolves effective current definition via owner+stable Ref, validates its current identity, and maintains version-aware diagnostic pointer if necessary. Physical sources keyed only by physical resource, NOT product, Provision, or Price.

## Supported Local ARs (actual Phase2 persistence)
- `FlightCountInventory`: Id:long, OwnerAirlineId:int, FlightId:long, ResourceId:long, CountUnit:Person|Piece|Item|AnimalCarrier|Equipment, TotalCapacity:int>=0, ClosedForSale:bool, Status, Version, audit timestamps, RowVersion; `FlightCountAdjustment` Id,ParentId,PreviousTotal,NewTotal,ExpectedVersion,ResultingVersion,ReasonCode,ActorId,CorrelationId,OccurredAt.
- `FlightWeightInventory`: Id:long, OwnerAirlineId:int, FlightId:long, WeightResourceId:long, CapacityKg:decimal(18,3)>=0, ClosedForSale, Status, Version, audit timestamps, RowVersion; `FlightWeightAdjustment` same previous/new kg structure.
- `AirportSlotInventory`: Id:long, OwnerAirlineId:int, AirportId:int, FacilityId:long, StartUtc:DateTimeOffset, EndUtc:DateTimeOffset (half-open), CapacityPersons:int>=0, ClosedForSale, Status, Version, audit timestamps, RowVersion; `AirportSlotAdjustment` same previous/new people structure.
- For all: immutable physical key, one current active/non-retired record per key, optimistic version + SQL overlap locks for slots, adjustments immutable and idempotent by (parent,CorrelationId). No Held/Sold/Available counters in Phase 2.
- `DailyCount`, `RoomNight`, `AssignedAsset`: **documented only**, not activated/registered/persisted unless future real source and Owner phase authorization. External hotel/car supplier => Supplier authority works as a policy, no mirrored local inventory.
