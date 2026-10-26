# 03 — دیکشنری کامل A01–A24؛ چه چیزی از AncillaryOffers/Resolve به Hold منتقل شود؟

**Source of field names:** `AncillaryVariant.cs`, `CustomerSelectionContract.cs`, `SelectionOptionsInput`/six `*SelectionOptions.cs`, `ServiceResolvedSelectionDto<T>`, `*DetailsDto`. Status ثابتِ unsupported توسط Pack v10 و Source v9 به‌عنوان Gate ثبت شده است؛ هیچ Field invented برای «پرکردن جدول» ایجاد نشود. Options در Hold *فقط* همان `selected.options.{key}` موجود در Resolve است؛ همه نام‌ها camelCase.

| Variant | Family در Request | حداقل `unit` / `coverage` | `selected` کمینه / snapshot تخصصی | Inventory / وضعیت |
|---|---|---|---|---|
| **A01 ExtraCheckedBag** | `baggages` | `unit.travellerId`, `coverage.flightIds` بسته به Provision | `quantity` تعداد انتخابی؛ allowance/MaxKg ثابت از Definition در Hold snapshot اگر برای fulfillment لازم است | Count/FixedKg/Unlimited با Policy فعال |
| **A02 ExtraWeightPackage** | `baggages` | Traveller + FlightIds | `quantity`؛ `packageProductRef` در CustomerSelectionContract هست ولی در `BaggageSelectionOptions` موجود **نیست**؛ فقط وقتی Package یکتا از Definition استخراج شود | selection متغیر نامشخص → **GATED**؛ Fixed Source قابل اثبات → مجاز |
| **A03 Overweight** | `baggages` | Traveller + FlightIds | `options.baggage.{bagRef,measuredWeightKg}`؛ اندازه پذیرفته‌شده immutable | `AcceptedWeightKg/PerKilogram` در v10 پشتیبانی نمی‌شود؛ برای چنین Policy **GATED** |
| **A04 Oversize** | `baggages` | Traveller + FlightIds | `options.baggage.dimensions` با `{lengthCm,widthCm,heightCm}`؛ no `dimensionsCm` wire جدید | Local Count/Unlimited با Rules موجود |
| **A05 CabinBag** | `baggages` | Traveller + FlightIds | Qty در صورت انتخابی؛ سایر Facts از Definition | Count/Unlimited یا FixedKg بسته به Policy |
| **A06 SpecialEquipment** | `baggages` | Traveller + FlightIds | `options.baggage.dimensions?`, `weightKg?`; `equipmentKind` در Contract هست اما در `BaggageSelectionOptions` موجود نیست؛ از Definition فقط اگر یکتا | تجهیزات با گزینه متغیر فاقد Source → **GATED** |
| **A07 StandardSeat** | `SeatMaps/Holds` اختصاصی | Seat verifier موجود | `seatMapSelectionId`, SeatNumber و PricingId **از همان Verifier** | تجاری بدون Inventory allocation؛ فیزیکی در FlightFlow |
| **A08 PreferredSeat** | `SeatMaps/Holds` | مثل A07 | مثل A07 | تجاری، ۰ FlightFlow calls |
| **A09 ExtraSeat** | فعلاً generic ندارد | — | `purpose` واقعی و قرارداد ExtraSeat موجود نیست | **GATED**؛ نه Fake Seat Hold |
| **A10 CabinUpgrade** | `upgrades` | Traveller + FlightId | `targetCabinId/quoteRef` در `Resolve/Details` و Contract نیازمند Quote واقعی | **GATED** برای ExternalQuote/Approval بدون ACL؛ هیچ Quote فرضی |
| **A11 FreeSpecialMeal** | `meals` | Traveller + FlightId | `mealCode` **فقط اگر یکتا و در Definition/Details حاضر** در زمان Hold snapshot؛ Selected omitted اگر fixed | Unlimited/Local طبق Policy |
| **A12 PaidPreorderMeal** | `meals` | Traveller + FlightId | `quantity` اگر انتخابی؛ `menuItemRef` اگر fixed در Definition/Details به‌طور یکتا snapshot؛ در `SelectionOptionsInput` فعلی Family Meal ندارد | اگر Menu متغیر در یک Provision و Resolve آن را حمل نمی‌کند **GATED** |
| **A13 PetInCabin** | `pets` | Traveller + FlightIds | `options.pet.animalType`, `combinedWeightKg`, `carrierDimensions`, `otherAnimalCode?`,`animalAgeWeeks?`,`documentAcknowledgements?` | طبق Policy؛ Supplier/SubjectToConfirmation فاقد Adapter **GATED** |
| **A14 PetInHold** | `pets` | Traveller + FlightIds | مثل A13 + `sizeBracket?` وقتی Rule لازم می‌داند | Local/Unlimited supported; Provider unsupported |
| **A15 Wheelchair** | `assistedTravel` | Traveller + FlightId | `options.assistedTravel.assistanceSsrCode`؛ WCHR/WCHS با انتخاب واقعی | Unlimited/Local مطابق Policy |
| **A16 DisabilityAssistance** | `assistedTravel` | Traveller + FlightId | همان `assistanceSsrCode` | مثل A15 |
| **A17 MedicalEquipment** | `assistedTravel` | Traveller + FlightId | Evidence/Oxygen/FREMEC در `CustomerSelectionContract` اما `AssistedTravelSelectionOptions` فعلی کامل نیست | **GATED** تا قرارداد و حفاظت داده at-rest تأیید شود |
| **A18 Bassinet** | `assistedTravel` | ارائه `unit.travellerId` طبق Scope واقعی + FlightId | `options.assistedTravel.infantRef`, `guardianRef`؛ بدون استعلام Order؛ قواعد محلی | اگر Facts و Rule کافی نیست **GATED** |
| **A19 UnaccompaniedMinor** | `assistedTravel` | Traveller + Coverage واقعی | guardian contacts در Price Selection موجود نیست؛ ناسازگاری bool/text قبلی | **GATED**؛ نه placeholder contacts |
| **A20 Lounge** | `airportServices` | Traveller اگر Pax-scoped؛ FlightIds فقط اگر Provision/Source لازم | `options.airportService.timeWithOffset?`, `guestCount?`؛ airport/facility ثابت از Definition/Policy | AirportSlot نیازمند appointment دقیق و available slices؛ یا Unlimited |
| **A21 FastTrack** | `airportServices` | مثل A20 | همان optionهای AirportService | طبق Policy |
| **A22 CIP MeetAssist** | `airportServices` | مثل A20 | همان؛ service component codes fixed از Definition snapshot اگر fulfillment لازم | طبق Policy |
| **A23 PriorityBoardingCheckin** | `priorityServices` | Traveller + FlightId | `options.priority.optIn=true` اگر selected typed؛ می‌توان false را عدم خرید دانست، نه Hold | طبق Policy، no separate toggle service |
| **A24 OnboardWifi** | `connectivity` | Traveller + FlightId (یا Scope واقعی) | `options.connectivity.deviceCount?`; `planCode` اگر یکتا و fixed در Definition snapshot | چند Plan selectable بدون تمایز Source → **GATED** |

