# AeroTech Ancillary — Canonical Master Pack v12.1

**Approved phase:** Phase 1 (merged) + Phase 2 Stock & Capacity (Owner-approved for implementation).  
**Closed:** Phase 3 Reservation / physical allocation.  
**GitHub baseline:** `aliifarhadi/AeroTech.Ancillary`, `k8s-stg@933b7b7a793b426dbcb6362bbf8519886edf9080` (PR #1: v12.1 Phase 1 merged on 2026-10-08).  
**Phase 2 working branch for coding agent:** `feat/ancillary-v12.1-phase2-stock` (proposed; not created by this document).

## Why one version number

The Ancillary program is still **v12.1**. Phase 2 is the Stock/Capacity design and implementation stage **under the same approved v12.1 master authority**. Previous independent `1.x` labels for Phase 2 were a packaging mistake, not a different domain revision. **Do not increment or fork the design version** to start Phase 2.

## Package structure

- `PHASE1-APPROVED-REFERENCE/`: exact unchanged files from the Owner-approved Phase 1 v12.1 source pack; read for definitions and signed decisions. The embedded historical pre-approval language in `00` is superseded by its own approval addendum `09-OWNER-APPROVAL-ADDENDUM.md` and the merged GitHub source.
- `PHASE2-STOCK-CAPACITY/`: approved Phase 2 design, field-level catalog, scenarios and Agent instructions, labeled as v12.1 continuation.
- `PHASE2-STOCK-CAPACITY/08-READ-ME-START-HERE.md`: Phase 2 reading order and authority.
- `PHASE2-STOCK-CAPACITY/13-PHASE2-CODING-AGENT-PROMPT.md`: **implementation prompt**. Send this along with the entire ZIP to the Coding Agent.
- `PHASE2-STOCK-CAPACITY/15-NEXT-CHAT-CONTINUATION-PROMPT.md`: new-chat handoff.

## Ownership and implementation constraints

Owner approved Phase 2 decisions D1–D12 as recorded in `07` and `09`. Implement InventoryPolicy, typed FlightCount, FlightWeight and AirportSlot inventory configuration, PassengerUsageLimit policy, compatible Count+Weight bindings, persistence/backoffice/concurrency/audit/test evidence. DailyService and RoomNight require verified local ownership before active implementation; AssignedAsset remains deferred. No generic `AncillaryStockPool`. No authoritative local stock counters for FlightFlow or supplier-managed resources. Missing policy ≠ Unlimited.

Phase 2 only configures/audits resources: it does **not** consume stock or protect against real booking oversell. No edits to Phase 3 Hold/Confirm/Release or other repositories. After coding, require real SQL Server concurrency evidence and Owner audit before closure.

## Known pre-existing Phase 1 issues — do not silently heal

The v12.1 coding agent's report claimed 169 domain and 112 acceptance tests passing, not independently rerun by this packaging pass. The same report noted 31 legacy ServiceDefinitions without ServiceDateBasis, six historical XBAG_WEIGHT PerItem/Kilogram combinations, and external consumer / reference existence concerns. These must remain explicitly registered and are not a reason to invent stock ownership.

## Status

**PHASE2_APPROVED_FOR_CODING_AGENT — NO_PHASE3_APPROVAL**. The document package is ready to give to the coding agent; it does not claim Phase 2 source code has been implemented or its tests passed.
