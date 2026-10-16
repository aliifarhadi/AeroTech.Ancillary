# 06 — ماتریس قطعی ۲۴ Variant و حداقل Facts لازم

**منبع Code/Profile/PricingUnit:** Registry فعلی `AncillaryVariant.cs`. هر سطر بیان می‌کند Selection چه چیزی می‌خواهد، Domain چه چیزی چک می‌کند و Stage 3 چه تعهدی خواهد داشت. Optional Fields تنها وقتی Variant اجازه دهد فعال می‌شوند.

| Code | Profile / محصول | Definition/Options و Selection کلیدی | Billing / Coverage | Stage 3 و محدودیت |
|---|---|---|---|---|
| **A01** | Baggage / Extra checked bag | PTC، allowance پایه، extra `Quantity` | `PerPiece`, Pax/Portion | hold bag quota/eligible supplier؛ چند Pax جدا |
| **A02** | Baggage / Weight package | `PackageWeightKg`, `PackageProductRef`, `Quantity` | `PerItem`, Pax/Portion | وزن بسته معتبر، no double charge |
| **A03** | Baggage / Overweight | `BagRef`, `MeasuredWeightKg`, band | `PerPiece` یا `PerKilogram`, Pax/Portion | بدون وزن verified => quote/total ناقص |
| **A04** | Baggage / Oversize | `Dimensions`, maxLinearSumCm | `PerPiece`, Pax/Portion | overlap با A03 بر اساس ترکیب rule |
| **A05** | Baggage / Cabin bag | `Quantity`, cabin allowance | `PerPiece`, Pax/Portion | verified policy/space supplier |
| **A06** | Baggage / Sports/special equipment | `EquipmentKind`, `WeightKg`, `Dimensions` | `PerItem`/`PerPiece`, Pax/Portion | provider specialized approval |
| **A07** | Seat / Standard | SeatNumber, Cabin, characteristic, traveller/flight | `PerSeat`, Pax/Sector | SeatMap؛ physical owner FlightFlow |
| **A08** | Seat / Preferred | preferred characteristics, exitRow eligibility, SeatNumber | `PerSeat`, Pax/Sector | child/exit row restrictions؛ SeatMap |
| **A09** | Seat / ExtraSeat (EXST) | `RequiresAdjacentSeat`, purpose, extra occupied count | `PerSeat`, Pax/Sector | **generic AncillaryOffers**؛ coordinated physical+ticket action |
| **A10** | Upgrade / Cabin | from/to cabin, fare family, `TargetCabinId`, `QuoteRef` if dynamic | `PerPassenger`/`PerSeat`, Pax/Flight | verify booking class and need ticket exchange |
| **A11** | Meal / Free special meal | `MealCode`, diet, leadtime | `PerPassenger`/`PerItem`, Pax/Flight | SSR & explicit Free; confirmation if required |
| **A12** | Meal / Paid preorder | `MenuItemRef`, `Quantity`, meal family | `PerItem`/`PerPassenger`, Pax/Flight | verified menu stock, prep/cutoff |
| **A13** | Pet / cabin | `AnimalType`, weight/carrier, age/docs | `PerItem`, Pax/Portion | cabin pet quota/operating rules |
| **A14** | Pet / hold | A13 + `SizeBracket` | `PerItem`, Pax/Portion | hold capability & restrictions |
| **A15** | AssistedTravel / Wheelchair | `AssistanceSsrCode` WCHR/WCHS/WCHC | `PerPassenger`, Pax/Flight | SSR & airport/provider acknowledgement |
| **A16** | AssistedTravel / Disability | assistance type, contact/disclosure | `PerPassenger`, Pax/Flight | manual approval if relevant |
| **A17** | AssistedTravel / MedicalEquipment | `EquipmentCode`, docs, oxygenUnits | `PerPassenger`/`PerItem`, Pax/Flight | source approval, oxygen inventory if real |
| **A18** | AssistedTravel / Bassinet | `InfantRef`, `GuardianRef`, max age/weight | `PerPassenger`/`PerItem`, Pax/Flight | verified bassinet/seat compatibility |
| **A19** | AssistedTravel / UMNR | `ChildRef`, guardian contacts, connection policy | `PerPassenger`, Pax/Bound | guardian/age/route evidence |
| **A20** | AirportService / Lounge | AirportId/FacilityRef/TimeWithOffset/GuestCount | `PerPassenger`, pax/airport | time slot/supplier guest quota |
| **A21** | AirportService / FastTrack | AirportId, terminal, window | `PerPassenger`, pax/airport | supplier/local slot |
| **A22** | AirportService / CIP | included components/GuestCount/appointment | `PerPassenger`/`PerItem`, pax/order if supported | combined supplier package, no duplicate fees |
| **A23** | Priority / BoardingCheckin | OptIn, zone/priorityGroup, fare benefit | `PerPassenger`, pax/flight | fare inclusion, no duplicate entitlement |
| **A24** | Connectivity / Wi-Fi | PlanCode, DeviceCount, duration/data | `PerItem`/`PerPassenger`, pax/order if supported | verified activation/provider ref |

## قواعد تفاوت دسته‌ها
1. A07/A08 فقط SeatMap؛ A09 با اینکه Profile=`Seat` است در خروجی generic `AncillaryOffers` فعلی باقی می‌ماند. این یک **استثنای صریح** است، نه دو نوع Pricing Engine.
2. A11 free special meal می‌تواند SSR رایگان باشد؛ Free بودن A11 به معنی فرض رایگان‌بودن همه meal نیست.
3. A10 قیمت داینامیک بدون QuoteRef/Expiry معتبر، `NeedsQuote` است؛ Order/Ticket exchange را Ancillary انجام نمی‌دهد.
4. A13/A14 حیوان را هرگز با baggage «piece» یکسان نکنید؛ Carrier rule و Supplier approval جداست.
5. A18 infant+guardian، A19 minor+contacts و A15 SSR code مستلزم beneficiary دقیق‌اند؛ order-scoped بدون Pax برای اینها معتبر نیست.
6. A20-A22 Time/Facility ممکن است cross timezone باشد؛ appointment/slot صرفاً با source verified قابل Hold است.
7. A24 PlanCode انتخاب شده باید واقعی و قابل ارائه باشد؛ PlanCode دلخواه Caller پذیرفته نشود.

## PerOrder به معنی چه؟
Profileهای واقعاً Order-scoped فقط وقتی Rule/Provider اجازه دهد (مثلاً بسته مشترک CIP یا اتصال اینترنت گروهی) `CoverageScope.Order` و `TravellerId=null` خواهند داشت. قیمت یک‌بار برای همان Service ثبت می‌شود. برای هر Profile که ذاتاً Traveller-specific است، Scope=Order **رد** شود؛ مدل به زور از ۲۴ سناریو حق تازه اختراع نمی‌کند.

## سناریوهایی که باید از یک Engine عبور کنند
- پرواز با ۲ Bound و ۲ Pax (ADT/CHD)، Partial fare inclusion برای بار فقط یک Segment؛ اضافه بار مجزا برای هر واحد و هر نفر.
- یک meal free برای Pax1 و paid menu برای Pax2؛ selectionId مستقل و قیمت مستند.
- infant bassinet به همراه Guardian و محدودیت Exit-row برای child.
- چند خدمات با Supplier خارجی/Local/Manual، بازه زمانی متفاوت و امکان خریدار آژانس.
- Order بدون Traveller برای **محصولی که واقعاً Order-scoped تعریف شده**.
