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
