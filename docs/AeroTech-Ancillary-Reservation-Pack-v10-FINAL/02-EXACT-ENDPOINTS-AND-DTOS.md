# 02 — قرارداد دقیق HTTP و DTO — Reservation v10

**مرجع اصلی:** `01-MASTER-RESERVATION-CONTRACT-FA.md`. این فایل صرفاً تعیین دقیق Wire Contract است. مثال‌ها از نام/شناسه فرضی استفاده می‌کنند؛ Routeهای موجود از سورس v9 اتخاذ شده‌اند. Enumها در JSON مطابق Serializer فعلی پروژه serialize شوند؛ نمونه‌ها از **عدد واقعی Enum** استفاده می‌کنند تا وابسته به تنظیمات `JsonStringEnumConverter` نباشند.

## 1. جدول endpointها
| Operation | Method | Exact path | وضعیت |
|---|---|---|---|
| Create general hold | POST | `/Service/v1/Ancillaries/Service-Holds` | موجود؛ behavior تکمیل شود |
| Read hold | GET | `/Service/v1/Ancillaries/Service-Holds/{holdId:long}` | موجود؛ Unitهای جزئی پشتیبانی شوند |
| Confirm active hold | POST | `/Service/v1/Ancillaries/Service-Holds/{holdId:long}/Confirmations` | موجود؛ حفظ Released Units |
| Release remaining held | DELETE | `/Service/v1/Ancillaries/Service-Holds/{holdId:long}` | موجود؛ آزادسازی Inventory |
| Cancel confirmed | POST | `/Service/v1/Ancillaries/Service-Holds/{holdId:long}/Cancellations` | موجود؛ آزادسازی Local در Cancellation مجاز |
| Release one held unit | DELETE | `/Service/v1/Ancillaries/Service-Holds/{holdId:long}/Units/{unitId:long}` | **جدید** |
| Cancel one confirmed unit | POST | `/Service/v1/Ancillaries/Service-Holds/{holdId:long}/Units/{unitId:long}/Cancellations` | **جدید** |
| SeatMap commercial hold | POST | `/Service/v1/Ancillaries/SeatMaps/Holds` | موجود؛ `holdId?` برای append اضافه شود |

**عمداً نداریم:** `POST /Ancillaries/Hold` دوم، `POST /SeatMaps/Confirm` موازی، `POST /Options`، reservation-specific Price، `POST /Provider-Holds`، endpoint جدید عمومی Appendix/Append یا generic EMD.

## 2. POST Service-Holds — Request
```json
{
  "idempotencyKey": "ord-12001-anc-create-v1",
  "orderId": 12001,
  "reference": "ORD-12001-ANC",
  "requestedExpiresAt": "2026-10-26T12:30:00Z",
  "services": [
    {
      "orderServiceId": 501,
      "serviceDefinitionId": 2101,
      "provisionId": 3101,
      "travellerId": 701,
      "coverageScope": 2,
      "coveredFlightIds": [801, 802],
      "quantity": 2
    },
    {
      "orderServiceId": 502,
      "serviceDefinitionId": 2202,
      "provisionId": 3202,
      "travellerId": 702,
      "coverageScope": 1,
      "coveredFlightIds": [801],
      "quantity": 1
    }
  ]
}
```
### Header fields
| Field | Type | Required | Constraint / meaning |
|---|---|---|---|
| `idempotencyKey` | string | yes | طول 1..128؛ Scoped to یک create operation؛ same key+canonical request replay، متفاوت conflict |
| `orderId` | long | yes | `>0`؛ شناسه Ordering، نه SelectionSnapshot ID |
| `reference` | string | yes | طول 1..128؛ correlation مربوط به Order؛ NOT FlightFlow HoldId |
| `requestedExpiresAt` | DateTimeOffset | **yes** | UTC با offset معتبر؛ strictly آینده؛ اعتبار Reservation مستقل از Price Snapshot است |
| `services` | `AncillaryHoldServiceInput[]` | yes | 1..N؛ N فعلاً محدود به Validatorهای موجود؛ **سقف ثابت حدسی ایجاد نشود** |

