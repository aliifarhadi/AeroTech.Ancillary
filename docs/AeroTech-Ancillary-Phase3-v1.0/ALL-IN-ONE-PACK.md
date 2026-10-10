# AeroTech Ancillary Phase 3 - Complete Pack v1.0

# AeroTech Ancillary Phase 3 v1.0 - Authority, Work Packages and Execution Gates

Date: 2026-10-10. Status: **OWNER-REQUESTED PACK; IMPLEMENT P3.1 FIRST, GATE P3.2 AND P3.3**.

## 0. Mission and formal partition

Phase 3 is divided into three explicitly independent, serially accepted deliverables:

- **P3.1 Ancillary Shopping Engine:** A source-agnostic engine receives a canonical `AncillaryShoppingContext` and optional typed selection/filters; reads active catalog/eligibility/pricing/inventory configuration; returns `CanonicalAncillaryOfferResult`. It performs no input-source mapping, creates no Order, holds nothing, changes no catalog state, does not issue EMD/eTicket and does not infer guaranteed availability.
- **P3.2 Ancillary Shopping Engine Adapters:** Adapters translate a selected Flight Offer (AirAvail/AirOffer detail), an existing Order snapshot, or verified direct shopping input into the *same* context. Orchestrating Shopping and QuoteSelection, authentication, generated opaque offer refs, TTL, revalidation, and an internal HTTP entrypoint live at this boundary. No duplicated rule engine in adapters. No Ordering mutation unless separately approved in the Ordering repository.
- **P3.3 Ancillary Reservation Operations:** Real Hold, Confirm, Release, Expire, Cancel-confirmed, read/recovery with per-unit evidence, matched to Ordering's orchestration contract; finite local ledger and trusted supplier handling. Flight seat capacity stays FlightFlow-owned via Ordering. No payment/EMD/eTicket issuance in Ancillary.

**Execution:** Coding agent starts only P3.1 now. Completion of P3.1 requires an owner conformance review before ANY P3.2 implementation; completion of P3.2 requires another owner review before P3.3 implementation. P3.2/P3.3 in this pack are forward contracts and dependency/readiness specifications, not permission to implement them today. No stub public endpoints pretending to work.

## 1. Authority hierarchy

1. Canonical airline business/Ordering contractual boundaries and existing v12.2 approved Phase1/Phase2 decisions, definitions and enum identity.
2. This owner-requested Phase3 pack governs *new Phase3 scope only*. Where v12.2 `09-PHASE3-FUTURE-DESIGN-ONLY.md` conflicts with new owner direction, this pack's explicit phase separation prevails. It **does not** reopen v12.2 Phase1/Phase2 authoring or rewrite existing aggregates without direct P3 necessity.
3. Actual repository HEAD/committed code and verified external wire contracts.
4. Lufthansa JSON samples, public PSS/NDC references and donor code are behavior examples, not evidence of internal schemas or source-of-truth guarantees.

Do not generalize from Lufthansa JSON to identical AeroTech domain types. No additional independent roots for 24 variants, no generic EAV/JSON rule engine, no speculative services/reference records, no sample-derived prices, no tenant redesign, no migration/rollback work for disposable test data.

## 2. Verified repository references (must be refreshed by coding agent)

| Scope | Repository / ref verified 2026-10-10 | Permission |
|---|---|---|
| Ancillary | `aliifarhadi/AeroTech.Ancillary` `feat/ancillary-v12.2-phase1-phase2-rebuild@bee6ad67237be4f576e425686ce501f23c277952` | The ONLY writable repository, only files relevant to authorized P3.1 |
| Ancillary base | `k8s-stg@933b7b7a793b426dbcb6362bbf8519886edf9080` | comparison only; P3.1 must start from approved feature HEAD, not silently reset to older base |
| Ordering | `aliifarhadi/AeroTech.Ordering.Final` `k8s-stg@cd50a2a5fd18a372f32f2d4efac08dee0510f6ba` | READ-ONLY |
| FlightFlow | `aliifarhadi/Aerotech.FlightFlow` `k8s-stg@d2180b2e4c07789abde75e712e870ece8d2c776b` | READ-ONLY |
| AirAvail | `aliifarhadi/AeroTech.AirAvail` | **NOT ACCESSIBLE** through connected GitHub (404 at the time of this pack); do NOT claim its source reviewed or assume a live JSON response |

If any HEAD has advanced, report exact SHA and impacted paths before editing. Agent must not push/merge/deploy without owner instruction.

## 3. Primary normative source index

Source URLs are pinned to reviewed Ancillary feature branch; the engineer must check current contents:

- `docs/AeroTech-Ancillary-v12.2/00-START-HERE-AND-AUTHORITY.md` through `17-ENUM-VALUES-EXISTING-RULE-CATALOG-AND-DOCUMENT-ROUTING.md`, particularly **01,02,03,04,05,06,07,09,11,12,17**.
- `src/AeroTech.Ancillary.Domain/AncillaryServiceDefinitionAggregate/AncillaryServiceDefinition.cs`, `.Profile.cs`, `CustomerSelectionContract.cs`, `AncillaryVariant.cs`, typed `Specifications/*`.
- `src/AeroTech.Ancillary.Domain/AncillaryProvisionAggregate/AncillaryProvision.cs`, `.Rules.cs`, `Entities/*`, and `AncillaryPricingAggregate/*`.
- `src/AeroTech.Ancillary.Domain/AncillaryInventoryPolicyAggregate/*`, typed inventory roots, their repositories and configuration snapshot reader.
- `src/AeroTech.Ancillary.Domain/AncillaryReservationAggregate/*`, `src/AeroTech.Ancillary.Application/AncillaryReservationAggregate/Commands/HoldAncillaryServices/HoldAncillaryServicesService.cs` (P3.3 only; current Held is NOT real capacity).
- `samples/lufthansa-services-configuration.json`, `lufthansa-services-by-order.json`, `lufthansa-one-booking-v2-purchase-orders.json` (read-only fixtures).
- Ordering: `src/AeroTech.Ordering.Providers/Offer/Services/OfferProvider.cs`, `Wire/FlightOfferDetailResponse.cs`, `OfferResponseMapper.cs`; `src/AeroTech.Ordering.Domain/Providers/Offer/OfferDetail.cs`, `OfferReader.cs`; `src/AeroTech.Ordering.Application/OrderAggregate/Commands/CreateOrderFromOffer/CreateOrderFromOfferService.cs`.
- Ordering: `src/AeroTech.Ordering.Providers/FlightFlow/Services/FlightFlowReservationProvider.cs`; FlightFlow source `Flight.cs`, `FlightCapacity.cs`, Hold/Confirm/Release/Cancel handlers.
- `reports/V12.2-P2-BACKOFFICE-CLOSURE-RECHECK.md`: authoring CLOSED; fully connected inventory NOT closed.

**Evidence ledger:** The 3 Lufthansa samples contain 22 display categories/48 configured service codes, 114 service entries in by-order example, 6 purchased service entries. By-order statuses include guaranteed/pending/unknown; they are sample/provider statuses, never translated to a domain stock guarantee without an authority contract. Ordering's HTTP OfferProvider posts `v1/FlightOffers/Details` with `{OfferId}` and maps response to `OfferDetail`; it is NOT evidence that Ancillary may depend on Ordering's internal classes.

## 4. Non-negotiable open dependencies

Four Phase2 ports in last verified audit lack an authoritative connected source: `IInventoryResourceReference`, `IAirportFacilityReference`, `ICountingFamilyReference`, `IFlightFlowDelegationReference`. A read-only FlightFlow occurrence reference exists, but it does NOT prove inventory delegation. Do not repair these by accepting arbitrary IDs, creating fake records or claiming capacity guaranteed. In P3.1, return `Unknown`/`SourceUnavailable`/`CannotFullyEvaluate` precisely as appropriate. Track a distinct `BLOCKED_EXTERNAL_REFERENCE` verdict in conformance.

Real per-traveller cumulative usage across orders is NOT available merely because `PassengerUsageLimit` stores a maximum; P3.1 must evaluate only when a credible usage ledger/evidence port exists. Price decimals MUST come from verified currency reference: do not hardcode 2 or silently convert. Flight Offer `FareFamily` is a string while Ancillary Provision `FareFamilyId` is long; this is a **P3.2 contract gap**, not permission to parse/fabricate a numeric ID.

## 5. Gates and final reports

- **GATE 3.1:** Context field/rule traceability is complete; deterministic active product/provision/rate selection; typed schema for A01-A24; truthful pricing and availability; no AirAvail/Order dependency; tests green with exact commands and counts; all unavailable evidence distinctly reported. Approval required for P3.2.
- **GATE 3.2:** Each mapper has real source fixtures and explicit omissions; same canonical context leads to equivalent Engine result; offer token bound to actor/POS/traveller/flight/selection/pricing/TTL; no silent price change; auth and negative HTTP tests. Approval required for P3.3.
- **GATE 3.3:** real atomic capacity/provider outcomes, per-unit partial retry, no double FlightFlow seat hold, cancellation/refund boundary; SQL concurrency tests; no old shallow Held exposed as authoritative; dependency blockers resolved or explicitly left BLOCKED. Owner review before public use.

At every gate agent reports: exact commits, changed files, source-grounded decisions, deviations from this pack, tests run (commands/results), tests NOT run, failed/blocked cases, required owner decisions. **A proposed solution or compile-only success is not 'CLOSED'.**


---

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


---

# 02 - P3.1 Shopping Engine pipeline and canonical output (normative)

## A. Contract and public surface boundary

The Engine is a deterministic, **read/evaluate-only application/domain service**, not an HTTP gateway, purchase orchestrator, aggregate or catalog authoring tool. Its two logical operations are:

```
Shop(AncillaryShoppingContext context, ShoppingFilter filter)
    -> CanonicalAncillaryOfferResult

EvaluateSelection(AncillaryShoppingContext context,
                  CandidateIdentity candidate,
                  TypedAncillarySelection selection)
    -> CanonicalAncillarySelectionEvaluation
```

