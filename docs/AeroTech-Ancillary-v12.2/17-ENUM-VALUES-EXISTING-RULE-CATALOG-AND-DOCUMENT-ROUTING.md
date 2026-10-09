# 17 — Explicit enums, full existing Provision rule topology and document routing

This file closes ambiguity that previously caused guessed enums or incomplete Agent-generated types. **All current numeric values below were verified against `feat/ancillary-v12.1-phase2-stock@e23ba293`**. New values are v12.2 design decisions, not represented as existing code.

## 1. Existing enums — NEVER renumber
| Enum | exact values in current source |
|---|---|
| `ServiceCoverageScope` | `Sector=1, Portion=2, Journey=3, Order=4` |
| `PricingUnit` | `PerPassenger=1, PerRoom=2, PerItem=3, PerVehicle=4, PerSeat=5, PerPiece=6, PerKilogram=7` |
| `PurchaseStage` | `PreOrder=1, PostTicketed=2, Both=3, LegacyUnspecified=4` |
| `ProvisionApplicationType` | `Standard=1, Baggage=2, Seat=3` |
| `BaggageChargeKind` | `ExtraPiece=1, WeightPackage=2, Overweight=3, Oversize=4, SpecialEquipment=5` |
| `BookingMethod` | `Ssr=1, AuxiliarySegment=2, DisplayPriceContactCarrierForBooking=3, NoBookingProcessRequired=4, PerServiceRecord=5` |
| `ConfirmationRequirement` | `Immediate=1, SubjectToConfirmation=2` |
| `AncillaryDocumentType` | `None=1, EmdAssociated=2, EmdStandalone=3` |
| `InventoryAuthority` | `Unlimited=1, Local=2, Supplier=3, FlightFlow=4` |
| `LocalInventoryPattern` | `FlightCount=1, FlightWeight=2, FlightCountPlusWeight=3, AirportSlot=4, DailyCount=5, RoomNight=6, AssignedAsset=7` |
| `InventoryCountUnit` | `Person=1, Piece=2, Item=3, AnimalCarrier=4, Equipment=5` |
| `InventoryCapacityReadState` | `NotConfigured=1, Unlimited=2, ConfiguredNotGuaranteed=3, ClosedForSale=4, DelegatedCheckRequired=5, Unknown=6, UnsupportedPattern=7` |
| `PassengerUsageLimitScope` | `PerOrder=1, PerFlightOccurrence=2, PerServiceDate=3` |
| `ServiceDateBasis` | `FlightDeparture=1, ServiceStart=2, CheckIn=3, CoverageStart=4, Activation=5` |
| `FeeApplicationUnit` | `OneWay=1, RoundTrip=2, Item=3, SectorOrPortion=4, Ticket=5, PerOneKilogramOver=6, PerFiveKilogramsOver=7, HalfPercentOfFarePerKilogram=8, OnePercentOfFarePerKilogram=9, OneAndHalfPercentOfFarePerKilogram=10` |

`FeeApplicationUnit=6..10` are **existing enum members, NOT automatically executable percentage/kilogram pricing rules**; unsupported fee operations must be rejected at activation rather than silently interpreted as Item or zero. `ServiceCoverageScope.Portion=2` is the retained industrial coverage choice for flight-bound multi-segment *portion*; this v12.2 pack does NOT introduce a new Bound enum alias with numeric conflict.

## 2. New enums — exact v12.2 values, append-only from creation
| Enum/typed code | exact values |
|---|---|
| `AncillaryProfile` | `Baggage=1, Seat=2, Upgrade=3, Meal=4, Pet=5, AssistedTravel=6, AirportService=7, Priority=8, Connectivity=9` |
| `PriceOrigin` | `Filed=1, ExternalQuote=2, Free=3, NotAvailable=4, LegacyUnspecified=5` (last for unclassified historical data only, no publication) |
| `TaxTreatment` | `AddedToBase=1, IncludedInBase=2, LegacyUnknown=3` (LegacyUnknown blocks total-complete/publication) |
| `SelectionKind` | `SimpleOptIn=1, QuantityChoice=2, TypedForm=3, SeatMapSelection=4, ExternalQuote=5` |
| `PurchaseStage` APPEND | `OnBoard=5` (old 1–4 retained) |
| `PassengerUsageLimitScope` APPEND | `PerPortion=4` (*business label "PerBound" in documentation*; map to verified existing Portion coverage, don't renumber 1–3) |
| `UsageConsumptionUnit` | `PurchasedUnit=1, Kilogram=2` (only when the counting family needs unit differentiation) |
| `DocumentRouting` | `NoAncillaryDocument=1, Emd=2, TicketOrExchange=3` (v12.2 metadata; actual issuance P3) |
| `AncillaryVariantCode` | stable explicit strings `A01`..`A24`, NOT numeric enum requiring 24 individual tables |

Variant mapping **must** be centralized and exhaustive: `A01–A06=Baggage`, `A07–A09=Seat`, `A10=Upgrade`, `A11–A12=Meal`, `A13–A14=Pet`, `A15–A19=AssistedTravel`, `A20–A22=AirportService`, `A23=Priority`, `A24=Connectivity`.

**DocumentRouting reconciliation:** Existing `AncillaryDocumentType` supports only None/EMDA/EMDS; it has no Ticket value. Do **not** claim it already supports eTicket exchange. Add lightweight `DocumentRouting` metadata at Definition level only where necessary: `Emd` requires current `Document.Type=EmdAssociated|EmdStandalone`, `TicketOrExchange` requires `Document.Type=None` in Ancillary (document authority external to Ancillary) plus verified request/eligibility metadata; `NoAncillaryDocument` requires `None`. `Outcome.DocumentRequired` must not force Ancillary to issue EMD for `TicketOrExchange`; reconcile invariant as `external document required` vs `ancillary document required`, with explicit v12.2 field `RequiresExternalTicketAction:bool` under Upgrade/ExtraSeat spec. This is only a **declaration** for P3 Ordering, not ticket issuance here.

## 3. Existing ten shared Provision rule groups — complete field/child inventory
| Rule Entity (0..1 per Provision) | Scalar fields and owned child Entity rows (typed) |
|---|---|
| `ProvisionPassengerEligibilityRule` | `ProvisionPassengerType(PassengerTypeCode)` and `ProvisionEligibleAgeBand(AgeFromInclusive:int,AgeToExclusive:int?)`; no empty list interpreted as implicit deny when published common rule semantics say unrestricted |
| `ProvisionSalesRestrictionsRule` | `SalesEffectiveFrom:DateTimeOffset?`, `SalesDiscontinueAt:DateTimeOffset?`, `ProvisionPointOfSale(PointOfSaleId:long)`, `ProvisionCustomer(CustomerId:long)`, `ProvisionCustomerType(CustomerType)`; v12.2 published new Provision exactly ONE POS |
| `ProvisionGeographyRule` | `ProvisionOriginAirport(AirportId:int)`, `ProvisionDestinationAirport(AirportId:int)`, `ProvisionViaAirport(AirportId:int)`, `ProvisionRoutePair(OriginAirportId:int,DestinationAirportId:int,Direction)`, `ProvisionServiceLocation(LocationType,LocationId:int)`, `ProvisionCoverageCountry(CountryId:int)` |
| `ProvisionFlightApplicationRule` | `ProvisionMarketingAirline(AirlineId:int)`, `ProvisionOperatingAirline(AirlineId:int)`, `ProvisionFlightNumber(FlightNumber:string(16))`, `ProvisionFlight(FlightId:long)`, `ProvisionAircraft(AircraftId:int)` |
| `ProvisionFareApplicationRule` | `ProvisionAirFare(AirFareId:long)`, `ProvisionAirFareType(AirFareType)`, `ProvisionFareFamily(FareFamilyId:long)`, `ProvisionFareBasis(FareBasisCode:string(64))`, `ProvisionCabinClass(CabinClassId:int)`, `ProvisionRbd(RbdId:long)` |
| `ProvisionTravelDateRule` | `ProvisionPermittedTravelPeriod(StartDate:DateOnly,EndDate:DateOnly)`, `ProvisionBlackoutPeriod(StartDate,EndDate)`; inclusive date intervals; Blackout wins |
| `ProvisionDayTimeApplicationRule` | `ProvisionDayTimeWindow(DaysOfWeekMask:byte,StartLocalTime:TimeOnly?,EndLocalTime:TimeOnly?,Effect:Allow|Deny)`; Deny precedence |
| `ProvisionAdvancePurchaseRule` | `MinimumPeriod:int>=0`, `MaximumPeriod:int?`, `Unit:AirPrice.TimeUnit`, `SameTimeAsTicketed:bool` |
| `ProvisionBaggageApplicationRule` | `FreePieces:int?`, `FirstExcessPiece:int?`, `LastExcessPiece:int?`, `Weight:decimal?`, `WeightUnit`, `TravelApplication?`, `PurchaseApplication`, `RuleDeference?`, `ChargeKind:BaggageChargeKind?`, `AllowanceConcept:BaggageAllowanceConcept?`; v12.2 must not duplicate same immutable Spec field contradictorily |
| `ProvisionSeatApplicationRule` | `ProvisionSeatNumber(SeatNumber:string(16))`, `ProvisionSeatCharacteristic(CharacteristicCode:string(25))`; never actual seat ownership |

Each Rule Entity and row has `Id:long` and typed parent FK, immutable once Published (except the parent AR controlled lifecycle). The source's actual EF navigations/backing fields remain authoritative for technical mapping details. `AdvancePurchase` and exact error code migrations must preserve numeric enums from original source.

## 4. Publication and compatibility truth table
| Variant / document example | Definition/booking | DocumentRouting | Existing `Document.Type` | P1+P2 can claim |
|---|---|---|---|---|
| Free WCHR | SSR/WCHR, possible SubjectToConfirmation | NoAncillaryDocument | None | Valid Free request authoring; actual confirmation deferred |
| Paid extra bag with EMD | compatible booking and RFIC/RFISC | Emd | EmdAssociated or EmdStandalone as industry requires | correct filed price/docs metadata; no actual issuance |
| EXST / CBBG extra seat | validated specific booking + FlightFlow | TicketOrExchange **if actual carrier requires** | None | external Ticket action declaration; not an issued seat |
| Upgrade exchange | DynamicQuote + ticket exchange need | TicketOrExchange | None | quote required, no fake EMD ticket |
| Paid lounge with provider voucher | if carrier doc policy no Ancillary EMD | NoAncillaryDocument | None | proper supplier requirement; vouchers after P3 |

**Important:** actual RFIC/RFISC codes and whether EMD-A or -S must be sourced per airline/service policy. Do not auto assign them from `Profile`, or call `Document.Type=None` "no document anywhere" when `DocumentRouting=TicketOrExchange`.
