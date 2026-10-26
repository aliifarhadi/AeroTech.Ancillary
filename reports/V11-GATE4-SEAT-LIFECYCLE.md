# V11 Gate 4 — SeatMap facade and lifecycle

Date: 2026-10-27. Branch `feat/ancillary-offers-v9`, base `296197a`; Gates 1–4 and Hold contract R1 are uncommitted.
Authority: `docs/Pack-v11` 14, with 02 §4–§5, 05 §4–§6 and 07 L01–L21. The Owner's Gate 4 answer is in 01 §K: Q1 (a) for the HTTP evidence, and P1, which lists 16413 in 02 §5.
Verdict: INTERNAL_READY for the Owner's review. Live business flows over HTTP stay at Gate 5 (Q6). Nothing is committed or pushed.

## 1. What changed

- **Production code:** none. Everything pack 14 asks for already exists from v10 and Gates 1–2. See `reports/V11-GATE4-QUESTIONS-AND-CONFLICTS.md` §1 for the source map.
- **New test class** `Holds/V11LifecycleAcceptanceTests.cs`:
  - `V11_L02_L03…` — a replay with products and units in reverse order returns the same hold and unit ids; the same key with another `bagRef` gets 16403/409.
  - `V11_L09…` — a Cancelled unit keeps its typed `assistanceSsrCode` and reason in the GET. An Expired unit keeps its typed baggage choice, and the GET is byte-identical before and after the expiry poller runs.
  - `V11_L04_L05_L07_L12_L13…` — real A03 baggage and A11 meal, then an A07 seat appended to the same hold (the append replays with the same ids and one verification), then:
    1. release the meal,
    2. confirm,
    3. cancel the baggage.

    The GET shows `baggages` (Cancelled, reason, `bagRef`), `meals` (Released, `mealCode`) and `seats` (A07, Confirmed, `seatNumber`, `seatMapSelectionId`). There are no allocation rows.
  - `V11_L21…` — the reservation namespace has exactly one aggregate root (`AncillaryReservation`). The Application has exactly six reservation services, one per operation. No service exists per family.
- **Extended:** `V10_S10_S11_V11_L15_L17_L18…` now also starts from `HoldAncillaryServicesService` and the GET. It asserts that no reachable type is `IOrderCommercialContextReader` or lives in an `.Orders` namespace, in addition to the FlightFlow checks.

## 2. L01–L21 (pack 07)

| ID | Test | Evidence |
|---|---|---|
| L01 | `V11_L01…`, `M1_N09…` | same ids on replay, also after the provision is retired |
| L02 | `V11_L02_L03…`, `V10_I17…`, Domain `V10_L19_L20…` | changed typed option / guest count → 16403 |
| L03 | `V11_L02_L03…` | reversed order → same hold and ids |
| L04 | Domain `V10_L09_L10_L04…`, `V11_L04…`, `V10_S02_S05…` | Confirm skips the Released unit |
| L05 | Domain `V10_L01_L08_L06_L07…`, `V11_L04…` | the others stay unchanged |
| L06 | Domain `V10_L09_L10_L04…` | releasing a Confirmed unit is refused (409), nothing changes |
| L07 | Domain `V10_L11_L12_L13_L14…`, `V11_L04…` | the reason is stored; the others stay unchanged |
| L08 | Domain `V10_L11_L12_L13_L14…` | another reason → 16413/409 |
| L09 | `V11_L09…` | Cancelled and Expired units keep their typed selection |
| L10 | Domain `V10_L11…`, `SEAT_A03…` | a replayed release or cancel changes nothing |
| L11 | `M1_N11_an_expired_hold_cannot_be_confirmed`, `SEAT_A05…` | 16407 even before the poller |
| L12 | `V10_S01_S08_S09_S15…`, `V11_L04…` | one hold, seat unit, zero allocations |
| L13 | `V10_S02_S05…`, `V11_L04…` | same `holdId`; existing units untouched |
| L14 | `V10_E05…`, `V11_L04…` | append replay without a second verification |
| L15, L18 | `V10_S10_S11_V11_L15_L17_L18…` | no FlightFlow type reachable from Hold, SeatMaps/Holds, Confirm, Release, Cancel, Expire or GET |
| L16 | `git diff --stat 296197a` (below) | Price, Resolve and SeatMaps contracts unchanged |
| L17 | `V10_S10_S11_V11_L15_L17_L18…` | no Ordering type reachable |
| L19 | HTTP (§3) and `V10_unit_ids_must_be…` | absent → all; `?unitIds=` empty value → 400; an empty list is refused at the command level |
| L20 | Domain `V10_L15…` | a foreign or wrong-state unit id → nothing changes |
| L21 | `V11_L21…` | one aggregate, one service per operation |
| W25 | `V11_W25_W26…`, `V11_W29…` | `seats` family shape |

