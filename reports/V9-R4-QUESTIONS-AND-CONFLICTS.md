# V9-R4 — یک `selectionId` برای هر unit به جای `quantityOptions` امضاشده

| | |
|---|---|
| شاخه | `feat/ancillary-offers-v9` روی `324e329`، R1 تا R3 commit نشده‌اند |
| گزارش مالک (2026-10-26) | در جواب `POST OtaPanel/v1/AncillaryOffers` برای هر تعداد یک `d_` طولانی جدا آمده است |

## 1. علت (از کد و سند)

- بسته v8 این شکل را خواسته است:
  - `docs/Shopping/01-OVERVIEW-AND-BUSINESS-CONTRACT-FA.md:57`: «هر Quantity انتخاب‌شده `d_` مستقل و امضاشده دارد».
  - `docs/Shopping/02-EXACT-API-DTO-AND-FIELD-CONTRACT.md:62`: «one for each integer allowed count, each signed d_».
  - دلیل آن روز: پیش از R3، Submit برای Direct وجود نداشت و `d_` مستقیم به Resolve می‌رفت. پس تعداد باید داخل امضا (`q`) می‌بود تا قابل دست‌کاری نباشد.
- کد: `Shopping/Offers/AncillaryOfferListing.cs` برای هر تعداد مجاز یک `d_` امضا می‌کند و `totalAmount = unitAmount × quantity` می‌گذارد.
- Token نمونه شما decode شد. claimها یکسان‌اند (`rate`، `sc`، `tr`، `cv`، `ob`، `sb`، `exp`) و فقط `q` فرق می‌کند. هر Token حدود 338 کاراکتر است.
- از R3 به بعد هر انتخاب از Submit می‌گذرد و سرور همه‌چیز را آنجا ثابت می‌کند. پس دیگر لازم نیست تعداد داخل امضا باشد.

## 2. پیشنهاد

- **Shopping:**
  - هر unit فقط **یک** `selectionId` دارد.
  - `quantityOptions` حذف می‌شود.
  - `quantityPolicy {selectable, min, max, unit}` حداکثر واقعی را نشان می‌دهد، یعنی حداکثر پس از محدودیت موجودی. مجموعه تعدادهای مجاز امروز هم پیوسته است (از min تا اولین تعدادی که مجاز نیست).
  - `unitPrice` می‌ماند. جمع همان `unitPrice × quantity` است.
- **Token `d_`:**
  - به جای `q`، بازه مجاز را امضا می‌کند (`qn` و `qx`).
- **Submit:**
  - خط Direct `{selectionId, quantity?}` است.
  - اگر `quantity` نیامد، min حساب می‌شود.
  - تعداد بیرون از بازه امضاشده 422/16821 می‌گیرد، با پیشوند `selections[i]`.
- **Resolve و GetBySubmitId:**
  - بدون تغییر. `selected.quantity` همان تعداد ثبت‌شده است.
- **تست‌ها:**
  - تست‌هایی که `quantityOptions` را می‌خوانند (V8 و V9) به همین شکل اصلاح می‌شوند.

## 3. سؤال

### Q1 — این پیشنهاد اجرا شود؟ (از بسته v8، فایل‌های 01 §57 و 02 §62، فاصله می‌گیرد)

Answer: بله (مالک، 2026-10-26: «اجرا کن»)
