# 03 — کاتالوگ Aggregate / Entity / ValueObject پیشنهادی Phase 2

> **Project authority:** AeroTech Ancillary **v12.1 — Phase 2: Stock & Capacity**. Continuation of the approved Phase 1 v12.1; not a separately versioned specification. Phase 3 remains closed.


توضیح: کدهای انگلیسی نام پیشنهادی .NET هستند؛ این‌ها **API ATPCO نیستند**. پس از تأیید Owner و Audit کد v12.1 باید اسم نهایی با استاندارد پروژه freeze شود. هیچ `UniversalStockPool`، EAV و JSON scope ساخته نشود.

## A. `AncillaryInventoryPolicy` [Aggregate Root | 1:1 ServiceDefinition identity]

این یک **registry کوچک مالکیت/استراتژی** است، نه Aggregate نگهداری ظرفیت. هدف: بدون بازکردن Aggregate فاز اول، برای هر محصول صریحاً تعیین شود کنترل با چه کسی و کدام مدل است.

| Field | Type | قانون |
|---|---|---|
| `Id` | long | PK |
| `ServiceDefinitionId` | long | شناسه آخرین Revision تجاری معتبر، برای اعتبارسنجی Cross-aggregate |
| `ServiceDefinitionRef` | string | هویت پایدار محصول در همه Revisionها؛ یکتایی Policy فعال بر `(OwnerAirlineId,ServiceDefinitionRef)`، نه صرفاً Id نسخه |
| `OwnerAirlineId` | int | جداسازی tenant |
| `Authority` | enum `Unlimited=1,Local=2,Supplier=3,FlightFlow=4` | required، نه nullable/heuristic |
| `LocalPattern` | enum nullable | فقط Authority=Local: FlightCount,FlightWeight,FlightCountPlusWeight,AirportSlot,DailyCount,RoomNight,AssignedAsset |
| `ProviderKey` | string? | برای Supplier/FlightFlow، از providers معتبر؛ بدون فرض default |
| `Status` | Draft/Active/Suspended/Retired | activation مستقلاً کنترل شده |
| `Version` | long | optimistic concurrency |
| `CreatedAt`,`UpdatedAt`,`ActivatedAt` | DateTimeOffset | audit |
| `Consumption` | typed VO(s) below | فقط مطابق pattern و max 2 منبع flight |
| `PassengerUsageLimits` | IReadOnlyList<PassengerUsageLimit> | children؛ اختیاری، حداکثر یک قاعده برای scope یکسان |

**Supporting reference-data entity (NOT a stock pool):** `InventoryResourceDefinition {Id:long, OwnerAirlineId:int, ResourceKind:enum, ResourceCode:string, ProviderResourceRef?:string, FacilityId?:long, Status:enum}`، فقط در صورتی که از ReferenceData یا provider ثبت‌شده چنین هویت معتبری در دسترس نباشد. این entity صرفاً یک registry کم‌حجم برای منابع مشترک است و **هیچ Total/Held/Confirmed/Counter** ندارد. ساخت Aggregate مستقل کاتالوگ منبع تنها با شواهد نیاز approval شود؛ در غیر این صورت existing master resource ID استفاده گردد.

**Child Entity:** `PassengerUsageLimit {Id,InventoryPolicyId,LimitScope:PerOrder|PerFlightOccurrence|PerServiceDate,MaxUnits:int>0,CountingFamilyCode:string}`. `CountingFamilyCode` یک کلید **ثبت‌شده** برای جلوگیری از خرید دوباره بین Productهای معادل است؛ نباید آزاد/حدسی باشد. برای قوانین Catalog-only با `MaxQuantity` مشابه در Provision، یک Rule مالک مشخص شود تا دو منبع معنی ایجاد نشود.

**VOهای typed (نه جدول EAV):** `FlightCountConsumption {ResourceId,CountPerUnit:int>0}`, `FlightWeightConsumption {ResourceId,KgPerUnit:decimal(18,3)>0 OR WeightFromAcceptedQuantity}`, `AirportSlotConsumption {FacilityId,OccupancyDurationMinutes:int>0}`, `DailyCountConsumption {ResourceId,CountPerUnit}`, `RoomNightConsumption {PropertyId,RoomTypeId,RoomsPerBooking}`, `AssignedAssetConsumption {AssetKind,MinDurationMinutes?}`. شناسه resource فقط به منابع از پیش ثبت‌شده و مالک معتبر وصل می‌شود. اگر وزن انتخابی بر اساس kg است، آن عدد باید از **مقدار پذیرفته‌شده سرویس** استخراج شود نه از نام محصول. «هم 1 piece و هم 12 kg» ترکیب مجاز محدود به دو VO مشخص است، نه DSL عمومی.

## B. `FlightCountInventory` [Aggregate Root | finite count]

`Id:long`, `OwnerAirlineId:int`, `FlightId:long`, `ResourceId:long` (منبع شمارشی تایپ‌شده، ثبت‌شده و قابل اشتراک بین چند محصول)، `CountUnit:Person|Piece|Item|AnimalCarrier|Equipment` (closed enum per verified use), `TotalCapacity:int>=0`, `ClosedForSale:bool`, `Status:Draft|Active|Suspended|Retired`, `Version:long`, `CreatedAt`,`UpdatedAt`.

**Child** `FlightCountAdjustment {Id,FlightCountInventoryId,PreviousTotal,NewTotal,ReasonCode,ActorId,At}` immutable.

Natural unique: `(OwnerAirlineId,FlightId,ResourceId)` برای active allocation. ظرفیتی که از provider به‌صورت guaranteed allotment دریافت شود Local است؛ وقتی supplier مرجع است SupplierManaged بماند. `TotalCapacity` نه ظرفیت تمام هواپیما و نه seat RBD است.

## C. `FlightWeightInventory` [Aggregate Root | finite commercial weight]

`Id`, `OwnerAirlineId`, `FlightId`, `WeightResourceId`, `CapacityKg:decimal(18,3)>=0`, `ClosedForSale`, `Status`, `Version`, timestamps.

**Child** `FlightWeightAdjustment {Id,FlightWeightInventoryId,PreviousCapacityKg,NewCapacityKg,ReasonCode,ActorId,At}` immutable.

Natural unique `(OwnerAirlineId,FlightId,WeightResourceId)`. واحد ثابت کیلوگرم؛ تبدیل سایر واحدها فقط در مرز ورودی با precision مشخص. این **سهمیه فروش تجاری** است، نه تأیید load/balance/safety یا weight & balance؛ اگر carrier source دیگر دارد integration برای فاز بعد لازم است.

## D. `AirportSlotInventory` [Aggregate Root | facility/time-slot]

`Id`, `OwnerAirlineId`, `AirportId:int`, `FacilityId:long`, `StartUtc:DateTimeOffset`, `EndUtc:DateTimeOffset`, `CapacityPersons:int>=0`, `ClosedForSale`, `Status`, `Version`, timestamps.

**Child** `AirportSlotAdjustment {Id,AirportSlotInventoryId,PreviousCapacity,NewCapacity,ReasonCode,ActorId,At}`.

Key unique `(FacilityId,StartUtc,EndUtc)`؛ علاوه‌بر unique exact-key، **هر هم‌پوشانی زمانی در همان Facility** باید با transaction/guard قفل‌شونده رد شود (unique equality به تنهایی کافی نیست). `Facility` دارای IANA timezone و scope مکان است (reference external/ReferenceData، نه Aggregate جدید speculative). محصول Lounge با dwell time دو ساعت باید تمام slotهایی که با حضورش تلاقی دارند مصرف کند؛ اگر زمان ورود نامعلوم است provider-managed یا unavailable/unknown برگردد؛ یک slot ورود به‌تنهایی Occupancy را نشان نمی‌دهد.

## E. `DailyServiceInventory` [Aggregate Root | date-bound finite count]

`Id`, `OwnerAirlineId`, `ResourceId:long`, `ServiceDate:DateOnly`, `Capacity:int>=0`, `CountUnit:Person|Item|Session`, `ClosedForSale`, `Status`, `Version`, timestamps.