`git diff --stat 296197a` over the SeatMaps, Price, Resolve and seat-hold contract folders lists only the four seat-hold files whose return type became the Receipt (Gate 1, pack 02 §2).

## 3. HTTP evidence (Owner Q1 (a))

- My own host was run from a side build (scratch `v11/g4host`), with `ASPNETCORE_ENVIRONMENT=Development`, on `http://localhost:5291` against the dev DB.
  - Port 4046 was not in use before or after.
  - After the run the host was stopped and port 5291 was confirmed free.
- Script: scratch `v11/g4_http.py`; raw answers: scratch `v11/g4_http.json`.
- Base: `/Service/v1/Ancillaries`.

| Request | Status | Body (shortened) |
|---|---|---|
| `GET Service-Holds/999999999` | 404 | `16401` The ancillary hold was not found |
| `POST Service-Holds/999999999/Confirmations` | 404 | `16401` |
| `DELETE Service-Holds/999999999` (no `unitIds`) | 404 | `16401` — absent means all, so the hold is looked up |
| `DELETE Service-Holds/999999999?unitIds=` | 400 | binding: `unitIds`: "The value '' is invalid." — an empty value never expands to all |
| `DELETE …?unitIds=0` | 400 | Validation failed: `UnitIds[0]` must be greater than 0 |
| `DELETE …?unitIds=5&unitIds=5` | 400 | Validation failed: `UnitIds` |
| `POST …/Cancellations {"reasonCode":"CUSTOMER_REQUEST"}` | 404 | `16401` |
| `POST …/Cancellations` with `"amount":1` | 400 | `$.amount` could not be mapped |
| `POST Service-Holds` with the pack 02 §1 sample | 409 | `16419` provision 701 is not active — the body binds and reaches the verifier; the sample provisions do not exist on dev |
| `POST Service-Holds`, `baggages` with the `assistedTravel` key | 400 | `$.baggages[0]…options.assistedTravel` could not be mapped |
| `POST Service-Holds`, `meals` with `options` | 400 | `$.meals[0]…selected.options` could not be mapped |
| `POST Service-Holds` with `pets` | 400 | `$.pets` could not be mapped |
| `POST Service-Holds` with `infantRef` | 400 | `…assistedTravel.infantRef` could not be mapped |
| `POST Service-Holds` with `unit:{}` | 400 | Validation failed: `…Unit.TravellerId` must be greater than 0 |
| `POST SeatMaps/Holds {}` | 400 | `Seats`, `Reference`, `IdempotencyKey` are required |
| `POST SeatMaps/Holds` with malformed JSON | 400 | JSON error |

- The host log (scratch `v11/g4host.log`, 338 lines) has no FlightFlow, Ordering or OccupiedSeat line.
- Its only error is a `TaskCanceledException` on one GET from my readiness probe. The probe's curl had a 2 s timeout and fired while the first database connection was opening.
- Observation, platform behaviour, not changed: binding errors answer in the ASP.NET `ProblemDetails` shape (`[ApiController]`), while validation and business errors answer in the platform `{"errors":[…]}` shape.

## 4. Run (R03)

2026-10-27, logs in scratch `v11/full4`:

| Step | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 errors, 61 warnings |
| Domain conformance | 293 discovered, 293 passed |
| Shopping | 126 discovered, 125 passed, 1 skipped |
| Application acceptance | 292 discovered, 292 passed |
| Aborted or crashed runs | none |

Freeze pins are unchanged: 133 files, `3E998D04…E90`. Gate 4 touched no reservation source file.

## 5. Left for Gate 5

Live business flows over HTTP on dev with the Q6 catalog (A15 Immediate, A20 local Supplier/Unlimited) and a real seat-map selection id.
