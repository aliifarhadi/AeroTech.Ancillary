# 01 — Master Reservation v11 — Domain First، کامل و عملیاتی

## A. علت دقیق نقص v10 و تصمیم حفظ/بازنویسی
**ریشه:** Pack v10 ابتدا ledger، SQL lock، expiry و وضعیت‌ها را طراحی کرد، اما پرسش بنیادی «مشتری دقیقاً چه Ancillary با کدام گزینه را رزرو کرده است؟» را پیش از طراحی DTO و Aggregate پاسخ نداد. `AncillaryReservationUnit` فقط DefinitionId/ProvisionId، `CoverageScope`، FlightIds و Qty را داشت؛ انتخاب‌های `Selected.Options` از Price/Resolve (SSR، حیوان/ابعاد، appointment، ...)، به‌جز Seat و دو Fact Slot، داخل Reservation ذخیره نشد. تست‌ها mostly inventory و lifecycle بودند، نه **Round-trip انتخاب تجاری**. بنابراین کد «کار می‌کند» ولی Contract بد و Domain ناقص می‌ماند.

**حکم:** کد v10 **پاک نشود**؛ Aggregate Root، Unit identity، status/idempotency، Seat facade، Ledger، child Allocation، SQL transaction و تست‌های واقعی **حفظ**؛ تنها API Hold و Read، typed Selection و Persistence متناظر بازطراحی شود. کد deprecated بعد از تست حذف گردد، نه قبل. `Retire` منابع دارای مصرف فعال یک نقص ایمنی مستقل P0 است.

## B. حدود مالکیت
- Ordering: Order/OrderService، انتخاب پذیرفته‌شده، تطبیق Traveller و Flight با Order در **خود Ordering**؛ EMD/Ticket/Payment و هماهنگی FlightFlow؛ درخواست S2S را می‌سازد.
- Ancillary: فقط ورودی S2S را دریافت می‌کند؛ **هیچ Call/lookup/validation membership با Ordering، Offer، Price، Snapshot قبلی** ندارد. تنها واقعیت‌های محلی Definition، Provision، Active Policy، Candidate variant و Rules، Qty، Inventory Source/Capacity، expiry، replay، status بررسی می‌شوند.
- FlightFlow: موجودی واقعی صندلی هواپیما؛ هیچ Seat Hold از Ancillary به FlightFlow در v11 نیست.
- Source of truth: Reservation برای **جزئیات پذیرفته‌شده عملیاتی** و Inventory Allocation؛ Ordering برای قیمت/مالی/سند. قرارداد Caller مبتنی بر اعتماد و احراز هویت Service است، نه ادعای Proof تاریخی.

## C. Flow دقیق
1. `AncillaryOffers` به Frontend `baggages[]`, `meals[]` ... می‌دهد؛ Variant Card/Product/Unit و options inline.
2. Frontend به `/Price` انتخاب‌های نوعی را می‌دهد؛ Price یک `selectionId=s_...` سبد می‌دهد. `Resolve` سپس `ServiceResolvedSelectionDto<T>` با `provision.provisionId`, `definition.variantCode`, `unit`, `coverage`, `selected` و `details` برمی‌گرداند.
3. Ordering از `Resolve` و Order **در سمت خودش** حداقل Payload رزرو را می‌سازد؛ برای هر Family کارت/Variant/Product/Units. **در Ancillary هیچ رجوعی به Ordering یا Price وجود ندارد**.
4. `POST Service-Holds`: Ancillary روی هر Product از `provisionId` Active Provision+Definition/Variant/Scope و InventoryPolicy را می‌خواند؛ فقط selected fields مرتبط را Validate می‌کند؛ انتخاب نهایی را immutable می‌سازد؛ ظرفیت Count/Weight/Slot را در یک Transaction و قفل منبع تخصیص می‌دهد؛ Root+Units+Allocations را ذخیره می‌کند؛ Receipt کوچک می‌دهد.
5. Ordering درخواست اختصاصی `SeatMaps/Holds` را برای Seat-only یا Append به همان Hold ارسال می‌کند. Ancillary فقط commercial verifier و lifecycle را اجرا می‌کند؛ ۰ تماس فیزیکی/OccupiedSeats.
6. `Confirm` روی Root واحد، حتی اگر بعضی Units Released شده باشند. `DELETE Service-Holds/{holdId}` همه یا فقط UnitIds[] Held را آزاد می‌کند. `Cancellations` Confirmedها را طبق reason cancel می‌کند. Expiry برای Held منطقی و persisted است.
7. `GET Service-Holds/{holdId}` historical typed selection را از **داده پایدار همان Reservation** می‌دهد، حتی پس از انقضا/بازنشستگی Definition/Price Snapshot؛ بدون lookup بیرونی.

## D. قرارداد ذهنی Wire — متقارن با AncillaryOffers
`AncillaryOffers` = `baggages → [{variantCode,products:[{productKey,...,units:[{unit,coverage,quantityPolicy,selectionId/reference,...}]}]}]`.

