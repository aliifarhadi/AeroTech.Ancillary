# 02 — FINAL Ancillary Domain Master v9.1

**Status:** implementation authority.  
**Supersedes:** Ancillary Domain Master v9.0 and every earlier Ancillary domain document.

## 1. Domain purpose

`AeroTech.Ancillary` owns an airline-facing **ancillary marketplace**:

```text
Supplier
+ optional-service definition
+ applicability / filed price
+ supplier-specific fulfillment state where Ancillary is the provider
+ Ancillary-owned quota
+ published commercial read model consumed by AirAvail
```

It is not restricted to airline-self-supplied services.

It does not own:

```text
Aircraft/Airport/City/Country/Airline/Cabin master -> AirInfo
Fare/FareFamily/RBD/POS master                    -> AirPrice
live flight/physical seat inventory                -> FlightFlow
customer-facing ancillary Offer                    -> AirAvail
Order                                               -> Ordering
accountable ETKT/EMD document stock/issuance        -> Ordering
```

Complex standalone products whose fulfillment semantics are not representable as an Ancillary service remain in their own bounded context. Supplier support does not mean every travel product is forced into Ancillary.

## 2. Aggregate roots — FINAL

Exactly:

```text
Supplier
AncillaryServiceDefinition
AncillaryProvision
AncillaryReservation
AncillaryStockPool
```

Do not create aggregate roots named/equivalent to:

```text
AncillaryProduct
AncillaryPriceRule
Meal
Lounge
Seat
Baggage
ServiceSubCode
GenericRule
```

## 3. Supplier

`Supplier` identifies who supplies/fulfils one or more ancillary services inside the Ancillary marketplace and carries only the typed routing classification needed by Ancillary.

### Fields

```text
Id                       long
OwnerAirlineId           int
Name                     string(100)
FulfillmentKind          SupplierFulfillmentKind
FulfillmentProviderKey   string(50)?
Status                   SupplierStatus
CreatedAt                DateTimeOffset
RetiredAt                DateTimeOffset?
```

```text
SupplierFulfillmentKind
  Local
  External
```

```text
SupplierStatus
  Active
  Retired
```

### Rules

- `Id` is the canonical runtime identity, but **must never select behavior by itself**.
- `OwnerAirlineId > 0`.
- `Name` is required.
- one Supplier can back many ServiceDefinitions.
- several Suppliers may offer the same broad service type or same industry sub-code.
- Supplier retirement prevents **new** sale/publish usage but never invalidates already sold Order services.
- `Local` uses the Ancillary local fulfillment capability and `FulfillmentProviderKey` must be null.
- `External` requires a non-empty `FulfillmentProviderKey`.
- an unknown/unregistered external provider key is a configuration error and fails deterministically.
- provider credentials, URLs, secrets, tokens and protocol-specific settings are not Supplier domain fields.
- do not implement `switch (supplier.Id)` or equivalent hard-coded ID routing.
- `FulfillmentKind` is classification/routing, not a growing list of supplier-specific lifecycle modes. External differences live behind the adapter selected by `FulfillmentProviderKey`.
- the v9.1 adapter boundary must honor the canonical reserve/confirm/read/validate/release/cancel contract. Do not invent extra lifecycle capabilities until a concrete Supplier contract proves they are required.

A row representing the airline itself is allowed and is treated like any other Supplier identity; `SupplierId` is never optional merely because the airline supplies the service itself.

### Provider-key boundary

`Supplier.FulfillmentProviderKey` is **not** the same concept as `OrderService.FulfillmentProviderKey`.

```text
OrderService.FulfillmentProviderKey = "Ancillary"
  -> Ordering routes the sold service to the Ancillary bounded context

Supplier.FulfillmentProviderKey
  -> Ancillary routes an External supplier to a registered supplier adapter
```

## 4. AncillaryServiceDefinition

A published ServiceDefinition is an immutable version of **what is being sold and by which Supplier**.

### Fields

