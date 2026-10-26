# V11 Gate 3 — Inventory and Hold

Date: 2026-10-27. Branch `feat/ancillary-offers-v9`, base `296197a`; Gates 1–3 and Hold contract R1 are uncommitted.
Authority: `docs/Pack-v11` 13, with 05 and the inventory part of 04.
Verdict: INTERNAL_READY for the Owner's review. A live Local run on dev is **GATED**: dev has no reference source for the flight and stock resources. Nothing is committed or pushed.

## 1. How the sibling does it

- **`InventoryLedger`** (`Persistence/AncillaryReservationAggregate/InventoryLedger.cs`):
  - One transaction per hold.
  - An exclusive `sp_getapplock` per source, owned by the transaction, taken in sorted order (kind, then id), with a 20 s wait.
  - A timeout raises 16622/409. `BalancesAsync(now)` returns capacity, status, `ClosedForSale` and the active usage.
- **Capacity adjust** (`AdjustFlightCountInventoryService`):
  - Locks the source through the same ledger and reads the active usage.
  - The Domain refuses a capacity below it (16621). Retire now follows this pattern exactly.
- **Policy activation evidence:**
  - `IInventoryCommercialFactsReader` (Persistence, data only) → `InventoryPolicyEvidenceBuilder` (Application) → `AncillaryInventoryPolicy.Activate` (Domain rule).

## 2. What changed

### 2.1 Retire guard (pack 13 item 5)

- **Domain:** `FlightCountInventory.Retire`, `FlightWeightInventory.Retire` and `AirportSlotInventory.Retire` take the active usage. If it is above 0, they refuse with 16621/409 (`InventoryCapacityBelowActiveUsage`).
- **Application:** the three retire services lock the source with the ledger, read `BalancesAsync(now)[source].ActiveUsage`, retire, save and commit in the same transaction.
  - This is the same lock a Hold and an Adjust take.
  - No new coordinator.

### 2.2 A slot policy needs an appointment (the Owner's "yes", 2026-10-26)

- `InventoryCommercialFactsReader` returns the definition's `AirportService.RequiresSpecificAppointment`.
- `InventoryPolicyEvidenceBuilder` passes it on.
- `AncillaryInventoryPolicy.EnsureLocalIsComplete` refuses the AirportSlot pattern unless the value is `true` (16606/409).

### 2.3 Hold input mapping (pack 13 items 1–4, 6)

No Gate 3 code was needed: Gates 1–2 already do this mapping.
- `coverage.flightIds` → `CoveredFlightIds`.
- `selected.quantity` (or the fixed quantity) → demand.
- `airportService.timeWithOffset`/`guestCount` → slot demand.

The path runs through `AncillaryHoldAllocator`, `AncillaryHoldChecks` and the ledger. The source is derived from the policy and the database, never from the caller. Replay compares the typed options; since R1 they are typed per family. There is no mutable held counter, and allocation rows stay as history.

### 2.4 Tests

- `V10_I19` lost its retire race, because a retire with a live hold is now refused. It keeps the close and suspend races.
- **New:**
  - Domain `V11_I15`.
  - Acceptance `V11_I15_I23`, `V11_I15_a_weight_or_slot_source…`, `V11_I17`, `V11_I22` and `V11_I23`.
  - Domain `V10_I01_I02_I03_I04` gains the Retired-policy case.
- Slot-policy fixtures now carry `RequiresSpecificAppointment = true`. `P2InventoryPolicyConformanceTests` asserts 16606 without it.

## 3. Acceptance I01–I23 (pack 07)

All tests below run against SQL Server through the real ledger. The race tests start every attempt together (`InventoryHarness.RaceAsync`, `Task.WhenAll`), each in its own `AncillaryScope`, which means its own DbContext and SQL connection.

