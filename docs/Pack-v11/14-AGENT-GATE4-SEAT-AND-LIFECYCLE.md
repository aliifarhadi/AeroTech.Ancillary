# Coding Agent — Gate 4 — SeatMap commercial facade, full lifecycle / partial operations

**Precondition:** Gate3 approved. Authority `02` (six routes) and `05` state matrix. No architecture changes.

1. Preserve `POST /Service/v1/Ancillaries/SeatMaps/Holds` request/validator shape (`holdId?`, `seats[]`, `seatMapSelectionId` etc.) and existing verifier. Map commercial Seat to same Reservation/Unit with actual SeatNumber/PricingId from existing verifier; no physical Inventory allocation.
2. Seat-only create returns Receipt; append to mixed Hold with `holdId` returns **same** root/Unit list; replay of same append key returns same UnitIds without re-verifying. Header and OrderId comparisons are local equality checks of request vs *AncillaryReservation*, not lookup Ordering.
3. Preserve v10 Root lifecycle methods/status enums; Confirm all held, partial Release on `DELETE ?unitIds=..`, partial Cancel via same `Cancellations` request. Released/cancelled selected unit stays historically readable; no API per Unit.
4. Both Seat-only and mixed holds show the typed history in the GET, seats in the `seats` family (02 §3, Owner Q3); no live Definition read. The existing `Expire` poller is reused; stock usage expires logically without it; transitions are idempotent.
5. **Zero** outgoing FlightFlow seat-Holds/OccupiedSeats/Seat Release, **zero** Ordering call/reader for any Reservation path. Don't add `IOrderCommercialContextReader` to verify membership. Ordering/FlightFlow retain physical seat responsibility.

## Tests
L01–L21 and W25, all Seat regression tests, SQL mixed Bag+Seat+Meal, historical GET and 0 external calls. HTTP evidence at Gate 4 (Owner, Gate 4 Q1): a host of the agent's own on a separate port, against the dev DB, proves only what needs no catalog — `DELETE …?unitIds=` with and without an empty value, the Hold strict-JSON 400s, 404 for an unknown hold on every lifecycle route and the SeatMaps/Holds 400 for a malformed body; the business flows are proven by the SQL acceptance tests and run live at Gate 5 (Q6). Report `reports/V11-GATE4-SEAT-LIFECYCLE.md` with command and HTTP evidence. No commit/push.
