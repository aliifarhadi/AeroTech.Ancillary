# AeroTech Ancillary Phase 3 v1.0 - Authority, Work Packages and Execution Gates

Date: 2026-10-10. Status: **OWNER-REQUESTED PACK; IMPLEMENT P3.1 FIRST, GATE P3.2 AND P3.3**.

## 0. Mission and formal partition

Phase 3 is divided into three explicitly independent, serially accepted deliverables:

- **P3.1 Ancillary Shopping Engine:** A source-agnostic engine receives a canonical `AncillaryShoppingContext` and optional typed selection/filters; reads active catalog/eligibility/pricing/inventory configuration; returns `CanonicalAncillaryOfferResult`. It performs no input-source mapping, creates no Order, holds nothing, changes no catalog state, does not issue EMD/eTicket and does not infer guaranteed availability.
- **P3.2 Ancillary Shopping Engine Adapters:** Adapters translate a selected Flight Offer (AirAvail/AirOffer detail), an existing Order snapshot, or verified direct shopping input into the *same* context. Orchestrating Shopping and QuoteSelection, authentication, generated opaque offer refs, TTL, revalidation, and an internal HTTP entrypoint live at this boundary. No duplicated rule engine in adapters. No Ordering mutation unless separately approved in the Ordering repository.
- **P3.3 Ancillary Reservation Operations:** Real Hold, Confirm, Release, Expire, Cancel-confirmed, read/recovery with per-unit evidence, matched to Ordering's orchestration contract; finite local ledger and trusted supplier handling. Flight seat capacity stays FlightFlow-owned via Ordering. No payment/EMD/eTicket issuance in Ancillary.

**Execution:** Coding agent starts only P3.1 now. Completion of P3.1 requires an owner conformance review before ANY P3.2 implementation; completion of P3.2 requires another owner review before P3.3 implementation. P3.2/P3.3 in this pack are forward contracts and dependency/readiness specifications, not permission to implement them today. No stub public endpoints pretending to work.

## 1. Authority hierarchy

1. Canonical airline business/Ordering contractual boundaries and existing v12.2 approved Phase1/Phase2 decisions, definitions and enum identity.
2. This owner-requested Phase3 pack governs *new Phase3 scope only*. Where v12.2 `09-PHASE3-FUTURE-DESIGN-ONLY.md` conflicts with new owner direction, this pack's explicit phase separation prevails. It **does not** reopen v12.2 Phase1/Phase2 authoring or rewrite existing aggregates without direct P3 necessity.
3. Actual repository HEAD/committed code and verified external wire contracts.
4. Lufthansa JSON samples, public PSS/NDC references and donor code are behavior examples, not evidence of internal schemas or source-of-truth guarantees.

Do not generalize from Lufthansa JSON to identical AeroTech domain types. No additional independent roots for 24 variants, no generic EAV/JSON rule engine, no speculative services/reference records, no sample-derived prices, no tenant redesign, no migration/rollback work for disposable test data.

## 2. Verified repository references (must be refreshed by coding agent)

| Scope | Repository / ref verified 2026-10-10 | Permission |
|---|---|---|
| Ancillary | `aliifarhadi/AeroTech.Ancillary` `feat/ancillary-v12.2-phase1-phase2-rebuild@bee6ad67237be4f576e425686ce501f23c277952` | The ONLY writable repository, only files relevant to authorized P3.1 |
| Ancillary base | `k8s-stg@933b7b7a793b426dbcb6362bbf8519886edf9080` | comparison only; P3.1 must start from approved feature HEAD, not silently reset to older base |
| Ordering | `aliifarhadi/AeroTech.Ordering.Final` `k8s-stg@cd50a2a5fd18a372f32f2d4efac08dee0510f6ba` | READ-ONLY |
| FlightFlow | `aliifarhadi/Aerotech.FlightFlow` `k8s-stg@d2180b2e4c07789abde75e712e870ece8d2c776b` | READ-ONLY |
| AirAvail | `aliifarhadi/AeroTech.AirAvail` | **NOT ACCESSIBLE** through connected GitHub (404 at the time of this pack); do NOT claim its source reviewed or assume a live JSON response |

If any HEAD has advanced, report exact SHA and impacted paths before editing. Agent must not push/merge/deploy without owner instruction.

## 3. Primary normative source index

Source URLs are pinned to reviewed Ancillary feature branch; the engineer must check current contents:

- `docs/AeroTech-Ancillary-v12.2/00-START-HERE-AND-AUTHORITY.md` through `17-ENUM-VALUES-EXISTING-RULE-CATALOG-AND-DOCUMENT-ROUTING.md`, particularly **01,02,03,04,05,06,07,09,11,12,17**.
- `src/AeroTech.Ancillary.Domain/AncillaryServiceDefinitionAggregate/AncillaryServiceDefinition.cs`, `.Profile.cs`, `CustomerSelectionContract.cs`, `AncillaryVariant.cs`, typed `Specifications/*`.
- `src/AeroTech.Ancillary.Domain/AncillaryProvisionAggregate/AncillaryProvision.cs`, `.Rules.cs`, `Entities/*`, and `AncillaryPricingAggregate/*`.
- `src/AeroTech.Ancillary.Domain/AncillaryInventoryPolicyAggregate/*`, typed inventory roots, their repositories and configuration snapshot reader.
- `src/AeroTech.Ancillary.Domain/AncillaryReservationAggregate/*`, `src/AeroTech.Ancillary.Application/AncillaryReservationAggregate/Commands/HoldAncillaryServices/HoldAncillaryServicesService.cs` (P3.3 only; current Held is NOT real capacity).
- `samples/lufthansa-services-configuration.json`, `lufthansa-services-by-order.json`, `lufthansa-one-booking-v2-purchase-orders.json` (read-only fixtures).
- Ordering: `src/AeroTech.Ordering.Providers/Offer/Services/OfferProvider.cs`, `Wire/FlightOfferDetailResponse.cs`, `OfferResponseMapper.cs`; `src/AeroTech.Ordering.Domain/Providers/Offer/OfferDetail.cs`, `OfferReader.cs`; `src/AeroTech.Ordering.Application/OrderAggregate/Commands/CreateOrderFromOffer/CreateOrderFromOfferService.cs`.
- Ordering: `src/AeroTech.Ordering.Providers/FlightFlow/Services/FlightFlowReservationProvider.cs`; FlightFlow source `Flight.cs`, `FlightCapacity.cs`, Hold/Confirm/Release/Cancel handlers.
- `reports/V12.2-P2-BACKOFFICE-CLOSURE-RECHECK.md`: authoring CLOSED; fully connected inventory NOT closed.

**Evidence ledger:** The 3 Lufthansa samples contain 22 display categories/48 configured service codes, 114 service entries in by-order example, 6 purchased service entries. By-order statuses include guaranteed/pending/unknown; they are sample/provider statuses, never translated to a domain stock guarantee without an authority contract. Ordering's HTTP OfferProvider posts `v1/FlightOffers/Details` with `{OfferId}` and maps response to `OfferDetail`; it is NOT evidence that Ancillary may depend on Ordering's internal classes.

## 4. Non-negotiable open dependencies

Four Phase2 ports in last verified audit lack an authoritative connected source: `IInventoryResourceReference`, `IAirportFacilityReference`, `ICountingFamilyReference`, `IFlightFlowDelegationReference`. A read-only FlightFlow occurrence reference exists, but it does NOT prove inventory delegation. Do not repair these by accepting arbitrary IDs, creating fake records or claiming capacity guaranteed. In P3.1, return `Unknown`/`SourceUnavailable`/`CannotFullyEvaluate` precisely as appropriate. Track a distinct `BLOCKED_EXTERNAL_REFERENCE` verdict in conformance.

Real per-traveller cumulative usage across orders is NOT available merely because `PassengerUsageLimit` stores a maximum; P3.1 must evaluate only when a credible usage ledger/evidence port exists. Price decimals MUST come from verified currency reference: do not hardcode 2 or silently convert. Flight Offer `FareFamily` is a string while Ancillary Provision `FareFamilyId` is long; this is a **P3.2 contract gap**, not permission to parse/fabricate a numeric ID.

## 5. Gates and final reports

- **GATE 3.1:** Context field/rule traceability is complete; deterministic active product/provision/rate selection; typed schema for A01-A24; truthful pricing and availability; no AirAvail/Order dependency; tests green with exact commands and counts; all unavailable evidence distinctly reported. Approval required for P3.2.
- **GATE 3.2:** Each mapper has real source fixtures and explicit omissions; same canonical context leads to equivalent Engine result; offer token bound to actor/POS/traveller/flight/selection/pricing/TTL; no silent price change; auth and negative HTTP tests. Approval required for P3.3.
- **GATE 3.3:** real atomic capacity/provider outcomes, per-unit partial retry, no double FlightFlow seat hold, cancellation/refund boundary; SQL concurrency tests; no old shallow Held exposed as authoritative; dependency blockers resolved or explicitly left BLOCKED. Owner review before public use.

At every gate agent reports: exact commits, changed files, source-grounded decisions, deviations from this pack, tests run (commands/results), tests NOT run, failed/blocked cases, required owner decisions. **A proposed solution or compile-only success is not 'CLOSED'.**