### Unit fields — input اصلی
| Field | Type | Req | Rule & source |
|---|---|---|---|
| `orderServiceId` | long | yes | `>0`، داخل درخواست یکتا؛ Ordering مالک آن است؛ duplicate live رزرو ممنوع |
| `serviceDefinitionId` | long | yes | `>0`، از `Resolve.Definition.Id` قبول‌شده در Ordering؛ باید current authorized Definition باشد |
| `provisionId` | long | yes | `>0`، از `Resolve.Provision.Id`؛ FK متعلق به Definition؛ status قابل‌استفاده |
| `travellerId` | long? | conditional | برای `Order` باید null؛ برای `Sector/Portion/Journey` باید >0 و با Order match شده باشد |
| `coverageScope` | `ServiceCoverageScope` | yes | `Sector=1, Portion=2, Journey=3, Order=4`؛ مطابق Provision و Order |
| `coveredFlightIds` | long[] | yes | distinct، positive، canonical sorted در persistence؛ Sector=یک؛ Portion/Journey=تمام پروازهای مربوط؛ Order ممکن است [] |
| `quantity` | int | yes | >=1 و در Min/Max Provision؛ عدد نهایی قبول‌شده در Ordering |
| `inventoryFacts` | `InventoryHoldFacts?` | conditional | فقط وقتی `Local FlightWeight AcceptedWeight` یا `Local AirportSlot` واقعاً استفاده می‌شود؛ روی بقیه **نباید مقدار اضافی** فرستاده شود |

**عدم افزودن fields:** `offerId`, `selectionId`, `unitPrice`, `currencyId`, `taxes`, `PCC`, `seatNumber`, `pricingId`, `stockPoolId`, `providerKey` برای این Generic DTO نداریم. Ordering قبلاً Offer/Price را پذیرفته و ذخیره کرده است. Seat fields فقط از Facade اختصاصی SeatMap می‌آیند.

### `InventoryHoldFacts` — تنها داده جدید و کمینه
```json
{
  "acceptedWeightKgTotal": 16.5
}
```
یا برای Slot:
```json
{
  "timeWithOffset": "2026-10-26T16:00:00+03:30",
  "guestCount": 1
}
```
| Field | Type | When required | Meaning |
|---|---|---|---|
| `acceptedWeightKgTotal` | decimal? | فقط `FlightWeightConsumptionMode.AcceptedWeightKg` | **وزن کل پذیرفته‌شده برای همین Unit و Quantity، نه وزن هر قطعه**. precision طبق `InventoryRules.IsKg`، مثبت، با Order accepted selection reconcile شود |
| `timeWithOffset` | DateTimeOffset? | فقط `Local AirportSlot` | زمان appointment واقعی با timezone offset؛ ذخیره و مقایسه در UTC؛ از selection `timeWithOffset` معتبر سفارش |
| `guestCount` | int? | فقط `AirportSlot` | تعداد همراه، صفر یا بیشتر؛ base persons از Policy محاسبه می‌شود، نه از Front |

هیچ `JsonObject`، `SelectionOptions` بزرگ یا Nullable DTO ترکیبی 24 Variant به Hold اضافه نشود؛ Details کامل متعلق به Ordering هستند. API باید body field ناشناخته یا ناسازگار با pattern را رد کند؛ با راهکار serializer موجود، نه ساخت Framework جدید.

## 3. Hold response و GET — DTO موجود حفظ شود
Response HTTP `201` برای Create، `200` برای Read/Confirm/Release/Cancel. Seat append نیز `201` یا `200` طبق کنوانسیون Controller موجود؛ تست Contract باید یک status ثابت را pin کند (پیشنهاد `201` مانند Hold فعلی). **همه fields فعلی `AncillaryHoldResult` و `AncillaryHoldDto` حفظ شوند**؛ فیلدهای جدید اضافه نمی‌کنیم مگر ضرورت عملی اثبات شود.

نمونه کامل با fields موجود؛ `status` عدد واقعی enum:
```json
{
  "holdId": 9001,
  "idempotencyKey": "ord-12001-anc-create-v1",
  "orderId": 12001,
  "reference": "ORD-12001-ANC",
  "status": 1,
  "expiresAt": "2026-10-26T12:30:00Z",
  "units": [
    {
      "unitId": 9101,
      "orderServiceId": 501,
      "serviceDefinitionId": 2101,
      "provisionId": 3101,
      "travellerId": 701,
      "coverageScope": 2,
      "coveredFlightIds": [801, 802],
      "quantity": 2,
      "seatMapSelectionId": null,
      "seatNumber": null,
      "pricingId": null,
      "status": 1,
      "cancellationReasonCode": null
    }
  ]
}
```
`unitId` همان مرجع عملیاتی جزء است. تغییر Reference یا ایجاد ProviderUnitRef جعلی ممنوع. `StockPoolId` فعلی internal placeholder است و در پاسخ الزامی نیست؛ تخصیص واقعی باید از جدول Allocation قابل بازیابی باشد.

