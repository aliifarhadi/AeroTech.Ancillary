# 03 — دیکشنری کامل A01–A24؛ چه چیزی از AncillaryOffers/Resolve به Hold منتقل شود؟

**Source of field names:** `AncillaryVariant.cs`, `CustomerSelectionContract.cs`, the six `*SelectionOptions.cs` records (the Hold types them per family, not through the `SelectionOptionsInput` union, Owner R1), `ServiceResolvedSelectionDto<T>`, `*DetailsDto`. Status ثابتِ unsupported توسط Pack v10 و Source v9 به‌عنوان Gate ثبت شده است؛ هیچ Field invented برای «پرکردن جدول» ایجاد نشود. Options در Hold *فقط* همان `selected.options.{key}` موجود در Resolve است؛ همه نام‌ها camelCase.

| Variant | Family در Request | حداقل `unit` / `coverage` | `selected` کمینه / snapshot تخصصی | Inventory / وضعیت |
|---|---|---|---|---|
| **A01 ExtraCheckedBag** | `baggages` | `unit.travellerId`, `coverage.flightIds` بسته به Provision | `quantity` تعداد انتخابی؛ allowance/MaxKg ثابت از Definition در Hold snapshot اگر برای fulfillment لازم است | Count/FixedKg/Unlimited با Policy فعال |
| **A02 ExtraWeightPackage** | `baggages` | Traveller + FlightIds | `quantity`; the package is the Definition's `serviceDefinitionRef` (`TypedSelectionForm.WithFixedChoices`), snapshotted at Hold | Supported (Owner Q1) |
| **A03 Overweight** | `baggages` | Traveller + FlightIds | `options.baggage.{bagRef,measuredWeightKg}`؛ اندازه پذیرفته‌شده immutable | `AcceptedWeightKg/PerKilogram` در v10 پشتیبانی نمی‌شود؛ برای چنین Policy **GATED** |
| **A04 Oversize** | `baggages` | Traveller + FlightIds | `options.baggage.dimensions` با `{lengthCm,widthCm,heightCm}`؛ no `dimensionsCm` wire جدید | Local Count/Unlimited با Rules موجود |
| **A05 CabinBag** | `baggages` | Traveller + FlightIds | Qty در صورت انتخابی؛ سایر Facts از Definition | Count/Unlimited یا FixedKg بسته به Policy |
| **A06 SpecialEquipment** | `baggages` | Traveller + FlightIds | `options.baggage.dimensions?`, `weightKg?`; `equipmentKind` fixed by `Definition.Baggage.EquipmentKind`, snapshotted in product `details` | Supported (Owner Q1) |
| **A07 StandardSeat** | `SeatMaps/Holds` اختصاصی | Seat verifier موجود | `seatMapSelectionId`, SeatNumber و PricingId **از همان Verifier** | تجاری بدون Inventory allocation؛ فیزیکی در FlightFlow |
| **A08 PreferredSeat** | `SeatMaps/Holds` | مثل A07 | مثل A07 | تجاری، ۰ FlightFlow calls |
| **A09 ExtraSeat** | فعلاً generic ندارد | — | `purpose` واقعی و قرارداد ExtraSeat موجود نیست | **GATED**؛ نه Fake Seat Hold |
| **A10 CabinUpgrade** | `upgrades` | Traveller + FlightId | `FixedAncillary`: no selected options; `toCabinId` fixed by `Definition.Upgrade.ToCabinId`, snapshotted in product `details` | Supported for `FixedAncillary` only; a quote (`quoteRef`, external quote) is **GATED** (Owner Q1) |
| **A11 FreeSpecialMeal** | `meals` | Traveller + FlightId | `selected` omitted; `mealCode` fixed by `Definition.Meal.MealCode`, snapshotted in product `details` | Unlimited/Local per Policy; supported |
| **A12 PaidPreorderMeal** | `meals` | Traveller + FlightId | `quantity` when chosen; `menuItemRef` fixed by `Definition.Meal.MenuItemRef`, snapshotted in product `details` | Supported (Owner Q1) |
| **A13 PetInCabin** | `pets` | — | — | **GATED** (Owner Q1): the Definition forces `SubjectToConfirmation`; the `pets` array is not a Hold member (Owner R1 Q3), so a request with pets fails binding (400) before any write |
| **A14 PetInHold** | `pets` | — | — | **GATED** (Owner Q1), as A13 |
| **A15 Wheelchair** | `assistedTravel` | Traveller + FlightId | `options.assistedTravel.assistanceSsrCode`؛ WCHR/WCHS با انتخاب واقعی | Unlimited/Local مطابق Policy |
| **A16 DisabilityAssistance** | `assistedTravel` | Traveller + FlightId | همان `assistanceSsrCode` | مثل A15 |
| **A17 MedicalEquipment** | `assistedTravel` | Traveller + FlightId | Evidence/Oxygen/FREMEC در `CustomerSelectionContract` اما `AssistedTravelSelectionOptions` فعلی کامل نیست | **GATED** تا قرارداد و حفاظت داده at-rest تأیید شود |
| **A18 Bassinet** | `assistedTravel` | — | `infantRef`/`guardianRef` are traveller refs scoped to the offer | **GATED** (Owner Q1); `infantRef`/`guardianRef` are not Hold members (Owner R1 Q3) |
| **A19 UnaccompaniedMinor** | `assistedTravel` | Traveller + Coverage واقعی | guardian contacts در Price Selection موجود نیست؛ ناسازگاری bool/text قبلی | **GATED**؛ نه placeholder contacts |
| **A20 Lounge** | `airportServices` | Traveller if Pax-scoped; FlightIds only if the Provision or the Source needs them | `options.airportService.timeWithOffset?`, `guestCount?`; `airportId` from the Definition and `facilityId` from the Provision rule or the Definition, snapshotted in product `details` | AirportSlot needs an exact appointment and available slices; or Unlimited |
| **A21 FastTrack** | `airportServices` | مثل A20 | همان optionهای AirportService | طبق Policy |
| **A22 CIP MeetAssist** | `airportServices` | as A20 | as A20 | per Policy |
| **A23 PriorityBoardingCheckin** | `priorityServices` | Traveller + FlightId | `options.priority.optIn=true` اگر selected typed؛ می‌توان false را عدم خرید دانست، نه Hold | طبق Policy، no separate toggle service |
| **A24 OnboardWifi** | `connectivity` | Traveller + FlightId (or the real scope) | `options.connectivity.deviceCount?`; the plan is the Definition's `serviceDefinitionRef` | Supported (Owner Q1) |

