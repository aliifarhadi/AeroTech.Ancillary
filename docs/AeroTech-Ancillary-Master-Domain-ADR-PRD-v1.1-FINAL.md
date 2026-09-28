# AeroTech.Ancillary — Master Domain ADR/PRD v1.1 FINAL
## SINGLE V1 DOMAIN AUTHORITY

**Status:** FINAL DOMAIN CLOSURE  
**Scope:** Airline ancillary catalog, sellability, pricing, quote authority, fulfillment/document requirements  
**Purpose:** define the complete V1 business domain, field-level model, invariants, lifecycle and behavior so implementation requires no domain invention.

---

# 0. Executive decision

`AeroTech.Ancillary` is the airline commercial authority for optional/ancillary services.

It owns:

- ancillary product identity and classification;
- ancillary sellability/applicability;
- ancillary commercial pricing;
- ancillary quote authority;
- booking/fulfillment requirements as commercial metadata;
- EMD requirement/RFIC/RFISC metadata;
- baggage-product entitlement definition;
- airline-specific ancillary quote validity policy.

It does not own:

- flight, cabin or RBD inventory;
- physical seat availability/hold/confirm/release;
- base airfare or fare-rule pricing;
- accepted Order history;
- EMD/ETKT document lifecycle;
- payment;
- DCS delivery/consumption;
- revenue accounting or interline settlement.

The canonical V1 sales path is post-Order:

```text
Existing Order
    -> discover sellable ancillaries
    -> customer selects exact ancillary choices
    -> create immutable exact Ancillary Quote
    -> Ordering validates/accepts Quote Item(s)
    -> Ordering owns purchased service/history
    -> external operational owner fulfills where required
    -> Ordering issues EMD when required
```

---

# 1. Domain authority and benchmark conclusions

The V1 domain is benchmarked against airline Optional Services/ancillary behavior visible in:

- ATPCO Optional Services S5/S7 semantics;
- IATA EMD-A / EMD-S and RFIC/RFISC semantics;
- Amadeus airline ancillary merchandising behavior;
- Sabre ancillary shopping/booking behavior;
- AeroTech FlightFlow, AirPrice, AirAvail and Ordering ownership boundaries.

Canonical conclusions:

1. Product definition and provision/applicability are distinct concepts.
2. Ancillary commercial sellability is distinct from operational inventory availability.
3. Included baggage allowance is distinct from purchased baggage.
4. EMD requirement is commercial metadata; the EMD document itself is not Ancillary ownership.
5. A commercial quote must be immutable and short-lived.
6. A quote is not proof of physical seat/resource availability.
7. Optional-service pricing must be deterministic across passenger, segment/portion/journey and quantity.
8. Quote acceptance must never require Ordering to recalculate an ancillary price.
9. Local POS/travel clock facts used in applicability are business facts, not server-time assumptions.
10. Interline concurrence, dynamic pricing, full tax jurisdiction and local quota inventory are valid airline concerns but are intentionally outside V1 behavior.

---

# 2. Canonical ownership boundary

| Business truth | Canonical owner | Ancillary role |
|---|---|---|
| Flight schedule/status | FlightFlow | context only |
| Cabin/RBD/flight capacity | FlightFlow | context only; never duplicate |
| Seat map / physical seat state | FlightFlow | external operational authority |
| Seat hold/confirm/release | FlightFlow | external fulfillment |
| AirFare / FareBasis / FareFamily | AirPrice | matching context only |
| Base airfare and fare rules | AirPrice | never calculate |
| Base air Offer composition | AirAvail | future consumer/composer |
| Ancillary catalog | Ancillary | authoritative |
| Ancillary applicability | Ancillary | authoritative |
| Ancillary price | Ancillary | authoritative |
| Ancillary quote | Ancillary | authoritative |
| Purchased ancillary | Ordering | authoritative commercial history |
| Paid-seat purchased history | Ordering | authoritative commercial history |
| ETKT/EMD | Ordering | document lifecycle authority |
| Payment/funding | JetPay / financial orchestration | external |
| Delivery/consumption/no-show | DCS / delivery owner | external |
| Revenue accounting/settlement | accounting/settlement owner | external |

---

# 3. V1 airline product scope

## 3.1 Supported now

V1 supports commercial definition, sellability and pricing for:

1. paid seat;
2. prepaid checked baggage;
3. additional carry-on baggage;
4. excess-weight / oversize / special-item baggage;
5. meal / inflight service;
6. priority boarding;
7. airport service;
8. lounge;
9. generic flight-associated optional service;
10. generic standalone airline optional service representable by EMD-S.

V1 supports:

- charged ancillaries;
- free ancillaries;
- free ancillary with EMD requirement;
- passenger-specific products;
- per-passenger-per-segment pricing;
- quantity/occurrence-based purchase;
- segment scope;
- caller-declared multi-segment portion scope;
- journey scope;
- direct/B2C, agency, corporate and backoffice sales contexts through the canonical shared `SalesChannel`;
- private/customer-specific availability;
- marketing/operating carrier applicability;
- codeshare contexts;
- passenger-type and age restrictions;
- fare/fare-family/cabin/RBD restrictions;
- route/flight/equipment/date/day/time restrictions;
- EMD-A and EMD-S metadata;
- SSR booking metadata;
- external-provider fulfillment metadata;
- multi-passenger and multi-segment Orders.

## 3.2 Deferred in V1

The domain recognizes but V1 does not execute:

- interline/validating-carrier concurrence;
- full ATPCO S6 concurrence;
- pre-Order ancillary retail integration into AirAvail;
- bundles;
- loyalty points/miles pricing;
- bid/auction upgrades;
- dynamic/revenue-management ancillary pricing;
- subscription/pass products;
- full jurisdictional tax engine;
- ancillary penalty schedule engine;
- ancillary exchange/reissue calculation;
- involuntary disruption reaccommodation;
- DCS consumption processing;
- locally owned quota inventory;
- third-party hotel/car/insurance inventory;
- tiered FirstOccurrence/LastOccurrence pricing;
- maker-checker release workflow.

Deferred behavior may not be approximated or guessed by V1.

---

# 4. Ubiquitous language

## Ancillary Service
Stable airline catalog identity for an optional service.

## Provision
Commercial rule describing when a Service is sellable, its price and its effective commercial requirements.

## Coverage
Exact transport scope to which a candidate or quoted ancillary applies.

## CoverageRef
Typed identity of one exact Segment, Portion or Journey coverage.

## Sellable Candidate
Non-persisted commercial preview showing what could be bought in a supplied context. It is not acceptance authority.

## Quote
Immutable, expiring commercial acceptance authority for exact customer selections.

## Quote Item
One exact passenger/order + coverage + quantity + price + fulfillment/document snapshot.

## Application Unit
The business unit that determines how many times a configured fee is applied.

## Fulfillment Requirement
Commercial metadata telling Ordering which external operational behavior is required.

## Document Requirement
Commercial metadata stating whether the sold ancillary requires no accountable document, EMD-A or EMD-S.

---

# 5. Domain roots and supporting concepts

V1 has exactly three business Aggregate Roots:

1. `AncillaryService`
2. `AncillaryProvision`
3. `AncillaryQuote`

Supporting domain concepts that are not Aggregate Roots:

- `AncillaryAirlineQuotePolicy`
- `LocalDateTimeWithOffset`
- `AncillaryCoverageRef`
- `AncillaryPassengerApplicability`
- `AncillaryApplicabilityCondition`
- `AncillaryTaxComponent`
- `AncillaryBaggageDefinition`
- `AncillaryQuoteItem`
- `AncillaryQuoteTax`
- `AncillaryQuoteBaggage`
- request/context/result records defined by this Master.

There is no V1:

- `AncillaryOrder`;
- `AncillaryReservation`;
- `AncillaryInventory`;
- `AncillaryPayment`;
- generic Saga/Workflow domain root;
- ServiceRevision/ProvisionRevision aggregate;
- occurrence-history aggregate.

---

# 6. Canonical value objects

## 6.1 `LocalDateTimeWithOffset`

Represents the business-local wall clock used by sales/travel applicability.

| Field | Type | Null | Rule |
|---|---|---:|---|
| `LocalDate` | `DateOnly` | no | local calendar date |
| `LocalTime` | `TimeOnly` | no | local wall-clock time |
| `UtcOffsetMinutes` | `short` | no | `-840..+840` |

Rules:

1. The instant is derived from these values; callers do not supply a second instant field.
2. Sales-date rules use `LocalDate`.
3. Day-of-week travel rules use `LocalDate`.
4. Departure-time rules use `LocalTime`.
5. No server-local timezone participates in domain decisions.

