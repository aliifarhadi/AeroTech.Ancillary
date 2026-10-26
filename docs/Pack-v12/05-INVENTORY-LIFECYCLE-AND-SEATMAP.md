# 05 — Local Inventory، Lock، Lifecycle و SeatMap

## مسیر Create در HEAD v11
1. REST strict binder/validator; family variants flatten to `HoldSelection` via `AncillaryHoldFamilies.Selections`.
2. `HoldSelectionVerifier` با Readerهای محلی Active Provision و Active Definition، Family/Variant و Holdability را بررسی می‌کند؛ `TypedSelectionForm.Fields` optionهای پشتیبانی‌شده را می‌سنجد و `WithFixedChoices` داده ثابت را تکمیل می‌کند. **هیچ Lookup به Order، Price Snapshot یا FlightFlow انجام نمی‌شود.**
3. `HoldAncillaryServicesService.HoldAsync` ابتدا Idempotency replay را از خود Reservation بررسی می‌کند؛ برای Create جدید Source/Policy محلی تخصیص داده می‌شود. `IAncillaryHoldLock` برای OrderId correlation lock می‌گیرد، ولی Order membership را بررسی نمی‌کند.
4. `AncillaryHoldAllocator` از Policy/Source و Quantity/FlightIds/Typed Airport selection، allocations را تعیین می‌کند.
5. `InventoryLedger.OpenAsync(sources)` تحت SQL transaction، `sp_getapplock` source-key ثابت و sorted، balances را از active usage می‌خواند. عدم ظرفیت → rollback کل Root، تمام Unitها و Allocations.
6. `AncillaryReservation.Hold` + SaveChanges + ledger.Commit در همان transaction؛ Receipt کوتاه.

## Inventory Policy — بدون leakage در API
| نوع | Input مجاز | محاسبه usage | Gate |
|---|---|---|---|
| Unlimited | Quantity/Selected مطابق Product | هیچ Allocation | No false capacity |
| FlightCount | Real FlightIds، Quantity | Count units × applicable policy per flight | Distinct source/lock |
| FixedWeight/Count+Weight | FlightIds، Quantity | ثابت از Definition/Policy × Quantity | AcceptedWeightKg/PerKilogram هنوز GATED |
| AirportSlot | `timeWithOffset`/`guestCount` از `selected.options.airportService` | Policy `OccupancyMinutes`, `PeoplePerAcceptedUnit` و Slot source | بدون Timestamp/Facility معتبر fail-closed |
| Supplier | فقط adapter قرارداد واقعی | رزرو بیرونی مستقل با ref واقعی | بدون adapter = GATED، fake Held ممنوع |
| SeatMap Commercial | SeatMapSelectionId + verifier | **zero Ancillary physical inventory allocation** | FlightFlow only by Ordering |

**Invariants:** TotalCapacity/CapacityKg روی Hold کاهش نمی‌یابد؛ active usage حاصل از ledger. اگر Multi-resource است ترتیب lock ثابت باشد؛ خطای یکی همه تخصیص‌ها را rollback کند. Retire منبع با active usage باید تحت همان lock رد شود. v11 بر اساس report guard را به `RetireFlightCountInventoryService`, `RetireFlightWeightInventoryService`, `RetireAirportSlotInventoryService` افزوده؛ دوباره‌کاری نکن، مستقل صحتش را تست کن.

## Lifecycle Matrix
| Before | Action | After | Constraint |
|---|---|---|---|
| no Hold | Create | Held | future expiry، nonempty units، all sources available |
| Held live | AppendSeats | Held + Seat Unit | header exact، root same، no physical Seat allocation |
| Held live | Partial Release | some Units Released، Root Held/Released | only held Units؛ replay released idempotent |
| Held live | Confirm whole | active Held Units Confirmed | Released units unchanged؛ no EMD |
| Confirmed | Cancel selected/all | selected Confirmed → Cancelled | reason stored و replay matching |
| Held expired | Confirm | error `16407` | Logical usage free even before poller |
| Held expired | poller | Expired persisted | saved Unit selected remains |
| Confirmed | Release | reject | use Cancel instead |
| any final state | GET | historical details | no Definition live query |

Partial operations must reject foreign/invalid IDs *before mutations*, all-or-nothing. Correct idempotency key with changed payload rejects 409; same request returns same HoldId/UnitIds even if Definition retired after valid Hold.

## P0: Empty Query روی `DELETE` نباید Release-All شود
**سورس مستقیم:** `ServiceController.Release(long holdId, [FromQuery] long[]? unitIds, ...) => ServiceReleaseAncillaryHoldCommand(holdId, unitIds is { Length: >0 } ? unitIds : null)` و Domain `AncillaryReservation.Release(null, now)` مسیر Release ALL است. ورودی Query حاضر ولی Empty عمداً All نیست.

**حداقل اصلاح مورد انتظار:**
1. در Controller یا Binder موجود presence خام Query Key را از value مستقل تشخیص بده؛ اگر `unitIds` وجود دارد ولی هیچ positive long valid عضو ندارد → 400؛ هرگز `[]` را به null تبدیل نکن.
2. اگر `unitIds` واقعاً absent باشد، `null` را برای Release All بفرست؛ اگر حاضر و nonempty، خود آرایه را unchanged بفرست؛ duplicate/zero/foreign رد شود.
3. تست HTTP واقعی برای absent, `?unitIds=`, repeated blank, `?unitIds=9101&unitIds=9102`, `?unitIds=0`, non-numeric, duplicate و Root SQL state before/after. هیچ Route اضافه نکن.
4. `Cancel` با `unitIds:[]` نیز fail-closed بماند؛ Root آن را reject می‌کند. در HTTP response code مطابق existing convention، نه invented.

## SeatMap boundary
`POST /Service/v1/Ancillaries/SeatMaps/Holds`: `holdId` اختیاری؛ `OrderCommercialVersion` و Passenger/Flight/Fare facts و `seatMapSelectionId` در Seat verifier موجود. نتیجه Commercial Unit با `SeatNumber` Persist، بدون `StockPoolId` و Ancillary Allocation. **Ordering مسئول حمل SeatNumber به FlightFlow** است؛ Ancillary هیچ Seat Lock فیزیکی ندارد. Release/Cancel Commercial Reservation معادل آزادکردن صندلی فیزیکی نیست و نباید چنان ادعایی کند. قرارداد `SeatMaps/Resolve` دست‌نخورده بماند.

## GATEهای واقعی
Local FlightCount/Weight/AirportSlot SQL concurrency در گزارش Gate5 v11 موفق است، ولی Live Dev به‌علت نبود Resource/Facility Reference **GATED**؛ این عبارت نباید به «Live Verified» تبدیل شود. Expiry Poller و mixed HTTP live بر اساس گزارش ایجنت دارای Evidence هستند؛ اجرای مستقل در این Pack انجام نشده است.
