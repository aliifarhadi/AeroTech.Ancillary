# CODING AGENT — P3.2.A Backoffice AncillaryOffers by AirAvail OfferId

## Mission / execution authority
Implement **one actual Backoffice read-only endpoint** in `aliifarhadi/AeroTech.Ancillary`, branch `feat/ancillary-v12.2-phase1-phase2-rebuild`, using existing P3.1 Engine and **AirAvail `Service/v1/FlightOffers/Details` as producer**. The owner wants a usable BackOffice list of ancillary offers for the selected (priced) AirAvail OfferId, with passenger, flight, bound, fare, baggage, eligibility, price/fees, availability, selection contract and explanatory statuses. It must be credible for an airline/PSS, not a demo. **Do not start Hold, Confirm, ordering changes, payment or EMD.**

Read all four documents in this pack in order:
1. `00-AUTHORITY-SOURCE-AUDIT.md`
2. `01-FLOW-CONTEXT-MAPPING-AND-BOUNDARIES.md`
3. `02-BACKOFFICE-ENDPOINT-AND-RESPONSE-CONTRACT.md`
4. `03-BENCHMARK-TESTS-CLOSURE-GATES.md`
They are normative for this task. Also read existing Phase3 v1.0 and v12.2 authority. Where current code contradicts the pack, document source evidence and resolve minimally, never guess. Primary external evidence is **producer AirAvail source**; Ordering is the reference implementation of HTTP calling/consumer mapping. Verify all Git HEADs fresh before coding.

Pinned source:
- `AeroTech.AirAvail k8s-stg@8c6f5d18e17caf9eb47dfd039c20df11f76840f9` — its root `AeroTech.AirAvail/AirOffer/src`, actual `ServiceController`, `ServiceFlightOfferDetailDto`, `ServiceFlightOfferDetailService`, `ServiceFlightOfferDetailMapper`, `JourneyOfferIdCodec`.
- `AeroTech.Ordering.Final k8s-stg@cd50a2a5fd18a372f32f2d4efac08dee0510f6ba` — `OfferProvider`, `FlightOfferDetailResponse`, `OfferResponseMapper`, `OfferReader`, Provider DI.
- `AeroTech.Ancillary feat/ancillary-v12.2-phase1-phase2-rebuild@856ccbe8b41bb46439d8d27b52ad647c4cf9c054` — source `Ancillary.Shopping` P3.1 **R1 already committed** and authoring phases frozen.

## Required implementation, nothing extra
**HTTP:** `POST /Backoffice/v1/AncillaryOffers/` with body `{ "offerId": "..." }`. Backoffice authorization exactly `SurfaceAuthorization.Backoffice`, actor/airline scoped. Existing `BaseResult<T>` style when applicable. Do not add client-supplied tenant/POS/currency/quantity/availability override parameters.

**Application flow:** authorize → resolve owner, actor and correct unique selling POS based on authoritative scope → call AirAvail `FlightOffers/Details` with exactly same POST/JSON-envelope semantics as Ordering → validate response and source commercial association → map to `AncillaryShoppingContext` `ShoppingStage.PreOrder` with exact traveler/coupon/fare/baggage/bound joins → call **existing** `IAncillaryShoppingEngine.ShopAsync(context)` → project to Backoffice DTO with travel details and typed offers/blocked reasons as pack 02 → return.

**Implementation placement:** reuse solution architectural conventions (Backoffice controller/handler/application service, Providers typed HttpClient, Domain port if house convention, new narrowly scoped mapping/projection class). No new root, DB table, read-model snapshot, Pricing rules, inventory engine, event, UI or Infrastructure package. Do not duplicate existing P3.1 rules or introduce a new 24-profile switch. Reuse its `CustomerSelectionContract` and Canonical results.

