# AeroTech.Ancillary — Source Audit of S2-01 @520e493 (Owner Review)

Date: 2026-10-20. Repository: `aliifarhadi/AeroTech.Ancillary`, branch `k8s-stg`, HEAD `520e493a53faee724bb60592b691680a4c8d705f`; prior HEAD `7efee95cbd01b5ff30bd4c8c36326debc5020574`. The GitHub compare API confirms **one commit ahead**. This review is scoped ONLY to Ancillary Stage 2, not identity, account administration, changes in other services, or Stage 3.

**Verdict: S2_CODE_IMPLEMENTED_WITH_BLOCKERS — NOT S2_CLOSED.** The agent's `S2_INTERNAL_CONTRACT_READY` is a progress label, not a demonstration that the full contract is delivered. Operational Order flows remain `EXTERNAL_PENDING`, as the current `NotConnectedOrderCommercialContextReader` yields 16815/503.

## A. Observed implementation

- Three new `POST /{Backoffice,Api,OtaPanel}/v1/AncillaryOffers/Selections/EvaluateBatch` routes; existing single-Evaluate routes retained.
- Offer/Order source dispatcher, Order context port, and missing-connection fail-closed provider.
- JWS/HMAC-SHA256 stateless `pricedSelectionRef` for Order-bound selections, no Quote table or custom Identity subsystem.
- `POST /Service/v1/Ancillaries/Selections/Resolve`, re-evaluation of source, commercial-version check, separate `service` and `includedSeat` outputs. No Hold/Payment.
- SeatMap's three existing route/response shapes remain. No commercial Category => physically free eligible seat, consistent with the last Owner override. Occupancy missing a flight is rejected.
- Price units and free-price calculations materially improved; fingerprint expanded; synthetic Order/Seat/Token tests added. A19 remains fail-closed without verified guardian contacts.
- **Important functional limitation:** OfferId-only `EvaluateBatch` intentionally issues **no pricedSelectionRef**; a commit-capable selection must be evaluated against `orderId` when the Order exists.

## B. Blocking discrepancies, evidenced in committed source

### B1 — Resolve is not an adequate Ordering handoff for typed services (BLOCKER)

`src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Dto/Service/ServiceResolvedServiceDto.cs`: returns `Kind, ServiceDefinitionRef, DefinitionVersionId, ProvisionId, PricingRevisionId, Rfic, Rfisc, SsrCode, CoverageScope, OrderTravellerIds, TravellerRefs, FlightIds, PortionRefs, SeatNumber, FlightCapacityId, SelectedOptions, Price, RequiresCheckAtReserve`.

`.../Queries/ResolveSelections/Service/ServiceSelectionResolutionMapper.cs:30-52`: maps only option choices via `OfferSelectionResultsMapper.SelectedOptions`, NOT full `AncillarySelection` (the signed token holds full selection). It also omits `VariantCode`, `ServiceTypeCode`, `ServiceSubCode`, `BookingMethod`, `DocumentRouting`, `DocumentType`, `ConfirmationRequirement`, and provider classification/evidence required for downstream service fulfillment/document routing. For A03/A04/A06/A13/A14/A17/A20/A22/A24, typed measurements, equipment, documents, time, guest/device values may be lost. Even where the Price field contains count, that does not reproduce the original user selection. Source: `src/AeroTech.Ancillary.Shopping/Selection/AncillarySelection.cs`.

**Action:** expose a precise, typed, canonical `Selection` (all customer inputs needed to fulfill the service), service/subcode and present booking/document facts already known on the canonical candidate. Do NOT fabricate external provider keys or demand new fields/aggregates from Ordering. Guard missing OrderTravellerId for passenger-scoped service; valid Order-scoped service needs no dummy traveller. Add round-trip tests for Quantity, Pet, Appointment, WiFi, SSR, Free and Seat.

### B2 — A08 Exit Row cannot be purchased through the new SeatMap Batch route (BLOCKER)

