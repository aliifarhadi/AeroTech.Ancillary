# Gate 3 — Coding Agent — SeatMap تجاری و چرخه عمر کامل Reservation

**Precondition:** Gate2 accepted, SQL allocation working. Authority `01`, exact APIs `02`, lifecycle `03`, Seat boundary `05`, failures `06`.

## Work (narrow scope)
1. Preserve the existing `POST /Service/v1/Ancillaries/SeatMaps/Holds` commercial facade; **add optional holdId** to request and map Seat Units into the same `AncillaryReservation` when provided. No other SeatMap routes added.
2. If `holdId` omitted, create Seat-only Reservation using existing path; if supplied, validate same OrderId, Reference, ExpiresAt, Root Held/unexpired and append to that Root. Parent `holdId` must NOT change.
3. Store `AppendIdempotencyKey` on inserted Seat Units; same append retry exact payload returns existing UnitIds; changed payload conflict. SeatMap verifier is commercial-only and must NOT call FlightFlow or read physical OccupiedSeat.
4. `SeatMapSelectionId`, `SeatNumber`, `PricingId`, `Quantity=1`, `Sector`, exact FlightId preserved. All Seat Units `Allocations=[]` even when same Reservation has Baggage stock allocations.
5. Partial operations stay on the existing group routes: `DELETE Service-Holds/{holdId}?unitIds=…` (only Held) and `POST Service-Holds/{holdId}/Cancellations {reasonCode, unitIds?}` (only Confirmed); no per-unit route. Reuse current `AncillaryHoldResult`, no DTO for each family.
6. Update existing Confirm, Release-all, Cancel-all so previously Released/Cancelled Units stay as they are. Selective release/cancel must alter **only** the selected units' allocation visibility, not others. No physical Seat change.
7. Read and expire preserve truthful lazy/persisted status. Cancel after Confirmation only; financial/EMD in Ordering.
8. Ensure all operations serialize by existing Order lock and appropriate DB transaction; no new queue/provider engine.
9. No call to FlightFlow `Seat-Holds`, `OccupiedSeats` or `Confirm/Cancel` in Ancillary; test with outbound spy.

## Tests
- S01..S15 plus L01..L23 and E05..E08. Mixed Hold with 2 Baggages + 1 Meal + 1 Seat. Release Meal (`?unitIds=`) → Confirm rest → Cancel Baggage only (`unitIds`) → Seat stays Confirmed; all stock counts correct.
- Seat-only create and append same HoldId; duplicate input, changed replay key, expired parent, foreign order, concurrent append.
- `SeatMap.Resolve`, pricing, `AncillaryOffers/Price/Resolve` unchanged by full regression; old SeatMap tests must pass.
- Verify **no physical Seat availability check** from Ancillary. If existing commercial SeatMap checks aircraft seat *existence* and fare, preserve; do not interpret as physical occupied check.

## Deliverable
`reports/V10-GATE3-SEAT-AND-LIFECYCLE.md` including endpoint samples, exact response Unit refs/status and SQL allocations before/after release/cancel; `git diff` bounded scope; all suites with execution proof.
