# V12.1 Gate 3 - Inventory and Seat Lifecycle Regression

Date: 2026-10-27
Authority: `CLAUDE.md`, then `docs/Pack-v12.1/AeroTech-Ancillary-Reservation-Pack-v12.1-GATE1-MINIMAL/13-AGENT-GATE3-INVENTORY-SEAT-LIFECYCLE.md`
Result: **No production code change.** The source already has every protection Gate 3 checks. The one missing proof, the I05 same-key race for the generic Hold, is now a SQL test, and it passes. Nothing is committed. Gate 4 has not started.

## 1. Source state and diff

| Item | Value |
|---|---|
| Branch / HEAD | `feat/ancillary-offers-v9` / `e6a2012ee101546f5728db6a68844adcb6051204` (unchanged) |
| `src` diff | 1 file, `ServiceController.cs`, the Gate 1 guard only. No `src` file changed in Gate 3. |
| Gate 3 diff | `tests/.../Holds/V10InventoryAllocationAcceptanceTests.cs`: new test `V12_I05_…` and `using System.Collections.Concurrent;` |
| Earlier uncommitted work | Gate 1 and Gate 2 files, listed in their reports |
| Reservation source pin | unchanged: 133 files, `BB7E5038C5CDE87014A85FEFC94801D779241782DD242F31875E4AD4EA9890B7` |
| SeatMap / Shopping diff | `git diff HEAD` on `Query/Shopping/SeatMaps`, `RestApi/V1/SeatMaps` and `AeroTech.Ancillary.Shopping` is empty. `SeatMaps/Resolve` and `SeatMaps/Holds` are unchanged. |

## 2. The six Gate 3 checks

### 2.1 Allocation comes from the local policy and the typed options

| Policy | Source | Allocation |
|---|---|---|
| Unlimited | `AncillaryHoldAllocator.cs:82-114` | none; the policy has no count, weight or slot binding |
| FlightCount | `:86-91` | one row per covered flight, `count.RequiredUnits(quantity)` |
| FixedKg weight | `:93-98` | one row per covered flight, `weight.RequiredKg(quantity, null)` |
| Count + Weight | `:86-98` | both rows, allocated together or not at all |
| AirportSlot | `:101-111` | `timeWithOffset` and `guestCount` from `selected.options.airportService`; one row per contiguous slot, persons = `slot.RequiredPersons(quantity, guests)` |
| Seat | `:67` | none |
| AcceptedWeightKg | `AncillaryHoldChecks.cs:82-83` | GATED (16419) |
| Supplier stock | `AncillaryHoldChecks.cs:76-77` | refused (16419); no adapter, no fake hold |
| External supplier | `AncillaryHoldChecks.cs:38-39` | refused (16104 / 422) |

Tests: `V10_I01_I02_I03_I04_I05_I12…`, `V10_I06_I08_I09_I25_L04_L12…`, `V10_I11_I14_I15…`, `V10_I17_…_V12_D07_W03…`, `V10_S01_S08_S09_S15…`.

### 2.2 Races between orders

- **Lock order.** `InventoryLedger.cs:21-25` sorts the sources by kind and then by id. `:26` opens one transaction. `:47` takes `sp_getapplock` in Exclusive mode with the Transaction as owner, for every source.
- **Last unit.** Four orders race for the last count unit, the last 12.5 kg and the last slot person. Each race ends with exactly one hold, the others get 16417, and usage equals capacity (`V10_I21_I22_I23…`).
- **All or nothing.** When one source in a hold is short, nothing is written: no root, no unit, no allocation (`V10_I11_I14_I15…`, `V10_I16_I28…`, `V10_I26…`).

### 2.3 Retiring a source in use

- The three retire services take the ledger lock on the source first (`Retire*InventoryService.cs:41`), read its active usage under that lock (`:47`), and the domain refuses to retire while usage is above zero (`:49`, 16621).
- A filtered unique index (`[Status] <> Retired`) on `FlightCountInventoryConfiguration.cs:24-25`, `FlightWeightInventoryConfiguration.cs:25-26` and `AirportSlotInventoryConfiguration.cs:24-25` allows one live source per key. So a source in use cannot be replaced to escape its allocations.
- Tests: `V11_I15_I23…` (a retire races a hold, one wins alone), `V11_I15…` (a weight or slot source with a live hold retires only after the release), `V10_I24…`, `V10_I19…`.

### 2.4 Lifecycle

| Step | Tests |
|---|---|
| Confirm all, skipping released units | Domain `V10_L09_L10_L04…`; acceptance `V10_L02_L08_L10_L12_L14_E08…` |
| Partial release and partial cancel, all or nothing, refused before any change | Domain `V10_L01_L08_L06_L07…`, `V10_L15…`; acceptance `V10_L02_L08…`, `V12_L01_L02_L03_L04…` |
| Repeating a release is a no-op | `AncillaryReservation.Release` returns `false`; Domain `V10_L01_L08_L06_L07…` |
| Cancel keeps its reason; another reason answers 16413 | Domain `V10_L11_L12_L13_L14…`; acceptance `V10ReservationAcceptanceTests.cs:60` |
| A cancelled confirmed unit frees its stock and keeps its allocation row as history | `V10_I06_I08_I09_I25_L04_L12…` (usage 9.000 → 3.000) |
| Logical expiry before the poller | `V10_I07_I10…` (the stock is free before the poller runs; Confirm is refused) |
| Expiry persisted by the poller | `SEAT_A05…`; Domain `V10_L16_L17_L18…` |
| Replay | `V11_L02_L03_V12_W02_W03…`, `V11_L01_V12_D10…`, `V12_I05…` |
| A confirmed unit keeps its stock past the hold deadline | `V11_I17…` |

