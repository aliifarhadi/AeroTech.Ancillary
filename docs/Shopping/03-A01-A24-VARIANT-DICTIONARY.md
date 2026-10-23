# v8 LEAN — دیکشنری فیلد به فیلد ۲۴ Variant و DTOهای نوعی

**مرجع:** فقط Pack v8. برای **Quantity-only** شناسه هر تعداد در `quantityOptions[]` و بدون Snapshot/POST صادر می‌شود. فقط Typed با POST Options ذخیره می‌شود. Resolve صرفاً `{ids:[]}` دارد.  
**Baseline Source:** `AncillaryVariant.cs`, `CustomerSelectionContract.cs`, `AncillarySelection.cs`, Specificationها در `k8s-stg@49e9209`; D2 دو فیلد جدید Oxygen را اضافه می‌کند. هرگونه مغایرت باید در Gate0 گزارش شود.

## 1. قواعد لازم‌الاجرا برای تمام Variantها

- نام Familyها: `baggages`, `meals`, `pets`, `assistedTravel`, `airportServices`, `priorityServices`, `connectivity`, `upgrades`, `extraSeats`, `travelInsurance`. آخرین دو مورد فعلاً empty.
- Scope/Unit تعیین می‌کند **چند ردیف Shopping** داریم؛ داشتن Qty یا فرم آن عدد را تغییر نمی‌دهد.
- اگر **هیچ ورودی ندارد** → `selectionId` امضاشده `d_` در Shopping؛ اگر **فقط Quantity صحیح قابل شمارش** دارد → یک ردیف Shopping با `quantityOptions:[{quantity,selectionId,totalAmount}]` و **هیچ Snapshot/POST**؛ اگر انتخاب تخصصی دارد → `reference` امضاشده `r_` از Shopping و فقط پس از POST معتبر، `selectionId` از نوع `s_` در Snapshot.
- `quantity` فقط برای Typed دارای QuantitySelectable در Body POST است. برای Qty-only، Quantity داخل شناسه امضاشده `d_` و در `quantityOptions[]` بوده و GET/POST ندارد.
- DTOهای تخصصی نباید ۳۰ فیلد Nullable ناشی از `AncillarySelection` را در سطوح عمومی منتشر کنند. از Domain صرفاً mapping داخلی field-specific انجام می‌شود.
- برای Free همچنان RateId واقعی مثبت و Rate صفر الزامی است؛ SeatMap A07/A08 FROZEN، A09/Insurance/A19 gated.
- `selected` فقط در Snapshot Typed و خروجی تخصصی Resolve است؛ **در Resolve Request مطلقاً وجود ندارد**.
- هر فیلد نوعی باید منبع تعریف و Required شرطی داشته باشد؛ در صورت `Unknown` بودن گزینه اجباری، محصول publish نشود.

## 2. مقادیر و Source فیلدهای همه Variantها

در ستون «گزینه‌های مشتری» نام‌ها *CamelCase JSON* همان فیلدهای Domain/`AncillarySelection` است. واژه «ثابت» یعنی مقدار از Definition معلوم است و کاربر نباید دوباره آن را انتخاب کند.