`.../Queries/EvaluateBatch/{Api,Backoffice,OtaPanel}/*SelectionLineInput.cs`: Seat line has only `seatMapSelectionId`; `EvaluateBatchService.WellFormed` explicitly rejects Seat lines containing `selection`. The seat path `Seat(...)` obtains an existing `SeatTravellerOffer` and tests readiness, with no place to send `ExitRowTermsAccepted`. Agent's own report §9 flags the gap. Preserve frozen SeatMap JSON/IDs; permit a minimally scoped `seatSelection` or `exitRowTermsAccepted` on the **new batch input**, and validate original seat identity and required terms via the existing engine at Evaluate AND Resolve. Never accept a self-declared eligibility proof in lieu of trusted facts; no new Seat API required.

### B3 — List still lacks complete bounded inline option contract (CONTRACT BLOCKER)

`.../Dto/Backoffice/BackofficeAncillaryOfferDto.cs`, `.../Dto/Api/ApiAncillaryDto.cs`, `.../Dto/OtaPanel/OtaPanelServiceDto.cs`: existing List shapes contain Quantity + required fields but no verified `optionGroups`, `interaction.requiresDetails`, `isComplete`, or specific constrained inline choices. `OfferOptionCatalog` is only used when creating Details; recent diff did not change List response mappers. Master and `docs/ancillary_s2_execution_pack/03-API-AND-INTEGRATION-CONTRACT-FREEZE.md` require a usable single-call List for simple products and on-demand Details for large/dynamic forms.

**Action:** additive response fields in each surface, sourced from existing OptionCatalog/Definition/Provision, bounded without per-product N+1 I/O. Preserve existing envelopes and SeatMap. Add golden/semantic tests including 20 offers + 1 details call, source unknown vs truly empty. No new domain entity, no per-variant service endpoints.

### B4 — Basket duplicate rule rejects some distinct selections (CONTRACT BLOCKER)

`.../Queries/EvaluateBatch/SelectionBasketRules.cs:12`: groups **all** Ancillary lines by `Item.OfferItemRef` and flags the whole group as `DuplicateSelection`, regardless of selection value. This conflicts with S26 of Master Pack: two legitimate distinct selections from the same Definition/candidate may differ in time, option, reference or coverage. Must validate actual business conflict scope, not prohibit every repeated candidate by hash. An **exact duplicate** can remain blocked. Add paired tests for distinct typed selections allowed when provision permits vs same event/beneficiary/coverage duplicate rejected. If the domain disallows multiples for a variant, respect that sourced cap.

### B5 — Order traveller mapping can silently return empty (HANDOFF VALIDITY)

`.../Common/Orders/OrderShoppingContextMapper.cs`: absent mapping from source TravellerRef to the Order traveller leaves `OrderTravellerId` null. `.../Service/ServiceSelectionResolutionMapper.cs:42` uses `.OfType<long>()`, silently yielding an empty OrderTravellerIds list while status may remain `Valid`. For beneficiary-required products, fail-closed when Order has no matching traveller; no dummy mapping, no new identity service. For genuine Order-scoped products allow empty list. Add negative test.

### B6 — List option-ref binding not implemented at Batch input (CONTRACT DISCUSSION, no invented field)

`.../Queries/EvaluateBatch/{Api,Backoffice,OtaPanel}/*SelectionLineInput.cs` has no `selectedOptionRefs`, although API Freeze §5 describes it. The existing engine validates authored values, so this is not proof of an authorization bypass. Do not introduce mandatory refs for free text / typed values. Decide a narrow optional field and validate an explicitly issued optionRef against the actual current candidate/field/value only when supplied or when the final frozen contract requires it. Keep codes and error handling already present.

### B7 — Resolve signs SourceVersion and seat facts, but does not compare them to live values (BLOCKER for Seat revalidation)

