# پرامپت ایجنت Frontend — Ancillary Variant Cards + Typed Batch Forms

**این فایل برای ایجنت Frontend است، نه Backend.** ابتدا قرارداد قطعی `01-IMPLEMENTATION-MAP-AND-API-CONTRACT.md` را بخوان. اگر Backend هنوز v9 را تحویل نداده، فقط آماده‌سازی UI مطابق قرارداد و تست با fixture انجام بده؛ endpoint فرضی یا fallback پنهان نساز. Framework، repo، route و ساختار state management موجود Frontend را **ابتدا بازرسی کن**؛ بدون شاهد نام فایل/پکیج اختراع نکن.

## 1. هدف

- برای هر عنصر `baggages[]`, `meals[]`, `pets[]`, `assistedTravel[]`, `airportServices[]`, `priorityServices[]`, `connectivity[]`, `upgrades[]` یک **Card به ازای Variant** نمایش بده.
- در هر Card محصولات `products[]` را نشان بده و برای هر Product، `units[]` را به تفکیک Traveller/Flight/Portion/Order نشان بده.
- **Unit مستقل** است؛ هیچ ID/price/reference از Unit دیگر کپی نشود و هیچ Product با `variantCode` یکسان ادغام تجاری نگردد.
- فقط برای Unitهای `reference`-دار گزینه‌های تخصصی را بارگیری/ارسال کن؛ برای Direct و Quantity هیچ GET/POST Options وجود ندارد.

## 2. Mapping API شناخته‌شده

| response key | route Family |
|---|---|
| `baggages` | `Baggages` |
| `meals` | `Meals` (در v9 Typed POST ندارد) |
| `pets` | `Pets` |
| `assistedTravel` | `AssistedTravel` |
| `airportServices` | `AirportServices` |
| `priorityServices` | `PriorityServices` |
| `connectivity` | `Connectivity` |
| `upgrades` | `Upgrades` (در v9 Typed POST ندارد) |
| `extraSeats` / `travelInsurance` | gated و در این فاز خالی؛ کارت خالی نساز |

برای Familyهای Typed مجاز فقط همان شش Family ذکرشده در `01` استفاده شود. Prefix `Backoffice`, `OtaPanel`, `Api` براساس Surface فعلی Front و احراز هویت موجود، نه حدس در Client.

## 3. رفتار Card/Product/Unit

- کلید کارت: `${family}:${variantCode}`؛ کلید محصول: `productKey` سرور؛ کلید Unit در UI از ترتیب Array + object موجود تعیین شود (ID مصنوعی به Backend ارسال نشود).
- نام Card: `title` سطح Variant؛ نام Product: `title` سطح Product؛ details هر Product را فقط در همان Product نمایش بده.
- برای `PerTravellerPerFlight`: نام Traveller + Flight؛ برای `PerTravellerPerPortion`: Traveller + مسیر Portion و Flightهای پوشش‌یافته؛ برای `PerTraveller`: Traveller؛ برای `PerOrder`: یک سطر انتخاب. **Portion را به Flight منفرد تبدیل نکن.**
- قیمت Unit را از `unitPrice` و گزینه Quantity را از `quantityOptions[].totalAmount` بگیر؛ قیمت چند ارز را بدون تبدیل معتبر جمع نکن. صفر قیمت معتبر است.
- Quantity = 0 یا عدم انتخاب یعنی هیچ `selectionId` در Basket اضافه نشود؛ برای Quantity >0، عین `selectionId` همان گزینه را نگه دار؛ هیچ Snapshot POST انجام نده.
- Direct: پس از انتخاب، `selectionId` همان Unit را در Basket بگذار.
- Typed: در لیست فقط `reference` وجود دارد؛ هر Unit state فرم مستقل به‌ازای همان Reference داشته باشد.

## 4. GET/Forms به‌صورت Batch

هنگامی که Card یا مرحله تعیین گزینه‌های یک Family باز شد، Referenceهای لازم و هنوز فرم‌بارگذاری‌نشده را جمع کن. حداکثر ۵۰ Reference در یک Call:

```http
POST /OtaPanel/v1/AncillaryOffers/AssistedTravel/Options/Forms
Content-Type: application/json
```

```json
{"offerId":"OFFER-123","references":["r_FOR_ADT1","r_FOR_ADT2"]}
```

Response `forms[]` را با **reference دقیق** به state محلی Unit مربوطه نگاشت کن؛ تفاوت `fields`, `choices`, `required`, `min/exclusiveMin/max` برای هر Traveller محفوظ بماند. اگر Batch خطا داد، هیچ Form ناقص را معتبر نشان نده؛ پیام خطا نمایش بده و retry کاربر ممکن باشد. دوباره‌بارگذاری غیرضروری برای Cardهای دیگر انجام نده.