| Code | Family | انتخاب مشتری و شرط (نوع) | `details` اختصاصی از Domain | وضعیت تولید |
|---|---|---|---|---|
| **A01** | Baggages | `quantity:int` مستقل؛ `travellerRef`+`boundRef` از Unit، نه فرم | `chargeKind=ExtraPiece`, `allowanceConcept`, `maxKgPerPiece?`؛ اگر نداریم null | فعال اگر rate کامل؛ `PerTravellerPerPortion` وقتی Provision.Portion، `PerTravellerPerFlight` وقتی Sector؛ در هر حالت Quantity فقط QuantityOptions است |
| **A02** | Baggages | `packageProductRef:string` اگر از Definition ثابت نیست، Typed Options؛ `quantity:int` مستقل | `chargeKind=WeightPackage`, `packageWeightKg`, `allowanceConcept` | Rate و package واقعی الزامی، انتخاب بسته را به‌جای وزن جعلی نسازید |
| **A03** | Baggages | `bagRef:string` required، `measuredWeightKg:decimal>0` required | `chargeKind=Overweight`, `weightFromExclusiveKg`, `weightToInclusiveKg`, `chargeCombination?` | Typed؛ `PerKilogram` بدون قیمت قطعی D1 = GATED |
| **A04** | Baggages | `dimensions:DimensionsCm` required | `chargeKind=Oversize`, `maxSizeCm?`, `maxLinearSumCm?` | Typed |
| **A05** | Baggages | `quantity:int` مستقل | `chargeKind=ExtraPiece`, `allowanceConcept`, `maxKgPerPiece?` | Direct یا Quantity بر اساس Provision |
| **A06** | Baggages | `equipmentKind:string` required؛ `dimensions:DimensionsCm?` به شرط MaxSize/MaxLinear؛ `weightKg:decimal?` به شرط MaxKg | `chargeKind=SpecialEquipment`, `equipmentKind`, `maxSizeCm?`, `maxKgPerPiece?` | Typed مگر همه انتخاب‌ها در Definition قفل و سایر requiredها نداشته باشد |
| **A07** | SeatMap | `seatNumber`/eligible details | SeatMap فعلی | **FROZEN**؛ نه List جدید |
| **A08** | SeatMap | همان | SeatMap فعلی | **FROZEN**؛ نه List جدید |
| **A09** | ExtraSeats | `seatNumber:string`, `purpose:ExtraSeatPurpose` (اگر Definition لازم دارد)؛ Qty معمولاً ثابت ولی از Provision | `seatPurpose=ExtraSeat`, `extraSeatPurpose`, `extraOccupiedSeatCount`, `requiresAdjacentSeat`, `requiresExternalTicketAction` | **GATED D3**؛ قبل FlightFlow evidence هیچ ID/Options فعال |
| **A10** | Upgrades | `targetCabinId:int`؛ فقط FixedAncillary به‌صورت قابل درج؛ `quoteRef:string` در مدل‌های نیازمند Quote | `fromCabinId`, `toCabinId`, `allowedUpgradeKind`, `requiresTicketExchange` | Dynamic Quote GATED؛ Fixed فقط با Rate Filed و NoApproval |
| **A11** | Meals | `mealCode:string` از Definition `Meal.MealCode` ثابت؛ اگر انتخاب دیگری باقی نیست Direct | `mealKind=SpecialRequest`, `mealCode`, `dietaryCode?`, `cateringLeadTimeMinutes` | zero-priced Rate **واقعی** لازم، بدون PricingRateId ممنوع |
| **A12** | Meals | `menuItemRef:string` از Definition (`Meal.MenuItemRef`)؛ `quantity:int` مستقل | `mealKind=PaidPreorder`, `menuItemRef`, `dietaryCode?`, `cateringLeadTimeMinutes`, `exclusiveMealFamilyCode?` | اگر MenuItemRef یک SKU مشخص است Direct/QuantityOptions؛ اگر چند انتخاب واقعی و Rate جدا دارند Typed |
| **A13** | Pets | `animalType:PetAnimalType`, `combinedWeightKg:decimal>0`, `carrierDimensions:DimensionsCm`, `otherAnimalCode:string?` اگر RegisteredOther، `animalAgeWeeks:int?`, `documentAcknowledgements:string[]?` طبق Domain | `transportMode=Cabin`, `maxCombinedWeightKg`, `carrierDimensionsMaxCm`, `minAnimalAgeWeeks?`, `allowedAnimalTypes`, `requiredDocumentCodes` | Typed؛ اگر supplier/manual approval لازم دارد GATED |
| **A14** | Pets | مثل A13 + `sizeBracket:string` اگر AllowedHoldAnimalSizeBrackets | `transportMode=Hold`, `allowedHoldAnimalSizeBrackets`, بقیه A13 | Typed؛ GATED اگر Approval/Quote |
| **A15** | AssistedTravel | `assistanceSsrCode:string` انتخاب از `Wheelchair.AllowedSsrCodes` | `assistanceKind=Wheelchair`, `allowedSsrCodes`, `assistanceLevelCode?`, `leadTimeMinutes` | Typed، حتی قیمت صفر Rate واقعی |
| **A16** | AssistedTravel | `assistanceSsrCode:string` از Disability.AllowedSsrCodes | `assistanceKind=DisabilityAssistance`, `allowedSsrCodes`, `requiredCommunicationMethod?` | Typed، دستی/Approval GATED |
| **A17** | AssistedTravel | `equipmentCode:string` از MedicalServiceCode؛ `evidenceDocumentRefs:string[]?`; `oxygenUnits:decimal?` برای Oxygen؛ **`oxygenFlowRateLitersPerMinute:decimal>0` برای Oxygen**؛ **`frequentTravellerMedicalCardNumber:string?` برای Oxygen** | `assistanceKind=MedicalEquipment`, `medicalServiceCode`, `equipmentKind`, `requiresMedicalApproval`, `evidenceTypeCodes`, `oxygenUnits?` | `OWNER-D2` دو فیلد؛ `requiresMedicalApproval=true` ⇒ GATED؛ دیگر EquipmentKindها دو فیلد Oxygen ندارند |
| **A18** | AssistedTravel | `infantRef:string` + `guardianRef:string`؛ PTC مرجع از Shopping trusted context (not guessed from refs)؛ `flightId` از Unit | `assistanceKind=Bassinet`, `maxInfantWeightKg?`, `maxInfantAgeMonths?`, `compatibleSeatGroups[]`, `requiresInfantAndGuardian` | Typed؛ اگر Guardian eligibility غیرقابل اثبات و فروش قطعی وابسته به آن است GATED |
| **A19** | AssistedTravel | Source Contract: `guardianHandoffContact:text`, `guardianPickupContact:text`؛ Source selection object: **دو Boolean** به نام `guardianHandoffContactProvided` و `guardianPickupContactProvided`؛ `childRef` | `assistanceKind=UnaccompaniedMinor`, `minAgeYears`, `maxAgeYearsExclusive`, `guardianContactRequired`, `connectionPolicy`, `allowedTransitAirportIds[]` | **GATED بر اثر mismatch**؛ هیچ mapping بدیهی Bool↔Text وجود ندارد؛ گزارش Owner الزامی |
| **A20** | AirportServices | `airportId:int` ثابت؛ `facilityRef:long?` اگر مدل Facility؛ `timeWithOffset:DateTimeOffset?` اگر appointment؛ `guestCount:int?` اگر Guest policy | `kind=Lounge`, `airportId`, `terminalRef?`, `facilityId?`, `direction`, `serviceWindowStart/End?`, `ianaTimeZone?`, `visitDurationMinutes?`, `maxGuestsPerPrimary?`, `requiresSpecificAppointment` | Direct اگر همه ثوابت‌اند؛ Typed اگر appointment/facility انتخابی |
| **A21** | AirportServices | همان فیلدهای A20 بدون الزامات اضافه | `kind=FastTrack`, همان spec AirportService | Direct/Typed تابع Provision؛ supplier-approval واقعی GATED |
| **A22** | AirportServices | همان + `guestCount?` از Policy | `kind=Cip`, `includedComponentCodes[]`, همان spec AirportService | Direct/Typed؛ Quote/Approval GATED |
| **A23** | PriorityServices | `optIn:true` صریح | `kind:PriorityKind`, `priorityZoneCode?`, `priorityGroupCode?`, `fareBenefitRef?`, `airportIds[]` | **TypedOptions برای رضایت صریح**؛ ID در List به‌طور خودکار با optIn=true صادر نشود |
| **A24** | Connectivity | `planCode:string` از Definition ثابت؛ `deviceCount:int?` فقط اگر MaxDevices تعریف و کاربر می‌تواند تنظیم کند | `planKind`, `durationMinutes?`, `includedDataMb?`, `maxDevices?`, `deliveryStage`, `fulfillmentProviderRef?` | Direct با plan ثابت و بدون device choice؛ Typed اگر deviceCount انتخابی |