`ShoppingFilter` optional `ProfileCodes:string[]?`, `VariantCodes:string[]?`, `ServiceDefinitionRefs:string[]?`, `FlightRefs:string[]?`, `TravellerRefs:string[]?`; validated against the exact A01-A24 registry and context. Filters **narrow**, never broaden authorization. Default means all active and eligible service candidates for the given context. `CandidateIdentity` is an *internal composite address* (definitionVersionId + provisionId + coverage/traveller + optional pricing revision), never a public trusted `ServiceOfferId`. Use existing constructors/names/DI conventions. No implement-now API routes or opaque offer storage in P3.1.

Return candidates which are actionable, need input, need quote, and optional negative diagnostics on explicit filtered requests. For ordinary all-products shopping, it is reasonable to omit irrelevant nonmatching products but **never** translate indeterminate eligibility into eligible. Caller can request diagnostics explicitly for support/audit without leaking other POS's catalog.

## B. Read-only ports and candidate selection

Proposed logical ports; **reuse existing repo contracts/read models; add only the narrowest port required**:

- `IActiveAncillaryDefinitionReader`: current ACTIVE Definition/version, typed Spec, SelectionContract, Booking+Document routing, supplier ID. Never fallback to Draft, Suspended or old version in Shopping. Stable service ref is not itself sellable.
- `IActiveAncillaryProvisionReader`: ACTIVE applicable provision(s) for exact Definition version and authorized POS, 10 typed rule groups + bounded profile rules, Sequence, price origin, Quantity and Coverage.
- `IActiveAncillaryPricingReader`: ACTIVE revision/rates+tax/fees for `Paid+Filed`. Free/NotAvailable/ExternalQuote **must not** call filed money as if a price existed.
- `IInventoryConfigurationReader`: existing active Policy for stable product identity; typed Local pattern, authority binding, statuses and source read-state. No policy => NotConfigured. Its config is not a Hold or remaining balance.
- `IReferenceFactReader` if a typed rule needs independently verified static reference (airport country/IANA time zone, fare family identifier, currency decimal places). Do not have engine call AirAvail/Ordering directly.
- `IUsageEvidenceReader` ONLY if an actual complete usage source exists; otherwise the engine reports cumulative remaining `Unknown/RequiresEvidence`. A unit fixture is not a production connected implementation.
- `IAvailabilityEvidenceReader` ONLY for a connected read-only authority that can give genuinely scoped/as-of availability; otherwise report supplier/FlightFlow `DelegatedCheckRequired`, not guaranteed. No attempt to simulate providers.

Minimum pipeline:

1. Validate `ContextSchemaVersion`, source provenance/completeness, scoped identities, unique references and stage.
2. Load current ACTIVE products and current authoritative typed specifications; apply requested variant filter.
3. Load ACTIVE provisions matching the *exact* definition version + authorized POS. Filter dates/stage/customer. Reject ambiguously scoped POS. Evaluate all 10 shared rule groups with correct three-valued semantics (`Match`, `DoesNotMatch`, `InsufficientFact`), plus typed family predicates as relevant.
4. Enumerate supported **coverage candidates** based on `CoverageScope`: `Sector` -> flight occurrence, `Portion` -> contiguous multi-flight portion, `Journey` -> verified itinerary-wide scope, `Order` -> only when the context actually supports an order-level commercial selection. Do not flatten bound-level price to flight-level lines. Bind per-traveller or explicit all-traveller group only when authored semantics allow.
5. Resolve overlapping provision precedence **from authoritative actual source**. `Sequence` is authored precedence; preserve deterministic ordering and refuse same-precedence conflicting matches. If lower/higher ordinal precedence is unverified in current source, record a design decision/blocker BEFORE coding winner selection; never depend on EF/SQL return order.
6. Re-evaluate Definition spec constraints, day/time/date, advance cutoff and included fare benefits; assemble `SelectionContract` from existing `CustomerSelectionContract.For` without making another registry of 24 forms.
7. Match a unique `AncillaryPricingRate` for `Paid+Filed` by requested exact currency, PTC and age band. Rates are authored by currency; never perform unofficial FX or fallback to another currency. Price origin `ExternalQuote` => `QuoteRequired`, amount null. `Free` => `Free` (no fabricated zero priced-rate/fee line). `NotAvailable` => unavailable/non-sellable.
8. Determine QuantityRule, passenger family limit, available quota/physical policy and approval/capability **separately**. If evidence is incomplete, mark *unknown* and require recheck; never report true guaranteed stock from P2 configuration.
9. Return deterministic candidate evaluation list + contract + machine-readable diagnostics. No save, Hold, supplier mutation, issued document, or price acceptance inside this Engine.

Efficiency: read products/provisions/prices/policies in bounded batches, evaluate immutable projections, avoid one DB call per traveller x flight x candidate. Index effective POS/version and pricing selectors; tests assert no unbounded N+1 when scaling sample fixtures.

## C. Canonical response model: detailed field dictionary

```
CanonicalAncillaryOfferResult
  ContextSchemaVersion: int
  EvaluatedAtUtc: DateTimeOffset
  OwnerAirlineId: int
  PointOfSaleId: long
  RequestedCurrencyId: int
  SourceVersion: string? // echoed for future revalidation, not publicly authoritative
  Candidates: CanonicalAncillaryOfferCandidate[]
  Diagnostics: ShoppingDiagnostic[]
  HasBlockedCandidates: bool

CanonicalAncillaryOfferCandidate
  CandidateIdentity: CandidateIdentity       // internal, NOT public purchase ID
  ServiceDefinitionRef: string
  DefinitionVersionId: long
  DefinitionVersion: int
  SupplierId: long
  ProvisionId: long
  ProvisionSequence: int
  Profile: AncillaryProfile                    // existing enum 1..9
  VariantCode: AncillaryVariantCode           // existing A01..A24
  ServiceTypeCode: string
  ServiceSubCode: string
  CommercialName: string
  Description: string?
  Booking: BookingSummary                     // existing booking policy projection
  Document: DocumentRoutingSummary            // no invented eTicket emission
  Scope: SelectionCoverage                    // typed scoped refs (zero/order? validated)
  Travellers: string[]                       // covered actual traveller refs
  FlightRefs: string[]                      // may contain 2+ flights with ONE charge basis
  PortionRefs: string[]                     // explicit portion association
  Eligibility: EligibilityAssessment
  Selection: CustomerSelectionContract       // reuse existing v12.2 variant factory
  Quantity: QuantityAssessment
  Price: PriceAssessment
  Availability: AvailabilityAssessment
  Confirmation: ConfirmationAssessment
  OfferReadiness: NotEligible|NeedsSelection|NeedsQuote|NeedsVerification|Selectable|Unavailable
  ExpiresAtUtc: DateTimeOffset?              // only from actual source validity; no fabricated TTL
  ReasonCodes: string[]                     // from closed documented diagnostic registry
```

`EligibilityAssessment`: `Status=Eligible|NotEligible|InsufficientContext`; `MatchedRuleProvenance` internal/audit only (DefinitionVersionId/ProvisionId, no PII); `ReasonCodes`. `IsBookable` **must not** mean capacity or supplier confirmation; prefer `OfferReadiness` instead of ambiguous Boolean.

`SelectionCoverage`: `CoverageScope` existing enum numeric `Sector=1,Portion=2,Journey=3,Order=4`, and `TravellerRefs`, `FlightRefs`, `PortionRefs`. One candidate has exactly ONE charging scope. Include service occurrence/appointment references in typed selection where necessary rather than adding an unauthorized `ServiceCoverageScope` numeric member.

`QuantityAssessment`: `Unit` from existing QuantityRule, `MinPerSelection>=1`,`MaxPerSelection>=Min`, optional `CumulativeLimit` with family/scope/unit, `VerifiedConsumed`/`VerifiedRemaining` only when evidence is complete. `ZeroIsDeselect=true` for UI quantity skip, but POST Add quantity zero is NEVER an accepted sale. `UnitsPerPurchase` and `ConsumptionUnit` honor weight/package distinction.

`PriceAssessment`:
```
  Origin: Filed|ExternalQuote|Free|NotAvailable
  Status: Complete|QuoteRequired|Incomplete|Unavailable
  CurrencyId: int?
  PricingRevisionId: long?
  PricingRateId: long?
  PricingUnit: existing PricingUnit?
  BaseAmount: decimal?                        // major units, 19,6
  AddedTaxLines: PriceComponent[]             // code, location, amount, currency, treatment
  IncludedTaxLines: PriceComponent[]          // informational, included in base; NEVER add again
  AppliedUnitFeeLines: PriceComponent[]
  UnappliedFeeLines: PriceComponent[]         // FeeApplicationUnit and pending aggregation scope
  CompleteUnitTotal: decimal?                 // null unless actually complete
  RequestedQuantityTotal: decimal?           // null if selection/quantity/fee basis unresolved
  IsOrderLevelTotalComplete: bool
  QuoteProviderKey: string?
  ExternalQuoteRef: string?                   // from actual provider, never invented
  QuoteExpiresAtUtc: DateTimeOffset?
  ReasonCodes: string[]
```

`PriceComponent` must preserve actual Tax/Fee `Code`, optional `CountryId`, `StationAirportId`, `Amount` and currency, TaxTreatment (`IncludedInBase|AddedToBase` only when known), FeeApplicationUnit. For `Filed`, existing `AncillaryPricingRate.UnitTotal` logic is a starting point; **unit total is not automatically order-level complete** when Fee is per Ticket/OneWay/RoundTrip/SectorOrPortion. Do not multiply a ticket fee by passenger or by every covered flight. Unsupported proportional fee units 6..10 must stay blocked rather than interpreted as Item. If FX/currency decimal source is unavailable, return incomplete/blocked and not a made-up amount.

`AvailabilityAssessment`:
```
  InventoryAuthority: Unlimited|Local|Supplier|FlightFlow|Unknown
  ConfigState: existing InventoryCapacityReadState
  CheckedState: NotChecked|AvailableAsOf|Unavailable|Unknown|SourceUnavailable
  IsGuaranteed: false                         // ALWAYS false in P3.1 before actual provider/hold
  RequiresCheckAtReserve: bool
  AvailabilityAsOfUtc: DateTimeOffset?
  AvailabilityValidUntilUtc: DateTimeOffset?
  ConsumptionPreview: Count/Weight/Slot estimates only if authoritative bindings exist
  ReasonCodes: string[]
```

`ConfirmationAssessment`: `BookingMethod` and `ConfirmationRequirement` (existing), optional quote/provider capability, `ProviderConfirmationStatus=NotRequested` in P3.1. Shopping's `quotaStatus=guaranteed` in the Lufthansa fixture is **NOT** a local `IsGuaranteed=true` proof. Physical inventory is not passenger entitlement and stock config is not stock allocation.

