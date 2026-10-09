# 04 — قواعد دقیق دامنه، تعارض‌ها و مثال‌های قبولی

> **Project authority:** AeroTech Ancillary **v12.1 — Phase 2: Stock & Capacity**. Continuation of the approved Phase 1 v12.1; not a separately versioned specification. Phase 3 remains closed.


## قاعده 1: Inventory ≠ Eligibility ≠ Supplier Acceptance

در Shopping ابتدا eligibility از Provision، سپس binding و ظرفیت مرجع (در صورت وجود) و سپس pricing معتبر بررسی می‌شود؛ ترتیب اجرای واقعی API نهایی بعد از قراردادهای AirAvail/Ordering تعیین می‌شود. `NotAvailable` یک تصمیم تجاری است؛ `Capacity=0` کمبود منبع واقعی؛ `ClosedForSale` بستن دستی منبع با حفظ مقدار؛ `SupplierUnavailable` یعنی authority در دسترس نیست؛ `Unknown` هرگز equal Unlimited نیست.

## قاعده 2: کلید منابع و Scope

تمامی keys از type و occurrence واقعی استفاده می‌کنند:
- Flight count `(OwnerAirlineId,FlightId,ResourceId)`؛ Flight Number در تاریخ‌های متفاوت یکی نیست.
- Flight weight `(OwnerAirlineId,FlightId,WeightResourceId)`.
- Airport slot `(FacilityId,StartUtc,EndUtc)`، با FK به Airport.
- Daily count `(OwnerAirlineId,ResourceId,ServiceDate)`.
- Hotel night `(OwnerAirlineId,PropertyId,RoomTypeId,NightDate)`.
- Asset `(OwnerAirlineId,AssetId)`.

`ServiceDefinitionId` در binding است، نه لزوماً unique stock key. اگر محصولات مختلف یک pool مشترک دارند، یک منبع فیزیکی برمی‌گزینند؛ چند بار کپی ظرفیت ممنوع.

## قاعده 3: مصرف و واحد

- مقدار فروش `Quantity=2` همراه `PricingUnit=PerItem` لزوماً دو kg نیست؛ تعیین مصرف از `InventoryConsumption` typed الزامی.
- وزن تجاری: `KgRequired = AcceptedQuantity × FixedKgPerUnit` یا `AcceptedWeightKg`، بسته به قرارداد. عدد همیشه decimal(18,3) و مثبت. هیچ استخراج از ServiceDefinitionRef مثل `XBAG_WEIGHT_5KG` مجاز نیست.
- ظرفیت count: `UnitsRequired = AcceptedQuantity × CountPerUnit`. اگر یک نفر 2 چمدان خریده، 2 slot/2 piece می‌گیرد.
- یک خرید می‌تواند **دو منبع مستقل** مصرف کند: Oversize piece + WeightKg. این ترکیب در Policy از پیش شناخته‌شده است؛ نه expression DSL.
- Group/Family policy از ProductVariant/PriceVariant مستقل است؛ دو RatePlan هتل ظرفیت شبانه یکسان دارند.

## قاعده 4: منبع مشترک

مثال A: Flight F100, Resource `CabinPetCarrier`, Total=2. Product PETC_A و PETC_B به Resource یکسان وصل هستند. پس از دو Hold آینده مجموع، خرید سوم رد می‌شود (در Phase 3). اگر برای هر Product دو ردیف capacity جدا ساخته شود باید reject شود یا هویت مشترک resource را استفاده کنند.

مثال B: `CateringKitchen` با 80 وعده کلی و quota جدا از نوع `Kosher` با 10 مورد. برای هم‌زمان کنترل هر دو، نیاز به ترکیب دو Count pool است؛ **در نسخه حداقلی Phase 2 فقط یک physical count pool در هر product مجاز است**. اگر هر دو سقف الزام قطعی دارند، این سناریو در Exception Register ثبت و الگوی two-count در بازنگری Owner باز می‌شود؛ آن را با تعبیر غلط Count+Weight حل نکنید.

## قاعده 5: Passenger rule / Scope

مثال: One meal per passenger per Flight. `PassengerUsageLimit(PerFlightOccurrence,1)` همراه `FlightCountInventory` و PTC Eligibility. بر اساس `TravellerId` پایدار از Ordering و `FlightId` یکتا، نه نام/سن مسافر یا تعداد سفارش. دو Order مختلف برای همان Traveller نباید قانون را دور بزنند. اگر شناسه‌های Traveller بين دو سفارش stable نیستند، کنترل cross-order بدون Customer identity reliable نیست؛ فاز 3 باید تکلیف آن را روشن کند؛ `PerOrder` را نمی‌توان به جای PerPassenger global استفاده کرد.

