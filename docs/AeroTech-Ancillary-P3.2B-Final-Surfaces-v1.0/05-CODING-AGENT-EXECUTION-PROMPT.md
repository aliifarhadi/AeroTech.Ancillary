# Coding Agent — P3.2B FINAL: Engine-First, Three Modular Surfaces, Details and Selection

## Your role

You are coding agent; implement code/tests only. Owner approves business policy and architecture. **No invented concepts, no speculative fields, no broad rewrites, no extra services.** Deliver a genuinely working Phase 3.2 (read-only offer shopping + details + selection-evaluation) across **Backoffice, Agency API, OtaPanel** with independent DTOs/mappers and one source-neutral engine. Read all docs `00`–`04` in this same pack **before editing**. The `00`–`04` pack is the controlling P3.2B spec, with canonical P3.1 and published v12.2 Phase1/2 as lower-level authority; benchmark only supplies demonstrated patterns, not fictional airline prices.

## Pinned sources / branch

- Edit ONLY `aliifarhadi/AeroTech.Ancillary`, branch `feat/ancillary-v12.2-phase1-phase2-rebuild`. Observed HEAD `d598ff7082b14483940f32dab1a7a053eae47b5d` (check new HEAD before editing, inspect any later commits).
- Read `aliifarhadi/AeroTech.AirAvail`, `k8s-stg@ac840401d38cb407af339d6fa07f98e052b1bd04` at `RestApi/V1/AirOffers/Controllers/{Backoffice,Ota,OtaPanel}`, `Services/Services/{BackofficeSearchFlightOffersService,OtaSearchFlightOffersService}`, `RestApi/_Shared/Services/AirOfferIdentityService.cs`, and producer `Service/v1/FlightOffers/Details` DTO/Service/Mapper. **Their commercial request/identity behavior is authoritative for the matching Ancillary surface.** Pin any newly observed source SHA in report.
- Read `aliifarhadi/AeroTech.Ordering.Final`, `k8s-stg@cd50a2a5fd18a372f32f2d4efac08dee0510f6ba` `Providers/Offer/Services/OfferProvider.cs`, response envelope and mapper to corroborate AirAvail consumption (read-only).
- Read existing Ancillary `P3.2A-R1` report, Phase3 pack and Domain/Shopping tests. Do not copy legacy AirAvail `AncillaryOffers` engine (only FlightOffers/Details). Do not write to AirAvail, AirPrice, Ordering, FlightFlow.

## Nonnegotiable architecture

**One canonical input (`AncillaryShoppingContext`) + one rule/pricing engine (`IAncillaryShoppingEngine`) + one canonical output (`CanonicalAncillaryOfferResult`/selection evaluation).** The reusable orchestration must end in a **surface-neutral snapshot** (FlightOffer + canonical context/result(s)), not `AncillaryOffersDto`. Each Surface owns **its own** inbound contract, outbound contract, mapper, controller, query handler and trusted sales scope resolver. Keep current engine/business model intact unless a SPECIFIC regression proves a bug.

Three routes:
```
POST /Backoffice/v1/AncillaryOffers
POST /Api/v1/AncillaryOffers
POST /OtaPanel/v1/AncillaryOffers
```
Backoffice request `{offerId, options?}` same five AirAvail options and semantics. Api and OtaPanel request `{offerId}` only, same as AirAvail `FlightOffers/Price`; customer/agency/office/country from trusted claims / `X-Country`, channel API, currency current AirAvail default 70 and clock sale instant. Separate surface auth policies (`SurfaceAuthorization.Backoffice`, `.Ota`, `.OtaPanel`), not just route aliases. The BO explicit `pointOfSaleId` must retain AirAvail's behavior, **do not add one-sided authorization or change input semantics**. Fix source contract discrepancies only if verified; explain cross-service policy problems without unilateral changes.

## Do it in this order (one implementation, phase gates; no approval ping-pong)

### STEP 0: exact audit with evidence
- Commit/path dependency map of source and target. Confirm existing `GetAncillaryOffersService` returns Backoffice-shaped common DTO and only one controller exists. Trace `ShopAsync`, `PriceAssessor`, `QuantityAssessor`, `AvailabilityAssessor`, `EvaluationSelection` and module references/DI. Capture exact AirAvail Api/OtaPanel auth/claim semantics, multi-POS behavior and OfferId/Details rules. Report only actionable discrepancies.

### STEP 1: Engine/pricing domain tests BEFORE any surface refactor
- Run original Domain, Shopping, Query/SQL, Acceptance tests and all R1 regression IDs, 24 variants and 10 rule groups. Add **business** tests that demonstrate correct price: base+added tax+item fee; included tax counted once; non-Item fee not part of unit amount; Free and ExternalQuote amount semantics; target-currency rate/scale; `PerPiece`, `PerItem`, packaged 10kg, true `PerKilogram` billable quantity when verified; distinguish searched offer price from final selected amount. Fix only proven engine errors and add failing-then-passing tests. No invented kg formula: when weight basis or excess delta policy is unknown return incomplete/null, not guessed total. Treat inventory/status and missing evidence as sourced.
- Explicitly report `ENGINE_ACCEPTANCE_PASS` or blockers before moving on; do not hide blockers by changing tests.

