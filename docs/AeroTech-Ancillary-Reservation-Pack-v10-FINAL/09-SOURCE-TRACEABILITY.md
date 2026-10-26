# 09 — Source Traceability؛ منبع، گپ و مرز دقیق تغییرات

**Pin:** Ancillary `feat/ancillary-offers-v9@2025efd4fbdb4ddb9c72a1ac1eb5023c0b29a3c0`; FlightFlow `k8s-stg@962f41880df0f10a1d105559c233af63ef9db968`.

| Area | Existing real file (prefix `src/AeroTech.Ancillary.` unless stated) | واقعیت سورس | مورد لازم v10 |
|---|---|---|---|
| Root | `Domain/AncillaryReservationAggregate/AncillaryReservation.cs` | Hold status فوری، Confirm/Release/Cancel all | Unit-targeted transitions، correct mixed Unit states، expiry required |
| Unit | `Domain/AncillaryReservationAggregate/Entities/AncillaryReservationUnit.cs` | IDs, Scope, Qty, SeatNumber, StockPoolId placeholder | `Allocations`, `AppendIdempotencyKey`, فقط 2 Fact (`AppointmentTimeWithOffset`, `GuestCount`) |
| Hold Application | `Application/AncillaryReservationAggregate/Commands/HoldAncillaryServices/HoldAncillaryServicesService.cs` | Validate Def/Provision/Supplier؛ ReserveOrderService check؛ Save؛ no stock | Lock+source allocation atomic، reject unsupported |
| Hold DTO | `Application/AncillaryReservationAggregate/Commands/HoldAncillaryServices/AncillaryHoldInputs.cs` | general fields؛ Seat fields optional | minimal conditional `InventoryFacts`; seat-specific از Generic جدا بماند |
| Hold result | `Application/.../HoldAncillaryServices/AncillaryHoldResult.cs` | root + per-unit ids/status | reuse، no duplicate DTO |
| General Controller | `RestApi/V1/AncillaryReservationAggregate/Controllers/ServiceController.cs` | Create/Read/Confirm/Release/Cancel | `unitIds` اختیاری روی Query `DELETE` و بدنه `Cancellations`؛ هیچ Route تازه |
| Seat Controller | `RestApi/V1/SeatMaps/Controllers/ServiceController.cs` | Resolve + Holds | `holdId?` به SeatMap Holds؛ no other route |
| Seat Application | `Application/AncillaryReservationAggregate/Commands/HoldSeatSelections/HoldSeatSelectionsService.cs` | Verify commercial then new general Hold | holdId present -> append same Root, no FlightFlow calls |
| Seat verifier | `Query/Shopping/SeatMaps/Queries/VerifySeatSelections/SeatSelectionVerifier.cs` | price/eligibility+SeatMap catalog؛ no occupied call | preserve unchanged؛ do not add physical check |
| Inventory policy | `Domain/AncillaryInventoryPolicyAggregate/AncillaryInventoryPolicy.cs` | Unlimited/Local/Supplier، Pattern، Count/Weight/Slot refs | bind active policy to real local sources at Hold |
| Local Count | `Domain/FlightCountInventoryAggregate/FlightCountInventory.cs` | TotalCapacity و Admin AdjustTo | sum ledger under source lock؛ Admin below active blocked |
| Local Weight | `Domain/FlightWeightInventoryAggregate/FlightWeightInventory.cs` | CapacityKg و AdjustTo | kg ledger; precision; no capacity decrement |
| Local Slot | `Domain/AirportSlotInventoryAggregate/AirportSlotInventory.cs` | CapacityPersons, UTC slices, facility refs | overlapping slot ledger; contiguous cover |
| Slot DB lock | `Persistence/AirportSlotInventoryAggregate/AirportSlotInventoryRepository.cs` | SQL Server sp_getapplock Session for facility admin | equivalent transaction-owned source locks for Hold |
| Allocation | no existing table/class in reviewed branch | **gap** | one new child entity/table, index/FK |
| Reservation DB | `Persistence/AncillaryReservationAggregate/AncillaryReservationConfiguration.cs` | FK Units, unique root idempotency | new child relationship; stable schema |
| Unit DB | `Persistence/AncillaryReservationAggregate/AncillaryReservationUnitConfiguration.cs` | Seat columns, CoveredFlightIds primitive | 3 Facts+AppendKey+Allocation relationship |
| Repo | `Persistence/AncillaryReservationAggregate/AncillaryReservationRepository.cs` | Read Held/Confirmed OrderService refs، expire reads | status + expiry stock query with SQL locks |
| Expiry | `Application/AncillaryReservationAggregate/Commands/ExpireAncillaryHolds/ExpireAncillaryHoldsService.cs` | marks expired persisted | keep; calculate stock lazy before worker |
| Price | `Application/SelectionSnapshotAggregate/Commands/PriceAncillaryOffers/PriceAncillaryOffersService.cs` | `s_` snapshot TTL and selection data | **NO changes** |
| Resolve | `Query/Shopping/AncillaryOffers/Queries/ResolveAncillarySelection/Service/ServiceResolvedSelectionsMapper.cs` | full accepted typed details | **NO changes**؛ Ordering adapter maps to Hold |
| Seat physical | `AeroTech.FlightFlow/FlightFlow/src/AeroTech.FlightFlow.RestApi/V1/FlightAggregate/Controllers/ServiceController.cs` | Seat-Holds/Confirm/Release | **No Ancillary call or edit** |

