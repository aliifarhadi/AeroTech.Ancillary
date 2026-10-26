# V9-R1 — یک مسیر Forms و یک مسیر Submit برای همه خانواده‌ها — سؤال‌ها

| | |
|---|---|
| شاخه | `feat/ancillary-offers-v9` (روی `324e329`)، commit نشده |
| درخواست مالک (2026-10-25) | یک `POST {Surface}/v1/AncillaryOffers/Options/Forms` برای گرفتن فرم همه Referenceها، و یک POST واحد به‌جای شش مسیر POST خانواده‌ای که ورودی‌اش options همه انواع Ancillary را در خود دارد |

## 1. وضع فعلی کد و شواهد

- Forms فعلی: `POST {Surface}/v1/AncillaryOffers/{family}/Options/Forms` با بدنه `{offerId, references}`. خانواده در مسیر فقط برای بررسی است؛ خود `r_` نرخ را امضا کرده و خانواده از Definition آن نرخ معلوم می‌شود (`TypedSelectionService.TargetAsync`: `definition.Profile != family` ← 16819).
- Submit فعلی: شش مسیر `…/<Family>/Options/Batch`، هر کدام با نوع options خودش (`BaggageSelectionOptions`، `PetSelectionOptions`، `AssistedTravelSelectionOptions`، `AirportServiceSelectionOptions`، `PrioritySelectionOptions`، `ConnectivitySelectionOptions`). همه سخت‌گیر در برابر عضو ناشناخته‌اند (`JsonUnmappedMemberHandling.Disallow`).
- مسیرهای تک‌موردی قبلی هنوز هستند: `GET {family}/{reference}/Options` و شش `POST <Family>/{reference}/Options`.
- در این repo و Ordering.Final و AirAvail و AirPrice هیچ ورودی چندشکلی (polymorphic) با discriminator پیدا نشد (جست‌وجوی `JsonPolymorphic` و `TypeDiscriminator` و `JsonDerivedType(..., "...")`).
- قاعده‌های موجود: بسته 01 §5.1 می‌گوید ورودی `object`، `JsonElement` یا dictionary ساخته نشود. حافظه «no copied field bags» می‌گوید رکوردی با فیلدهای nullable بی‌ربط به هم ساخته نشود. ولی دستور شما (یک POST برای همه خانواده‌ها) بر بسته مقدم است.

## 2. سؤال‌ها

### Q1 — ورودی Forms واحد

`POST {Surface}/v1/AncillaryOffers/Options/Forms`

- الف (پیشنهاد): `{offerId, references:["r_…","r_…"]}` بدون خانواده. خانواده از خود Reference خوانده می‌شود. در پاسخ هر فرم `reference`، `family` (مثل `AssistedTravel`) و `variantCode` و `fields` دارد.
- ب: `{offerId, items:[{family, reference}]}`، همان ورودی GET قبلی برای هر عضو. خانواده دوبار گفته می‌شود و فقط برای بررسی است.

پیشنهاد: الف.

Answer: الف (مالک، 2026-10-25: «با همه پیشنهادها موافقم»)

### Q2 — شکل options در POST واحد

`POST {Surface}/v1/AncillaryOffers/Options` (یا `…/Options/Batch`؛ Q4)

- الف (پیشنهاد): هر خط یک عضو جدا برای هر خانواده دارد و دقیقاً یکی از آن‌ها پر است. نوع هر عضو همان record فعلی است:
  `{offerId, selections:[{reference, baggage?, pet?, assistedTravel?, airportService?, priority?, connectivity?}]}`.
  Validator بررسی می‌کند که دقیقاً یک عضو پر باشد. سرویس بررسی می‌کند که آن عضو با خانواده خود Reference یکی باشد، وگرنه 16819 (مثل امروز). نوع‌ها typed و سخت‌گیر می‌مانند.
- ب: discriminator: `{reference, options:{"family":"Baggages", …}}` با JSON چندشکلی (`JsonPolymorphic`). در پروژه‌ها سابقه ندارد.
- ج: یک options تخت با همه فیلدها (18 فیلد nullable). همان «field bag» ممنوع است؛ پیشنهاد نمی‌کنم.

پیشنهاد: الف.

Answer: الف (مالک، 2026-10-25)

### Q3 — مسیرهای قدیمی

حذف شوند یا بمانند؟

- `GET {family}/{reference}/Options` و شش `POST <Family>/{reference}/Options` (تک‌موردی v8)
- `POST {family}/Options/Forms` و شش `POST <Family>/Options/Batch` (همین شاخه، commit نشده)

پیشنهاد: همه حذف شوند و فقط دو مسیر جدید بمانند، تا قاعده ۲ CLAUDE.md (یک مسیر نوشتن) رعایت شود. Pin مسیرها از 191 به 170 − 7×3 + 2×3 = 155 می‌رسد.

Answer: همه حذف شوند (مالک، 2026-10-25)

### Q4 — نام دو مسیر

- Forms: `POST {Surface}/v1/AncillaryOffers/Options/Forms` (همان که گفتید)
- Submit: `POST {Surface}/v1/AncillaryOffers/Options` (پیشنهاد) یا `POST {Surface}/v1/AncillaryOffers/Options/Batch`

Answer: `POST {Surface}/v1/AncillaryOffers/Options` (مالک، 2026-10-25)

## 3. بدون تغییر

همه‌یا‌هیچ، یک بار بارگذاری Offer، یک `SaveChanges`، سقف ۵۰، پیش‌وند `references[i]` / `selections[i]` در خطا، ResolveGrouped، کارت‌بندی پاسخ Shopping، Engine، Domain و Migration.