## 6.2 `AncillaryCoverageRef`

| Field | Type | Null |
|---|---|---:|
| `Scope` | `AncillaryCoverageScope` | no |
| `Ref` | `string` | no |

Rules:

- Ref is trimmed and non-empty.
- `Segment` -> Ref resolves one `SegmentRef`.
- `Portion` -> Ref resolves one `PortionRef`.
- `Journey` -> Ref resolves one `JourneyRef`.
- Scope and Ref together form the business identity.
- untyped raw coverage strings are not valid.

---

# 7. Aggregate Root — AncillaryService

## 7.1 Purpose

`AncillaryService` answers:

- what optional service is being sold?
- how is it classified?
- what is its default booking/fulfillment behavior?
- what accountable-document identity is normally required?
- for baggage, what entitlement is being purchased?

It does not contain route-specific prices or passenger-specific eligibility.

## 7.2 Fields

| Field | Type | Null | Meaning / constraint |
|---|---|---:|---|
| `Id` | `long` | no | stable catalog identity |
| `OwnerAirlineId` | `long` | no | airline owning the service definition |
| `SubCode` | `string` | no | exactly 3 uppercase alphanumeric |
| `IsIndustryDefined` | `bool` | no | industry-defined vs carrier-defined |
| `ServiceType` | `AncillaryServiceType` | no | service classification |
| `GroupCode` | `string` | no | exactly 2 uppercase alphanumeric |
| `SubGroupCode` | `string?` | yes | exactly 2 uppercase alphanumeric when supplied |
| `CommercialName` | `string` | no | 1..100 |
| `Description` | `string?` | yes | max 500 |
| `BookingMethod` | `AncillaryBookingMethod` | no | default booking semantic |
| `SsrCode` | `string?` | yes | exactly 4 uppercase chars when applicable |
| `DefaultFulfillmentMode` | `AncillaryFulfillmentMode` | no | default operational requirement |
| `FulfillmentProviderKey` | `string?` | yes | provider identity when required |
| `DefaultDocumentRequirement` | `AncillaryDocumentRequirement` | no | None/EmdA/EmdS |
| `ReasonForIssuanceCode` | `string?` | yes | RFIC; one uppercase letter when EMD required |
| `ReasonForIssuanceSubCode` | `string?` | yes | RFISC; exactly 3 uppercase alphanumeric when EMD required |
| `QuantityUnit` | `AncillaryQuantityUnit` | no | Unit/Piece/Kilogram/Pound |
| `State` | `AncillaryDefinitionState` | no | Draft/Released/Suspended/Discontinued |
| `SalesEffectiveFrom` | `DateOnly` | no | first sellable local POS date |
| `SalesDiscontinueOn` | `DateOnly?` | yes | inclusive last sellable local POS date |
| `CreatedAt` | `DateTimeOffset` | no | audit instant |
| `ReleasedAt` | `DateTimeOffset?` | yes | first release instant |
| `Revision` | `int` | no | starts at 1; commercial provenance |

Domain identity rule:

```text
(OwnerAirlineId, SubCode, ServiceType)
```

must be unique in the business catalog.

## 7.3 `AncillaryBaggageDefinition`

Exists only for `ServiceType=BaggageCharge`.

| Field | Type | Null | Meaning |
|---|---|---:|---|
| `Id` | `long` | no | child identity |
| `AncillaryServiceId` | `long` | no | parent |
| `PieceCount` | `int?` | yes | purchased piece entitlement |
| `WeightPerPiece` | `decimal(9,3)?` | yes | weight limit/value per piece |
| `WeightUnit` | `WeightUnit?` | yes | Kilogram/Pound |
| `TotalWeight` | `decimal(9,3)?` | yes | total weight entitlement for weight-based product |
| `IsPrepaid` | `bool` | no | prepaid vs other charge semantics |
| `BaggageKind` | `AncillaryBaggageKind` | no | Checked/CarryOn/ExcessWeight/Oversize/SpecialItem |

Rules:

- `PieceCount > 0` when supplied.
- weight values must be `> 0`.
- WeightUnit is required when any weight exists.
- Checked/CarryOn/ExcessWeight must have at least one quantity descriptor.
- included fare baggage is never represented by this entity.

## 7.4 `AncillaryMedia`

Optional non-pricing catalog metadata.

| Field | Type | Null |
|---|---|---:|
| `Id` | `long` | no |
| `AncillaryServiceId` | `long` | no |
| `MediaReference` | `string` | no |
| `SortOrder` | `int` | no |

Media never changes sellability or price.

## 7.5 Service invariants

1. `OwnerAirlineId`, `SubCode` and `ServiceType` become immutable after first Release.
2. `IsIndustryDefined` may change only before first Release; afterwards it is immutable.
3. `Discontinued` is terminal.
4. `Suspended` may return to `Released`.
5. `DefaultDocumentRequirement=EmdA` requires flight-associated semantics.
6. `EmdS` must not require ET coupon association.
7. `BookingMethod=Ssr` requires `SsrCode`.
8. `DefaultFulfillmentMode=FlightFlowSeat` requires a flight-associated service.
9. `ExternalProvider` fulfillment requires a provider key.
10. Service definition never contains a route/passenger-specific price.

---

# 8. Catalog enums

## `AncillaryServiceType`

```text
FlightService
BaggageCharge
Merchandise
TicketRelated
ReissueRefundRelated
```

V1 operational focus is `FlightService` and `BaggageCharge`; the remaining classifications allow standard EMD-S/ticket-related representation without redesign.

## `AncillaryBookingMethod`

```text
Ssr
AuxiliarySegment
ContactCarrier
NoBookingRequired
DefinedByProvision
```

## `AncillaryFulfillmentMode`

```text
None
FlightFlowSeat
Ssr
ExternalProvider
LocalQuota
```

V1 behavior:

- `None`: supported.
- `FlightFlowSeat`: supported as external FlightFlow fulfillment requirement.
- `Ssr`: metadata supported; mutation is source-gated.
- `ExternalProvider`: metadata supported; actual provider contract is source-gated.
- `LocalQuota`: DEFER; no V1 inventory behavior exists.

## `AncillaryDocumentRequirement`

```text
None
EmdA
EmdS
```

No ambiguous `Either` value exists in V1.

## `AncillaryQuantityUnit`

```text
Unit
Piece
Kilogram
Pound
```

## `AncillaryBaggageKind`

```text
Checked
CarryOn
ExcessWeight
Oversize
SpecialItem
```

## `WeightUnit`

```text
Kilogram
Pound
```

## `AncillaryDefinitionState`

```text
Draft
Released
Suspended
Discontinued
```

---

# 9. Aggregate Root — AncillaryProvision

## 9.1 Purpose

A Provision answers:

- whether a Service is sellable in the supplied context;
- which passenger/transport scope it applies to;
- its exact fixed commercial price;
- quantity limits;
- booking/fulfillment/document overrides;
- commercial refundability and commissionability;
- fixed V1 tax components.

A Service may have multiple Provisions.

## 9.2 Fields

| Field | Type | Null | Meaning / constraint |
|---|---|---:|---|
| `Id` | `long` | no | aggregate identity |
| `AncillaryServiceId` | `long` | no | parent Service reference |
| `OwnerAirlineId` | `long` | no | must equal Service OwnerAirlineId |
| `Sequence` | `int` | no | deterministic precedence; >0 |
| `State` | `AncillaryDefinitionState` | no | lifecycle |
| `SalesEffectiveFrom` | `DateOnly` | no | inclusive local POS date |
| `SalesDiscontinueOn` | `DateOnly?` | yes | inclusive |
| `TravelEffectiveFrom` | `DateOnly?` | yes | inclusive origin-local travel date |
| `TravelDiscontinueOn` | `DateOnly?` | yes | inclusive |
| `CoverageScope` | `AncillaryCoverageScope` | no | Segment/Portion/Journey |
| `ApplicationUnit` | `AncillaryApplicationUnit` | no | pricing application unit |
| `FirstOccurrence` | `int?` | yes | reserved for deferred occurrence-tier pricing; MUST be null in released V1 |
| `LastOccurrence` | `int?` | yes | reserved for deferred occurrence-tier pricing; MUST be null in released V1 |
| `MinimumQuantity` | `decimal(9,3)` | no | >0 |
| `MaximumQuantity` | `decimal(9,3)` | no | >= MinimumQuantity |
| `FeeDisposition` | `AncillaryFeeDisposition` | no | Charged/Free/Unavailable |
| `Amount` | `decimal(19,4)?` | yes | per-application configured amount |
| `CurrencyId` | `int?` | yes | required for Charged |
| `Commissionable` | `bool` | no | commercial/accounting snapshot term |
| `Refundability` | `AncillaryRefundability` | no | V1 servicing term |
| `BookingMethodOverride` | `AncillaryBookingMethod?` | yes | overrides Service |
| `DocumentRequirementOverride` | `AncillaryDocumentRequirement?` | yes | overrides Service |
| `FulfillmentModeOverride` | `AncillaryFulfillmentMode?` | yes | overrides Service |
| `FulfillmentProviderKeyOverride` | `string?` | yes | required when effective fulfillment needs provider |
| `CreatedAt` | `DateTimeOffset` | no | audit instant |
| `ReleasedAt` | `DateTimeOffset?` | yes | first release instant |
| `Revision` | `int` | no | starts at 1; commercial provenance |

