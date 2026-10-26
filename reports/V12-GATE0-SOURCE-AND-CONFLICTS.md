# V12 Gate 0 — read-only source and contract audit

Date: 2026-10-27. Authority: `docs/Pack-v12` 10 (Gate 0), 00–09; `CLAUDE.md` wins.
Mode: read-only. No code, schema, test, lock or DB change. This report is the only file written.
Verdict: the v11 source matches the v12 contract in substance. The pack's **P0 is not reproducible over HTTP** (F02). Five pack text corrections are needed (F01, F04–F07). One new risk is not verified yet (F03), and one test gap remains (F08). STOP: Owner approval is needed before Gate 1.

## 1. HEAD and working tree

| Item | Value (commands run 2026-10-27) |
|---|---|
| `git rev-parse HEAD` | `8c2fb087d43c04a06cf9eb18dc1bdd5734fff51d` — equals the pack benchmark |
| `git log -3 --oneline` | `8c2fb08 v11`, `296197a v11 Docs and Reports`, `7a74055 V10 Code` |
| Uncommitted since HEAD | the Owner-ordered route rename `Service-Holds` → `Hold`: `ServiceController.cs`, the three pin test files, `docs/Pack-v11/01`, `02`, `SHA256SUMS.txt` |
| Untracked | `docs/Pack-v12/`, `docs/AeroTech-Ancillary-Reservation-Pack-v12-SOURCE-ALIGNED.zip` |
| Pack integrity | `python VERIFY-PACK.py` in `docs/Pack-v12`: PASS, 16 files |

## 2. Findings

| Id | File:Line / evidence | Severity | Finding | Proposed minimal fix | Migration | Test |
|---|---|---|---|---|---|---|
| F01 | `ServiceController.cs:24,39,43,47,51` (`Hold`, `Hold/{holdId:long}`, …) vs `Service-Holds` in Pack-v12 `00` (1), `02` (7), `07` (3) | High (doc) | The pack names the old routes; the Owner renamed them after the pack was written | replace `Service-Holds` with `Hold` in 00, 02, 07 | none | `V12_C02_C04…` route pin (48/48 affected tests passed after the rename) |
| F02 | `ServiceController.cs:48-49`; `ServiceReleaseAncillaryHoldCommandValidator.cs:11-15`; `AncillaryReservation.cs:192-204`; HTTP evidence scratch `v11/g4_http.json` (run 2026-10-27, own host :5291) | **P0 → not reproducible** | See §3. `?unitIds=` is refused with 400 at binding, before the controller. The ternary can only receive an empty array when the key is absent, which is the intended "release all" | none required; correct pack 00, 01 §6, 05 "P0", 07 L03/L04, 09, 11 | none | Gate 4 HTTP rows (§3) |
| F03 | ASP.NET collection binding (not run) | Open risk, **NOT VERIFIED** | A query key the binder does not recognise as `unitIds` (for example `?unitIds[]=5` or `?unitId=5`) may bind as "absent", which means release **all** Held units | Gate 1 probe first (§3.3); if confirmed, the Owner decides: (a) keep, document; (b) refuse unknown query keys on `DELETE Hold/{id}`; (c) make "release all" explicit | none | new HTTP probe |
| F04 | `ExceptionFactory.cs:152` (16401/404), `:203` (16418/404), `AncillaryHoldChecks.cs:38-39` → `ExceptionFactory.cs:32` (16104/422) | Medium (doc) | Pack 02-F lists 16418 under 409 (it is 404) and does not name 16401 for an unknown hold. 07 W12 expects a "16419-equivalent" for an unsupported supplier; the source answers 16104/422 (also seen live in `reports/V11-ALL-VARIANTS-LIVE.md` §4). Two 400 shapes exist: binding = ASP.NET ProblemDetails, validation = platform `Validation failed` | correct 02-F and 07 W12 | none | `M1_A12_A14…` (16104) |
| F05 | `AncillaryReservationUnit.cs:63-64` (`Sector` ⇒ exactly one flight); dev A20 provisions are Sector (`reports/V11-ALL-VARIANTS-LIVE.md` source inspection) | Medium (doc) | The A20 unit in the pack 02-A sample has no `coverage`. On a Sector provision it is refused | add `"coverage":{"flightIds":["801"]}` to the A20 sample, or note "Order-scope provision only" | none | — |
| F06 | `AncillaryHoldDto.cs`, `AncillaryHoldMapper.cs` | Low (doc) | 02-C puts Connectivity `deviceCount` under `products[].details`. It is in `units[].selected.options.connectivity`; Assisted travel, Priority and Connectivity have no `details` | correct the 02-C sentence | none | `V11_D01…`, `V11_W29…` |
| F07 | `AncillaryUnitOfferDto.cs:13-14`, `SelectedUnitDto.cs:6-8` | Low (doc) | The pack does not mention R1-Q5 (the six-key options union still in the Offers and Resolve output, waiting for an Owner order) | add to 00 or 01 as an open item | none | — |
| F08 | `HoldAncillaryServicesService.cs:82-89` (`catch … FindByIdempotencyKeyAsync … Replayed(winner…)`) | Low (test gap) | v12 I05 (two concurrent creates with the same idempotency key → one Root) has code but no SQL race test for the generic Hold. The seat append has one (`V10SeatAndLifecycleAcceptanceTests.cs:328`) | add one acceptance race test in Gate 3 if the Owner wants I05 proven | none | new I05 |
| F09 | `reports/V11-ALL-VARIANTS-LIVE.md` §2 | Info (data) | Two dev products created on the Owner's order carry a 0 IRR placeholder price: `UPG_ECO_TO_BUS_FIXED` (A10), `MEAL_SAFFRON_CHICKEN_LOCAL` (A12) | the Owner gives the real price or retires them | none | — |

