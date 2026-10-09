# AeroTech Ancillary v12.2 — Master Domain & Implementation Authority

**Status:** APPROVED ARCHITECTURAL DIRECTION; executable specification for rebuilding and fully implementing **Phase 1 + Phase 2** on the existing Ancillary solution. **Phase 3 is architecturally designed here but NOT authorized to implement.** Date: 2026-10-09.

## 1. Non-negotiable mission
Build the *smallest real airline-credible Ancillary Domain* that can **author, validate, publish, price/configure capacity and retrieve configuration for ALL 24 primary airline scenarios A01–A24** before Phase 3. Leave a clean foundation for shopping, passenger-specific offers, reserve, Order change and EMD after this stage. No decorative empty profiles, TODO-only entities, fake capacity, provider guarantees or claim that a tested fixture means real fulfilment.

Target: `https://github.com/aliifarhadi/AeroTech.Ancillary`, branch `feat/ancillary-v12.1-phase2-stock` (Agent shall branch for v12.2 from reviewed SHA `e23ba293b2578362005840967c3c07aa77dbd174` after checking actual HEAD, with no silent rebase). Baseline `k8s-stg@933b7b7a793b426dbcb6362bbf8519886edf9080` carries the merged v12.1 Phase1; feature branch contains Phase2 and pricing rewrites plus Lufthansa samples. **If HEAD advanced, Agent first produces changed-file impact diff then proceeds only with compatible delta.**

## 2. Authority order and scope
1. **This v12.2 pack**, ADR + field contracts + scenarios + invariants + conformance matrix (normative for changes).
2. Current airline business/industrial contract and valid external codes where v12.2 does not replace them.
3. Current repo code (reuse patterns, solution structure, framework, error handling); do not duplicate.
4. v12.1 packs/older donors ONLY for historical behavior/source tracing; v12.2 replaces conflicting provisions.

Scope **inside Ancillary repository only**: Supplier (preserve), ServiceDefinition, Provision and its rule groups, Pricing, InventoryPolicy, FlightCountInventory, FlightWeightInventory, AirportSlotInventory, CommandDb/QueryDb, Backoffice DTO/handlers, solution-owned enums/contracts, migrations and tests. Retain v12.1 domain conventions, existing IDs and lifecycle. Preserve the existing AncillaryReservation/Phase3 slice, except a strictly unavoidable compile-only change that must be justified; do NOT make any Hold/Confirm/Release semantics appear production-capable.

**No changes** to AirAvail, AirOffer, AirPrice, FlightFlow, Ordering, JetPay, AirInfo, other repositories or their contracts. No real Shopping/ServiceList, OrderChange, payment, allocation, reserve/confirm/release/expire, fulfilment, eTicket/EMD emission or new distributed infrastructure in Phase1+2 work. Model Phase3 *interfaces and examples only*, with separate future approval gate.

## 3. Three deliverables and strict meaning of DONE
- P1: Every A01–A24 carrier-admin authoring scenario has typed schema, valid lifecycle, applicable Provision and price policy; query/DTO round trip and rejection of wrong-family fields. Free/NotAvailable/paid/external-quote cases represented correctly. All 10 existing shared Provision rule groups preserved.
- P2: Explicit inventory authority/binding and its actual evidence; Count/Weight/Slot configuration + adjustments/locking/audit; purchase/usage limits not confused with physical stock; unavailable sources not silently mapped to Unlimited/Guaranteed; each A01–A24 has a truthful configuration snapshot and domain tests.
- P3: Complete **design only** of Offer/Selection/AddService/Reserve/Confirm/Release/Issue interfaces and invariants compatible with a minimal Lufthansa-like external API. **Do not code P3.**

Accept P1+2 only with SQL Server integration tests, migration dry-runs, real HTTP Backoffice smoke, all scenario tests and complete gap register. `PASS_DOMAIN` and `BLOCKED_PROVIDER_SOURCE` are distinct statuses. No unilateral Agent push/merge/deployment; stop at Owner audit and submit report. The previous Currency Reference defect is a real external source issue: do not adjust AirInfo or fake currency decimals in production.

## 4. Reading order
`01-DECISIONS` → `02-DOMAIN-GRAPH-AND-LIFECYCLES` → `03-PROFILES-AND-TYPED-FIELDS` → `04-24-SCENARIOS` → `05-PROVISION` → `06-PRICING` → `07-INVENTORY` → `08-BACKOFFICE` → `09-PHASE3-BLUEPRINT` → `10-PERSISTENCE-MIGRATIONS` → `11-CONFORMANCE` → `12-STRESS` → `13-CODE-AUDIT-DELTA` → `14-AGENT-PROMPT` → `15-TRACEABILITY` → `16-CONTINUATION` → `17-ENUMS-AND-RULE-ROWS`.

## 5. Definition of evidence
- **LH-S:** exact three `samples/lufthansa-*.json` files in branch (sample data, not internal Lufthansa database design); owner-provided screenshots and observed POST request.
- **PUBLIC:** official API/public policy docs specified in `15-TRACEABILITY` (prove commercial and wire behavior, not internal aggregate graph).
- **DESIGN:** AeroTech design decision to implement the supported behavior with minimal invariants; not claimed as an identical Lufthansa/Sabre internal implementation.
- **DEFERRED_P3 / BLOCKED_REFERENCE:** no fabricated PASS or guaranteed capacity.
