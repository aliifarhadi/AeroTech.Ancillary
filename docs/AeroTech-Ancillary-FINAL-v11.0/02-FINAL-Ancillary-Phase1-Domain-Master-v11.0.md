# 02 — FINAL Ancillary Phase 1 Domain Master v11.0

## 1. Design principle

Keep the domain small:

```text
Supplier
AncillaryServiceDefinition
AncillaryProvision
```

These are the only Phase 1 commercial concepts the agent may expand.

Existing `AncillaryReservation` is operational baseline and frozen for this phase; it is not part of the Phase 1 design work.

## 2. Supplier

The baseline already has:

```text
Id                       long
OwnerAirlineId           int
Name                     string
FulfillmentKind          Local | External
FulfillmentProviderKey   string?
Status                   Active | Retired
CreatedAt                DateTimeOffset
RetiredAt                DateTimeOffset?
```

Phase 1 rules:

- preserve this shape unless a compile-safe Backoffice lifecycle change absolutely requires an existing field;
- do not implement adapter resolution or provider calls;
- do not route behavior by numeric SupplierId;
- do not add credentials, URLs, secrets or transport configuration;
- multiple Suppliers may define their own ServiceDefinitions for the same broad service or sub-code.

## 3. AncillaryServiceDefinition — what the service is

Preserve the baseline identity/classification shape:

```text
Id                       long
OwnerAirlineId           int
SupplierId               long
ServiceDefinitionRef     string(30)
Version                  int
ServiceTypeCode          string
ServiceSubCode           string(3)
SubCodeSource            Industry | CarrierDefined
GroupCode                string
SubGroupCode             string?
Description1Code         string?
Description2Code         string?
CommercialName           string
Description              string?
Document                 DocumentDefinition
Booking                  BookingDefinition
SalesEffectiveFrom       DateOnly?
SalesDiscontinueOn       DateOnly?
Status                   Draft | Active | Suspended | Retired
CreatedAt                DateTimeOffset
ActivatedAt              DateTimeOffset?
SuspendedAt              DateTimeOffset?
RetiredAt                DateTimeOffset?
```

Do not add v10-only speculative fields such as `SupplierServiceRef` or `IssueRequirement` in Phase 1.

### DocumentDefinition

Keep:

```text
Type   None | EmdAssociated | EmdStandalone
Rfic   string(1)?
Rfisc  string(3)?
```

This is document policy metadata only. Phase 1 does not issue EMDs.

### BookingDefinition

Keep:

```text
Method    SSR | AuxiliarySegment | DisplayPriceContactCarrierForBooking | NoBookingProcessRequired | PerServiceRecord
SsrCode   string(4)?
SsimCode  string(4)?
```

This allows wheelchair/meal/pet/special-handling style services to carry standard booking metadata without creating service-specific aggregates.

## 4. ServiceDefinition lifecycle

Phase 1 Backoffice must make the existing status model usable:

```text
Draft -> Active
Active -> Suspended
Suspended -> Active
Draft/Active/Suspended -> Retired
```

Rules:

- Draft may be edited.
- Active is immutable commercial history; changes requiring semantic modification create a revised version.
- Retired is terminal for new authoring use.
- activating requires an Active Supplier.
- historical IDs/versions are never reused.

A simple `Revise` operation may create `Version + 1` as Draft by copying the prior definition; it must not create a second domain model.

## 5. AncillaryProvision — when/to whom/where/at what filed price

Target Phase 1 shape:

```text
Id                     long
ServiceDefinitionId    long
Sequence               int
Status                 Draft | Active | Suspended | Retired

SalesEffectiveFrom     DateTimeOffset?
SalesDiscontinueAt     DateTimeOffset?

CoverageScope          ServiceCoverageScope
Passenger              PassengerCriteria
Sales                  SalesCriteria
Travel                 TravelCriteria
Fare                   FareCriteria
AdvancePurchase        AdvancePurchaseCriteria?

Quantity               QuantityRule
Application            ProvisionApplication
Outcome                CommercialOutcome
Fee                    FeeDefinition?
PriceLines[]            ProvisionPriceLine
Settlement             SettlementDefinition
Availability           AvailabilityDefinition

// legacy baseline operational metadata, frozen in Phase 1:
Fulfillment             FulfillmentDefinition

CreatedAt              DateTimeOffset
ActivatedAt            DateTimeOffset?
SuspendedAt            DateTimeOffset?
RetiredAt              DateTimeOffset?
```

`FulfillmentDefinition` already exists at the source baseline. Phase 1 MUST NOT redesign it or build behavior around it. Its future ownership is explicitly deferred to the owner.

## 6. PassengerCriteria

Phase 1 CORE:

```text
PassengerTypeCodes   PassengerTypeCode[]?  // AeroTech.Messages.AirPrice.Enums.PassengerTypeCode
```

Use separate Provisions for ADT, CHD and INF prices.

Do not add:

```text
MinAgeYears
MaxAgeYears
FrequentFlyerStatusRanks
OccurrenceFirst/Last
CustomerScore
Keyword
```

## 7. SalesCriteria

```text
PointOfSaleIds       long[]?
CustomerIds          long[]?
CustomerTypes        CustomerType[]?  // AeroTech.Messages.Core.Enums.CustomerType
```

No raw channel/PCC/office duplicate when a canonical POS identity already represents the sales context.

## 8. TravelCriteria

```text
OriginAirportIds          int[]?
DestinationAirportIds     int[]?
ViaAirportIds             int[]?
RoutePairs                RoutePair[]?
TravelFrom                DateOnly?
TravelTo                  DateOnly?
DaysOfWeek                DayOfWeek[]?
TimeFrom                  TimeOnly?
TimeTo                    TimeOnly?
MarketingAirlineIds       int[]?
OperatingAirlineIds       int[]?
FlightNumbers             string[]?
FlightIds                 long[]?
AircraftIds               int[]?
```

`RoutePair`:

```text
OriginAirportId
DestinationAirportId
Direction = Directional | BothDirections
```

## 9. FareCriteria

```text
AirFareIds          long[]?
AirFareTypes        AirFareType[]?  // AeroTech.Messages.AirPrice.Enums.AirFareType
FareFamilyIds       long[]?
FareBasisCodes      string[]?
CabinClassIds       int[]?
RbdIds              long[]?
```

IDs remain canonical numeric IDs. Do not replace FareFamilyId with a display string.

## 10. AdvancePurchaseCriteria

Authoring shape:

```text
Period   int > 0
Unit     TimeUnit  // reuse canonical AeroTech.Messages.AirPrice.Enums.TimeUnit; do not create a duplicate enum
```

Phase 1 only stores and validates this shape. It does not calculate eligibility. Reuse the existing canonical `TimeUnit` values from Contracts rather than creating an Ancillary-specific time-unit enum.

## 11. ProvisionApplication

Closed Phase 1 union:

```text
Standard
Baggage
Seat
```

### Standard

No extra fields. Use this for meal, wheelchair, insurance, lounge, priority, fast track, Wi-Fi, pet, meet & assist and similar services unless a real structured requirement proves otherwise.

### BaggageApplication

```text
FreePieces              int?
FirstExcessPiece        int?
LastExcessPiece         int?
Weight                  decimal(9,2)?
WeightUnit              WeightUnit  // use canonical AeroTech.Messages.AirPrice.Enums.WeightUnit (Kg/Lbs)
TravelApplication       AllSectors | AtLeastOneSector | MostSignificantSector | AnySectorOnJourney | null
PurchaseApplication     Prepaid | CheckIn | PrepaidAndCheckIn
RuleDeference           MarketingCarrier | OperatingCarrier | null
```

Phase 1 stores/validates these typed descriptors. It does not implement baggage-selection algorithms.

### SeatApplication

```text
SeatNumbers                 string[]?
SeatCharacteristicCodes     string[]?
```

Rules:

- at least one selector;
- if exact seat numbers are authored, AircraftIds must be populated;
- no live occupied/blocked state is stored in Ancillary;
- no seat hold is added in Phase 1.

## 12. QuantityRule

Preserve:

```text
Unit          Each | Piece | Kilogram
MinQuantity   int >= 1
MaxQuantity   int >= MinQuantity
```

## 13. CommercialOutcome

Preserve:

```text
Disposition       Paid | Free | NotAvailable
DocumentRequired  bool
BookingRequired   bool
```

Rules:

- Paid => Fee + at least one price line required;
- Free => no positive payable price;
- NotAvailable => no payable price.

## 14. Fixed filed price only

Phase 1 supports fixed filed money:

```text
FeeDefinition
  CurrencyId
  ApplicationUnit

ProvisionPriceLine
  Category        Ancillary | Tax | Fee
  Code            string?
  Name            string?
  CountryId       int?      // add only as evidence metadata
  StationAirportId int?     // add only as evidence metadata
  UnitAmount      decimal(18,2)
```

No FX and no dynamic/external quote pricing.

Current fee units that are not executable with a fully frozen formula remain non-activatable. Do not implement percentage-of-fare or per-kg formula semantics merely because the enum contains values.

## 15. Provision lifecycle and precedence metadata

Phase 1 must support:

```text
Draft -> Active
Active -> Suspended
Suspended -> Active
Draft/Active/Suspended -> Retired
```

Draft is editable. Active is immutable.

For later shopping, `Sequence` expresses author-controlled precedence. v11 freezes the intended semantics as:

```text
all populated criteria must match
candidate provisions ordered by Sequence ASC
first matching provision wins
```

However **Phase 1 does not implement this matcher**. It only authors/persists/returns the typed rules and their Sequence.

## 16. Money

Persist monetary amounts using the platform convention already used in this repository: `decimal(18,2)` for amounts.

Do not create an Ancillary-specific `decimal(19,4)` convention. ROE/FX precision is outside Phase 1.
