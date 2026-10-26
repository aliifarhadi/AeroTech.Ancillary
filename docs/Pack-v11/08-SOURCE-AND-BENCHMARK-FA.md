# 08 — شواهد Source، دلایل تصمیم و بنچ‌مارک؛ تفکیک صریح

## الف. حقایق سورس تأییدشده (کد v10 HEAD)
- `RestApi/.../HoldAncillaryServicesRequest.cs`: Request شامل `idempotencyKey`, `orderId`, `reference`, `requestedExpiresAt`, `services[]` است.
- `Application/.../AncillaryHoldInputs.cs`: ورودی flat شامل `orderServiceId`, `serviceDefinitionId`, `provisionId`, `travellerId`, `coverageScope`, `coveredFlightIds`, `quantity`, `inventoryFacts` است. لذا مشکل Leakage واقعی است.
- `Domain/.../AncillaryReservation.cs`: Root با `Hold/AppendSeats/Confirm/Release/Cancel/Expire` و Unit child موجود است.
- `Domain/.../Entities/AncillaryReservationUnit.cs`: `Baggage/Meal/Pet/SSR` انتخاب تخصصی ندارد؛ SeatNumber و دو فیلد Slot دارد. لذا نقص Snapshot واقعی است.
- `Persistence/.../InventoryLedger.cs`: SQL `sp_getapplock` و allocation predicate منابع مشترک واقعی است؛ بازنویسی منطقی نیست.
- `Query/Shopping/AncillaryOffers/Dto/Service/ServiceResolvedSelectionDto.cs`: `OfferItemId,VariantCode,SelectionScope,Unit,Coverage,SourceBinding,Definition,Provision,Pricing,Details,Selected`، با `SelectedUnitDto(Quantity, Options)`.
- `Query/Shopping/AncillaryOffers/Dto/Service/ServiceResolvedProvisionDto.cs`: `ProvisionId, CoverageScope` و مشخصات Booking/Document را دارد. پس Scope و Definition را می‌توان از Provision در Hold خواند.
- `Query/.../AncillaryOffers/Dto/OfferCoverageDto.cs`: `travellerRefs`, `portionRefs`, `flightRefs` **string arrays**؛ Stock Source در Inventory با FlightId numeric شناخته می‌شود. بنابراین `coverage.flightIds` عددی در Hold یک ترجمه ضروری Caller است، نه new coverage business concept.
- `Domain/.../CustomerSelectionContract.cs`: A02 PackageProductRef، A06 EquipmentKind، A12 MenuItemRef، A24 PlanCode را نشان می‌دهد، اما `Price/Selected` فعلی فقط ۶ مجموعه Options دارد. هیچ Field بی‌سند به Hold اضافه نکنید؛ اگر fixed unique نیست، GATED.
- `reports/V10-RESERVATION-FINAL-CLOSURE.md`: 287 Domain/254 SQL acceptance و 24 HTTP steps **گزارش‌شده**؛ `Ordering` Integration و Live Local HTTP هنوز NOT VERIFIED/GATED؛ این سند تست‌ها را مستقل اجرا نکرده است.

## ب. مرجع مسیر فایل‌های واقعی
Repository: `https://github.com/aliifarhadi/AeroTech.Ancillary/tree/7a74055c774b2466fe239b8307ec93b15264e50c`.

- [Actual reservation root](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7a74055c774b2466fe239b8307ec93b15264e50c/src/AeroTech.Ancillary.Domain/AncillaryReservationAggregate/AncillaryReservation.cs)
- [Actual Hold REST request](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7a74055c774b2466fe239b8307ec93b15264e50c/src/AeroTech.Ancillary.RestApi/V1/AncillaryReservationAggregate/Requests/HoldAncillaryServicesRequest.cs)
- [Actual Unit entity](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7a74055c774b2466fe239b8307ec93b15264e50c/src/AeroTech.Ancillary.Domain/AncillaryReservationAggregate/Entities/AncillaryReservationUnit.cs)
- [Existing Offer Group DTOs](https://github.com/aliifarhadi/AeroTech.Ancillary/tree/7a74055c774b2466fe239b8307ec93b15264e50c/src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Dto)
- [Actual Resolve typed DTO](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7a74055c774b2466fe239b8307ec93b15264e50c/src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Dto/Service/ServiceResolvedSelectionDto.cs)
- [Real Selection Options Contract](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7a74055c774b2466fe239b8307ec93b15264e50c/src/AeroTech.Ancillary.Application/SelectionSnapshotAggregate/Commands/PriceAncillaryOffers/SelectionOptionsInput.cs)

## ج. Benchmark جهانی و نسبت آن با تصمیم محلی
- [Duffel Order Services API](https://duffel.com/docs/api/orders) (رفتار Passenger/Segment و خدمات با شناسه مشخص، Quantity و Order service linkage): اثبات می‌کند هر خدمت باید شناسه و داده عملیاتی قابل فهم خود را داشته باشد. **APIهای v11 ما کپی Duffel نیستند**.
- [Travelport Air Booking Workbench](https://support.travelport.com/webhelp/JSONAPIs/Airv11/Content/Air11/Book/BookingGuide.htm): تفکیک offer/shopping، booking workbench و مراحل فروش؛ مفهومی، نه تأیید مالکیت Inventory داخل Ancillary.
- [IATA NDC](https://www.iata.org/en/programs/airline-distribution/retailing/ndc/): تفکیک Offer/Order و ارجاع‌های Pax/Service؛ به معنای استاندارد بودن `Family→Variant→Product→Unit` **نیست**؛ این ساختار تصمیم خاص AeroTech برای UI ساده و قرارداد S2S خواناست.
- FlightFlow sibling فقط **اصل Hold/Confirm/Release، idempotency و expiry** را الهام داده؛ Domain/Entity/Endpoint v11 عین FlightFlow کپی نمی‌شود.

## د. چرا «Typed outside, shared inside» درست است؟
| Choice | Benefit | What it avoids |
|---|---|---|
| Grouped-family Request consistent with AncillaryOffers | consumer knows category; low branching | `services[]` ambiguous generic bag |
| `selected` same field names as Resolve | no extra mapping of Customer options | invented `bagOptions` or `appointmentFacts` |
| Variant+Provision guard | caller cannot spoof product family/Scope | client-provided `coverageScope` |
| internal typed snapshot of selection | reconstruct full selected service without old Definition | ghost reservation just with stock |
| keep v10 Ledger + Root | reliable race-free stock / lifecycle | total rewrite and regression |
| no Ordering call/membership validation | precise service boundary | new ACL/Saga/performance coupling |

**حد مسئولیت مهم:** No comparison against previous `Price selectionId` → cannot prove the exact old customer choice, only exact accepted Hold payload. End-to-end fidelity is jointly an external mapping contract; this is not a reason to add an Ordering call to Ancillary.
