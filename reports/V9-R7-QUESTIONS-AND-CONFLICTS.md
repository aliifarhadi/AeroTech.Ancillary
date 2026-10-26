# V9-R7 — چرا `GET {S}/v1/AncillaryOffers/Price/{selectionId}` چیزهای بی‌ربط به انتخاب را هم می‌دهد

| | |
|---|---|
| شاخه | `feat/ancillary-offers-v9` روی `324e329`، commit نشده |
| شاهد زنده (2026-10-26، OtaPanel) | پاسخ 79,792 بایت است و 90 unit دارد، ولی فقط 6 unit انتخاب شده‌اند. Meals و Upgrades هیچ انتخابی ندارند و باز هم کامل آمده‌اند |

## 1. علت

- این پاسخ را خودم کل فهرست AncillaryOffers طراحی کردم و فقط unitهای انتخاب‌شده را با `selected` علامت زدم:
  - `GetPricedAncillaryOffersService` ← `GetAncillaryOffersService.ExecuteAsync(selection)` ← listing کامل.
  - `AncillaryOfferListing.cs:71-73`: هر row اضافه می‌شود. فقط rowی که منطبق است `Selected` می‌گیرد.
- از جمله شما در R3 («ریسپانس دقیقاً مشابه AncillaryOffers … که گزینه‌های انتخاب‌شده را هم در خودش داشته باشد») برداشت کردم «همان فهرست کامل به‌اضافه انتخاب‌ها».
- همین را در R3 (Q4) نوشتم و تأیید شد. ولی جمله شما «همان شکل» را می‌گفت، نه «همان محتوا». بنچ‌مارک هم همین را می‌گوید (از حافظه، در این نوبت دوباره بررسی نشده): پاسخ Amadeus «Flight Offers Price» و NDC `OfferPriceRS` فقط همان چیزی را دارد که قیمت شده، نه کل نتیجه جست‌وجو.

## 2. پیشنهاد

- `GET …/Price/{selectionId}` همان **شکل** AncillaryOffers را بدهد (خانواده ← variant ← product ← unit)، ولی **فقط unitهای انتخاب‌شده** را:
  - هر unit با `offerItemId` تازه، `options` (اگر دارد) و `selected {quantity, options?}`.
  - خانواده‌ای که انتخابی ندارد، آرایه خالی است.
- رفتار بقیه مثل امروز می‌ماند: Offer دوباره ساخته می‌شود و اگر انتخابی دیگر عرضه نشود 409 برمی‌گردد.
- جای تغییر: Shopping `AncillaryOfferListing` وقتی snapshot دارد، فقط rowهای منطبق را برمی‌گرداند. Query و mapperها تغییر نمی‌کنند.

## 3. سؤال

### Q1 — پیشنهاد اجرا شود؟

Answer:
