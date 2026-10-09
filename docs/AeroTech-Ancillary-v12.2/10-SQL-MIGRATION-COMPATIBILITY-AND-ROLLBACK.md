# 10 — Migration strategy / backward compatibility and source-of-truth controls

**Rule: no default destructive rewrite of Phase1/2 rows.** Existing v12.1 D14 cleanup and migrations may have already removed legacy tables. New v12.2 migrations must be additive where possible, replay-safe, idempotent on clean database, and tested against actual existing schema. The old dev-data wipe was a historical, explicitly authorized action and **not** permission to wipe again.

## Migration M0 — Freeze and data snapshot
1. Verify branch HEAD, `git status`, migration history in `AncillaryDbContext` AND `AncillaryQueryDbContext`, current SQL test environment. Create a source/path/line inventory of 24-family fields/validated enums. Do not alter ReferenceData or other service schemas.
2. If data exists, record row counts and primary keys for Supplier, Definition versions, Provision rules, Pricing/Rates/Components, Policy and typed stock tables, all adjustment tables; prepare database backup/restore evidence before destructive operations. No data deletion without explicit Owner instruction.
3. Compare each newly proposed schema field with current `.cs`, EF Configuration, existing migrations. Reuse fields rather than creating duplicates. Produce `v12.2-reuse-and-delta.md` showing each new field and why it cannot be represented today.

## M1 — Expand: add profile/variant and owned specs
- Add typed `AncillaryProfile`, validated `ServiceVariantCode` and family-specific spec tables (up to 9, child typed lists as necessary). **Add nullable for existing rows**, do NOT assume old `XBAG_WEIGHT`/sample catalog has correct package dimension or ServiceDateBasis. New Definitions require exactly one matching spec at creation; old rows may stay `LegacyNeedsClassification`, explicitly blocked from v12.2 new activation until reconciled.
- Preserve `Id`, `StableRef`, Version, Supplier, old industrial service codes, Date/PriceUnit, old child IDs and history.
- Add index uniqueness `(DefinitionId, Profile)` or PK DefinitionId in each spec table; Root publish validation ensures one and only one profile-spec across tables. No one giant sparse table.
- Add ProfileRule typed subordinate only for Pet/AssistedTravel/Airport/Upgrade cases actually needed; don't create empty placeholder spec tables for shared validators.

## M2 — Expand: Provision semantics and selection schema
- Add `PriceOrigin` normalized with existing `CommercialOutcome`. Carefully backfill **only provable**: `Paid + active Pricing => Filed`; Free=>Free; NotAvailable=>NotAvailable. `Paid` with missing pricing => `RequiresClassification` not `ExternalQuote` guessed. New Draft require explicit enum; Active legacy invalid row becomes read-only, do not reactivate until reconciled.
- Enforce new single-POS on **new v12.2 publications**. Old multiple-POS or empty-POS rows must be reviewed and split into new Provisions with duplicate rule trees and correct references **only with migration evidence**; never split automatically and create price collisions. Alternative: preserve old read-only Active for backward compatibility, and publish corrected versions via guided migration before production activation.
- Add `PassengerUsageLimitScope.PerPortion=4` as the next numeric enum value and mapping; retain `PerOrder/PerFlightOccurrence/PerServiceDate` ordinal IDs. Add commercial consumption unit only if needed for weight-package mixed families, otherwise refuse incompatible mixed-use family; not a physical allocation table.
- Type-specific requirement metadata should be derived from Spec/Variant when possible, not need another table; if a configured checklist differs per product, type it and persist only such configuration.
- For A24 onboard-only: append `PurchaseStage.OnBoard = 5` to author the onboard-only A24 case; preserve `LegacyUnspecified=4` as unpublishable. Do not change old enum numeric values.

## M3 — Expand: Pricing tax treatment / external quote
- Keep Rate/Component schema from v12.1 and their IDs, `decimal(19,6)`.
- Add `TaxTreatment:AddedToBase|IncludedInBase|LegacyUnknown` (explicit values and validator). For rows with documented true `TaxIncludedInSource` migrate to Included; documented false to Added only if source semantics proved; else LegacyUnknown and block claiming TotalComplete on new activation. Retain raw old flag for auditing until contract cleanup with reconciliation.
- Quote mode is Provision metadata; it does not insert fake `0` Rate. External quote source reference must point to actual configured provider. If source not connected, v12.2 catalog can persist Draft but cannot assert an operational offer or guaranteed sale.
- Test snapshot and historic prices: money no mixed currencies or extra precision; no tax double addition, no Fee with wrong unit in UnitTotal.

## M4 — ReadModel/Sync and indexing
- Mirror only necessary normalized authoritative fields in AncillaryQueryDbContext; keep current synchronizer and projections, no dead references. Apply Command-side migrations then Query-side in required order, verify both EF snapshots have no pending changes.
- Filtered SQL unique indexes: active/current Policy stable key; one active Pricing revision per provision; one current count/weight physical key. Slot interval overlap requires facility-scoped SERIALIZABLE/UPDLOCK,HOLDLOCK (or equivalent) not unique index alone; test actual SQL Server concurrency.
- Tenant unique indexes include OwnerAirlineId; source confirmation/tax currency IDs remain domain references and are not silently hardcoded.

## M5 — Reconcile and compatibility audit
- Compare before/after row counts, IDs, historic Rule/PTC/POS/tax/fee/Price/StockAdjustment details; record migration exceptions. New profiles can be published only with valid typed data; *do not auto-classify by name or Description/SSR string alone*.
- Round-trip on two databases: clean current migration, and realistic **pre-v12.2 seeded v12.1** DB. All results for failed source reference documented as `BLOCKED_REFERENCE` not test success.
- Backoffice old DTO compatibility: provide explicit versioned validation failure/migration guidance for old flat callers rather than silently mapping unknown fields. For new v12.2, require typed schema; if breaking contract is necessary identify exact paths and JSON changes in the Agent report.
- Production AirInfo currency `DecimalPlaces` observed wrong (168/170 0 in prior audit) => **external data-blocking issue**; no update to AirInfo and no runtime hardcoded ISO override. Domain tests may use accurate test-only reference data.

## Rollback / disaster-recovery truth
- Each migration includes fully checked `Down` shape and honest warning if **data-loss occurs on Down**. Reverting a table populated with v12.2 typed specs cannot reconstruct old `AncillaryPricingLine` without backup; don't claim reversible from schema-only `Down`. Prefer forward repair with tested backup restoration. Never let Agent wipe local dev database without renewed specific authorization.

## Release checklist
`dotnet build` all Solution no new warnings in changed files; `dotnet test` Domain & SQL Acceptance actual counts; CommandDb + QueryDb no pending model changes; migrations clean and seeded v12.1; concurrency 100 stress tests; real authenticated Backoffice smoke for 9 templates + all 24 payload shape; SQL unchanged M1 Phase3 Reservation tables/hashes; zero touched external repo; itemized PASS/FAIL/DEFERRED and SHA of audited commit.