**مهم:** جدول «Variant → همیشه یک Interaction» نیست. `QuantityPolicy` از `Provision.Quantity` تعیین می‌شود، مستقل از اینکه A15/A17 Typed است؛ خود `SelectionScope` از `Provision.CoverageScope` می‌آید. هیچ hardcode `A01→PerTravellerPerFlight` یا `A24→PerOrder` مجاز نیست؛ A24 Order تنها وقتی خود Domain Coverage=Order مجاز می‌دهد.

## 3. فیلدبه‌فیلد `details` تخصصی Familyها

### 3.1 `BaggageDetailsDto` فقط در `baggages[]`

| فیلد / نوع | مقدار از Source / تعریف |
|---|---|
| `chargeKind:string` | `BaggageSpecification.ChargeKind` |
| `allowanceConcept:string?` | `.AllowanceConcept` |
| `packageWeightKg:decimal?` | `.PackageWeightKg`, فقط A02 |
| `maxKgPerPiece:decimal?` | `.MaxKgPerPiece`، **فقدان آن به معنی 23kg نیست** |
| `weightFromExclusiveKg:decimal?` | `.WeightFromExclusiveKg`, A03 |
| `weightToInclusiveKg:decimal?` | `.WeightToInclusiveKg`, A03 |
| `maxSizeCm:DimensionsCm?` | `.MaxSize` |
| `maxLinearSumCm:decimal?` | `.MaxLinearSumCm` |
| `equipmentKind:string?` | `.EquipmentKind`, A06 |
| `chargeCombination:string?` | `.ChargeCombination`, A03/A04 |
| `includedBaggage` | فقط optional در **Shopping UI** اگر از Offer موجود است: `checkedPieces:int?`, `checkedWeight:decimal?`, `checkedWeightUnit:string?`, `cabinPieces:int?`, `cabinWeight:decimal?`, `cabinWeightUnit:string?`. در Resolve **نباید** از منبع خارجی دوباره آورده شود |

