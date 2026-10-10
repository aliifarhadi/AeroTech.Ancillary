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