اگر منابع واقعاً به Passenger مشخصی پیش‌فروش شده‌اند (entitlement/loyalty) نیاز به مرجع Entitlement/StoredValue است؛ این یک Inventory فیزیکی عمومی نیست و بدون قرارداد آن Ledger نباید ساخته شود.

## قاعده 6: زمان

- فروش Slot 11:00 تا 13:00: اگر هر 30 دقیقه 20 نفر ظرفیت دارد، مراجعه‌ای 90 دقیقه‌ای باید هر 3 slot مناسب را همزمان اشغال کند. `Availability` نباید فقط Slot ورود را ببیند.
- UTC instants و منطقه زمانی Facility/Property لازم است. slotهایی با هم‌پوشانی در یک facility و resource مشابه ممنوع؛ برای race ثبت slot باید transaction/lock روی Facility باشد چون unique exact-key مانع دو interval overlapping متفاوت نیست؛ adjacency `[11,12)` و `[12,13)` مجاز.
- Hotel CheckIn=10 Oct, CheckOut=12 Oct => شب 10 و 11، نه شب 12. اگر یکی از دو شب sold out باشد booking کل stay ناموجود است؛ average/total of dates نامعتبر.
- Daily Capacity=10 برای تاریخ 11 Oct با 10 برای 12 Oct مستقل است. Backoffice میتواند یک بازه 1000 روزه را در یک request بدهد اما ردیف‌های 1000 روزانه منطقی‌اند چون «هر روز ظرفیت قابل مصرف جدا» است؛ برخلاف TravelDate range که فقط شرط مجاز بودن را تعریف می‌کند.

## قاعده 7: lifecycle و ویرایش ظرفیت

- `TotalCapacity`/`CapacityKg`/`SellableRooms` تنها از Commandهای adjustment با `expectedVersion` تغییر می‌کنند؛ با PUT ساده overwrite بدون audit ممنوع.
- ظرفیت منفی، زمان معکوس، duplicated resource, mismatch unit, unregistered/foreign owner, unknown provider, double-count stock key رد.
- اگر ظرفیت عملیاتی Held/Confirmed داشته باشد، کاهش Total به کمتر از committed occupancy رد شود؛ پس از استقرار Phase 3 باید در همان write boundary enforce شود. Phase 2 فقط ظرفیت Configuration دارد، لذا نمی‌تواند وجود Holds بیرونی را نفی کند. Cutover باید reconcile کند.
- Status `Suspended/ClosedForSale` بدون حذف رکورد و بدون بازگرداندن ظرفیت مصرف‌شده. بازنشسته کردن Resources با تعهدات زنده مجاز نیست مگر reconciliation.
- مبلغ قیمت، تعداد OrderService و AvailabilityDescription هرگز با تغییر ظرفیت به شکل غیرقابل ردیابی تغییر نکنند.

## قاعده 8: source of truth و API failure

- اگر supplier inventory خارجی دارد: Cached `Available=5` فقط Informational است؛ تا recheck/hold supplier انجام نشود guarantee نیست. TTL و `observedAt` اجباری.
- اگر FlightFlow seat state `BLOCKED` یا `OCCUPIED` دهد، local Ancillary نباید با شمارش مکان‌ها آن را AVAILABLE نشان دهد.
- اگر Policy نیست، state `NotConfigured`, fail-closed برای فروش limited؛ fallback به Unlimited ممنوع.
- اگر flight changed/aircraft swap، previous resource scope مربوط به FlightId یا SeatNumber ممکن است معتبر نباشد؛ برای inventory زمان عملیات دوباره reconcile شود.

## قاعده 9: Phase 2 endpointها و Reservation موجود

`AncillaryReservation` فعلی Hold می‌دهد بدون decrement؛ تا اتصال Phase 3 اجرای Production برای inventory محدود تحت آن باید flag/capability guard داشته باشد. نباید response Phase 2 شامل `GuaranteedSellable` باشد. `GetCapacity` فقط configured baseline یا provider snapshot با authority/staleness را نشان دهد.

## قاعده 10: out of scope

- airline certified weight-and-balance, hazardous goods, seat map origin, hotel PMS master, eSIM provider SKU stock, loyalty credit ledger.
- universal multi-dimensional stock expression engine, dynamic key dictionary, reservation updates in Phase 2.
- artificially manufacturing a numeric quota for externally managed products.