There is no Provision-level `TaxIncluded` flag. Inclusion is defined only per tax component.

## 9.3 `AncillaryPassengerApplicability`

| Field | Type | Null |
|---|---|---:|
| `Id` | `long` | no |
| `AncillaryProvisionId` | `long` | no |
| `PassengerTypeCode` | `PassengerTypeCode?` | yes |
| `MinimumAge` | `int?` | yes |
| `MaximumAge` | `int?` | yes |

Rules:

- empty collection = all passengers;
- rows are OR;
- non-null fields within one row are AND;
- age range is `0..125`;
- MinimumAge <= MaximumAge;
- Age is never inferred from PTC.

If an otherwise relevant Provision contains an age criterion and caller Age is null:

```text
candidate omitted
+ Diagnostics[AgeContextRequired]
```

## 9.4 `AncillaryApplicabilityCondition`

Used in Include and Exclude collections.

| Field | Type | Null | Meaning |
|---|---|---:|---|
| `Id` | `long` | no | child identity |
| `AncillaryProvisionId` | `long` | no | parent |
| `PointOfSaleCountryId` | `long?` | yes | sales context |
| `SalesChannel` | `SalesChannel?` | yes | shared AeroTech sales channel |
| `CustomerId` | `long?` | yes | customer/private availability |
| `OriginCountryId` | `long?` | yes | coverage origin |
| `DestinationCountryId` | `long?` | yes | coverage destination |
| `OriginAirportId` | `long?` | yes | coverage origin |
| `DestinationAirportId` | `long?` | yes | coverage destination |
| `MarketingAirlineId` | `long?` | yes | segment predicate |
| `OperatingAirlineId` | `long?` | yes | segment predicate |
| `FlightNumberFrom` | `int?` | yes | inclusive |
| `FlightNumberTo` | `int?` | yes | inclusive |
| `AircraftId` | `long?` | yes | equipment predicate |
| `CabinClassId` | `int?` | yes | segment/fare predicate |
| `RbdId` | `long?` | yes | segment/fare predicate |
| `FareFamilyId` | `long?` | yes | passenger+air-service fare predicate |
| `AirFareId` | `long?` | yes | passenger+air-service fare predicate |
| `FareBasisPattern` | `string?` | yes | exact or trailing-`*` prefix only |
| `DayOfWeekMask` | `byte?` | yes | Monday..Sunday seven-bit mask |
| `DepartureTimeFrom` | `TimeOnly?` | yes | origin-local |
| `DepartureTimeTo` | `TimeOnly?` | yes | origin-local |

Within one condition, all non-null predicates are AND.

## 9.5 `AncillaryTaxComponent`

| Field | Type | Null | Meaning |
|---|---|---:|---|
| `Id` | `long` | no | child identity |
| `AncillaryProvisionId` | `long` | no | parent |
| `Code` | `string` | no | 2..3 uppercase alphanumeric |
| `Reference` | `string?` | yes | reference-data/external tax reference |
| `Amount` | `decimal(19,4)` | no | per-application amount; >=0 |
| `CurrencyId` | `int` | no | same as Provision CurrencyId in V1 |
| `IsIncludedInFee` | `bool` | no | true when component is already contained in base fee |

V1 has no percentage or tax-on-tax engine.

## 9.6 Provision invariants

1. Provision may Release only when referenced Service is Released.
2. OwnerAirlineId must equal Service OwnerAirlineId.
3. Sales date range must be valid and inside Service sales lifecycle.
4. Travel date range must be ordered.
5. Sequence > 0.
6. MinimumQuantity > 0.
7. MaximumQuantity >= MinimumQuantity.
8. `Charged` requires `Amount > 0` and CurrencyId.
9. `Free` requires Amount/CurrencyId null and must result in customer TotalAmount=0.
10. `Unavailable` requires Amount/CurrencyId null and never emits a sellable candidate/quote item.
11. Positive tax components are not allowed on a Free provision in V1.
12. All tax components use the Provision currency in V1.
13. Sum of tax components marked IncludedInFee must not exceed configured charged Amount per application.
14. FirstOccurrence/LastOccurrence MUST be null for a released V1 Provision.
15. Any attempt to Release them populated fails with `AncillaryOccurrencePricingNotSupported`.
16. `PerPassenger`, `PerPassengerPerSegment`, `PerOrder` require MinimumQuantity=MaximumQuantity=1.
17. Only `PerOccurrence` supports Quantity != 1.
18. `PerOccurrence` is passenger-bound in V1.
19. `PerOrder` has no passenger applicability and no passenger-specific fare predicates in V1.
20. effective EMD-A requires an exact passenger and exact segment coverage in the produced QuoteItem.
21. effective FlightFlowSeat requires exactly one covered flight segment.
22. Suspended/Discontinued Provision is not quoted.
23. Existing Quotes are never rewritten when Provision changes.

---

# 10. Provision enums

## `AncillaryCoverageScope`

```text
Segment
Portion
Journey
```

Definitions:

- Segment = one exact Air segment.
- Portion = one or more caller-declared consecutive Air segments inside one Journey.
- Journey = all segments carrying one JourneyRef.

## `AncillaryApplicationUnit`

```text
PerPassenger
PerPassengerPerSegment
PerOccurrence
PerOrder
```

## `AncillaryFeeDisposition`

```text
Charged
Free
Unavailable
```

## `AncillaryRefundability`

```text
NonRefundable
Refundable
SourceRuleRequired
```

`SourceRuleRequired` is future/source-gated and is never interpreted as permission to refund.

---

# 11. Airline quote policy

## `AncillaryAirlineQuotePolicy`

Domain policy record:

| Field | Type | Null | Rule |
|---|---|---:|---|
| `SellingAirlineId` | `long` | no | one policy per selling airline |
| `QuoteTtl` | `TimeSpan` | no | > 0 |

Behavior:

```text
SetAncillaryQuotePolicy(SellingAirlineId, QuoteTtl)
```

Rules:

1. QuoteTtl must be positive.
2. Selling airline must have an explicit policy before an exact Quote can be created.
3. Caller cannot override QuoteTtl.
4. `ExpiresAt = CreatedAt + QuoteTtl`.
5. Policy changes affect only newly created quotes.
6. Existing quote validity never changes because policy later changes.
7. Missing policy -> `AncillaryQuoteTtlNotConfigured`.
8. Policy changes produce no Ancillary V1 domain event.

---

# 12. Quote/shopping context

Ancillary does not fetch or own complete Order/Flight/AirFare aggregates. Caller supplies an exact authoritative snapshot context.

## 12.1 `AncillarySalesContextSnapshot`

| Field | Type | Null |
|---|---|---:|
| `Channel` | `SalesChannel` | no |
| `ActorId` | `long?` | yes |
| `OfficeId` | `long?` | yes |
| `CustomerId` | `long?` | yes |
| `PointOfSaleCountryId` | `long` | no |
| `RequestedCurrencyId` | `int` | no |
| `SellingAirlineId` | `long` | no |
| `SalesTime` | `LocalDateTimeWithOffset` | no |

Catalog selection in V1:

```text
AncillaryService.OwnerAirlineId == SalesContext.SellingAirlineId
```

Marketing and Operating carrier are applicability facts; they do not choose catalog ownership.

## 12.2 `AncillaryTravellerContext`

| Field | Type | Null |
|---|---|---:|
| `PassengerRef` | `string` | no |
| `SourceOrderTravellerId` | `long?` | yes |
| `PassengerTypeCode` | `PassengerTypeCode` | no |
| `Age` | `int?` | yes |

`PassengerRef` is unique in request context.

## 12.3 `AncillarySegmentContext`

