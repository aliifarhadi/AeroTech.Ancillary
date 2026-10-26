# Coding Agent — Gate 4 — SeatMap commercial facade, full lifecycle / partial operations

**Precondition:** Gate3 approved. Authority `02` (six routes) and `05` state matrix. No architecture changes.

1. Preserve `POST /Service/v1/Ancillaries/SeatMaps/Holds` request/validator shape (`holdId?`, `seats[]`, `seatMapSelectionId` etc.) and existing verifier. Map commercial Seat to same Reservation/Unit with actual SeatNumber/PricingId from existing verifier; no physical Inventory allocation.
2. Seat-only create returns Receipt; append to mixed Hold with `holdId` returns **same** root/Unit list; replay of same append key returns same UnitIds without re-verifying. Header and OrderId comparisons are local equality checks of request vs *AncillaryReservation*, not lookup Ordering.
3. Preserve v10 Root lifecycle methods/status enums; Confirm all held, partial Release on `DELETE ?unitIds=..`, partial Cancel via same `Cancellations` request. Released/cancelled selected unit stays historically readable; no API per Unit.
4. Both Seat-only and mixed should show `GET` typed history; no live Definition read required. `Expire` poller existing reused; stock usage expires logically without it; idempotent transitions.
5. **Zero** outgoing FlightFlow seat-Holds/OccupiedSeats/Seat Release, **zero** Ordering call/reader for any Reservation path. Don't add `IOrderCommercialContextReader` to verify membership. Ordering/FlightFlow retain physical seat responsibility.

## Tests
L01–L21, all Seat regression tests, SQL mixed Bag+Seat+Meal, historical GET and 0 external calls. Report `reports/V11-GATE4-SEAT-LIFECYCLE.md` with command and HTTP evidence. No commit/push.