## 3. P0 trace (pack 10 step 5)

### 3.1 Path

1. **Binder:** `[FromQuery] long[]? unitIds` (`ServiceController.cs:48`).
2. **Controller:** `unitIds is { Length: > 0 } ? unitIds : null` (`:49`).
3. **Validator:** `UnitIds` NotEmpty + distinct when not null; each > 0 (`ServiceReleaseAncillaryHoldCommandValidator.cs:11-15`).
4. **Domain:** `Release(null)` releases every Held unit; `Release(ids)` releases only those, all-or-nothing (`AncillaryReservation.cs:192-204`).

### 3.2 Recorded HTTP behaviour

From `reports/V11-GATE4-SEAT-LIFECYCLE.md` §3; raw answers in scratch `v11/g4_http.json`. The run used the same controller code, with the route then named `Service-Holds`.

| Request | Status | Reached | Meaning |
|---|---|---|---|
| `DELETE …/999999999` (absent) | 404 `16401` | handler with `null` | absent = all |
| `DELETE …/999999999?unitIds=` | **400** binding: `unitIds`: "The value '' is invalid." | never | explicit empty is refused before the controller |
| `DELETE …?unitIds=0` | 400 Validation failed `UnitIds[0]` | validator | never expands |
| `DELETE …?unitIds=5&unitIds=5` | 400 Validation failed `UnitIds` | validator | never expands |

Conclusion:
- Over HTTP the binder never hands the controller a present-but-empty array. The only way to reach `Length == 0` is an absent key.
- So L03 already holds, and L04 ("binder array length = 0") cannot be built over HTTP.
- The `Cancel` body `unitIds: []` is refused by `CancelAncillaryHoldValidator.cs:12-15` (NotEmpty when not null). Test: `V10_A01_A02_A16_I13…`, `cancels.Validate(… [])` is false.

### 3.3 Not verified (F03)

Cases not yet sent:
- `?unitIds=abc`,
- `?unitIds=&unitIds=`,
- `?unitIds[]=5`,
- `?unitIds[0]=0`,
- `?unitId=0`,
- `?unitIds=5,6`.

