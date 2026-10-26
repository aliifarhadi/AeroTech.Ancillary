# 05 — SeatMap و Handoff سرویس Ordering — قرارداد دقیق بدون موجودی فیزیکی در Ancillary

## 1. مالکیت‌های قطعی
| موضوع | Owner این فاز | Ancillary مجاز است؟ |
|---|---|---|
| Commercial Seat Selection (تعیین محصول/نرخ/قیمت تجاری) | Ancillary | بله، SeatMap Engine و Verifier فعلی |
| Ancillary commercial hold/confirm/release/cancel برای Seat | Ancillary | بله، از طریق `AncillaryReservation` |
| اشغال فیزیکی SeatNumber، جلوگیری از رزرو تکراری بین سفارش‌ها | FlightFlow به دستور Ordering | **خیر** |
| `Seat-Holds`, `OccupiedSeats`, `SeatHoldReference` | Ordering/FlightFlow | **هیچ call تازه** از Ancillary |
| OrderService/Ticket/EMD/Refund و نگاشت Traveller | Ordering | خیر، فقط S2S facts معتبر قبول می‌کند |
| انقضای physical seat hold | Ordering/FlightFlow | خیر، فقط انقضای تجاری Ancillary |

**تعریف دقیق:** اگر SeatMap commercial Hold در Ancillary موفق بود، آن سرویس انتخاب صندلی از نظر Ancillary Held شده است؛ این **تضمین اشغال فیزیکی صندلی نیست**. Ordering مسئول هماهنگی دو نتیجه است. این یک تصمیم فعلی مالک است و هیچ `FlightFlowSeatStatus`، `OccupiedSeat` یا `PhysicalReservationProof` را به Ancillary تحمیل نکنید.

## 2. کد قابل Reuse
- `src/AeroTech.Ancillary.RestApi/V1/SeatMaps/Controllers/ServiceController.cs` موجود: `POST /Service/v1/Ancillaries/SeatMaps/Resolve` و `POST /Service/v1/Ancillaries/SeatMaps/Holds`.
- `HoldSeatSelectionsService` موجود: از `ISeatSelectionVerifier.VerifyAsync` استفاده و خروجی را به Hold عادی map می‌کند؛ همین Facade حفظ شود.
- `SeatSelectionVerifier` موجود: SeatMap catalog و pricing/eligibility را با facts دریافت‌شده می‌سنجد. در این کد `SeatMapSeat(..., false)` بدون occupied inventory از FlightFlow ساخته می‌شود. **این رفتار از نظر تفکیک مالکیت حفظ شود**؛ اجبار به استعلام OccupiedSeat اضافه نشود.
- `SeatMapSelectionId` فعلی شناسه مسافر/پرواز/صندلی/نسخه Pricing را حمل می‌کند؛ دست‌کاری فرمت و Endpoint آن ممنوع.

## 3. Flow (Seat only)
1. Ordering از FlightFlow **در مسیر فعلی خود** flight+requested seat hold فیزیکی گرفته است یا خودش هماهنگی آن را انجام می‌دهد.
2. Ordering `POST SeatMaps/Holds` با `holdId` خالی، `idempotencyKey`, `orderId`, `orderCommercialVersion`, `reference`, `requestedExpiresAt`, `pointOfSaleId`, `currencyId`, `seats[]` می‌فرستد.
3. SeatMap Verifier اطلاعات بیزنسی همان Seat را بررسی می‌کند؛ بدون استعلام occupied/FlightFlow.
4. `AncillaryReservation.Hold` تنها با Seat Units و `Allocations=[]` ساخته می‌شود؛ پاسخ همان header و UnitRefs.
5. Confirm/Release/Cancel همه از `Service-Holds/{holdId}` هستند؛ هیچ SeatMap-specific Confirm/Cancel endpoint جدیدی لازم نیست.

## 4. Flow (Mixed — NonSeat + Seat)
1. Ordering `POST Service-Holds` برای تمام NonSeat Units می‌فرستد و `holdId` می‌گیرد.
2. Ordering `POST SeatMaps/Holds` همان seat DTO فعلی + `holdId` این Reservation می‌فرستد.
3. Ancillary **همان Root** را load می‌کند و validate می‌کند: OrderId برابر، Reference برابر، ExpiresAt برابر یا درخواست هم‌معنا، Status Held، not expired؛ Seat IDs جدید، OrderServiceIds یکتا، و idempotency append.
4. Verifier فعلی SeatMap بر اساس input seat products را verify می‌کند، Unitهایی با `Quantity=1`, `CoverageScope=Sector`, `CoveredFlightIds=[FlightId]`, `SeatMapSelectionId`, `SeatNumber`, `PricingId` می‌سازد.
5. در همان Reservation، Seat Unitها ثبت می‌شوند، **no Local allocation**؛ پاسخ `holdId` تغییر نمی‌کند.
6. در صورت عدم موفقیت مرحله 2، Root NonSeat همان Held می‌ماند، و **Ordering** تصمیم Release یا Retry می‌گیرد؛ هیچ Saga در Ancillary اضافه نکنید.
7. Confirm همه فعال‌ها با یک call، Cancel/Release همچنان عمومی.

### Idempotency Append
- `idempotencyKey` در درخواست SeatMap append **کلید همان عملیات append** است؛ روی Unitهای جدید با `AppendIdempotencyKey` ذخیره می‌شود؛ جدول عملیات جدا ساخته نمی‌شود.
- همان `holdId`+key+exact seats دوباره ⇒ response Hold جاری بدون append جدید/ذخیره دوباره؛ همان key با seats متفاوت ⇒ 409.
- Seat order-service موجود با key جدید ⇒ conflict؛ no silent overwrite.
- عملیات append و root change با lock OrderId موجود و DB transaction انجام شود.
- در صورت replay پس از تغییر قیمت/Policy، اول canonical saved request را مقایسه کن و اگر exact است همان نتیجه قبلی را بده؛ verification جدید قیمت نباید یک Hold واقعاً ایجادشده را در retry بی‌اعتبار کند.

