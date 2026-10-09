# Phase-1 Aggregate and Entity Catalog - OWNER APPROVAL REQUIRED

## DDD topology (EXACT)
```
Supplier                             [existing supporting aggregate; unchanged]
  AncillaryServiceDefinition         [commercial aggregate root 1]
    AncillaryProvision               [commercial aggregate root 2; 1:N via ServiceDefinitionId]
      ProvisionPassengerEligibilityRule        [0..1 Entity]
        ProvisionPassengerType                 [0..N Entity]
        ProvisionEligibleAgeBand               [0..N Entity]
      ProvisionSalesRestrictionsRule           [0..1 Entity]
        ProvisionPointOfSale                   [0..N Entity]
        ProvisionCustomer                      [0..N Entity]
        ProvisionCustomerType                  [0..N Entity]
      ProvisionGeographyRule                   [0..1 Entity]
        ProvisionOriginAirport                 [0..N Entity]
        ProvisionDestinationAirport            [0..N Entity]
        ProvisionViaAirport                    [0..N Entity]
        ProvisionRoutePair                     [0..N Entity]
        ProvisionServiceLocation               [0..N Entity]
        ProvisionCoverageCountry               [0..N Entity]
      ProvisionFlightApplicationRule           [0..1 Entity]
        ProvisionMarketingAirline              [0..N Entity]
        ProvisionOperatingAirline              [0..N Entity]
        ProvisionFlightNumber                  [0..N Entity]
        ProvisionFlight                        [0..N Entity]
        ProvisionAircraft                      [0..N Entity]
      ProvisionFareApplicationRule             [0..1 Entity]
        ProvisionAirFare                       [0..N Entity]
        ProvisionAirFareType                   [0..N Entity]
        ProvisionFareFamily                    [0..N Entity]
        ProvisionFareBasis                     [0..N Entity]
        ProvisionCabinClass                    [0..N Entity]
        ProvisionRbd                           [0..N Entity]
      ProvisionTravelDateRule                  [0..1 Entity]
        ProvisionPermittedTravelPeriod         [0..N Entity]
        ProvisionBlackoutPeriod                [0..N Entity]
      ProvisionDayTimeApplicationRule          [0..1 Entity]
        ProvisionDayTimeWindow                 [0..N Entity]
      ProvisionAdvancePurchaseRule             [0..1 Entity]
      ProvisionBaggageApplicationRule          [0..1 Entity, only baggage]
      ProvisionSeatApplicationRule             [0..1 Entity, only seat]
        ProvisionSeatNumber                    [0..N Entity]
        ProvisionSeatCharacteristic            [0..N Entity]
      (Owned Value Objects: QuantityRule, CommercialOutcome, SettlementPolicy,
       CoverageScope and retained legacy Fulfillment/Availability metadata)
      AncillaryPricing                 [commercial aggregate root 3; 0..N via ProvisionId]
        AncillaryPricingLine           [1..N Entity; one base per selector]
```
Aggregate roots reference other aggregate roots by ID only. Rule entities and their child rows may NOT have their own repositories or autonomous lifecycles. A rule group may be unmaterialized if all its criteria are absent. The existing `Supplier` is not counted among the three commercial design roots; the existing `AncillaryReservation` remains frozen until Phase 3.

## AR1: AncillaryServiceDefinition (source-preserving + two explicit business invariants)
| Field | Type/contract | Notes |
|---|---|---|
| Id | long | >0; generated platform ID |
| OwnerAirlineId | int | >0 |
| SupplierId | long | active supplier required for publication |
| ServiceDefinitionRef | string(30) | immutable identity within OwnerAirline |
| Version | int | >=1, monotonic |
| ServiceTypeCode | string(1) | existing canonical ATPCO service type mapping or carrier-defined policy |
| ServiceSubCode | string(3) | preserve industry reference validation |
| SubCodeSource | Industry / CarrierDefined | existing |
| GroupCode, SubGroupCode | string, string? | existing |
| Description1Code, Description2Code | string? | existing |
| CommercialName, Description | string(100), string(500)? | existing |
| PricingUnit | PricingUnit (required) | immutable across published versions of ServiceDefinitionRef |
| ServiceDateBasis | enum (required) | `FlightDeparture=1`, `ServiceStart=2`, `CheckIn=3`, `CoverageStart=4`, `Activation=5` |
| Document | DocumentDefinition VO | Type, RFIC/RFISC; no EMD execution |
| Booking | BookingDefinition VO | Method, SSR, SSIM |
| SalesEffectiveFrom, SalesDiscontinueOn | DateOnly? | service-level sales availability |
| Status and timestamps | Draft/Active/Suspended/Retired + timestamps | existing |