`ShoppingDiagnostic`: `Code` stable string; `Severity=Info|Warning|Blocker`; `CandidateIdentity?`; `MissingContextFields?`; `SourceAuthority?`; `EvidenceKind=SOURCE|DESIGN|BLOCKED`; NO raw user input/medical documents. Use existing exception/error framework conventions for invalid root requests; business nonmatch can be a typed result, not 500.

## D. Source-grounded constraints and exact calculations

- Definitions: only current `Active` version; stable ServiceDefinitionRef never identifies a sellable price version on its own. Provision must belong to exact version and exact published POS. Price origin and Outcome must form valid pairs (`Paid+Filed`, `Paid+ExternalQuote`, `Free+Free`, `NotAvailable+NotAvailable`).
- Price selection: `currency==context.CurrencyId`, PTC/age exact predicate with unambiguous winner. `decimal` money; verify currency DecimalPlaces from authoritative source; never use `float` for monetary arithmetic and never guess KWD/JPY/EUR scale.
- Price formula for a **single billable unit** = `BaseAmount + AddedToBase taxes + fees that apply per Item` (when defined). Included tax does not add. Other fee bases remain unapplied and prevent false final total. For a 100 base, 9 added tax, 5 included tax -> unit total 109 and included tax 5 reported separately. For one 7 ticket fee, unit total stays 100 until a ticket-scoped calculation is possible.
- `PerKilogram` quote must use verified billed kilograms vs `PerItem` package quantity. A 10-kg baggage package sold as PerItem is ONE item, not 10 priced units. Accepted selected quantity 1..N; 0=no selection, not a free purchase.
- PassengerUsageLimit fields `CountingFamilyCode, LimitScope, MaxUnits, ConsumptionUnit, UnitsPerPurchase`; no measured remaining unless complete trusted usage evidence. Under PreOrder no OrderId and no PerOrder scope evidence, report unverified, not zero.
- `Supplier` or `FlightFlow` delegated inventory: show `DelegatedCheckRequired`; do not call provider within domain evaluation if only configuration exists. Optional read-only evidence must be scoped to exact offer/traveller/flight/quantity and freshness.
- Shopping date/time uses verified service basis. DayTime deny beats allow; blackouts beat allowed periods; advancement cutoff uses actual `AirPrice.TimeUnit`; ticketed-at fact required when `SameTimeAsTicketed` is true.
- `ServiceDefinition.DocumentRouting` existing `NoAncillaryDocument|Emd|TicketOrExchange` remains metadata. No EMD issuance, no guessing EMD-S vs EMD-A, no declaring EXST/Upgrade as ordinary EMD only.
- `SeatMapSelection`: Engine can show category candidate and selection metadata; specific selected seat **cannot** be advertised confirmed/assigned without FlightFlow/seat-owner evidence. Existing FlightFlow Hold seat string does not prove comprehensive occupied-number seat-map enforcement.
- External quote product (e.g. A10) returns `QuoteRequired`, no fake `0.00` money. A stage3.2 connected provider can later quote and resubmit to engine for validated selection.

## E. Profile dispatch and 24 closed variants

Use existing nine typed Specifications and one published `CustomerSelectionContract` factory. Explicit cases:

- Baggage A01..A06: allowance, purchased extras, tier ordinal, selected weight/dims/equipment code; no false weight/capacity equivalence.
- Seat A07/A08/A09: cabin/characteristic/exit eligibility, seat assignment readiness distinct from price; A09 extra capacity/document action is not A08.
- Upgrade A10: from/to cabin, upgrade method and ticket/exchange routing, authoritative quote and inventory pending.
- Meal A11/A12: free SSR vs paid menu, mutually exclusive family and catering cutoff.
- Pet A13/A14: typed pet form, animal type/size/weight/doc, flight-by-flight acceptance; a multi-flight portion requires evidence for every covered flight.
- AssistedTravel A15..A19: SSR WCHR/BLND/medical/bassinet/UMNR, guardian and lead-time rules; only bounded proof of document/evidence fields, no raw medical data in persistent offers.
- AirportService A20..A22: real facility/terminal/appointment/timezone/guests and closed-for-sale; if Facility reference not connected, return verification required.
- Priority A23: included Fare benefit and duplicate entitlement check before paid sale.
- Connectivity A24: verified aircraft service, optional OnBoard stage and supplier capability; no imagined stock.

Do not create 24 independently persisted shopping tables/services/controllers. A compact switch on existing profile/variant rules is allowed. Any genuinely distinct logic deserves a narrowly typed evaluator, not an extensible expression runtime.

## F. P3.1 minimum demonstration and proof

Agent must demonstrate:

1. `Shop(identicalContext)` deterministic ordering and output, and same results regardless of source provenance when verified facts are otherwise identical.
2. Two POS values return only their own active provisions/rates; customer/customer-type filters cannot be bypassed by omitting data.
3. One Portion with 2 flights returns a **single** applicable Portion-priced candidate (if authored) rather than duplicated charges.
4. One matching fare per traveller/flight, not a guessed all-passenger fare; missing numeric FareFamilyId does not match numeric filter.
5. Free WCHR has no active paid rate/zero-fake line and may require confirmation. ExternalQuote upgrade has null amount/quote needed.
6. Baggage additional-piece ordinals correct with verified allowance; no double counting; 10kg package is 1 item.
7. Taxes included/excluded, non-item fee aggregation and currency scale are correct.
8. All four inventory authority types are represented honestly; lacking one of four external reference ports gives `BLOCKED_EXTERNAL_REFERENCE`, never guaranteed.
9. Unsupported/missing contextual facts yield explicit typed insufficient/verification result, not an empty 'eligible' list and not a silent allow.
10. No code changes outside Ancillary; zero mutation on catalog/inventory/order/payment/reservation in engine tests.


---

# 03 - P3.2 Shopping Engine Adapters and real ingress (DESIGN NOW; NO P3.2 CODE BEFORE P3.1 APPROVAL)

## A. Responsibilities

Adapter converts **verified input facts** from a specific source to `AncillaryShoppingContext` v1. All adapters call the exact SAME P3.1 `Shop/EvaluateSelection`; they cannot contain proprietary `if (A13)...` eligibility, price calculations, inventory math, separate profile registry, or accept a client-supplied `IsAvailable`/Money as final. In P3.2 the application boundary also manages Shopping HTTP, QuoteSelection orchestration, authenticated source resolution, opaque service offer tokens, read-only provider quotes, expiration and revalidation. Physical reservation remains P3.3.

```
AirAvail FlightOffers/Details -- FlightOfferContextAdapter --+
                                                       |
Authorized Order snapshot ----- OrderContextAdapter ---+--> CanonicalContext --> Engine
                                                       |
Trusted itinerary/fare ---------- DirectContextAdapter -+
                                                       |
                                       Candidate + selection validation --> OfferTokenStore
```

## B. `FlightOfferContextAdapter` - verified source and mapping

Ordering real source at `k8s-stg@cd50a2a5`: `OfferProvider.GetByOfferIdAsync()` issues `POST v1/FlightOffers/Details` (relative to `Offer:BaseUrl`) with `{ OfferId = offerId }`. It expects `OfferEnvelope<FlightOfferDetailResponse>` and calls `OfferResponseMapper.ToDomain` before `Order.Create`. These are proof of *Ordering's existing external consumer contract*, NOT proof of AirAvail repository implementation or actual staging response (AirAvail repository not accessible to this audit). The owner-supplied request has `offerId` but the live endpoint was not invoked from the available tools. Never copy an auth bearer token into code, test fixtures, logs or documentation.

Precise mapping audit for P3.2:

| Existing Offer wire field | Context target | Source issues / required checks |
|---|---|---|
| `OfferId` | `SourceIdentity.TrustedSourceReference` | Must be resolved against authorized current sales scope, validated expiry; never trust arbitrary decoded token payload |
| `LastTicketingDate` | `SourceIdentity.ValidUntilUtc` ONLY IF that is its actual semantic (otherwise separate TicketingCutoff fact) | LastTicketingDate != universally OfferExpiresAt; NEVER silently treat as equivalent |
| `CurrencyId` | `CurrencyId` | Check authoritative currency reference; no guessed decimals |
| `AirTransports[].BoundId` | `ShoppingPortion.PortionRef` | BoundId is separate from BoundOfferId |
| `AirTransports[].BoundOfferId` | Adapter-local join key | Used by PricingUnit.CoveredBoundOfferIds; NOT surfaced as final PortionRef |
| `AirTransports[].Flights[].FlightId/FlightVersion` | `ShoppingFlight` IDs/version | Flight must be in verified matched Bound |
| `Flights[].FlightCapacityId` | Source-specific later reservation binding (if needed) | DO NOT require it in core Engine or confuse with FlightId; Ordering forwards it to FlightFlow as a string ID |
| `MarketingAirlineId/OperatingAirlineId` | Flight airline IDs | Offer uses `long`; Provision rules use `int`; checked numeric conversion and real reference semantics required, otherwise block not truncate |
| `AircraftId/CabinClassId/RbdId` | Flight and per-passenger Fare facts as appropriate | AircraftId source `long?`, provision `int`; no string label to ID inference |
| `Origin/DestinationAirportId`, `Legs` | Flight/Portion location and via stops | Source `long` vs Rule `int`; airport country/IANA/local time must come from trusted reference, not inferred from UTC offset |
| `PricingUnits[].CoveredBoundOfferIds` | PricingUnit -> Portion mapping | Resolve through BoundOfferId mapping exactly as `OfferResponseMapper.MapPricingUnits()`; orphan => reject |
| `PricingUnits[].FareComponents[]` | candidate Fare facts | `AirFareId`, `BoundId`, `FareBasis`, `FareFamily` string, `FareType` string, `BookingClass` string. NEVER invent FareFamilyId or enum AirFareType when only text exists |
| `Tickets[].TravellerRef/TravellerIndex/PTC` | Traveller identity/PTC | DateOfBirth, Gender/Office/Customer/actor NOT in Offer Detail; obtain from authorized sales/input scope |
| `Tickets[].Coupons[].BoundId/FlightId` | per-passenger Fare and allowance coverage | Join by TravellerRef+FlightId/Bound; do NOT spread one fare/allowance to all travellers |
| `Coupons[].BaggagePieces/Weight/Unit` and Cabin equivalents | `TravellerFlightBaggageFacts` | Missing Unit in Ordering mapper => null allowance; do not interpret missing as 0 |
| `Coupons[].Pricings` fare line `Reference` | Fare mapping | Ordering `OfferReader` resolves `AirFareId` from Fare-category line reference; use real contract, no randomly first fare if ambiguous |
| `RatesOfExchange`, `OrderCharges` | optional trusted price reference facts, not ancillary authored rates | Base Flight price is NOT an Ancillary filed rate. No FX without verified rule/scale |