| Field | Type | Null | Rule |
|---|---|---:|---|
| `SegmentRef` | `string` | no | unique within request |
| `SourceOrderServiceId` | `long?` | yes | source Air service |
| `FlightId` | `long` | no | operational flight reference |
| `FlightNumber` | `int?` | yes | marketing/operational matching input |
| `OriginAirportId` | `long` | no | source fact |
| `OriginCountryId` | `long` | no | source fact |
| `DestinationAirportId` | `long` | no | source fact |
| `DestinationCountryId` | `long` | no | source fact |
| `MarketingAirlineId` | `long` | no | source fact |
| `OperatingAirlineId` | `long` | no | source fact |
| `OriginDeparture` | `LocalDateTimeWithOffset` | no | origin-local departure |
| `AircraftId` | `long?` | yes | source-gated equipment context |
| `CabinClassId` | `int?` | yes | source context |
| `RbdId` | `long?` | yes | source context |
| `Sequence` | `int` | no | >0; unique within JourneyRef |
| `JourneyRef` | `string` | no | journey group; non-empty |
| `PortionRef` | `string?` | yes | caller-declared portion; unique across request when supplied |

Rules:

1. SegmentRef unique across request.
2. JourneyRef identifies one logical Journey.
3. Sequence is unique only inside each JourneyRef.
4. PortionRef, when supplied, resolves one exact Portion and therefore cannot be reused by another Portion in the same request.
5. Every Portion belongs to exactly one JourneyRef.
6. A Portion cannot cross JourneyRef boundaries.
7. Segments of one Portion must be consecutive by Sequence within that Journey.
8. Ancillary never creates arbitrary contiguous subsets.

## 12.4 `AncillaryFareContext`

| Field | Type | Null |
|---|---|---:|
| `PassengerRef` | `string` | no |
| `SourceOrderServiceId` | `long` | no |
| `AirFareId` | `long` | no |
| `FareBasis` | `string?` | yes |
| `FareFamilyId` | `long?` | yes |
| `CabinClassId` | `int?` | yes |
| `RbdId` | `long?` | yes |

There may be one applicable FareContext for each passenger + Air service represented in the supplied Order context.

Missing data required by a configured Provision is never inferred.

---

# 13. Coverage generation

## Segment

One candidate coverage for every supplied segment:

```text
CoverageRef = { Scope=Segment, Ref=SegmentRef }
```

## Portion

Only caller-declared portions exist.

```text
CoverageRef = { Scope=Portion, Ref=PortionRef }
```

Rules:

- no automatic contiguous-subset generation;
- missing PortionRef means that segment participates in no Portion candidate;
- malformed/non-consecutive PortionRef -> `InvalidPortionContext`;
- relevant Portion provision with no evaluable declared Portion -> `PortionContextAbsent`;
- Portion never crosses a Journey.

## Journey

One candidate per JourneyRef:

```text
CoverageRef = { Scope=Journey, Ref=JourneyRef }
```

It contains all segments of that Journey ordered by Sequence.

---

# 14. Multi-segment applicability semantics

For each candidate coverage:

## 14.1 Coverage-level predicates

Evaluate once:

- OriginAirportId / OriginCountryId = first covered segment origin.
- DestinationAirportId / DestinationCountryId = last covered segment destination.
- POS/channel/customer = SalesContext.

## 14.2 Segment/fare predicates

The following apply to covered segments:

- MarketingAirlineId;
- OperatingAirlineId;
- FlightNumber range;
- AircraftId;
- CabinClassId;
- RbdId;
- FareFamilyId;
- AirFareId;
- FareBasisPattern;
- DayOfWeekMask;
- DepartureTime range.

For passenger-bound ApplicationUnits, fare predicates use the exact passenger + SourceOrderService fare context.

## 14.3 Include

A multi-segment Include row matches only when:

```text
coverage-level predicates match
AND
ALL covered segments satisfy all non-null segment/fare predicates
```

Include collection semantics:

```text
empty -> no include restriction
non-empty -> at least one Include row must match
```

## 14.4 Exclude

A multi-segment Exclude row matches when:

```text
coverage-level predicates match
AND
ANY covered segment satisfies all non-null segment/fare predicates
```

Any matching Exclude rejects the candidate.

Exclude wins over Include.

## 14.5 Missing context

When a Provision needs a value that the supplied context does not contain:

```text
candidate omitted
+ Diagnostics[ApplicabilityContextMissing]
```

No default/fallback inference is allowed.

---

# 15. Deterministic Provision selection

For every Service + passenger/order application + exact CoverageRef:

1. keep only Released Service/Provisions;
2. validate service sales lifecycle;
3. validate provision sales lifecycle;
4. validate travel lifecycle;
5. evaluate passenger applicability;
6. evaluate Include;
7. evaluate Exclude;
8. reject V1 occurrence-tier rules;
9. suppress FeeDisposition=Unavailable;
10. keep matching Provisions.

When multiple Provisions match:

1. lowest Sequence wins;
2. if two or more matching Provisions have the same lowest Sequence, no candidate is emitted;
3. return `Diagnostics[ProvisionOverlap]`;
4. never choose cheapest;
5. never pick one arbitrarily.

---

# 16. Canonical pricing behavior

## 16.1 ApplicationUnit × CoverageScope matrix

Let `N` be number of exact segments in the coverage.

| ApplicationUnit | Segment | Portion | Journey |
|---|---:|---:|---:|
| `PerPassenger` | `1` | `1` | `1` |
| `PerPassengerPerSegment` | `1` | `N` | `N` |
| `PerOccurrence` | `Quantity` | `Quantity` | `Quantity` |
| `PerOrder` | `1` | `1` | `1` |

`ApplicationCount` is the exact multiplier produced by this matrix.

## 16.2 Quantity rules

- PerPassenger -> exact Quantity=1.
- PerPassengerPerSegment -> exact Quantity=1.
- PerOrder -> exact Quantity=1.
- PerOccurrence -> Quantity within Provision Min/Max.
- PerOccurrence requires exact passenger.
- changing Quantity requires a new exact Quote.
- Ordering never changes Quantity on an existing QuoteItem.

## 16.3 Base price

For Charged:

```text
UnitBaseAmount = Provision.Amount
BaseAmount = UnitBaseAmount × ApplicationCount
```

For Free:

```text
UnitBaseAmount = 0
BaseAmount = 0
TaxAmount = 0
TotalAmount = 0
```

## 16.4 Fixed tax components

For each configured tax component:

```text
CalculatedTaxAmount = TaxComponent.Amount × ApplicationCount
```

Quote tax snapshot `Amount` stores this calculated amount.

```text
TaxAmount =
    sum(all calculated QuoteTax.Amount)

AddedTaxAmount =
    sum(QuoteTax.Amount where IsIncludedInFee=false)

TotalAmount =
    BaseAmount + AddedTaxAmount
```

Therefore:

```text
TotalAmount may be less than BaseAmount + TaxAmount
```

when one or more tax components are already included in BaseAmount.

`TaxAmount` is the aggregate reportable tax total.

Every `AncillaryQuoteTax.Amount` is a distinct historical tax fact for Ordering/accounting projection.

## 16.5 Currency

- each Charged Provision has one CurrencyId;
- all its tax components use that CurrencyId;
- V1 does not convert FX;
- if requested currency differs, candidate is unquoteable;
- return `Diagnostics[CurrencyMismatch]`;
- no guessed rate.

---

# 17. Sellable discovery

## 17.1 `GetSellableAncillariesRequest`

```text
OrderId
OrderCommercialVersion
SalesContext
Travellers[]
Segments[]
FareContexts[]
RequestedSubCodes[]?
```

Rules:

- RequestedSubCodes null/empty = shop all released Services for SellingAirlineId.
- this operation creates no Quote and no acceptance authority.

## 17.2 `SellableAncillaryCandidate`

