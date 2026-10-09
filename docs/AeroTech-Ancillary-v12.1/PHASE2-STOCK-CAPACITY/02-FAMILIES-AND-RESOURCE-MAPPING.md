# 02 — الگوهای واقعی ظرفیت و نگاشت سرویس‌ها

> **Project authority:** AeroTech Ancillary **v12.1 — Phase 2: Stock & Capacity**. Continuation of the approved Phase 1 v12.1; not a separately versioned specification. Phase 3 remains closed.


**نکته:** اینجا خانواده‌ها براساس **شکل مصرف منبع** هستند، نه ATPCO GroupCode یا نوع قیمت. منابع نمونه، مالک و عدد ظرفیت نمونه فرضی‌اند مگر با سند مشخص شده باشند.

| خانواده محصول | Source of truth | Policy / Local Model | scope دقیق | مصرف منابع | محدودیت مستقل |
|---|---|---|---|---|---|
| Seat selection صندلی شماره‌دار | FlightFlow | `FlightFlowManaged` | FlightId + SeatNumber + Traveller | 1 صندلی در FlightFlow؛ Ancillary موجودی موازی ندارد | PTC/Fare/blocked-seats در Provision/seat provider |
| Extra bag per piece | airline/handling اگر سهمیه تجاری تعریف شده باشد | `FlightCountInventory` | FlightId + baggage compartment quota | 1 یا N piece | per-passenger max piece در Provision یا UsageLimit |
| Extra bag package 5/10kg | airline commercial quota | `FlightWeightInventory` | FlightId + commercial weight allowance | بسته 5/10 kg؛ مستقل از `PricingUnit=PerItem` | safety / load-control بیرون از این stock |
| Sports gear / bicycle | airline با محدودیت خاص | `FlightCountInventory` و اختیاری `FlightWeightInventory` | FlightId + oversize resource | 1 case + مصرف وزن واقعی/ثابت | نوع هواپیما/سایز/embargo تجاری |
| PETC/AVIH | carrier/handling | `FlightCountInventory` یا `SupplierManaged` | FlightId + cabin/hold resource | 1 carrier/cage slot | per-passenger limit و operational approval |
| UMNR/escort | carrier/provider | `FlightCountInventory` یا supplier | FlightId + staff quota | 1 supported child | age, flight, advance notice، provider acceptance |
| Special meal / preorder | catering/carrier | `FlightCountInventory` | FlightId + production quota; اگر همه غذاها یک محدودیت مشترک دارند ResourceId مشترک | 1 meal | advance purchase و PTC مستقل |
| Wheelchair assistance (service only) | airport handler | `SupplierManaged` یا اگر ثابتاً نامحدود عملیاتی است `Unlimited` | provider | supplier response | WCHR/WCHS/WCHC requirement؛ wheelchair physical asset جدا |
| Loanable wheelchair / equipment | operator | `AssignedAssetInventory` (only if locally owned) | physical AssetId + occupied interval | asset occupancy | assistance booking requirement |
| Lounge / CIP | lounge operator | `AirportSlotInventory` یا SupplierManaged | AirportId + FacilityId + exact slot | one person across occupied slot(s) | per traveller session limit |
| Fast-track / priority airport | facility/operator | `AirportSlotInventory` یا DailyService | AirportId + Facility + slot/date | number of entries | flight eligibility/advance booking |
| Airport transfer/shared shuttle | transfer supplier / fleet | `AirportSlotInventory` برای seat-in-departure slot، یا `AssignedAssetInventory` برای خودرو اختصاصی | ServiceOrigin/Destination + departure occurrence / VehicleId | count or exclusive vehicle | passenger party size and route |
| Hotel room / night | hotel PMS, unless guaranteed allotment is owned by our company | `SupplierManaged` or `RoomNightInventory` | PropertyId + RoomTypeId + each NightDate | 1 room on **every** night of stay | guest occupancy/LOS/room rate separate |
| Airport parking / storage locker | operator | `AssignedAssetInventory` | asset/unit + rental interval | one identified asset | date/time product eligibility |
| Tour/service appointment limited per day | local supplier | `DailyServiceInventory` | ResourceId + ServiceDate | service units | operator approval/cutoff |
| Insurance / eSIM | issuer | `Unlimited` ONLY when truly unlimited; otherwise `SupplierManaged` | no local finite counter | no debit, provider activation | age/geography/eligibility, issuer constraints |
| Per-passenger one meal/flight | customer/order rule | `PassengerUsageLimit` **plus** flight meal physical quota | TravellerIdentity + FlightId + service-family | once per pax | no extra physical inventory row |
| Named-person allowance e.g. prepaid 3 lounge visits | entitlement issuer | external entitlement authority; optionally future `PassengerEntitlementInventory` when internal ledger evidenced | BeneficiaryId + time period | entitlement decrement | distinct from lounge physical slot |

### Scope is not a model

«محدود به فرودگاه» یا «محدود به یک مسافر» به تنهایی تعریف Stock نیستند. به عنوان مثال ظرفیت 20 نفر در لانژ فرودگاه از نوع Slot/Occupancy است؛ محدودیت 1 ورودی برای هر مسافر نوع UsageLimit؛ هر دو با `AirportId` و `TravellerId` مرتبط‌اند، اما **منبع مصرف متفاوت** دارند. با افزودن FlightId و TravellerId به یک جدول Generic نمی‌توان همه آن‌ها را درست کرد.

### ظرفیت مشترک و مصرف دوگانه

نمونه: پرواز F101 ظرفیت 2 جابه‌جایی PETC دارد؛ PETC و درخواست حیوان دیگر اگر از همین منبع فیزیکی استفاده کنند، به همان `FlightCountInventory` وصل می‌شوند؛ دو Counter مجزا ممنوع.

نمونه دوم: دوچرخه 1 Slot تجهیزات Oversize و 12 کیلوگرم از commercial weight quota می‌گیرد. این **دو منبع** باید در Phase 3 به شکل تضمین‌شده با reservation چندمنبعی کنترل شوند؛ یک Success و یک Failure نباید موجودی هدررفته باقی بگذارد. در Phase 2 فقط binding و quantity mapping تعریف می‌شود.

### مبنای زمان

- Flight resource: Identity یک occurrence واقعی `FlightId`، نه FlightNumber یا تاریخ تنها.
- Airport slot: `AirportId + FacilityId + StartUtc + EndUtc`؛ از منطقه زمانی ثبت‌شده Facility برای نمایش و تعریف روز محلی استفاده شود. پایان بازه exclusive؛ DST ambiguity در Backoffice باید شفاف‌سازی/رد شود.
- Daily / Room Night: `ServiceDate:DateOnly` در منطقه محلی provider/property؛ Arrival/Departure date hotel روی `[checkin,checkout)` و هر شب مستقل مصرف می‌شود.
- Asset calendar: occupied interval `[StartUtc,EndUtc)`؛ contiguous sessions بدون overlap مجاز است.
- PassengerUsageLimit: scope دقیق `PerOrder`, `PerFlightOccurrence`, `PerServiceDate`، نه شمارش مجموع همه سفرها مگر تحت قرارداد Entitlement.

### قواعد availability از نگاه مشتری

`EligibleButOutOfStock` با `NotEligible`, `CommercialNotAvailable`, `UnknownSupplierAvailability`, `OperationallyClosed`, `FreeUnlimited` و `BlockedSeat` یکسان نیست. Product ممکن است درست Price شود اما availability برای رزرو معلوم نباشد؛ هیچ قیمت یا S7 rule نباید به‌تنهایی فروش را تضمین کند.
