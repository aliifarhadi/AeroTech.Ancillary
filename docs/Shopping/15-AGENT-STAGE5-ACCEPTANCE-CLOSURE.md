# Agent Stage 5 — Closure QA, not feature building

**Pack Authority (must be locally present):** `00-START-HERE.md`, `01-OVERVIEW-AND-BUSINESS-CONTRACT-FA.md`, `02-EXACT-API-DTO-AND-FIELD-CONTRACT.md`, `03-A01-A24-VARIANT-DICTIONARY.md`, `04-SIGNED-IDS-TYPED-SNAPSHOT-AND-SECURITY.md`, `05-ORDERING-HANDOFF-AND-TESTS.md`, `06-BENCHMARK-SOURCE-DECISIONS.md`, `07-RELEASE-READINESS-AND-UNRESOLVED.md`. **If any file is absent or `python3 VERIFY-PACK.py` fails, STOP before modifying code. Do not consult earlier Pack versions.**


**Prerequisite:** Owner approved Stage4; no new feature or entity at closure.

1. Run baseline build, Domain/Shopping/API/EF acceptance suites; collect raw commands, outputs, newly failing and preexisting warnings separately.
2. Execute **all 78 cases** in `05` with evidence mapping (`PASS`, `FAIL`, `GATED`); a GATED case cannot be claimed completed. Verify HTTP fixtures for `Backoffice`, `OtaPanel`, `Api`, `Service Resolve` and negative HMAC.
3. Compare SeatMap paths to baseline SHA and byte hash, including controllers/DTO/reservation; any unapproved SeatMap diff is an immediate failure.
4. Diff migration: one new Typed SelectionSnapshot table (unless previously present), plus only necessary D2 A17 domain fields/migration; zero Direct/Qty token tables; no cleanup job/aggregate/second Shopping engine.
5. Full flow demo: Shopping A01 Qty2 per Traveller×Portion, select d_; Shopping A15 r_; GET/POST A15 s_; one mixed `ResolveGrouped({ids})`, family DTO mapped, price zero and positive; second same Resolve yields same answer. Authenticate issuer Surface and internal Ordering caller. Demonstrate invalid tamper/expiry/cross POS refusal.
6. SQL proof actual Free PricingRateId with price 0; `currencyId` Catalog correctness; no sensitive A17 stored in plaintext.
7. Document limitations: A09 gated pending FlightFlow; A17 encryption; A19 guardian model; Insurance Domain missing; post-order/Reservation/EMD/outside Scope not implemented.
8. Deliver `reports/V8-FINAL-CLOSURE.md`: SHA/diff; 78 outcomes; test command evidence; actual endpoints/examples; unresolved risk; explicit `GO/NO-GO` **recommendation only**. Do not merge/commit/push until Owner.

**Closure definition:** user can select and Resolve a real, correctly scoped Ancillary with simplest UX, without generic DTO, without fake auth, without SeatMap regression. Ownership verification by Ordering is the next separate integration phase and **cannot be claimed complete here**.