Additional fields for `ShoppingTraveller` DOB/Gender/guardian etc must come from verified upstream customer/passenger request. `PointOfSaleId/CustomerId/CustomerType` from authenticated sales context and identity, never from an unauthenticated caller string. `LastTicketingDate` must remain a *ticketing deadline*, not automatically a quote expiry; add separate field to canonical SourceIdentity if real offer TTL differs. Reconcile versioned Offer changes with Ordering's `OfferId` guard.

**P3.2 blocker:** Without readable AirAvail source / captured sanitized real `/FlightOffers/Details` response, do not declare the adapter `SOURCE_VERIFIED` or claim exact end-to-end parity. It can be implemented against the witnessed Ordering contract with a candid `CONSUMER_CONTRACT_VERIFIED / PRODUCER_NOT_VERIFIED` status, then source-verified later by fixture from live authorized response. The access token originally shared by owner is not to be reproduced/reused in pack or checked into tests.

## C. `OrderContextAdapter` (post-booking, post-ticketed)

Only load Order via authorized Ordering read contract, not Ancillary's own copy/SQL cross-service join. Normalize actual `OrderTraveller`, `OrderJourney/Segment`, `OrderAirTransportService` (actual accepted fare and baggage), `OrderService`, `OrderFarePricingUnit`, issued/void/refunded documents where policy needs it. Use committed OrderCommercialVersion for quote binding. Scope by requesting seller/authorized office. Existing services and documents must have complete/correct source-reported status; unknown refund consumption must not automatically re-credit usage. `PostTicketed` is an existing Provision stage and should be used only when ticket issuance is proven. For a **created but not yet ticketed Order**, there is no automatically valid Stage enum `PreOrder` or `PostTicketed`; do not silently reinterpret. Record a REQUIRED owner contract decision or specifically proven policy before claiming post-create/pre-ticket shopping is supported.

`OrderContextAdapter` cannot create/change an Order. Ordering owns accepted Order prices, service addition, funding, issuance and refund. Adapter must never expose internal seller Office or actor identifiers to other customer scopes.

## D. `DirectContextAdapter`

For internal test/backoffice/approved partner caller who supplies selected Flights/Fares but no OfferId: every fact must come with explicit authority/provenance. Accepting `{FlightId, FareBasis}` from an arbitrary client does NOT establish a trusted Fare, Currency, POS, or flight schedule. Need verified source readers; if absent, reject `UntrustedFact`/`InsufficientContext`. Never give `Direct` a bypass of the same eligibility and price rules.

## E. Thin Shopping HTTP/application contract (PROPOSED; NOT EXISTING)

Preserve repository conventions (existing `Service/v1/Ancillaries/Service-Holds` route exists for shallow reservations). Proposed separate internal logical endpoints when P3.2 is authorized:

```
POST /Service/v1/Ancillaries/Offers/Search
  { source: { type: "Offer"|"Order"|"Direct", offerId?:..., orderId?:... },
    filters?: { variants?:["A01"], travellerRefs?:["T1"], flightRefs?:[...] } }
  -> CanonicalAncillaryOfferResult projection (never exposed untrusted internal CandidateIdentity as a valid buying token)

POST /Service/v1/Ancillaries/Offers/Selections/Quote
  { contextRef:opaque, candidateRef:opaque, selection:typed-profile-payload }
  -> BoundServiceOffer { serviceOfferId:opaque, price/availability/verification/expiry,... }

GET /Service/v1/Ancillaries/Offers/{serviceOfferId}
  -> authenticated token-scoped read/revalidation; never a free unscoped browse of other POS offers
```

Concrete URI/name and HTTP auth must be checked against current project conventions at P3.2 gate. **No public `POST /orders/{orderId}/services` in the Ancillary repository**: that is Ordering/agency surface. A thin public Add may take `serviceId/travelerId/quantity` only if selection and quote were already bound and verified in `serviceId` lookup.

P3.2 token model (authoritative server store or authenticated integrity-protected compact token) binds:
`OwnerAirlineId, POS, AuthorizedActorScope, SourceKind/Ref/Version, DefinitionVersionId, ProvisionId, PricingRevisionId+RateId if filed, TravellerRefs, Flight/PortionRefs, exact typed selection canonical digest, allowed accepted quantity, Money/currency/components including tax treatment and fee bases or supplier QuoteRef, Booking/Document routing, Quote expiry and context digest.` Must not contain raw PII or medical inputs. Immutable snapshot is not a guarantee of unchanged provider availability; verify before sale/Hold. TTL is the **minimum** of proven Offer validity / real pricing quote validity / explicit owned token TTL; do not invent external guarantee. Client cannot submit final price or substitute another traveller/office.

Validation: Token absent/expired/modified => reject. Changed OrderCommercialVersion, changed active published rule/price/provider quote => reject/requote per correct commercial rule; do not silently reprice an already accepted Order. Same-bound multi-flight offer holds one commercial line unless rule says otherwise.

QuoteSelection flow must evaluate typed input using P3.1, possibly query a *trusted* external quote/seat availability source if a real integration is wired, then produce bound offer or `QuotePending/NotAvailable/QuoteSourceUnavailable`. Supplier pricing `ExternalQuote` **never** falls back to 0 or customer amount. The Engine is still sole owner of eligibility/price-origin rule decisions; orchestration just supplies verified quote evidence and retries.

## F. P3.2 tests required for approval

1. Sanitized **real** AirAvail response (or explicit producer-blocked), compare adapter Context with known per-traveller coupon, BoundOfferId map, fareId and baggage; preserve 2-flight Portion.
2. FareFamily string vs numeric `FareFamilyId` mismatch => unresolved, never guessed; `AirFareType` text vs enum requires resolver; long->int overflow blocked.
3. `LastTicketingDate` not treated as Offer TTL unless explicitly proven.
4. Equivalent verified facts from Offer and Order paths yield equivalent shopping result where purchase stage and fare state are also equivalent.
5. Authorized POS vs forged customer/office; prevent cross-office leakage; no trust of user-submitted amount, customer ID or age.
6. All 24 typed forms survive HTTP validation; free/quote/selected-seat differ correctly; Offer token tampering/TTL and unknown versions fail closed.
7. Original `AirAvail` auth token is never logged, stored or reused in tests. No other repository modification.
8. Endpoint denied for deleted/retired/suspended product, stale pricing/price-origin contradiction, sold-out/unknown provider evidence.
9. Real call integration, bounded latency, idempotent reference issue, and no N+1 source calls; report producer/test environment availability separately.


---

# 04 - P3.3 Real Hold/Confirm/Release/Expire/Cancel/Recovery (DESIGN ONLY until P3.2 owner closure)

## A. What exists and why it is insufficient

Current Ancillary REST route is `Service/v1/Ancillaries`: `POST Service-Holds`, `GET Service-Holds/{holdId}`, `POST Service-Holds/{holdId}/Confirmations`. Source: `src/AeroTech.Ancillary.RestApi/V1/AncillaryReservationAggregate/Controllers/ServiceController.cs`.

Current `HoldAncillaryServicesService.NewHoldAsync` verifies Definition/Provision/Supplier and checks duplicate OrderService IDs, then `AncillaryReservation.Hold` sets aggregate `Status=Held` immediately. **There is no transactional FlightCount/Weight/AirportSlot allocation and no authoritative supplier approval evidence in this path.** `AncillaryReservationUnit.StockPoolId?` is a legacy placeholder and must not be construed as a stock authority. Current `Confirm()` transitions every Held unit to Confirmed without provider evidence. Existing operations must **not** be exposed as production truthful reservations before authorized replacement.

The P3.3 objective is to evolve the EXISTING AncillaryReservation aggregate and existing controller/application conventions with the smallest correct set of typed operation/evidence children. **No second booking orchestrator or universal stock aggregate.** Ordering remains the orchestration and accepted-price/Order source-of-truth.

## B. Source-of-truth boundaries and important Seat correction

| Subject | Authority |
|---|---|
| Order, order service, accepted money, payment/guarantee, ticket/EMD/coupon, refund | Ordering |
| Ancillary service policy/eligibility/pricing, local counted capacity, supplier reservation evidence | Ancillary |
| Flight cabin/RBD capacity, seat hold/confirm/release/confirmed-seat cancel | FlightFlow via **existing Ordering reservation provider** |
| Actual provider approval/external ancillary inventory | Real supplier/provider; Ancillary records response/evidence, does not pretend success |

Ordering `FlightFlowReservationProvider.PlanUnits()` maps `OrderSeatService` to its `OrderAirTransportService` and deduplicates seat-consuming flight units; `HoldRequestFor` sends the flight capacity ID and optional requested seat. FlightFlow `FlightCapacity.HoldSeat` updates Held/Remaining counters and `ConfirmHeldSeats` moves Held->Confirmed with locks. **DO NOT make Ancillary call FlightFlow to independently hold A07/A08 on top of the already-held Ordering air service.** The Ancillary commercial/service fulfillment may need verification of the *existing* FlightFlow seat unit, not a second capacity hold. A09 EXST requires an explicit extra occupied capacity and ticket/document semantics: BLOCK until a verified PSS/FlightFlow/Ordering contract exists. A10 Upgrade/Exchange similarly requires verified provider quote and ticketing contract. No silent model invention.

Old v12.2 Phase3 design diagram suggested `Ancillary -> FlightFlow` delegation; the actual Ordering provider shows a second delegated hold would risk double capacity. New architecture must have a **single** seat capacity owner and one orchestration path. Final routing of supplementary A09 and new post-booking seat assignment is an explicit owner integration decision at P3.3 gate, not something the Ancillary agent may solve by changing Ordering.

