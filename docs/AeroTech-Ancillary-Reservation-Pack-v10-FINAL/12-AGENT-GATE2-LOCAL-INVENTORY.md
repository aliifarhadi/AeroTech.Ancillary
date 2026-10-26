# Gate 2 — Coding Agent — واقعی‌کردن Local Hold و عدم Oversell

**Precondition:** Gate1 accepted. Authority `01`, algorithm `04`, failure `06`, tests `07`. Scope: Ancillary service only.

## Implementation tasks
1. Extend `HoldAncillaryServicesService.NewHoldAsync` rather than create another reservation orchestrator. Keep idempotency replay, OrderId distributed lock, existing Validator and result shape.
2. Implement one small internal inventory allocation storage/transaction component **within existing Application/Persistence conventions**. Do not build generic Flow Engine or another Aggregate. It must resolve exact Active InventoryPolicy and source IDs, build demands from qty and inventory facts, take SQL locks, calculate active allocation, persist unit+allocations in ONE transaction.
3. Mandatory patterns: `Unlimited`, `Local FlightCount`, `FlightWeight`, `FlightCountPlusWeight`, `AirportSlot`. All multi-flight and multi-slot allocations atomic. Supplier/unsupported و هر RequiresProviderApproval/SubjectToConfirmation بدون proof واقعی reject early. For legacy local live holds with no allocation evidence, block affected source until safe reconciliation; do not assume zero demand.
4. Use **SQL Server transaction-owned per-source lock** (or one proven SQL-equivalent with same guarantee) and deterministic resource ordering; existing `ancillary-hold:{orderId}` lock alone insufficient. For tests use actual concurrent SQL transactions.
5. Build active-usage SQL predicate from Unit status and Reservation.ExpiresAt at current UTC (Held future + legacy Held null for safety + Confirmed). Never mutate TotalCapacity in Hold; never add independently cached `HeldCount` counters.
6. Ensure Admin Adjust operations cannot reduce capacity below committed+held uses under same resource locking; closed/suspended blocks only *new* holds, not historical release.
7. Handle weight exactly: fixedKg x quantity, acceptedWeightKgTotal no multiplication; slot TimeWithOffset+GuestCount, overlapping slices contiguous.
8. Local Hold all-or-nothing for one call; any shortfall rolls entire operation back. No Partial/Pending/Saga enum. Resource stock reference from Policy must be verified; never trust caller stock ID.
9. `Expire` effective inventory already releases logically; retain existing poller for persistence; no second expiry service.
10. Keep SeatMap commercial completely out of Local allocations.

## Mandatory acceptance tests
- SQL I01..I28 including concurrent last Count/Weight/Slot (I21..I23). Mock-only test does not count.
- Real SQL transaction rollback for combined Count+Weight I14/I15 and multi-flight I16.
- Cross-Product stock sharing I07.
- Admin Adjust concurrent with Hold I24.
- `TotalCapacity` unchanged I25.
- replay identical key no second allocations, changed InventoryFacts conflict.
- Existing Domain and Shopping suites green; SQL migration empty pending.

## Deliverable
`reports/V10-GATE2-LOCAL-INVENTORY.md` with **SQL test output** (commands, database isolation, test fixture), before/after capacity and active-usage rows, benchmark single-source vs shared resource, overflow/precision results, changed files, no fake claims.

**Stop and report** if `AcceptedWeightKgTotal` cannot be grounded for an actual product: gate that *specific* product/policy and continue others, never invent weight. Supplier remains gated. No deploy before closure.
