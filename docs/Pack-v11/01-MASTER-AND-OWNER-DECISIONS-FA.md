# 01 — Master Reservation v11 — Domain First، کامل و عملیاتی

## A. علت دقیق نقص v10 و تصمیم حفظ/بازنویسی
**ریشه:** Pack v10 ابتدا ledger، SQL lock، expiry و وضعیت‌ها را طراحی کرد، اما پرسش بنیادی «مشتری دقیقاً چه Ancillary با کدام گزینه را رزرو کرده است؟» را پیش از طراحی DTO و Aggregate پاسخ نداد. `AncillaryReservationUnit` فقط DefinitionId/ProvisionId، `CoverageScope`، FlightIds و Qty را داشت؛ انتخاب‌های `Selected.Options` از Price/Resolve (SSR، حیوان/ابعاد، appointment، ...)، به‌جز Seat و دو Fact Slot، داخل Reservation ذخیره نشد. تست‌ها mostly inventory و lifecycle بودند، نه **Round-trip انتخاب تجاری**. بنابراین کد «کار می‌کند» ولی Contract بد و Domain ناقص می‌ماند.

**حکم:** کد v10 **پاک نشود**؛ Aggregate Root، Unit identity، status/idempotency، Seat facade، Ledger، child Allocation، SQL transaction و تست‌های واقعی **حفظ**؛ تنها API Hold و Read، typed Selection و Persistence متناظر بازطراحی شود. کد deprecated بعد از تست حذف گردد، نه قبل. `Retire` منابع دارای مصرف فعال یک نقص ایمنی مستقل P0 است.

## B. حدود مالکیت
- Ordering: Order/OrderService، انتخاب پذیرفته‌شده، تطبیق Traveller و Flight با Order در **خود Ordering**؛ EMD/Ticket/Payment و هماهنگی FlightFlow؛ درخواست S2S را می‌سازد.
- Ancillary: فقط ورودی S2S را دریافت می‌کند؛ **هیچ Call/lookup/validation membership با Ordering، Offer، Price، Snapshot قبلی** ندارد. تنها واقعیت‌های محلی Definition، Provision، Active Policy، Candidate variant و Rules، Qty، Inventory Source/Capacity، expiry، replay، status بررسی می‌شوند.
- FlightFlow: موجودی واقعی صندلی هواپیما؛ هیچ Seat Hold از Ancillary به FlightFlow در v11 نیست.
- Source of truth: Reservation برای **جزئیات پذیرفته‌شده عملیاتی** و Inventory Allocation؛ Ordering برای قیمت/مالی/سند. قرارداد Caller مبتنی بر اعتماد و احراز هویت Service است، نه ادعای Proof تاریخی.

## C. Flow دقیق
1. `AncillaryOffers` به Frontend `baggages[]`, `meals[]` ... می‌دهد؛ Variant Card/Product/Unit و options inline.
2. Frontend به `/Price` انتخاب‌های نوعی را می‌دهد؛ Price یک `selectionId=s_...` سبد می‌دهد. `Resolve` سپس `ServiceResolvedSelectionDto<T>` با `provision.provisionId`, `definition.variantCode`, `unit`, `coverage`, `selected` و `details` برمی‌گرداند.
3. Ordering از `Resolve` و Order **در سمت خودش** حداقل Payload رزرو را می‌سازد؛ برای هر Family کارت/Variant/Product/Units. **در Ancillary هیچ رجوعی به Ordering یا Price وجود ندارد**.
4. `POST Hold`: for each product Ancillary reads the Active Provision from `provisionId`, then the Definition, Variant, Scope and InventoryPolicy. It validates only the selected fields of that variant against the static rules of the Definition and Provision, captures the fixed choices of the Definition with the same rule Price uses (both through the port in 05 §1), makes the final selection immutable, allocates Count/Weight/Slot stock in one transaction under the source lock, saves Root + Units + Allocations, and returns a small Receipt.
5. Ordering درخواست اختصاصی `SeatMaps/Holds` را برای Seat-only یا Append به همان Hold ارسال می‌کند. Ancillary فقط commercial verifier و lifecycle را اجرا می‌کند؛ ۰ تماس فیزیکی/OccupiedSeats.
6. `Confirm` روی Root واحد، حتی اگر بعضی Units Released شده باشند. `DELETE Hold/{holdId}` همه یا فقط UnitIds[] Held را آزاد می‌کند. `Cancellations` Confirmedها را طبق reason cancel می‌کند. Expiry برای Held منطقی و persisted است.
7. `GET Hold/{holdId}` historical typed selection را از **داده پایدار همان Reservation** می‌دهد، حتی پس از انقضا/بازنشستگی Definition/Price Snapshot؛ بدون lookup بیرونی.

