# 06 — Failure, Retry, Expiry, Migration و قواعد عملیاتی

## 1. ترتیب ایمن عملیات و rollback
- **Create Local Hold:** ابتدا همه DTOها، policies، source ids و demandها بررسی شوند؛ در یک تراکنش با lock منابع، ظرفیت فعلی سنجیده، یک Reservation + Units + allocations نوشته شوند. Failure هر جزء ⇒ zero new row.
- **Seat Append:** Root و Unitها با order lock و transaction خوانده شوند؛ existing Root changes و Seat verifier results با یک save ثبت شوند؛ failure append نباید Unit قبلی را حذف کند.
- **Confirm:** فقط وقتی Root Held و تمام Unitهای فعال واقعاً Held و non-expired هستند؛ Released units untouched. درخواست Confirm اگر همه قبلاً Confirmed شده‌اند replay موفق است. **Replay یک عملیات قبلاً کامل‌شده قبل از new-operation state check ارزیابی می‌شود.**
- **Release/Cancel Unit:** lock مربوط Reservation/Order، load child، status change و save یک‌جا. Capacity از لحظه Commit دیگر توسط ledger مصرف نمی‌شود.
- **Expire:** Read به‌صورت lazy با UTC اکنون Hold را منقضی نشان دهد؛ Background Poller فعلی صرفاً وضعیت را persisted می‌کند؛ عدم اجرای Poller نباید منجر به oversell/under-availability شود.
- **Unknown failure در Save:** از key idempotent، state واقعی ذخیره‌شده read شود؛ اگر ذخیره شده، همان Hold و Unit refs پاسخ داده شود؛ اگر نشده، failure و retry؛ از ساخت Hold مجدد بدون بررسی جلوگیری شود.

## 2. جدول انقضا و اثر موجودی
| Time/State | Query availability | Read Hold | Confirm allowed? |
|---|---|---|---|
| Held، deadline آینده | counted | Held | yes |
| Held قدیمی، deadline `null` | **counted تا reconciliation** | Held با هشدار legacy | هیچ Hold بدون Expiry جدیدی مجاز نیست |
| Held، deadline گذشته، poller هنوز اجرا نشده | **NOT counted** | Expired (effective) | no |
| persisted Expired | NOT counted | Expired | no |
| Confirmed، deadline گذشته | counted | Confirmed | idempotent no-op |
| Released | NOT counted | Released | no |
| Cancelled | NOT counted | Cancelled | no |

تکرار Release یا Cancel نباید Allocation را دومرتبه آزاد کند؛ چون ledger immutable است، double-release به لحاظ محاسباتی ناممکن می‌شود.

## 3. Idempotency دقیق
| Operation | Idempotency identity | Same request | Different request |
|---|---|---|---|
| Create Hold | `Reservation.IdempotencyKey` (unique) | same HoldId, Units, Expiry | 409 |
| Seat append | `(HoldId, AppendIdempotencyKey)` با Unitهای موجود | same parent, no new Units | 409 |
| Confirm | `holdId` + current state | no-op if already confirmed | invalid transition 409 |
| Release all/unit | holdId (+unitId) | no-op if already released | wrong/confirmed state 409 |
| Cancel all/unit | holdId (+unitId) + original reasonCode | no-op when already cancelled with same reason | 409 when reason changes (do not overwrite audit) |
| Expire | holdId + cutoff | no-op after persisted expiration | no double resource effect |

Canonical payload comparisons include ALL capacity-relevant facts (`acceptedWeightKgTotal`, `timeWithOffset`, `guestCount`) and SeatMap ID/SeatNumber. Current `EnsureSameContent` in Domain compares existing base fields only; it must be extended accordingly. Do not introduce `RequestFingerprint` table solely for this.

## 4. Live competition / lock order
- Hold OrderA/OrderB competes for same stock → both order locks are different؛ ensure inventory source-level locks inside SQL transaction and source key ordering prevents double assignment.
- Unit Cancel overlapping with New Hold → at worst the new Hold momentarily reads stale available and returns shortage; must never oversell.
- Two Seat append requests to same Root → existing `ancillary-hold:{orderId}` lock and DB Unit uniqueness protect against duplicate Unit.
- Admin adjustment reducing capacity below active ledger usage → reject under same source lock.
- Client timeout after successful Commit → retry returns existing Hold, no new allocation.

## 5. Failure Matrix
| Error | Applied behavior | Compensation |
|---|---|---|
| Local one stock shortage in mixed Create | all operation rollback | none |
| Unsupported inventory Pattern/Supplier | fail pre-write, explicit error | none |
| Seat append invalid selection | leave old Root Held unchanged | Ordering decides release/retry |
| FlightFlow fails physical seat hold | Ancillary never involved in physical assignment | Ordering release any prior Ancillary hold |
| Seat commercial Append success, Ordering later fails | Ancillary Unit remains Held till Release/Expire | Ordering Release call (no FlightFlow call from Ancillary) |
| Confirm after deadline | reject | Ordering compensation if necessary |
| Cancel before Confirm | reject; use Release | none |
| Provider external timeout | **not implemented/gated**, no Fake Held | none |
| Expiry poller unavailable | effective status already expired; allocation not counted | poller later persists status |
| Response status write succeeded but caller timed out | same key returns stored state | none |

## 6. Storage/Migrations
**CommandDb only** (`AncillaryDbContext`, schema `Ancillary`):
1. Add `AncillaryReservationUnitAllocations` table with `Id`, `AncillaryReservationUnitId`, `ResourceKind`, `InventorySourceId`, `Amount decimal(19,3)`.
2. Add three nullable facts + `AppendIdempotencyKey` to ReservationUnit. No renaming/deleting existing tables.
3. Indexes as `03-DOMAIN...`; no unnecessary shadow FK or duplicate provider reference.
4. `Down` migration reverses only these additive changes and is demonstrably reversible **in a test database**; never drop active historical data on deployed DB.
5. No QueryDb projection unless a concrete existing consumer requires it; Hold GET reads CommandDb via current pattern.
6. Run EF `has-pending-model-changes` and inspect actual migration SQL. No private production/dev data reset or deletion without explicit owner authorization.
7. Legacy held/confirmed records created before allocation feature: enumerate and report affected rows. **If a legacy live reservation lacks allocation evidence for a Local stock source, block new reservations on that source until verified reconciliation; never assume zero historical consumption.** Do not silently mark them guaranteed; decide migration/expiry safely with owner. Avoid hidden backfill synthetic allocations.

## 7. Instrumentation حداقلی
- Log non-sensitive `holdId`, `orderId`, `unitId`, `resourceKind`, `sourceId`, outcome `Held/Shortage/Released/Confirmed`, trace/correlation. Never log DOB, FREMEC/medical details, tokens or raw `SeatMapSelectionId`.
- Measure per batch `reservationDuration`, number of resources, lock timeout/shortage, expired holds count. No new monitoring platform.
- `InventoryAuthority.Supplier` gated until a real supplier contract and idempotency guarantee is provided؛ no generic provider Saga, new Queue, background recovery worker or Pending enum in this Pack.

## 8. ممنوعیت‌های Regression
- `SeatMap` Resolve & Shopping unchanged; no occupied seat query from Ancillary.
- `AncillaryOffers`, `Price`, `Resolve` v9 unchanged.
- `FlightFlow`, Ordering and EMD/Payment code untouched.
- Existing 5 numeric statuses preserved.
- Reuse all existing v9 tests and add specific SQL concurrency tests؛ no mock-only claim of correctness.
