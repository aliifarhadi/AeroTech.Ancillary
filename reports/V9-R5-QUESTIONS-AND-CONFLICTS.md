# V9-R5 — چرا Submit و Resolve طبق خواسته شما نشده‌اند

| | |
|---|---|
| شاخه | `feat/ancillary-offers-v9` روی `324e329`، commit نشده |
| شاهد | اجرای زنده OtaPanel در 2026-10-26 (scratch `v9/live-r4-otapanel`) |
| خواسته شما (R3) | «Submit دقیقاً همان options را برای هر ancillary بگیرد، دقیقاً با همان اسامی و دیگر نیاز نباشد کاربر حدس بزند … برای Resolve فقط کافی است Id مرحله Submit را داشته باشیم و خروجی Resolve نباید فرق کند» |

## 1. مقایسه با پاسخ زنده

| جا | شکل امروز (زنده) |
|---|---|
| unit در Shopping | `{"reference": "r_…", "options": {"assistedTravel": {"assistanceSsrCode": {"control": "select", …}}}}` |
| `selected` در GetBySubmitId | `{"quantity": 1, "options": {"assistedTravel": {"assistanceSsrCode": "BLND"}}}` |
| خط Submit | `{"reference": "r_…", "assistedTravel": {"assistanceSsrCode": "BLND"}}`، **بدون `options`** |
| آیتم Resolve | `"selectionId": "l_4GnC…"` و `"selected": {"assistanceSsrCode": "BLND"}` |

### فاصله 1 — Submit همان نام‌ها را ندارد

- در Shopping و در GetBySubmitId گزینه‌ها زیر `options.<family>` هستند. در Submit همان عضو خانواده مستقیم کنار `reference` می‌آید. Frontend باید بداند که `options` را بردارد، یعنی هنوز حدس می‌زند.
- Swagger برای هر خط Submit یک record با 9 عضو اختیاری نشان می‌دهد: `selectionId`، `quantity`، `reference` و شش خانواده. از روی آن معلوم نیست هر محصول کدام را می‌خواهد.
- علت: نمونه‌ای که در R3 (Q2) پیشنهاد دادم همین شکل بی‌`options` بود. آن را با شکل unit مقایسه نکردم.

### فاصله 2 — خروجی Resolve فرق کرده است

- `selectionId` هر آیتم پیش از R3 همان Idی بود که فراخواننده فرستاده بود. حالا Id داخلی خط (`l_…`) است، که فراخواننده هیچ‌جا ندیده است، نه در پاسخ Submit و نه در GetBySubmitId.
- `selected` در Resolve بدون عضو خانواده و بدون `quantity` است. همین انتخاب در GetBySubmitId شکل دیگری دارد.
- علت: پیشنهاد Q3 در R3 (Id جدا برای هر خط) از من بود. `selected` را هم همان‌طور که در Resolve بود نگه داشتم.

## 2. سؤال‌ها

### Q1 — خط Submit

- الف (پیشنهاد): خط Submit دقیقاً نام‌های unit را داشته باشد:
  - `{"selectionId": "d_…", "quantity": 2}`، یا
  - `{"reference": "r_…", "options": {"assistedTravel": {"assistanceSsrCode": "BLND"}}}`.

  Frontend `reference` و `options` همان unit را برمی‌دارد و جای هر توصیف، مقدار می‌گذارد. Recordهای typed هر خانواده زیر `options` می‌مانند.
- ب: همان‌طور بماند.

Answer: الف (مالک، 2026-10-26: «با پیشنهادها موافقم»)

### Q2 — خروجی Resolve

- الف (پیشنهاد):
  - شکل خروجی همان بماند.
  - `selectionId` هر آیتم همان Idی باشد که فراخواننده در Submit برای آن خط فرستاده است: `d_` برای Direct و `r_` برای خط گزینه‌دار. Id داخلی `l_` از پاسخ حذف می‌شود.
  - `selected` به شکل GetBySubmitId درآید: `{quantity, options: {<family>: …}}`.
- ب: `selectionId` هر آیتم حذف شود و فقط Id سبد (`s_`) بالای پاسخ بیاید.
- ج: همان‌طور بماند (`l_…`).

Answer: الف (مالک، 2026-10-26: «با پیشنهادها موافقم»)

### Q3 — نام‌ها (بنچ‌مارک از حافظه، در این نوبت دوباره بررسی نشده)

- مرجع‌ها:
  - NDC: `OfferID` برای Offer و `OfferItemID` برای هر قلم آن. `OrderCreate` به همین دو ارجاع می‌دهد.
  - Duffel: هر خدمت قابل خرید `available_services[].id` دارد و در ساخت Order به شکل `services: [{id, quantity}]` فرستاده می‌شود.
- پیشنهاد:
  - unit در Shopping: `offerItemId`، یک نام برای هر دو نوع `d_` و `r_`. وجود `options` نشان می‌دهد که گزینه لازم است.
  - خط Submit: `{offerItemId, quantity?, options?}`.
  - پاسخ Submit و ورودی Resolve و GetBySubmitId: `selectionId` (`s_`)، تنها جایی که این نام می‌ماند.
  - آیتم Resolve: `offerItemId`، همان Idی که در Submit فرستاده شده (Q2-الف).

Answer: الف (مالک، 2026-10-26: «با پیشنهادها موافقم»)

اگر منظور شما فاصله دیگری است (مسیر، نام endpoint یا شکل دیگری)، لطفاً همان را بگویید.