## D. قرارداد ذهنی Wire — متقارن با AncillaryOffers
`AncillaryOffers` = `baggages → [{variantCode,products:[{productKey,...,units:[{unit,coverage,quantityPolicy,selectionId/reference,...}]}]}]`.

`Hold` = **همان nesting و namesِ قابل انتقال**: `baggages → [{variantCode,products:[{provisionId,units:[{orderServiceId,unit,coverage,selected}]}]}]`. برای Pet/Meal/Assisted/Airport/... همین الگو. **فقط داده موردنیاز رزرو**: Product در Hold به جای presentation `productKey` دارای `provisionId` (از Resolve) است؛ `unit` و `coverage` در Hold، شناسه‌های بالفعل Order یعنی `travellerId` و `flightIds` را دارند، زیرا `travellerRef`/`flightRefs`ِ Offer به‌تنهایی Stock/OrderServiceId را مشخص نمی‌کنند. شرح دقیق تبدیل در 02؛ هیچ `serviceDefinitionId`, `coverageScope`, `inventoryFacts` از Caller مطالبه نشود.

`selected` از **نام/شکل موجود `Resolve.selected`**: `quantity:int`, `options:{baggage|pet|assistedTravel|airportService|priority|connectivity: {existing-fields}}`. برای Qty ثابت=1 و بدون گزینه، selected می‌تواند حذف شود؛ `quantity=1` در Domain از Provision مشتق. اگر گزینه مشتری متغیر و معتبر نیست، Product gated و نه guessed.

## E. Root/Unit در Domain
- یک `AncillaryReservation` با `Id`, `IdempotencyKey`, `OrderId`, `Reference`, `ExpiresAt`, `Status`, `Units[]` و عملیات Root `Hold`, `AppendSeats`, `Confirm`, `Release(unitIds?)`, `Cancel(reasonCode,unitIds?)`, `Expire`.
- هر `AncillaryReservationUnit` دارای `Id`, `OrderServiceId`, `ProvisionId`, **derived** `ServiceDefinitionId`, `VariantCode`, `CoverageScope`, `TravellerId?`, `CoveredFlightIds`, `Quantity`, `Status`, **Typed Selection** و `Allocations[]` می‌باشد.
- Child `AncillaryReservationUnitAllocation` = `(ResourceKind,InventorySourceId,Amount)` immutable؛ Hold، Available را از Active Held+Confirmed روی ledger می‌سنجد.
- The typed selection holds the business choice only: baggage options, SSR code, airport appointment and guests, connectivity device count, priority opt-in, seat number. Fixed facts of the Definition are snapshotted with the unit: `serviceDefinitionRef` for every unit (it is also the A02 package and the A24 plan), and `equipmentKind`, `mealCode`, `menuItemRef`, `toCabinId`, `airportId`, `facilityId` where the variant has them. No clone of the Pricing or the Offer.
- ۲۴ Aggregate یا ۲۴ Entity ایجاد نشوند؛ ۶–۸ Selection VO/Owned type حداقلی فقط در صورت نیاز variant فعال. اطلاعات ثابت مؤثر Fulfillment در لحظه Hold snapshot می‌شود. **اولین Hold GET باید انتخاب قبلی را بدون live Definition بازگرداند**.

