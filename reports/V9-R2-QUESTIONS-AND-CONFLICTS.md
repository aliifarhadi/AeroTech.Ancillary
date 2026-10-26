# V9-R2 — خالی بودن meals و pets و کامل بودن ورودی `/Options` — سؤال‌ها

| | |
|---|---|
| شاخه | `feat/ancillary-offers-v9` (روی `324e329`)، commit نشده |
| گزارش مالک (2026-10-25) | ۱) در جواب `POST OtaPanel/v1/AncillaryOffers`، `meals` و `pets` خالی‌اند. ۲) به نظر می‌رسد `POST OtaPanel/v1/AncillaryOffers/Options` گزینه‌های همه انواع را ندارد |
| شاهد زنده | host خودم (4146) با Offer `1|1|1;…24283…;3|1551571415767973888||70|1792915276` (IKA→IST و IST→IKA، 2026-10-28): baggages A03، assistedTravel A16، airportServices A21 و A20، upgrades A10. `meals`، `pets`، `priorityServices` و `connectivity` خالی |

## 1. علت (از کد و داده، نه حدس)

### Meals
- همه ۱۱ تعریف Meal در dev فعال‌اند و Provision فعال و نرخ دارند: A11 روی مسیرهای IKA با نرخ 0 IRR، A12 برای Flex. مسیر، fare family، نوع مسافر، تاریخ سفر و advance purchase با این Offer می‌خوانند.
- هر ۱۱ تعریف `ExclusiveMealFamilyCode = MAIN_MEAL` دارند (`Ancillary.MealSpecifications`).
- Engine برای Meal دارای family، از Context «خدمات قبلی مسافر» را می‌خواهد (`Shopping/Engine/ProvisionEvaluation.ProfileRules.cs:239-242`). اگر `ExistingServicesComplete` برقرار نباشد، نتیجه Unknown و آماده‌بودن `NeedsVerification` می‌شود.
- برای منبع Offer، Context همیشه `ExistingServicesComplete = false` و `FareBenefitsComplete = false` می‌گذارد (`Query/Shopping/Common/FlightOffers/FlightOfferShoppingContextMapper.cs:33-34`). برای منبع Order، اولی `true` است (`OrderShoppingContextMapper.cs:53`).
- فهرست فقط سطرهای Selectable یا NeedsSelection+Eligible و قابل‌تحقق را امضا می‌کند (`Shopping/Selections/SelectionSigner.cs:34-38`). پس Mealها پیش از Order هرگز دیده نمی‌شوند.
- همین قاعده روی Baggage هم اثر دارد: قاعده excess-piece در `SharedRules.cs:391`.
- این نقطه از P3.2A به‌صورت «آیا نبود Order یعنی خدمت قبلی ندارد؟» باز مانده بود.

### Pets
- Domain برای Pet الزاماً `SubjectToConfirmation` می‌خواهد (`AncillaryServiceDefinition.Profile.cs:130`، `AncillaryProvision.Publication.cs:249`). داده dev هم همین است (`BookingConfirmationRequirement = 2`).
- محصولِ نیازمند تأیید تأمین‌کننده در فروش آنی امضا نمی‌شود. این همان O04 در v8 است که GATED گزارش شد.

### Priority (اضافه بر گزارش شما)
- هر دو A23 در dev `FareBenefitRef` دارند (`FLEX_PRIORITY`، `PREMIUM_PRIORITY`).
- Offer هیچ fact درباره مزایای fare ندارد (`FareBenefitsComplete = false`؛ AirAvail S2S چنین داده‌ای نمی‌دهد). پس نتیجه NeedsVerification و پنهان است (`ProfileRules.cs:466`).