| Field | Type | Null |
|---|---|---:|
| `AncillaryServiceId` | `long` | no |
| `AncillaryServiceRevision` | `int` | no |
| `AncillaryProvisionId` | `long` | no |
| `AncillaryProvisionRevision` | `int` | no |
| `OwnerAirlineId` | `long` | no |
| `SubCode` | `string` | no |
| `ServiceType` | `AncillaryServiceType` | no |
| `GroupCode` | `string` | no |
| `SubGroupCode` | `string?` | yes |
| `CommercialName` | `string` | no |
| `PassengerRef` | `string?` | yes |
| `SourceOrderTravellerId` | `long?` | yes |
| `CoverageRef` | `AncillaryCoverageRef` | no |
| `SourceOrderServiceIds` | `IReadOnlyCollection<long>` | no |
| `FlightIds` | `IReadOnlyCollection<long>` | no |
| `CoverageScope` | `AncillaryCoverageScope` | no |
| `ApplicationUnit` | `AncillaryApplicationUnit` | no |
| `QuantityUnit` | `AncillaryQuantityUnit` | no |
| `MinimumQuantity` | `decimal(9,3)` | no |
| `MaximumQuantity` | `decimal(9,3)` | no |
| `UnitBaseAmount` | `decimal(19,4)` | no |
| `UnitTaxComponents` | collection `AncillaryUnitTaxComponent` | no |
| `PreviewBaseAmount` | `decimal(19,4)` | no |
| `PreviewTaxAmount` | `decimal(19,4)` | no |
| `PreviewTotalAmount` | `decimal(19,4)` | no |
| `CurrencyId` | `int` | no |
| `Commissionable` | `bool` | no |
| `Refundability` | `AncillaryRefundability` | no |
| `BookingMethod` | `AncillaryBookingMethod` | no |
| `SsrCode` | `string?` | yes |
| `FulfillmentMode` | `AncillaryFulfillmentMode` | no |
| `FulfillmentProviderKey` | `string?` | yes |
| `DocumentRequirement` | `AncillaryDocumentRequirement` | no |
| `ReasonForIssuanceCode` | `string?` | yes |
| `ReasonForIssuanceSubCode` | `string?` | yes |
| `BaggageDefinition` | `AncillaryQuoteBaggage?` | yes |

## 17.3 `AncillaryUnitTaxComponent`

Same commercial shape as Quote tax, but Amount is the configured **per-application** amount before multiplication.

| Field | Type | Null |
|---|---|---:|
| `Code` | `string` | no |
| `Reference` | `string?` | yes |
| `Amount` | `decimal(19,4)` | no |
| `CurrencyId` | `int` | no |
| `IsIncludedInFee` | `bool` | no |

## 17.4 Preview semantics

Discovery preview is calculated for:

```text
PreviewQuantity = 1
```

and the canonical matrix.

For a PerOccurrence Provision whose MinimumQuantity > 1:

- preview remains a per-one-occurrence price preview;
- Min/Max tells the caller valid purchasable quantities;
- preview itself cannot be accepted.

## 17.5 NoMatchingProvision diagnostic scope

When RequestedSubCodes is explicit:

- emit one `NoMatchingProvision` diagnostic for each requested SubCode with no sellable candidate;
- Diagnostic.SubCode MUST be populated.

When shop-all is requested:

- if no sellable candidate exists at all, emit one `NoMatchingProvision` diagnostic with no SubCode;
- do not emit one diagnostic per catalog service.

---

# 18. Aggregate Root — AncillaryQuote

## 18.1 Purpose

A Quote is the immutable exact commercial authority Ordering may accept.

It protects against:

- catalog/provision changes after customer selection;
- price races;
- order changes;
- quantity substitution;
- ambiguous coverage;
- recalculation in Ordering.

## 18.2 `AncillaryQuote` fields

| Field | Type | Null | Meaning |
|---|---|---:|---|
| `Id` | `long` | no | aggregate identity |
| `QuoteReference` | `Guid` | no | public opaque identity |
| `OrderId` | `long` | no | V1 source Order |
| `OrderCommercialVersion` | `int` | no | Order version bound to quote |
| `CurrencyId` | `int` | no | exact quote currency |
| `CreatedAt` | `DateTimeOffset` | no | creation instant |
| `ExpiresAt` | `DateTimeOffset` | no | exact validity deadline |
| `State` | `AncillaryQuoteState` | no | Active/Revoked |
| `SalesContext` | `AncillarySalesContextSnapshot` | no | immutable |
| `Items` | collection `AncillaryQuoteItem` | no | one or more |

Validity:

```text
State == Active
AND now < ExpiresAt
```

There is no Consumed state; Ordering owns purchase/idempotency.

## 18.3 `AncillaryQuoteItem`

| Field | Type | Null | Meaning |
|---|---|---:|---|
| `Id` | `long` | no | exact acceptance item identity |
| `AncillaryQuoteId` | `long` | no | parent |
| `AncillaryServiceId` | `long` | no | provenance |
| `AncillaryServiceRevision` | `int` | no | provenance |
| `AncillaryProvisionId` | `long` | no | provenance |
| `AncillaryProvisionRevision` | `int` | no | provenance |
| `OwnerAirlineId` | `long` | no | service-owner snapshot |
| `SubCode` | `string` | no | snapshot |
| `ServiceType` | `AncillaryServiceType` | no | snapshot |
| `GroupCode` | `string` | no | snapshot |
| `SubGroupCode` | `string?` | yes | snapshot |
| `CommercialName` | `string` | no | snapshot |
| `PassengerRef` | `string?` | yes | exact passenger scope |
| `SourceOrderTravellerId` | `long?` | yes | Ordering mapping |
| `CoverageRef` | `AncillaryCoverageRef` | no | exact typed coverage |
| `SourceOrderServiceIds` | collection `long` | no | exact Air-service coverage |
| `FlightIds` | collection `long` | no | exact operational coverage |
| `CoverageScope` | `AncillaryCoverageScope` | no | snapshot |
| `ApplicationUnit` | `AncillaryApplicationUnit` | no | snapshot |
| `Quantity` | `decimal(9,3)` | no | exact immutable selected quantity |
| `QuantityUnit` | `AncillaryQuantityUnit` | no | snapshot |
| `ApplicationCount` | `decimal(9,3)` | no | exact multiplier used |
| `UnitBaseAmount` | `decimal(19,4)` | no | per-application price |
| `BaseAmount` | `decimal(19,4)` | no | calculated |
| `TaxAmount` | `decimal(19,4)` | no | all tax components |
| `TotalAmount` | `decimal(19,4)` | no | authoritative customer total |
| `CurrencyId` | `int` | no | quote currency |
| `TaxComponents` | collection `AncillaryQuoteTax` | no | immutable |
| `Commissionable` | `bool` | no | snapshot |
| `Refundability` | `AncillaryRefundability` | no | snapshot |
| `BookingMethod` | `AncillaryBookingMethod` | no | effective snapshot |
| `SsrCode` | `string?` | yes | effective snapshot |
| `FulfillmentMode` | `AncillaryFulfillmentMode` | no | effective snapshot |
| `FulfillmentProviderKey` | `string?` | yes | effective snapshot |
| `DocumentRequirement` | `AncillaryDocumentRequirement` | no | effective snapshot |
| `ReasonForIssuanceCode` | `string?` | yes | RFIC snapshot |
| `ReasonForIssuanceSubCode` | `string?` | yes | RFISC snapshot |
| `BaggageDefinition` | `AncillaryQuoteBaggage?` | yes | purchased entitlement |

## 18.4 `AncillaryQuoteTax`

| Field | Type | Null | Meaning |
|---|---|---:|---|
| `Code` | `string` | no | tax code |
| `Reference` | `string?` | yes | reference |
| `Amount` | `decimal(19,4)` | no | calculated amount after ApplicationCount |
| `CurrencyId` | `int` | no | quote currency |
| `IsIncludedInFee` | `bool` | no | explains total construction |

## 18.5 `AncillaryQuoteBaggage`

| Field | Type | Null |
|---|---|---:|
| `BaggageKind` | `AncillaryBaggageKind` | no |
| `PieceCount` | `int?` | yes |
| `WeightPerPiece` | `decimal(9,3)?` | yes |
| `TotalWeight` | `decimal(9,3)?` | yes |
| `WeightUnit` | `WeightUnit?` | yes |
| `IsPrepaid` | `bool` | no |

## 18.6 Quote state

```text
Active
Revoked
```

Expiry is a time-derived eligibility fact, not a separate lifecycle state.

Old active quote remains valid until ExpiresAt unless explicitly revoked, even if Service/Provision later becomes Suspended/Discontinued.

Material product withdrawal requiring immediate stop uses explicit Quote revocation.

---

# 19. Exact Quote creation

## 19.1 `CreateAncillaryQuoteRequest`

```text
OrderId
OrderCommercialVersion
SalesContext
Travellers[]
Segments[]
FareContexts[]
RequestedItems[]
```

`RequestedItems` is required and non-empty.

There is no `RequestedSubCodes` on CreateQuote.

## 19.2 `AncillaryRequestedItem`

| Field | Type | Null |
|---|---|---:|
| `AncillaryServiceId` | `long` | no |
| `PassengerRef` | `string?` | yes |
| `CoverageRef` | `AncillaryCoverageRef` | no |
| `Quantity` | `decimal(9,3)` | no |