`.../Common/PricedSelections/PricedSelection.cs` carries `SourceVersion`, `Seat {TravellerRef, FlightRef, FlightId, FlightCapacityId, SeatNumber}`, `ServiceDefinitionRef`, `DefinitionVersionId`, `ProvisionId`, `PricingRevisionId`, `CoverageScope` and `Units`. However `.../Queries/ResolveSelections/ResolveSelectionsService.cs:90-115` accepts `Valid` on `OrderCommercialVersion`, `CurrencyId` and `Total` plus fresh candidate re-evaluation. For generic ancillaries the offered hash binds source/candidate revisions; for **SeatMap**, `OfferItemRef.OfSeat` hashes neither the source version nor the aircraft/flight version, and no explicit comparison is done to `token.SourceVersion` or token seat/coverage facts. A layout/flight-capacity/source change with the same seat identifier and price could therefore be reported `Valid` absent an OrderCommercialVersion bump, notwithstanding the signed facts. Preserve the legacy ID but compare signed source-version and seat/flight identity to current authoritative facts at Resolve; reject Stale on mismatch. Add price-unchanged source mutation tests. Never assume Ordering version always increments on supplier seat/aircraft changes.

### B8 — Partial Basket can receive valid-looking tokens despite `allReady=false` (CONTRACT CLARIFICATION)

`.../Queries/EvaluateBatch/EvaluateBatchService.cs:68-74` issues a signed token to every `line.CanPurchase` before building the Basket's `allReady`; `SelectionBasketRules.Basket` independently checks all lines. Owner override v1.1 says no *committable* token for an incomplete/unverified Basket, but user also expects a reference for each individually valid selection. Make the chosen behavior consistent: either keep per-line tokens explicitly noncommittable until basket valid, or withhold tokens for the whole Basket while `allReady=false`; test an all-valid + one invalid case. Do not invent basket persistence or force all purchases into one atomic Order transaction.

## C. Open scope / external, not Ancillary code blockers to solve by redesign

- **Ordering connector absent:** `src/AeroTech.Ancillary.Providers/Orders/NotConnectedOrderCommercialContextReader.cs` explicitly fails 16815/503. End-to-end HTTP acceptance (Order created → List → EvaluateBatch → Resolve → Add to Order) cannot be demonstrated against real Ordering yet. This must stay `EXTERNAL_PENDING`; do not implement or alter Ordering in this repository.
- **A04, A09, A10, A13/14, A17, A19** have intentionally fail-closed catalog/scenario gaps. Passing 25 synthetic variant tests does not mean 24 production-purchasable variants. Report truth per variant `Supported | NeedsQuote | NeedsVerification | UnsupportedSource`; do not invent facts or open Stage 1.
- **Shared fees:** currently `PriceAssessor` prevents a purchasable line with unapplied shared fees; Basket reports incomplete. This is a safe fail-closed result, not a reason to add an unapproved fee allocator to S2. Include an acceptance test that Resolve never returns Valid for a non-fully-priced fee scenario.
- **Agent report outdated:** `reports/S2-IMPLEMENTATION-CONFORMANCE-v1.1.md` claims changes uncommitted, but commit `520e493` is pushed. Update evidence/revision in a new report, without rewriting history.
- **Tests:** agent reports 317 Shopping, 259 Domain Conformance and 224 Application Acceptance passed (`800` in total), but this review could not independently execute .NET tests; no GitHub workflow runs or commit statuses were present for this SHA. Source and test code were inspected, not run. Do not mark `S2_CLOSED` until test and contract evidence is verifiable.

## D. Acceptance before Owner closes Stage 2

1. Exactly the desired public routes and no new identity / persistence subsystem; `SeatMapsDto` frozen.
2. `List` 3 surfaces: simple products editable inline with real bounded options; Details on demand.
3. `EvaluateBatch`: typed ancillary and seat selection, pricing, one token per accepted Order-bound selection; no token for rejected/incomplete.
4. `Resolve`: authentic token re-evaluated, current Order version, precise canonical selection+commercial+document metadata+OrderTravellerIds for each line, no dummy fields. Specific rejected statuses for changed/unavailable sources.
5. Basket preserves valid distinct selections and blocks true duplicates and mutually exclusive seats; shared fees never silently vanish.
6. At least one complete paid seat, one category-free seat, one exit-row conditional seat test through SeatMaps → EvaluateBatch → Resolve.
7. Pinned existing single routes, `SeatMapsDto`, Domain S1, no Quote table, no new Infrastructure auth. Report actual executed tests separately from external Integration pending.

Links: https://github.com/aliifarhadi/AeroTech.Ancillary/commit/520e493a53faee724bb60592b691680a4c8d705f and path-based references above on this SHA.
