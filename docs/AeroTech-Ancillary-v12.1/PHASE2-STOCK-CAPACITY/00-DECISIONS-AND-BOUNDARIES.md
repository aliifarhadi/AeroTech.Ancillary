# 00 — مرجع تصمیم و محدوده Phase 2: Ancillary Stock / Capacity

> **Project authority:** AeroTech Ancillary **v12.1 — Phase 2: Stock & Capacity**. Continuation of the approved Phase 1 v12.1; not a separately versioned specification. Phase 3 remains closed.


**Status: OWNER APPROVED — PHASE 2 AUTHORING AND CAPACITY CONFIGURATION ONLY. See documents 09-14 for implementation authority.**
**Date: 2026-10-08**
**Repository checked:** `aliifarhadi/AeroTech.Ancillary`, `k8s-stg@933b7b7a793b426dbcb6362bbf8519886edf9080` verified as PR #1 merge of Phase 1 v12.1; coding authority is only Phase 2 as narrowed in documents 09-14. Phase 1 report has open data mapping and integration issues: see 09.

## تصمیم راهبردی

**مدل عمومی `AncillaryStockPool` از Pack v12/v12.1 فقط یک پیشنهاد موقت بود و با این ADR جایگزین می‌شود.** هیچ Aggregate عمومی دارای `ScopeKind + ScopeReference + CapacityUnit + Quantity` ساخته نشود. هر **الگوی مصرف واقعی منبع** یک Aggregate مستقل و جدول‌های typed خاص خود را دارد؛ برای هر نام محصول (Meal/PETC/Seat/...) Aggregate جدا نمی‌سازیم.

سه موضوع را با هم مخلوط نکنید:

1. `AncillaryProvision` و Pricing: شرایط تجاری، مجاز بودن فروش، نسخه قیمت، مهلت خرید، Disposition؛ ATPCO Optional Services S7 فیلد Check Availability دارد، اما مرجع Inventory عملیاتی نیست.
2. Phase 2: تعریف مالكیت موجودی، منابع، محدودیت تعداد/وزن/زمان/اتاق، سهمیه فروش، و مدیریت تغییر ظرفیت با ممیزی.
3. Phase 3: Hold/Confirm/Expire/Release و پیوند به سفارش و کنترل مصرف واقعی. Phase 2 **صرفاً با تعریف ظرفیت** فروش بیش‌ازظرفیت را در Hold فعلی متوقف نمی‌کند؛ نباید availability معتبر عملیات فروش را قبل از Phase 3 ادعا کند.

## چهار حالت کنترل موجودی در Product Policy

- `Unlimited`: حقیقتاً هیچ موجودی قابل اتمامی وجود ندارد، نه «صفر» و نه «نامعلوم»؛ هیچ مصرف منابعی ندارد.
- `LocallyManaged`: دقیقاً یکی از الگوهای typed زیر، یا ترکیب کنترل‌شده FlightCount+FlightWeight، توسط Ancillary مدیریت می‌شود.
- `SupplierManaged`: مرجع معتبر Inventory تأمین‌کننده است. فقط رابط و `ProviderReference` نگه داشته می‌شود؛ بدون کپی موجودی به‌عنوان حقیقت محلی.
- `FlightFlowManaged`: فقط برای موجودی‌ای که مرجع آن FlightFlow است (به‌ویژه صندلی شماره‌دار هواپیما)، بدون Counter موازی.

`Unknown`, `NotConfigured`, `OutOfStock`, `Suspended`, `NotEligible`, `SupplierUnavailable` و `Unlimited` وضعیت‌های متمایزند. پیش‌فرض نبود Configuration هرگز Unlimited نیست.

## اصل مالکیت فیزیکی

Capacity به **منبع واقعی** تعلق دارد، نه لزوماً به `AncillaryServiceDefinition` یا `AncillaryProvision`؛ چند محصول می‌توانند از یک منبع مصرف کنند. برای مثال دو نوع غذا یک سهمیه تولید یا دو نوع حیوان یک سقف مشترک پرواز مصرف می‌کنند. `InventoryAssignment` اتصال تجاری محصول به منبع است، نه مالک موجودی فیزیکی. ظرفیت مستقل برای دو SKU با منبع مشترک، خطر oversell دارد.

`ServiceDefinitionId`/`SupplierId`/`FlightId`/`AirportId` همگی شناسه مرجع typed هستند. Policy باید روی هویت پایدار `OwnerAirlineId + ServiceDefinitionRef` در تمام Revisionها یکتا بماند و به current ServiceDefinitionId معتبر متصل شود؛ یک نسخه جدید Product نباید موجودی فیزیکی را تکثیر کند. فروش یک Ancillary در دو پرواز متفاوت، دو Scope جدا دارد؛ Provision و Pricing نباید به علت عوض‌شدن ظرفیت بازنویسی شوند. برای قرارداد چند supplier، مالکیت موجودی روی Resource/Assignment مشخص می‌شود، نه از مسیر Price یا Offer حدس زده شود.

## عدم دخالت در سرویس‌های دیگر

`AeroTech.FlightFlow`: کلاس واقعی `FlightCapacity` دارای `BaseCap`, `Held`, `Confirmed`, `AvailableCap`, `FlightSeatHold` و `SeatNumber` است؛ موجودی صندلی و Hold بلیت باید در FlightFlow باقی بماند. Ancillary فقط availability/assignment seat و پاسخ provider را در Phase 3 هماهنگ می‌کند.

`AncillaryReservationUnit` موجود `StockPoolId?` و `ProviderUnitRef?` دارد، اما اکنون `HoldAncillaryServicesService` هیچ stock را رزرو نمی‌کند و از `AncillaryHoldChecks` صرفاً اعتبارسنجی تجاری/مالکیت می‌گیرد. `StockPoolId` به معنی الزام ایجاد StockPool عمومی نیست؛ تغییر/مهاجرت فیلد legacy فقط در Phase 3 و با قرارداد سازگاری.

`QuantityRule.MinQuantity/MaxQuantity` و `PricingUnit` با `CapacityUnit` متفاوتند. ظرفیت تجاری وزن، Payload قابل‌پرواز/تأیید Load Control نیست. `MustCheckAvailability` فعلی یک Boolean ضعیف است؛ تبدیل آن به Policy اجرایی مستلزم تصمیم صریح و Migration سازگار است.

## حداقل قرارداد مدل‌های فیزیکی

- `FlightCountInventory`: سهمیه شمارشی برای FlightId و یک Resource/QuotaIdentity.
- `FlightWeightInventory`: سهمیه **تجاری** کیلوگرم برای FlightId و WeightQuotaIdentity.
- `AirportSlotInventory`: ظرفیت در AirportId + FacilityId + UTC slot.
- `DailyServiceInventory`: ظرفیت شمارشی برای ResourceId + ServiceDate.
- `RoomNightInventory`: ظرفیت به‌ازای PropertyId + RoomTypeId + NightDate، مستقل از RatePlan.
- `AssignedAssetInventory`: منبع دارای هویت فیزیکی + تقویم اشغال؛ فقط با نیاز احرازشده پیاده شود.
- `PassengerUsageLimit`: سیاست سهمیه مصرف هر مسافر، نه موجودی فیزیکی؛ به Policy تعلق دارد. نگهداری تراکنش‌های مصرف آن، بخشی از Phase 3 است.

## دستور خروج

Owner approved the 12 decisions in 07. Phase 2 implementation is authorized ONLY to the limits of the numbered implementation prompt 13; reservation, hold, confirm, release, expire, and operational stock debit remain Phase 3 and forbidden here.
