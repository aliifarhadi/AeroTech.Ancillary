# 14 — EXECUTABLE Coding Agent prompt — AeroTech Ancillary v12.2 P1/P2 rebuild (stop BEFORE P3)

## ROLE
You are **implementation-only Coding Agent**. Owner/Architect already CLOSED domain choices in v12.2 docs 00–13. Do not "helpfully" pick a new inheritance pattern, generic Rule Engine, new bounded context, API standard, or resource authority. Your code must conform to this pack exactly, preserving current .NET solution/framework conventions and existing source baseline. If one exact physical/source fact is unverified, fail closed, annotate blocker, and finish all independent P1/P2 paths without asking the Owner to decide general architecture.

## TARGET + SAFE BRANCH
Repo `aliifarhadi/AeroTech.Ancillary`, read `feat/ancillary-v12.1-phase2-stock`, verified source SHA `e23ba293b2578362005840967c3c07aa77dbd174` and exact latest HEAD (compare first). Work only on a new local branch `feat/ancillary-v12.2-phase1-phase2-rebuild` from verified v12.1 feature HEAD; **do not commit/push/merge without explicit Owner permission**. No code in external repos. No AirAvail, FlightFlow, Ordering, AirPrice, JetPay, AirInfo edits. Never alter `AncillaryReservationAggregate/**` or existing P3 tests except strictly compile-only and individually evidenced changes after owner approval.

## AUTHORITY
`00-START-HERE` then `01-CLOSED-ADR`, `02-DOMAIN`, `03-TYPED-SPEC`, `04-24-SCENARIOS`, `05-PROVISION`, `06-PRICING`, `07-INVENTORY`, `08-BACKOFFICE`, `09-PHASE3-DESIGN-ONLY`, `10-MIGRATION`, `11-CONFORMANCE`, `12-STRESS`, `13-SOURCE-DELTA`, `15-SOURCES`, `17-ENUM-VALUES`. v12.1 packs are historical/donors only if not contradicted. **Do not implement chapter 09.**

## 0. Mandatory read-only conformance audit, then implementation
1. `git rev-parse HEAD`, `git status --short`, `git branch`, tree; list each existing AR, Entity/VO, enum numeric IDs, DTO, API endpoint, Command/QueryDb EF config, last migration, and `reports/` tests. Capture exact `path:line` evidence. Identify reuse; do not rebuild everything by copy.
2. Create `reports/V12.2-REUSE-AND-CHANGE-REGISTER.md` listing for **each v12.2 new field** whether already present, needs existing VO extension, owned spec, new enum, migration and contract change. Any mismatching enum numeric values => STOP that specific change and state conflicts.
3. Source samples `samples/lufthansa-services-configuration.json`, `samples/lufthansa-services-by-order.json`, `samples/lufthansa-one-booking-v2-purchase-orders.json` are examples, never dynamic business rules or hardcoded sales prices.

## 1. Execute P1 commercial rebuild
1. Add closed `AncillaryProfile` 9 values and type-checked A01–A24 Variant mapping, with immutable published `(owner,stableRef,profile,variant)` identity; **one `AncillaryServiceDefinition` AR**.
2. Reuse existing Header, Supplier, Booking, Document and statuses. Add up to 9 owned/Entity profile specs with the **exact variant-dependent fields/types/mandatory rules** in chapter 03 and proper EF and QueryDb projections; no arbitrary object/JSON/EAV. Don't create dead empty Spec tables for shared rules.
3. Preserve all ten existing shared Provision Rule groups, including Baggage and Seat; add `DocumentRouting` metadata (`NoAncillaryDocument=1|Emd=2|TicketOrExchange=3`) without falsely modifying existing `AncillaryDocumentType` to include Tickets; implement single-POS active Provision invariant, typed profile rules when necessary, `PriceOrigin` and `CustomerSelectionContract` metadata (typed, no real shopper PII values). Extend `PurchaseStage` with OnBoard=5 to author A24; old numerics retained.
4. `QuantityRule` accepted min >=1; stepper 0 means no purchase. Add explicit Bound scope via existing `ServiceCoverageScope.Portion=2` and `PassengerUsageLimit.PerPortion=4`; no mislabeled numeric enums.
5. Retain Money+Rates+Components; add correct tax Included vs Added arithmetic (no double charge); ExternalQuote priced origin as trusted future quote capability, **no fake 0.00 Pricing**; preserve actual currency decimal evidence/unknown fail closed. Separate provision per POS.
6. Full existing lifecycle with Draft/Active/Revision/Suspend/Retire, proper tenant auth, immutable published versions and exact invariant guards.