A safe probe, with no DB write, needs my own host. Send each with `0` or a bad value to an unknown hold, then read the status:
- 400 means the key was bound and refused.
- 404 means the key was ignored and treated as "all".

Running that host starts its hosted services (expiry poller, outbox publisher) against the dev DB, so it is not strictly read-only. It waits for the Owner's approval (Gate 1).

## 4. Contract audit (pack 02, 03)

| Item | Source | Result |
|---|---|---|
| Request families and types | `HoldAncillaryServicesRequest.cs` (7 typed arrays, no `pets`), generic `AncillaryVariantHoldInput<TSelected>` chain, `SelectedUnitInput`/`SelectedUnitInput<TOptions>`, five `*HoldOptionsInput`, `AssistedTravelHoldSelectionOptions` | equal to 02-A; strict JSON on all records |
| `unit.travellerId` | `HoldUnitInput(long TravellerId)`, validator > 0 | equal ("if `unit` is sent, `travellerId` is required") |
| `selected.quantity` | validator > 0; provision bounds 16409 in the Domain | equal |
| Receipt | `AncillaryHoldReceipt(holdId, reference, status, expiresAt, units[unitId, orderServiceId, status])` | equal to 02-B |
| GET | `AncillaryHoldDto` typed per family, `SeatUnitHoldDto` with `seatNumber`/`seatMapSelectionId`, no `pricingId` | equal to 02-C except F06 |
| Confirm, Release, Cancel | routes now `Hold…` (F01); semantics equal to 02-D (§3) | equal |
| SeatMaps/Holds | `HoldSeatSelectionsRequest.cs` unchanged, not strict (pack v11 decision) | equal to 02-E |
| Status codes | `ExceptionFactory.cs:152-206` | see F04 |
| Variant matrix (03) | gates in `AncillaryHoldChecks.cs`: confirmation `:55`, variant gate (A17, A18, A19) `:58`, external quote `:61`, external supplier `:38-39`, AcceptedWeight `:82`; seats skip allocation `AncillaryHoldAllocator.cs:67` | equal to 03; §6 lists the proof per variant |

## 5. Domain, persistence, inventory and boundaries

| Item | Source | Result |
|---|---|---|
| Root methods | `AncillaryReservation.cs`: `Hold:59`, `AppendSeats:81`, `HoldsSeats:115`, `EnsureSameContent:137,149`, `StatusOf:165`, `EffectiveStatus:170`, `Confirm:175`, `Release:192`, `Cancel:228`, `Expire:269` | equal to pack 04 |
| Unit limits | `AncillaryReservationUnit.cs:13-17`: SeatMapSelectionId 256, SeatNumber 16, AppendIdempotencyKey 128, VariantCode 8, ServiceDefinitionRef 30; `StockPoolId`/`ProviderUnitRef` legacy nullable `:145,147` | equal to pack 04 |
| Owned selections and precision | `AncillaryReservationUnitConfiguration.cs:40-47`, `HasPrecision(18, 3)` | equal (pack 04 says 18,3) |
| Migration | `20261026190047_V11ReservationSelections`: 7 child tables, last applied on dev (Gate 5); `has-pending-model-changes` → none (2026-10-27, before the rename, which has no model change) | equal; no new migration needed |
| Ledger | `InventoryLedger.cs:47` `sp_getapplock` Exclusive, owner Transaction | equal |
| Retire guard | the three `Retire*InventoryService.cs:47-49` read `ActiveUsage` under the ledger lock | equal |
| No Ordering / FlightFlow from reservation operations | `V10_S10_S11_V11_L15_L17_L18…` (reachability from Hold, SeatMaps/Holds, Confirm, Release, Cancel, Expire, GET) | proven by test |

## 6. Pack 07 rows against existing evidence

