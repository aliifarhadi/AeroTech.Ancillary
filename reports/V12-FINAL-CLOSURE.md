# V12.1 Final Closure - Reservation (Hold) Endpoints

Date: 2026-10-27
Authority: `CLAUDE.md`, then `docs/Pack-v12.1/AeroTech-Ancillary-Reservation-Pack-v12.1-GATE1-MINIMAL/14-AGENT-GATE4-CONFORMANCE-CLOSURE.md`

**Verdict: PASS WITH GATED.**

- Every scenario that can be run inside Ancillary passed in this round, on the real SQL Server engine and partly over real HTTP.
- Local stock on the dev database is **GATED**.
- The Ordering end-to-end flow is **NOT VERIFIED**.
- No reproducible safety defect remains. F03 was fixed in Gate 1. F02 was never reproduced.
- Two design questions are open for the Owner (§11). Neither is a safety defect.
- Nothing is committed.

## 1. Source state

| Item | Value |
|---|---|
| Branch | `feat/ancillary-offers-v9` |
| HEAD | `e6a2012ee101546f5728db6a68844adcb6051204` ("Hold Endpoint Renamed + v12 Stage 0") |
| Production change in v12.1 | one file: `src/AeroTech.Ancillary.RestApi/V1/AncillaryReservationAggregate/Controllers/ServiceController.cs` (+10 / -1, the F03 guard) |
| Test changes | `Holds/V10InventoryAllocationAcceptanceTests.cs`, `Holds/V11LifecycleAcceptanceTests.cs`, `Holds/V11TypedSelectionAcceptanceTests.cs`, `Holds/V11WireAcceptanceTests.cs`, three pin files (`P2Boundary…`, `V12Boundary…`, `V121FinalCrossPhase…`), new `Fixtures/ReservationHttpHost.cs` |
| Reports | `V12-GATE1-MINIMAL-SAFETY.md`, `V12-GATE2-TYPED-REHYDRATION.md`, `V12-GATE3-INVENTORY-LIFECYCLE.md`, this file |
| Untracked, the Owner's | `docs/Pack-v12.1/`, `docs/AeroTech-Ancillary-Reservation-Pack-v12.1-GATE1-MINIMAL.zip` |
| Reservation source pin | 133 files, `BB7E5038C5CDE87014A85FEFC94801D779241782DD242F31875E4AD4EA9890B7` |
| Migrations | none added; no schema change |

Gate 4 itself added one test, `V12_W01_W02_W03_W04_W05_W06…` in `V11LifecycleAcceptanceTests`. It also added one registration in `ReservationHttpHost`, so the isolated host can serve `POST Hold`.

## 2. Commands and counts

All runs were on 2026-10-27. Script: scratch `v11/gate4_full.ps1`.

| Step | Command | Result |
|---|---|---|
| Clean build | `dotnet build AeroTech.Ancillary.sln -v q --nologo --no-incremental -nodeReuse:false` | 0 Error(s), 61 Warning(s). Same count as the 61-warning baseline. Kinds: NU1902/NU1903 package advisories, CS8907, CS0108, NU1510, CS8604, CS0618. |
| Domain | `dotnet test tests/AeroTech.Ancillary.Domain.ConformanceTests/… --no-build` | 293 discovered, 293 passed, 0 failed, 0 skipped, no abort |
| Shopping | `dotnet test tests/AeroTech.Ancillary.Shopping.Tests/… --no-build` | 126 discovered, 125 passed, 1 skipped, 0 failed, no abort |
| Application acceptance | `dotnet test tests/AeroTech.Ancillary.Application.AcceptanceTests/… --no-build` | 304 discovered, 304 passed, 0 failed, 0 skipped, no abort |

- The one Shopping skip is the existing `V9PriceRevalidationTests.TwoProductsOfOneCountingFamilyCannotExceedTheFamilyLimitTogether` from the v9 R8 round. **No new skip.**
- Acceptance went from 300 to 304: the two Gate 1 HTTP tests, the Gate 3 I05 race and the Gate 4 HTTP Hold test.
- The SQL tests run on `DotAirAncillary_Tests_{guid}`, a database the fixture builds with the migrations. The HTTP tests use the in-process host `ReservationHttpHost` on `127.0.0.1:0`, which has no hosted services and connects only to that database.
- No dev database, token or production system was used, and no data was cleaned on the Owner's database.

