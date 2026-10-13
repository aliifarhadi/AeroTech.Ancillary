# 17 — Exact enums and wire contracts relevant to v12.3

| Enum | Numeric values / invariant |
|---|---|
| `AncillaryProfile` | Baggage=1 Seat=2 Upgrade=3 Meal=4 Pet=5 AssistedTravel=6 AirportService=7 Priority=8 Connectivity=9 |
| `ServiceVariant` | A01–A24 fixed string variants, A07/A08/A09 Seat only |
| `ProvisionApplicationType` | Standard=1 Baggage=2 Seat=3 |
| `ServiceCoverageScope` | Sector=1 Portion=2 Journey=3 Order=4 (NO Bound alias) |
| `PricingUnit` | PerPassenger=1 PerRoom=2 PerItem=3 PerVehicle=4 PerSeat=5 PerPiece=6 PerKilogram=7 |
| `InventoryAuthority` v12.3 | Unlimited=1 Local=2 Supplier=3; **DELETE FlightFlow=4** (no historical data) |
| `InventoryCapacityReadState` | NotConfigured=1 Unlimited=2 ConfiguredNotGuaranteed=3 ClosedForSale=4 DelegatedCheckRequired=5 Unknown=6 UnsupportedPattern=7; **APPEND NotApplicable=8** for Seat commercial-only context |
| `LocalInventoryPattern` | FlightCount=1 FlightWeight=2 FlightCountPlusWeight=3 AirportSlot=4 DailyCount=5 RoomNight=6 AssignedAsset=7; unsupported patterns do not auto-activate |
| `InventoryCountUnit` | Person=1 Piece=2 Item=3 AnimalCarrier=4 Equipment=5 |
| `PurchaseStage` | PreOrder=1 PostTicketed=2 Both=3 LegacyUnspecified=4 OnBoard=5 |
| `PriceOrigin` | Filed=1 ExternalQuote=2 Free=3 NotAvailable=4 LegacyUnspecified=5; last cannot publish |
| `PassengerUsageLimitScope` | PerOrder=1 PerFlightOccurrence=2 PerServiceDate=3 PerPortion=4 |
| `UsageConsumptionUnit` | PurchasedUnit=1 Kilogram=2 |
| `AncillaryDocumentType` | None=1 EmdAssociated=2 EmdStandalone=3; no invented Ticket enum |
| `DocumentRouting` | NoAncillaryDocument=1 Emd=2 TicketOrExchange=3 |
| `BookingMethod` | Ssr=1 AuxiliarySegment=2 DisplayPriceContactCarrierForBooking=3 NoBookingProcessRequired=4 PerServiceRecord=5 |
| `ConfirmationRequirement` | Immediate=1 SubjectToConfirmation=2 |

## Wire invariants
- Do not trust arbitrary incoming airline ID, serviceDefinitionRef, POS, seat facts or offer refs from an unauthenticated actor. Existing per-surface scope resolution and source identity maintained.
- `POST /Api/v1/AncillaryOffers` vs `/Api/v1/SeatOffers`: both use same owner+AirOffer+traveller+POS+currency context; outputs differ and cannot return the opposite family. Same for `OtaPanel` and `Backoffice`.
- Seat `OfferItemRef` is commercial scoped reference, not physical seat assignment. PricingRevisionId scoped to candidate; seatNumber selection only verified against trusted characteristic facts. No `AVAILABLE/OCCUPIED` fields from Ancillary.
- Explicit Filed Money or Free only; no fake zero on missing output. Selection evaluation handles incomplete price/context properly.
- AncillaryInventoryPolicy is for nonSeat only, never produced for Seat by any route; a `NotApplicable` inventory read state is NOT an availability guarantee.
- All current nonSeat enum numbers stable; migration only for obsolete enum FlightFlow removal in greenfield schema.