### STEP 2: clean shared orchestration
- Refactor common `GetAncillaryOffersService` into **canonical** service. It consumes source FlightOffer, validates joins, enriches facts, maps canonical context, invokes existing ShoppingEngine, returns app read snapshot. Common service must NOT reference `Backoffice...Response` or `AncillaryOffersDto` or other surface output types.
- Move source-specific `Shopping/FlightOffers` adapter responsibilities into an appropriate app/integration boundary without dependency cycles; leverage existing projects, avoid unnecessary new csproj/aggregates. Provider alone owns AirAvail wire. Surface-specific `SalesScopeResolver` constructs trusted scope; common helper can implement identical Api/OtaPanel commercial policy.
- AirAvail eligibility yields a **set** of allowed POS IDs, not a mandatory singleton. P3.1 Engine expects one POS -> invoke engine separately per eligible POS and combine scope-preserving canonical results without price mixing or arbitrary first POS. If reference source is still NotConnected, provide typed `BLOCKED_EXTERNAL_REFERENCE` and do not claim API live parity. Do not hardcode office as POS.
- Verify `Channel` enum ordinals differ across repos; map named semantics, not integers.

### STEP 3: THREE distinct DTO responses & controllers
- Put every Surface's Request, Response and Mapper in separate surface folder/namespace; distinct types even where one request property is identical. **API is NDC-inspired normalized JSON**, with `dataLists.{passengers,paxSegments,serviceDefinitions}` and `aLaCarteOffers[].offerItems[]`, each item with PTC/segment coverage, price unit/taxes/fees, quantity, selection requirements, readiness/verification. Do not dump operational/backoffice diagnostics or internal revision/provision IDs into public API; do not claim certified IATA NDC protocol. BO = rich operator journey/blocked reasons, Panel = traveler/flight-grouped sales UI.
- Use a read-only opaque ItemRef bound to trusted offer/context; don't pretend it is a bookable OfferItemId. One and only one enum-to-display mapping per Surface (pure presentation). Do not recalculate any prices in mappers.
- Golden JSON snapshots for **BO, API, Panel**, examples in Swagger/OpenAPI using actual compiled DTO serializer and sanitized real AirAvail fixture. Ensure existing BO route & payload compatibility unless explicit contract change is justified.

### STEP 4: Details and selection support
- Three `POST /{Surface}/v1/AncillaryOffers/Details` with separate DTOs, BO options same AirAvail, API/Panel no options. Implement canonical detail read model from **published** service Definition/Specification, chosen Provision and authoritative Pricing/Inventory read model. Re-fetch/re-evaluate source + scope; do not return product just by arbitrary DB ID. Baggage A01–A06 details must include included allowance, allowed bag weights/dimensions, rate tiers if published, restrictions, selection schema, breakdown, refund/confirmation terms **only if modeled**. Seat A07–A09 detail exposes rules/characteristics, NOT fabricated seat rows. Profiles A10–A24 get typed available data; no invented blanket shape.
- Three `POST /{Surface}/v1/AncillaryOffers/Selections/Evaluate` with scoped typed selection. Call existing `IAncillaryShoppingEngine.EvaluateSelectionAsync` and return actual candidate changes, trusted amount or incomplete/quote state, never guarantee a hold or issue EMD.
- Define separate SeatMaps endpoint with trusted provider interface. Implement real integration if source exists and is verified. Else explicitly respond `SEAT_MAP_SOURCE_UNAVAILABLE` and report it open; do not fill fake seat rows and do not mark seat availability checked. **This is not a pretext to reopen Stage1/2 reservation.**

### STEP 5: prove cross-surface commercial equivalence and real runtime
- Tests include: BO options accepted like AirAvail; Agency API/OtaPanel ignore/reject forged BO fields; `X-Country` & agency claims used, authorized POS set (0/1/many) with no made-up default; same trusted shopping context -> same candidate eligibility/price/status across all three despite JSON differences. Wrong-surface auth denied before upstream request; no office/agency secrets leak to API.
- Real HTTP integration with AirAvail staging, real readable Ancillary SQL catalog and surface credentials for all three (if missing credentials/real POS service, **mark blocked**, do not present mock as live). Test Details with A01 + seat metadata; negative quoted upgrade, missing timezone/fare-family/seatmap, stale/refused Offer, invalid coupon joins and taxes.
- Build/tests + exact source SHA and fixtures; no changes to Ordering, AirAvail, FlightFlow, DB migrations/booking/reservation. Run deletion/reference audit before removing common old DTO. Document all changes with line links + test proof.

## Closure deliverables

1. `reports/P3.2B-SOURCE-ARCHITECTURE-AUDIT.md`: source SHA, current/target architecture graph, AirAvail three-surface parity table, POS-set and enum mapping.
2. `reports/P3.2B-ENGINE-AND-PRICE-VERIFICATION.md`: actual test fixtures with rate/fee/tax math and results; each known source gap explicit.
3. `reports/P3.2B-SURFACE-DTO-TRACEABILITY.md`: API NDC-inspired field-by-field mapping and benchmark URLs; BO/Panel differences. Exact request & response examples and Swagger/OpenAPI schema proof.
4. `reports/P3.2B-DETAILS-AND-SELECTION-VERIFICATION.md`: Baggage A01–A06 coverage, Seat rules/SeatMap status, selection/quote proof, no invented fields.
5. `reports/P3.2B-FINAL-CLOSURE.md`: scope, HEAD/commit, commands, pass/fail, three real HTTP traces, exact unresolved external dependencies. Verdict must be one of `P3_2B_CLOSED` or `P3_2B_PARTIAL_EXTERNAL_BLOCKED`; no false claim. DO NOT stop at happy-path mocks or 24 shapely DTO fixtures.

## Hard constraints

Do **not** reopen closed Stage 1/2 domain, do not copy legacy AirAvail Ancillary code, do not change another repository, do not invent financial formulas, do not add 3 pricing engines or one general surface-switched DTO, do not fabricate provider seat maps / POS eligibility / expiry / EMD. No endless micro-fix loop. If external dependency truly prevents closure, leave an exact typed blocked response, explain and continue all independently executable work before reporting. Prefer source-backed small cohesive changes over redesigning the entire project.
