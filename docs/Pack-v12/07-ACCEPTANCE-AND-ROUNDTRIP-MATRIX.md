# 07 — Acceptance Contract + Execution Evidence Ledger

**معیار:** Test سبز فقط با command واقعی و discovered=executed و log همان Gate. نقل گزارش v11، اجرای تست توسط تولیدکننده این Pack نیست. هیچ Test Skip تازه یا `Assert.True(true)` جایگزین معیارهای دامنه‌ای نباشد.

## A. Gate 0 — Contract/Shape (HTTP واقعی)
| ID | Scenario | Expected |
|---|---|---|
| W01 | Mixed Hold A01+A11+A15+A20 with actual enabled Provision | `201` receipt، دقیقاً 4 Units و 4 OrderServiceIds |
| W02 | Same idempotencyKey + same semantic payload (order of family/units changed) | same HoldId/UnitIds؛ no duplicate ledger rows |
| W03 | Same key, changed SSR/BagRef/Quantity/Appointment | `409`, no new write |
| W04 | `services`, `coverageScope`, `serviceDefinitionId`, `inventoryFacts`, `stockPoolId` provided | 400 strict JSON |
| W05 | Unexpected `meals[].selected.options.baggage` | 400 before allocation |
| W06 | `pets[]`, `extraSeats[]` supplied | 400 unsupported wire |
| W07 | `unit:{}` / no required travellerId | fail-closed when unit is present |
| W08 | Missing/empty Products/Units, duplicate OrderServiceId, duplicate flightIds | 4xx, atomic no writes |
| W09 | `selected.quantity=0`, negative, > local provision bounds | 4xx, no writes |
| W10 | `A15` SSR not in available allowed values | 4xx, no writes |
| W11 | A23 optIn=false | reject purchase, no receipt |
| W12 | unsupported Supplier, invalid Source Reference | 16419-equivalent, no fabricated ref |

## B. Historical reconstruction (Domain/SQL Reload/GET)
| ID | Scenario | Expected persisted identity |
|---|---|---|
| D01 | A01/A02/A05 Qty | same Quantity after reload, Released/Cancelled/Expired |
| D02 | A03 BagRef + measured decimal 25.500 | same `BagRef`, decimal value (not float approximation) |
| D03 | A04 3 Dimensions; A06 weight and equipmentKind | same three dimensions, weight, fixed equipment |
| D04 | A11 MealCode; A12 MenuItemRef | true fixed choices from Definition at time of Hold |
| D05 | A10 FixedAncillary ToCabinId | same target cabin; quoted mode rejected |
| D06 | A15/A16 SSR code | same code after Retire Definition |
| D07 | A20–22 `timeWithOffset` + guestCount, AirportId/FacilityId | exact offset and fixed context; no read from active Definition |
| D08 | A23 true/no bogus priority child; A24 deviceCount + Plan | correct option, no extra family keys |
| D09 | A07/A08 SeatNumber + selection token, SeatMap append | same seat details, no pricingId in GET |
| D10 | GET after Definition retired, Provision invalid or Price snapshot expired | historical output invariant |
| D11 | Rehydrate after migration and restart | row count and typed selections identical |
| D12 | GATED A09/A13/A14/A17/A18/A19/A10 Quote | no hold row, no bogus data, no fake Supplier ref |

## C. Inventory SQL transaction & concurrency
| ID | Scenario | Expected |
|---|---|---|
| I01 | 2+ concurrent Orders for last Flight Count | exactly one committed, others out of stock; usage<=capacity |
| I02 | simultaneous weight competition | exactly one winner (depending test capacity), decimal fidelity |
| I03 | same Airport Slot and overlapping appointment window | no double-book above people capacity |
| I04 | MultiFlight Count/Weight/Slot one source insufficient | 0 Units and 0 Allocations committed |
| I05 | Idempotency race same key | one Root, same response winner |
| I06 | Release Held Unit | usage freed only for its allocations; others unchanged |
| I07 | Cancel Confirmed Unit | usage freed; historical status remains |
| I08 | Expired Held with no poller | available usage ignores expired; Confirm blocked |
| I09 | Retire source with active usage | rejected while lock held; cannot replace capacity silently |
| I10 | Lock contention, timeout, transaction rollback | existing error; no partial allocation |

## D. Lifecycle & Empty-Query Regression P0
| ID | Request | Expected |
|---|---|---|
| L01 | `DELETE /Service-Holds/9001` (no query) | releases all Held |
| L02 | `DELETE /Service-Holds/9001?unitIds=9101` | releases only 9101 |
| **L03** | **`DELETE /Service-Holds/9001?unitIds=`** | **400; ZERO state changes** |
| **L04** | **present-but-empty `unitIds` (binder array length=0)** | **400; never all** |
| L05 | duplicate, 0, foreign ID, malformed ID | fail-closed, unchanged all-or-nothing |
| L06 | Repeat same release on selected released Units | idempotent same statuses |
| L07 | Confirm active with one Released Unit | only remaining Held confirmed |
| L08 | Confirm after expiry | 409, no usage held |
| L09 | Cancel confirmed with reason + unitIds | only targeted Units Cancelled |
| L10 | Cancel same IDs with changed reason | 409 (existing 16413) |
| L11 | Cancel `unitIds:[]` | 4xx, never Cancel all |
| L12 | Seat append with same key | no duplicated Unit; same Root |
| L13 | Seat commercial no FlightFlow outbound or physical allocation | no seat occupancy update by Ancillary |

**P0 invariant:** any Query with an explicitly supplied but invalid/empty `unitIds` must leave every Unit and ledger row unchanged. Test both REST Controller binding and Root domain independently.

## E. Actual reports vs independent findings
| Evidence | Reported by `reports/V11-FINAL-CLOSURE.md` | Independently validated in this Pack |
|---|---|---|
| `dotnet build` | 0 errors / 61 warnings | **NO — environment no repo build** |
| Domain | 293 discovered / 293 passed | **NO** |
| Application | 300 discovered / 300 passed | **NO** |
| Shopping | 126 discovered / 125 passed / 1 existing skip | **NO** |
| SQL concurrency | separate scopes and `sp_getapplock`, one winner | **NO** |
| Live HTTP | 53/53 steps, mixed + seat append + expiry | **NO** |
| Live Local Flight/Slot | **GATED** | **NO** |
| Ordering end-to-end E01–E03/E06 | **NOT VERIFIED** | **NO** |
| Release explicit empty | **new finding against HEAD** | Code-level confirmed, HTTP repro pending |

## F. Mandatory run records from agent
For each Gate report: branch/sha, `git status --short`, command exact, date/time, discovered/executed/passed/skipped/failed/aborted, DB isolation strategy, HTTP JSON fixture/response sanitized, SQL before/after and transaction evidence, route diff, `grep`/search no Order outbound or FlightFlow physical writes, and explicit `GATED` table. Close only after owner review. Do not reuse past numbers as newly executed tests.