### 3.2 `MealDetailsDto` فقط در `meals[]`
`mealKind:string` (required), `mealCode:string?`, `menuItemRef:string?`, `dietaryCode:string?`, `cateringLeadTimeMinutes:int`, `exclusiveMealFamilyCode:string?`. منبع `MealSpecification`.

### 3.3 `PetDetailsDto` فقط در `pets[]`
`transportMode:string`, `maxCombinedWeightKg:decimal`, `carrierDimensionsMaxCm:DimensionsCm`, `minAnimalAgeWeeks:int?`, `allowedAnimalTypes:{animalType:string,otherCode:string?}[]`, `requiredDocumentCodes:string[]`, `allowedHoldAnimalSizeBrackets:{code:string,weightFromExclusiveKg:decimal,weightToInclusiveKg:decimal}[]`. منبع `PetSpecification`.

### 3.4 `AssistedTravelDetails` فقط `assistedTravel[]`، نوعی نه ۵ object nullable

- **A15** `kind:"Wheelchair"`, `allowedSsrCodes:string[]`, `assistanceLevelCode:string?`, `leadTimeMinutes:int`.
- **A16** `kind:"DisabilityAssistance"`, `allowedSsrCodes:string[]`, `requiredCommunicationMethod:string?`.
- **A17** `kind:"MedicalEquipment"`, `medicalServiceCode:string`, `equipmentKind:string`, `requiresMedicalApproval:bool`, `evidenceTypeCodes:string[]`, `oxygenUnits:decimal?` (Definition config, با مقدار Selected متفاوت)؛ دو فیلد D2 **selected customer fields** هستند نه تعریف قیمت ثابت.
- **A18** `kind:"Bassinet"`, `maxInfantWeightKg:decimal?`, `maxInfantAgeMonths:int?`, `compatibleSeatGroups:string[]`, `requiresInfantAndGuardian:bool`.
- **A19** `kind:"UnaccompaniedMinor"`, `minAgeYears:int`, `maxAgeYearsExclusive:int`, `guardianContactRequired:bool`, `connectionPolicy:string`, `allowedTransitAirportIds:int[]`؛ فعلاً gated.

