# 03 - Normalized AncillaryProvision children: complete Phase 1 schema

**Contract:** Every row below is a typed child Entity (not an aggregate root), with `Id:long`, `AncillaryProvisionId:long` FK, and a single value or narrow tuple. Unless marked optional, each numeric ID is >0; duplicates per parent are forbidden. Only parent aggregate can add/change/remove its children. ID generator matches current framework; private setters, backing-field collections; FK cascade for Draft children, no deletion of published history. No generic `ConditionType/Value` or serialized list columns.

## Root carried forward
`AncillaryProvision`:
`Id:long`; `ServiceDefinitionId:long`; `Sequence:int >0`; `Status:ProvisionStatus(Draft/Active/Suspended/Retired)`; `SalesEffectiveFrom:DateTimeOffset?`; `SalesDiscontinueAt:DateTimeOffset?`; `CoverageScope:ServiceCoverageScope`; `AdvancePurchase:{Period:int>0,Unit:AirPrice.TimeUnit}?`; `Quantity:{Unit:AncillaryQuantityUnit,MinQuantity:int,MaxQuantity:int}`; `Application:{Type:Standard|Baggage|Seat,Baggage?,Seat?}`; `Outcome:{Disposition:Paid|Free|NotAvailable,DocumentRequired:bool,BookingRequired:bool}`; `Settlement:{ReissueRefund,FormOfRefund?,Commissionable,InterlineSettlement}`; `Availability:{MustCheckAvailability:bool}`; `Fulfillment:{FulfillmentProviderKey:string}` existing/frozen metadata; `CreatedAt`, `ActivatedAt?`, `SuspendedAt?`, `RetiredAt?`.
**REMOVE pricing ownership** (`Fee`, `PriceLines`, `ProvisionPriceLine` once backfilled). `CurrencyId` and filed money move to `AncillaryPricing`. `FeeApplicationUnit` moves to Pricing as optional compatible legacy charge/application basis, not product PricingUnit.

## Passenger / sales - distinct child row types
| Entity | Columns after common ID/FK | Per-parent uniqueness | Semantics |
|---|---|---|---|
| `ProvisionPassengerType` | `PassengerTypeCode:AirPrice.PassengerTypeCode` | PTC | eligibility (not a price) |
| `ProvisionPointOfSale` | `PointOfSaleId:long` | POS ID | canonical AirPrice point of sale |
| `ProvisionCustomer` | `CustomerId:long` | Customer ID | canonical Core customer |
| `ProvisionCustomerType` | `CustomerType:Core.CustomerType` | enum | eligibility |

## Geographic conditions - typed child rows
| Entity | Columns | Unique |
|---|---|---|
| `ProvisionOriginAirport` | `AirportId:int` | AirportId |
| `ProvisionDestinationAirport` | `AirportId:int` | AirportId |
| `ProvisionViaAirport` | `AirportId:int` | AirportId |
| `ProvisionRoutePair` (EXISTING) | `OriginAirportId:int`, `DestinationAirportId:int`, `Direction:RoutePairDirection` | normalized (Origin,Destination,Direction); reject same airport; if both-direction compare inverse for duplicates |

## Airline / flight / aircraft
| Entity | Columns | Unique |
|---|---|---|
| `ProvisionMarketingAirline` | `AirlineId:int` | AirlineId |
| `ProvisionOperatingAirline` | `AirlineId:int` | AirlineId |
| `ProvisionFlightNumber` | `FlightNumber:string(16)` (trim/upper/no spaces) | FlightNumber |
| `ProvisionFlight` | `FlightId:long` | FlightId |
| `ProvisionAircraft` | `AircraftId:int` | AircraftId |

## Fare conditions
| Entity | Columns | Unique |
|---|---|---|
| `ProvisionAirFare` | `AirFareId:long` | AirFareId |
| `ProvisionAirFareType` | `AirFareType:AirPrice.AirFareType` | enum |
| `ProvisionFareFamily` | `FareFamilyId:long` | FareFamilyId |
| `ProvisionFareBasis` | `FareBasisCode:string(10)` (verify current max against contracts/source) | normalized FareBasisCode |
| `ProvisionCabinClass` | `CabinClassId:int` | CabinClassId |
| `ProvisionRbd` | `RbdId:long` | RbdId |

## Travel restrictions - exact new children (critical)
| Entity | Columns | Conditions/invariants |
|---|---|---|
| `ProvisionTravelDate` | `TravelDate:DateOnly` | 1 row per permitted **individual** travel date; up to 1000+ rows, unique date, no CSV/JSON. This is the explicit exact-date whitelist |
| `ProvisionSeasonalPeriod` | `StartDate:DateOnly`, `EndDate:DateOnly` | multiple allowed date windows, endpoints inclusive, Start<=End; duplicates refused; overlap of positive windows can be coalesced by Backoffice but must not alter published identity |
| `ProvisionBlackoutPeriod` | `StartDate:DateOnly`, `EndDate:DateOnly` | multiple denied date windows, endpoints inclusive, Start<=End; single date => equal bounds; takes precedence over allowed dates |
| `ProvisionDayTimeRestriction` | `DayOfWeek:DayOfWeek`, `StartTime:TimeOnly?`, `EndTime:TimeOnly?`, `Effect:Allow|Deny` | one weekday/time rule per row; either BOTH times null (whole day) OR BOTH set and StartTime<=EndTime; no implicit across-midnight (split explicitly across days); no duplicate identical rows; deny overrides allow |