| ID | Pack row | Test | Evidence |
|---|---|---|---|
| I01 | Unlimited → Held, zero allocations | `V10_I01_I02_I03_I04_I05_I12…` | held quantity 2; no allocation row |
| I02 | Missing, Draft or Retired policy refused | `V10_I01_I02_I03_I04_I05_I12…` (missing, Draft, Suspended); Domain `V10_I01_I02_I03_I04…` (missing, Draft, Retired) | 16419; reservations `0\|0` |
| I03 | Shared count source across products | `V10_I07_I10…` | two products use 2.000 of 2; the third order gets 16417 |
| I04 | FixedKg × quantity | `V10_I11_I14_I15…` | 2 × 12.5 kg = 25.000 |
| I05 | Count+Weight second part fails → nothing | `V10_I11_I14_I15…` | 16417; count stays 2.000; `0\|0` |
| I06 | Portion, one flight without a source | `V10_I16_I28…` | 16419; both sources 0.000; with the second source, both get 1.000 |
| I07 | Two contiguous slot slices | `V10_I17_I18_I19_I20_L19_L20…` | morning 3.000 + noon 3.000 |
| I08 | Slot gap | same | 16419 |
| I09 | `guestCount` adds persons | same | 2 guests + 1 = 3.000 persons |
| I10 | No appointment where one is required | same | 16402; no clock substitute |
| I11 | Last count unit, 4 orders | `V10_I21_I22_I23…` | 1 success, codes `[16417]`, usage 1.000 |
| I12 | Last kilograms, 4 orders | same | 1 success, usage 12.500 |
| I13 | Last slot person, 4 orders | same | 1 success, usage 1.000 |
| I14 | Adjust below active usage | `V10_I24…`; Domain `V10_I24…` | 16621; capacity `3\|2`, usage 2.000; Adjust vs Hold race: one wins |
| I15 | Retire with active usage | Domain `V11_I15`; `V11_I15_I23…`; `V11_I15_a_weight_or_slot_source…` | 16621 for Count, Weight and Slot; Retire vs Hold race: exactly one wins |
| I16 | Hold never changes capacity | `V10_I06_I08_I09_I25…`, `V10_I11_I14_I15…` | capacity\|version `10\|2`, `5\|2`, `30.000\|2` before and after |
| I17 | Confirmed counted after expiry | `V11_I17_a_confirmed_unit_keeps_its_stock_after_the_hold_deadline` | clock +31 min: usage 1.000; a new hold gets 16417 |
| I18 | Expired Held freed before the poller | `V10_I07_I10…` | clock +31 min: a new hold succeeds while old rows still say Held |
| I19 | Unsupported Supplier | `V10_I01_I02_I03_I04_I05_I12…` | 16104/422 before any write |
| I20 | AcceptedWeight / per-kilo | same; Domain `V10_I12…` | 16419; weight usage 0.000 |
| I21 | Failure after Save | `V10_I26_a_save_that_fails_after_the_write…` | no reservation and no allocation |
| I22 | Two products on one source in one hold | `V11_I22…`; Domain `V10_I07_I08_I14…` | 2 + 1 on a 2-unit source: 16417, usage 0.000; 1 + 1: held, usage 2.000 |
| I23 | Real SQL lock timeout | `V11_I23_a_real_sql_lock_timeout…` | another connection holds the applock: 16622 after 20 s, `0\|0`, usage 0.000; after release the same request holds once, and a replay returns the same id |

## 4. SQL usage before and after

Values are read by the tests from `AncillaryReservationUnitAllocations` and the source tables.

| Test | Before | After |
|---|---|---|
| `V10_I06…` | capacity `10\|2` | holds (6 confirmed + 3 held) → usage 9.000; cancel the confirmed one → 3.000; capacity still `10\|2` |
| `V10_I24…` | usage 2.000, capacity `3\|2` | Adjust to 1 refused; unchanged |
| `V11_I15_I23…` | one live hold | Retire refused; after release, the race ends `2\|0` / 1.000 (hold won) or `4\|0` / 0.000 (retire won) |
| `V11_I15_a_weight_or_slot_source…` | weight 12.500, slot 1.000 | both retires refused; after release both are Retired at version 3 |
| `V11_I17…` | confirmed 1.000 of 1 | after the deadline still 1.000; the next hold is refused |
| `V11_I23…` | 0.000 while the lock is held | 1.000 after the lock is released |

## 5. Failure rollback

- A save that fails after the write leaves nothing (`V10_I26`).
- A refused Count+Weight hold, a refused two-product hold and a lock timeout write no reservation and no allocation (`V10_I11…`, `V11_I22`, `V11_I23`).

## 6. Migration

- None in Gate 3, and none in R1.
- The only v11 migration is Gate 2's `20261026190047_V11ReservationSelections`, already applied to dev.

## 7. W01, R03, R04

- **W01:** `V11_W01_the_pack_sample_binds…` and `V11_W01_W12_a_family_hold…` pass.
- **R03** (2026-10-27, logs in scratch `v11/full3`):

| Step | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 errors, 59 warnings |
| Domain conformance | 293 discovered, 293 passed |
| Shopping | 126 discovered, 125 passed, 1 skipped |
| Application acceptance | 288 discovered, 288 passed |
| Aborted or crashed runs | none |

- **R04:** the concurrency proofs use real SQL connections (see §3). There is no in-memory fake of the ledger or the lock.
- All prior v10 stock tests pass in the same run.

## 8. Notes

- `V11_I23` takes 20 s because the ledger wait is fixed (`InventoryLedger.SourceLockWait`). Making it configurable would shorten the test. That needs the Owner's order and is not done.
- Freeze pins: 133 files, `3E998D04…E90` (changed by R1, not by Gate 3).