## 19.3 Duplicate selection rule

Two RequestedItems are duplicates when these are equal:

```text
AncillaryServiceId
PassengerRef
CoverageRef
```

Duplicate request is structurally invalid:

```text
AncillaryContextInvalid
```

A customer requiring multiple occurrences expresses them as one RequestedItem with the intended Quantity.

## 19.4 All-or-nothing Quote creation

CreateAncillaryQuote is atomic at the business-selection level.

For every RequestedItem Ancillary must independently prove:

- Service is released and belongs to SellingAirlineId;
- exact CoverageRef resolves;
- passenger relation is valid;
- one deterministic Provision applies;
- Quantity is valid;
- currency is quoteable;
- required applicability context exists;
- effective booking/fulfillment/document metadata is valid;
- exact price can be calculated.

If **any** RequestedItem cannot be quoted:

```text
Quote = null
Diagnostics[] = all deterministic diagnostics for invalid requested items
```

and no partial Quote is created.

Rationale:

> Ordering submitted an explicit customer selection. Silently removing one requested item would change the customer's commercial intent.

A successful Quote contains exactly one QuoteItem corresponding to each RequestedItem.

---

# 20. Quote acceptance validation

Canonical read behavior:

```text
ValidateQuoteItemAcceptance(
    QuoteReference,
    QuoteItemId,
    OrderId,
    OrderCommercialVersion
)
```

It validates:

1. Quote exists;
2. State=Active;
3. now < ExpiresAt;
4. Quote.OrderId matches;
5. OrderCommercialVersion matches;
6. QuoteItem belongs to Quote.

It returns the exact immutable QuoteItem snapshot.

It never:

- receives a replacement Quantity;
- recalculates price from current Provision;
- changes coverage;
- reprices;
- mutates Quote.

Ordering must use QuoteItem.Quantity and QuoteItem monetary facts as supplied.

---

# 21. Service-specific airline behavior

## 21.1 Paid seat

Ancillary owns:

- seat ancillary product identity;
- commercial eligibility;
- price;
- EMD metadata.

FlightFlow owns:

- seat map;
- specific seat operational availability;
- hold/confirm/release;
- operational SeatNumber state.

Canonical flow:

```text
discover paid-seat ancillary
-> exact quote for passenger + one Segment
-> Ordering accepts commercial quote
-> channel/Ordering supplies requested SeatNumber where supported
-> FlightFlow performs operational mutation
-> Ordering owns paid seat history
-> Ordering issues EMD-A if required
```

Rules:

- Ancillary Quote is not proof that SeatNumber is available.
- price-by-window/aisle/exit-row remains source-gated until authoritative seat attributes are available.
- effective FlightFlowSeat coverage is exactly one segment.

## 21.2 Prepaid/additional checked baggage

Ancillary owns:

- purchased baggage product;
- entitlement;
- eligibility;
- price;
- EMD metadata.

Included fare allowance remains a Fare/Order air-service entitlement and is never converted into an ancillary.

Example:

```text
included fare allowance: 1 x 23kg
purchased ancillary:    +1 x 23kg
```

Both must coexist in Order history.

Operational bag acceptance remains DCS/provider responsibility.

## 21.3 Carry-on / excess / oversize / special baggage

Commercial product can be represented using `AncillaryBaggageDefinition`.

V1 does not infer operational acceptance rules not provided by authoritative upstream context.

## 21.4 Meal / inflight service

Supported:

- catalog definition;
- passenger/flight applicability;
- pricing;
- NoBookingRequired;
- SSR metadata;
- EMD metadata if required.

Actual SSR provider execution remains source-gated.

## 21.5 Priority / airport / lounge

Supported:

- passenger/context eligibility;
- price;
- EMD metadata;
- no-booking or external-provider requirement.

Actual delivery/consumption remains external.

## 21.6 Standalone service

May be:

- `PerOrder`;
- EMD-S;
- not flight-associated;
- no Air-service coverage only when the Service semantics legitimately require none.

## 21.7 Free ancillary

Free means:

```text
BaseAmount=0
TaxAmount=0
TotalAmount=0
```

It may still require EMD-A or EMD-S.

## 21.8 Codeshare

Catalog selection uses SellingAirlineId.

MarketingAirlineId and OperatingAirlineId participate only in Provision applicability.

Ancillary does not infer validating/interline concurrence in V1.

---

# 22. Booking and fulfillment semantics

Effective values are:

```text
Provision override ?? Service default
```

for:

- BookingMethod;
- FulfillmentMode;
- FulfillmentProviderKey;
- DocumentRequirement.

## None
No provider reservation is required by Ancillary metadata.

## FlightFlowSeat
Ordering invokes the existing FlightFlow fulfillment behavior. Ancillary never mutates FlightFlow.

## Ssr
Quote carries SsrCode/booking metadata. No SSR success is invented without an authoritative provider.

## ExternalProvider
Quote carries FulfillmentProviderKey. Provider behavior remains source-gated.

## LocalQuota
DEFER. No V1 quota behavior exists.

---

# 23. EMD/document semantics

Ancillary supplies document requirements, not accountable documents.

## EMD-A QuoteItem requires

- exact PassengerRef;
- exact flight/Air-service coverage;
- RFIC;
- RFISC;
- enough exact coverage for Ordering to associate the service with the applicable ET coupon.

## EMD-S

- ET coupon association not required.

Ordering owns:

- EMD identity/document number;
- stock;
- issuing/validating facts;
- document/coupon state;
- issue/void/refund/exchange lifecycle.

Quote metadata remains historical commercial evidence after document state changes.

---

# 24. Lifecycle

## 24.1 AncillaryService

```text
Draft -> Released -> Suspended -> Released
Draft -> Discontinued
Released -> Discontinued
Suspended -> Discontinued
```

Discontinued is terminal.

## 24.2 AncillaryProvision

Same state transitions.

## 24.3 AncillaryQuote

```text
Active -> Revoked
```

Natural expiry occurs when `now >= ExpiresAt`.

No edit/refresh-in-place behavior exists. Repricing creates a new Quote.

---

# 25. Revision semantics

Revision is business provenance, distinct from lifecycle state.

## 25.1 Service material changes

Increment Revision once per successful change affecting any:

- GroupCode;
- SubGroupCode;
- CommercialName;
- IsIndustryDefined when still legally mutable;
- BookingMethod;
- SsrCode;
- DefaultFulfillmentMode;
- FulfillmentProviderKey;
- DefaultDocumentRequirement;
- RFIC/RFISC;
- QuantityUnit;
- SalesEffectiveFrom/SalesDiscontinueOn;
- baggage-definition fields.

Non-material:

- Description-only change;
- media-only change;
- lifecycle transition.

OwnerAirlineId/SubCode/ServiceType are immutable after first Release.

## 25.2 Provision material changes

Every change to commercial Provision definition is material and increments Revision, including:

- sales/travel dates;
- CoverageScope;
- ApplicationUnit;
- quantity bounds;
- fee/currency;
- commission/refundability;
- booking/document/fulfillment overrides;
- passenger applicability;
- Include/Exclude conditions;
- tax components;
- Sequence.

Lifecycle transition alone does not increment Revision.

## 25.3 Historical rule

Old Quotes preserve their own snapshot and revisions. A current catalog/provision change never rewrites accepted historical facts.

---

# 26. Domain events

Canonical V1 domain event names:

```text
AncillaryServiceDefined
AncillaryServiceChanged
AncillaryServiceReleased
AncillaryServiceSuspended
AncillaryServiceResumed
AncillaryServiceDiscontinued

AncillaryProvisionDefined
AncillaryProvisionChanged
AncillaryProvisionReleased
AncillaryProvisionSuspended
AncillaryProvisionResumed
AncillaryProvisionDiscontinued

AncillaryQuoteCreated
AncillaryQuoteRevoked
```

Child edits are represented by parent Changed events.

## `AncillaryServiceChanged`

Minimum business payload:

```text
AncillaryServiceId
Revision
IsMaterialChange
Snapshot
OccurredAt
```

Rules:

- any successful ChangeAncillaryService emits exactly one Changed event;
- material change increments Revision before event;
- Description-only change emits event with unchanged Revision and `IsMaterialChange=false`;
- lifecycle commands emit lifecycle events, not Changed.

## `AncillaryProvisionChanged`

Any successful commercial Provision change:

- increments Revision;
- emits one Changed event with current full commercial snapshot.

`SetAncillaryQuotePolicy` has no V1 domain event.

No other business event name may be invented as part of V1 domain behavior.

---

# 27. Diagnostics

## 27.1 `AncillaryDiagnostic`

