# 06 - Upgrade vs reset; command/read databases; migration safety

## Recommendation
Evolve from `AeroTech.Ancillary@6b0a80f` on `k8s-stg`. Do not reset to `843cf7c`, rewrite existing Supplier/ServiceDefinition/Backoffice or delete existing Hold/Get/Confirm. Current code includes large tested authoring stack. The domain refactor changes Provision columns materially but should not force whole-project recreation.

## Source audit work before writing migration
- Inspect actual SQL Server schema, command migrations `20261007202914_V11Phase1CommercialAuthoring` and query migration `20261007202918_V11Phase1CommercialAuthoringQuery`, snapshots and possibly installed records. Print counts of ServiceDefinitions, Provisions by state/disposition, old PriceLines by category/ currency and date/list contents. Check dependent FK, read projections and exposed public DTO consumers.
- Compare exact path diff from baseline `e2a8c9f` and from initial `843cf7c`. Record immutable reservation paths and hashes. Keep all write work in this repository ONLY.
- Validate actual EF infrastructure convention: PK type, value generator, `HasMany` vs `OwnsMany`, query synchronizer, transaction/UoW. AirPrice's table-nested OwnsMany is benchmark, not a blind copy.

## Stage A: additive schema
- Add mandatory PricingUnit to ServiceDefinition via safely nullable or carefully default-free migration; do **not** assign PerItem as a universal default. New typed Provision tables from doc 03 with explicit PK/FK; add Pricing root and PricingLine tables from doc 04; add transactional uniqueness filtered indexes for ACTIVE price per Provision and ACTIVE Sequence per Definition.
- Keep legacy primitive/list columns, `Fee` and `ProvisionPriceLines` present initially. New read-model migrations must be explicitly aligned to command schema; use independently versioned query context/migration as in source.
- Default new code reads v12 normalized representations only after backfill and reconciliation; avoid duplicate sources of truth. For rollout needing coexistence, temporary dual-read or compatible DTO is acceptable **only with tests and planned deletion**; no permanent conflicting duplicate fields.

## Stage B: deterministic data mapping and reconciliation
1. `ServiceDefinition.PricingUnit`: require an Owner-approved per-ServiceDefinitionId mapping; Auto-mapping solely from GroupCode/ServiceSubCode is forbidden because identity is not sufficient to infer PerPassenger vs PerRoom etc. Report unmapped rows and DO NOT force activate them.
2. Single-value old `TravelFrom/TravelTo` => a Seasonality window when both present; if one bound missing, distinguish unbounded side via explicit approved rule or leave for mapping review. `DaysOfWeek + TimeFrom/To` => DayTime rows when semantically equivalent. Old primitive selectors each become unique row. v11 `SeatNumbers`, `SeatCharacteristicCodes` become typed rows. `ProvisionRoutePair` existing row IDs preserved where practical.
3. For each historical paid Provision, create one initial Pricing version with matching old currency/application unit and lines, preserving exact amounts, Category, Code, Name, CountryId and StationAirportId. Copy old row IDs to an old/new mapping log; no mixed selector may be invented from PTC eligibility. When v11 had separate ADT/CHD provisions, keep their individual prices and identities unless Owner approves semantic consolidation.
4. For historical Free/NotAvailable Provisions, do not invent a Paid Pricing version. For every active historical Paid Provision, import one active price atomically or leave unchanged behind compatibility gate; DO NOT temporarily publish missing price.
5. Compare before/after row counts per service/provision, total amounts BY original price line, statuses and date selectors, and command vs query payload equality. Produce auditable CSV/Markdown mapping report with ids and ambiguity. If >1 old base line per Provision or bad status/currency data cannot map deterministically, `REPORT_GAP_AND_STOP` before dropping old columns.

## Stage C: cutover and cleanup
- Switch application writes to normalized child entities and Pricing root, query synchronizer and DTO readers to new structure, prove all old and new history can be retrieved. Validate Active uniqueness and amount invariants, then only in a **separate reviewed migration** drop unused primitive columns/old PriceLine table or mark decommissioned. Never drop operational Reservation columns or clean historic Hold records as a side effect.
- If already in use by other services, do not silently break REST response contracts. Provide consumer impact audit and Owner-approved additive compatibility strategy; no unrequested Ordering/AirAvail edit.
- Maintain DB rollback policy appropriate to deployed data: rollback schema may be technically reversible without being content-lossless; generate backup/export and explicit reversible mapping before irreversible migration.

## Physical constraints
- PK on every typed child Id and Pricing.Id; FK child->Provision / Pricing->Provision / PricingLine->Pricing; no cascade from ServiceDefinition to published Provision/Pricing history.
- Indexed `ProvisionTravelDate(AncillaryProvisionId,TravelDate)`, `ProvisionSeasonalPeriod(ProvisionId,StartDate,EndDate)`, `ProvisionBlackoutPeriod`, DayTime unique tuple, per-parent selector duplicate prevention. SQL Server filtered unique `IX_Pricing_OneActivePerProvision` on `AncillaryProvisionId` with predicate `Status = Active`; use actual enum value.
- `IX_Provision_ServiceDefinition_Sequence_Active` retained as currently enforced. `IX_Pricing_Provision_Version` unique; Status index as needed.
- PTC and age range overlap detection is aggregate-level and transaction-safe at price publish; SQL uniqueness alone cannot prevent interval overlap. Concurrency token/guard on price swap + filtered index ensure at-most-one ACTIVE.
- `Money decimal(18,2)` as existing Ancillary convention; preserve source precision if different. `CurrencyId` on Pricing root. `DateOnly`, `TimeOnly`, UTC offset/mapped DateTimeOffset using EF provider mappings. No CSV, JSON column, dynamic DSL or recurring future extra tables.

## Tests required before deployment
Migrate a clone of existing v11 DB; verify no losses, no price double count, all active statuses preserved; assert command/read migrations no pending changes; verify fresh empty DB works; test rollback/backups; assert HOLD/CONFIRM migration tables/schemas unchanged; run full solution build/domain/acceptance tests and mapping reconciliation. Don't declare PASS based on prior agent report.
