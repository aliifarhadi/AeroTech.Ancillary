# Gate 1 — Coding Agent — HTTP Safety + Contract Conformance

## Preconditions
فقط پس از قبول گزارش Gate0 و **تأیید صریح Owner برای کدنویسی**. هیچ commit/push/merge بدون دستور جداگانه. Branch/sha فعلی را گزارش بده؛ اگر Pack با HEAD conflict داشت توقف و سؤال با پیشنهاد مشخص.

## Authority
`00` + `02-EXACT-API-CONTRACTS.md` + `07-ACCEPTANCE-AND-ROUNDTRIP-MATRIX.md` + `09-SOURCE-TRACEABILITY-AND-REFACTOR-MAP.md`. `CLAUDE.md` wins. Scope فقط API safety و conformance proven gaps؛ نه rewrite v11.

## Required P0
کد `src/AeroTech.Ancillary.RestApi/V1/AncillaryReservationAggregate/Controllers/ServiceController.cs` در DELETE با `unitIds is { Length: > 0 } ? unitIds : null` خطر `explicit empty → release all` دارد. **با کمترین تغییر** Presence و محتوای query را تفکیک کن: absent `unitIds` = Release All؛ present nonempty positive distinct = Release selected؛ present blank/empty/malformed = 400 و ZERO DB changes. رفتار existing `ReleaseAncillaryHoldValidator` و `AncillaryReservation.Release` را حفظ کن؛ Domain را بی‌دلیل تغییر نده. راهکار جدید برای Query Binder/Controller با pattern موجود پروژه و تست HTTP واقعی مستند شود.

## Contract guards
- هیچ `services[]`, `serviceDefinitionId`, `coverageScope`, `inventoryFacts`, `stockPoolId`, `offerId`, `selectionId` در Generic Hold.
- Strict typed family arrays؛ `pets` GATED، `seats` فقط seat endpoint.
- `meals/upgrades` selected بدون foreign options؛ `assistedTravel` فقط SSR کد برای A15/A16.
- Receipt fieldها محدود `holdId,reference,status,expiresAt,units`؛ GET typed from stored data؛ idempotency unaffected.
- Cancel `unitIds:[]` reject، cancel reason replay unchanged.

## Mandatory tests
HTTP route/controller integration و unit validation: L01–L06, L11, W04–W08, positive partial Release and deliberate All. Save DB before/after برای explicit empty، foreign، malformed و duplicate. Authorization/Identity تغییر نکن. تمامی test suites متاثر را run و counts را در گزارش ثبت کن.

## Output
`reports/V12-GATE1-HTTP-SAFETY.md`: exact diff, test IDs, before/after SQL evidence, errors, no-migration statement, unrelated files untouched, remaining gaps. **STOP** for Owner review before next Gate; no commit/push/merge.
