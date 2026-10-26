# 03 — A01–A24: Family، Selected Value، Source، Support و Round-trip

**دقت:** `AncillaryVariant.cs` وجود ۲۴ کد را اثبات می‌کند، اما قابلیت Hold را اثبات نمی‌کند. وضعیت زیر از Registry، `CustomerSelectionContract`, `HoldSelectionVerifier`, source v11 و گزارش `V11-FINAL-CLOSURE` استخراج شده؛ PASS تست‌ها فقط **گزارش ایجنت** هستند. `Supported` مشروط به وجود Product/Provision/Policy معتبر و Inventory قابل دسترس است. Fixed facts زیر از `TypedSelectionForm.WithFixedChoices` هنگام Hold گرفته و پایدار می‌شوند؛ از Caller گرفته نمی‌شوند.

| کد | معنی | Family/Route | Dynamic selection از Caller | Fixed choice ذخیره‌شده از Catalog | Support/Inventory |
|---|---|---|---|---|---|
| A01 | ExtraCheckedBag | `baggages` | quantity | allowance از Definition برای Validation، نه Price snapshot | Supported: Count/FixedKg/Unlimited طبق Policy |
| A02 | ExtraWeightPackage | `baggages` | quantity | `serviceDefinitionRef` = Package identity | Supported با Policy سازگار |
| A03 | Overweight | `baggages` | `bagRef`, `measuredWeightKg`, quantity اگر انتخابی | Product ref | Supported برای `PerPiece`؛ AcceptedWeight/PerKilogram GATED |
| A04 | Oversize | `baggages` | `dimensions{lengthCm,widthCm,heightCm}` | Product rules | Supported، Local/Unlimited |
| A05 | CabinBag | `baggages` | quantity | Product/Allowance rules | Supported |
| A06 | SpecialEquipment | `baggages` | `dimensions?`, `weightKg?` طبق Rule | `equipmentKind` از Definition | Supported |
| A07 | StandardSeat | `SeatMaps/Holds` | `seatMapSelectionId`, per-pax Flight context | SeatNumber/Provision از Seat verifier | Supported commercial؛ بدون Seat physical |
| A08 | PreferredSeat | `SeatMaps/Holds` | همان A07 | همان A07 | Supported commercial |
| A09 | ExtraSeat | هیچ Hold در v11 | `purpose` در Catalog contract فقط | — | GATED؛ مسیر فروشی/رزرو تعریف نشده |
| A10 | CabinUpgrade | `upgrades` | quantity فقط اگر لازم؛ quoteRef ارسال نشود | `toCabinId` در FixedAncillary | Supported فقط FixedAncillary؛ ExternalQuote GATED |
| A11 | FreeSpecialMeal | `meals` | Quantity ثابت یا تعیین‌شده | `mealCode` مانند VGML | Supported؛ GET باید MealCode را بازسازی کند |
| A12 | PaidPreorderMeal | `meals` | quantity | `menuItemRef` ثابت از Definition | Supported؛ GET باید MenuItemRef را بازسازی کند |
| A13 | PetInCabin | `pets` **در Hold وجود ندارد** | AnimalType/CombinedWeight/Carrier در Catalog/Price contract | — | GATED؛ SubjectToConfirmation |
| A14 | PetInHold | `pets` **در Hold وجود ندارد** | AnimalType/CombinedWeight/Carrier/Document codes | — | GATED؛ SubjectToConfirmation |
| A15 | Wheelchair | `assistedTravel` | `options.assistedTravel.assistanceSsrCode` | — | Supported اگر Product Immediate؛ WCHR/WCHS |
| A16 | DisabilityAssistance | `assistedTravel` | `assistanceSsrCode` | — | Supported اگر Rule اجازه دهد |
| A17 | MedicalEquipment | `assistedTravel` **ولی Hold رد می‌کند** | EquipmentCode/Evidence/Oxygen در Selection Contract | — | GATED؛ نیازمند قرارداد و Privacy/At-rest review |
| A18 | Bassinet | `assistedTravel` **ولی Hold رد می‌کند** | `infantRef`/`guardianRef` Offer-scoped | — | GATED؛ نگاشت شناسه و Policy تصویب نشده |
| A19 | UnaccompaniedMinor | `assistedTravel` **ولی Hold رد می‌کند** | GuardianHandoff/GuardianPickup contacts در Customer Contract | — | GATED؛ Price input/Protection کامل نیست |
| A20 | Lounge | `airportServices` | `timeWithOffset?`, `guestCount?` طبق Provision | `airportId`, `facilityId?` | Supported با Local/Unlimited؛ Slot نیاز به Appointment واقعی |
| A21 | FastTrack | `airportServices` | همان A20 | همان A20 | Supported طبق Policy |
| A22 | CIP MeetAssist | `airportServices` | همان A20 | همان A20 | Supported طبق Policy |
| A23 | PriorityBoardingCheckin | `priorityServices` | `options.priority.optIn=true` | Product/Variant خود قرارداد opt-in است | Supported؛ `false` نباید Hold شود |
| A24 | OnboardWifi | `connectivity` | `deviceCount?` اگر Rule دارد | `serviceDefinitionRef` = fixed plan | Supported اگر Product/Plan valid |

