# V11 Gate 4 — questions and conflicts

Date: 2026-10-27. Branch `feat/ancillary-offers-v9`, base `296197a`. Gates 1–3 and Hold contract R1 are uncommitted.
Authority: `docs/Pack-v11` 14, with 02 §4–§5, 05 §4–§6 and 07 L01–L21.
Status: answered 2026-10-27. The Owner chose Q1 (a). Q1 and P1 are merged into the pack (01 §K, 02 §5, 14); this file is history.

## 1. How the sibling does it today

Gate 4 asks for no new mechanism. Everything below exists from v10 (`7a74055`) and Gates 1–2. Each item was read in the source on 2026-10-27.

| Pack 14 item | Where it already is |
|---|---|
| SeatMaps/Holds facade, same Root/Unit, no physical allocation | `HoldSeatSelectionsService.HoldAsync` → `ISeatSelectionVerifier` (`SeatSelectionVerifier`: pricing engine, AirInfo LOPA, ReferenceData) → `HoldAncillaryServicesService.HoldAsync`/`AppendSeatsAsync`; seat units are Sector, quantity 1, with `SeatMapSelectionId`, `SeatNumber` and `PricingId`, and no allocation |
| Append returns the same root; append replay without re-verifying | `FindHeldSeatsAsync` runs before the verifier; `AppendSeats` compares the header locally |
| Confirm all, partial Release, partial Cancel | `DELETE Service-Holds/{holdId}?unitIds=`, `POST …/Confirmations`, `POST …/Cancellations {reasonCode, unitIds?}`; Domain `Confirm`, `Release(unitIds)`, `Cancel(reason, unitIds)` are all-or-nothing |
| Expire | `AncillaryHoldExpiryPoller` → `ExpireAncillaryHoldsService`; logical expiry through `StatusOf`/`EffectiveStatus` before the poller runs |
| GET seats family | `AncillaryVariantHoldDto<SeatUnitHoldDto>` (R1) |
| Zero FlightFlow, zero Ordering | FlightFlow providers are used only by stock-source activation (`IFlightOccurrenceReference`) and the SeatMap display (`IOccupiedSeatProvider` in `GetSeatMapService`); `IOrderCommercialContextReader` is used only by Shopping (`OrderShoppingSource`) |
| Contracts of SeatMaps, Price and Resolve unchanged | `git diff --stat 296197a` over their request and DTO folders lists only the four seat-hold files whose return type became the Receipt (Gate 1, pack 02 §2) |

## 2. Field and rule audit (pack 02 §4–§5 against the code)

| Item | Pack | Code | Result |
|---|---|---|---|
| SeatMaps/Holds request fields and types | 02 §4.4 tables | `HoldSeatSelectionsRequest`, `SeatHoldSelectionInput`, `SeatHoldFlightInput`, `SeatHoldFareInput` | equal; binding not strict, as the pack decides (02 §1.5) |
| SeatMaps/Holds answer | Receipt, 201 | `Created(…, AncillaryHoldReceipt)` | equal |
| Confirm | no body, 200 Receipt, idempotent, Released untouched | yes (`M1_N11…replays_safely`, Domain `V10_L09_L10_L04`) | equal |
| Release `unitIds` | absent = all Held; positive, distinct; all-or-nothing | controller maps an empty array to absent; validator refuses an empty list, 0 and duplicates | equal in code; the HTTP behaviour of `?unitIds=` with an empty value is not proven yet (Q1) |
| Cancel | `reasonCode` 1..50, `unitIds?`; same reason replays; another reason 409 | yes; the other-reason conflict is **16413**/409 (`V10_L11_L12_L13_L14`) | pack 02 §5 lists the kept codes without 16413 (P1) |
| Held unit cancelled before Confirm | refused | `SEAT_A04` | equal |
| Expired before Confirm | 16407/409, even before the poller | `M1_N11_an_expired_hold_cannot_be_confirmed`, `SEAT_A05` | equal |

No contract defect was found in Gate 4's surfaces.

## 3. L01–L21 coverage

| ID | Covered by an existing test | Gap I will close (tests only) |
|---|---|---|
| L01 | `V11_L01`, `M1_N09` | — |
| L02 | `V10_I17…` (same key, other `guestCount` → 16403), Domain `V10_L19_L20` | — |
| L03 | none (the code compares an unordered set: `AncillaryReservation.EnsureSameContent`) | replay with products and units in reverse order → same ids |
| L04–L08, L10, L20 | Domain `V10_L01…`, `V10_L09…`, `V10_L11…`, `V10_L15…`; acceptance `V10_L02_L08…`, `SEAT_A03`, `SEAT_A04` | — |
| L09 | Released only (`V11_D01…`) | a Cancelled and an Expired unit keep their typed selection and reason in the GET |
| L11 | `M1_N11`, `SEAT_A05` | — |
| L12–L14 | `V10_S01…`, `V10_S02_S05…`, `V10_E05…` | — |
| L15, L18 | `V10_S10_S11…` (no FlightFlow type reachable from any reservation operation) | — |
| L16 | git diff above | — |
| L17 | none (the reachability test does not look at Ordering) | extend it: no `IOrderCommercialContextReader` and no `.Orders` type reachable |
| L19 | command level only (`V10_unit_ids_must_be…`) | HTTP: absent vs `?unitIds=` (Q1) |
| L21 | none | reflection test: one reservation aggregate, one service per operation, no family-specific reservation type |
| SQL mixed Bag+Seat+Meal | `V10_S02_S05…` uses Lounge definitions named `G3_BAG`/`G3_MEAL` | real A03 baggage + A11 meal + A07 seat append, partial release/confirm/cancel, typed GET by family, exact stock |

## 4. Planned without question

- The tests listed in §3, in a new `Holds/V11LifecycleAcceptanceTests.cs`, plus the Ordering assertions in `V10_S10_S11…`.
- No production code change, unless a new test fails.
- Report `reports/V11-GATE4-SEAT-LIFECYCLE.md`.

## 5. Questions

### Q1 — HTTP evidence (pack 14: "report with command and HTTP evidence")

The pack asks for HTTP evidence at Gate 4. Your Q6 answer creates the dev catalog for live flows (A15 Immediate, A20 local Supplier/Unlimited) at Gate 5.

- **(a) Recommended.** Run my own host from a side build on a separate port, never your session on 4046, against the dev DB. Prove over HTTP only what needs no catalog:
  - `DELETE …?unitIds=` with and without an empty value (L19),
  - the strict-JSON 400s of the Hold request (W10/W28),
  - 404 for an unknown hold on every lifecycle route,
  - the SeatMaps/Holds 400 for a malformed body.

  The business flows stay in the SQL acceptance tests now and run live at Gate 5, as Q6 planned.
- **(b)** Create the Q6 dev catalog now and run every Gate 4 flow over HTTP at Gate 4, including a seat hold. That needs AirInfo LOPA, ReferenceData and a real seat-map selection id on dev.
- **(c)** No HTTP at Gate 4; everything at Gate 5.

## 6. Pack correction proposed with your answer

**P1:** add 16413 (cancel replay with another reason) to the list of kept codes in 02 §5. It is the existing v10 code, and L08's expected 409 already matches it.
