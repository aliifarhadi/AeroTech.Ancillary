# 05 — Backoffice، Query، Portها و خروجی قرارداد Phase 3

> **Project authority:** AeroTech Ancillary **v12.1 — Phase 2: Stock & Capacity**. Continuation of the approved Phase 1 v12.1; not a separately versioned specification. Phase 3 remains closed.


## طراحی API برای Phase 2: فقط مدیریت ظرفیت و استعلام وضعیت مدیریت

مسیرهای پیشنهادی زیر illustrative هستند؛ نهایی‌سازی نام Path/DTO بعد از ممیزی قواعد Authorization و conventional namespace انجام شود. **نباید** نسخه `Hold/Confirm/Release` جدید در فاز 2 بسازیم.

| عملیات | پیشنهاد endpoint | نکته |
|---|---|---|
| تعریف/ویرایش Policy Draft | `POST/PUT Backoffice/v1/AncillaryInventoryPolicies` | یک Policy فعال در هر ServiceDefinition؛ mode/config audited |
| Active/Suspend/Retire Policy | `.../{id}/Activate|Suspend|Retire` | فعال‌سازی مستلزم resource bindings معتبر |
| FlightCount | `POST/GET Backoffice/v1/FlightCountInventories`; `.../{id}/Adjust` | owner/flight/resource typed |
| FlightWeight | `POST/GET Backoffice/v1/FlightWeightInventories`; `.../{id}/Adjust` | kg decimal(18,3) |
| AirportSlot | `POST/GET Backoffice/v1/AirportSlotInventories`; `.../{id}/Adjust` | facility/timezone canonical intervals |
| DailyService | `POST/GET Backoffice/v1/DailyServiceInventories`; `.../{id}/Adjust` | batch interval authoring -> independent date resources |
| RoomNight | `POST/GET Backoffice/v1/RoomNightInventories`; `.../{id}/Adjust` | batch date ranges into nightly buckets |
| AssignedAsset | `POST/GET .../AssignedAssets`; `.../{id}/Blackouts` | only after proven scenario |
| Snapshot (not guarantee) | `GET Backoffice/v1/AncillaryInventorySnapshots` | mode, configured quantity, authority, observedAt, state |

For supplier/FlightFlow policy، `GET` یک snapshot/ref از Provider نشان می‌دهد **بدون** حفظ counters موهوم. Operations adjust برای external mode به‌صراحت 409/unsupported می‌دهد. در HTTP backoffice نقش owner/tenant کنترل شود و actor/audit required. `PUT` status و value با `expectedVersion`؛ Duplicate natural key به 409 ترجمه شود.

## Portهای Application / Integration

```text
IInventoryPolicyRepository
IFlightCountInventoryRepository
IFlightWeightInventoryRepository
IAirportSlotInventoryRepository
IDailyServiceInventoryRepository
IRoomNightInventoryRepository
IAssignedAssetInventoryRepository (deferred)
IInventorySourceAvailabilityReader     // only read, no reserve
IFlightFlowSeatAvailabilityReader     // delegated source, not database sync
ISupplierAvailabilityReader           // optional provider adapter, read-only
```

یک `IInventoryAvailabilityReader` facade قابل قبول است **اگر** فقط dispatch به typed model/read source کند و باعث `UniversalStockPool`/EAV نشود. هر write Repository مربوط به Aggregate واقعی خودش است.

## Query contract: `InventoryAvailabilitySnapshot`

Snapshot read-only، نه Aggregate، به شکل زیر:

`ServiceDefinitionId:long`, `ProvisionId:long?`, `Authority:Unlimited|Local|Supplier|FlightFlow`, `Model:enum?`, `ResourceLocator:typed DTO union`, `ObservedAt:DateTimeOffset`, `StaleAfter:DateTimeOffset?`, `State:NotConfigured|Configured|Unlimited|Closed|Unavailable|Unknown|SupplierCheckRequired|Delegated`, `ConfiguredCapacity:{int|decimal}?`, `AvailableQuantity:{int|decimal}?`, `CapacityUnit:enum?`, `Guarantee:bool=false` در Phase2.

**Readiness:** Snapshot برای finite بدون real consumption ledger باید مقدار `AvailableQuantity` را nullable/Unknown بگذارد یا به‌وضوح `ConfiguredRemainingNotEnforced` نام‌گذاری کند. هیچ client آن را با available-for-order اشتباه نگیرد.

## Stock adjustments and audit

`AdjustCapacity` فقط absolute new total + expected version (برای audit ثبت Previous و New)، دلیل و actor. هر adjustment یک child immutable در همان AR است و در یک transaction ذخیره می‌شود. ظرفیت تاریخی با replacement مستقیم پاک نشود. برای 1000 روز `BatchUpsert` یک عملیات atomically consistent است و تفکیک per date حفظ می‌شود؛ در دیتابیس atomicity / partial batch failure صریح گردد.

## handoff رسمی به Phase 3 — نه پیاده‌سازی در Phase 2

Phase 3 باید قرارداد allocation را برای هر مدل جداگانه بررسی کند:

- `FlightCount`: hold/release/confirm of integer units keyed FlightId/ResourceId.
- `FlightWeight`: hold/release/confirm decimal kg، با پذیرش weight evidence.
- `AirportSlot`: رزرو تمام slotهای اشغال شده با lock اتمیک؛ arrival + duration.
- `DailyService`: per-date quantity reservation.
- `RoomNight`: all nights atomic یا Saga supplier compensations if external؛ چند روز هم‌زمان.
- `AssignedAsset`: interval exclusivity with overlapping-booking check.
- `PassengerUsageLimit`: usage-counter cross-order با شناسه مسافر قابل اتکا و idempotent.
- `Supplier`/`FlightFlow`: adapter حق reserve/confirm/release، TTL، retry، partials، cancellation را از قرارداد واقعی بگیرد؛ read snapshot guarantee نیست.

Phase 3 تازه تصمیم می‌گیرد `AncillaryReservationUnit.StockPoolId?` چگونه migrates/aligns to typed reservation references. امروز دست زدن به M1/Reservation موجود ممنوع است.

## انتقال از v12.1

1. صرفاً پس از Owner audit، آخرین SHA Phase1 را مبنا بگیرید. Verified PR #1 merged as `k8s-stg@933b7b7a793b426dbcb6362bbf8519886edf9080`; use this exact SHA as the baseline. The report still contains explicit open points; do not mark business cutover as closed.
2. نسبت `MustCheckAvailability` فعلی Provision با Policy جدید explicit map شود. در تضادها error و migration exception؛ Boolean را بی‌سر و صدا به Unlimited/Supplier translate نکنید.
3. برای 31 legacy ServiceDefinitions نگاشت Authority/LocalPattern منوط به actual supplier/resource evidence است. حدس از SubCode ممنوع؛ `Unknown/NotConfigured` باقی بماند تا مالك تصمیم بگیرد.
4. اگر قبلاً Hold/Confirmed عملیاتی وجود دارد، حتی اگر stock history نداشته باشد، Phase2 نمی‌تواند آن را صفر فرض کند؛ فرایند cutover قبل از Phase3 اجباری است.
5. ذخیره و استعلام در CommandDb/QueryDb با timestamps/versions و synchronization مطابق framework، بدون تغییر interface سایر سرویس‌ها.