## الف. قرارداد `selected.options` در پرواز واقعی (بدون Generic Nullable Giant)

نمونه `A15`:
```json
{"variantCode":"A15","products":[{"provisionId":703,"units":[{"orderServiceId":503,"unit":{"travellerId":1001},"coverage":{"flightIds":[801]},"selected":{"options":{"assistedTravel":{"assistanceSsrCode":"WCHR"}}}}]}]}
```

Sample `A04` (dimensions use `SelectedDimensionsInput`):
```json
{"variantCode":"A04","products":[{"provisionId":706,"units":[{"orderServiceId":506,"unit":{"travellerId":1001},"coverage":{"flightIds":[801]},"selected":{"options":{"baggage":{"dimensions":{"lengthCm":120,"widthCm":80,"heightCm":60}}}}}]}]}
```

نمونه `A20`:
```json
{"variantCode":"A20","products":[{"provisionId":704,"units":[{"orderServiceId":504,"unit":{"travellerId":1001},"selected":{"options":{"airportService":{"timeWithOffset":"2026-10-27T08:00:00+03:30","guestCount":1}}}}]}]}
```

## ب. snapshot چه چیزهایی را باید نگه دارد؟
- **Selected values**: همه مقادیر متغیر و تأثیرگذار در اجرای خدمت (SSR، Pet، BagRef، Dimensions، Time/GuestCount، DeviceCount). حفظ decimals و DateTimeOffset.
- **Fixed fulfillment facts** (Owner Q4): only `serviceDefinitionRef` (every unit), `equipmentKind` (A06), `mealCode` (A11), `menuItemRef` (A12), `toCabinId` (A10 fixed), `airportId` and `facilityId` (A20–A22), captured with the Price rule. *Not* a picture of the Definition or of the pricing.
- **Commercial boundaries**: `VariantCode`, `ProvisionId`, derived `CoverageScope`, `Quantity`, `TravellerId`, `CoveredFlightIds` و status/refs.
- Unit نه قیمت/مالیات/EMD و نه Outcome مالی را کپی کند. آن‌ها متعلق Order هستند.

## ج. Missing source
- Gate 0 found a unique source for every fixed choice of A02, A06, A10 (fixed), A11, A12, A20–A22 and A24 (`TypedSelectionForm.WithFixedChoices`); these variants are supported.
- A13/A14 (forced confirmation), A18 (offer-scoped refs), A09, A10 with a quote, A17 and A19 stay **GATED**; the Hold refuses them with an explicit error and no `XxxRef` field is invented. Pets and the A18 refs are not members of the Hold request at all (Owner R1 Q3).
- *Supported* = `Hold → SQL → GET` returns the complete values and the stock stays correct; a record in the registry alone does not mean a variant is implemented.