### 3.5 `AirportServiceDetailsDto` فقط `airportServices[]`
`kind:string`, `airportId:int`, `terminalRef:string?`, `facilityId:long?`, `direction:string`, `serviceWindowStart:string? (HH:mm:ss)`, `serviceWindowEnd:string?`, `ianaTimeZone:string?`, `visitDurationMinutes:int?`, `maxGuestsPerPrimary:int?`, `requiresSpecificAppointment:bool`, `includedComponentCodes:string[]` (تنها A22). Source `AirportServiceSpecification` و Provision rule. ISO offset زمان انتخاب‌شده فقط در `selected.timeWithOffset`.

### 3.6 `PriorityDetailsDto` فقط `priorityServices[]`
`kind:string`, `priorityZoneCode:string?`, `priorityGroupCode:string?`, `fareBenefitRef:string?`, `airportIds:int[]`. A23 checkbox optIn **قبل از SelectionId** لازم است.

### 3.7 `ConnectivityDetailsDto` فقط `connectivity[]`
`planKind:string`, `durationMinutes:int?`, `includedDataMb:int?`, `maxDevices:int?`, `deliveryStage:string`, `fulfillmentProviderRef:string?`, `eligibleAircraftIds:int[]`. `selected.planCode` از Product تعریف‌شده ثابت می‌آید؛ `selected.deviceCount` فقط اگر user انتخاب کرده باشد.

### 3.8 `UpgradeDetailsDto` فقط `upgrades[]`
`fromCabinId:int`, `toCabinId:int`, `allowedUpgradeKind:string`, `requiresTicketExchange:bool`, `eligibleFareFamilyIds:long[]`. `quoteRef` به هیچ شکل برای موارد ExternalQuote در این فاز جایگاه ندارد، زیرا آنها از Shopping حذف‌اند.

### 3.9 `ExtraSeatDetailsDto` فقط `extraSeats[]` **GATED D3**
`seatPurpose:string`, `requiresExitRowEligibility:bool`, `requiresAdjacentSeat:bool`, `extraSeatPurpose:string?`, `extraOccupiedSeatCount:int?`, `requiresExternalTicketAction:bool`, `seatCharacteristicCodes:string[]`, `applicableCabinIds:int[]`. فقط پس از بررسی موجودی/OccupiedSeat FlightFlow، بدون تغییر SeatMap.

### 3.10 `TravelInsuranceDetailsDto` — **وجود خارجی در سورس فعلی ندارد**
این Family با مقدار `[]` در Envelope می‌ماند تا Extension آینده؛ هیچ policy پوشش پزشکی، provider، policyId، VariantCode جعلی یا PricingRateID برای آن نوشته نشود. نمونه تصویر فقط نحوه UX PerOrder را نشان می‌دهد.

## 4. GET Options field descriptor: نوع دقیق control، شرط نمایش و منبع choice