### 2.5 Seat

| Check | Evidence |
|---|---|
| Seat-only hold and append go through `SeatMaps/Holds`, `SeatHoldSelectionInput` and `ISeatSelectionVerifier` | `SEAT_A01…A06`, `V10_S01…`, `V10_S02_S05…`, `V10_S03_S04…`, `V10_S06_S07…`, `V10_E05…`, `V10_S02_concurrent_appends…` |
| No physical seat stock | `AncillaryHoldAllocator.cs:67` returns without an allocation; Domain `AncillaryReservationUnit.cs:71` forbids allocations on a seat unit |
| No call to FlightFlow or Ordering | the reachability test `V10_S10_S11_V11_L15_L17_L18…`: no type reachable from Hold, SeatMaps/Holds, Confirm, Release, Cancel, Expire or GET reaches FlightFlow or Ordering. No Gate 3 line references FlightFlow, Ordering or `HttpClient` (grep of the added lines: 0). |
| An append must match the existing header | `V10_S03_S04_L23_E07…` |

### 2.6 F03 and I05

- **F03:** the Gate 1 guard and test (`V12_L01_L02_L03_L04…`) are unchanged and passed in this run.
- **I05 (new):** `V12_I05_the_same_hold_sent_concurrently_creates_one_hold_and_every_answer_is_that_hold_or_a_busy_order`, details in §3. No idempotency code was changed.

## 3. The I05 test

The setup is one Local FlightCount product with a source of 10. In each round, four identical hold requests (same key, same content) run at the same time, each in its own `AncillaryScope` with its own DbContext, on the isolated test database.

| Round | Order lock | Expected and asserted |
|---|---|---|
| 1 | One lock shared by the four requests. This is how production behaves: `RedLockDistributedLock` tries once and returns 16416 when the lock is taken. | 1 to 4 requests succeed. Every other request gets 16416 / 409 and writes nothing. |
| 2 | No shared lock, so all four reach SQL. This forces the catch-and-replay path that Gate 0 named (`HoldAncillaryServicesService.cs:82-89`). | All four succeed. |

After each round the same request is sent once more. Both rounds then assert:

- every successful answer and the retry carry the same hold id and the same unit ids;
- SQL holds exactly 1 reservation with 1 unit for the order;
- there is exactly 1 allocation row of 1.000;
- source usage is 1.000 after round 1 and 2.000 after round 2.

The test does not show which path, the first replay check or the catch-and-replay, answered each losing request.

Result: 1 discovered, 1 passed (12 s alone). It passed again in the full run (§4).

## 4. Commands and counts

All runs use the isolated `DotAirAncillary_Tests_{guid}` database. No host, no hosted service, no token, and no dev or production database were used.

| Suite | Command | Result |
|---|---|---|
| I05 alone | `dotnet test <acceptance csproj> --no-build --filter "FullyQualifiedName~V12_I05"` | 1 discovered, 1 passed, 0 failed, 0 skipped |
| Domain build | `dotnet build tests/AeroTech.Ancillary.Domain.ConformanceTests/… -v q --nologo -nodeReuse:false` | 0 Error(s), 0 Warning(s) |
| Domain | `dotnet test <domain csproj> --no-build --logger "console;verbosity=normal"` | 293 discovered; Total tests 293, Passed 293; no Failed, Skipped or aborted line (2.7 s) |
| Acceptance build | `dotnet build tests/AeroTech.Ancillary.Application.AcceptanceTests/… -v q --nologo -nodeReuse:false` | 0 Error(s) |
| Holds + Boundary | `dotnet test <acceptance csproj> --no-build --filter "FullyQualifiedName~.Holds.\|FullyQualifiedName~.Boundary." --logger "console;verbosity=normal"` | 100 discovered (99 before Gate 3 + I05); Total tests 100, Passed 100; no Failed, Skipped or aborted line (1.4 min) |

Cleanup: the test databases belong to the test fixture. No row was written to the dev database in Gate 3.

## 5. GATED, unchanged

| Item | State |
|---|---|
| A09 extra seat, A13/A14 pets, A17/A18/A19, A10 quote | refused; no request member or a 16419 refusal (Gate 2 report §2, point 5) |
| Supplier stock, external supplier, AcceptedWeightKg | refused (§2.1) |
| Local stock on the dev database | GATED: the resource and facility references are not connected; only the SQL tests prove it |
| Ordering end-to-end | NOT VERIFIED |

## 6. Open points for the Owner

1. **I05 with a busy lock.** Pack 07 row I05 says "one Root, same response winner". With the production order lock, a duplicate that hits the lock while the first request is still running gets 16416 / 409, not the winner's receipt. A retry then gets the same hold, which the test proves. The existing seat-append race test (`V10_S02_concurrent_appends…`) already accepts 16416 for a repeated identical request. Recommendation: keep it this way. A 409 "order busy" is safe and retryable, and changing it would mean a waiting lock or a key lookup on busy, which is a change to working idempotency code. If the Owner wants every concurrent duplicate to receive the receipt, that needs an order.
2. **Gate 2 Q1** (replay does not compare family and `variantCode`) is still unanswered; see `reports/V12-GATE2-TYPED-REHYDRATION.md` §7.

## 7. Stop

Gate 3 ends here. Gate 4 was not started. Nothing was committed, pushed or merged.
