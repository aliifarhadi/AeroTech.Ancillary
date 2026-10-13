# BACKEND CODING AGENT — EXECUTE v12.3 ONLY

**READ THIS FILE AND 00–12 + 14–17 OF THE SAME v12.3 PACK BEFORE CHANGING CODE.** You are the coding agent, not the architect. Implement exactly the approved Ancillary-only P1/P2 correction. Do not ask the owner about historical data: **there is none**. Do not use v12.2 or earlier Seat Policy prompts as authority. Do not modify any other microservice repo. No P3 Hold/Reservation/Confirm/Order implementation.

## Why the owner ordered this change
The current implementation wrongly made sale/activation of **commercial Seat category pricing** depend on an `AncillaryInventoryPolicy` delegated to FlightFlow. In `AncillaryInventoryPolicy.Activate`, FlightFlow evidence is required, but the registered provider returns `SourceUnavailable`. This is a **wrong service boundary**: FlightFlow owns actual seat map and physical availability; Ancillary owns airline's commercial Seat product type, applicability and price. The correct model has no Seat inventory policy or FlightFlow client in Ancillary. A separate external composition layer may eventually combine SeatMap and priced SeatOffers, but building that integration is NOT this phase.

The owner also explicitly reminded us that **P2 Shopping already exists and must be repaired now**. Your job includes existing `AncillaryShoppingEngine` and surface APIs, not only Backoffice CRUD. Distinct SeatOffers and general AncillaryOffers are consumer contracts on top of one canonical Shopping Engine, not justification to duplicate its logic. IATA/Amadeus show distinct seatmap/service-list consumer concepts; this does not establish Amadeus internal microservice design. Never claim you have implemented a real SeatMap.

## Environment and authority
- Repo: `https://github.com/aliifarhadi/AeroTech.Ancillary`, branch `k8s-stg` as reviewed at `8fda5f1e8c2586a08fa995a0f8701e9299182986` on 2026-10-13. Check actual new HEAD, compare to this base, read current code; do not silently overwrite later merges.
- Order: v12.3 pack > live code patterns > official industry facts for terms. Older v12.2 only provides background, not conflicting requirements.
- Deliverable branch/commit per normal owner workflow; no uncontrolled unrelated source edits.

## Execute in this order — ALL tasks mandatory

### T0. Inventory source baseline without touching code
Record HEAD, working tree, `git diff --stat`, `dotnet --version`, solution names, current test discovery. Record precise current routes for all three Shopping surfaces, Seat/Provision/Pricing CRUD and Policies. Save baseline test output; if it already fails, distinguish pre-existing faults from your changes. Report before/after file list. No setup of FlightFlow server.

### T1. Domain/contract cleanup, no Seat policy
1. Remove `InventoryAuthority.FlightFlow=4` from project-owned live enum; **do not renumber** Unlimited=1, Local=2, Supplier=3. No historic data makes old enum round-trip preservation unnecessary. Remove every switch and evidence property referring to FlightFlow. Keep nonSeat Local/Supplier/Unlimited.
2. Reject `AncillaryProfile.Seat` in InventoryPolicy **Define, Change, Activate** (not just frontend). Reuse typed `InventoryCommercialFactsReader` to resolve Definition ID, stable ref, Profile, owner; avoid three duplicated queries and do not mistake current Definition pointer for physical seat capacity.
3. `Seat` returns unambiguous inventory **NotApplicable** state, not `NotConfigured` or `Unlimited`; if needed append `InventoryCapacityReadState.NotApplicable=8`. Do not create a fake policy record or fake guarantee.
4. Keep all Count/Weight/Slot physical ARs for nonSeat, concurrency/idempotency, supplier validation, and PassengerUsageLimits.

### T2. Remove direct FlightFlow and fake SeatMap surface
5. Remove `IFlightFlowDelegationReference`, provider stub, DI registration, and FlightFlow branching in `InventoryPolicyEvidenceBuilder`, `InventoryReferenceProbe`, `AvailabilityAssessor`.
6. Remove `IFlightSeatMapReference`, `NotConnectedFlightSeatMapReference`, `GetSeatMapService` + its Api/OtaPanel/Backoffice Handler/Query, and three `/AncillaryOffers/SeatMaps` stub routes. Update OpenAPI/DI and tests. Do NOT replace with a fake seatmap or direct FlightFlow proxy.
7. Existing `IFlightOccurrenceReference` is used to verify **other products'** flight-based Count/Weight; eliminate concrete `FlightFlowFlightOccurrenceReference` HTTP client/wire/config, preserving truthful **generic** reference semantics. If trustworthy existing Ancillary-local source unavailable, register explicit `SourceUnavailable` fallback. Leave nonSeat activation blocked by real unavailable reference, clearly report it; do not bypass or fake Verified. Confirm there are no FlightFlow HttpClient/BaseUrl runtime references in src. Do not change any other repo.