`ServiceDateBasis` is the authoritative meaning of date/time criteria: flight ancillaries -> FlightDeparture; hotel -> CheckIn; shared/private transfer and lounge occurrence -> ServiceStart (unless tied to FlightDeparture by explicit business definition); insurance -> CoverageStart; SIM/eSIM -> Activation. It is FIXED for an identity once published. If a product has genuinely different temporal or charging basis, create a new ServiceDefinitionRef. Prevent date-sensitive publication if its source lacks a resolved occurrence date basis. This is a locally proposed field, NOT an actual ATPCO S5 field.

Behaviors: DefineDraft, EditDraft, Activate, Suspend, Reactivate, Retire, Revise; require consistency of ServiceTypeCode/SubCode/reference, owner/supplier status, PricingUnit and ServiceDateBasis across versions. No implied automatic classification -> PricingUnit mapping; make the selected value explicit and validate against an **approved** service-family compatibility matrix (doc 04); do not invent family mapping from unrelated ATPCO sub-code prefix.

## AR2: AncillaryProvision (root, no own money)
| Field | Type | Constraint |
|---|---|---|
| Id / ServiceDefinitionId | long/long | positive, immutable parent identity |
| Sequence | int | >0; active uniqueness (ServiceDefinitionId, Sequence) |
| Status | Draft/Active/Suspended/Retired | frozen after initial publication |
| CoverageScope | existing typed ServiceCoverageScope | preserve source numeric values |
| Quantity | QuantityRule VO | Unit, MinQuantity, MaxQuantity |
| ApplicationType | Standard/Baggage/Seat | with corresponding specialized rule entity only |
| Outcome | CommercialOutcome VO | Paid/Free/NotAvailable; DocumentRequired/BookingRequired |
| Settlement | SettlementPolicy VO | ReissueRefund, FormOfRefund?, Commissionable, InterlineSettlement |
| Availability | existing AvailabilityDefinition VO | legacy declaration, no Phase-1 availability check |
| Fulfillment | existing FulfillmentDefinition VO | legacy immutable operational metadata until Phase-3 ownership decision |
| CreatedAt/ActivatedAt/SuspendedAt/RetiredAt | timestamps | immutable history semantics |
| typed rule children | 10 optional rule Entities | see below |

REMOVE public authoring representation of redundant v12 `TravelDates`, `SeasonalPeriods`, flat time criteria and directly owned ~25 peer collections. REMOVE duplicate SaleEffective/DiscontinueAt from root **domain authoring**: move into SalesRestrictionsRule. Archived old DB columns may remain read-only until migration verified. DO NOT remove valid root Quantity/Outcome/Settlement/CoverageScope as these are distinct business facts. No `Fee`/`PriceLines` in this AR.

### 10 optional rule group Entities
All group Entities have `Id:long`, `AncillaryProvisionId:long`, plus listed data. Max one of each typed group per Provision, with unique FK. They have no public constructor, separate repo or lifecycle; all modifications go through Draft root. Numeric IDs reference existing canonical masters.
| Rule Entity | Own fields | Purpose |
|---|---|---|
| ProvisionPassengerEligibilityRule | none; child lists | Passenger type and age qualification, not price |
| ProvisionSalesRestrictionsRule | SalesEffectiveFrom:DateTimeOffset?, SalesDiscontinueAt:DateTimeOffset? | Sale time window and allowed buyers/channels |
| ProvisionGeographyRule | none; child lists | travel route and nonflight service location/coverage |
| ProvisionFlightApplicationRule | none; child lists | marketing/operating carrier, flight/equipment |
| ProvisionFareApplicationRule | none; child lists | fare identity/class, cabin, RBD |
| ProvisionTravelDateRule | none; child lists | allowed date intervals and blackout intervals |
| ProvisionDayTimeApplicationRule | none; child lists | weekly hours with explicit Allow/Deny |
| ProvisionAdvancePurchaseRule | MinimumPeriod:int, Unit:AirPrice.TimeUnit, SameTimeAsTicketed:bool | minimum lead time to occurrence; ticket affinity if applicable |
| ProvisionBaggageApplicationRule | FreePieces:int?, FirstExcessPiece:int?, LastExcessPiece:int?, Weight:decimal(9,2)?, WeightUnit:AirPrice.WeightUnit, TravelApplication:BaggageTravelApplication?, PurchaseApplication:BaggagePurchaseApplication, RuleDeference:BaggageRuleDeference? | same source-verified typed baggage policy |
| ProvisionSeatApplicationRule | none; child lists | eligible seat numbers/characteristics, NOT actual occupancy |