## 3. Scenario evidence (pack 07)

Every test named here passed in the run of §2. "HTTP" means real HTTP through the isolated host.

### W: strict contract

| Id | Evidence |
|---|---|
| W01 | `V12_W01_…` HTTP: a mixed hold of A01 + A11 + A15 + A20 answers 201 with exactly 4 units. SQL shows 1 hold with 4 units. |
| W02 | `V12_W01_…` HTTP: the same body with the families in reverse order returns the same `holdId`. Also `V11_L02_L03_V12_W02_W03…` and `V12_I05…`. |
| W03 | `V12_W01_…` HTTP: a changed SSR answers 409 with 16403. `V11_L02_L03_V12_W02_W03…`: BagRef, SSR and quantity, no write. `V10_I17…V12_D07_W03…`: appointment time and guest count. |
| W04 | `V12_W01_…` HTTP: `coverageScope`, `serviceDefinitionId` and `inventoryFacts` each answer 400. `V11_W02_W06_W10_W11_W19_W21_W28…` (serializer). `stockPoolId` was not sent on its own; every unknown member is refused by strict JSON. |
| W05 | `V12_W01_…` HTTP: `meals[].selected.options.baggage` answers 400. Also the theory case `W10-meals-take-no-options`. |
| W06 | `V12_W01_…` HTTP: `pets` answers 400. Theory cases `W28-pets` and `A09` (`extraSeats`). |
| W07 | `V11_W09_W15_W28…` (`unit:{}`, empty holds) |
| W08 | `V11_W09_W15_W28…`, `V10_A01_A02_A16_I13…` (duplicate `orderServiceId`, malformed unit lists) |
| W09 | `V11_W09…` (quantity 0, the validator requires > 0), `V11_W07_W08_W13_W14…` (above the maximum, 16409) |
| W10 | `V11_D23_V12_D02…` (SSR not allowed, 16402) |
| W11 | `V11_D23_V12_D02…` (`optIn:false`, 16402) |
| W12 | `M1_A12_A14…`, `V10_I01_I02_I03_I04_I05_I12…` (external supplier, 16104 / 422) |

### D: typed round-trip

| Id | Evidence |
|---|---|
| D01–D06, D08 | `V11_D01_D10_D21_W17_W18_V12_D02…`: A03, A06, A10, A11, A12, A15, A20, A23 and A24 read back from SQL in a new scope, then again after a release and the retirement of the provisions. `V11_matrix_a_supported_variant…` (A02, A04, A05, A16, A21, A22). |
| D02 precision | A03 measured weight 27.125 kg is read back exactly. 27.1255 kg is refused with 16402 (`V11_D23_V12_D02…`). |
| D07 | `V10_I17…V12_D07_W03…`: the appointment is read back from SQL as `+03:30`, same instant, guest count 2. |
| D09 | `V11_L04_L05_L07_L12_L13…` (A07 seat 12A with its `seatMapSelectionId`), `SEAT_A01…`, `V10_S02_S05…` (append to a mixed hold). The GET has no `pricingId` (`SeatUnitHoldDto`). |
| D10 | `V11_L01_V12_D10…`: the provision and the definition are retired, the replay returns the same hold, and the GET JSON is byte-equal. `V11_L09…` (a cancelled and an expired unit keep their selection and reason). |
| D11 | Every acceptance test reloads in a new DbContext on a migrated database. A host restart is not tested separately. |
| D12 | `V11_matrix_a_gated_variant…` (A17, A19), `V11_D22…` (A18), theory cases `W28-pets` (A13/A14) and `A09`, Domain `V11_D14…` (A10 quote) |

### I: SQL contention and retirement