| v12 Id | Existing evidence | State |
|---|---|---|
| W01 | live mixed holds (`V11-FINAL-CLOSURE.md` §4, `V11-ALL-VARIANTS-LIVE.md` §3) | covered |
| W02, W03 | `V11_L02_L03…`, live Gate 5 steps 02–03, `V10_I17…` | covered |
| W04 | live Gate 5 step 15 (four members), `V11_W02_W06_W10…` theory | covered (no separate `stockPoolId` case; any unknown member is refused) |
| W05, W06 | `V11_W02_W06_W10_W11_W19_W21_W28…`, Gate 4 HTTP | covered |
| W07, W08 | `V11_W09_W15_W28…`, `V10_A01_A02_A16_I13…` | covered |
| W09 | `V11_W09…` (quantity 0), `V11_W07_W08_W13_W14…` (above max → 16409) | covered |
| W10, W11 | `V11_D23…` (SSR, OPT-IN) | covered |
| W12 | `M1_A12_A14…` | covered; the code is 16104 (F04) |
| D01–D10 | `V11_D01_D10_D21_W17_W18…`, `V11_L09…`, `V11_L04…`, `V11_matrix…`, live Gate 5 steps 16–21 | covered |
| D11 | reload from a fresh DbContext in every acceptance test; host restart not separately proven | covered by reload; restart NOT VERIFIED (low) |
| D12 | `V11_matrix…` (A17, A19), `V11_D22…` (A18), W28 (pets), `extraSeats` (A09), Domain `V11_D14` (A10 quote) | covered |
| I01–I04, I06–I10 | `V10_I21_I22_I23…`, `V10_I16_I28…`, `V10_I11…`, `V10_I06…`, `V10_I07_I10…`, `V11_I15…`, `V11_I15_I23…`, `V11_I23…`, `V10_I26…` | covered |
| I05 | none for the generic Hold | **gap** (F08) |
| L01–L03, L05–L13 | Gate 4 HTTP, live Gate 5 steps 05–13 and 26, Domain `V10_L…`, `SEAT_A03`, reachability test | covered; malformed `?unitIds=abc` not sent (F03 probe) |
| L04 | — | not constructible over HTTP (§3.2) |

## 7. Tests

- Discovered now (`dotnet test --no-build --list-tests`, 2026-10-27): Domain 293, Shopping 126, Application acceptance 300.
- Last full run (2026-10-27, before the route rename): build 0 errors / 61 warnings; 293/293, 125 passed + 1 skipped / 126, 300/300; no abort.
- After the rename only the affected classes were run (Boundary, `V11WireAcceptanceTests`, `V121FinalCrossPhase…`): 48/48.
- No test was run in this Gate.

## 8. A01–A24

| Variant | State | Gate in source |
|---|---|---|
| A01–A06, A10 (`FixedAncillary`), A11, A12, A15, A16, A20–A24 | supported | — |
| A07, A08 | supported through `SeatMaps/Holds` | no allocation (`AncillaryHoldAllocator.cs:67`) |
| A09 | GATED | no `extraSeats` member (strict JSON) |
| A10 with a quote | GATED | `AncillaryHoldChecks.cs:61` |
| A13, A14 | GATED | no `pets` member; the Definition forces confirmation |
| A17, A18, A19 | GATED | `AncillaryHoldChecks.cs:55,58` |
| Supplier stock, AcceptedWeight | unsupported | `AncillaryHoldChecks.cs:38-39`, `:82` |
| Local stock live on dev | GATED | no dev reference source; dev has 0 Local policies and 0 stock sources |

## 9. Recommendation and STOP

1. **No source change is needed for the P0** (F02).
2. Before any code, the Owner decides whether to run the F03 probe (needs my host; its hosted services touch the dev DB).
3. The pack text corrections (F01, F04–F07) can be made in Pack-v12 itself on the Owner's order.
4. Gate 2 and Gate 3 have no proven gap except the optional I05 test (F08). Pack 12 says to report `NO CODE CHANGE REQUIRED` in that case.

Waiting for the Owner. No commit, push or merge.