## F. Inventory و موارد GATED
- `Unlimited`: بدون Allocation؛ `Local FlightCount`, `FlightWeight FixedKg`, `FlightCountPlusWeight`, `AirportSlot`: با ledger فعلی و قفل SQL مشترک؛ `TotalCapacity` تغییر نکند؛ هیچ counter موازی یا coordinator عمومی ایجاد نشود.
- `Supplier` without a real adapter and `AcceptedWeightKg`/`PerKilogram` are unsupported. GATED variants (Owner, Q1): A09 ExtraSeat (no sale path), A10 with a quote (only `FixedAncillary` is supported), A13/A14 pets (the domain forces `SubjectToConfirmation`), A17 medical and A19 unaccompanied minor (gated in Price), A18 bassinet (`infantRef`/`guardianRef` are offer-scoped refs). An unsupported path always returns an explicit error, never a fake Held.
- SeatMap تنها commercial unit و بدون Allocation. **Retire یک Source دارای ActiveUsage>0 باید 409** زیر همان قفل فعلی بدهد. Connection pool tuning عملیاتی است، نه علت بازطراحی.

## G. عملیات بیرونی و گزارش دقیق
`POST Hold`؛ `GET Hold/{id}`؛ `POST .../Confirmations`؛ `DELETE Hold/{id}?unitIds=...`؛ `POST .../Cancellations`؛ `POST SeatMaps/Holds` — **۶ Endpoint و فقط همین‌ها در Scope رزرو**. جزئیات و JSON کامل تک‌تک در 02.

## H. معیار Closure
برای تمام Variantهای اعلام‌شده Supported: `AncillaryOffers/Resolve.selected` → `Hold wire` → SQL → GET **بایت/مقدار انتخابی برابر**؛ تداخل Stock بر اساس SQL؛ بدون Order client در Ancillary؛ Seat physical untouched؛ replay/expiry/partial statuses؛ plain migration on emptied dev reservation tables (Q5)؛ تمام تست‌های 07 با نام و فرمان و نتیجه واقعی. **`NOT VERIFIED` و `GATED` هرگز PASS نیستند**.

## I. Owner decisions at Gate 0 (2026-10-26), merged into this pack
| # | Decision | Merged into |
|---|---|---|
| Q1 | Supported: A01, A02, A03 (PerPiece), A04, A05, A06, A10 (`FixedAncillary` only), A11, A12, A15, A16, A20, A21, A22, A23, A24; seats A07/A08 through `SeatMaps/Holds`. GATED: A09, A10 with a quote, A13/A14, A17, A19, A18. | 01 §F, 02 §1.4, 03, 04 §3, 07 D, 12 |
| Q2 | Option validation and fixed-choice capture go through one Application port (pattern of `ISeatSelectionVerifier`) implemented in Query with the Price rules (`TypedSelectionForm.Fields`, `TypedSelectionForm.WithFixedChoices`). Depth: static Definition/Provision rules only (required fields, allowed values, numeric ranges); no commercial eligibility. Shopping does not change. | 01 §C.4, 02 §1.5, 05 §1, 06 §A, 07 D23, 12 |
| Q3 | GET has a `seats` family with the same variant → product → unit layering; a seat unit shows `seatNumber` and `seatMapSelectionId`; `pricingId` is not exposed. | 02 §3, 07 W25, 11, 14 |
| Q4 | GET product = `{provisionId, serviceDefinitionRef, details?, units}`; `details` is a small record per family with existing Offers names only: `equipmentKind`; `mealCode`, `menuItemRef`; `toCabinId`; `airportId`, `facilityId`. Not a copy of the Definition. | 02 §3, 03 §ب, 04 §2–3, 07 W26 |
| Q5 | Dev DB data is test data: all reservation rows on dev are deleted; migrations are plain (no backfill); `CoveredFlightIds` becomes the child table `AncillaryReservationUnitFlights`. | 04 §5, 05 §3, 06 §B, 07 R01, 12 |
| Q6 | For the Gate 5 live run, create on dev one A15 with `Immediate` confirmation and one A20 with a local Supplier and an Unlimited policy. | 07 R05, 15 |
| Wire | Gate 0 fixed the representation: camelCase names, enums written by name (numbers accepted), every `long` written as a JSON string (numbers accepted). | 02 §0 |