## 5. Renderer محدود، بدون فرم‌ساز عمومی

فقط `fields` موجود و کنترل‌های زیر:

- `select`: select از `choices`، value دقیق API.
- `multi-select`: مجموعه valueهای `choices`، array.
- `text`: string.
- `number`: عدد decimal/int مطابق field و `min`, `exclusiveMin`, `max`; `exclusiveMin` یعنی strictly greater.
- `checkbox`: boolean؛ برای `optIn` true مطابق backend شرط است.
- `date-time`: رشته تاریخ/زمان با timezone offset معتبر، نه local بدون offset.
- `dimensions`: سه Number Input `lengthCm`, `widthCm`, `heightCm` (در cm).

برچسب‌ها/کمک‌متن فقط در UI translation dictionary موجود تعریف شوند؛ سیستم فرم‌ساز/ترجمه جدید نساز. شرط‌های واقعی را سرور قطعی می‌کند؛ frontend تنها UX validation سبک مطابق field descriptor انجام دهد. مقدارهای `required` وابسته به همان فرم Reference هستند.

## 6. POST Batch تخصصی

پس از تأیید انتخاب‌های Unit در Family، فقط Unitهای انتخاب‌شده را بفرست:

```http
POST /OtaPanel/v1/AncillaryOffers/AssistedTravel/Options/Batch
```

```json
{
  "offerId":"OFFER-123",
  "selections":[
    {"reference":"r_FOR_ADT1","options":{"assistanceSsrCode":"WCHR"}},
    {"reference":"r_FOR_ADT2","options":{"assistanceSsrCode":"WCHS"}}
  ]
}
```

Response `selections[{reference,selectionId,expiresAtUtc}]`: `selectionId`های `s_` را به Unitهای انتخاب‌شده وصل و در Basket ذخیره کن؛ state گزینه‌ها را مجبور نباشی بعداً برای Resolve به Backend بازفرست کنی. اگر خطای All-or-Nothing رخ داد، هیچ ID جدید به Basket نده و اجازه اصلاح همان فرم را بده. خطاهای 401/403/409/410/422 را به کاربر به زبان مناسب نشان بده؛ روی Expired باید Refresh Shopping انجام شود، نه استفاده از r_ قبلی.

## 7. Handoff به Ordering

هنگام ثبت سفارش فقط `d_`های Direct و Quantity و `s_`های Typed را ارسال کن؛ درخواست Resolve `{"ids":[...]}` ثابت مانده است. Duplicate ID در Basket ممنوع؛ هنگام حذف انتخاب، ID را از Basket پاک کن. شکل Response تخصصی Resolve یا SeatMap را تغییر نده.

## 8. کنترل‌های ضروری Frontend

1. دو Product با VariantCode مساوی، دو ردیف محصول زیر همان Card هستند.
2. یک Product با چند Traveller×Flight/Portion → Price و ID مستقل؛ اطلاعات یک Traveller به دیگری leak نشود.
3. Quantity 0/1/2 → 0 هیچ ID؛ 1/2 ID و totalPrice متفاوت همان Unit.
4. یک Forms با دو Reference و `choices` متفاوت → فرم‌ها مستقل باقی بمانند.
5. Batch Submit خطادار → هیچ ID فرضی/partial به Basket اضافه نشود.
6. ۵۰+ Reference → Chunkهای ۵۰؛ ترتیب mapping بر اساس Reference نه صرفاً Index UI.
7. Gated/empty family → Card نشان داده نشود.
8. Single GET/POST Options اگر Client فعلی از آن استفاده دارد بی‌دلیل حذف نشود، تا rollout backend کامل شود.
9. حفظ حق انتخاب و حذف در Basket، بدون دوباره خرید یک Unit تحت یک Card.
10. نمای موبایل و دسکتاپ، accessibility label برای Fieldهای dynamic و خطاهای فیلد بررسی شود.

## 9. گزارش و ممنوعیت‌ها

در گزارش خروجی بنویس: Repo و Commit Frontend، Componentهای واقعی تغییرکرده، call count قبل/بعد برای ۵ مسافر × ۲ Flight، نمونه screenshot یا UI test، mapping `reference -> form -> s_`, Unit/Portion coverage و تست‌های پاس‌شده. **بدون اجازه، Dependency/Form library تازه اضافه نکن؛ هیچ تغییر در Backend، SeatMap یا Ordering از این پرامپت مجاز نیست.**