## 4. POST SeatMaps/Holds — همان facade اختصاصی، append optional
Header فعلی **دقیقاً حفظ** شود و فقط `holdId?: long` اضافه شود.
```json
{
  "holdId": 9001,
  "idempotencyKey": "ord-12001-seat-append-v1",
  "orderId": 12001,
  "orderCommercialVersion": 1,
  "reference": "ORD-12001-ANC",
  "requestedExpiresAt": "2026-10-26T12:30:00Z",
  "pointOfSaleId": 70,
  "currencyId": 47,
  "ticketedAtUtc": null,
  "seats": [
    {
      "orderServiceId": 503,
      "travellerId": 701,
      "seatMapSelectionId": "<a-real-selection-id-from-SeatMaps>",
      "passengerTypeCode": 1,
      "dateOfBirth": null,
      "flight": {
        "flightNumber": "W5-100",
        "marketingAirlineId": 1,
        "operatingAirlineId": 1,
        "originAirportId": 11,
        "destinationAirportId": 21,
        "departureAt": "2026-11-01T06:00:00Z",
        "arrivalAt": "2026-11-01T08:00:00Z",
        "aircraftId": 111,
        "cabinClassId": 1,
        "rbdId": 4
      },
      "fare": {
        "airFareId": 778,
        "airFareType": 1,
        "fareBasis": "YTEST",
        "bookingClass": "Y"
      },
      "couponOpenForUse": null
    }
  ]
}
```
**تذکر:** جزئیات `flight`, `fare`, `passengerTypeCode` در نمونه فقط برای نمایش شکل‌اند؛ **Enumها و required fieldهای واقعی** دقیقاً از `SeatHoldSelectionInput`, `SeatHoldFlightInput`, `SeatHoldFareInput` و Validator جاری خوانده شوند؛ نام فیلدی اضافه/کم نشود. `couponOpenForUse` فقط در PostTicketed اجباری است. `currencyId=47` مثال از Catalog توسعه است، hardcode ممنوع.

| Mode | `holdId` | Rule |
|---|---|---|
| Seat-only | null/omitted | Reservation تازه از Seat Unitها بساز؛ Header `idempotencyKey` همان key ریشه است |
| Mixed Hold | existing positive | `OrderId` و `Reference` و expiry با Reservation والد باید match شوند؛ اضافه‌کردن Seat Unitها فقط وقتی Root Held و زنده باشد؛ `idempotencyKey` اینجا **AppendKey** است و برای duplicate request exact replay کنترل می‌شود |

Response: همان `AncillaryHoldResult` با **همان holdId**؛ شامل Unitهای قبلی و Seatهای جدید. SeatMap facade اجازه call به FlightFlow، occupied seats و Local Seat Inventory ندارد. 

### SeatMap درخواست: Contract فیلدبه‌فیلد مطابق سورس موجود

| Field | CLR type | Required | شرط |
|---|---|---|---|
| `holdId` | long? | optional/new | null: Seat-only create؛ >0: append به Root موجود |
| `idempotencyKey` | string | yes | در create کلید Root، در append کلید عملیات Seat append؛ 1..128 |
| `orderId` | long | yes | >0؛ همان OrderId parent در append |
| `orderCommercialVersion` | int | yes | >0؛ Verifier فعلی برای Order context |
| `reference` | string | yes | 1..128؛ match با parent در append |
| `requestedExpiresAt` | DateTimeOffset | yes | آینده؛ append must equal parent expiry |
| `pointOfSaleId` | long | yes | >0؛ از Ordering؛ فقط commercial pricing context |
| `currencyId` | int | yes | >0؛ از Ordering؛ no arbitrary hardcode |
| `ticketedAtUtc` | DateTimeOffset? | no | وقتی post-ticketed، coupon data per seat لازم |
| `seats` | `SeatHoldSelectionInput[]` | yes | حداقل یک seat |

#### `SeatHoldSelectionInput`

| Field | CLR type | Required | Rule |
|---|---|---|---|
| `orderServiceId` | long | yes | >0 و یکتا، belongs to Order |
| `travellerId` | long | yes | >0، belongs to Order |
| `seatMapSelectionId` | string | yes | شناسه واقعی و قابل Decode SeatMap، نه Snapshot `s_` |
| `passengerTypeCode` | `PassengerTypeCode` | yes | موجود enum، `ADT=1`, `CHD=27`, `INF` مقدار خودش؛ باید با Traveller match کند |
| `dateOfBirth` | DateOnly? | no | در صورت نیاز به قواعد Age؛ Verifier موجود حکم می‌کند |
| `flight` | `SeatHoldFlightInput` | yes | facts کامل پرواز طبق verifier |
| `fare` | `SeatHoldFareInput?` | conditional | اگر دسته تجاری قیمت Seat به AirFare وابسته است؛ Verifier موجود تصمیم می‌گیرد |
| `couponOpenForUse` | bool? | conditional | اگر `ticketedAtUtc` وجود دارد، non-null طبق Validator فعلی |

#### `SeatHoldFlightInput`