## J. Owner decisions on the Hold contract R1 (2026-10-26), merged into this pack
Source: `reports/V11-HOLD-CONTRACT-R1-QUESTIONS-AND-CONFLICTS.md` (the Owner accepted every recommendation).

| # | Decision | Merged into |
|---|---|---|
| R1-Q1 | The request is typed per family with the Offers generic pattern: `AncillaryVariantHoldInput<TSelected>` → `AncillaryProductHoldInput<TSelected>` → `AncillaryUnitHoldInput<TSelected>`. Option families take `{quantity?, options?{familyKey}}` with only their own key; `meals` and `upgrades` take `{quantity?}`. A valid request keeps the same JSON; a foreign key fails binding (400). | 02 §1.2–§1.5, 03, 07 W10 |
| R1-Q2 | The GET types `selected` per family the same way (typed records, null members omitted); seat units have their own record with `seatNumber` and `seatMapSelectionId`, which no other family carries. Storage does not change. | 02 §3, 07 W29 |
| R1-Q3 | The Hold's `assistedTravel` options carry `assistanceSsrCode` only; the `pets` array leaves the Hold request until a stage opens pets (400 when sent). | 02 §1.1, §1.4, §1.5, 03, 07 W28 |
| R1-Q4 | `unit.travellerId` is required whenever `unit` is sent; `unit:{}` is refused. | 02 §1.3, 07 W28 |
| R1-Q5 | The same six-key union in the Offers and Resolve output (`AncillaryUnitOfferDto.Options`, `SelectedUnitDto`) is fixed later in a separate step, only on the Owner's order. | — |

## K. Owner decisions at Gate 4 (2026-10-27), merged into this pack
Source: `reports/V11-GATE4-QUESTIONS-AND-CONFLICTS.md`.

| # | Decision | Merged into |
|---|---|---|
| G4-Q1 | Gate 4 HTTP evidence covers only what needs no catalog (empty versus absent `unitIds`, strict-JSON 400s, 404 for an unknown hold, SeatMaps/Holds 400), on a host of the agent's own; live business flows stay at Gate 5 (Q6). | 14 |
| G4-P1 | 16413 (cancel replay with another reason) is listed with the kept codes. | 02 §5 |

## L. Owner decisions at Gate 5 (2026-10-27), merged into this pack
Source: `reports/V11-GATE5-QUESTIONS-AND-CONFLICTS.md`.

| # | Decision | Merged into |
|---|---|---|
| G5-Q1 | The Q6 catalog (A15 `Immediate`; A20 on a local Supplier with `RequiresSpecificAppointment = true` and an Unlimited policy) is created through the Backoffice routes of the agent's own host with the Owner's Backoffice token, so the Domain validates it. | 15 |
| G5-Q2 | The live seat append uses a seat-map selection id from `POST OtaPanel/v1/SeatMaps` on the Owner's priced AirAvail offer. | 15 |
| G5-Q3 | "GET after the Definition is retired" uses one extra throwaway A15 (`Immediate`); the Q6 products stay active. | 15 |
| G5-R1 | The reservation routes are `Service/v1/Ancillaries/Hold`, `Hold/{holdId}`, `Hold/{holdId}/Confirmations`, `DELETE Hold/{holdId}` and `Hold/{holdId}/Cancellations` (renamed from `Service-Holds`, Owner 2026-10-27); `SeatMaps/Holds` is unchanged. | 01, 02 |