**Time-zone:** The rows are authored in the applicable **local service/flight departure time** of the service occurrence. UTC timestamps are not treated as a local DayOfWeek/TimeOnly. Phase 1 stores only; real occurrence local-time mapping and airport-specific timezone must be verified when the shopping/evaluation phase is explicitly opened. For nonflight service without a validated service occurrence timezone, Phase 1 may author rules but later evaluation must report unsupported context rather than guess.

**Optional legacy fields:** existing `TravelFrom/TravelTo` and `TimeFrom/TimeTo/DaysOfWeek` MUST be losslessly imported into normalized rows, or retained read-only during rollout pending deterministic conversion. A global TravelFrom/TravelTo single inclusive window may be converted to one `ProvisionSeasonalPeriod`; an old `DaysOfWeek` list + `TimeFrom/TimeTo` may be expanded into one `ProvisionDayTimeRestriction(Allow)` per day, BUT distinguish no weekdays vs every weekday: if no days and times specified, preserve unrestricted weekday intent with a representation that is provably equivalent. Do not silently create a different rule. Backfill report required. If unsupported, stop migration before dropping columns.

## Application / quantity children
Current simple typed `QuantityRule`, `ProvisionApplication`, `BaggageApplication`, `SeatApplication` VOs stay owned under Provision, except **normalize any multi-valued seat-number / characteristic selectors**:
- `ProvisionSeatNumber`: `Id`, `AncillaryProvisionId`, `SeatNumber:string` normalized uppercase, per-parent unique.
- `ProvisionSeatCharacteristic`: `Id`, `AncillaryProvisionId`, `CharacteristicCode:string` normalized uppercase, per-parent unique.
- `BaggageApplication`: `FreePieces:int?`, `FirstExcessPiece:int?`, `LastExcessPiece:int?`, `Weight:decimal(9,2)?`, `WeightUnit:AirPrice.WeightUnit`, `TravelApplication:BaggageTravelApplication?`, `PurchaseApplication:BaggagePurchaseApplication`, `RuleDeference:BaggageRuleDeference?` sourced from v11. Requires coherent piece ranges and positive weight. Flight seat numbers require at least one `ProvisionAircraft`.
- `Application=Standard` -> no baggage/seat specifics; Baggage -> only Baggage; Seat -> at least seat number or characteristic. Live seat occupancy stays FlightFlow.

## Semantics of authored criteria (for future consumer, NOT evaluator in Phase 1)
- **AND** across different populated dimensions; **OR** among allowed values within the same dimension; empty = unconstrained.
- Exact-date whitelist is independent of seasonality: if both configured, an occurrence must satisfy BOTH (intersection). Multiple seasons OR with each other. Multiple exact dates OR. Days/hours: if positive Allow rows exist, at least one must allow; Deny rows always exclude. Blackout overrides all positive date authoring.
- All dates in inclusively bounded local calendar dates. No implicit annual season recurrence: file each year's ranges or repeat with explicit future owner change. No implicit negative season beyond the positive whitelist.
- `SalesEffectiveFrom/SalesDiscontinueAt` in UTC offset timestamps describe publication/sales time and are NOT travel-date restrictions; keep on root.
- `Sequence ASC`: first matching active Provision for each service, later shopping policy only; unique active (ServiceDefinitionId,Sequence) enforced in DB.
- `NotAvailable` is an explicit eligible Provision outcome that would block sale when selected by future matcher, not the same as absence of an authored matching rule.

## Required typed methods
On Draft only: `Add/Change/RemovePassengerType`, `PointOfSale`, `Customer`, `CustomerType`, `OriginAirport`, `DestinationAirport`, `ViaAirport`, `RoutePair`, `MarketingAirline`, `OperatingAirline`, `FlightNumber`, `Flight`, `Aircraft`, `AirFare`, `AirFareType`, `FareFamily`, `FareBasis`, `CabinClass`, `Rbd`, `TravelDate`, `SeasonalPeriod`, `BlackoutPeriod`, `DayTimeRestriction`, `SeatNumber`, `SeatCharacteristic`; may implement concise private helpers, not expose a generic condition public API. `ReplaceDraftConditions` can receive coherent typed input and use existing stable IDs for unmodified children or deliberately replace Draft rows; no active mutation. Root `Activate`, `Suspend`, `Reactivate`, `Retire` from doc 02.

## Persistence and read-model requirements
- Each collection is mapped as relational 1:N with child ID PK and ProvisionId FK, a supporting (ProvisionId,Value) uniqueness key/index; choose EF `HasMany`/backing-field or `OwnsMany` consistent with domain Entity identity; do not join criteria across Provisions. Individual indexes only where query/use cases justify them; use batch write/read for 1000 dates and avoid N+1.
- Query projection needs child records with **real row IDs** so an analyst can change/delete one date; do not collapse back into opaque JSON string or comma-separated column.
- One read of complete Provision detail returns all children and correct enum IDs/names. Pagination list returns summary, not 1000 child objects per row.

## Structural test matrix
Empty all optional criteria; one exact date; 1000 exact dates; disjoint seasons; overlapping ranges; same-day blackout; Allow Mon 08:00-12:00 and Deny Mon 09:00-10:00; whole-day deny; multi-weekday; missing time bound; reversed time; duplicate date and criterion; same-route inverse duplicates; PTC any and ADT; source v11 primitive list roundtrip to normalized children; 13 family fixtures; 2 concurrent Draft edits; immutable Active children.