| Variant | Field Descriptorهای مجاز و Required | منابع/محدودیتِ واقعی |
|---|---|---|
| A02 | `packageProductRef`: select/string, required **فقط اگر چند بسته واقعاً انتخابی هستند** | Options از PackageProductRefs واقعی؛ اگر یک Definition ref ثابت است، omit Options و Quantity مستقل |
| A03 | `bagRef`: text/string R؛ `measuredWeightKg`: number/decimal R | `bagRef` مشتری، صرفا ID بار؛ weight >0؛ PricePerKg ناقص => Gate D1 |
| A04 | `dimensions`: dimensions/dimensions R | سه بعد decimal>0؛ MaxSize/LinearSum فقط اگر تعریف شده |
| A06 | `equipmentKind`: select/string R؛ `dimensions`: dimensions conditional R؛ `weightKg`: number conditional R | equipment code از Definition، max per Source |
| A09 | `seatNumber`: text/string R؛ `purpose`: select/enum if required | **GATED D3** |
| A10 | `targetCabinId`: select/int R اگر انتخاب هنوز باقی است | فقط FixedAncillary فاقد Quote و FiledPrice قطعی؛ از `toCabinId` تعریف‌شده |
| A11 | `mealCode`: select/string R اگر گزینه در Definition قفل نشده | Definition `MealCode` غالباً قفل؛ Direct |
| A12 | `menuItemRef`: select/string R اگر بیش از یک menu item واقعی دارد | منبع Product `MenuItemRef`، Qty مستقل |
| A13 | `animalType`: select/enum R؛ `combinedWeightKg`: number R؛ `carrierDimensions`: dimensions R؛ `otherAnimalCode`: text conditional؛ `animalAgeWeeks`: number optional؛ `documentAcknowledgements`: multi-select conditional | `PetSpecification.AllowedAnimalTypes`, DocCodes و محدودیت وزن و سن |
| A14 | فیلدهای A13 + `sizeBracket`: select/string conditional R | `AllowedHoldAnimalSizeBrackets`، بدون انتخاب bracket جعلی |
| A15 | `assistanceSsrCode`: select/string R | `Wheelchair.AllowedSsrCodes`; WCHR/WCHS/WCHC فقط اگر واقعاً مجاز |
| A16 | `assistanceSsrCode`: select/string R | `DisabilityAssistance.AllowedSsrCodes`; GATED if manual approval |
| A17 | `equipmentCode`: select/string R؛ `evidenceDocumentRefs`: string[] conditional R؛ `oxygenUnits`: decimal conditional R؛ `oxygenFlowRateLitersPerMinute`: decimal R if Oxygen؛ `frequentTravellerMedicalCardNumber`: text optional if Oxygen | `MedicalServiceCode`, `EvidenceTypeCodes`, D2 و EquipmentKind؛ بالینی max/min نسازید |
| A18 | `infantRef`: travellerSelect/string R؛ `guardianRef`: travellerSelect/string R | Must prove allowed INF/ADT references from trusted Offer context. If signed reference/local domain cannot prove actual list, **GATED**; never invent allowed ref set. No OfferId required in GET Body |
| A19 | `guardianHandoffContact`, `guardianPickupContact`: text R | **GATED** تا bool/text mismatch Domain حل شود؛ ChildRef از Unit |
| A20 | `airportId`: انتخاب فقط اگر چند مورد واقعی؛ `facilityRef`: select conditional R؛ `timeWithOffset`: dateTime conditional R؛ `guestCount`: number optional | `AirportServiceSpecification`/`Provision.AirportServiceRule`؛ Fixed airport از Definition auto-filled |
| A21 | مثل A20 | FastTrack-specific؛ no invented appointment |
| A22 | مثل A20 با guest count اگر policy | `includedComponentCodes` فقط display از Spec |
| A23 | `optIn`: checkbox/boolean **required true** | فقط POST Options با true ID می‌دهد؛ false/missing → 422 |
| A24 | `planCode`: select/string R if multiple genuine choices؛ `deviceCount`: number/int conditional user-choice | `Connectivity.PlanKind,MaxDevices`, fixed plan from Definition → direct |

**`quantity` در این جدول نیست**؛ چون محور مستقل است. هر Variant ممکن است Quantity selectable داشته باشد، حتی اگر Options هم لازم دارد.


## 5. Body دقیق POST Options؛ فقط Typed. Quantity-only اصلاً POST ندارد