### `/Options` و انواعی که عضو ندارند
- خط Submit شش عضو دارد: `baggage`، `pet`، `assistedTravel`، `airportService`، `priority`، `connectivity`. Meal و Upgrade عضو ندارند.
- بسته 03 (§A10–A12، سطرهای 32-34، 109-111 و 140-142) می‌گوید: A10 `targetCabinId`، A11 `mealCode` و A12 `menuItemRef`/`quantity` فقط وقتی پرسیده می‌شوند که انتخاب واقعاً باز باشد. اگر Definition آن را قفل کرده باشد، Direct است و POST ندارد.
- در مدل فعلی هر Definition دقیقاً یک `MealCode`، یک `MenuItemRef` یا یک `toCabinId` دارد. پس Meal و Upgrade هرگز ورودی ندارند و در جواب Shopping با `selectionId` (یا `quantityOptions`) می‌آیند، که جواب امروز A10 هم همین را نشان می‌دهد.
- Seat (A07/A08) از SeatMaps فروخته می‌شود. A09، A17، A19 و Insurance طبق تصمیم‌های قبلی GATED‌اند.

## 2. سؤال‌ها

### Q1 — Meal پیش از Order

- الف (پیشنهاد): برای منبع Offer، «خدمات قبلی» = تأییدشده و خالی (`ExistingServicesComplete = true`)، چون پیش از Order خدمتی خریده نشده. یک خط در `FlightOfferShoppingContextMapper`.
  - پیامد ۱: Mealها و قاعده excess-piece در Baggage روی Offer کار می‌کنند.
  - پیامد ۲: دو Meal از یک family برای یک مسافر و پرواز هر دو قابل انتخاب‌اند. Resolve فعلی انحصار داخل سبد را بررسی نمی‌کند. باید یا Ordering این را بررسی کند، یا یک قاعده در Resolve (`MAIN_MEAL` تکراری برای یک unit ← رد) اضافه شود؛ Q1-b.
- ب: داده dev اصلاح شود (حذف `ExclusiveMealFamilyCode` از Mealها). انحصار از بین می‌رود.
- ج: همان‌طور بماند؛ Meal فقط پس از Order فروخته شود.

Q1-b (اگر الف): انحصار family داخل سبد در Resolve بررسی شود (پیشنهاد) یا به Ordering سپرده شود؟

Answer: الف + انحصار در Resolve (مالک، 2026-10-26: «برای Offer، خدمات قبلی «تأییدشده و خالی» حساب شود ... جلوگیری از دو Meal از یک family در یک سبد در Resolve بررسی شود»)

### Q2 — Pet

- الف (پیشنهاد): همان‌طور بماند (GATED). فروش Pet با تأیید تأمین‌کننده یک جریان جدا (درخواست، سپس تأیید) لازم دارد که در این بسته نیست.
- ب: Pet با وضعیت «نیازمند تأیید» در فهرست نمایش داده شود، بدون امکان خرید. این شکل پاسخ را عوض می‌کند و قرارداد جدید می‌خواهد.

Answer: الف، GATED بماند (مالک، 2026-10-26)

### Q3 — Priority (A23) با `FareBenefitRef`

- الف (پیشنهاد): داده dev اصلاح شود (حذف `FareBenefitRef` از دو A23)، چون منبعی برای مزایای fare نداریم.
- ب: برای Offer، `FareBenefitsComplete = true` با لیست خالی. یعنی هیچ fare شامل priority نیست؛ ممکن است خطا باشد و برای Flex دوبار فروخته شود.
- ج: همان‌طور بماند.

Answer: الف، `FareBenefitRef` از داده dev حذف شود (مالک، 2026-10-26)

### Q4 — اعضای Meal و Upgrade در `/Options`

- الف (پیشنهاد): اضافه نشوند. طبق بسته 03، Meal و Upgrade در مدل فعلی Direct‌اند و عضو آن‌ها همیشه خالی می‌ماند.
- ب: عضوهای `meal {mealCode?, menuItemRef?}` و `upgrade {targetCabinId}` اضافه شوند. این فقط وقتی معنا دارد که یک Definition چند MealCode، MenuItem یا کابین مقصد داشته باشد، که Domain امروز ندارد و تغییر Domain لازم است.

اگر منظور شما از «گزینه‌های کامل» چیز دیگری است (مثلاً فیلدی در یکی از شش عضو)، لطفاً بگویید کدام نوع و کدام فیلد.

Answer: الف، اضافه نشوند (مالک، 2026-10-26)