| Id | Evidence |
|---|---|
| I01, I02, I03 | `V10_I21_I22_I23…`: four orders race for the last count unit, the last kilogram and the last person. Each race ends with one hold and 16417 for the others. |
| I04 | `V10_I11_I14_I15…`, `V10_I16_I28…` (nothing written when one source is short) |
| I05 | `V12_I05…`. With the shared order lock, every caller gets the same hold or 16416. Without it, all four get the same hold. Both ways: one hold, one allocation row, exact usage. |
| I06 | `V10_I06_I08_I09_I25_L04_L12…`, `V12_L01_L02_L03_L04…` (usage 3 → 2 → 1 → 0) |
| I07 | `V10_I06_I08_I09_I25_L04_L12…` (cancel of a confirmed hold: usage 9.000 → 3.000, ledger rows kept) |
| I08 | `V10_I07_I10…` (stock is free before the poller runs; Confirm is refused) |
| I09 | `V11_I15_I23…`, `V11_I15…` (retire refused while in use, 16621; a racing retire and hold, one wins) |
| I10 | `V11_I23…` (a real `sp_getapplock` timeout gives 16622 and writes nothing), `V10_I26…` (a save that fails leaves nothing) |

### L: lifecycle and query-key safety

| Id | Evidence |
|---|---|
| L01–L04 | `V12_L01_L02_L03_L04…` over HTTP (§6) |
| L05 | the same HTTP test (duplicate, 0, `abc`); unit of another hold answers 16418 / 404 (`V10ReservationAcceptanceTests.cs:61`); Domain `V10_L15…` (all or nothing) |
| L06 | Domain `V10_L01_L08_L06_L07…` (repeating a release is a no-op), `SEAT_A03…` |
| L07 | `V10_L02_L08_L10_L12_L14_E08…`, Domain `V10_L09_L10_L04…` |
| L08 | `V10_I07_I10…`, `SEAT_A05…` (16407 after expiry) |
| L09 | `V10_L02_L08…`, Domain `V10_L11_L12_L13_L14…` |
| L10 | `V10ReservationAcceptanceTests.cs:60` (16413) |
| L11 | `V12_L05…` over HTTP: `Cancellations {"unitIds":[]}` answers 400 |
| L12 | `V10_S02_concurrent_appends…`, `SEAT_A01…`, `V10_S06_S07_L21…` |
| L13 | `V10_S10_S11_V11_L15_L17_L18…` (reachability; see §7) |

## 4. A01–A24

### Supported

| Variant | Test | Result |
|---|---|---|
| A01 | `V12_W01_…` (HTTP) | held, 201 |
| A02 | `V11_matrix_a_supported_variant…(A02)` | held, read back |
| A03 per piece | `V11_D01…`, `V11_L02_L03…`, `V11_D23…` | held, read back; wrong BagRef or weight refused |
| A04 | `V11_matrix…(A04)` | held, read back |
| A05 | `V11_matrix…(A05)` | held, read back |
| A06 | `V11_D01…` | held; equipment kind and dimensions read back |
| A07 | `V11_L04_L05_L07_L12_L13…`, `SEAT_A01…A06`, `V10_S…` | held through `SeatMaps/Holds` |
| A08 | same `SeatMaps/Holds` path as A07; no A08-specific test in this suite | path proven by A07. A08 was held live in v11 Gate 5 (`reports/V11-FINAL-CLOSURE.md`); that run was not repeated here. |
| A10 fixed | `V11_D01…` | held; target cabin read back |
| A11 | `V11_D01…`, `V11_L02_L03…`, `V12_W01_…` | held |
| A12 | `V11_D01…` | held; menu item read back |
| A15 | `V11_D01…`, `V11_L02_L03…`, `V12_W01_…`, `V11_D23…` | held; wrong SSR refused |
| A16 | `V11_matrix…(A16)` | held |
| A20 | `V11_D01…`, `V12_W01_…`, `V11_L01_V12_D10…`, `V10_I17…` | held; airport, facility and appointment read back |
| A21 | `V11_matrix…(A21)` | held |
| A22 | `V11_matrix…(A22)` | held |
| A23 | `V11_D01…`, `V11_D23…` | held with `optIn:true`; `false` refused |
| A24 | `V11_D01…`, `V11_D23…` | held; too many devices refused |

### GATED, fail-closed

