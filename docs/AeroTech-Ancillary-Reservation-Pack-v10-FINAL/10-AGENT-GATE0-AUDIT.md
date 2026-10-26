# Gate 0 — Coding Agent — ممیزی فقط خواندنی، بدون کد

> **وضعیت: بسته (2026-10-26).** خروجی: `reports/V10-GATE0-SOURCE-AUDIT.md` و `reports/V10-GATE0-QUESTIONS-AND-CONFLICTS.md`. پاسخ شش سؤال: `AeroTech-Ancillary-v10-Gate0-Owner-Decisions-FA.md`، ادغام‌شده در `01` تا `14`. ممیزی دوباره لازم نیست.

**Role:** شما Coding Agent پروژه `AeroTech.Ancillary` هستید. مالک تصمیم‌های Domain/بیزنس را تأیید کرده و Master این Pack مرجع شماست. در Gate 0 **هیچ کد، Migration، Config، تست یا Branchی را تغییر ندهید.** هدف Grounding و شناسایی دقیق تعارض قبل از تولید کد است.

## Read exactly in order
1. `00-START-HERE.md`
2. `01-MASTER-RESERVATION-CONTRACT-FA.md`
3. `02`..`09` به ترتیب
4. repo `feat/ancillary-offers-v9@2025efd4fbdb4ddb9c72a1ac1eb5023c0b29a3c0` یا HEAD جدیدترِ descendant با diff explicit
5. `CLAUDE.md` و framework conventions، `ExceptionFactory`, validation patterns, EF database stack, `IDistributedLock`, migrations, SQL acceptance tests.

## Audit checks
- Baseline SHA, clean tree, branch, source statuses؛ verify existing `AncillaryReservation`/Unit, SeatMap Hold Facade and Unit statuses.
- Check `ServiceCoverageScope` values, InventoryAuthority/Pattern values, QuantityRule, ConsumptionMode and `InventoryRules.IsKg` actual scale.
- Check source resolving `(OwnerAirlineId, FlightId, ResourceId)`: Policy `ResourceId` vs Inventory **row id**. Check AirportSlot slices `FacilityId/Start/End` and lock owner Session implementation.
- Confirm SQL Server connection/EF transaction APIs and whether one `UnitOfWork.SaveChangesAsync` can run within one explicit transaction.
- Confirm whether `Allocation` class/table genuinely absent; no second source ledger exists.
- Check `SeatSelectionVerifier` commercial-only behavior, that no outgoing FlightFlow physical call exists; identify any existing code that accidentally queries OccupiedSeat.
- Check new Hold includes legacy `RequestedExpiresAt?`; list current valid existing rows with null expiry **or active local reservations without Allocation evidence**؛ identify stock sources requiring temporary block until reconciliation, without deleting production data.
- Check `SeatMapSelectionId` replays, price-only verification, `OrderCommercialVersion` needed, postticket coupon validation.
- Check current generic Hold does not copy money; no extra public DTO fields; current `StockPoolId` placeholder.
- Verify `Ordering` repo access; if unavailable, keep adapter field mapping as contract, not claim actual end-to-end.
- Check `Supplier` and current rejection; no code implementing fake adapter.
- Compare current Source with Source Traceability v10; pinpoint all file paths, possible Pin/Hash tests that will legitimately change.

## Mandatory questions resolved in report, NOT guessed in code
1. Stock SQL source lookup exactly how? Existing Query/Read models vs CommandDb direct.
2. Which `InventoryRules.IsKg` precision? Exact EF decimal needed.
3. Does `AirportSlot` source partition across contiguous windows as assumed? Evidence from Source.
4. Do actual accepted Order selections always yield an accepted total weight for `AcceptedWeightKg` policy? If not, specify gating per variant, never infer. **Answered:** no sellable product yields it; `AcceptedWeightKg` is Unsupported and no field is built (Owner Q5).
5. Any existing order mapping for `ServiceResolvedSelectionDto` to Hold? Report only.
6. Do administrative Capacity Adjust operations need same transaction locks? Provide file list.
7. Which `ExceptionFactory` codes can be reused for `shortage`, `wrong state`, `append conflict`? List; no arbitrary constants.
8. Is `ServiceController` protected by Service authentication in deployment? If not, security blocker, no new anonymous public API.

## Output, exactly one report
Create `reports/V10-GATE0-SOURCE-AUDIT.md` with:
- baseline / root sha / file inventory / relevant source extracts
- each required operation marked `EXISTS`, `NEEDS_CHANGE`, `GATED` (no fake pass)
- field-level DTO mapping / SQL table migration plan / lock plan
- behavior and breaking changes (especially null expiry, replays, partial release)
- acceptance test-to-file matrix `07`
- list **only real** conflicts with source or Master; no speculative implementation questions.

**Exit:** present report to Owner; do not execute Gate 1 until approved. This protects against another broad incorrect code generation.
