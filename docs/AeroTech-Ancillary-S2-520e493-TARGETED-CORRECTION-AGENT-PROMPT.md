# Coding Agent — Targeted S2 Closure Correction for `k8s-stg@520e493`

## Binding instructions

This is a **targeted correction**, not another architecture pack. Read `reports/S2-IMPLEMENTATION-CONFORMANCE-v1.1.md`, `CLAUDE.md`, Owner scope correction v1.1, and the attached `S2-520e493-OWNER-CODE-AUDIT.md`. Verify the current branch HEAD before making changes. Follow the owner authority and existing sibling patterns where actually relevant. **Only change `AeroTech.Ancillary`**. The user expressly forbids expanding into Identity, data protection databases, custom caller systems, new cross-service auth, unrelated ErrorCode catalogs, rewriting Stage 1/3, and changes to Ordering/FlightFlow/AirAvail/AirPrice. No new Aggregate, new Quote table or migration; no arbitrary rollback without reason. No commit/push/merge unless separately ordered. `SeatMapsDto` and routes frozen. Agent coding only; owner decides any novel business rule.

## Must-fix code gaps

**B1 — Complete Resolve Handoff:** Extend `ServiceResolvedServiceDto` / mapper with complete canonical selection needed for fulfillment (dimensions/weight/quantity/typed code/document refs/time/guest count/device count/etc., per variant), and existing commercial facts: `VariantCode`, `ServiceTypeCode/SubCode`, `BookingMethod`, `DocumentRouting`, `DocumentType`, `ConfirmationRequirement` and sourced supplier/provider identification if actually available. Avoid inventing a new provider contract. Preserve existing response fields. Do not return `Valid` for passenger-scoped items lacking a trusted `OrderTravellerId`. No forced traveller for genuine order-scope. Tests ensure request's accepted choice equals the payload Ordering receives for a baggage quantity, one typed pet/special equipment, airport service appointment, WiFi and assistance. Separate `includedSeat` stays free of fake service SKU.

**B2 — A08 Exit Row:** Extend the **new** Batch Seat input minimally with optional client acknowledgement, without modifying the existing SeatMap response or legacy routes. Evaluate with the actual Seat candidate + supplied terms through existing rules; re-evaluate through Resolve from signed token. Do not treat acceptance of terms as proof of child/adult eligibility or supplier approval. Tests: eligible adult accepted, missing terms incomplete, ineligible child blocked, changed occupied seat stale; existing paid/free seat tests stay green.

**B3 — Inline List:** Add bounded, source-derived `selectionContract`/required input constraints, verified `optionGroups` with `choices` and `sourceState`, explicit presentation-level `interaction.requiresDetails/isComplete` to the three existing List DTO surfaces **additively**. Preserve existing contracts/goldens; avoid N+1 loading per list item and do not put large supplier catalogs or long conditions in List. `Details` stays on demand; `optionRef` semantics need not be mandatory for unenumerable typed selections. Tests: A01 quantity, A02 ref, A24 plan and an SSR; 20-item list with 0 details calls, one complex click with 1 detail call; unknown option source never pretends empty catalog.

**B4 — Duplicate semantics:** Revise `SelectionBasketRules.Settle` to distinguish exact duplicate/mutually exclusive service from legitimate distinct typed selection instances sharing a candidate ref. This must derive from real provision/domain restrictions rather than newly invented policy. Exact duplicate and collision on same Pax/Seat/Flight still rejected. Include tests for two different appointment/option instances where allowed and two true duplicates. If the existing Domain has no evidence a type permits duplicates, keep fail-closed and report that exact restriction rather than guessing.

**B5 — Beneficiary mapping:** Validate Order source traveller→`OrderTravellerId` coverage and fail closed for traveler-bound services when mapping missing, without touching Identity or defining fake travelers. Negative test in Resolve.

**B6 — Option reference contract alignment:** Compare actual Batch requests to `docs/ancillary_s2_execution_pack/03-API-AND-INTEGRATION-CONTRACT-FREEZE.md` §5. Add optional `selectedOptionRefs` solely if needed to fulfill issued-choice binding, and compare against currently authored options; never require it for non-enumerated text/measurements; do not invent error codes.

**B7 — Signed-seat/source revalidation:** `ResolveSelectionsService.Resolve` must compare signed `SourceVersion` and applicable seat (`FlightId`, `FlightCapacityId`, `FlightRef`, `TravellerRef`, `SeatNumber`, Coverage) against the fresh authoritative computation, not merely currency+total+commercialVersion. SeatMap's historical 43-char ID remains intact. Mutating flight version/layout or physical binding while keeping seat number and price must make the old token stale. For ancillary items, verify version/candidate binding via the existing hashed offerItemRef rather than an extra persistence system.

**B8 — Partial Basket token semantics:** Owner override v1.1 requires no committable token for an incomplete Basket. Current Batch issues per-line tokens before computing `allReady`. Align token issuance and Basket status with that rule using a narrow test (one valid + one invalid line). Do not persist the Basket and do not assume Ordering accepts partial purchases automatically.

## Preserve / do not change

- Owner-approved no-category = AVAILABLE and explicit 0, provided flight physical source is verified. Missing flight occupancy ⇒ not free.
- Existing 43-char SeatMap hash and no decode; regenerate/revalidate in same service.
- Signed stateless order-bound `pricedSelectionRef`; current ICallerContext and Service routes; no key DB.
- Single Engine / two sources / three surface DTOs / separated EvaluateBatch route; existing single Evaluate route untouched.
- A19 and other genuinely source-unverifiable service variants fail closed. Do NOT create synthetic guardian, pet approval, external quotes, or Weight basis to force green.
- Shared fees: not accidentally counted twice, not incorrectly omitted while issuing a Valid quote. Do not build a new finance workflow in S2.
- Real Order adapter absent ⇒ return the existing 503; document external handoff and no operational HTTP closure. No additions or assumptions in Ordering repository.

## Evidence required in final report

1. Exact current SHA and changed paths + why each was necessary, no unrelated changes.
2. Contract JSON before/after for all three List surfaces, EvaluateBatch with Seat confirmation, and S2S Resolve including typed selection, quantity and document metadata.
3. Detailed scenario matrix A01..A24 with honest supported/pending outcomes; no universal green claim without authoritative sources.
4. Build/test commands actually run, discovered and executed test counts, failed/skipped counts. Verify SeatMap golden byte/semantic compatibility, single-route backward compatibility, no migrations and no surprise domain edits.
5. Runtime internal integration status separately from external dependency on Ordering and FlightFlow; do not claim `S2_CLOSED` based solely on fakes.
6. Stop and ask only if *actual source* proves an owner decision is necessary. Present one minimally scoped recommendation with supporting path and line, not a new architecture.

**Output:** one concise implementation/conformance report under `reports/`, existing tests and a patch confined to Ancillary; no commit or push without the owner's separate authorization.