| Variant | POST Body fields | Required / special handling |
|---|---|---|
| A01 | **No POST for Qty-only** | Quantity from `quantityOptions[]` signed ID; if no selectable Quantity, Direct |
| A02 | `packageProductRef:string` iff user-selected, `quantity:int` iff Typed+Qty | fixed package + Qty-only ⇒ no POST; selectable package ⇒ Typed POST |
| A03 | `bagRef:string`, `measuredWeightKg:decimal` | R; D1 PerKg بدون Total دقیق → unpublished |
| A04 | `dimensions:{lengthCm,widthCm,heightCm}` | R, each decimal>0 |
| A05 | **No POST for Qty-only** | Quantity from `quantityOptions[]` signed ID; if fixed, Direct |
| A06 | `equipmentKind:string`, `dimensions?:DimensionsCm`, `weightKg?:decimal` | Equipment kind R; dimensions/weight according to Definition rule |
| A07, A08 | none | SeatMap مستقل و untouched |
| A09 | `seatNumber:string`, `purpose?:string` | D3 gated: no POST activated |
| A10 | `targetCabinId:int` iff genuinely selectable | Fixed plan/price Direct when fixed; no Quote |
| A11 | `mealCode:string` iff truly selectable | if fixed Direct, no POST |
| A12 | `menuItemRef:string` iff selectable, `quantity:int` iff selectable | may combine typed+qty, one POST |
| A13 | `animalType:string`, `otherAnimalCode?:string`, `animalAgeWeeks?:int`, `combinedWeightKg:decimal`, `carrierDimensions:DimensionsCm`, `documentAcknowledgements?:string[]` | conditional other/document fields as Domain |
| A14 | A13 fields plus `sizeBracket?:string` | only when genuine bracket choice |
| A15 | `assistanceSsrCode:string` | required, allowed Wheelchair SSR |
| A16 | `assistanceSsrCode:string` | required, allowed Disability codes |
| A17 | `equipmentCode:string`, `evidenceDocumentRefs?:string[]`, `oxygenUnits?:decimal`, `oxygenFlowRateLitersPerMinute?:decimal`, `frequentTravellerMedicalCardNumber?:string` | Oxygen D2 fields conditional; no invented clinical bounds; approval => excluded |
| A18 | `infantRef:string`, `guardianRef:string` | required and typed, from existing Shopping traveller refs |
| A19 | `guardianHandoffContact:string`, `guardianPickupContact:string` | GATED until source text vs bool conflict resolved |
| A20 | `airportId?:int`, `facilityRef?:long`, `timeWithOffset?:DateTimeOffset`, `guestCount?:int` | only *customer-selectable* fields; fixed from Definition don't solicit |
| A21 | same as A20 | FastTrack exact policy |
| A22 | same as A20 | CIP guest count only defined policy |
| A23 | `optIn:true` | explicit consent required; `false` not issue Snapshot |
| A24 | `planCode?:string`, `deviceCount?:int` | fixed plan from Definition; only non-fixed fields POST |

**Examples of Typed POST Body — valid JSON:**
```json
{"assistanceSsrCode":"WCHS"}
```
```json
{"menuItemRef":"MENU-A","quantity":2}
```
```json
{"equipmentCode":"AOXY","oxygenUnits":2,"oxygenFlowRateLitersPerMinute":2.0,"frequentTravellerMedicalCardNumber":"FREMEC-EXAMPLE"}
```

These Typed POST examples use the same response schema:

```json
{"selectionId":"s_<22-char-random>","expiresAtUtc":"2026-10-23T19:00:00Z"}
```

Expiry in this final sample is a **fixture only**, not a global rule; `expiresAtUtc` comes from the signed reference/source expiry and configured maximum age. For a service needing no POST (Direct), Shopping issues `d_` selectionId.

## 6. Output of Resolve — still type-specific, but input is unified

Request **always**:

```json
{"ids":["d_<direct>","d_<signed-qty>","s_<typed>"]}
```