## C. Minimum reservation aggregate/operation model (existing root evolves)

```
AncillaryReservation : AggregateRoot<long> [EXISTING, EVOLVE]
  Id:long; OrderId:long; IdempotencyKey:string(<=128); Reference:string(<=128)
  RequestedExpiresAt:DateTimeOffset?; EffectiveExpiresAt:DateTimeOffset?
  RequestFingerprint:string              // immutable canonical payload hash
  Status: derived summary of unit states; CreatedAt/UpdatedAt
  Units:IReadOnlyList<AncillaryReservationUnit>
  Attempts:IReadOnlyList<ProviderOperationAttempt> (only actual external operations)

AncillaryReservationUnit : existing Entity<long> [EVOLVE]
  Id; ReservationId; OrderServiceId:long; ServiceDefinitionId:long;
  DefinitionVersionId:long; ProvisionId:long; PricingRevisionId:long?;
  ServiceOfferId:string?; TravellerId:long?; CoverageScope:existing enum;
  CoveredFlightIds:long[]; PortionRefs:string[]?; Quantity:int;
  AcceptedCommercialSnapshotRef:string; FulfillmentProviderKey:string?;
  Status: unit state; RequestedExpiresAt:DateTimeOffset?;
  ProviderOperationRef:string?; ProviderUnitRef:string?;
  FailureReasonCode:string?; LastEvidenceAtUtc:DateTimeOffset?;
  Allocations:IReadOnlyList<CapacityAllocation>;

CapacityAllocation : owned child Entity (ONLY for a real Local resource)
  Id:long; ReservationUnitId:long;
  ResourceKind:FlightCount|FlightWeight|AirportSlot;
  FlightId:long?; ResourceId:long?; AirportId:int?; FacilityId:long?;
  SlotStartUtc:DateTimeOffset?; SlotEndUtc:DateTimeOffset?;
  CountUnits:int?; WeightKg:decimal(18,3)?; OccupiedPersons:int?;
  State:Held|Committed|Released|Expired|Cancelled;
  ExpiresAtUtc:DateTimeOffset?; CorrelationKey:string;
  AllocatedAtUtc:DateTimeOffset; ReleasedAtUtc:DateTimeOffset?;

ProviderOperationAttempt : owned operation evidence record
  Id:long; ReservationUnitId:long?; ProviderKey:string;
  Operation:Hold|Confirm|Release|CancelConfirmed|Read;
  IdempotencyKey:string?; RequestDigest:string;
  StartedAtUtc:DateTimeOffset; CompletedAtUtc:DateTimeOffset?;
  Outcome:Succeeded|Rejected|Pending|Unknown;
  ProviderOperationRef:string?; ProviderUnitRef:string?;
  ProviderStatusCode:string?; SafeFailureCode:string?;
```

This is the **minimum logical schema**, not a mandate to add one table per record if current solution supports a smaller matching persistence shape. Reuse current IDs, enums and status values; add enum members only with explicit migrations/tests and no renumbering. If `RequestFingerprint` duplicates existing `EnsureSameContent`, preserve semantics and implement one canonical identity check rather than two inconsistent sources. No sensitive provider request bodies or personal medical data in permanent logs.

Reservation root status is a DERIVED summary (all held, all confirmed, partially succeeded, pending, terminal etc.), never false full success when one child was rejected/unknown. Domain unit statuses need at least `Requested, HeldWithEvidence, PendingProvider, ReadyForDirectConfirm, ConfirmedWithEvidence, Rejected, Released, Expired, CancelPending, Cancelled` with stable numeric values determined after auditing current enum; `UnknownOutcome` belongs attempt/recovery status rather than falsely allowing fresh stock allocation. Do not use a public `Held` if underlying unit has no allocation/provider evidence.

## D. Reservation request (internal logical contract) and per-unit response

```
ReserveBatchRequest
  OrderId:long*; OrderCommercialVersion:string*;
  IdempotencyKey:string*; CorrelationReference:string*;
  RequestedExpiresAt:DateTimeOffset?;
  Services:ReservationRequestUnit[] (nonempty)

ReservationRequestUnit
  OrderServiceId:long*; ServiceOfferId:string*; TravellerId:long?;
  DefinitionVersionId:long*; ProvisionId:long*;
  Coverage:{FlightIds:long[], PortionRefs:string[], Scope:existing enum}*;
  Quantity:int>=1;
  AcceptedCommercialSnapshotRef:string*;
  AcceptedPriceCurrencyId:int?;
  ProviderKey:string?;
  TypedSelectionReference:string?;

ReservationBatchResult
  AncillaryReservationId:long;
  OperationReference:string;
  Status:Complete|Partial|Pending|Rejected|Unknown;
  EffectiveExpiresAtUtc:DateTimeOffset?;
  Units:ReservationUnitResult[]

ReservationUnitResult
  OrderServiceId:long; ReservationUnitId:long;
  Status:explicit truthful status;
  ProviderOperationRef:string?; ProviderUnitRef:string?;
  CapacityEvidenceRef:string?;
  EffectiveExpiresAtUtc:DateTimeOffset?;
  ReasonCode:string?;
  MayRetry:bool; RequiresReadBack:bool;
```

Fields marked `*` are logical contract requirements; do not claim they already exist on current HTTP. Ordering adapter to this exact DTO has not been approved or implemented. In integration, `OrderServiceId` must belong to the authenticated Order, and accepted snapshot is resolved from Ordering as authority. Do not make a claimant-supplied price valid. Bind `ServiceOfferId` to current exactly validated definition/provision/context/selection/price and TTL from P3.2.

## E. Exact operations and allowed transitions

| Operation | From | Required proof, atomic effect | Result |
|---|---|---|---|
| `Hold` local | Requested | Published product + checked offer, inventory source `Active`, real physical count/weight/slot in same DB transaction, TTL valid | HeldWithEvidence or Rejected |
| `Hold` supplier | Requested | Real provider reply incl reference; async/pending is NOT held | HeldWithEvidence, PendingProvider, Rejected, Unknown |
| `Hold` Unlimited | Requested | No finite *local* stock only; validate MustCheckAvailability, supplier/booking confirmation requirements | ReadyForDirectConfirm or pending with correct policy; NEVER guaranteed just for Unlimited |
| `Hold` FlightFlow seat | Already managed by Ordering flight-unit | VERIFY existing Ordering/FlightFlow reservation evidence where contract permits; do NOT send duplicate hold | No duplicate stock allocation |
| `Confirm` local | HeldWithEvidence | Valid nonexpired allocation still owned by this unit, accepted funding/Order instruction from Ordering | Committed/ConfirmedWithEvidence exactly once |
| `Confirm` supplier | HeldWithEvidence/ReadyForDirectConfirm | Provider confirmation/result or documented no-confirm supplier contract | ConfirmedWithEvidence, PendingProvider, Rejected, Unknown |
| `Release` | HeldWithEvidence/PendingProvider as allowed | Cancel outstanding provider hold or local active allocations safely; compensate if provider outcome unknown | Released; may remain Pending until real evidence |
| `Expire` | HeldWithEvidence and clock >= effective expiry | CAS/locking and exactly-once local release; supplier status may require reconciliation | Expired only with appropriate local/provider semantics |
| `CancelConfirmed` | ConfirmedWithEvidence | Per-service provider cancellation scope or local consumed capacity release, actual policy + owner request | Cancelled/CancelPending; NOT a refund |
| `Read/Recover` | Unknown or Pending operation | Actual provider state read when available, or safe idempotent replay when provider explicitly supports it | Reconcile to truth without duplicate reservation |

**Rules:** Hold + Confirm not guaranteed combined transaction across multiple suppliers. Initial batch may partially succeed by distinct OrderService unit; emit all per-unit outcomes. Local multi-resource Count+Weight allocation MUST be atomic across both resource records. If one leg/portioned service needs all flights, define its own all-or-nothing unit requirement, not a false partial acceptance of a multi-flight purchased service. `Release` before confirmation != `CancelConfirmed` after confirmation. `CancelConfirmed` != refund/void/EMD cancellation. On provider technical timeout unknown, don't create a new hold with a new key until safe status is recovered.

## F. Capacity and concurrency specifics

- `FlightCountInventory` physical key `(OwnerAirlineId,FlightId,ResourceId)`; consumption `quantity*CountPerAcceptedUnit`; count-unit must match resource.
- `FlightWeightInventory` physical key `(OwnerAirlineId,FlightId,WeightResourceId)`; weight `quantity*FixedKgPerUnit` or trusted `AcceptedWeightKg`; decimal(18,3), no unsafe conversion.
- `AirportSlotInventory` physical half-open UTC intervals at verified `(OwnerAirlineId,AirportId,FacilityId)`; occupancy duration/people configured and validated, overlapping interval total usage bounded; 100 concurrent requests to 1 capacity => exactly 1 success.
- For one product with both count+weight requirements, acquire deterministic ordered resource locks in **one** DB transaction, recheck capacities inside transaction, append allocation(s), commit once or none. No global `StockPoolId` inferred from SKU.
- Two products bound to same physical ResourceId share one available pool and ledger; cannot each start at configured total. Active held + confirmed allocated <= current configured total at ALL times. Adjustments may not lower capacity below committed+held occupancy.
- On expiry/release/cancel retry, decrement allocation exactly once; replay same IdempotencyKey same canonical request returns same unit references/semantics, different payload same key => conflict. Concurrent replay returns first committed response, not a second allocated unit.
- Avoid holding SQL transaction/locks while awaiting slow supplier HTTP. Persist operation intent, dispatch via existing safe framework pattern, handle confirmed/pending/error and compensation. No invented infrastructure requirement; use established outbox/inbox if present.
- Evidence freshness matters: `AvailableAsOf` during Shopping is not a Hold. Revalidate at Hold under capacity lock or provider request.

## G. Proposed internal HTTP extensions (P3.3; adapt to existing controller convention)

Existing:
```
POST Service/v1/Ancillaries/Service-Holds
GET  Service/v1/Ancillaries/Service-Holds/{holdId}
POST Service/v1/Ancillaries/Service-Holds/{holdId}/Confirmations
```

