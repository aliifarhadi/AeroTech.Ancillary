# 01 - P3.1 Canonical ShoppingContext: complete field and rule evidence contract

**Implementation gate:** P3.1 only. **NOT an AirAvail/Ordering wire DTO.** The only consumer of this data model is Shopping evaluation, selection evaluation and result assembly. Input-source conversion is P3.2, deliberately absent here.

## 1. Context purpose and boundaries

A canonical Context carries **verified commercial facts**, not an HTTP request copy, the entire Offer/Order aggregate, a mutable catalog, an unauthenticated traveller PII bag or supplier/provider reservation state. It must provide the attributes used by existing 10 Provision rule groups, Definition specification checks, rate selectors, coverage construction and optional usage evidence. The engine obtains active definitions/provisions/rates/policies through reader ports instead of storing catalog content on Context.

Context members are proposed Phase3 C# *logical records* with type conventions; use existing solution enum contracts where semantic identity already exists, and do NOT create duplicate enums with guessed ordinals. `*`=required; `?`=optional **only if the dependent rule does not require it**. If a rule needs missing input, outcome is `InsufficientContext`/`RequiresVerification`, NEVER silent pass, implicit zero, inferred adult or false availability.

```
AncillaryShoppingContext
  ContextSchemaVersion: int (=1)*
  OwnerAirlineId: int*
  PointOfSaleId: long*
  CustomerId: long?
  CustomerType: existing typed customer-type enum? (required by matching customer-type rule)
  ShoppingStage: PreOrder|PostTicketed|OnBoard* (evaluation value, NOT v12.2 Provision `Both` or `LegacyUnspecified`)
  EvaluatedAtUtc: DateTimeOffset* (UTC from injected clock; never from client)
  CurrencyId: int* (requested sales currency; NOT an FX instruction)
  SourceIdentity: SourceIdentityContext*
  Travellers: IReadOnlyList<ShoppingTraveller>* (unique references)
  Portions: IReadOnlyList<ShoppingPortion>*
  Flights: IReadOnlyList<ShoppingFlight>*
  TravellerFareFacts: IReadOnlyList<TravellerFareFacts>*
  TravellerBaggageFacts: IReadOnlyList<TravellerFlightBaggageFacts> (may be empty = UNKNOWN, not zero)
  FareEntitlementFacts: IReadOnlyList<FareBenefitFacts> (may be empty = UNKNOWN)
  ExistingServiceFacts: IReadOnlyList<ExistingAncillaryServiceFacts> (may be empty = NOT PROVIDED unless explicitly complete)
  UsageEvidence: IReadOnlyList<VerifiedUsageEvidence> (optional, only certified records may prove remaining entitlement)
  CoverageCompleteness: FactCompleteness*
```

`SourceIdentityContext` is source-independent evidence metadata, NOT internal owner IDs exposed to public shoppers:
```
  SourceKind: Offer|Order|Direct
  TrustedSourceReference: string? // opaque origin identifier, not rule-engine input
  TrustedSourceVersion: string? // for revalidation, not a replacement for provider version proof
  OrderId: long? // only if real Order; pre-Order MUST be null
  OrderCommercialVersion: string? // if real Order
  TicketedAtUtc: DateTimeOffset? // required only by applicable SameTimeAsTicketed restriction
  ValidUntilUtc: DateTimeOffset? // upstream Offer/quote validity; never extend automatically
```

`FactCompleteness` holds narrow flags `TravellersComplete`, `ItineraryComplete`, `FareFactsComplete`, `BaggageFactsComplete`, `FareBenefitsComplete`, `ExistingServicesComplete`, and provenance of each; **no defaulting an omitted array to a factual zero**. It must not be used to bypass any published rule. `CallerAuthentication/Identity` and policy authorization are adapter/application concerns (P3.2), not shopper-provided values.

### ShoppingTraveller

