# Phase 2 Stock & Capacity — EXECUTION AUTHORITY — AeroTech Ancillary v12.1 / Phase 2

> **Project authority:** AeroTech Ancillary **v12.1 — Phase 2: Stock & Capacity**. Continuation of the approved Phase 1 v12.1; not a separately versioned specification. Phase 3 remains closed.


**Approved by Owner** on 2026-10-08. This supersedes the owner-review-only state of the earlier Phase 2 design. It does NOT close Phase 1 operational discrepancies or authorize Phase 3.

## Verified source
- GitHub `aliifarhadi/AeroTech.Ancillary`, merged branch `k8s-stg` SHA `933b7b7a793b426dbcb6362bbf8519886edf9080`; PR #1 merged Phase 1 v12.1, from branch `feat/ancillary-v12.1-phase1` commit `543d419402ebe9c97381790c4f2401e77d0ca34d`.
- Actual v12.1 agent report: `reports/V12.1-Phase1-Implementation-Report.md` in source repo, historical evidence of 169 domain plus 112 acceptance tests on agent's machine, NOT independently rerun in this pack.
- Repos `Aerotech.FlightFlow`, `AeroTech.Ordering.Final`, `Aerotech.AirPrice` inspected read-only; no cross-repo modifications permitted.

## Document authority order (strict)
1. Explicit Owner approval D1–D12 (`07`, `09`) and the **actual merged Phase 1 source**. Frozen Phase 1 canonical definitions, provision and pricing remain authoritative for those domains.
2. Phase 2 field catalog, invariants, contract and test obligations (`03`, `04`, `10`, `11`, `12`).
3. Phase 2 coding agent instruction (`13`) implementing those contracts and the Owner-approved scope.
4. Scenario context, benchmarks and prior proposals (`00`–`02`, `05`–`06`).
5. Prior v12.0 references and external benchmark documents are supporting evidence, not implementation authority.
**Conflict rule:** Any inconsistency among Owner decisions, canonical Phase 1 contracts and Phase 2 designs must be recorded for Owner decision before modifying an affected field; no autonomous change of authority.

Never substitute a generic StockPool, dynamic JSON conditions, event-driven phantom inventory, or an inferred supplier capability.

## Scope
MANDATORY: InventoryPolicy and authority dispatch, strongly typed FlightCount, FlightWeight, AirportSlot authoring, immutable adjustments, concurrency, audit, read-model, API, tests, migrations, provider boundary stubs, PassengerUsageLimit policy authoring.
CONDITIONAL: DailyService and RoomNight *only after* verified local capacity ownership/reference contract; if absent, deliver exact design and typed contract but no persistent speculative model/API. AssignedAsset is deferred for concrete physical assets. No Phase 3 reservation/allocation code.

## Required files for the coding agent
- Send **the full `AeroTech-Ancillary-v12.1-Phase2-Stock-Capacity.zip` package** plus `PHASE2-STOCK-CAPACITY/13-PHASE2-CODING-AGENT-PROMPT.md` as controlling instruction.
- Agent starts from exact merge SHA, creates new feature branch. It must not resume on the old Phase1 branch or overwrite already merged code.
- Finish with artifact 14 closure report, raw tests and owner review. Do not claim Phase 2 completion until Owner audits.

## Source links
- https://github.com/aliifarhadi/AeroTech.Ancillary/pull/1
- https://github.com/aliifarhadi/AeroTech.Ancillary/commit/933b7b7a793b426dbcb6362bbf8519886edf9080
- https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/Search_and_View_Services.htm
- https://developers.booking.com/connectivity/docs/b_xml-availability
- https://developers.booking.com/connectivity/docs/b_xml-roomrateavailability
- https://www.finnair.com/fi-en/pets-on-finnair-flights
- https://www.prioritypass.com/de-DE/lounges-prebook