### T3. Preserve Seat commercial authoring and pricing
8. Keep A07/A08/A09 Seat Definitions and POST/GET/PUT, full field/variant constraints. Keep 4 Seat Provision facade routes and validators plus all 10 common Rule groups. Keep existing generic Pricing and tax/fees/FX.
9. Test creation/activation Definition and Provision+filed Pricing+free Seat with zero policy, no FlightFlow settings. `MustCheckAvailability` must not cause false internal-inventory block. Preserve A07/A08 accepted max1, A09 extra seat semantics and document routing.
10. Preserve current HEAD null/empty POS = no POS restriction; nonempty applies filtering. **Do not restore v12.2 mandatory single POS.** Ensure authenticated tenant owner scope remains. Affected existing generic catalog guards must not be weakened to make tests pass.

### T4. Fix EXISTING P2 Shopping, not a new Phase3 engine
11. Retain `AncillaryShoppingEngine` and `ShoppingFilter`; implement narrow profile scoping in common service BEFORE expensive evaluation: `Seat` only for SeatOffers, eight others for AncillaryOffers. Do not duplicate engine, PriceAssessor, ProvisionEvaluation, currency source, retail mappers' shared code, or SQL readers.
12. In Shopping Seat flow do NOT query Seat InventoryPolicy or delegate source; build commercial outcome using published Definition/Provision/Pricing and supported rules, no availability guarantee. A typed `NotApplicable` capacity state is acceptable for canonical compatibility; public SeatOffers should not expose internal physical availability. `QuantityRule` min/max and passenger/fare/flight/exit-row commercial predicates must still apply.
13. Add working Seat-only HTTP Shopping routes to `Api`, `OtaPanel`, `Backoffice`: `POST /{Surface}/v1/SeatOffers`, `/Details`, `/Selections/Evaluate`. Use existing per-surface auth/scopes and current envelope/serializers. Existing `POST /{Surface}/v1/AncillaryOffers` plus `/Details` and `/Selections/Evaluate` become nonSeat only. Cross-family ref lookup must reject, including when someone copies a valid `OfferItemRef` from other route.
14. SeatOffers is a **per-category/traveller/flight/coverage priced offer**, not a FlightFlow SeatMap. Show price by variant/characteristic/cabin where actually derivable; never invent seat-number availability or claim all 12A/12B have the same attribute if source unknown. Existing `SeatCharacteristicFacts` must only be treated verified after trusted ingestion; `FlightOfferShoppingContextMapper` currently does not supply them. If concrete per-number price cannot be validated from AirOffer alone, return incomplete/needs-verification *for that selection*; do not fabricate numbers, categories, or zero price. Do not build BFF or FlightFlow connection under this task.
15. Preserve `EvaluateSelection` existing behavior and return exact reason codes, statuses, pricing refs, no false price acceptance. Preserve real API QueryDb mappings and OpenAPI snapshot for all three surfaces.

### T5. Tests / schema / release
16. Clean up current development migrations/snapshots if required; **NO historic data migration**, dual-read, expand/contract, backup requirement, or numeric old-Seat-policy compatibility. Only actual current SQL integrity/clean Up matters. Don't change nonSeat inventory resource schema beyond justified cleanup.
17. Execute ALL tests in `11`, including 24 variants x D/R/P/I/Q/N, 24 CT, 16 SH, SQL smoke and 8 stress journeys. Reuse existing tests instead of rewriting them to pass; update invalid 12.2 expectations only if new v12.3 rule justifies it. Check all .NET projects compile, all three routes HTTP-authenticated, QueryDb projection, price/currency calculations and no POS regression.
18. Strictly check `git diff --name-status`, all `rg FlightFlow` in runtime src, dead DI, unused SeatMap routes, no duplicate pricing engine, zero edits under Reservation/Ordering/FlightFlow repos.

## Acceptance criteria — any miss means NOT CLOSED
- AC1: Create/Publish A07/A08/A09 Definition+Provision+Pricing and query SeatOffers with NO Policy, NO FlightFlow URL, zero network calls to FlightFlow.
- AC2: General AncillaryOffers has NO Seat; SeatOffers has ONLY Seat; each of three surfaces has own authorization and family-scoped detail/evaluation.
- AC3: Seat price classification works from trusted facts; unknown per-number match => needs verification, not free/not guaranteed stock.
- AC4: No Seat InventoryPolicy or FlightFlow-specific Authority/port remains. Generic Count/Weight/Slot & Supplier policies still supported.
- AC5: All nonSeat A01–A06/A10–A24 domain+P2 Shopping behavior not broken, no regression in POS, tax/fees, error codes, 10 Provision rule groups, Command/Query SQL.
- AC6: No P3 code changes or claims of real booking.

## Output format — REQUIRED, no handwaving
Create `reports/V12.3-P1-P2-COMMERCIAL-SEAT-SHOPPING-CONFORMANCE.md` with (a) base/current/new SHA and exact changed file list; (b) numbered T1–T5 diffs with before/after code evidence; (c) actual routes per surface plus schema examples; (d) run commands, counts discovered/pass/fail/skip, DB engine and migration status; (e) status table for 24 A-groups, CT-01..24, SH-01..16; (f) grep/DI proof of zero direct FlightFlow references in runtime src; (g) actual remaining nonSeat reference blockers, with no fake PASS; (h) untouched P3 file list and final `READY_FOR_OWNER_AUDIT` or `BLOCKED` with causes. No unexecuted tests described as passed. Stop at owner review after coding and push per owner workflow.