## 2. Execute P2 capacity rebuild/finalization
1. Preserve `AncillaryInventoryPolicy` and three typed sources `FlightCountInventory`, `FlightWeightInventory`, `AirportSlotInventory`. One stable policy per owner+ServiceDefinitionRef, one current version read. Keep shipped current-version snapshot bug fix.
2. Enforce `Unlimited != Guaranteed`; `NotConfigured != Unlimited`; Supplier/FlightFlow delegation as explicit status; no seat-local inventory; no baggage local stock inferred by SKU; no fake facility/flight references.
3. Add Portion-scoped commercial limit where necessary; per-selection max vs per-traveller cumulative max vs physical pool must remain three separate concepts. One verified resource may be used by multiple SKUs. A real Count+Weight composition remains a fixed pair only.
4. Preserve atomic adjustments, correlation idempotency, filtered uniqueness, slot overlap serializable guards. Build and test on SQL Server (not only EF InMemory).
5. External physical source connections are NOT in scope: actual unconnected ports return blocked/not-configured with truthful evidence. Test fakes explicitly named as test-only; never hardcode actual source success.

## 3. REST/Backoffice and UI contract
1. Implement typed 9-template backend DTO contract, variant-specific field validation and read model; no controller-per-variant copy. Keep routes current unless chapter 08 documents unavoidable extension; version breaking contracts honestly.
2. 24 real authoring end-to-end samples, one each, with all mandatory typed values, Paid/Free/Quote policy, tenant POS, inventory policy, Get/Revise round trip. Every typed result must be persisted and retrievable via SQL/HTTP. No null-default fixture masquerading as capability.
3. If frontend repo not in Ancillary project, **do not implement unrelated UI**; write an exact JSON UI-form schema/read DTO spec for each of 9 Backoffice profiles, with field visibility and validation, so frontend team can build specialist pages without external edits. Server behavior authoritative.

## 4. Migrations and test evidence
1. Implement CommandDb/QueryDb additive expand/backfill/migrate/verify/contract per chapter 10, without dropping Owner data automatically. Legacy unknown classifier must fail to publish, not guessed from names. Preserve all business rules, historical price identities and adjustment audit records.
2. 24 scenarios `A01..A24 x D/R/P/I/Q/N` => 144 groups must be **individually reported**; 40 CT, 10 SQL-C, >=18 authenticated HTTP tests; 8 Journeys with P1/P2 assertions. Ensure real SQL Server concurrency and EF snapshots.
3. Run `dotnet build AeroTech.Ancillary.sln --no-incremental`, both test projects, EF `has-pending-model-changes` Command/Query, actual SQL migrations on fresh and seeded old DB, HTTP Backoffice smoke (if auth unavailable report `BLOCKED_AUTH` and do not mark PASS). Give command lines and actual counts with failure/truncated logs where applicable.
4. Observe active external DecimalPlaces defect; do not modify source outside repo nor hardcode workaround. Mark `BLOCKED_EXTERNAL_CURRENCY_DATA` truthfully. Any physical reference missing => `BLOCKED_EXTERNAL_RESOURCE_REFERENCE` for live use.

## 5. Invariants/fail-fast checks
- Incorrect profile+variant/spec; foreign family details; two spec rows; missing required spec; invalid request JSON/published Draft; invalid POS or duplicate active price; stale activation, source unknown.
- Wrong taxed total, wrong fee unit, wrong currency scale, double charged Baggage surcharge; product quota conflated with purchase max; Guarantee without source.
- Copy-to-version and query projections not preserving types; unrecognized enum values stored; extra seat counted as Ancillary physical item.
- No `Hold`/`Issue`/`OrderChange` exposed as done. Existing `AncillaryReservation` remains frozen and currently *not* a stock ledger.

## 6. Stop and report — don't push until authorized
Produce `reports/V12.2-PHASE1-PHASE2-IMPLEMENTATION-AND-CONFORMANCE.md` as chapter 11, complete matrix, exact actual test outputs, affected files, migration evidence, outstanding honest source blockers, git diff, future Phase3 prerequisites. Mark `PHASE1_PHASE2_DOMAIN_READY_FOR_OWNER_AUDIT` only after all achievable **P1/P2 domain** checks pass. Explicit label `PHASE3_DESIGN_ONLY_NOT_IMPLEMENTED`. No commit/push/merge, no external-service changes; wait for Owner audit. Do not ask Owner to resolve already-closed ADR choices.

## OUTPUT TO OWNER
1. one change report + exact code diff; 2. 24x6 checklist with test names; 3. schema/migration / data survival proof; 4. HTTP smoke proof; 5. honest known deployment blockers; 6. confirmation P3 untouched; 7. **short handoff prompt** for future Phase3 after Owner closure. Do not produce any unrelated projects.