### 27 typed criterion / period / selector child Entities
Each has Id:long and required FK to its owning **rule entity** (not an autonomous global condition table). All child rows are stored in relational tables, typed columns, unique per parent where applicable, and maintain stable IDs during draft editing. All are read-only once their Provision is published.
| Parent | Child Entity | Additional fields |
|---|---|---|
| Passenger | ProvisionPassengerType | PassengerTypeCode:AirPrice.PassengerTypeCode |
| Passenger | ProvisionEligibleAgeBand | AgeFromInclusive:int (>=0), AgeToExclusive:int? (>from), sorted disjoint half-open |
| Sales | ProvisionPointOfSale | PointOfSaleId:long |
| Sales | ProvisionCustomer | CustomerId:long |
| Sales | ProvisionCustomerType | CustomerType:Core.CustomerType |
| Geography | ProvisionOriginAirport | AirportId:int |
| Geography | ProvisionDestinationAirport | AirportId:int |
| Geography | ProvisionViaAirport | AirportId:int |
| Geography | ProvisionRoutePair | OriginAirportId:int, DestinationAirportId:int, Direction:RoutePairDirection |
| Geography | ProvisionServiceLocation | LocationType:Airport/City/Country, LocationId:int (canonical reference domain) |
| Geography | ProvisionCoverageCountry | CountryId:int (e.g. eSIM/insurance geographic coverage) |
| Flight | ProvisionMarketingAirline | AirlineId:int |
| Flight | ProvisionOperatingAirline | AirlineId:int |
| Flight | ProvisionFlightNumber | FlightNumber:string(16), normalized carrier-specific flight code |
| Flight | ProvisionFlight | FlightId:long |
| Flight | ProvisionAircraft | AircraftId:int |
| Fare | ProvisionAirFare | AirFareId:long |
| Fare | ProvisionAirFareType | AirFareType:AirPrice.AirFareType |
| Fare | ProvisionFareFamily | FareFamilyId:long |
| Fare | ProvisionFareBasis | FareBasisCode:string(64), validated against actual canonical source length |
| Fare | ProvisionCabinClass | CabinClassId:int |
| Fare | ProvisionRbd | RbdId:long |
| Travel Date | ProvisionPermittedTravelPeriod | StartDate:DateOnly, EndDate:DateOnly, inclusive; 1-day period when equal |
| Travel Date | ProvisionBlackoutPeriod | StartDate:DateOnly, EndDate:DateOnly, inclusive; denies even if permitted |
| Day/Time | ProvisionDayTimeWindow | DaysOfWeekMask:byte (7-day bitmask 1..127), StartLocalTime:TimeOnly?, EndLocalTime:TimeOnly?, Effect:Allow(1)/Deny(2) |
| Seat | ProvisionSeatNumber | SeatNumber:string(16) |
| Seat | ProvisionSeatCharacteristic | CharacteristicCode:string(25), canonical PADIS when Industry-defined |

There are exactly 27 explicitly enumerated typed row entities (2+3+6+5+6+2+1+2 = 27). Every row has parent FK, child unique constraints and specific Add/Change/Remove methods on the aggregate for frequently edited rows. Rare selectors may be replaced as a typed Draft collection via a single coherent domain command rather than hundreds of public wrappers.

## AR3: AncillaryPricing (retain v12 root and line)
| Field | Type | Constraint |
|---|---|---|
| Id | long | positive |
| AncillaryProvisionId | long | immutable FK to AR2 |
| Version | int | >=1 unique per Provision |
| PricingUnit | PricingUnit | copied from owning AR1 and validated (snapshot, never user-selectable) |
| CurrencyId | int | >0; all lines same currency |
| FeeApplicationUnit | existing typed FeeApplicationUnit? | legacy/travel charge scope, separate from PricingUnit |
| Status | Draft/Active/Suspended/Retired | published immutable |
| CreatedAt, ActivatedAt?, SuspendedAt?, RetiredAt? | DateTimeOffset | version history |

`AncillaryPricingLine` (entity) fields: `Id:long`, `AncillaryPricingId:long`, `PassengerTypeCode:AirPrice.PassengerTypeCode?`, `AgeFromInclusive:int?`, `AgeToExclusive:int?`, `Category:AncillaryPriceLineCategory(Ancillary=1/Tax=2/Fee=3)`, `Code:string(10)?`, `Name:string(100)?`, `CountryId:int?`, `StationAirportId:int?`, `Amount:decimal(18,2)`. Do not add line-level Status. ONE base line per selector key, no overlapping/ambiguous selector keys; tax/fee components refer to same selector; no cross-selector sum. Do not allow zero-priced Paid base. Free or unavailable is an Outcome, not an implicit zero-Paid rate.

## Future aggregates for visibility ONLY (not Phase-1 implementation authority)
**Phase 2**: `AncillaryStockPool` aggregate + provisional children `AncillaryCapacityBucket`, `AncillaryStockAdjustment` and `StockScope` VO; only after verifying supplier/FlightFlow inventory boundaries. No capacity booking/reservation in Phase 2.
**Phase 3**: existing `AncillaryReservation` aggregate + `AncillaryReservationUnit`; potential `AncillaryReservationAttempt` and provider references only if backed by Ordering/FlightFlow/provider contracts. Implements Hold/Release/Confirm/Cancel, expiry, partial and idempotency. Do not silently add a new Reservation aggregate or mutate it earlier.
