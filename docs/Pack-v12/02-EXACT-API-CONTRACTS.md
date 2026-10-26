# 02 — قرارداد دقیق HTTP بر اساس HEAD v11

**پیش‌فرض:** همه IDهای `long` در نمونه‌ها به‌شکل JSON string آمده‌اند (serializer پروژه اعداد را هم می‌پذیرد)، Enumها با نام serialize می‌شوند. زمان‌های نمونه صرفاً illustrative هستند و برای درخواست زنده باید Future باشند. مبنای واقعی: `ServiceController.cs`, `HoldAncillaryServicesRequest.cs`, `HoldSeatSelectionsRequest.cs`, `AncillaryHoldReceipt.cs` و `docs/Pack-v11/02-EXACT-RESERVATION-API-AND-DTOS.md`.

## A. Create Mixed Hold
`POST /Service/v1/Ancillaries/Service-Holds` → `201 Created` و Receipt.

```json
{
  "idempotencyKey": "ORD-12001-H1",
  "orderId": "12001",
  "reference": "ORD-12001",
  "requestedExpiresAt": "2026-11-05T10:30:00Z",
  "baggages": [
    {"variantCode":"A01","products":[{"provisionId":"701","units":[{"orderServiceId":"501","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]},"selected":{"quantity":2}}]}]},
    {"variantCode":"A03","products":[{"provisionId":"706","units":[{"orderServiceId":"506","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]},"selected":{"quantity":1,"options":{"baggage":{"bagRef":"BAG-01","measuredWeightKg":25.5}}}}]}]}
  ],
  "meals": [{"variantCode":"A11","products":[{"provisionId":"702","units":[{"orderServiceId":"502","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]}}]}]}],
  "assistedTravel": [{"variantCode":"A15","products":[{"provisionId":"703","units":[{"orderServiceId":"503","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]},"selected":{"options":{"assistedTravel":{"assistanceSsrCode":"WCHR"}}}}]}]}],
  "airportServices": [{"variantCode":"A20","products":[{"provisionId":"704","units":[{"orderServiceId":"504","unit":{"travellerId":"1001"},"selected":{"options":{"airportService":{"timeWithOffset":"2026-11-04T08:00:00+03:30","guestCount":1}}}}]}]}]
}
```

**توضیح مهم:** A03 با گزینه‌ی واقعی فقط در Policy supported مثل `PerPiece` قابل Hold است؛ `AcceptedWeightKg/PerKilogram` فعلاً GATED. محصول و Airport idهای مثال باید در محیط واقعی وجود داشته باشند، و `A20` برای Appointment باید Provision سازگار داشته باشد. JSON مثال Contract معتبر است، نه ادعای موفقیت با ID نمونه.

| Field | CLR | الزام و منبع |
|---|---|---|
| `idempotencyKey` | string [1..128] | ضروری؛ replay بدون ایجاد Unit جدید؛ تغییر درخواست → 409 |
| `orderId` | long positive | ضروری؛ فقط Caller correlation، بدون استعلام Ordering |
| `reference` | string [1..128] | ضروری؛ Correlation نه PNR ساختگی |
| `requestedExpiresAt` | DateTimeOffset future | ضروری؛ TTL خود Reservation |
| `baggages/meals/assistedTravel/airportServices/priorityServices/connectivity/upgrades` | nullable arrays | فقط خانواده دارای انتخاب؛ مجموع Units حداقل 1 |
| `<family>[].variantCode` | string | عضو Variant registry و معادل کد Definition resolved |
| `products[].provisionId` | long positive | ضروری؛ locator واقعی Resolve، نه `productKey` نمایشی |
| `products[].units` | array nonempty | حداقل 1؛ هر `orderServiceId` در کل درخواست یکتا |
| `units[].orderServiceId` | long positive | ضروری؛ Caller owns identity |
| `units[].unit.travellerId` | long positive | بسته به CoverageScope محلی؛ اگر `unit` فرستاده شد، `travellerId` ضروری |
| `units[].coverage.flightIds` | array<long> | بسته به Scope؛ positive/distinct؛ Sector دقیقاً 1 |
| `units[].selected.quantity` | int>0 | در صورت انتخاب تعداد؛ اگر ثابت از Provision derive؛ `0` هیچ Unit به Hold ندهد |
| `units[].selected.options` | Family-specific | فقط option همان family؛ کلید بیگانه رد می‌شود |

### Option DTOهای فعلی — فقط خانوادهٔ مرتبط

