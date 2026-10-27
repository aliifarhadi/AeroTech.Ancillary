# 06 — Handoff مصرف‌کننده Ordering؛ No Callback و No Membership Validation

## قرارداد مالکیت
Caller (Ordering یا Consumer مجاز) از Shopping `Resolve` انتخاب فروش‌رفته را می‌گیرد، Order و OrderService را در **سامانه خود** نگهداری می‌کند، و کوچک‌ترین Payload ممکن برای Ancillary را تولید می‌کند. Ancillary از IDهای Caller به‌عنوان Correlation استفاده می‌کند؛ **هیچ خواندن از Ordering، ACL، Membership Check، OrderStatus Check، Price/Resolve lookup یا Event-based approval** انجام نمی‌دهد.

## Mapping از Resolve به Hold
| Resolve / Caller | Hold input | Action |
|---|---|---|
| `definition.variantCode` | `family[].variantCode` | انتخاب Family مطابق `definition.profile` |
| `provision.provisionId` | `products[].provisionId` | Product locator واقعی؛ `productKey` presentation را نفرست |
| Caller new `OrderService.Id` | `units[].orderServiceId` | فقط Correlation؛ membership validate در Caller |
| Offer `unit.travellerRef` + Order Traveller mapping | `unit.travellerId` | عدد واقعی از Caller؛ برای Scope Order حذف |
| Offer `coverage.flightRefs` + Order Flight mapping | `coverage.flightIds` | عدد واقعی منطبق با local inventory keys؛ Scope را نفرست |
| `selected.quantity` | `selected.quantity` | اگر متغیر یا لازم؛ ثابت را Provision derive می‌کند |
| `selected.options.baggage` | `selected.options.baggage` | فقط BagRef, Weight, Dimensions مطابق variant |
| `selected.options.assistedTravel` | `selected.options.assistedTravel` | SSR برای A15/A16 فقط |
| `selected.options.airportService` | `selected.options.airportService` | appointment/guests |
| `selected.options.priority` | `selected.options.priority` | optIn true |
| `selected.options.connectivity` | `selected.options.connectivity` | DeviceCount اگر انتخاب دارد |
| Fixed `Definition.MealCode/MenuItemRef`, etc. | **حذف از Hold input** | Ancillary آن را از Catalog محلی با قواعد Price snapshot می‌کند |
| `selectionId` (`s_...`) / `offerId` / pricing total | **نباید ارسال شود** | قیمت/Order ownership بیرون Ancillary |
| `coverageScope`, `serviceDefinitionId`, stock source | **نباید ارسال شود** | Derived locally by Ancillary |

## نمونهٔ نگاشت A15
Caller sees `variantCode=A15`, `provision.provisionId=703`, `selected.options.assistedTravel.assistanceSsrCode=WCHR`, `travellerRef=ADT-1` و در Order رابطه ADT-1→TravellerId=1001, FlightRef→FlightId=801 ثبت کرده. Payload **تنها** واحد:
```json
{"assistedTravel":[{"variantCode":"A15","products":[{"provisionId":"703","units":[{"orderServiceId":"503","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]},"selected":{"options":{"assistedTravel":{"assistanceSsrCode":"WCHR"}}}}]}]}]}
```
Header `idempotencyKey/orderId/reference/requestedExpiresAt` طبق سند 02 اطراف آن افزوده می‌شود. هیچ تماس متقابل Ancillary به Ordering تعریف نشود.

## Seat
Ordering قبل از Hold از `SeatMaps/Resolve` یا SeatMap selection واقعی داده معتبر می‌گیرد. `SeatMaps/Holds` Commercial Ancillary Unit را ثبت می‌کند؛ مسیر FlightFlow Seat physical در Ordering جدا است. Sequence و Atomicity **cross-service** را Ordering مدیریت می‌کند؛ Ancillary نه Failure FlightFlow را observe می‌کند نه compensating calls می‌زند.

## محدودیت آگاهانه
بدون دوباره‌خوانی `Price/Resolve`، Ancillary نمی‌تواند ثابت کند Hold به همان Selection `s_` قبلی تعلق دارد؛ صرفاً Product/Provision/Policy و Options ارسالی را با قوانین محلی می‌سنجد. این Tradeoff مصوب است و نباید با Token Registry، cross-service ACL یا «اثبات قیمت» پنهانی دور زده شود.

## Guard مصرف‌کننده
Caller خودش OrderService duplication، Traveller/Flight membership، تاریخچه قیمت و Refund/EMD، و نگاشت ثابت variant→family را مسئول است. آزمایش اتصال Ordering E01–E03/E06 تا به‌صورت مستقل اجرا نشود `NOT VERIFIED` باقی می‌ماند؛ شواهد v11 ادعای موفقیت آن را ندارند.