**Child** `DailyCapacityAdjustment {Id,DailyServiceInventoryId,PreviousCapacity,NewCapacity,ReasonCode,ActorId,At}`.

Unique `(OwnerAirlineId,ResourceId,ServiceDate)`. تاریخ باید در timezone تعریف‌شده resource interpretation شود. این مدل برای capacity روزانه خدمات است، نه هتل؛ موجودی شبانه هتل منطق distinct دارد.

## F. `RoomNightInventory` [Aggregate Root | independently sellable room-nights]

`Id`, `OwnerAirlineId`, `PropertyId:long`, `RoomTypeId:long`, `NightDate:DateOnly`, `SellableRooms:int>=0`, `ClosedForSale`, `Status`, `Version`, timestamps.

**Child** `RoomNightAdjustment {Id,RoomNightInventoryId,PreviousSellable,NewSellable,ReasonCode,ActorId,At}`.

Unique `(OwnerAirlineId,PropertyId,RoomTypeId,NightDate)`. نرخ، Price و RoomRatePlan هیچ‌کدام کلید مستقل capacity نیستند. Multi-night stay همه شب‌های `[CheckIn,CheckOut)` را مصرف خواهد کرد. Local فقط وقتی operator contract حق فروش سهمیه‌ای را تضمین کند؛ در غیر این صورت SupplierManaged. Occupancy مسافر/نفر در `Provision` یا contract property/rate plan است، نه شمارنده اتاق.

## G. `AssignedAssetInventory` [Aggregate Root | physical named exclusive asset, OPTIONAL]

تنها در صورت سناریوی قطعی مالکیت خودروی شماره‌دار/تجهیز قابل قرض‌دهی ایجاد شود؛ نه برای صندلی هواپیما/FlightFlow.

`Id`, `OwnerAirlineId`, `AssetId:long`, `AssetKind:Vehicle|Equipment|Locker|FacilityRoom`, `LocationId`, `AvailabilityStatus:Active|OutOfService|Retired`, `Version`, timestamps.

**Child** `AssetBlackoutPeriod {Id,AssetInventoryId,FromUtc,ToUtc,Reason}` (مانع فروش/سرویس)؛ **Reservations/occupancy records فعلاً ساخته نمی‌شوند** و در Phase 3 پس از قرارداد Hold افزوده می‌شوند. Natural unique `(OwnerAirlineId,AssetId)`. نمی‌توان برای یک Asset دو interval overlapping فعال داشت.

## Type definitions and field consistency

- IDs: `long` برای منابع داخلی مگر Type اصلی reference چیز دیگری باشد؛ `FlightId` از FlightFlow؛ `AirportId` همان ReferenceData int؛ `ProviderKey` مطابق Fulfillment؛ `RoomTypeId` از supplier/Hotel catalog مرجع، بدون جعل shared ID.
- Quantities: `int` برای person/piece/room؛ `decimal(18,3)` برای kg. **نباید** QuantityUnit، PricingUnit و CapacityUnit با هم یک enum شوند.
- Version: RowVersion/optimistic concurrency مطابق EF/framework موجود؛ هر Configuration، adjustment و publish با actor/correlation auditable.
- Stock availability runtime Phase 3: `Free = Total - Held - Confirmed`; Phase 2 هنوز ledger Hold/Confirmed را نساخته و نباید Free واقعی را به عنوان guaranteed منتشر کند.
- Availability status: `Configured`, `NotConfigured`, `Delegated`, `ClosedForSale`, `Unknown`, `FiniteAvailableSnapshot` با timestamp/authority؛ «SoldOut» فقط وقتی real consumption evidence معتبر است.

## NOT aggregates

`Unlimited` Counter ندارد؛ `SupplierManaged` Counter ندارد؛ `FlightFlowManaged` Counter ندارد؛ `PassengerUsageLimit` Aggregate مستقل نیست؛ `Resource/Product binding` VO/Entity زیر policy یا inventory است؛ `StockAdjustment` همیشه child immutable است، نه یک AR دیگر. Release/Expire/Hold entity در Phase 2 ممنوع.