| Family | Options shape | Variant/قاعده |
|---|---|---|
| `baggages` | `{"baggage":{"bagRef":"...","measuredWeightKg":25.5}}` | A03؛ BagRef/وزن واقعی |
| `baggages` | `{"baggage":{"dimensions":{"lengthCm":120,"widthCm":80,"heightCm":60},"weightKg":18}}` | A04/A06 بر اساس Fixed Fields/Definition |
| `assistedTravel` | `{"assistedTravel":{"assistanceSsrCode":"WCHR"}}` | A15/A16 |
| `airportServices` | `{"airportService":{"timeWithOffset":"2026-11-04T08:00:00+03:30","guestCount":1}}` | A20/A21/A22، شرطی طبق Appointment/Facility Rule |
| `priorityServices` | `{"priority":{"optIn":true}}` | A23؛ false انتخاب نیست |
| `connectivity` | `{"connectivity":{"deviceCount":2}}` | A24 اگر DeviceCount انتخاب‌پذیر است |
| `meals`, `upgrades` | **بدون `options`**؛ فقط `quantity` در صورت نیاز | MealCode/MenuItemRef و ToCabinId از Definition ثابت و در Hold snapshot |
| `pets` | **هیچ Property در Create وجود ندارد** | A13/A14 GATED و درخواست باید 400 بخورد |

### مثال‌های مستقل کوچک
A12 Meal: `{"meals":[{"variantCode":"A12","products":[{"provisionId":"710","units":[{"orderServiceId":"510","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]},"selected":{"quantity":2}}]}]}]}`؛ `menuItemRef` از Definition snapshot می‌شود.

A04 Baggage: `{"baggages":[{"variantCode":"A04","products":[{"provisionId":"711","units":[{"orderServiceId":"511","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]},"selected":{"options":{"baggage":{"dimensions":{"lengthCm":100,"widthCm":75,"heightCm":55}}}}}]}]}]}`.

A23 Priority: `{"priorityServices":[{"variantCode":"A23","products":[{"provisionId":"712","units":[{"orderServiceId":"512","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]},"selected":{"options":{"priority":{"optIn":true}}}}]}]}]}`.

A24 Wi-Fi: `{"connectivity":[{"variantCode":"A24","products":[{"provisionId":"713","units":[{"orderServiceId":"513","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]},"selected":{"options":{"connectivity":{"deviceCount":2}}}}]}]}]}`.

**Pet A13/A14، A17 Medical، A18 Bassinet و A19 UMNR:** نمونه موفق Hold جعل نمی‌شود؛ تا GATE واقعی فقط خطای مشخص 400/16419 مطابق مسیر ورود. A10 فقط `FixedAncillary` با `toCabinId` ثابت از Definition؛ External Quote GATED.

## B. پاسخ مشترک تمام Writeها
```json
{
  "holdId": "9001",
  "reference": "ORD-12001",
  "status": "Held",
  "expiresAt": "2026-11-05T10:30:00+00:00",
  "units": [
    {"unitId":"9101","orderServiceId":"501","status":"Held"},
    {"unitId":"9102","orderServiceId":"502","status":"Held"},
    {"unitId":"9103","orderServiceId":"503","status":"Held"}
  ]
}
```
`holdId/unitId` ساخت Ancillary هستند؛ `orderServiceId` Correlation caller. پاسخ Write، سرویس Price و ProviderRef جعلی یا SourceId داخلی را Echo نمی‌کند؛ Detail با GET خوانده می‌شود.

## C. GET تاریخی
`GET /Service/v1/Ancillaries/Service-Holds/{holdId:long}` → `200`:
```json
{
  "holdId":"9001","orderId":"12001","reference":"ORD-12001","status":"Confirmed","expiresAt":"2026-11-05T10:30:00+00:00",
  "baggages":[{"variantCode":"A03","products":[{"provisionId":"706","serviceDefinitionRef":"BAG_OVERWEIGHT","units":[{"unitId":"9101","orderServiceId":"506","status":"Confirmed","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]},"selected":{"quantity":1,"options":{"baggage":{"bagRef":"BAG-01","measuredWeightKg":25.5}}}}]}]}],
  "meals":[{"variantCode":"A11","products":[{"provisionId":"702","serviceDefinitionRef":"MEAL_VGML","details":{"mealCode":"VGML"},"units":[{"unitId":"9102","orderServiceId":"502","status":"Confirmed","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]},"selected":{"quantity":1}}]}]}],
  "airportServices":[{"variantCode":"A20","products":[{"provisionId":"704","serviceDefinitionRef":"LOUNGE_IKA","details":{"airportId":2},"units":[{"unitId":"9103","orderServiceId":"504","status":"Cancelled","cancellationReasonCode":"CUSTOMER_REQUEST","unit":{"travellerId":"1001"},"selected":{"quantity":1,"options":{"airportService":{"timeWithOffset":"2026-11-04T08:00:00+03:30","guestCount":1}}}}]}]}]
}
```
صرفاً **نمونه Contract** و مقادیر illustrative. `products[].details` ثابت بر اساس Family شامل Baggage `equipmentKind`، Meal `mealCode/menuItemRef`، Upgrade `toCabinId`، Airport `airportId/facilityId` است؛ Priority بدون گزینه ذخیره‌ای تنها با Unit Held و Variant معلوم می‌شود؛ Connectivity `deviceCount` و `serviceDefinitionRef` Plan. `seats[]` در GET با `seatNumber` و `seatMapSelectionId` فقط در Seat Unit نمایش داده می‌شود. GET از Reservation DB بدون Price/Definition live read.