| Field | CLR type | Required / validation |
|---|---|---|
| `flightNumber` | string | yes، nonempty، max16 |
| `marketingAirlineId` | int | yes، >0 |
| `operatingAirlineId` | int | yes، >0 |
| `originAirportId` | int | yes، >0 |
| `destinationAirportId` | int | yes، >0 |
| `departureAt`, `arrivalAt` | DateTimeOffset | yes؛ زمان‌های واقعی Flight؛ صحت تجاری توسط verifier |
| `aircraftId` | int | yes، >0 |
| `cabinClassId` | int | yes، >0 |
| `rbdId` | long? | optional، اگر present >0 |

#### `SeatHoldFareInput`

| Field | CLR type | Validation |
|---|---|---|
| `airFareId` | long | >0 |
| `airFareType` | `AirFareType` | enum defined |
| `fareBasis` | string? | max64 |
| `bookingClass` | string? | max2 |

**منبع دقیق:** `HoldSeatSelectionsRequest`, `SeatHoldSelectionInput`, `SeatHoldFlightInput`, `SeatHoldFareInput` و Validatorهای آن‌ها در شاخه Pin‌شده؛ داده‌های نمونه بالا شناسه‌های تستی هستند. SeatMap فروش تجاری را verify می‌کند، نه occupied physical seat را.

## 5. Unit release و Cancel
```http
DELETE /Service/v1/Ancillaries/Service-Holds/9001/Units/9102
POST /Service/v1/Ancillaries/Service-Holds/9001/Units/9103/Cancellations
Content-Type: application/json
{"reasonCode":"CUSTOMER_REQUEST"}
```
- Release یک Unit: فقط `Held` زنده → `Released`؛ retry همان Unit released بدون اثر؛ `Confirmed` باید 409 بدهد.
- Cancel یک Unit: فقط `Confirmed` → `Cancelled`؛ retry Cancelled بدون اثر؛ `Held` باید 409 بدهد؛ reasonCode طول 1..50 مانند Aggregate موجود.
- هر دو response: همان `AncillaryHoldResult` کامل (بدون DTO تازه)، Status Unitها صریح؛ status والد طبق Master.
- اگر `unitId` متعلق به holdId نیست: `404`، نه اینکه از Reservation دیگری Unit را حذف کنیم.

## 6. Confirm/Release/Cancel whole
- `POST .../Confirmations` body ندارد؛ فقط Unitهای Held فعال Confirm می‌شوند؛ Released قبلی untouched.
- `DELETE .../Service-Holds/{id}` body ندارد؛ فقط Heldهای فعال Released می‌شوند؛ Confirmed ممنوع. اگر هیچ Held و همه قبلاً Released هستند replay no-op.
- `POST .../Cancellations` `{ "reasonCode":"CUSTOMER_REQUEST" }`؛ فقط Confirmedهای فعال Cancelled؛ Released قبلی untouched. `Cancelled` replay no-op.
- Confirm/Release/Cancel و Unit commands باید تحت کنترل concurrency واحد Reservation+Inventory اجرا شوند، نه تنها `Load → Save` آزاد.

## 7. خطاها و Retry؛ بدون ساخت Error Framework تازه
| Failure | HTTP | Required behavior |
|---|---|---|
| Validation: invalid/missing/extra fact | 400 or 422 مطابق Validator existing | هیچ Unit/Allocation نوشته نشود |
| Definition, Provision, Order/Unit not found | 404 مطابق ExceptionFactory موجود | fail closed |
| expired or invalid state | 409/410 بر اساس mapping موجود | status حقیقت را حفظ کند |
| idempotencyKey same / different body | 200/201 replay / 409 conflict | هیچ تخصیص اضافه در replay |
| Local shortage (count/weight/slot) | 409 / domain-specific code | همه Local allocations همان operation rollback شوند |
| source closed / policy inactive / supplier unsupported | 409/422 matching existing error conventions | نه Held جعلی و نه fallback unlimited |
| concurrent final unit | یک 201، دیگری conflict | موجودی منفی هرگز |

**ایجنت مجاز نیست کد خطای عددی حدسی بسازد.** Gate 0 `ExceptionFactory`/REST mapping را ممیزی می‌کند؛ تنها خطای جدید واقعاً لازم در همان pattern پروژه تعریف و در ماتریس تثبیت شود.

## 8. Compatibility/No scope creep
- مسیرهای 8گانه فوق canonical هستند؛ مسیر قدیمی با body قدیمی تنها در صورتی حفظ می‌شود که Validator جدید هنوز روی آن سازگار است؛ mandatory ExpiresAt برای ایجاد Hold این فاز یک تغییر رفتار عمدی است و باید در گزارش breaking-change ثبت شود.
- `Service-Holds` / `SeatMaps/Holds` پاسخ یکسان دارند؛ APIهای Shopping/Price/Resolve و SeatMap pricing خارج Scope.
- API عمومی Front/Backoffice رزرو جدید نسازید؛ فقط Service-only.