`Hold` = **همان nesting و namesِ قابل انتقال**: `baggages → [{variantCode,products:[{provisionId,units:[{orderServiceId,unit,coverage,selected}]}]}]`. برای Pet/Meal/Assisted/Airport/... همین الگو. **فقط داده موردنیاز رزرو**: Product در Hold به جای presentation `productKey` دارای `provisionId` (از Resolve) است؛ `unit` و `coverage` در Hold، شناسه‌های بالفعل Order یعنی `travellerId` و `flightIds` را دارند، زیرا `travellerRef`/`flightRefs`ِ Offer به‌تنهایی Stock/OrderServiceId را مشخص نمی‌کنند. شرح دقیق تبدیل در 02؛ هیچ `serviceDefinitionId`, `coverageScope`, `inventoryFacts` از Caller مطالبه نشود.

`selected` از **نام/شکل موجود `Resolve.selected`**: `quantity:int`, `options:{baggage|pet|assistedTravel|airportService|priority|connectivity: {existing-fields}}`. برای Qty ثابت=1 و بدون گزینه، selected می‌تواند حذف شود؛ `quantity=1` در Domain از Provision مشتق. اگر گزینه مشتری متغیر و معتبر نیست، Product gated و نه guessed.

## E. Root/Unit در Domain
- یک `AncillaryReservation` با `Id`, `IdempotencyKey`, `OrderId`, `Reference`, `ExpiresAt`, `Status`, `Units[]` و عملیات Root `Hold`, `AppendSeats`, `Confirm`, `Release(unitIds?)`, `Cancel(reasonCode,unitIds?)`, `Expire`.
- هر `AncillaryReservationUnit` دارای `Id`, `OrderServiceId`, `ProvisionId`, **derived** `ServiceDefinitionId`, `VariantCode`, `CoverageScope`, `TravellerId?`, `CoveredFlightIds`, `Quantity`, `Status`, **Typed Selection** و `Allocations[]` می‌باشد.
- Child `AncillaryReservationUnitAllocation` = `(ResourceKind,InventorySourceId,Amount)` immutable؛ Hold، Available را از Active Held+Confirmed روی ledger می‌سنجد.
- Typed Selection حتماً از فعل کسب‌وکار خاص بگوید: `BaggageSelected`, `PetSelected`, `AssistanceSsrCode`, `AirportService appointment/guests`, `MealCode/MenuItemRef`, `Connectivity Plan/Devices`, Seat number. هیچ clone کل Pricing و Offer لازم نیست.
- ۲۴ Aggregate یا ۲۴ Entity ایجاد نشوند؛ ۶–۸ Selection VO/Owned type حداقلی فقط در صورت نیاز variant فعال. اطلاعات ثابت مؤثر Fulfillment در لحظه Hold snapshot می‌شود. **اولین Hold GET باید انتخاب قبلی را بدون live Definition بازگرداند**.

## F. Inventory و موارد GATED
- `Unlimited`: بدون Allocation؛ `Local FlightCount`, `FlightWeight FixedKg`, `FlightCountPlusWeight`, `AirportSlot`: با ledger فعلی و قفل SQL مشترک؛ `TotalCapacity` تغییر نکند؛ هیچ counter موازی یا coordinator عمومی ایجاد نشود.
- `Supplier` بدون Adapter واقعی؛ `AcceptedWeightKg`/`PerKilogram` unsupported؛ A09 ExtraSeat/A10 Quote/A17 medical/A19 UMNR با دلیل واقعی gated. در مسیر unsupported **حتماً** خطای صریح، بدون Fake Held.
- SeatMap تنها commercial unit و بدون Allocation. **Retire یک Source دارای ActiveUsage>0 باید 409** زیر همان قفل فعلی بدهد. Connection pool tuning عملیاتی است، نه علت بازطراحی.

## G. عملیات بیرونی و گزارش دقیق
`POST Service-Holds`؛ `GET Service-Holds/{id}`؛ `POST .../Confirmations`؛ `DELETE Service-Holds/{id}?unitIds=...`؛ `POST .../Cancellations`؛ `POST SeatMaps/Holds` — **۶ Endpoint و فقط همین‌ها در Scope رزرو**. جزئیات و JSON کامل تک‌تک در 02.

## H. معیار Closure
برای تمام Variantهای اعلام‌شده Supported: `AncillaryOffers/Resolve.selected` → `Hold wire` → SQL → GET **بایت/مقدار انتخابی برابر**؛ تداخل Stock بر اساس SQL؛ بدون Order client در Ancillary؛ Seat physical untouched؛ replay/expiry/partial statuses؛ Legacy migration ایمن؛ تمام تست‌های 07 با نام و فرمان و نتیجه واقعی. **`NOT VERIFIED` و `GATED` هرگز PASS نیستند**.