## D. Confirm / Release / Cancel / Expiry

| Method/Route | Body/Query | Success | Semantics |
|---|---|---|---|
| `POST /Service/v1/Ancillaries/Service-Holds/9001/Confirmations` | بدون body | 200 Receipt | همه Held فعال Confirmed؛ Released دست‌نخورده؛ دوباره Confirm idempotent؛ EMD صادر نمی‌کند |
| `DELETE /Service/v1/Ancillaries/Service-Holds/9001` | بدون `unitIds` | 200 Receipt | **آگاهانه همه** Held را Release می‌کند |
| `DELETE /Service/v1/Ancillaries/Service-Holds/9001?unitIds=9101&unitIds=9102` | nonempty distinct IDs | 200 Receipt | Partial و Atomic؛ confirmed/foreign/invalid → خطا و rollback |
| `POST /Service/v1/Ancillaries/Service-Holds/9001/Cancellations` | `{"reasonCode":"CUSTOMER_REQUEST","unitIds":[9101]}` | 200 Receipt | فقط Confirmed Unitها؛ تمام انتخاب یا انتخابی؛ Reason immutable بر Replay |
| `POST /Service/v1/Ancillaries/Service-Holds/9001/Cancellations` | `{"reasonCode":"CUSTOMER_REQUEST","unitIds":null}` | 200 Receipt | Cancel تمام Confirmedها |
| `GET` بعد از deadline | - | 200 with effective Expired | Logical stock free حتی قبل از poller؛ Poller status persisted |

**P0 Contract correction (v12 target; not yet implemented):** اگر `unitIds` key در Release Query **حاضر** باشد ولی collection تهی/blank/invalid باشد، HTTP **400** و هیچ تغییر DB. فقط `unitIds` واقعاً absent یعنی All. کد HEAD فعلاً برای `Length:0` آن را null می‌کند؛ حذف این conflation الزامی است. در Cancel، `unitIds:[]` نیز باید fail-closed بماند (در Root `Select` رد می‌شود). Replay با کلید یکسان/Body تغییر کرده → 409؛ Cancel same reason no-op، reason متفاوت 409.

## E. SeatMap Hold / Append — ورودی موجود و freeze
`POST /Service/v1/Ancillaries/SeatMaps/Holds` → `201` همان Receipt:
```json
{
  "idempotencyKey":"ORD-12001-SEAT-1","orderId":"12001","orderCommercialVersion":1,
  "reference":"ORD-12001","requestedExpiresAt":"2026-11-05T10:30:00Z",
  "pointOfSaleId":"1","currencyId":47,"ticketedAtUtc":null,"holdId":"9001",
  "seats":[{
    "orderServiceId":"505","travellerId":"1001","seatMapSelectionId":"REAL-SEATMAP-SELECTION-ID",
    "passengerTypeCode":"ADT","dateOfBirth":null,
    "flight":{"flightNumber":"W5-100","marketingAirlineId":1,"operatingAirlineId":1,"originAirportId":11,"destinationAirportId":21,"departureAt":"2026-11-05T06:00:00Z","arrivalAt":"2026-11-05T08:00:00Z","aircraftId":111,"cabinClassId":1,"rbdId":"4"},
    "fare":{"airFareId":"778","airFareType":1,"fareBasis":"YTEST","bookingClass":"Y"},
    "couponOpenForUse":null
  }]
}
```
`holdId=null` برای Seat-only Root تازه؛ برای Append ID Root موجود و Header متناسب. `seatMapSelectionId` باید از خروجی واقعی SeatMap باشد و verifier تجاری آن را Resolve کند؛ مقدار مثال صرفاً placeholder است. `orderCommercialVersion`, `pointOfSaleId`, `currencyId`, Flight facts و Fare context در این Endpoint موجودند؛ به Generic Hold منتقل نشوند. فیزیک صندلی در FlightFlow از Ordering اجرا می‌شود، نه در این Endpoint. Endpoint `SeatMaps/Resolve` جدا و frozen است.

## F. خطاهای موجود / Status
- 400: unknown JSON member مانند `services`, `pets`, `coverageScope`, `inventoryFacts`، یا Shape invalid؛ 422 (طبق Exception mapping موجود) invalid business fields `16402`.
- 409: `16403` idempotency conflict؛ `16407` expired؛ `16413` cancelled with different reason؛ `16417` insufficient inventory؛ `16418` foreign Unit؛ `16419` not reservable (ممکن است HTTP mapping دقیق بسته به handler متفاوت باشد).
- 404: Hold ناشناخته؛ Retire با Active Usage طبق گزارش v11 رد می‌شود.
- بدون خطای تازه و تغییر taxonomy مگر سورس/تست اثبات کند. Gatedها هیچ `Held` جعلی برنمی‌گردانند.