| Field | Type | Null |
|---|---|---:|
| `Code` | `AncillaryDiagnosticCode` | no |
| `AncillaryServiceId` | `long?` | yes |
| `SubCode` | `string?` | yes |
| `ProvisionId` | `long?` | yes |
| `PassengerRef` | `string?` | yes |
| `CoverageRef` | `AncillaryCoverageRef?` | yes |

No localized human text is domain authority.

Diagnostics deduplicate by:

```text
(Code,
 AncillaryServiceId,
 ProvisionId,
 PassengerRef,
 CoverageRef)
```

## 27.2 Diagnostic codes

```text
ProvisionOverlap
CurrencyMismatch
ApplicabilityContextMissing
AgeContextRequired
InvalidPortionContext
PortionContextAbsent
RequestedQuantityOutOfRange
NoMatchingProvision
```

## 27.3 `PortionContextAbsent`

Returned when a relevant Portion Provision cannot be evaluated because caller supplied no valid declared Portion candidate.

It never silently falls back to Segment or Journey.

---

# 28. Fatal business errors

The following are fatal domain/business errors when their applicable operation requires the fact:

```text
AncillaryServiceNotFound
AncillaryServiceNotReleased
AncillaryServiceIdentityImmutable

AncillaryProvisionNotFound
AncillaryProvisionNotReleased
AncillaryProvisionInvalidDateRange
AncillaryProvisionInvalidQuantity
AncillaryProvisionOverlap
AncillaryOccurrencePricingNotSupported

AncillaryContextInvalid
AncillaryContextContradictory

AncillaryQuoteTtlNotConfigured
AncillaryQuoteNotFound
AncillaryQuoteExpired
AncillaryQuoteRevoked
AncillaryQuoteOrderMismatch
AncillaryQuoteCommercialVersionMismatch
AncillaryQuoteItemNotFound

AncillaryEmdMetadataRequired
AncillarySsrCodeRequired
AncillaryFulfillmentProviderRequired
```

Operation semantics take precedence over generic error naming:

- runtime CurrencyMismatch is a non-fatal Diagnostic, not a fatal error;
- runtime ambiguous same-Sequence match returns `ProvisionOverlap` Diagnostic;
- exact CreateQuote requested-item failures return Diagnostics and no Quote under the all-or-nothing rule;
- structurally malformed/contradictory request remains fatal;
- a configuration/management operation that can prove invalid overlap may use `AncillaryProvisionOverlap` as fatal validation.

---

# 29. Canonical domain operations

## Quote policy

```text
SetAncillaryQuotePolicy
```

## Service lifecycle

```text
DefineAncillaryService
ChangeAncillaryService
ReleaseAncillaryService
SuspendAncillaryService
ResumeAncillaryService
DiscontinueAncillaryService
```

## Provision lifecycle

```text
DefineAncillaryProvision
ChangeAncillaryProvision
ReleaseAncillaryProvision
SuspendAncillaryProvision
ResumeAncillaryProvision
DiscontinueAncillaryProvision
```

## Shopping / quote

```text
GetSellableAncillaries
CreateAncillaryQuote
GetAncillaryQuote
ValidateQuoteItemAcceptance
RevokeAncillaryQuote
```

There is no Ancillary-domain operation named:

```text
ReserveAncillary
ConfirmAncillary
IssueAncillary
PayAncillary
RefundAncillary
```

Those behaviors belong to other bounded contexts.

---

# 30. Historical consistency

Every exact QuoteItem must remain sufficient to explain:

- which airline/service/provision was sold;
- catalog and provision revision;
- SubCode/classification/name;
- passenger;
- exact transport coverage;
- quantity and quantity unit;
- application unit/count;
- base/tax/total price;
- currency;
- included-vs-added tax composition;
- commissionability;
- refundability;
- booking method;
- fulfillment requirement;
- EMD requirement/RFIC/RFISC;
- purchased baggage entitlement where applicable.

Changing current Service/Provision definition never changes old Quote history.

Hard deletion of released business meaning is not part of V1 lifecycle; Discontinued represents end-of-sale.

---

# 31. Canonical end-to-end flows

## A01 — Define paid baggage
Define baggage Service + entitlement -> define Provision -> Release both -> matching discovery exposes candidate.

## A02 — Shop baggage for existing Order
Caller supplies exact passenger/segment/fare/sales context -> candidate returns passenger, CoverageRef, entitlement, preview price and EMD metadata.

## A03 — Exact baggage Quote
Customer selects exact bag + passenger + coverage + quantity -> exact Quote created -> price/terms immutable.

## A04 — Included vs purchased baggage
Fare-included allowance remains Air-service entitlement; purchased bag is a separate ancillary commercial service.

## A05 — Paid seat
Discover commercial paid seat -> Quote exact passenger+segment -> Ordering accepts -> FlightFlow handles seat operation -> EMD-A where required.

## A06 — Free EMD ancillary
Free Provision -> total zero -> Quote still carries EMD metadata -> Ordering may issue zero-value EMD under its document rules.

## A07 — Free no-document ancillary
Free Provision + None document -> zero-value purchased service; no document requirement.

## A08 — Standalone EMD-S
Standalone Service -> EMD-S -> no ET coupon association.

## A09 — Include/Exclude
Include matches but one Exclude row matches any covered segment -> candidate suppressed.

## A10 — Agency/private ancillary
Channel/customer/POS restriction matches only eligible agency/customer context.

## A11 — Passenger restriction
ADT-only product omitted for CHD/INF. Age-specific rule with unknown age returns AgeContextRequired.

## A12 — Provision precedence
Multiple matching provisions -> lowest Sequence. Tie at lowest Sequence -> no candidate + ProvisionOverlap diagnostic.

## A13 — Price changes after Quote
Q1 remains old exact price until expiry/revocation; Q2 uses new Provision revision.

## A14 — Order changes after Quote
CommercialVersion mismatch prevents acceptance.

## A15 — Quote expires
At `now >= ExpiresAt`, acceptance fails; new Quote required.

## A16 — Service/Provision suspended
No new discovery/Quote. Existing active Quote remains valid unless explicitly revoked.

## A17 — Currency mismatch
No FX -> affected candidate omitted + CurrencyMismatch diagnostic.

## A18 — SSR product
Quote carries SSR code/booking requirement; no external success inferred.

## A19 — Seat operational refusal
Commercial Quote can be valid while FlightFlow later refuses a requested seat. Ancillary commercial authority does not become operational availability authority.

## A20 — Ordering idempotency
Repeated acceptance of same QuoteReference/QuoteItem does not create duplicate purchase in Ordering. Purchase idempotency belongs to Ordering.

## A21 — Portion product
Caller declares a PortionRef over consecutive segments in one Journey -> Portion Provision evaluated exactly over that scope -> no alternative subset generated.

## A22 — Missing Portion context
Portion Provision exists but no declared PortionRef -> no candidate + PortionContextAbsent.

## A23 — Multi-segment Include
Include carrier/fare predicates must hold for ALL segments in Portion/Journey.

## A24 — Multi-segment Exclude
A matching Exclude on ANY covered segment rejects candidate.

## A25 — PerPassengerPerSegment pricing
Three-segment Portion -> ApplicationCount=3 -> configured fee and tax components multiplied by 3.

## A26 — PerOrder pricing
Four passengers do not multiply PerOrder fee. ApplicationCount=1 and no passenger binding.

## A27 — PerOccurrence quantity
Customer chooses Quantity=2 within Min/Max -> ApplicationCount=2 -> exact QuoteItem quantity=2.

## A28 — Quantity change
Customer changes 2 to 3 -> old QuoteItem cannot be modified; new Quote required.

## A29 — Multi-item exact selection failure
Customer asks for Bag + Seat. Bag is valid; Seat requested item is not quoteable -> no partial Quote; response contains diagnostics for failed selection.

## A30 — Duplicate requested item
Same Service+Passenger+CoverageRef repeated -> AncillaryContextInvalid.

## A31 — Codeshare
Selling airline selects catalog; marketing/operating carrier predicates decide Provision applicability.

## A32 — Multi-journey Order
Sequence restarts independently within each JourneyRef; Portion remains inside one Journey and cannot cross it.

## A33 — Included tax
Included tax is recorded in TaxAmount and QuoteTax but not added a second time to TotalAmount.

## A34 — Quote policy absent
Selling airline without QuoteTtl policy cannot produce exact Quote.

## A35 — Policy changed after Quote
Existing Quote keeps original ExpiresAt; new Quotes use new TTL.

