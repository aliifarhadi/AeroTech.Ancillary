# 08 - Phase 3 ADRs, traceable gaps and owner-review decisions

## A. Closed-for-P3.1 design decisions

| ID | Decision | Grounding |
|---|---|---|
| P3-ADR-01 | Engine uses ONE source-agnostic `AncillaryShoppingContext`; inbound Offer/Order/Direct mapping later | Owner explicit 2026-10-10 decision |
| P3-ADR-02 | Engine fetches active authored catalog/pricing/inventory configuration through read ports and returns one canonical typed result | Existing v12.2 roots, Owner decision |
| P3-ADR-03 | Context contains verified business facts required by ten Provision rule groups; unknown facts stay unknown | Existing v12.2 Rule Entities, 05/17 |
| P3-ADR-04 | Do NOT model 24 independent ARs or a dynamic EAV rule interpreter; reuse nine profiles and existing CustomerSelectionContract | Approved v12.2 ADR-01/02/11 |
| P3-ADR-05 | Candidate is NOT a purchase authority. Opaque buyer-facing ServiceOfferId/TTL storage is P3.2, after fully bound selection/quote | Approved v12.2 ADR-14 and owner separation |
| P3-ADR-06 | Availability in P3.1 never implies a Hold or guarantee; local capacity configuration and usage maximum are not measured remaining stock | Approved v12.2 07; code inspection |
| P3-ADR-07 | Engine does not map AirAvail DTOs, mutate Ordering, call FlightFlow Hold, issue EMD/ticket or modify current reservation endpoints | Owner separation and current source |
| P3-ADR-08 | P3.2/P3.3 implementation waits for independent preceding gate approval; source claims and blocked integration tracked honestly | Owner step-by-step request |

## B. Open contract decisions (do not resolve by guessing)

| ID | Needed by | Unresolved fact / owner or provider action | Agent handling |
|---|---|---|---|
| P3-OPEN-01 | P3.1 | Exact `Provision.Sequence` winner direction if not established by committed authoring logic | Audit and quote source; fail on collision until proven |
| P3-OPEN-02 | P3.1/P3.2 | Trusted full consumption ledger for PerOrder/PerFlight/PerPortion/PerServiceDate passenger limit | Return remaining Unknown and block final entitlement guarantee |
| P3-OPEN-03 | P3.1/P3.3 | Count/weight Inventory Resource registry authority | `BLOCKED_EXTERNAL_REFERENCE`; no fake records |
| P3-OPEN-04 | P3.1/P3.3 | Facility/airport IANA timezone reference | Block schedule/slot-specific verified sale |
| P3-OPEN-05 | P3.1/P3.3 | Counting-family reference | No fabricated cumulative entitlement or family membership |
| P3-OPEN-06 | P3.3 | FlightFlow delegated capability/seat-map uniqueness proof | Read-only/blocked pending verified contract |
| P3-OPEN-07 | P3.2 | AirAvail repository/live source response inaccessible in this review | Use consumer contract as explicitly lower evidence; require sanitized real fixture for closure |
| P3-OPEN-08 | P3.2 | Numeric FareFamilyId from an Offer whose wire FareFamily is `string`; typed FareType resolver; trusted local timezone/country IDs | Need authority resolver; never parse guess |
| P3-OPEN-09 | P3.2 | `LastTicketingDate` and actual Offer TTL are not the same documented datum | Keep separate; issuer of TTL must be identified |
| P3-OPEN-10 | P3.2 | What stage is created-but-not-ticketed Order? Provision only knows PreOrder/PostTicketed/OnBoard and `Both` predicate | Owner stage contract decision before supporting that pathway |
| P3-OPEN-11 | P3.2/P3.3 | Exact public `AddService` commercial contract in Ordering | No Ancillary-owned mutation; separate Ordering change approval |
| P3-OPEN-12 | P3.3 | Owner-confirmed single-holder routing for A07/A08; avoid double FlightFlow Hold; A09 EXST extra capacity + eTicket | Keep source boundary and block unsupported variants |
| P3-OPEN-13 | P3.3 | A10 Upgrade external quote, inventory, exchange and ticketing provider | QuoteRequired/Blocked without live verified contract |
| P3-OPEN-14 | P3.3 | Supplier-specific partial release/confirm/cancel/readback and retry semantics | Do not promote Pending/Unknown to Held/Confirmed |
| P3-OPEN-15 | P3.1/P3.2 | ReferenceData Currency DecimalPlaces integrity (previous sync defect) | No guessed default 2; block unsupported/incorrect authoritative data |

## C. Provenance classification

- `SOURCE_VERIFIED`: directly examined code at a pinned SHA and correct authoritative source.
- `CONSUMER_CONTRACT_VERIFIED`: Ordering's consumer DTO/Mapper proves its expectations, not AirAvail provider completeness.
- `BENCHMARK_SAMPLE`: Lufthansa JSON reflects a sample external interface, not its guaranteed backend.
- `DESIGN_DECISION`: required new model/adapter/reservation semantics, subject to owner gates and conformance.
- `BLOCKED_EXTERNAL_REFERENCE`: cannot fully fulfill without actual reference/provider contract; NOT a failed domain test and NOT success.

## D. No silent source reconciliation

The older Phase3 design-only text contains an `Ancillary -> FlightFlow` delegation step, while actual Ordering already uses FlightFlow for Air+Seat capacity. P3.3 must not unilaterally implement both. Record owner review of the single routing decision before reserving seats, especially A09. Current P3.1 does not touch it.

Likewise, the v12.2 Phase2 closure report was generated for an earlier audit HEAD and was committed in Ancillary commit `bee6ad6` together with FlightFlow occurrence reference. It explicitly reports `PHASE2_AUTHORING_CLOSED=yes`, `PHASE2_FULLY_CLOSED=no` and 4 unconnected sources. Refresh on any new commits and do not assume that a committed report itself means integration proof.