Required new logical operations, proposed paths (not pre-existing):
```
POST Service/v1/Ancillaries/Service-Holds/{holdId}/Releases
POST Service/v1/Ancillaries/Service-Holds/{holdId}/Extensions (only with real provider support)
POST Service/v1/Ancillaries/Service-Holds/{holdId}/Recoveries
POST Service/v1/Ancillaries/Service-Holds/{holdId}/Confirmed-Cancellations
GET  Service/v1/Ancillaries/Service-Holds/{holdId}/Units/{unitId}
```

Expire is normally an internal idempotent scheduled application command driven by verified `IClock` + saved time. Internal service may offer explicit `Expire` for operations/repair but don't expose an anonymous public expired-state mutator. Route shape/method may be adjusted to actual ASP.NET conventions with documented evidence, not changed without tests. Require service-to-service authorization, caller scope and order-owner check; `GET` cannot leak other agency reservations. For cancellation support explicit `OrderServiceId`/unit targets, not always whole batch. Never reuse GET to mutate state.

HTTP/semantic failures: invalid request/selection/context, expired offer, stale commercial version, lost/unknown provider outcome, reference source unavailable, insufficient capacity, customer/POS mismatch, already confirmed, already released, and idempotency conflict each have distinct safe machine-readable result. Use current error framework/status mappings instead of invented numeric codes. No accidental auto-confirm after timeout.

## H. Integration readiness blockers and closure

- Need real, documented provider read/hold/confirm/cancel capability and supplier-specific partial/atomic behavior before claiming supplier `Held`.
- Need authoritative resource registries for Local count/weight, facility reference/zone for airport slots, counting-family registry/ledger, and FlightFlow delegation reference. Never bypass Phase2 blocks by making implicit local data.
- Need owner-approved settlement/order instructions contract and accepted snapshot binding from Ordering; Ancillary cannot independently decide payment was guaranteed or issue documents.
- Need to distinguish separate A09 seat inventory and A10 ticket exchange. If not available, mark these variants `BLOCKED_PROVIDER_CONTRACT` with truthful Shopping output rather than advertise reservation as implemented.
- Existing Ordering `IReservationProvider` has explicit capability model. Actual FlightFlow provider declares `AtomicAllOrNothing`, `SupportsReadBack=false`, `PreConfirmationReleaseScope=Operation`, `PostConfirmationCancelScope=Unit`; preserve these semantics rather than pretending all providers can partially release or read back.

## I. Mandatory P3.3 conformance/stress

SQL Server integration against real constraints, 100 concurrent holds cap=1; simultaneous create+release+expire+confirm; count+weight atomic rollback; shared resource across 2 products; overlapping airport interval and DST; duplicate key same/different digest; provider timeout-after-success then replay/read-back; multi-flight atomic unit; partially accepted mixed supplier order; cross-POS/cross-airline denial; expired quote/booking cutoff; confirmed cancellation per unit and replay; supplier pending vs held; free SSR requiring approval; FlightFlow double-hold negative test; A09/A10 explicit blocked test; immutability of accepted price and no EMD issuance from Ancillary. Separate externally blocked integration results from test failures.


---

# 05 - Phase3 Traceability, A01-A24 scenarios, integration and stress matrix

**Test status in this pack = NOT EXECUTED.** These are acceptance criteria. Agent must run source-level tests and report exact commands/results, never copy Phase2 historical counts as Phase3 evidence.

## 1. Definition of Done per section

P3.1 `PASS_ENGINE`: complete typed Context, 10-rule traceability, active/current version selection, 24-variant candidate/schema, accurate price/tax/fees, truthful inventory/usage limits and deterministic results, no source DTO coupling.

P3.2 `PASS_ADAPTERS`: only after gate approval; producer-verified or honestly source-blocked mappings, authenticated Shopping/Quote HTTP, bound tokens/TTL/revalidation, negative security and version tests; no Order mutation.

P3.3 `PASS_RESERVATION`: only after gate approval; actual allocation/provider outcome and per-unit consistency under SQL concurrency, safe retry/read/compensation, no double-flight-seat hold, cancellation separate from refund and issuance. If provider/resource capability absent => `BLOCKED_EXTERNAL_REFERENCE`, NOT `PASS_RESERVATION`.

Every variant row includes both initial `Shop` candidate evaluation and `EvaluateSelection` typed input. An incomplete mandatory input returns `NeedsSelection`, not fictional immediate confirmed/held stock.

## 2. Exhaustive profile/scenario tests (P3.1)

| Test ID | Variant / test story | Assert |
|---|---|---|
| E-A01 | One traveller checked bag; 1 free piece and distinct first/second paid piece tier provisions | correct ordinal/portion, one rate, cannot sell duplicate included free allowance, min qty positive |
| E-A02 | Traveller buys two 10-Kg packages; `PricingUnit=PerItem` | price=2 x package; quantity limit uses kg entitlement only when configured, never 20 copies of item price |
| E-A03 | Verified baggage 27 kg inside `(23,32]`; outside bracket | typed form + bracket match and reject outside; no implicit combined surcharge |
| E-A04 | Oversize with dimensions and optional A03 combinability | dimensions required, commercial combination policy respected, approval can be pending |
| E-A05 | Cabin allowance known vs unknown; additional cabin bag | no double charge included cabin bag; unknown => insufficient evidence |
| E-A06 | Sports equipment code, size/weight and supplier | typed selection; no fake physical equipment capacity, supplier delegated status |
| E-A07 | Standard Seat at selected flight and traveller | SeatMapSelection; price or Free as authored; no confirmed seat assignment from metadata |
| E-A08 | Preferred exit-row with child traveller | eligibility/required safety terms and verified aircraft seat characteristic; child blocked as policy demands |
| E-A09 | EXST additional occupied seats | never treat as ordinary preferred seat; extra seat count + ticket/exchange contract required, reservation capability blocked if unproven |
| E-A10 | Cabin upgrade external quote | original/target cabin eligibility; amount=null, QuoteRequired and TicketOrExchange metadata as authored |
| E-A11 | Free WCHR? no, this is free special meal SSR | Free no filed pricing; correct PTC, catering cutoff and meal exclusivity, potential pending confirmation |
| E-A12 | Paid preorder meal/menu | required menu item, correct per-item rate, excludes incompatible menu/time selections |
| E-A13 | Cabin cat/dog/weight/dimensions across outbound+return | evaluated for both flights, subject to supplier approval, never accept quota based on one flight alone |
| E-A14 | Hold pet size/weight bracket + required docs | correct variant/supplier + named size bracket, no phantom quota |
| E-A15 | Wheelchair WCHR/WCHS/WCHC | Free SSR, right assistance code, no EMD paid line, confirmation separate from eligibility |
| E-A16 | BLND/DEAF/DPNA | typed code and communication restrictions, no arbitrary medical data requirement |
| E-A17 | MEDA/AOXY/STCR | evidence refs/oxygen amount as applicable, medical/supplier Pending, no real medical info retained in logs |
| E-A18 | Bassinet and associated adult | infant+guardian+flight, real availability unknown without bassinet source, no miscount as regular seat |
| E-A19 | UMNR traveller age + handoff/pickup schema + connection rules | multi-flight coverage and lead time, guardian form requirements, supplier may reject/pending |
| E-A20 | Lounge venue/terminal/time/guest count | ServiceStart date basis; unknown facility time zone fails closed, no imaginary room capacity |
| E-A21 | FastTrack local appointment and DST ambiguity | location/slot schema; DST gap/duplicate instant handled with authoritative zone, unknown source blocked |
| E-A22 | CIP package includes Lounge element | one package charge (no automatic double charge constituent products), supplier/slot proof required |
| E-A23 | Priority paid vs included in fare | included benefit not billed twice; verified duplicate entitlement per flight if available |
| E-A24 | On-board WiFi plan and aircraft capability | OnBoard context separate from P3 `Both`, source verified; unknown supplier availability not guaranteed |

## 3. Ten Provision rule groups - must be individually exercised

| ID | Rule | Test cases |
|---|---|---|
| RULE-01 | PassengerEligibility | 2 PTCs, boundary ages, leap-year birthday, missing DOB when age band used, child exit row |
| RULE-02 | SalesRestrictions | POS A vs POS B, customer-specific vs customer type, exact from/to UTC boundary, no leaking unpublished provision |
| RULE-03 | Geography | origin/destination/via, route pair direction, coverage country, missing verified country, airport ID overflow |
| RULE-04 | FlightApplication | marketing vs operating carrier, flight number vs ID, aircraft match/mismatch, missing aircraft |
| RULE-05 | FareApplication | FareId, FareBasis, FareType, numeric FareFamilyId, cabin, RBD; one PTC different coupon fare; no string->id guess |
| RULE-06 | TravelDate | allowed inclusive start/end, blackout precedence, multi-flight differing travel dates |
| RULE-07 | DayTimeApplication | weekday masks, start-inclusive/end-exclusive, overlapping Deny/Allow, DST mismatch/source unavailable |
| RULE-08 | AdvancePurchase | minimum/maximum exact boundary with various real TimeUnit, SameTimeAsTicketed without actual ticket timestamp |
| RULE-09 | BaggageApplication | free pieces and excess ordinal; Piece vs Weight, purchase vs travel application, overlapping surcharge prevention |
| RULE-10 | SeatApplication | selected seat number and characteristics, absent seat map evidence, unavailable group, non-seat profile forbidden |

## 4. Core engine price, inventory and contract tests