| Item | Test | Result |
|---|---|---|
| A09 | theory case `A09` (`extraSeats`) | 400; no request member |
| A10 quote | Domain `V11_D14…` | refused 16419 |
| A13, A14 | theory case `W28-pets` | 400; no request member |
| A17, A19 | `V11_matrix_a_gated_variant…` | refused 16419, nothing written |
| A18 | `V11_D22…` | refused, nothing written |
| Supplier stock | `V10_I01…` (external supplier, 16104) | A Supplier policy can only be activated with the provider key of the active **external** supplier (`AncillaryInventoryPolicy.EnsureSupplierIsRecorded`). Every such product is refused with 16104 before the policy check at `AncillaryHoldChecks.cs:76-77`. |
| AcceptedWeightKg | `V10_I01…` | refused 16419 |

No supplier, provider or FlightFlow reference is made up anywhere.

## 5. GET after status changes and retirement

GET loads only the reservation aggregate (`GetAncillaryHoldByIdService`). `AncillaryHoldMapper` reads only unit columns and the static `AncillaryVariant` table. What the tests prove:

| Stored choice | Test |
|---|---|
| SSR | `V11_D01…` (after release and provision retirement) |
| Dimensions | `V11_D01…` (A06) |
| Menu item | `V11_D01…` (A12) |
| Appointment with offset | `V10_I17…V12_D07…` |
| Plan, as `serviceDefinitionRef` of A24 | `V11_D01…`, whole-JSON comparison before and after retirement |
| Seat (number and selection id) | `V11_L04_L05_L07_L12_L13…` through partial steps |
| Cancelled and expired units with their reason | `V11_L09…` |
| After the **definition** is retired | `V11_L01_V12_D10…` |

A seat unit is not separately read after a seat product is retired. The source proof covers it, because GET reads no catalog.

## 6. F03, before and after (Gate 1)

| Request | Before the guard (red run, 2026-10-26 23:04 UTC) | After the guard |
|---|---|---|
| `DELETE …/Hold/{id}?unitId=<u0>` | 200, **every** unit released | 400 before MediatR, nothing changed |
| `unitIds[]=`, `unitIds[0]=`, `foo=1`, `unitIds=u0&unitId=u1` | not run without the guard | 400, nothing changed |
| `unitIds=`, `unitIds=abc`, `unitIds=0`, duplicate ids | 400 already (F02) | 400, nothing changed |
| no query / `unitIds=u0` / `UNITIDS=u1` | release all / only u0 / only u1 | unchanged |

State while the nine refused calls run: units `1|1|1`, ledger `3|3.000`, usage `3.000` before and after. Then `3|1|1` / 2.000, `3|3|1` / 1.000, `3|3|3` / 0.000. Details are in `reports/V12-GATE1-MINIMAL-SAFETY.md`.

## 7. Boundaries

| Check | Evidence |
|---|---|
| No Ordering access, no order membership check, no FlightFlow call | `V10_S10_S11_V11_L15_L17_L18…`: no type reachable from Hold, SeatMaps/Holds, Confirm, Release, Cancel, Expire or GET reaches Ordering or FlightFlow. The v12.1 diff adds no Ordering, FlightFlow or `HttpClient` reference. |
| Routes | `V12_C02_C04…` passes with the pinned list. `git diff HEAD -- src` changes no `[Http*]` or `[Route]` attribute. |
| Shopping and SeatMap frozen | `git diff HEAD` on `src/AeroTech.Ancillary.Shopping`, `Query/Shopping/SeatMaps` and `RestApi/V1/SeatMaps` is empty |
| Migrations | Source newest `20261026190047_V11ReservationSelections` (command) and `20261016205637_V132ProvisionCancellationRulesQuery` (query) are the last applied on dev (read-only `SELECT`). `has-pending-model-changes`: "No changes have been made to the model since the last migration." |
| JSON / list-column ban | `V11_D01…` counts 0 `nvarchar(max)` columns on `AncillaryReservation%` tables. Flights and selections are child tables. |

No migration was modified, so no roll-forward or roll-back plan is needed. Rolling back v12.1 means reverting `ServiceController.cs` and the test files. The database is untouched.

## 8. Traces to the pack