Output maintains exact Family arrays and per-type `details` and `selected`. The Snapshot is read internally; it is a bug if Agent exposes `selected` in Resolve request. Quantity is returned in `pricing.quantity`; for Qty-only, `selected` is omitted. Source-specific examples of all output fields in `02-EXACT-API-DTO-AND-FIELD-CONTRACT.md` and Overview §8.

### 6.1 Specialist mapping table for Resolve

| Family | Specific `details` | `selected` from persisted snapshot (only when user supplied it) |
|---|---|---|
| Baggage | BaggageSpecification/ProvisionBaggageRule | `packageProductRef`, `bagRef`, `measuredWeightKg`, `dimensions`, `equipmentKind`, `weightKg` as Variant permits |
| Meal | MealSpecification | `mealCode` or `menuItemRef` if selected; fixed data stays in details |
| Pet | PetSpecification | animal choice, weight, carrier dimensions, docs, sizeBracket |
| AssistedTravel | AssistedTravelSpecification subprofile | SSR code, equipment/docs/oxygen, infant/guardian refs |
| AirportService | AirportServiceSpecification | appointment/facility/guestCount if user-selected |
| Priority | PrioritySpecification | `optIn` if required; false never selected |
| Connectivity | ConnectivitySpecification | `planCode`/`deviceCount` only if customer-selected |
| Upgrade | UpgradeSpecification | `targetCabinId` only if real customer choice |
| ExtraSeat | SeatSpecification | gated D3, no live ID |
| TravelInsurance | none in registry | gated, no live ID |

## 7. Prohibited shortcuts

1. Do not treat quantity as Scope, nor `selectionScope` as Quantity limit.
2. For Qty-only list show independent signed IDs **per offered quantity** in `quantityOptions[]`; do not create a generic ID without quantity.
3. Do not put sensitive Typed values inside ID nor require Front to resend Typed values at Resolve; Qty integer is explicitly signed in `d_`.
4. Do not invent Family-independent `AncillarySelection` JSON with nullable fields for other types.
5. Do not infer Free RateId absent, fall back to Provision, or hardcode 0 price.
6. Do not confuse `PerTravellerPerPortion` with Flight, or create duplicate service per connecting leg.
7. Do not synthesize `A-INS` or resolve A09 before independent authority.
8. Do not accept unvalidated optIn automatically (A23).
9. Do not add fields beyond Domain source and owner-approved D2.
10. Medical fields and FREMEC should not leak in ID, URL, logs, or accidental POST response.


## 8. الزامات قطعی امنیتی و تحویل Pack v8

- تمام `r_` و `d_` باید از Shopping **واجد شرایط** و با HMAC-SHA256 و Expiry صادر شوند. شناسه unsigned حتی اگر قابل Decode باشد در GET/POST/Resolve پذیرفته نمی‌شود.
- `reference` فقط Typed و `quantityOptions[]` فقط Quantity-only. Variant دارای Quantity **و Typed** در `POST Options` یک‌باره `quantity` و انتخاب تخصصی را می‌فرستد.
- Free با PricingRate واقعی صفر است؛ هیچ fallback به ProvisionId یا PriceOrigin.Free بدون Rate.
- `A09` gated تا FlightFlow proof؛ `A17` با FREMEC gated تا approved encryption؛ `A19` gated تا حل ناسازگاری string vs bool؛ Insurance خارج Registry است.
- تمام اعداد EUR/ISO fixture هستند و `currencyId` داخلی از Catalog می‌آید (EUR=47 طبق Owner).
- جدول Variant مشخص می‌کند **کدام فیلدها معنی دارند**؛ در خروجی هر Subtype فقط فیلدهای مرتبط با همان Variant serialized شوند، نه همه فیلدهای خانواده با null.
- هر Field Required شرطی در جدول باید هم در GET Options description، هم POST validator، هم Snapshot mapping و هم Resolve selected با همان نوع و همان نام وجود داشته باشد.