```text
Id                       long
OwnerAirlineId           int
SupplierId               long
ServiceDefinitionRef     string(30)
Version                  int

ServiceTypeCode          string(1)
ServiceSubCode           string(3)
SubCodeSource            ServiceSubCodeSource
GroupCode                string(2)
SubGroupCode             string(2)?
Description1Code         string(2)?
Description2Code         string(2)?

CommercialName           string(100)
Description              string(500)?

Document                 DocumentDefinition
Booking                  BookingDefinition

SalesEffectiveFrom       DateOnly?
SalesDiscontinueOn       DateOnly?

Status                   ServiceDefinitionStatus
CreatedAt                DateTimeOffset
ActivatedAt              DateTimeOffset?
SuspendedAt              DateTimeOffset?
RetiredAt                DateTimeOffset?
```

### Runtime identity

```text
Id long
```

is the only internal identity used by Provision, Order snapshots, Reservation and StockPool.

### Business/display metadata

```text
OwnerAirlineId + ServiceDefinitionRef + Version
```

remains useful for human/audit purposes but is **not** a foreign key and must not be used to resolve the sold service during fulfillment.

### Lifecycle

```text
Draft
Active
Suspended
Retired
```

Rules:

- `Version >= 1`.
- only one Active version per `OwnerAirlineId + ServiceDefinitionRef`.
- Active fields are immutable.
- `Revise` creates a new Draft row with a new `Id` and incremented Version.
- `SupplierId` is immutable once published.
- activation requires the referenced Supplier to be Active.
- supplier retirement removes its definitions from new shopping but old IDs remain valid for historical fulfillment/cancel/refund.

## 5. Industry sub-code reference — no mutable ServiceSubCode aggregate

`ServiceSubCode` remains an S5-style classification field on ServiceDefinition.

```text
ServiceSubCodeSource
  Industry
  CarrierDefined
```

### Industry source

For `Industry`, code semantics are resolved from a read-only `IndustryServiceSubCodeReference` dataset.

Minimum reference fields:

```text
Code                 string(3)
ServiceTypeCode      string(1)
Rfic                 string(1)?
GroupCode            string(2)
SubGroupCode         string(2)?
Description1Code     string(2)?
Description2Code     string(2)?
CommercialName       string
DocumentType         AncillaryDocumentType?   // only where authoritative dataset supplies it
```

Rules:

- an Industry code must exist in the reference dataset;
- ServiceDefinition classification fields must equal the authoritative reference entry;
- Backoffice must not allow an analyst to redefine an Industry code's semantic meaning;
- the donor's two hard-coded examples (`0CC`, `0BX`) are test fixtures, not a complete production table.

### Carrier-defined source

Carrier-defined sub-codes are allowed under the airline's own namespace/rules.

For a CarrierDefined code:

- it must satisfy the platform's accepted ATPCO-compatible 3-character code format;
- the airline supplies its classification/name/document semantics at definition time;
- those semantics are immutable after first publication of that ServiceDefinition version.

The implementation must not guess an Industry meaning for a code absent from the reference dataset.

## 6. ServiceTypeCode — ATPCO values

Accepted source values:

```text
A  Baggage Allowance
B  Carry-on Allowance
C  Baggage Charges
E  Embargoes
F  Flight Related
M  Merchandise
T  Ticket Related
R  Reissue/Refund
Z  Branded Fares
```

CORE examples:

```text
extra / overweight baggage charge -> C
paid seat / meal / flight-related service -> normally F when its industry semantics are flight-related
```

Do not assign `F` merely because every ancillary happens to be sold with a flight.

## 7. DocumentDefinition

```text
Type       AncillaryDocumentType
Rfic       string(1)?
Rfisc      string(3)?
```

```text
AncillaryDocumentType
  None
  EmdAssociated
  EmdStandalone
```

Rules:

- EMD types require valid RFIC/RFISC semantic evidence.
- Ancillary never allocates accountable EMD document numbers.
- Ordering owns accountable-document issuance.

## 8. BookingDefinition

```text
Method      BookingMethod
SsrCode     string(4)?
SsimCode    string(4)?
```

```text
BookingMethod
  Ssr
  AuxiliarySegment
  DisplayPriceContactCarrierForBooking
  NoBookingProcessRequired
  PerServiceRecord
```

`Ssr` requires `SsrCode`.

## 9. AncillaryProvision

A Provision is an immutable published rule defining **when/how/how much** a ServiceDefinition is offered.

### Fields

```text
Id                     long
ServiceDefinitionId    long
Sequence               int
Status                 ProvisionStatus

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
Settlement             SettlementDefinition
Availability           AvailabilityDefinition
Fulfillment            FulfillmentDefinition

CreatedAt              DateTimeOffset
ActivatedAt            DateTimeOffset?
SuspendedAt            DateTimeOffset?
RetiredAt              DateTimeOffset?
```

### Status

```text
Draft
Active
Suspended
Retired
```

### Matching rule

For a ServiceDefinition candidate:

```text
Supplier Active for new shopping
+ ServiceDefinition Active/effective
+ Provision Active/effective
+ every populated criterion matches
-> order candidates by Sequence ascending
-> first matching candidate wins
```

If the winner is `NotAvailable`, no purchasable OfferItem is emitted.

### Sequence invariant

At most one Active Provision for:

```text
ServiceDefinitionId + Sequence
```

A revision receives a new Provision `Id`; activation retires/supersedes the prior active row at that sequence atomically.

## 10. ServiceCoverageScope

```text
Sector
Portion
Journey
Order
```

The accepted sale stores concrete:

```text
CoveredFlightIds long[]
```

## 11. PassengerCriteria — CORE only

```text
PassengerTypeCodes   PassengerTypeCode[]?
```

There are **no implementation fields** for:

```text
MinAgeYears
MaxAgeYears
OccurrenceFirst
OccurrenceLast
FrequentFlyerStatusRanks
Keyword
CustomerIndexScoreMin
CustomerIndexScoreMax
```

Those are legitimate ATPCO benchmark dimensions but are not part of v9.1 runtime/domain code until authoritative context and a real requirement exist. They are recorded in the Gap Register, not as dead nullable fields.

Different ADT/CHD/INF prices are separate PTC-qualified Provisions, not `AdultPrice/ChildPrice/InfantPrice` columns.

## 12. SalesCriteria

```text
PointOfSaleIds        long[]?
CustomerIds           long[]?
CustomerTypes         CustomerType[]?
```

Rules:

- PointOfSale IDs are canonical AirPrice IDs.
- CustomerType uses the platform's canonical values.
- do not duplicate Country/Channel/AgencyOffice dimensions that are already represented by the resolved PointOfSale.
- no PCC until a canonical platform PCC field exists.
- no inactive `OrganizationTypeCodes` field in v9.1.

## 13. TravelCriteria

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

### RoutePair

```text
OriginAirportId       int
DestinationAirportId  int
Direction             Directional | BothDirections
```

Rules:

- lists inside one criterion are OR;
- populated different criteria are AND;
- `RoutePairs` exists to avoid origin/destination cartesian-product mistakes;
- `FlightIds` is the exact platform flight-instance selector for exceptions/campaigns;
- broad route/flight-number/date/equipment rules remain preferable where per-flight maintenance is not required.

## 14. FareCriteria — CORE only

```text
AirFareIds          long[]?
AirFareTypes        AirFareType[]?
FareFamilyIds       long[]?
FareBasisCodes      string[]?
CabinClassIds       int[]?
RbdIds              long[]?
```

Rules:

- FareFamily matching uses `FareFamilyId`, never display name.
- FareBasis matching is normalized exact string matching; no wildcard mini-language.
- TicketDesignator/AccountCode/TourCode/TariffCode/RuleNumber are not implemented fields in v9.1; see Gap Register.

## 15. AdvancePurchaseCriteria

```text
Period             int
Unit               Minute | Hour | Day | Month
SameTimeAsTicketed bool
```

Keep calendar units explicit; do not flatten day/month rules into arbitrary minutes.

## 16. QuantityRule

```text
Unit           AncillaryQuantityUnit
MinQuantity    int
MaxQuantity    int
```

```text
AncillaryQuantityUnit
  Each
  Piece
  Kilogram
```

Rules:

- minimum >= 1 for a purchasable item unless the specific service contract explicitly permits zero;
- maximum >= minimum;
- quantity is not a generic room/car/resource allocation model.

## 17. ProvisionApplication — CORE only

Closed union:

```text
StandardApplication
BaggageApplication
SeatApplication
```

No `UpgradeApplication` implementation in v9.1. Upgrade remains a separate future gap until the full availability/reprice/servicing contract is closed.

## 18. StandardApplication

No fields.

Used when no service-specific commercial semantics beyond Provision criteria/quantity are required.

## 19. BaggageApplication

```text
FreePieces              int?
FirstExcessPiece        int?
LastExcessPiece         int?
Weight                  decimal(9,2)?
WeightUnit              Kilogram | Pound
TravelApplication       BaggageTravelApplication?
PurchaseApplication     Prepaid | CheckIn | PrepaidAndCheckIn
RuleDeference           MarketingCarrier | OperatingCarrier | null
```

```text
BaggageTravelApplication
  AllSectorsOnBaggageTravel
  AtLeastOneSectorOnBaggageTravel
  MostSignificantSectorOnBaggageTravel
  AnySectorOnJourney
```

The travel-application semantics map to ATPCO baggage behavior and must not become unrelated booleans.

## 20. SeatApplication

```text
SeatNumbers                 string[]?
SeatCharacteristicCodes     string[]?
```

Rules:

1. at least one selector is required;
2. exact `SeatNumbers` require non-empty `Travel.AircraftIds`;
3. each selected seat must exist in the AirInfo static seat map of every AircraftId in that Provision; otherwise split the Provision by compatible aircraft/layout;
4. characteristic codes must be AirInfo/IATA PADIS-compatible codes;
5. when both selectors are populated, both must match;
6. Ancillary stores no seat geometry/occupancy/blocked state;
7. FlightFlow remains live seat availability/assignment authority.

This supports practical authoring such as:

```text
Aircraft 123 -> [1A,1C,2A,2C] -> fixed filed price
Aircraft 123 -> PADIS characteristic W -> fixed filed price
```

## 21. CommercialOutcome

```text
Disposition        Paid | Free | NotAvailable
DocumentRequired   bool
BookingRequired    bool
```

Rules:

- Paid requires Fee.
- Free has no positive payable price.
- NotAvailable produces no purchasable OfferItem.

## 22. FeeDefinition — v9.1 implemented shape

```text
CurrencyId          int
ApplicationUnit     FeeApplicationUnit
PriceLines          ProvisionPriceLine[]
```

Only fixed filed monetary pricing is implemented in v9.1.

There is no dead `Mileage`, `ExternalQuote`, or generic `PercentageOfFare` implementation field.

### FeeApplicationUnit

```text
OneWay
RoundTrip
Item
SectorOrPortion
Ticket
PerOneKilogramOver
PerFiveKilogramsOver
HalfPercentOfFarePerKilogram
OnePercentOfFarePerKilogram
OneAndHalfPercentOfFarePerKilogram
```

Only an application unit whose calculation is actually implemented and covered by tests may be activated. The first closure must fully support the fixed-amount units used by CORE scenarios.

### ProvisionPriceLine

```text
Category        AncillaryPriceLineCategory
Code            string?
Name            string?
UnitAmount      decimal       // persisted as decimal(18,2)
```

```text
AncillaryPriceLineCategory
  Ancillary
  Tax
  Fee
```

Money rules:

- persisted amount precision follows platform convention `decimal(18,2)`;
- currency-specific display/rounding is handled by current platform monetary rules;
- ROE is not stored here;
- Ancillary never converts filed price into selling currency.

## 23. SettlementDefinition

```text
ReissueRefund          Refundable | NonRefundable | NonRefundableReusable
FormOfRefund           OriginalPayment | EVoucher | null
Commissionable         bool
InterlineSettlement    bool
```

This is sold commercial policy. Supplier identity is a separate concept and must not be encoded in settlement booleans.

## 24. AvailabilityDefinition

```text
MustCheckAvailability bool
```

This indicates that availability must be checked before acceptance; it does not make Ancillary owner of physical seat inventory.

## 25. FulfillmentDefinition

```text
FulfillmentProviderKey string(50)
```

No `ReservationMode` field.

Typical Ordering ownership:

```text
marketplace service fulfilled through Ancillary -> Ancillary
physical seat assignment                         -> FlightFlow
```

Ordering resolves provider capability. When provider = Ancillary, Ancillary loads the immutable ServiceDefinition and Supplier, then routes by typed Supplier policy.

## 26. Supplier-specific fulfillment rule

For every Ancillary-owned reserve/confirm/read/validate/cancel operation:

```text
ReservationUnit.ServiceDefinitionId
-> immutable ServiceDefinition
-> SupplierId
-> Supplier
-> FulfillmentKind
   Local    -> local Ancillary capability
   External -> FulfillmentProviderKey -> registered supplier adapter
```

Rules:

- `SupplierId` identifies the supplier; it is never a strategy discriminator;
- different Suppliers may have different external adapters, but routing is through the typed fields above;
- the sold ServiceDefinition's SupplierId is authoritative even if the Supplier is later retired;
- do not route using service name, sub-code, `ServiceDefinitionRef`, or hard-coded numeric IDs;
- external supplier references returned during reserve/confirm/read/cancel are stored as provider evidence (`ProviderUnitRef` or current equivalent);
- endpoint URLs, credentials and secrets remain integration configuration outside Supplier domain state;
- accountable EMD issuance remains Ordering authority;
- **no generic Supplier `Issuance`/`Activation` endpoint exists in v9.1 CORE**. Add such a capability only when a concrete real supplier contract requires a distinct post-confirm operation.

## 27. Published commercial read model consumed by AirAvail

The published projection must contain enough data for AirAvail to evaluate without calling an Ancillary rule-evaluation API:

```text
Suppliers needed for display/active-sale filtering
ServiceDefinitions
Provisions
ProvisionPriceLines
BaggageApplications
SeatApplications
```

At minimum each published ServiceDefinition row exposes:

```text
Id
OwnerAirlineId
SupplierId
SupplierName
ServiceDefinitionRef
Version
classification/document/booking/display fields
status/effective dates
```

Each Provision row exposes its canonical `Id`, `ServiceDefinitionId`, Sequence, criteria, filed fee, settlement, availability and fulfillment fields.

AirAvail is the only live eligibility/pricing evaluator.

## 28. AncillaryReservation

Used only for Order services whose `FulfillmentProviderKey == "Ancillary"`.

### Root

```text
Id                    long
IdempotencyKey        string
OrderId               long
Reference             string
RequestedExpiresAt    DateTimeOffset?
ExpiresAt             DateTimeOffset?
Status                AncillaryReservationStatus
CreatedAt             DateTimeOffset
UpdatedAt             DateTimeOffset
Units[]
```

There is no reservation-level selling currency field.

### Unit

```text
Id                    long
OrderServiceId        long
ServiceDefinitionId   long
ProvisionId           long
TravellerId           long?
CoverageScope         ServiceCoverageScope
CoveredFlightIds      long[]
Quantity              int
StockPoolId           long?
ProviderUnitRef       string?
Status                AncillaryReservationUnitStatus
CancellationReasonCode string?
```

Removed from v8:

```text
ServiceDefinitionRef
ServiceDefinitionVersion
AcceptedRevenue
CurrencyId used for price validation
```

### Reservation status

```text
Held
Confirmed
Released
Expired
Cancelled
```

### Hold validation

Ancillary Hold validates only fulfillment facts:

```text
idempotency
OrderServiceId uniqueness/current state
ServiceDefinitionId exists
ProvisionId belongs to ServiceDefinitionId
quantity
coverage identity
Ancillary-owned quota if applicable
supplier-specific reserve behavior
concurrency
```

It does **not**:

```text
re-evaluate fare/POS/customer/route
re-price the service
convert currencies
compare selling amount against filed amount
```

Ordering has already accepted AirAvail's commercial Detail and is trusted as the caller.

## 29. AncillaryStockPool

Only for inventory/quota actually owned by Ancillary, for example a fixed meal allotment.

```text
Id                    long
OwnerAirlineId        int
ServiceDefinitionId   long
FlightId              long
Capacity              int
Held                  int
Confirmed             int
Status                AncillaryStockPoolStatus
RowVersion             concurrency token
CreatedAt             DateTimeOffset
UpdatedAt             DateTimeOffset
```

```text
AncillaryStockPoolStatus
  Active
  Suspended
  Retired
```

Invariant:

```text
Held >= 0
Confirmed >= 0
Held + Confirmed <= Capacity
```

Unique live stock identity:

```text
ServiceDefinitionId + FlightId
```

No `ServiceDefinitionRef + Version` relationship exists.

## 30. Idempotency and occurrence identity

For one traveller-covered service:

```text
ServiceDefinitionId
+ TravellerId
+ canonical sorted CoveredFlightIds
```

is the stable sold-service occurrence identity used for duplicate/max-count checks where applicable.

For order-scoped coverage, use:

```text
ServiceDefinitionId + OrderId
```

Do not build identity from names or business refs.

## 31. Price acceptance boundary

Commercial flow:

```text
Ancillary filed price
-> AirAvail evaluates + converts to selling currency
-> AirAvail Detail freezes filed + selling evidence
-> Ordering accepts/stores selling commercial snapshot
-> Ancillary fulfillment receives no selling-price validation responsibility
```

If the selling currency/rate/amount is wrong, the failure belongs in AirAvail/Ordering acceptance, not in Ancillary Hold.

## 32. Historical integrity

Old Orders remain valid after:

```text
ServiceDefinition revision
Provision revision/retirement
Supplier retirement
price changes
```

because the sold Order snapshot contains immutable IDs and commercial/display evidence.

Minimum Ordering accepted ancillary evidence:

```text
ServiceDefinitionId
ProvisionId
SupplierId
ServiceDefinitionRef         // snapshot/display only
ServiceDefinitionVersion     // snapshot/display only
SupplierName                 // snapshot/display only where needed
classification/document/booking/settlement snapshots
quantity/coverage
filed price evidence
selling price/ROE evidence
FulfillmentProviderKey        // Ordering-level provider key, e.g. Ancillary/FlightFlow
```

`Supplier.FulfillmentProviderKey` is not part of the sold AirAvail/Ordering commercial snapshot and does not leak outside Ancillary supplier routing.

No historical operation resolves a sold service by `ServiceDefinitionRef + Version`.

## 33. Explicitly not implemented in v9.1 domain code

The following remain industry-backed future extensions and are documented in `08-FINAL-Gap-Register-v9.1.md`, but must **not** appear as inactive nullable code fields now:

```text
Age qualifiers
Passenger occurrence qualifiers
Frequent-flyer status qualifiers
Keyword/customer-score qualifiers
Organization-type qualifier
Ticket designator
Account code
Tour code
Tariff/rule number
Mileage pricing
External dynamic quote method
UpgradeApplication
full interline concurrence engine
PCC
```

Adding one later requires an authoritative input contract and a concrete scenario, not speculative placeholders.