| Member | Type | Requirement / meaning |
|---|---|---|
| `TravellerRef` | `string`* | Unique in Context, stable across selected flight/coupon references within this shopping call |
| `OrderTravellerId` | `long?` | Only for a real Order; required by existing `PassengerUsageLimit.PerOrder` key, absent pre-Order |
| `PassengerTypeCode` | existing typed PTC* | Must map to published PassengerEligibility and Rate selector without guessing |
| `DateOfBirth` | `DateOnly?` | Needed to calculate verified age at flight/service date; not mandatory for PTC-only rules |
| `VerifiedAgeAtTravel` | `int?` | May be used instead of DOB if calculated by trusted upstream from actual applicable service date; provenance required |
| `AgeEvidenceAsOfDate` | `DateOnly?` | Required if VerifiedAgeAtTravel present; age without as-of-date may not satisfy a date-scoped restriction |
| `AssociatedAdultRef` | `string?` | Bassinet/infant eligibility and guardian relationship; never guess |
| `StableTravellerIdentity` | `string?` | Only verified stable opaque identity for cross-order usage; NOT passport number/email/raw PII and NOT synthesized from transient `TravellerRef` |
| `VerifiedExitRowEligible` | `bool?` | Null=not verified; user checkbox alone is not an authoritative safety assessment |

Age calculation: use local date of covered service/flight (not UTC date of engine call). At birthdays, age bands use inclusive lower, exclusive upper bound. Multi-flight coverage can have different ages; evaluate per flight or reject ambiguous aggregate shortcuts.

### ShoppingPortion

| Member | Type | Meaning |
|---|---|---|
| `PortionRef` | `string`* | Canonical bound/portion identity, not assumed `BoundOfferId` |
| `Sequence` | `int`* | Positive travel order |
| `FlightRefs` | `IReadOnlyList<string>`* | Nonempty, ordered; 1..N flights; multi-flight one billable Portion when Provision coverage says so |
| `OriginAirportId`, `DestinationAirportId` | `int`* | Verified endpoint geography |
| `ViaAirportIds` | `IReadOnlyList<int>` | Derived from verified itinerary; absent vs verified-none distinguished by completeness |
| `DirectionOrJourneyRef` | `string?` | Only if sourced and required to evaluate route/direction or itinerary policy; never invent 'roundtrip' solely from count |

### ShoppingFlight

| Member | Type | Purpose |
|---|---|---|
| `FlightRef` | `string`* | Context-local stable occurrence ID; unique |
| `FlightId` | `long`* | Real flight occurrence ID; key for flight-scoped rule and inventory reference |
| `FlightVersion` | `int?` | Revalidation token only if sourced; missing version must not mean version=0 |
| `FlightNumber` | `string?` | Matches ProvisionFlightNumber when rule requires |
| `MarketingAirlineId`,`OperatingAirlineId` | `int?` | Match carrier filters; no marketing=operating fallback |
| `OriginAirportId`,`DestinationAirportId` | `int`* | Verified geography |
| `ViaAirportIds` | `IReadOnlyList<int>` | Verified intermediate locations when rule requires |
| `OriginCountryId`,`DestinationCountryId` | `int?` | Country filtering: MUST come from reference authority, not string heuristics |
| `OriginTerminalId`,`DestinationTerminalId` | `int?` | For airport service applicability when verified |
| `DepartureAt`,`ArrivalAt` | `DateTimeOffset`* | Real instants including original offset; canonical UTC for comparisons |
| `OriginIanaTimeZoneId` | `string?` | Required to interpret local weekday/time windows without assuming offsets on DST transition |
| `AircraftId` | `int?` | Needed for aircraft and connected-service eligibility; checked numeric conversion in adapter |
| `CabinClassId` | `int?` | Use actual reference identity for fare/seat/upgrade rules |
| `RbdId` | `long?` | Actual RBD reference, not booking-class label |
| `FlightStatus` | typed optional | If no trusted status, do not infer Flight open/sellable |

Do not require FlightFlow-specific `FlightCapacityId` to evaluate common Shopping rules. The source-specific reservation lookup/binding lives in adapters/Ordering. If a chosen offer must bind to a FlightFlow capacity identifier for a later workflow, it must come from a separately verified source/booking snapshot and be protected in the token/operation; Shopping Engine does NOT fabricate it. Rule entity `ProvisionFlight.FlightId` uses `long` and `ProvisionAircraft.AircraftId` uses `int`.