## A36 — Product withdrawn immediately
Suspension/discontinue blocks new Quotes; if active quotes must also stop, they are explicitly revoked.

---

# 32. Stress/conformance scenarios

Minimum domain conformance must cover:

1. 100 passengers × 8 segments with no passenger/coverage leakage.
2. multiple JourneyRefs with independent Sequence ranges.
3. valid and invalid PortionRef contiguity.
4. Portion not crossing JourneyRef.
5. Include ALL vs Exclude ANY over a 3-segment Portion.
6. fare predicate differing on one segment of Portion.
7. same SubCode across two SellingAirlines without collision.
8. codeshare marketing vs operating carrier restrictions.
9. ADT/CHD/INF mixed eligibility.
10. age-required rule with Age=null.
11. private customer/agency/channel restriction.
12. 20 overlapping provisions with deterministic lowest-Sequence winner.
13. same-lowest-Sequence ambiguity.
14. PerPassenger Segment/Portion/Journey pricing.
15. PerPassengerPerSegment N multiplier.
16. PerOccurrence quantity boundaries.
17. PerOrder with multiple passengers.
18. requested quantity outside bounds.
19. duplicate RequestedItems.
20. all-or-nothing multi-item Quote.
21. exact expiry boundary.
22. CommercialVersion change immediately before acceptance.
23. Service revision while Quote active.
24. Provision revision while Quote active.
25. Suspended/Discontinued Service with active Quote.
26. explicit Quote revocation.
27. missing quote policy.
28. changed QuoteTtl with old Quote unchanged.
29. included tax not double-counted.
30. mixed included/non-included tax reconstruction.
31. currency mismatch with no FX.
32. free ancillary with EMD-A.
33. free ancillary with no document.
34. EMD-A without exact passenger/segment fails.
35. EMD-S standalone.
36. SSR without SsrCode cannot Release effective definition.
37. ExternalProvider without provider key cannot Release effective definition.
38. FlightFlowSeat with non-Segment coverage cannot produce QuoteItem.
39. included baggage and purchased baggage coexist distinctly.
40. paid-seat commercial success followed by operational seat refusal.
41. Description-only Service change emits non-material Changed event.
42. IsIndustryDefined change after first Release fails.
43. FirstOccurrence/LastOccurrence populated on V1 Release fails.
44. local SalesTime date boundaries around midnight/UTC offset.
45. origin-local DOW/time rules across different UTC offsets.
46. NoMatchingProvision diagnostic per explicitly requested SubCode.
47. shop-all with zero candidates emits one global NoMatchingProvision diagnostic.
48. historical Quote remains explainable after catalog discontinuation.

---

# 33. Domain conformance matrix

| Capability | V1 |
|---|---|
| service/SubCode catalog | NOW |
| carrier-defined subcode | NOW |
| Group/SubGroup | NOW |
| RFIC/RFISC | NOW |
| SSR booking metadata | NOW |
| passenger/PTC applicability | NOW |
| age applicability | NOW |
| POS/channel/customer applicability | NOW |
| origin/destination applicability | NOW |
| marketing/operating carrier applicability | NOW |
| flight-number applicability | NOW |
| equipment applicability | NOW |
| cabin/RBD/fare-family/AirFare applicability | NOW |
| FareBasis exact/prefix | NOW |
| sales/travel date | NOW |
| origin-local DOW/time | NOW |
| Segment coverage | NOW |
| caller-declared Portion coverage | NOW |
| Journey coverage | NOW |
| PerPassenger pricing | NOW |
| PerPassengerPerSegment pricing | NOW |
| PerOccurrence fixed-unit pricing | NOW |
| PerOrder pricing | NOW |
| fixed ancillary fee | NOW |
| configured fixed tax components | NOW |
| included vs added tax history | NOW |
| prepaid/additional baggage | NOW |
| paid-seat commercial pricing | NOW |
| free ancillary | NOW |
| immutable exact Quote | NOW |
| all-or-nothing multi-item Quote | NOW |
| quote TTL per selling airline | NOW |
| quote CommercialVersion binding | NOW |
| codeshare applicability | NOW |
| exact acceptance validation | NOW |
| FlightFlow seat/resource inventory | EXTERNAL |
| included fare baggage allowance | EXTERNAL |
| EMD issue/lifecycle | EXTERNAL — Ordering |
| ETKT | EXTERNAL — Ordering |
| payment | EXTERNAL |
| delivery/consumption | EXTERNAL |
| local ancillary quota | DEFER |
| dynamic ancillary pricing | DEFER |
| full tax engine | DEFER |
| FX conversion | DEFER |
| tiered First/LastOccurrence pricing | DEFER |
| interline concurrence | DEFER |
| bundles | DEFER |
| points/miles price | DEFER |
| disruption reaccommodation | DEFER |
| ancillary exchange/refund calculation | DEFER |
| maker-checker release | DEFER |

---

# 34. What Ancillary must never infer

Ancillary MUST NOT infer:

- physical seat availability;
- current FlightFlow capacity;
- ETKT validity;
- EMD issuance/state;
- payment success;
- refund authorization;
- exchange permission;
- currency FX;
- absent fare context;
- absent airport/country mapping;
- passenger Age from PTC;
- Portion boundaries;
- Journey boundaries not supplied by caller;
- validating/interline concurrence;
- SSR provider success;
- operational delivery;
- previously purchased occurrence count.

Missing authority results in a defined diagnostic/error or deferred behavior, never a guess.

---

# 35. Frozen V1 owner decisions

1. Bounded context name is `AeroTech.Ancillary`.
2. Ancillary owns commercial catalog/sellability/pricing, not operational inventory.
3. V1 has exactly three Aggregate Roots: Service, Provision, Quote.
4. FlightFlow remains seat/flight resource authority.
5. AirPrice remains AirFare/fare-rule authority.
6. Ordering remains purchased-service and ETKT/EMD authority.
7. V1 shopping starts from an existing Order.
8. Service/Provision split follows S5/S7-like semantics.
9. exact Quote is immutable acceptance authority.
10. discovery preview is not acceptance authority.
11. Quote is bound to OrderCommercialVersion.
12. Quantity is exact in QuoteItem and cannot be altered at acceptance.
13. exact multi-item Quote is all-or-nothing.
14. no FX guessing.
15. purchased baggage is separate from included fare baggage.
16. paid-seat product is Ancillary commercial truth; physical seat state is FlightFlow truth.
17. EMD requirement/RFIC/RFISC are Ancillary commercial metadata; document lifecycle is Ordering.
18. no local quota inventory in V1.
19. FirstOccurrence/LastOccurrence tier pricing is DEFER.
20. Portion is caller-declared only.
21. Include uses ALL covered segments; Exclude uses ANY.
22. SellingAirlineId selects catalog in codeshare contexts.
23. business-local Sales/Departure time uses `LocalDateTimeWithOffset`.
24. tax inclusion authority exists only per tax component.
25. QuoteTtl is explicit per SellingAirline domain policy.
26. no maker-checker workflow in V1.
27. no full ATPCO clone; only business-relevant optional-service semantics are represented.

---

# 36. Benchmark traceability

## ATPCO Optional Services

V1 preserves the benchmarked separation of:

- Service/SubCode identity;
- Group/SubGroup classification;
- Provision/applicability;
- passenger restrictions;
- geography/flight/equipment/fare context;
- Booking Method/SSR;
- sales/travel dates;
- fixed optional-service fee;
- baggage charge vs baggage allowance;
- occurrence semantics as a recognized but deferred tiering capability.

## IATA EMD

V1 preserves:

- EMD-A vs EMD-S;
- RFIC;
- RFISC;
- accountable-document requirement separate from catalog/pricing;
- passenger/flight association needed for EMD-A.

## Amadeus/Sabre public behavior

V1 preserves the industry separation between:

- ancillary merchandising/pricing;
- airline Order ownership;
- operational inventory/fulfillment;
- document processing;
- payment/delivery.

---

# 37. Final domain baseline

The V1 domain is complete when a caller can supply an authoritative existing-Order sales/passenger/flight/fare context and Ancillary can deterministically answer:

> Which optional services are commercially sellable, to which passenger/order scope, over which exact Segment/Portion/Journey coverage, in what quantity, at what exact price, under what booking/fulfillment/document requirements, and for how long that exact commercial decision may be accepted.

No implementation is allowed to introduce a new domain aggregate, field meaning, lifecycle state, pricing rule, coverage rule, error/diagnostic semantic, or ownership boundary not defined by this Master without Owner-approved domain amendment.

---

**END — AeroTech.Ancillary Master Domain ADR/PRD v1.1 FINAL**