| ID | Assert |
|---|---|
| PRICE-01 | EUR base 100 + added tax 9 + included tax 5 -> unit total 109; included 5 reported and NOT added again |
| PRICE-02 | 7 fee `PerTicket` appears unapplied; pricing calculation never multiplies by two travellers/three flight legs |
| PRICE-03 | FeeApplicationUnit enum 6..10 NOT implemented -> `Incomplete/Blocked`, not Item |
| PRICE-04 | EUR 2dp, JPY 0dp, KWD 3dp determined by authorized reference; unknown source or wrong decimals fails closed |
| PRICE-05 | Missing active Pricing for Paid+Filed not sold; Free no fake priced rate; Paid+ExternalQuote null amount/QuoteRequired |
| PRICE-06 | Ambiguous same-currency same-PTC/age price rates => reject, not take first |
| PRICE-07 | Correct refund/penalty metadata not calculated by shopping if source missing; document Routing matches Definition only |
| INVENTORY-01 | No Policy => NotConfigured; Unlimited => no local finite pool, not `Guaranteed` |
| INVENTORY-02 | Supplier/FlightFlow => DelegatedCheckRequired, no local seat bucket |
| INVENTORY-03 | Local ConfiguredNotGuaranteed vs ClosedForSale; missing resource => SourceUnavailable |
| INVENTORY-04 | 10kg package count for entitlement with Kg family vs purchased units; missing ledger -> remaining Unknown |
| INVENTORY-05 | One shared physical ResourceId across commercial variants, no independent remaining stock inferred |
| ENGINE-01 | Two-flight one Portion => one portion-priced service, and per-flight Sector candidate only when authored |
| ENGINE-02 | Repeated same canonical context yields same stable deterministic candidate content and ordering |
| ENGINE-03 | Definition Active but Provision Suspended => non-sellable; Active Pricing of unrelated version cannot be used |
| ENGINE-04 | Same key with different buyer Office/Customer cannot access same commercial terms; filter can't override POS scope |
| ENGINE-05 | `SelectionContract` fields/requiredness come from existing factory for all 24 variants; no foreign-profile fields |
| ENGINE-06 | Incomplete age/fare/baggage/DST data => insufficient evidence, never unconditional eligible |
| ENGINE-07 | Unauthorized internal `CandidateIdentity` is not a purchase token and cannot be used for booking |
| ENGINE-08 | No reservation/provider mutation, query-only DB assertions, no P3.1 HTTP routes |

## 5. P3.2 adapter and token tests (deferred)

`ADP-01..12`: source-proven mapping; BoundOfferId-vs-BoundId; per-coupon AirFareId; FareFamily string-vs-ID; long-to-int checked; PTC+DOB provenance; tickets/Offer expiration differences; post-create pre-ticketed stage blocked; POS/office forged scope; quote expired; token replay/tamper and sales currency manipulation; alternate SourceKind equivalence; no token secrets in logs; sanitized authentic AirAvail fixture or explicit `PRODUCER_NOT_VERIFIED`.

## 6. P3.3 reservation integration tests (deferred)

`RES-01` 100-way cap=1 -> exactly one valid local Hold; `RES-02` count+weight atomic rollback; `RES-03` same physical resource two commercial products; `RES-04` airport overlaps/slot DST; `RES-05` same idempotency key same digest returns same unit refs; `RES-06` same key different digest fails; `RES-07` supplier response lost-after-commit -> Unknown, recovered without duplicate allocation; `RES-08` supplier mixed Pending/Confirmed/Rejected per-unit; `RES-09` expired Hold vs Confirm concurrent under lock; `RES-10` Cancel confirmed subset and replay exactly once; `RES-11` multi-flight atomic unit/no half service; `RES-12` FlightFlow seat double-hold refused; `RES-13` A09 EXST blocked until multi-seat and document contract; `RES-14` A10 Upgrade blocked without quote/Exchange capability; `RES-15` no EMD/ticket issued by Ancillary; `RES-16` unknown provider outcome not auto-replayed with a NEW idempotency key; `RES-17` cross-office/airline/Order unit authorization denied; `RES-18` no false refund during capacity cancellation.

## 7. Evidence-report format (mandatory)

For every tested row emit:

```
Test ID | Scenario | Source evidence | Actual test method/file | Execution command
Expected result | Actual result | PASS/FAIL/BLOCKED_EXTERNAL_REFERENCE/NOT_RUN
```

Include `dotnet test ...` exact command and failing test names without invented totals; SQL integration tests distinguished from in-memory Domain tests. HTTP API smoke in P3.2/P3.3 only after authorized. A fixture that pretends missing provider references exist proves DOMAIN behavior **only**, not external integration or production readiness. `PASS_DOMAIN` and `BLOCKED_EXTERNAL_REFERENCE` are distinct.


---

# COPY THIS TO CODING AGENT - AeroTech Ancillary P3.1, not P3.2/P3.3

You are the coding agent for `aliifarhadi/AeroTech.Ancillary`. Implement ONLY **Phase 3.1: canonical context-driven Ancillary Shopping Engine** according to the attached `AeroTech-Ancillary-Phase3-v1.0` pack. The owner has deliberately split Phase 3 into (1) Engine, (2) Adapters, (3) Reservation Operations, and requires **independent closure**. Do not implement (2) or (3) without a later explicit owner authorization.

## Reading order and unambiguous authority

1. `00-READ-ME-AUTHORITY-AND-GATES.md` first for scope and mandatory stop conditions.
2. `01-SHOPPING-CONTEXT-FIELD-CONTRACT.md` for exact Engine input field dictionary and ten-rule mapping.
3. `02-ENGINE-PIPELINE-CANONICAL-OFFER-AND-DECISIONS.md` for pure read/evaluate pipeline and fully typed output.
4. `05-CONFORMANCE-TRACEABILITY-AND-NEGATIVE-TESTS.md` for exhaustive 24 variants and stress/negative tests.
5. `03-ADAPTERS-CONTRACT-AND-INTEGRATION-GATES.md` and `04-RESERVATION-PROTOCOL-ENDPOINTS-AND-INVARIANTS.md` **READ as forward compatibility only**. Do not implement them.
6. `08-ADR-DEPENDENCY-AND-DECISION-REGISTER.md` for source claims and blocked decisions.
7. Existing `docs/AeroTech-Ancillary-v12.2/` pack for ALL existing approved domain/field/enum decisions; actual source is source of current names/ordinals.

Where new Phase3 pack disagrees with older Phase3 design-only document, apply the new owner-requested separation. Where it disagrees with actual stable domain enum numbers or v12.2 Phase1/2 published invariants, STOP, report exact conflict and propose smallest correction; do not invent a compromise silently.

## Repository and source guardrails

- ONLY writable repo: `aliifarhadi/AeroTech.Ancillary`, expected feature branch `feat/ancillary-v12.2-phase1-phase2-rebuild`, last observed HEAD `bee6ad67237be4f576e425686ce501f23c277952`. Check branch/HEAD, clean/dirty state and differences. Do not reset/merge old `k8s-stg`; never overwrite user changes.
- READ ONLY: `aliifarhadi/AeroTech.Ordering.Final@cd50a2a5`, `aliifarhadi/Aerotech.FlightFlow@d2180b2e4`, AirPrice, AirAvail, and any other repository. AirAvail was unavailable to the reviewer; DO NOT claim it inspected. Do not reuse a token supplied in an earlier chat.
- No changes to existing Phase1/Phase2 published catalog or inventory authoring semantics; no renumbered enums, no invented currency scales, no new universal StockPool, no 24 aggregate roots, no dynamic rule JSON/EAV engine, no cross-repository modification, no speculative provider mock presented as real.
- No changes to `AncillaryReservationAggregate/**` or existing reservation controllers/commands, except if there is an unavoidable *compile-only* change: explain and request owner approval before touching. Engine MUST have no `Hold/Confirm/Release` effect.
- No public shopping, quote or AddService endpoints in this pass; no Adapter implementation, no AirAvail/Ordering DTO reference in Engine, no unapproved Order mutation/payment/EMD/ticketing.
- No schema changes or migrations are expected for P3.1. If you believe one is necessary, report the exact cause and stop before migration. Tests may create fixtures but no fake provider in production.
- Source code conventions and solution folders prevail over invented project/assembly structure; keep readable maintainable .NET code with existing DI and domain test styles.

## Required implementation order (do not skip the design/source audit)

**Step 0 - preflight evidence (write `reports/P3.1-SOURCE-AUDIT.md` before code)**
- Confirm HEAD/dirty files; list current 9 typed profiles/A01..A24, active lifecycle selectors, all ten rule groups and their child field types, pricing modes, rate selectors, tax treatment, fee units, inventory states and selection factory.
- Build table `Rule -> required context facts -> missing fact behavior -> evaluator -> TestID` for ALL TEN Provision rule groups and profile-specific additions.
- Check each proposed field type against actual source, notably `ProvisionFareFamily.FareFamilyId:long` while Ordering wire `OfferFareComponent.FareFamily:string`, and source `long` marketing/aircraft/airport IDs vs domain `int`. Do not conflate these.
- Check how overlapping `Provision.Sequence` works in actual code/ADR. If priority direction unknown, register a blocking question BEFORE choosing silently.
- Register four missing Phase2 reference providers as blocked for genuine external availability/usage evidence.

**Step 1 - canonical contract only**
- Create immutable typed `AncillaryShoppingContext` and child records/VOs in the narrowest suitable domain/application contracts location; import/reuse existing enums rather than redefining.
- Validate completeness/provenance, IDs, traveller/flight/portion/fare relationships and context stage. Never demand OrderId when shopping pre-order.
- Separate typed selected-input facts from baseline Context. Reuse `CustomerSelectionContract.For` for metadata.
- Write tests before adapters or source DTO mapping.

**Step 2 - read/evaluation service**
- Implement `Shop` and (if relevant to published typed rules) `EvaluateSelection` as pure read/evaluate, with narrowly scoped active catalog/pricing/inventory reader ports and deterministic candidate ordering.
- Support all ten rule groups and nine profiles; only current Active Definition, Active Provision matching exact owner/POS/version, Active applicable Pricing. No Draft/Suspended fallback.
- Build correct Sector/Portion/Journey/Order candidate coverage, including multi-flight one billable Portion. Do NOT calculate whole Order totals by blindly multiplying each leg.
- Return explicit `Eligible/NotEligible/InsufficientContext`, `NeedsSelection`, `NeedsQuote`, `NeedsVerification` and `Unavailable` as separate truthful cases. On missing evidence, fail closed without pretending sold-out or eligible.
- Implement validated currency/PTC/age rate selection; distinguish `Filed`, `ExternalQuote`, `Free`, `NotAvailable` and incomplete price, taxes included vs added, non-item fee bases, unlimited vs provider-delegated vs local configured inventory.
- For `Seat` do not call FlightFlow Hold; for `Pet` and other supplier services do not fabricate approval. Engine never returns guaranteed inventory merely from published policy.