### TravellerFareFacts (one verified applicable fare/coupon relation)

| Member | Type | Rule served |
|---|---|---|
| `TravellerRef`,`FlightRef`,`PortionRef` | `string`* | Defines exact passenger/flight/portion relationship; no cross-join |
| `AirFareId` | `long?` | `ProvisionAirFare` |
| `AirFareType` | existing typed enum? | `ProvisionAirFareType` |
| `FareFamilyId` | `long?` | `ProvisionFareFamily` (numeric ID) |
| `FareFamilyCodeOrName` | `string?` | Diagnostic/display ONLY; cannot be equated to numeric ID |
| `FareBasisCode` | `string?` | `ProvisionFareBasis` exact code |
| `CabinClassId` | `int?` | Ticket/coupon-specific eligibility; may differ from flight fallback |
| `RbdId` | `long?` | Ticket/coupon-specific RBD |
| `BookingClass` | `string?` | Diagnostic; do not equate to `RbdId` without reference lookup |
| `FareSnapshotVersion` | `string?` | Guard on deferred accepted sale |

A fare may apply to multiple flights/people but must be **expanded with proven coverage**. Contradictory duplicate FareFacts for same person/flight cause a data-validation error, not winner-by-list-order. Never reuse one passenger's fare for another.

### TravellerFlightBaggageFacts (one trusted allowance per traveller/flight)

- `TravellerRef:string`, `FlightRef:string`, `PortionRef:string`.
- `CheckedPieces:int?`, `CheckedWeight:decimal?`, `CheckedWeightUnit:existing typed weight-unit?`.
- `CabinPieces:int?`, `CabinWeight:decimal?`, `CabinWeightUnit:existing typed weight-unit?`.
- `SourceCompleteness:Verified|Missing|Partial`; unknown is NOT zero free baggage.
- Optional `FareBaggageEntitlementRef:string?` if authoritative source provides reference.

A01..A05 need these facts to correctly compute ordinal tiers and already-included entitlements. A02 `PackageWeightKg` from Definition is sold per package; don't multiply rate by its Kg unless the PricingUnit explicitly calls for per Kg. Overweight/oversize need trusted *selected-item* measured input, not inferred from allowance.

### FareBenefitFacts (avoid charging included ancillary twice)

`TravellerRef:string; FlightRefs:string[]; BenefitCode:typed or vetted string; IncludedOrEntitled:bool; SourceReference:string?; EvidenceTimeUtc:DateTimeOffset?; EvidenceCompleteness:Verified|Unknown`. Used only when the actual fare/channel contract describes included Seat/Priority/Meal/Baggage benefit; never infer entitlements from FareFamily text.

### ExistingAncillaryServiceFacts / VerifiedUsageEvidence

`ExistingAncillaryServiceFacts` logical fields: `ServiceRef:string`, `OrderServiceId:long?`, `CountingFamilyCode:string?`, `TravellerRef:string`, `PortionRef:string?`, `FlightRefs:string[]`, `ServiceDate:DateOnly?`, `Quantity:int`, `ConsumptionUnits:decimal?`, `CommercialState:Active|Cancelled|Refunded|...` **map from actual Ordering values, not guesses**, `DocumentState:source-derived?`, `EvidenceSourceRef:string?`. Whether cancelled/refunded services consume entitlement is a **policy-specific decision**, not blanket subtraction. Use only trustworthy status mapping.

`VerifiedUsageEvidence`: `CountingFamilyCode:string`, `LimitScope:existing enum`, `UsageSubjectKey:source-derived typed key`, `UnitsConsumed:decimal`, `IsCompleteForScope:bool`, `AsOfUtc:DateTimeOffset`, `SourceVersion:string?`, `SourceAuthority:string`. For PerFlightOccurrence/PerPortion/PerServiceDate the stable traveller identity must be real; PerOrder needs real `OrderId` + `OrderTravellerId`. If no complete source, Engine can show configured maximum but NOT an asserted remaining quantity.

### Selection facts are NOT part of generic Context

