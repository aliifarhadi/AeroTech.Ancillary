# Required agent handoff and audit report

> **Project authority:** AeroTech Ancillary **v12.1 — Phase 2: Stock & Capacity**. Continuation of the approved Phase 1 v12.1; not a separately versioned specification. Phase 3 remains closed.


The agent must create `reports/PHASE2-STOCK-CAPACITY-IMPLEMENTATION-REPORT.md` with ALL of:
1. Source SHA and branch, exact diff from `k8s-stg@933b7b7a793b426dbcb6362bbf8519886edf9080`, commit SHA only if owner explicitly authorized commit/push; working tree state.
2. All aggregate roots/entities/VOs/enums, field types, keys, DB index filters, quantity precision, lifecycle; list changes to inherited v12.1 code.
3. For each of 31 pre-existing product definitions, show ONLY recorded/proven policy mapping; unmapped remain NotConfigured, not guessed.
4. Phase1 open issues left unchanged: ServiceDateBasis 31 unset, 6 XBAG weight legacy mismatch, AirAvail SQL consumer, ReferenceData validity, legacy migrations, pricing line concurrency. Any newly blocked inventory authoring due to those must be in an exception register.
5. Decision evidence for local resource IDs and ownership; if actual master lookup missing, list affected activation blocked and state `Awaiting owner/source evidence`.
6. 62-ID case-by-case matrix, test method, run result, deferred classification, and exact failure reasons.
7. Raw `dotnet build`, domain test and acceptance test output including SQL Server; migration up/down roundtrip on clone, pending model changes check, reconciliation CommandDb vs QueryDb, no destructive migration.
8. Concurrency proof: same-version adjustment race, policy duplicate race, airport interval overlap race with distinct intervals and correct locking, idempotency retry and duplicate correlation behavior.
9. Read models and HTTP smoke with actual authorized Backoffice token, including 401/403, tenant leaks, 404, 409, 422; if no such environment exists label NOT TESTED, never fake.
10. No changes to `AncillaryReservationAggregate`, `FlightFlow`, `Ordering`, `AirPrice`, `JetPay`, `AirAvail`, `Framework`, ReferenceData. If any are changed in spite of guard stop and report.
11. Provider/source capability matrix including explicitly deferred Daily, RoomNight, AssignedAsset, and FlightFlow paid seat assignment not equated with flight ticket hold.
12. `PHASE2_READY_FOR_OWNER_AUDIT` or `PHASE2_BLOCKED` verdict; no autonomous advance to Phase3. `PRODUCTION_BOOKING_WITH_LIMITED_STOCK=NO_GO` until Phase3 stock allocations/cutover are tested.

The agent must not claim that a historical report proves current code. All tests and DB checks should be freshly rerun in the implementation branch. If a required test cannot run due to missing SQL Server, include exact blocked command and evidence and mark NO_GO for that criterion.