## 1. کدام فایل‌ها نباید تغییر کنند؟
`Shopping/Offers`, `Shopping/Engine`, `Query/Shopping/AncillaryOffers` Price/Resolve, `Query/Shopping/SeatMaps` shopping/resolve, `Domain/AncillaryPricingAggregate`, `Domain/AncillaryServiceDefinitionAggregate`, `FlightFlow` و `Ordering`. دست‌کاری Verifier SeatMap فقط اگر Test واقعی ثابت کند بدون آن Compilation یا commercial correctness ممکن نیست و باید جدا با گزارش به مالک برگردد؛ نه refactor اختیاری. **تنها استثنای مستند (تصمیم مالک Gate 0):** اگر تست ثابت کند محصولی در Price پذیرفته می‌شود ولی به‌علت Supplier بدون Adapter در Hold همیشه رد می‌شود، یک Guard کوچک در همان مسیر عرضه/Price با تست منفی (Gate 4)؛ بدون Engine یا DTO جدید.

## 2. از کجا نباید سورس را کپی کنیم؟
- `docs/AeroTech-Ancillary-Master-Pack-v13.2-FINAL/10-S3-RESERVATION-AGGREGATE-LIFECYCLE.md` مدلی از Pending و Supplier recovery پیشنهاد کرده بود؛ در این scope پیاده نمی‌شود.
- `docs/AeroTech-Ancillary-Phase3-v1.0/04-RESERVATION-PROTOCOL-ENDPOINTS-AND-INVARIANTS.md` طراحی قبلی را دارد؛ تنها شواهد وضعیت قبل، نه authority بعد از تصمیم Seat Owner.
- گزارش `SEAT-RESERVATION-CONFORMANCE.md` نشان می‌دهد Seat facade تجاری از قبل ساخته شده؛ قرار نیست دوباره پیاده شود.
- فایل‌های `docs/Shopping/FrontEnd` در Repo ممکن است قرارداد قدیمی Forms/Batch را نشان دهند؛ Agent نباید آن‌ها را مبنای Reservation قرار دهد.

## 3. Gate0 Source Audit required
- قبل از تغییر بررسی شود HEAD دقیقاً Pin بالا یا descendant آن است؛ هر تغییر بعدی از HEAD در گزارش به مالک اعلام شود.
- framework DB transaction، Serializer JSON، ExceptionFactory، UnitOfWork، IAM/RBAC Service Controller و inventory SQL provider شناخته شوند.
- اگر Ordering source inaccessible است، ادعای verified mapping یا واقعی Full E2E Order-to-Ancillary ممنوع؛ contract tests مستقل + gap report تهیه شود.
- قبل از migration فهرست Reservationهای Held موجود و AllocationSourceهای فعلی گزارش شود تا تاریخچه اعتبارسازی نشود.