## 5. Ordering → Ancillary mapping؛ field by field
| Ordering source | Hold destination | Verified by / nuance |
|---|---|---|
| `Order.Id` | `orderId` | Ordering/Adapter |
| `OrderService.Id` | `orderServiceId` | Ordering؛ Ancillary duplicate-live check |
| `Resolved.Definition.Id` | `serviceDefinitionId` | Ancillary Definition/Provision check |
| `Resolved.Provision.Id` | `provisionId` | Ancillary FK rule/status |
| `OrderTraveller.Id` | `travellerId` | Ordering mapping از `Resolve.Unit.TravellerRef`; صرفاً TravelRef خام را به long parse نکنید |
| `OrderFlight.Id` / verified flight physical ids | `coveredFlightIds[]` | Ordering mapping از `Resolve.Coverage.FlightRefs/PortionRefs`؛ Portion می‌تواند چند Flight داشته باشد |
| `Resolved.Provision.CoverageScope` | `coverageScope` | `ServiceCoverageScope` enum موجود، نه `SelectionScope` REST |
| `Resolved.Selected.Quantity` | `quantity` | Ordering accepted quantity؛ recheck against Provision |
| Accepted Airport Service appointment | `inventoryFacts.timeWithOffset` | فقط Slot؛ UTC equivalent برای allocation |
| Accepted Airport Service guest count | `inventoryFacts.guestCount` | فقط Slot؛ صفر یا positive per selected |
| `SeatMap.Resolve` + seat info in Ordering | SeatMap `seats[]` fields | Verifier اختصاصی، همان نوع DTO فعلی |
| `FlightFlow SeatHoldReference` | **هیچ Ancillary field** | صرفاً در Ordering/FlightFlow نگهداری می‌شود |
| `Accepted price / taxes / currency` | **هیچ Hold field** | Stored in Ordering، نه در Reservation |
| `selectionId` (`s_`) از Price | **نه ورودی الزامی Hold** | قبل از Hold در Ordering مصرف و accepted snapshot پایدار شده است؛ Expiry shopping نباید Retry رزرو Order ذخیره‌شده را بشکند |

### نکات مرزی
- `Resolved` REST یک `OfferUnitDto` و `OfferCoverageDto` دارد. Unit/TravellerRefs به‌صورت string در Offer scope هستند و `OrderServiceId/TravellerId/FlightIds` باید با Order verified نگاشت شوند. این mapping بدون دسترسی به سورس Ordering.Final در همین Pack یک **integration contract** است، نه ادعای وجود Adapter فعلی.
- Ordering باید قبل از Call Hold، **Order ownership، service принадлежность، funds/guarantee condition برای Confirm، عدم ابطال تجاری** را بررسی کند. Ancillary بدون اتصال به Order DB قادر نیست این claims را خودش اثبات کند.
- `RequestedExpiresAt` بهتر است برابر یا کوتاه‌تر از effective FlightFlow hold deadline همان سفارش باشد. این هماهنگی توسط Ordering انجام می‌شود؛ Ancillary FlightFlow را استعلام نمی‌کند.
- Seat physical خطا داد: Ordering Ancillary Hold را Release می‌کند یا اصلاً آن را ایجاد نمی‌کند. Ancillary هیچ compensation پنهان FlightFlow ندارد.
- Cancel Ancillary commercial Seat **فیزیک صندلی را آزاد نمی‌کند**. Ordering پس از تصمیم درست خودش FlightFlow Cancel/Release را انجام می‌دهد.

## 6. End-to-end نمونه سناریو
- `OrderId=12001`، `SeatMapSelectionId=...`, `seatNumber=12A`, `flightId=801`، `orderServiceId=503`.
- FlightFlow از Ordering seat `12A` را Hold کرده است. Ancillary `SeatMaps/Holds {holdId:9001,...}` را دریافت و Unit `9103` Commercial Held می‌سازد؛ هیچ call به FlightFlow.
- پس از Confirm، Unit `9103` Confirmed و سایر Unitها نیز Confirmed می‌شوند.
- Cancel Unit `9103` با `POST Service-Holds/9001/Cancellations {"reasonCode":"CUSTOMER_REQUEST","unitIds":[9103]}`: commercial Cancelled و بدون Local allocation؛ درخواست واقعی آزادسازی SeatNumber اگر لازم باشد توسط Ordering به FlightFlow ارسال می‌شود. آزادسازی Held همان Seat با `DELETE Service-Holds/9001?unitIds=9103` است.

## 7. تست‌های مرز SeatMap
- Mock/Spy روی Ancillary outgoing HTTP: **صفر** فراخوانی `Flights/OccupiedSeats`, `Flights/Seat-Holds`, `Seat-Holds/{id}/...` از SeatMap Reservation.
- دو seat یکسان در یک Hold رد شوند؛ برای دو Order متفاوت Seat physical collision در Ancillary بررسی نمی‌شود (صرفاً FlightFlow مسئول آن است).
- SeatMap standalone و append هر دو same Domain shape و lifecycle دارند؛ append HoldId ثابت.
- SeatMap Shopping/Resolve/Price hash و output regression unchanged.
- End-to-end واقعی با Ordering/FlightFlow فقط با وجود Repo/Endpoint consumer معتبر انجام می‌شود؛ نبود این دسترسی را PASS گزارش نکنید.
