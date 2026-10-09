# 07 — تصمیم‌های مصوب Owner برای اجرای Phase 2

> **Project authority:** AeroTech Ancillary **v12.1 — Phase 2: Stock & Capacity**. Continuation of the approved Phase 1 v12.1; not a separately versioned specification. Phase 3 remains closed.


## تصمیم‌های مورد تأیید

- [x] **D1:** مدل Generic `AncillaryStockPool` حذف شود؛ الگوهای typed منابع مستقل جایگزین شوند. «هم‌نوعی رفتار» مبنای reuse است، نه یک Aggregate برای هر SKU.
- [x] **D2:** یک `AncillaryInventoryPolicy` کوچک فقط برای انتخاب authority/pattern/bindings per Definition identity (cross-version) تعریف شود؛ Counter ندارد. منابع ResourceId باید از مرجع معتبر تأمین شوند، registry مستقل فقط در صورت ضرورت اثبات‌شده. وضعیت نامعلوم هرگز Unlimited محسوب نشود.
- [x] **D3:** `FlightCountInventory` برای PETC/Meals/UMNR/Item quotas و ظرفیت مشترک چند Product تأیید شود.
- [x] **D4:** `FlightWeightInventory` فقط برای **Commercial Bag/Sports Quota** و نه aircraft load & balance تأیید شود؛ نمونه Count+Weight دارای binding ترکیبی مشخص باشد.
- [x] **D5:** `AirportSlotInventory` بر اساس Facility و slot اشغال، نه AirportId تنها، تأیید شود. محل/تایم‌زون منبع اصلی خارج از این aggregate تأیید شود.
- [x] **D6:** `DailyServiceInventory` برای date-only capacity و `RoomNightInventory` برای hotel room-type per-night **مستقل** باشند؛ hotel فقط وقتی local allotment واقعی دارد.
- [x] **D7:** `AssignedAssetInventory` فقط طراحی شود و ساخت آن به سناریوی تأییدشده خودرو/تجهیزات محدود موکول گردد، نه آنکه پیش‌فرض پیاده‌سازی شود.
- [x] **D8:** `PassengerUsageLimit` child policy/usage restriction باشد و Inventory فیزیکی جدا محسوب نشود؛ Entitlement برای مسافر مشخص تنها پس از قرارداد source قابل توسعه است.
- [x] **D9:** `Unlimited`, `SupplierManaged`, `FlightFlowManaged` هیچ Counter محلی authoritative ندارند؛ Seat occupancy در FlightFlow بماند.
- [x] **D10:** Phase2 فقط publish/edit/adjust/configured snapshot است؛ Hold/Confirm/Release/Expire در Phase3 و بدون «ادعای oversell protection» قبل از آن.
- [x] **D11:** Active Product Policy باید consumer unit mapping verified داشته باشد؛ از نام سرویس، قیمت یا SubCode مقدار kg/item استنتاج نکنیم؛ Stock Shared با resource واقعی مدل شود.
- [x] **D12:** ابتدا SHA واقعی اجرای Phase1 v12.1 audit/closure شود؛ سپس Agent Prompt Phase2 براساس سورس push شده ساخته شود.

## برنامه پیاده‌سازی داخل **همان Phase 2** (نه فاز جدید)

**حداقل مورد استفاده بالفعل:**
1. policy + Unlimited/External/FlightFlow delegation + supplier/source ownership.
2. FlightCount، FlightWeight، AirportSlot و PassengerUsageLimit (برای درخواست‌های واقعی غذا/بار/PETC/لانژ).
3. DailyService و RoomNight صرفاً برای منبع locally managed واقعی که تیم عرضه/هتل تأیید کرده؛ طراحی و scenario tests در همین Pack قطعی‌اند، اما schema/concrete endpoints بدون provider contract نباید اختراع شود.
4. AssignedAsset فقط اگر مشخص شده خودمان AssetId و کیلومتر/زمان اشغال را کنترل می‌کنیم.

**ملاک خروج:** تفکیک مدل‌ها، ممیزی، concurrency برای authoring، migrations safe، جلوگیری از duplication منابع، شواهد دقیق regression؛ سپس Owner مرز Phase3 را باز می‌کند.

## تصمیم‌های تجاری/Provider لازم پیش از کد

1. برای هر یک از 31 ServiceDefinition موجود: Authority و pattern + resource owner با شواهد supplier/operator.
2. FlightFlow seat map و premium seat assignment واقعاً به چه endpoint/contract متصل خواهند شد؟ بدون فرض اینکه `FlightCapacity` reservation ticket همان paid seat assignment باشد.
3. آیا داده Flight BaggageQuota از LoadControl/Handling وارد می‌شود یا manually managed commercial allowance است؟
4. Airport lounge بازه occupancy دارد یا فقط Entry slot و provider own capacity؟
5. هتل/خودرو جزو Ancillary سرویس فعلی است یا supply team جدا و فقط adapter می‌خواهد؟
6. آیا برای یک مسافر هویت پایدار در چند سفارش وجود دارد؟ اگر نه per-passenger per-flight global limit فاز3 به تصمیم هویتی نیاز دارد.

## ممنوعیت‌های صریح

**Owner approved all D1-D12.** Implementation is now authorized by 13-PHASE2-CODING-AGENT-PROMPT.md within Phase 2 only. No reservation code or other-repository edits. Agent نباید در Phase2 رزرو Existing را بازنویسی کند، serviceهای خارجی را دست بزند یا Modelهای فرضی Room/Vehicle برای supplier که API ندارد بسازد.