## الف. قرارداد `selected.options` در پرواز واقعی (بدون Generic Nullable Giant)

نمونه `A15`:
```json
{"variantCode":"A15","products":[{"provisionId":703,"units":[{"orderServiceId":503,"unit":{"travellerId":1001},"coverage":{"flightIds":[801]},"selected":{"options":{"assistedTravel":{"assistanceSsrCode":"WCHR"}}}}]}]}
```

نمونه `A13` (به‌صورت Family `pets[]` و enum AnimalType **نمایشی**):
```json
{"variantCode":"A13","products":[{"provisionId":705,"units":[{"orderServiceId":505,"unit":{"travellerId":1001},"coverage":{"flightIds":[801]},"selected":{"options":{"pet":{"animalType":1,"combinedWeightKg":7.5,"carrierDimensions":{"lengthCm":45,"widthCm":30,"heightCm":25}}}}}]}]}
```
`animalType:1` فقط جایگزین نمونه است؛ مقدار enum واقعی باید از `PetAnimalType`/serializer سورس به‌دست آید، نه اینکه در Client Hardcode شود.

نمونه `A20`:
```json
{"variantCode":"A20","products":[{"provisionId":704,"units":[{"orderServiceId":504,"unit":{"travellerId":1001},"selected":{"options":{"airportService":{"timeWithOffset":"2026-10-27T08:00:00+03:30","guestCount":1}}}}]}]}
```

## ب. snapshot چه چیزهایی را باید نگه دارد؟
- **Selected values**: همه مقادیر متغیر و تأثیرگذار در اجرای خدمت (SSR، Pet، BagRef، Dimensions، Time/GuestCount، DeviceCount). حفظ decimals و DateTimeOffset.
- **Fixed fulfillment facts**: فقط Fact ثابت مؤثر بر انجام خدمت (MealCode/MenuItemRef، PlanCode، AirportId/FacilityId، Pet transport mode، service components) اگر Source واقعی فعال مقدار آن را نشان می‌دهد. *نه* عکس Definition کامل یا Object pricing.
- **Commercial boundaries**: `VariantCode`, `ProvisionId`, derived `CoverageScope`, `Quantity`, `TravellerId`, `CoveredFlightIds` و status/refs.
- Unit نه قیمت/مالیات/EMD و نه Outcome مالی را کپی کند. آن‌ها متعلق Order هستند.

## ج. اگر Source فاقد داده است
- اگر `Resolve.details` یا `Definition` یک مقدار انتخابی را **یکتا** تعیین نمی‌کند و Selected.Options هم آن را حمل نمی‌کند: **Hold رد شود**؛ هیچ `XxxRef` جدید در Hold برای پرکردن نقص Price خلق نشود.
- اگر Source و قواعد مربوط Variant قابل‌اثبات است، فقط همان Variant و Pattern را فعال کنید؛ گیت دیگر Variantها مانع عبور A01/Seat/A15/A20 واقعی نشود.
- *Supported* = `Hold→SQL→GET` مقادیر کامل و Stock سالم؛ صرف داشتن Record/Variant در Registry نشانه پیاده‌شدن نیست.
