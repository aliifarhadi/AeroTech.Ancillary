# V9 Gate 0 — نقشه تغییر (فقط خواندن)

| | |
|---|---|
| مرجع | `docs/Shopping/FrontEnd/01..05` (v9) |
| Baseline | `feat/ancillary-offers-v8@324e32999bac6a3a9dfd69604dbe9e31272c43f0` = HEAD، درخت تمیز (فقط پوشه بسته v9 track نشده) |
| شاخه کار | `feat/ancillary-offers-v9` از همان SHA |

## 1. تصمیم‌های مالک برای این بسته (2026-10-24)

1. یک پیاده‌سازی: Forms و Batch همان سرویس‌های موجود Options هستند. مسیرهای تک‌موردی همان سرویس را صدا می‌زنند و مراحل مشترک (بارگذاری Offer، Guard، Context، Proof، ساخت Snapshot) یک بار نوشته می‌شوند.
2. خطای عضو Batch: همان `BusinessException` با همان Code و HttpStatus، و پیش‌وند `references[i]: ` یا `selections[i]: ` در پیام. این فقط در مسیر Batch است و پیام مسیرهای تک‌موردی تغییر نمی‌کند.
3. `optionsEndpoint` از پاسخ Shopping حذف می‌شود.

## 2. شش خانواده تایپی (بدون تغییر)

`BaggageSelectionOptions`، `PetSelectionOptions`، `AssistedTravelSelectionOptions`، `AirportServiceSelectionOptions`، `PrioritySelectionOptions` و `ConnectivitySelectionOptions` همگی `[JsonUnmappedMemberHandling(Disallow)]` دارند و `ISelectionOptions` را پیاده می‌کنند. Meals و Upgrades مسیر POST ندارند.

## 3. نقشه عملیات به فایل

| عملیات v9 | فایل‌ها |
|---|---|
| گروه‌بندی Card/Product/Unit | Query `Dto/AncillaryVariantGroupDto`، `AncillaryProductGroupDto`، `AncillaryUnitOfferDto` و سه معادل `Dto/Backoffice/Backoffice*` (به‌خاطر `EnumValueDto`)؛ `AncillaryOffersDto`، `BackofficeAncillaryOffersDto`؛ `AncillaryOffersMapper`، `BackofficeAncillaryOffersMapper`؛ سه handler فهرست (ثابت `Surface` حذف می‌شود). `AncillaryOfferDto` و `BackofficeAncillaryOfferDto` حذف می‌شوند. `OfferFamilyRoutes.Of` هم حذف می‌شود چون فقط `optionsEndpoint` از آن استفاده می‌کرد. |
| Batch Forms | Query `GetSelectionOptions/`: `ISelectionOptionsFormsQuery`، `ExecuteBatchAsync` در همان `GetSelectionOptionsService`، `{Surface}GetSelectionOptionsFormsQuery(+Handler)`، `SelectionOptionsMapper` (نگاشت فرم‌ها)، `Dto/SelectionOptionsFormsDto` و `SelectionOptionsFormDto`؛ کمکی `Common/SelectionBatch` (پیش‌وند اندیس) |
| Batch Submit | Application `SubmitSelectionOptions/`: `ISubmitSelectionOptionsBatchCommand`، `SelectionOptionsLine`، `SubmitBatchAsync` در همان `SubmitSelectionOptionsService`، `{Surface}SubmitSelectionOptionsBatchCommand(+Handler+Validator)`، اعتبارسنج مشترک `SelectionOptionsValidator` (همان سه قاعده فعلی، از اعتبارسنج تک‌موردی جدا شده)، `SelectionSnapshotBatchResult` و `SelectionSnapshotLineResult`؛ پورت `ITypedSelectionVerifier.VerifyBatchAsync` با `TypedSelectionBatchVerification`؛ پیاده‌سازی در Query `TypedSelectionVerifier` |
| Routes | سه Controller: یک `POST {family}/Options/Forms` و شش `POST <Family>/Options/Batch` در هر کدام؛ RestApi `V1/AncillaryOffers/Requests/SelectionOptionsFormsRequest` و `SubmitSelectionOptionsBatchRequest<TOptions>` + `SelectionOptionsLineRequest<TOptions>` (سخت‌گیر در برابر عضو ناشناخته) |
| Pin مسیرها | `V12BoundaryAcceptanceTests`: از 170 به 191 مسیر (21 مسیر جدید) |

## 4. بدون تغییر

Engine، `AncillaryOfferListing`، `AncillaryOfferRow`، `TypedSelectionService`، Codec و JWS، Domain، `SelectionSnapshot`، Persistence و Migration، ResolveGrouped، SeatMap، Reservation، سیاست‌های Sales Scope. نیازی به جدول، تنظیم، Cache یا سرویس تازه نیست.

## 5. انتخاب‌های داخل چارچوب بسته (برای بازبینی)

- ترتیب بررسی همان مسیر تک‌موردی فعلی است: اول همه Referenceها خوانده می‌شوند (امضا و انقضا)، سپس Offer یک بار بارگذاری می‌شود، سپس Form یا Proof برای هر عضو ساخته می‌شود.
- شکل ورودی Forms (۱ تا ۵۰، خالی، بیش از 1024، تکراری) در سرویس Query بررسی می‌شود و 16801/422 می‌دهد، مثل `ResolveGroupedSelectionsService`.
- شکل ورودی Batch POST در Validator بررسی می‌شود و 400 با مسیر فیلد (مثل `Selections[1].Reference`) می‌دهد، مثل POST تک‌موردی فعلی.
- Fixture تستی `LabVerifier` (کپی منطق Verifier) حذف می‌شود. تست‌ها از `TypedSelectionVerifier` واقعی با یک منبع Offer جعلی استفاده می‌کنند که تعداد `LoadAsync` را می‌شمارد.

## 6. Blocker

ندارد. P10 (Pets) مثل O04 در v8 GATED می‌ماند، چون Domain برای Pet تأیید تأمین‌کننده لازم دارد و فهرست به Pet هیچ `r_` نمی‌دهد. مسیر Batch برای Pets ساخته و با `r_` امضاشده مستقیم آزموده می‌شود.