## منبع تک‌تک فیلدها
- **Caller:** `orderServiceId`، `travellerId` (در Scope مسافردار)، `coverage.flightIds` (در Scope پروازی)، مقدار `selected.quantity` وقتی متغیر، `selected.options` نوعی. هیچ اطلاعات Pricing/Stock/Tax/PCC/EMD ارسال نشود.
- **Local Provision:** `provisionId`, `ServiceDefinitionId`, `CoverageScope` (Sector=1, Portion=2, Journey=3, Order=4 در سورس v10)، Quantity bounds، Product Application، Validity.
- **Local Definition:** `VariantCode`, `Profile`, `ServiceDefinitionRef`، Fixed choices شامل `equipmentKind`, `mealCode`, `menuItemRef`, `toCabinId`, `airportId`; FacilityId از Rule/Provision محلی.
- **Local Policy/Inventory:** SourceId/ResourceKind, Count/Weight/Slot policy, OccupancyMinutes/PeoplePerAcceptedUnit؛ این‌ها **ورودی Wire نیستند**.
- **Seat verifier:** `seatNumber`, `pricingId` داخلی، Variant و Provision. `seatMapSelectionId` Caller correlation ذخیره می‌شود.

## ماتریس اثبات Round-trip (برای تمام موارد Supported)

| انتخاب | GET Persisted | حداقل Test |
|---|---|---|
| A01/A02/A05 Qty | `selected.quantity` + `serviceDefinitionRef` | مقدار بعد از SQL reload و تغییر تعریف برابر باشد |
| A03 BagRef/Weight | `selected.options.baggage` | اعشار، ref و Quantity دقیق؛ Policy Gated جدا |
| A04/A06 dimensions/equipmentKind | `options.baggage.dimensions/weightKg`, `details.equipmentKind` | هر بعد، مقدار ثابت و عدم ورود گزینه اضافی |
| A10 toCabin | `details.toCabinId` | Source fixed, immutable, ExternalQuote rejected |
| A11/A12 meal | `details.mealCode/menuItemRef` | Definition retired، GET همان انتخاب تاریخی |
| A15/A16 SSR | `selected.options.assistedTravel.assistanceSsrCode` | ارسال WCHR/WCHS، همان کد روی SQL/GET |
| A20–A22 Slot | `options.airportService.timeWithOffset/guestCount`, `details.airportId/facilityId` | offset+count حفظ، consumption بر پایه Policy، release capacity |
| A23 opt-in | اگر Unit وجود دارد، OptIn true؛ GET مطابق Source Serializer | `false` و extra field رد شوند؛ null-VO ساخته نشود |
| A24 device/plan | `options.connectivity.deviceCount`, `serviceDefinitionRef` | مقدار DeviceCount و Plan پایدار |
| A07/A08 Seat | `seats[].units[].seatNumber/seatMapSelectionId` | Appended/Standalone و 0 physical allocations |

## تست‌های GATED (منفی، نه green-by-fake)
هر مورد A09، A13، A14، A17، A18، A19 و A10 Quote باید برای ورودی مربوطه fail-closed بدهد، بدون Reservation و بدون Allocation/ProviderRef؛ `pets` top-level در v11 ناشناخته و 400 است. اگر بعداً Owner آن‌ها را باز کند، ابتدا DTO مستقل و Slot/Provider واقعی طراحی شود، نه boolean override و fake status.

## پرسش‌هایی که به حدس تبدیل نمی‌شوند
`MenuItemRef` و `Plan` در محصولات ثابت فعلی از Definition گرفته می‌شوند؛ اگر مدل Catalogue اجازه انتخاب چند MenuItem در یک Definition را پیدا کند، همین شکل Hold دیگر نماینده گزینهٔ واقعی نیست و باید **پیش از عرضه** Contract بازطراحی و Owner تأیید کند. چنین تنوعی در سورس بررسی‌شده اثبات نشده است؛ اکنون فیلد جدید اختراع نشود.
