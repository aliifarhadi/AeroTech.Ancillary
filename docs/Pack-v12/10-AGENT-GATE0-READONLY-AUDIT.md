# Gate 0 — Coding Agent — READ ONLY Source/Contract Audit

## Mission
با سورس **واقعی فعلی** مخزن `aliifarhadi/AeroTech.Ancillary` روی branch `feat/ancillary-offers-v9` قرارداد Pack v12 را ممیزی کن. **هیچ کد، Schema، Report قدیمی، Lock/DB یا فایل تستی را تغییر نده**؛ فقط گزارش جدید `reports/V12-GATE0-SOURCE-AND-CONFLICTS.md` با اجازه Owner بنویس. اگر نوشتن گزارش هم نیازمند دستور باشد، متنش را برای Owner ارائه بده.

## Authority
`00-START-HERE.md` و `01..09` همین Pack تنها مرجع قرارداد؛ `CLAUDE.md` بالادستی. HEAD benchmark `8c2fb087d43c04a06cf9eb18dc1bdd5734fff51d` ولی HEAD جدید را حتماً ثبت کن. Docs v10/v11 فقط historical evidence. No Ordering outbound, no FlightFlow seat physical, no Price/Resolve recall, no consumer ACL.

## Steps
1. `git rev-parse HEAD`, `git status --short`, `git log -3 --oneline`; commit delta from `8c2fb...` در صورت تغییر.
2. بازخوانی سورس Contract: `HoldAncillaryServicesRequest`, `IHoldAncillaryServicesCommand`, `AncillaryHoldFamilies`, typed DTOs, `HoldSelectionVerifier`, `CustomerSelectionContract`, `AncillaryVariant`, `ServiceController` و `SeatMaps/Holds`; DTO field-by-field vs 02 and variant-by-variant vs 03؛ هیچ فیلد invented.
3. Domain: Root/Unit/Args/Owned types/mapper/persistence؛ تحقق SQL reload historical selection، شش family details، Fixed Choices/Quantity؛ مرز Seat commercial. در صورت mismatch بنویس `FILE:LINE`, expected, actual, impact.
4. Inventory: SQL ledger lock, existing Allocator source rules, Retire active usage protection, transaction guarantee, concurrency test evidence; بدون اعتماد کور به Closure v11.
5. **Critical P0**: دقیقاً ثابت کن Controller `unitIds` absent، `?unitIds=`, `?unitIds=0`, empty array-bound، duplicated IDs را چگونه به Domain می‌فرستد. TRACE: HTTP binder → controller ternary → validator → Root.Release(null). اگر bug هنوز هست P0 RED.
6. Dependency reachability: از Generic Hold, Seat Hold, GET, Confirm, Release, Cancel, Expire هیچ call/ACL به Ordering/FlightFlow physical وجود ندارد. XML/JSON blob عمومی در persistence هم نیست.
7. Report inventory of existing tests (actual names/count) and what is still GATED. Execution of read-only tests allowed only if محیط ایمن و بدون DB mutation قطعی است؛ هرگز ادعای run بدون اجرای واقعی نکن.

## Deliverable
جدول `FindingId, File:Line, Source Evidence, Severity, Proposed Minimal Fix, Migration Impact, TestId`; status برای W/D/I/L، P0 Release explicit-empty، GATED table A01..A24، actual HEAD. در انتها **Stop and request Owner approval** قبل از Gate 1. هیچ commit/push/merge.
