# Gate 3 — Coding Agent — Inventory & SeatMap Lifecycle Regression

## Preconditions
Owner approves Gate3. Work only on source-proven missing protections; preserve v10/v11 SQL ledger and `sp_getapplock` and transaction granularity. Do not create provider/FlightFlow/Ordering adapter, Saga, fake holds, or new stock counter.

## Validation
1. For `Unlimited` none allocated; `FlightCount`, `FixedKg`, Count+Weight and `AirportSlot` allocation built from local Policy and Typed Options (`timeWithOffset`, `guestCount`). `AcceptedWeightKg/PerKilogram` GATED. `Supplier` fail-closed.
2. Different Orders competing last Count, Weight and Slot use independent SQL scopes; source locks sorted; capacity invariant and no partial Root/Units on failure.
3. Retire Count/Weight/Slot active Source blocked under same lock; attempts to replace Source after Retire must not evade active allocation. Recheck guard and races, do not duplicate mechanism.
4. Confirm-all, Partial Release/Cancel, Expire with and without poller, replay, reason immutability and saved history.
5. Seat-only and append via existing `SeatMaps/Holds`, `SeatHoldSelectionInput` and `ISeatSelectionVerifier`; 0 physical Seat inventory and 0 FlightFlow outbound from Ancillary. Header matches existing Root when append. `SeatMaps/Resolve` freeze.
6. Regression P0: query `unitIds` present empty never releases all, repeated malformed or foreign IDs never writes.

## Evidence
Execute SQL tests I01–I10 and lifecycle tests L01–L13 on **isolated test database only**; report test totals, lock outcomes, SQL before/after and cleanup; do not use production or owner dev unless specifically authorized. Public benchmark is conceptual; do not claim it supplies AeroTech provider refs.

## Deliverable
`reports/V12-GATE3-INVENTORY-LIFECYCLE.md`: file diff, results actual, all GATED preserved, no cross-service outbound proof. STOP for Owner review. No commit/push/merge.