**Step 3 - tests and evidence**
- Implement P3.1 cases in `05-CONFORMANCE-TRACEABILITY-AND-NEGATIVE-TESTS.md`: all A01-A24, RULE-01..10, PRICE, INVENTORY, ENGINE and negative/incomplete context tests. Preserve existing test styles and repo-level compilation conventions.
- Tests must prove two POS isolated, per-coupon fare correct, one Portion with two flights no double charge, missing numeric FareFamilyId not interpreted from string, free SSR not charged, 10kg package one item, tax 100+9+5=109, no unknown reference => guaranteed, unknown age/DST blocked.
- Domain fake repositories may test blocked behavior, but they DO NOT prove connected provider availability. No P3.2/P3.3 HTTP smoke or SQL allocation tests should be claimed in this phase.
- Run solution build, relevant existing P1/P2 regression, new unit/acceptance tests. Report exact commands and results, never invent a test count or use past reports as current evidence.

**Step 4 - closure report and STOP**
- Write `reports/P3.1-ENGINE-CLOSURE-REVIEW.md`: SHA, changed files, precise typed context definition, candidate response examples for at least A01, A08, A10, A13, A15, A20, A22 and A24; each ten-rule mapping and test evidence; tests NOT run; any external-source blockers/owner decisions; prove zero modifications to other repositories/reservation module.
- Conformance verdict `P3_1_READY_FOR_OWNER_REVIEW` only if all P3.1 evidence complete. Otherwise `P3_1_BLOCKED` with exact missing proof.
- Stop. No implementation of P3.2/P3.3 and no automatic push/merge/deploy. The owner reviews Phase3.1 before next authorization.

## Critical rejection conditions

STOP and document a BLOCKER rather than guessed implementation if: (a) numeric FareFamilyId has no verified source and a rule requires it, (b) authoritative timezone/currency/usage data missing, (c) no Active matching provision/rate, (d) provider inventory reference not connected, (e) existing enum/coverage/sequence semantics ambiguous, or (f) architecture requires editing Ordering, FlightFlow or AirAvail.

Success is a trustworthy Engine, not a screenshot, a sample converted to production DTO, a fake Guaranteed offer, or a shallow Held record. The user wants three independently verified boundaries and step-by-step delivery.


---

# Continuation prompt for next chat - Ancillary Phase3

The owner approved **the decomposition** of AeroTech Ancillary Phase3 into:
1. Phase3.1 independent canonical Ancillary Shopping Engine receiving its own typed canonical context, matching existing active Definition/Provision/Pricing/InventoryPolicy with full eligibility/price/schema/availability truth, returning canonical offer candidates.
2. Phase3.2 adapters: AirAvail FlightOffer Details, authorized Ordering Order snapshot and verified direct input into exactly that Context, shopping/selection/quote HTTP and bound opaque ServiceOfferId issuance.
3. Phase3.3 real Ancillary reservation Hold/Confirm/Release/Expire/Cancel/Recover with genuine capacity or supplier evidence, no double FlightFlow hold and no EMD issuance by Ancillary.

The pack `AeroTech-Ancillary-Phase3-v1.0` is authoritative for NEW Phase3 boundaries, under the v12.2 approved Phase1/2 contracts. Agent starts ONLY P3.1 and stops for owner audit; P3.2/P3.3 are forward design until separately authorized. Do not reopen P1/P2 or implement fake inventory/hold. No other repository may be modified.

Verified baseline 2026-10-10:
- Ancillary `feat/ancillary-v12.2-phase1-phase2-rebuild@bee6ad67237be4f576e425686ce501f23c277952`.
- Ordering `k8s-stg@cd50a2a5fd18a372f32f2d4efac08dee0510f6ba`, FlightFlow `k8s-stg@d2180b2e4c07789abde75e712e870ece8d2c776b` (read only).
- AirAvail GitHub inaccessible (404), no live authenticated FlightOffers/Details response verified. Ordering's real OfferProvider POST route and wire DTO were inspected. Do not claim full producer contract verification.
- P2: `PHASE2_AUTHORING_CLOSED=yes`, `PHASE2_FULLY_CLOSED=no`; unresolved Inventory Resource, Airport Facility, Counting Family, FlightFlow delegation authority. No manufactured reference records.
- Actual `AncillaryReservation.Hold` still stores Held without local/provider allocation. FlightFlow Hold genuinely consumes flight capacity; Ordering already groups Air + Seat services per passenger/flight, thus Ancillary must not double reserve.
- Lufthansa fixtures: 22 display categories / 48 codes, by-order 114 services, six purchase order service examples; sample statuses are not authoritative AeroTech stock.

Authority docs: `docs/AeroTech-Ancillary-v12.2/` 00..17, especially 01,02,03,04,05,06,07,09,17; this pack 00..08. Agent prompt is `06-CODING-AGENT-EXECUTION-PROMPT.md`.

On next agent result, audit actual code vs contracts field by field, all 24 variants, all ten rules, status/price/tax/currency truth, no cross-scope changes, tests and actual source evidence. Only owner can authorize next phase.


---

# 08 - Phase 3 ADRs, traceable gaps and owner-review decisions

## A. Closed-for-P3.1 design decisions

| ID | Decision | Grounding |
|---|---|---|
| P3-ADR-01 | Engine uses ONE source-agnostic `AncillaryShoppingContext`; inbound Offer/Order/Direct mapping later | Owner explicit 2026-10-10 decision |
| P3-ADR-02 | Engine fetches active authored catalog/pricing/inventory configuration through read ports and returns one canonical typed result | Existing v12.2 roots, Owner decision |
| P3-ADR-03 | Context contains verified business facts required by ten Provision rule groups; unknown facts stay unknown | Existing v12.2 Rule Entities, 05/17 |
| P3-ADR-04 | Do NOT model 24 independent ARs or a dynamic EAV rule interpreter; reuse nine profiles and existing CustomerSelectionContract | Approved v12.2 ADR-01/02/11 |
| P3-ADR-05 | Candidate is NOT a purchase authority. Opaque buyer-facing ServiceOfferId/TTL storage is P3.2, after fully bound selection/quote | Approved v12.2 ADR-14 and owner separation |
| P3-ADR-06 | Availability in P3.1 never implies a Hold or guarantee; local capacity configuration and usage maximum are not measured remaining stock | Approved v12.2 07; code inspection |
| P3-ADR-07 | Engine does not map AirAvail DTOs, mutate Ordering, call FlightFlow Hold, issue EMD/ticket or modify current reservation endpoints | Owner separation and current source |
| P3-ADR-08 | P3.2/P3.3 implementation waits for independent preceding gate approval; source claims and blocked integration tracked honestly | Owner step-by-step request |

## B. Open contract decisions (do not resolve by guessing)

| ID | Needed by | Unresolved fact / owner or provider action | Agent handling |
|---|---|---|---|
| P3-OPEN-01 | P3.1 | Exact `Provision.Sequence` winner direction if not established by committed authoring logic | Audit and quote source; fail on collision until proven |
| P3-OPEN-02 | P3.1/P3.2 | Trusted full consumption ledger for PerOrder/PerFlight/PerPortion/PerServiceDate passenger limit | Return remaining Unknown and block final entitlement guarantee |
| P3-OPEN-03 | P3.1/P3.3 | Count/weight Inventory Resource registry authority | `BLOCKED_EXTERNAL_REFERENCE`; no fake records |
| P3-OPEN-04 | P3.1/P3.3 | Facility/airport IANA timezone reference | Block schedule/slot-specific verified sale |
| P3-OPEN-05 | P3.1/P3.3 | Counting-family reference | No fabricated cumulative entitlement or family membership |
| P3-OPEN-06 | P3.3 | FlightFlow delegated capability/seat-map uniqueness proof | Read-only/blocked pending verified contract |
| P3-OPEN-07 | P3.2 | AirAvail repository/live source response inaccessible in this review | Use consumer contract as explicitly lower evidence; require sanitized real fixture for closure |
| P3-OPEN-08 | P3.2 | Numeric FareFamilyId from an Offer whose wire FareFamily is `string`; typed FareType resolver; trusted local timezone/country IDs | Need authority resolver; never parse guess |
| P3-OPEN-09 | P3.2 | `LastTicketingDate` and actual Offer TTL are not the same documented datum | Keep separate; issuer of TTL must be identified |
| P3-OPEN-10 | P3.2 | What stage is created-but-not-ticketed Order? Provision only knows PreOrder/PostTicketed/OnBoard and `Both` predicate | Owner stage contract decision before supporting that pathway |
| P3-OPEN-11 | P3.2/P3.3 | Exact public `AddService` commercial contract in Ordering | No Ancillary-owned mutation; separate Ordering change approval |
| P3-OPEN-12 | P3.3 | Owner-confirmed single-holder routing for A07/A08; avoid double FlightFlow Hold; A09 EXST extra capacity + eTicket | Keep source boundary and block unsupported variants |
| P3-OPEN-13 | P3.3 | A10 Upgrade external quote, inventory, exchange and ticketing provider | QuoteRequired/Blocked without live verified contract |
| P3-OPEN-14 | P3.3 | Supplier-specific partial release/confirm/cancel/readback and retry semantics | Do not promote Pending/Unknown to Held/Confirmed |
| P3-OPEN-15 | P3.1/P3.2 | ReferenceData Currency DecimalPlaces integrity (previous sync defect) | No guessed default 2; block unsupported/incorrect authoritative data |

## C. Provenance classification

- `SOURCE_VERIFIED`: directly examined code at a pinned SHA and correct authoritative source.
- `CONSUMER_CONTRACT_VERIFIED`: Ordering's consumer DTO/Mapper proves its expectations, not AirAvail provider completeness.
- `BENCHMARK_SAMPLE`: Lufthansa JSON reflects a sample external interface, not its guaranteed backend.
- `DESIGN_DECISION`: required new model/adapter/reservation semantics, subject to owner gates and conformance.
- `BLOCKED_EXTERNAL_REFERENCE`: cannot fully fulfill without actual reference/provider contract; NOT a failed domain test and NOT success.

## D. No silent source reconciliation

The older Phase3 design-only text contains an `Ancillary -> FlightFlow` delegation step, while actual Ordering already uses FlightFlow for Air+Seat capacity. P3.3 must not unilaterally implement both. Record owner review of the single routing decision before reserving seats, especially A09. Current P3.1 does not touch it.

Likewise, the v12.2 Phase2 closure report was generated for an earlier audit HEAD and was committed in Ancillary commit `bee6ad6` together with FlightFlow occurrence reference. It explicitly reports `PHASE2_AUTHORING_CLOSED=yes`, `PHASE2_FULLY_CLOSED=no` and 4 unconnected sources. Refresh on any new commits and do not assume that a committed report itself means integration proof.

