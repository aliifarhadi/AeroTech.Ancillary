# Coding Agent — Gate 3 — connect minimal selected input to v10 InventoryLedger

**Precondition:** Gate2 approved. Authority `05` and inventory part of `04`. Preserve v10 Hold service and three Source aggregates.

1. Map `coverage.flightIds` numeric to internal CoveredFlightIds, exactly when policy uses real flight stocks; `selected.quantity` (or fixed 1) to actual demand, `selected.options.airportService.timeWithOffset/guestCount` to AirportSlot; no public `inventoryFacts`.
2. Use only existing `AncillaryHoldAllocator`, `AncillaryHoldChecks`, `InventoryLedger` + transaction-held `sp_getapplock` sorted keys. Stock Source is derived from policy and DB; no ResourceId input from Caller. Validate required identity, Policy.Active and source ownership, statuses and ClosedForSale.
3. Mandatory Local patterns from v10: FlightCount, Fixed FlightWeight, CountPlusWeight, AirportSlot; Unlimited zero allocations; Unsupported Supplier, AcceptedWeight, unready product fail explicit. MultiFlight/Count+Weight/Slots all-or-nothing. No changes to FlightFlow.
4. Keep exact canonical replay including selected typed fields; same request replay => same IDs, changed options/Qty => conflict. Never generate alternative Reservation on timeout before checking saved key.
5. Close **Retire active source P0** with `ActiveUsage>0` guard under same existing inventory source SQL lock for Count, Weight and Slot; no new retirement coordinator. Admin Adjust semantics v10 retained.
6. Keep SQL concurrency proof; no extra mutable `HeldCount`, no double allocation or double inventory release; historical ledger rows remain.

## Acceptance
I01–I23 with actual concurrent independent SQL connections; W01/R03/R04; all prior v10 stock tests still pass. For Local live without Reference source, mark `GATED`, not PASS. Report `reports/V11-GATE3-INVENTORY.md` with SQL before-after usage, failure rollback and migration results. No commit/push.