| Pack file | Where it is checked |
|---|---|
| 02 HTTP contract | §3 W and L rows; routes in §7. Gate 0 errata F04 confirmed: 16418 is 404, an external supplier is 16104 / 422, and binding versus validation give different 400 shapes. |
| 03 variant matrix | §4 |
| 04 aggregate and persistence | `reports/V12-GATE2-TYPED-REHYDRATION.md` §2–§4 |
| 05 inventory, lifecycle and SeatMap | `reports/V12-GATE3-INVENTORY-LIFECYCLE.md` §2 |

`docs/Pack-v11` is history. The v12.1 pack 00 makes it evidence only. Its `00-START-HERE.md` still names the 7a74055 baseline, and it was not edited.

## 9. Evidence ledger

| Claim | Evidence (this round) | State |
|---|---|---|
| Build and all suites green | §2 | VERIFIED |
| Strict JSON on POST Hold | HTTP `V12_W01_…` | VERIFIED |
| Query-key safety on DELETE Hold | HTTP `V12_L01_L02_L03_L04…` plus the red run | VERIFIED |
| Typed choices survive SQL, release, cancel, expiry and retirement | §3 D, §5 | VERIFIED |
| Local stock is correct under contention | §3 I, real SQL Server, `sp_getapplock` | VERIFIED (test database) |
| Same-key race gives one hold | `V12_I05…` | VERIFIED |
| GATED variants fail closed | §4 | VERIFIED |
| No Ordering or FlightFlow call | reachability test, diff grep | VERIFIED |
| Local stock on the dev database | resource and facility references are not connected | **GATED** |
| Ordering end-to-end (E01–E03, E06) | no integration run | **NOT VERIFIED** |
| SeatMaps/Holds over real HTTP | not run in v12.1; its route, binding and code are unchanged | not run (covered by acceptance tests) |
| A08 hold | no A08-specific test; same path as A07 | not separately run |
| Host restart (D11) | not separately run | not run |

## 10. Risk register

| # | Risk | Impact | Likelihood | Mitigation / state |
|---|---|---|---|---|
| R1 | Replay ignores family and `variantCode` (Gate 2 Q1) | Low: the stored receipt is returned, nothing is written | Low: needs a client bug that reuses a key | Owner decision (§11) |
| R2 | A concurrent duplicate gets 16416 instead of the receipt | Low: 409 is retryable; a retry returns the same hold | Medium under client retries | Owner decision (§11) |
| R3 | Local stock is not live on dev | Local products cannot be sold live | Certain until the references exist | GATED |
| R4 | The Ordering integration is untested | Production flow unproven | High until run | NOT VERIFIED |
| R5 | Unknown query keys on GET / Confirm are ignored | None: those routes have no query semantics | — | none needed |
| R6 | A run on a memory-starved machine can hit a 30 s SQL timeout (memory `sql-express-memory-starvation`) | False red runs | Seen before; not in this round | environment |

## 11. Open questions for the Owner

1. **Gate 2 Q1:** should replay also compare the request's family and `variantCode`? My recommendation is (b), yes. See `reports/V12-GATE2-TYPED-REHYDRATION.md` §7.
2. **Gate 3 I05:** may a concurrent duplicate that hits the busy order lock keep answering 16416? My recommendation is to keep it. See `reports/V12-GATE3-INVENTORY-LIFECYCLE.md` §6.

## 12. Pack integrity

This is separate from the application closure. Each zip was extracted with scratch `v11/verify_zip.py` into a new empty folder, and its own `VERIFY-PACK.py` was run with `python -I -X utf8`:

| Zip | Result |
|---|---|
| `docs/AeroTech-Ancillary-Reservation-Pack-v12.1-GATE1-MINIMAL.zip` | PASS: 16 files verified with SHA-256; docs 00..14 present |
| `docs/AeroTech-Ancillary-Reservation-Pack-v12-SOURCE-ALIGNED.zip` | PASS: 16 files verified with SHA-256; docs 00..14 present |

The verifier inside both zips has SHA-256 `ac555b88…e6f2`, the same as the copy in `docs/Pack-v12.1`.

## 13. Stop

The closure ends here. Nothing was committed, pushed or merged. The next step waits for the Owner's instruction.