**Critical correctness requirements:**
- Real AirAvail producer includes `PricedAt`, `JourneyType`, `AirTransports/BoundOfferId`, `Tickets/Coupons`, `PricingUnits`, `OrderCharges`, `RatesOfExchange`. Do not assume Ordering's older wire type is exact producer. JSON numeric strings accepted where declared; enum converters consistent with producer.
- Decode/sales commercial context by **source protocol**, not homegrown client trust: current AirAvail codec is plain text and not a signed authorization token; caller Auth + seller scope must be validated. If commercial identity cannot be matched from producer DTO, explain exact missing reliable contract. Do not claim `OfferId` authenticates an agency.
- Current AirAvail `ServiceFlightOfferDetailService` recalculates prices, and maps `PricedAt` from encoded `SalesDate`: it is NOT proof of fresh quote. `LastTicketingDate != OfferExpiresAt`. No invented TTL.
- `OwnerAirlineId` + `PointOfSaleId` cannot be guessed from a flight/OfferId. `ICallerContext` has AirlineOfficeId, not PointOfSaleId. Audit existing AirPrice POS/office reference. Use verified unique reference mapping or fail closed `POS_CONTEXT_UNRESOLVED` (explicit blocker, no global default). Do not change AirPrice or AirAvail to hide this gap.
- `FareFamily` is string, published Provision FareFamilyId long; no fabricated ID. `DateOfBirth`, real seat map, IANA zone/country, verified passenger usage, fare-included benefit evidence do not magically come from offer details. Propagate unknown for rules that need them; don't mark `ExistingServicesComplete` true without evidence or zero usage due to missing source.
- Build correct flight refs, covering every traveler/coupon/flight/portion, `BoundOfferId → BoundId`, fare reference, baggage allowance with unit; a technical leg is NOT extra priced coupon. `FlightCapacityId` NOT ancillary local stock.
- Display `Offers` and `BlockedOffers` accurately: not all returned candidates are immediately purchasable. No newly fabricated `serviceOfferId`; `CandidateIdentity` explicitly only an address, not an authorized bearer/purchase token. No `Held`, `Guaranteed`, EMD `Issued` claims.
- **Never call** AirAvail's existing `/AncillaryOffers` or `/AncillaryOffers/Details` for this functionality: that is a distinct legacy catalog/evaluator limited to old product scenarios; Ancillary v12.2 Engine supplies commercial offers. Leave AirAvail code untouched.

## Mandatory workflow and proof
**STEP 0: source contract audit** before changes. Pin `git rev-parse HEAD`, `git status`, inspect all producer/consumer files above and current Backoffice authorization. Write `reports/P3.2A-SOURCE-AND-COMMERCIAL-MAPPING-AUDIT.md` with producer/consumer drift, exact POS/office source, mapping completeness and true blockers. Do NOT start implementing prices with guessed IDs.

**STEP 1: implement smallest valid read-only vertical slice.** Add HTTP client, mapper, application orchestration, controller, DTO and DI. Keep Engine untouched unless a **reproducible** integration defect cannot be solved in Adapter; any proposed Engine change must be separately justified with failing test and authority impact. Do not reopen P1/2, reservation or other repos.

**STEP 2: tests** from `03` (CT-01..CT-26). Minimum automated: truthful HTTP Backoffice, auth, actual producer DTO fixture, multi-bound/PNR-less journey, PTC/coupon, FareFamily ambiguity, taxes+fees, no POS fallback, unknown zones, missing reference, no provider hold, SQL Server catalog integration, existing P3.1 regression. Use a **sanitized capture of actual AirAvail FlightOffers/Details** if available and verify against source code; no fictional real fixture. For real live smoke, use staging only with legitimately configured credentials, NEVER check secrets into repo, log raw OfferId or personal profiles. Run host HTTP end-to-end before closing; no success based only on mocked DTO.

**STEP 3: accountable report** `reports/P3.2A-BACKOFFICE-OFFERID-CLOSURE-REVIEW.md`: exact SHA/current working tree, changed paths, producer URL and request shape, concrete mapping table per field, verified DOB/POS/timezone/FareFamily data sources (or `BLOCKED`), complete sample redacted response, counts and command lines for tests, availability of staging and live results, gaps, owner decisions and the gate verdict. No unilateral push/merge/deploy; give owner patch/PR instructions as needed.

**STOP CONDITIONS — not authorization to invent:**
- Cannot authoritatively resolve PointOfSaleId from OfferId+authorized caller+verified POS reference: implement secure fail closed, mark real-catalog E2E BLOCKED; ask for exact commercial POS contract. Do not change public request to sneak a PointOfSaleId or use the first DB record.
- Real AirAvail wire differs from pinned producer: correct contract from current producer and its tests; report drift.
- Provider not reachable due to credential/network: keep HTTP integration test with vetted stub and report actual live NOT RUN; no fake green E2E.
- No full passenger DOB: products needing age remain `NeedsVerification`, others may be discoverable.
- `CandidateIdentity` re-evaluation is not quote purchase authority. Do not build token store/Order mutation now.

## Required closure labels
`P3_2A_READY_FOR_OWNER_REVIEW` only when core tests pass. `P3_2A_LIVE_E2E_VERIFIED` only after authorized real host + actual AirAvail HTTP success **and** owner/POS matches a published product with realistic evaluated response. If external dependencies remain, clearly state `P3_2A_PARTIAL_BLOCKED`, specific need and next owner action. No decorative DONE.

## Final deliverable
Send a short technical summary in Persian, exact code refs/commit, HTTP request/response proof, tests and blockers. Provide a downloadable Markdown report and proposed next action; no expanded architecture diagrams or feature backlog unrelated to this endpoint.
