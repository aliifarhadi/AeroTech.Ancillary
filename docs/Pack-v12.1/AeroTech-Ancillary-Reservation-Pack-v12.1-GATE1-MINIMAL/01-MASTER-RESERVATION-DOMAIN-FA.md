# 01 — مدل تجاری و تصمیم نهایی Domain-First

## 1. چرا v10 از Domain جا ماند؟
در v10 ابتدا Lifecycle و SQL Inventory طراحی شدند و `AncillaryReservationUnit` به شناسهٔ Provision، Quantity، Scope، FlightIds و تعدادی Fact عمومی محدود ماند. این‌ها «چه مقدار ظرفیت رزرو شد؟» را جواب می‌دادند؛ نه «مشتری کدام Meal/Bag/SSR/Appointment را خرید؟». تفاوت اطلاعات `Resolve.selected` با فیلدهای ذخیره‌شده Unit، تست Round-trip نداشت. خطا از Specification/Contract بود نه الزاماً پیاده‌سازی Coding Agent.

**اصلاح کافی چیست؟** از ورودی خانواده‌محور، محل Product (`provisionId`) و انتخاب واقعی Typed را بگیر؛ با قواعد محلی محصول همان لحظه Fixed Choices را تکمیل کن؛ در یک Unit با Data owned ذخیره کن؛ از داده پایدار همان Unit، حتی بعد از Retire یا Expiry، GET بازسازی کن. تست Input → Domain → SQL → Reload → GET و Replay باید آن را اثبات کند. v11 این ساختار را در سورس دارد؛ Pack v12 برای داوری و اصلاح ایمنی باقی‌مانده است، نه ایجاد دوباره.

## 2. تصمیم بازنویسی
**Refactor محدود v11؛ نه Delete و Restart.** Preserve: `AncillaryReservation`، `AncillaryReservationUnitAllocation`، Status transitions، SQL ledger، `sp_getapplock`، inventory sources، partial release/cancel، SeatMap facade. Preserve/verify: V11 typed family contract، fixed-choice capture، persisted typed selections، owned tables. Correct: اختلال «empty selection → all» در HTTP Release؛ Gapهای اثبات‌شده، نه حدس‌های جدید. Migration موجود `20261026190047_V11ReservationSelections` را بدون بازسازی مخرب نگه‌دار.

## 3. مسئولیت‌ها و مرز اعتماد
- Shopping/AncillaryOffers: محصولات، variant، options inline؛ `/Price` قیمت/اعتبار سبد و `s_...`؛ `/Resolve` جزئیات قبول‌شده.
- Ordering (Caller): تبدیل شناسه‌های Offer-scoped به `travellerId` و `flightIds` واقعی و نگهداری قیمت/EMD/Ticket/Refund؛ هماهنگی Hold آن با FlightFlow. صحت تعلق Service/Traveller به Order **فقط در Ordering**.
- Ancillary: Definition/Provision/Policy/Capacity/Source و انتخاب تخصصی، Hold/Confirm/Release/Cancel/Expiry، ماندگاری جزئیات تجاری. **بدون Ordering lookup, callback, ACL**؛ هیچ اجبار به `s_`/Price دوباره.
- FlightFlow: Seat physical assignment/occupancy/release؛ Ancillary فقط Commercial Selection برای Seat.

## 4. Flow انتهابه‌انتها
1. Caller از قبل `Resolve` را دیده و خودش OrderService را ثبت کرده است.
2. برای هر OrderService یک Unit در خانوادهٔ مناسب با Product/Provision و فیلدهای انتخاب می‌فرستد.
3. Ancillary `provisionId` را به Active Provision/Definition/Variant/Policy می‌برد؛ `coverageScope` و مقدار `Quantity` پیش‌فرض را از داده محلی derive می‌کند؛ Static rules را با `TypedSelectionForm.Fields` بررسی و Fixed Choices را با `WithFixedChoices` ثبت می‌کند. Membership به Order یا برابری با سبد قبلی **بررسی نمی‌شود**.
4. Allocation برای Local Count/Weight/Slot از Policy و Typed Options ساخته و در همان Transaction با Root/Units نوشته می‌شود. `Unlimited` Allocation ندارد؛ `Supplier` بدون Adapter reject می‌شود.
5. پاسخ Write تنها `holdId, reference, status, expiresAt, units[{unitId,orderServiceId,status}]` است.
6. Seat Commercial از `SeatMaps/Holds` با `holdId` اختیاری می‌آید؛ Append به Root موجود می‌تواند انجام شود، بدون تماس FlightFlow.
7. قبل از Deadline، Confirm Units Held را Confirmed می‌کند؛ Release فقط Held را آزاد می‌کند؛ Cancel فقط Confirmed را Cancelled؛ Expiry باعث آزادشدن Logical Usage و در Poller Persist می‌شود.
8. GET همیشه از Reservation/owned data و برای همه Unitها حتی Released/Cancelled/Expired جزئیات را نمایش می‌دهد. ProviderReference فقط اگر واقعاً موجود باشد، نه Fake. Confirm مساوی EMD نیست.

## 5. مدل Unit و معنای History
یک `AncillaryReservation` Root و چند Unit با شناسه و وضعیت مستقل. Unit شامل (OrderServiceId, ProvisionId, Derived DefinitionId, VariantCode, ServiceDefinitionRef snapshot, TravellerId?, FlightIds child, Quantity, Status, CancellationReason?, Typed Details?, Allocations) است. Value Objectهای انتخاب نباید «بر اساس تغییر Definition زنده» در GET ساخته شوند. تاریخچه انتخاب پذیرفته‌شده مستقل از Pricing و Offer TTL است.

## 6. خطرات تصمیم‌گیری
- **Gate0 F02:** `?unitIds=` بر اساس evidence HTTP با 400 رد می‌شود؛ ادعای P0 قدیمی معتبر نیست. **F03 / safety:** Query key اشتباه مثل `unitId` یا `unitIds[]` نباید به Release-All تعبیر شود. Guard حداقلی در Controller، بدون دست‌کاری Domain/Validator؛ قبل از mutation تست شود.
- **Observed not verified:** Ordering end-to-end، Live Local Source، build/test مستقل از گزارش v11.
- **GATED deliberate:** A09, A10 با External Quote, A13/A14, A17/A18/A19، Supplier بدون Adapter و AcceptedWeightKg policy. صرف وجود Registry/Selection Contract به معنی پشتیبانی Hold نیست.
- **Scope out:** Price recheck، Fraud/Authentication طراحی جدید، EMD، خرید Post-order به سبک سایر سیستم‌ها، Provider saga. این حوزه‌ها بعداً جداگانه تصمیم‌گیری شوند.

## 7. معیار Done
محتوای هر Unit بازسازی‌پذیر، انطباق Contract با Source، SQL last stock و rollback، عدم outbound به Ordering/FlightFlow، Idempotent retries، Release/Cancel جزئی، Retire safe و Verify-zip. نتیجه تست‌ها فقط با command/run در همان Gate اعلام شود.