Optional `AncillarySelection` is a **separate typed argument** to `EvaluateSelection(context, candidate, selection)` for concrete Pet dimensions, SeatNumber, amount of baggage, UMNR/medical evidence references, CIP appointment, etc. Use 9 existing profiles and v12.2 `CustomerSelectionContract.For(definition)` to validate closed field names/types/requiredness. Do not introduce `Dictionary<string,object>`/free-form JSON as the domain rule engine. Do not persist sensitive medical/passport/guardian details in Catalog, Context log, or Offer token text.

## 2. Requirements traceability: all TEN Provision Rule groups

| Existing rule group | Context attributes | Behavior on missing required fact |
|---|---|---|
| 1 `PassengerEligibility` | PTC, birthdate/verified age and service date | No match if disproven; `InsufficientContext` when age unverified |
| 2 `SalesRestrictions` | POS, customer ID/type, UTC evaluation time | Unknown customer / POS => no unauthorized sale; fail closed |
| 3 `Geography` | airport IDs, origin/destination, via, route direction, countries, selected service location | Block/require proof of unprovided country/location; no random country ID mapping |
| 4 `FlightApplication` | marketing/operating IDs, flight number/ID, aircraft | Cannot match carrier/aircraft if missing or incorrect numeric type |
| 5 `FareApplication` | fare ID, fare-type enum, FareFamilyId, FareBasis, cabin and RBD per coupon | String FareFamily DOES NOT satisfy numeric FareFamilyId filter |
| 6 `TravelDate` | verified local service/flight dates | Inclusive allowed dates; blackout wins; unknown date blocks |
| 7 `DayTimeApplication` | IANA zone, local weekday/time, UTC instant | Deny overrides allow; no assumed DST offset |
| 8 `AdvancePurchase` | evaluated UTC, service date instant, ticketed timestamp for `SameTimeAsTicketed` | Exact cutoff comparison; unknown ticket timestamp cannot silently pass |
| 9 `BaggageApplication` | checked/cabin allowance, piece ordinal, selected weight, coverage and travel/purchase basis | Unknown allowance/measured bag => `SelectionRequired`/`InsufficientContext` |
| 10 `SeatApplication` | selected seat number/characteristics from VERIFIED map, flight, traveller, eligibility evidence | Category-level candidate may show schema; specific seat cannot be sold without verification |

Profile-specific rules: Pet (`animal type/age/weight/dimensions/documents` from typed selection), AssistedTravel (lead/connection constraints/medical evidence), AirportService (facility/location/guest/time), Upgrade (from/to cabin and provider quote), Meal (lead/catering and exclusivity), Priority (fare entitlement), Connectivity (aircraft/plan/provider). Only facts actually needed by active rules must be mandatory. No speculative one-size-fits-all giant traveller object.

## 3. Context validation invariants

1. `OwnerAirlineId>0, PointOfSaleId>0, CurrencyId>0`, time is UTC normalized, no duplicate traveller/portion/flight refs, no dangling references.
2. Portion ordered flights all exist, exactly one applicable itinerary ownership, no duplicate flight within same portion, no contradictory multi-portion membership unless source model explicitly permits.
3. `DateOfBirth` cannot be after applicable travel date; infant/adult relationship requires verified traveller refs if enforced.
4. Stage evaluation `PreOrder|PostTicketed|OnBoard`, while Provision `Both` is **an applicability condition**, not a stage supplied by the caller.
5. Offer/order provenance may be opaque but MUST be stable enough for subsequent re-evaluation; pre-order has NO real OrderId, so PerOrder usage limits cannot claim a measured balance.
6. No price/Availability/POS/traveller/flight fact accepted as trusted merely because client supplied it. Trust annotation/validation at adapter boundary; P3.1 tests can provide verified fixtures.
7. Numeric ID types must be verified, not converted with overflow or string parsing guesses. `DateTimeOffset` instant plus IANA timezone where local rules require it.

## 4. P3.1 acceptance contract

Ship the precise canonical records/VOs and an exhaustive **`ExistingRule -> ContextField -> TestID -> MissingFactOutcome`** traceability table. First agent action: cross-check every field/enum above against actual committed source and report any newly discovered mismatch before implementation. Do not implement mapping from AirAvail or Ordering yet.
