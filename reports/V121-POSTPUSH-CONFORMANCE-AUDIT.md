# AeroTech Ancillary v12.1 — post-push conformance audit

Date: 2026-10-09. Repository `aliifarhadi/AeroTech.Ancillary` only. Branch `feat/ancillary-v12.1-phase2-stock`.

## 1. Baseline and verdict

- Audited HEAD: `889d5146a37751029baeced931545d7c0f2736fe` ("Fix"). `git rev-parse HEAD` and `origin/feat/ancillary-v12.1-phase2-stock` both returned this SHA and `git status --short` was empty before any change, so the working HEAD had not diverged.
- Corrections of this audit are **uncommitted** in the working tree (section 7). Nothing was committed, pushed or merged. No other repository was changed. Phase 3 was not started.

| Finding | Result |
|---|---|
| F01 currency reference data | **BLOCKED_WITH_EVIDENCE** — the data is wrong at its source (AirInfo); Ancillary fails closed; nothing was overridden |
| F02 real Backoffice HTTP contract | **PASS** — 63 of 63 authenticated requests as expected; one case not provable on dev data (three-decimal currency accepted), blocked by F01 |
| F03 `RequiresAvailabilityCheck` version selection | **CORRECTED** — current definition version only; test added |
| F04 stale `ServiceDefinitionId` on the policy | **CORRECTED** — current version id returned read-only beside the stored pointer; no cross-aggregate mutation; test added |
| F05 report truthfulness and migrations | **VERIFIED and AMENDED** — counts reproduced on a clean checkout of `889d514`; completion report amended; `Down` pinned as schema-only |

**Release verdict: NOT READY for multi-currency production** while F01 stands (only EUR, USD and zero-decimal amounts can be priced correctly). Domain, query and HTTP contract: ready for Owner review after the corrections below. No automatic merge.

`V121_POSTPUSH_AUDIT_READY_FOR_OWNER_REVIEW`

## 2. F01 — currency reference data

### What the code does

- [Money.cs:41-48](../src/AeroTech.Ancillary.Domain/AncillaryPricingAggregate/ValueObjects/Money.cs#L41-L48): an unknown currency throws 16511, an amount with more decimals than the reference allows throws 16512. There is no default scale.
- [ReferenceDataCurrencyReference.cs:13-16](../src/AeroTech.Ancillary.ServiceHost/ReferenceData/ReferenceDataCurrencyReference.cs#L13-L16): the host reads `DecimalPlaces` from `ReferenceData.Currencies` only.
- [CurrencySyncer.cs:24](../src/AeroTech.Ancillary.ReferenceData/Syncing/CurrencySyncer.cs#L24) and [:32](../src/AeroTech.Ancillary.ReferenceData/Syncing/CurrencySyncer.cs#L32): that table is a synchronized copy of AirInfo `v1/Financials/Currencies` ([AirInfoClient.cs:10](../src/AeroTech.Ancillary.ReferenceData/AirInfo/AirInfoClient.cs#L10)). It is not Ancillary-owned seed data.

### SQL evidence (read-only, dev `DotAirAncillary`)

```sql
SELECT DecimalPlaces, COUNT(*) FROM ReferenceData.Currencies GROUP BY DecimalPlaces;   -- 0 -> 168, 2 -> 2
SELECT Id, Code, DecimalPlaces, RoundingFactor FROM ReferenceData.Currencies WHERE Code IN (...) ORDER BY Id;
```

170 rows; the only non-zero rows are EUR = 2 and USD = 2.

### Source evidence

A read-only `GET https://aerotech-airinfo.dotair.stg.agidp.ir/service/v1/Financials/Currencies` (the same anonymous call the host's synchronizer makes) returned HTTP 200 with 170 currencies and `decimalPlaces` = 0 for 168 of them. The local table is therefore a faithful copy; the defect is in the AirInfo data, not in the Ancillary mapping.

### Comparison with ISO 4217

ISO 4217 list one (SIX, published 2026-09-17) was fetched during this audit for the thirteen codes below. No launch-currency list exists in the pack; these are the currencies of the earlier sample catalog (IRR, USD) and the usual regional ones.

| Id | Code | Reference `DecimalPlaces` (dev = AirInfo) | ISO 4217 minor units | |
|---|---|---|---|---|
| 3 | AED | 0 | 2 | mismatch |
| 17 | BHD | 0 | 3 | mismatch |
| 30 | CHF | 0 | 2 | mismatch |
| 47 | EUR | 2 | 2 | match |
| 50 | GBP | 0 | 2 | mismatch |
| 69 | IQD | 0 | 3 | mismatch |
| 70 | IRR | 0 (rounding factor 1000) | 2 | mismatch on paper; whole-rial pricing may be intended — a business decision |
| 75 | JPY | 0 | 0 | match |
| 82 | KWD | 0 | 3 | mismatch |
| 113 | OMR | 0 | 3 | mismatch |
| 121 | QAR | 0 | 2 | mismatch |
| 148 | TRY | 0 | 2 | mismatch |
| 155 | USD | 2 | 2 | match |

The other 157 currencies were not compared one by one.

### Behaviour with this data (HTTP, section 3)

`GBP 7.50` and `KWD 12.125` → 422 / 16512 "Amount 12.125 has more decimals than currency 82 allows (0)."; `KWD 12` → 200. The system refuses rather than rounds.

### Test added

`F01_the_amount_scale_follows_the_currency_reference_only_and_a_wrong_or_missing_reference_fails_closed` ([V121PostPushAuditAcceptanceTests.cs:70](../tests/AeroTech.Ancillary.Application.AcceptanceTests/Inventory/V121PostPushAuditAcceptanceTests.cs#L70)): a probe currency with no reference row → 16511; with 0 decimals → 16512 for 7.5; with 2 decimals → accepted; turning the reference back to 0 or removing it refuses publication of the stored draft; with 2 again it publishes and 7.500000 is stored.

### Why this is blocked, and what unblocks it

Correcting the numbers means changing AirInfo's data (another service) or overriding synchronized data locally; the first is out of scope and the second would be undone by the next synchronization and would hide the defect. Neither was done. Needed from the data owner: correct `decimalPlaces` in AirInfo for every currency that will be sold, and a decision for IRR. After the next synchronization no Ancillary change is required.

## 3. F02 — real Backoffice HTTP contract

### Setup

- Host: the `ServiceHost` of this working tree, built to a side folder and started by me on `http://localhost:5299` with `ASPNETCORE_ENVIRONMENT=Development` (real Kestrel, real JWT bearer validation against the staging identity authority, real routing, model binding, JSON options, validators and exception mapping). Stopped after the run; the port was verified free.
- Authentication: a Backoffice access token supplied by the Owner during the audit (surface `backoffice`, context `Airline`). It is not stored in the repository.
- Database: dev `DotAirAncillary`, which held 0 rows in `Ancillary` and `ReadModel` before the run. The run created 33 + 29 rows (one supplier, two definitions and a revision, four provisions, their prices, one policy). They were deleted afterwards; both schemas are back at 0 rows and `ReferenceData` is unchanged at 415. This removal of the audit's own rows is not a second wipe of pre-existing data.
- Script: 63 scripted requests with expected status, business code and response facts (kept outside the repository; not an automated test of the solution — see section 8). It was executed three times: the first two runs each showed one difference that was a wrong expectation of the script (the list field is `rateCount`; a `long` version is a JSON string), not a defect of the service. The service was not changed between the runs; the audit's own rows were removed before each rerun. The figures below are from the third run.

### Result

63 requests, 63 as expected: 41 × 200, 6 × 400, 1 × 401, 1 × 404, 4 × 409, 10 × 422.

| Area | Requests and outcome |
|---|---|
| Authentication | no token → 401 |
| New provision members | missing `purchaseStage` → 400; `LegacyUnspecified` → 422 / 16302; `PreOrder` and `advancePurchase.maximumPeriod` 720 round-trip in the detail; `booking.confirmationRequirement` round-trips in the definition detail |
| DefinePricing | two currencies (EUR, USD), ADT / CHD rates, tax, fee per Item, fee per Ticket → 200, `currencyIds` [47, 155] |
| GetById | `basePrice`, `components`, `unitTotal` as `{amount, currencyId, currency}`; ADT EUR: unit total 45.00, `isUnitTotalComplete` false, `unappliedFees` = TKT 7.00 Ticket; CHD USD: 21.50, complete |
| Paginated | row `currencies` "EUR, USD", `rateCount` 4; after a switch: version 1 Retired "EUR, JPY", version 2 Active "EUR, JPY, USD" |
| ChangePricing | draft replaced → 200 and read back (unit total 47.20, complete); on an Active price → 409 / 16503 |
| Activate / Publish | provision `Publish` with a price → both Active; pricing `Activate` → Active; provision `Activate` with its active price → Active |
| Suspend / Reactivate | provision and pricing suspended and reactivated → 200 each |
| Revise / Switch | revision is Draft version 2; switch with a stale expectation → 409 / 16510; correct switch → Active |
| Zero decimals | JPY 1201 → stored and returned as 1201; JPY 1201.5 → 422 / 16512 |
| Two decimals | EUR 12.34 + tax 1.11 → unit total 13.45; EUR 12.345 → 422 / 16512 |
| Three decimals | KWD 12.125 → **422 / 16512 on dev** because the reference says 0 (F01). The accepted case cannot be shown over HTTP without correct reference data: **BLOCKED_WITH_EVIDENCE**. It is proven at service + SQL level with a test reference of 3 decimals (`PR04_PR05_PR06_PR07_amounts_of_zero_two_and_three_decimal_currencies_are_stored_exactly_and_a_finer_scale_is_refused`) |
| Other refusals | unknown currency → 422 / 16511; component in another currency → 422 / 16513; flat base component → 422 / 16514; passenger selector or two rates of one currency on a PerItem service → 409 / 16508; negative amount and seven decimals → 422 / 16502 |
| Malformed payloads | no `rates`, rate without `basePrice`, the old flat payload (`currencyId` + `priceLines`), unknown component category, non-numeric amount → 400; unknown pricing id → 404 / 16501 |
| Policy reads (F03, F04) | see sections 4 and 5 |

### Response examples (captured from the run)

`POST Backoffice/v1/AncillaryPricings` → 200

```json
{"data": {"id": "1558141084066906112", "ancillaryProvisionId": "1558141080484970496", "version": 1, "pricingUnit": "PerPassenger", "currencyIds": [47, 155], "status": "Draft"}, "errors": null}
```

`GET Backoffice/v1/AncillaryPricings/1558141084066906112` → 200 (one of four rates shown, component list shortened)

```json
{"data": {"id": "1558141084066906112", "version": 1, "pricingUnit": {"value": 1, "name": "PerPassenger", "title": "Per Passenger"}, "status": {"value": 1, "name": "Draft", "title": "Draft"}, "currencies": ["EUR", "USD"],
  "rates": [{"id": "1558141084087877632", "passengerTypeCode": {"value": 1, "name": "ADT", "title": "ADT"}, "ageFromInclusive": null, "ageToExclusive": null,
    "basePrice": {"amount": 40.0, "currencyId": 47, "currency": "EUR"},
    "components": [
      {"id": "1558141084087877633", "category": {"value": 2, "name": "Tax", "title": "Tax"}, "code": "VAT", "amount": {"amount": 4.0, "currencyId": 47, "currency": "EUR"}, "feeApplicationUnit": null, "taxIncludedInSource": null},
      {"id": "1558141084087877635", "category": {"value": 3, "name": "Fee", "title": "Fee"}, "code": "TKT", "amount": {"amount": 7.0, "currencyId": 47, "currency": "EUR"}, "feeApplicationUnit": {"value": 5, "name": "Ticket", "title": "Ticket"}, "taxIncludedInSource": null}],
    "unitTotal": {"amount": 45.0, "currencyId": 47, "currency": "EUR"}, "isUnitTotalComplete": false,
    "unappliedFees": [{"id": "1558141084087877635", "code": "TKT", "amount": {"amount": 7.0, "currencyId": 47, "currency": "EUR"}, "feeApplicationUnit": {"value": 5, "name": "Ticket", "title": "Ticket"}}]}]}, "errors": null}
```

`POST …/AncillaryPricings` with KWD 12.125 → 422

```json
{"errors": [{"code": 16512, "title": "Amount 12.125 has more decimals than currency 82 allows (0).", "detail": null, "metadata": null}]}
```

`POST …/AncillaryProvisions/{id}/SwitchActivePricing` with a stale expectation → 409

```json
{"errors": [{"code": 16510, "title": "The active pricing of the provision is not the expected one.", "detail": null, "metadata": null}]}
```

`POST …/AncillaryPricings` without `rates` (also the old flat payload) → 400

```json
{"type": "https://tools.ietf.org/html/rfc9110#section-15.5.1", "title": "One or more validation errors occurred.", "status": 400, "errors": {"Rates": ["The Rates field is required."]}}
```

### Observations from the HTTP run

- Values of type `long` are JSON strings (`"id": "1558…"`, `"version": "2"` on a policy); the sample in the completion report showed a numeric id and has been corrected by its amendment.
- A 400 caused by an unconvertible JSON value returns the framework's default text, which contains a CLR type name (`…DefineAncillaryPricing.PriceComponentInput`). This comes from the shared presentation layer, existed before this work and was not changed.
- Business errors use `{errors:[{code,title}]}`; binding errors use RFC 9110 problem details. Two shapes for the client, also pre-existing.

## 4. F03 — `RequiresAvailabilityCheck` follows the current version

- At `889d514` the snapshot service OR-ed the Active provisions of **every** definition version of the service identity (`var versions = …` / `versions.Contains(provision.ServiceDefinitionId)`, lines 29–40 of the committed file). Retiring a definition does not retire its provisions, so a retired version could keep the flag true for ever.
- No document of the pack requires a cross-version OR. Doc 04 selects candidates from the active definition and its active provisions; a provision of a retired definition is not a candidate.
- Correction — [GetInventoryConfigurationSnapshotService.cs:32-47](../src/AeroTech.Ancillary.Query/AncillaryInventoryPolicyAggregate/Queries/GetInventoryConfigurationSnapshot/GetInventoryConfigurationSnapshotService.cs#L32-L47): the service asks the one existing version-selection rule, `IInventoryCommercialFactsReader.FindCurrentAsync` ([InventoryCommercialFactsReader.cs:40-53](../src/AeroTech.Ancillary.Persistence/AncillaryInventoryPolicyAggregate/InventoryCommercialFactsReader.cs#L40-L53): Active version first, otherwise the highest non-retired version — the same rule policy activation uses), and looks only at the Active provisions of that version. `IsGuaranteed` stays false.
- Test `F03_the_availability_check_flag_follows_the_current_definition_version_and_never_a_retired_one` ([:114](../tests/AeroTech.Ancillary.Application.AcceptanceTests/Inventory/V121PostPushAuditAcceptanceTests.cs#L114)): version 1 with an Active must-check provision → true; while version 2 is only a draft → still version 1, true; version 1 retired (its provision verified still Active with the flag in SQL), version 2 active with a provision without the flag → **false**; a must-check provision on version 2 → true.
- HTTP: `GET …/AncillaryInventoryConfigurationSnapshots?ServiceDefinitionRef=SMOKE_PAX` after the revision →

```json
{"data": {"policyId": "1558141095223754752", "serviceDefinitionRef": "SMOKE_PAX", "currentServiceDefinitionId": "1558141095781597184", "authority": {"value": 1, "name": "Unlimited", "title": "Unlimited"}, "state": {"value": 2, "name": "Unlimited", "title": "Unlimited"}, "isGuaranteed": false, "requiresAvailabilityCheck": false, "reasonCode": null}, "errors": null}
```

## 5. F04 — stale `ServiceDefinitionId` on an Active policy

- Confirmed at `889d514`: the stored pointer changes only in `AncillaryInventoryPolicy.Activate` ([AncillaryInventoryPolicy.cs:101](../src/AeroTech.Ancillary.Domain/AncillaryInventoryPolicyAggregate/AncillaryInventoryPolicy.cs#L101), [:126](../src/AeroTech.Ancillary.Domain/AncillaryInventoryPolicyAggregate/AncillaryInventoryPolicy.cs#L126)); after a new definition version is published the detail kept showing the previous version id with nothing beside it.
- Chosen solution (the first option of the finding): the reads return a clearly named `CurrentServiceDefinitionId`, resolved at read time by the same rule, beside the stored `ServiceDefinitionId`. The aggregate is not mutated when a definition is revised; no event, no linking engine. The stored id remains what it is: the version that was validated at the last define / change / activation.
  - [BackofficeInventoryPolicyDto.cs:10](../src/AeroTech.Ancillary.Query/AncillaryInventoryPolicyAggregate/Dto/BackofficeInventoryPolicyDto.cs#L10), [InventoryConfigurationSnapshotDto.cs:9](../src/AeroTech.Ancillary.Query/AncillaryInventoryPolicyAggregate/Dto/InventoryConfigurationSnapshotDto.cs#L9)
  - [GetInventoryPolicyByIdService.cs:34-36](../src/AeroTech.Ancillary.Query/AncillaryInventoryPolicyAggregate/Queries/GetInventoryPolicyById/GetInventoryPolicyByIdService.cs#L34-L36), [GetInventoryPolicyByServiceIdentityService.cs:40-42](../src/AeroTech.Ancillary.Query/AncillaryInventoryPolicyAggregate/Queries/GetInventoryPolicyByServiceIdentity/GetInventoryPolicyByServiceIdentityService.cs#L40-L42), [InventoryPolicyMapper.cs:13-19](../src/AeroTech.Ancillary.Query/AncillaryInventoryPolicyAggregate/Queries/GetInventoryPolicyById/InventoryPolicyMapper.cs#L13-L19)
- Test `F04_an_active_policy_reports_the_current_definition_version_beside_its_stored_pointer_and_keeps_its_physical_binding` ([:163](../tests/AeroTech.Ancillary.Application.AcceptanceTests/Inventory/V121PostPushAuditAcceptanceTests.cs#L163)): Local FlightCount policy with one activated source of 2; new version published and old version retired while the policy stays Active → detail by id and by service identity: stored id = version 1, current id = version 2, policy version unchanged (2); snapshot: current id = version 2, configured count 2, same flight and resource; SQL: the policy row and the source row are byte-for-byte what they were (ids, status, versions, total). When every version is retired the current id is null in both reads and re-activation of the policy is refused (16617).
- HTTP: `GET …/AncillaryInventoryPolicies/1558141095223754752` → `"serviceDefinitionId": "1558141078840803328", "currentServiceDefinitionId": "1558141095781597184", "status": {"name": "Active"}, "version": "2"`; the by-identity read returns the same pair.
- Left for the Owner: the stored field keeps its name `serviceDefinitionId`. Renaming it (for example `validatedServiceDefinitionId`) would make the distinction self-explanatory but breaks the Backoffice contract; not done without a decision.

## 6. F05 — truthfulness of the completion report, migrations, data

### Counts reproduced

| Where | Build | Domain | Acceptance (SQL Server Express) |
|---|---|---|---|
| Clean detached checkout of `889d514` (temporary worktree, removed afterwards) | 0 errors, 13 warnings | 232 / 232 | 246 / 246 |
| Working tree with the corrections of this audit (`dotnet build AeroTech.Ancillary.sln --no-incremental`) | 0 errors, 13 warnings | 232 discovered / 232 passed / 0 failed / 0 skipped | 250 discovered / 250 passed / 0 failed / 0 skipped |

No "aborted" line in any run. `ef migrations has-pending-model-changes`: no changes for `AncillaryDbContext` and `AncillaryQueryDbContext`. The claims 232 / 232 and 246 / 246 of the completion report are true for the pushed commit.

Concurrency tests in the acceptance suite (all on SQL Server, all passed): `PR21_…`, `V12_P02_…`, `X09_…`, `X10_X11_…`, `X12_…`, `P2_X01_…` (count and slot), `P2_W03_…` (weight), `P2_A04_concurrent_…`, `P2_X03_…`, `P2_X10_…`. Every acceptance run creates its own database and applies all migrations of the three contexts to it; the migration classes use one further database each, seeded at the v11 level.

### Migrations reviewed

- `20261009124006_V121FinalPricingRates` ([Up, lines 12–124](../src/AeroTech.Ancillary.Persistence/Migrations/20261009124006_V121FinalPricingRates.cs#L12-L124)): creates the rate and component tables, copies existing lines, then drops `AncillaryPricingLines` and the root `CurrencyId` / `FeeApplicationUnit`. [Down, from line 127](../src/AeroTech.Ancillary.Persistence/Migrations/20261009124006_V121FinalPricingRates.cs#L127): drops both new tables, re-adds `CurrencyId` as NOT NULL default 0 and a nullable `FeeApplicationUnit`, recreates an empty lines table. **Down is schema-only and destroys every price**; it also leaves a default constraint on `CurrencyId` that the original column did not have. The query-side migration has the same shape.
- `20261009131917_V121FinalProvisionDescriptors`: adds five columns (existing provisions get `PurchaseStage` 4, existing definitions `BookingConfirmationRequirement` 1); Down drops them.
- The completion report did not claim a reversible data migration for these two, but no test covered their `Down` at all. Added: `F05_the_down_of_the_final_migrations_restores_the_schema_only_and_never_brings_the_prices_back` ([V121LegacyChainMigrationAcceptanceTests.cs:218](../tests/AeroTech.Ancillary.Application.AcceptanceTests/Migration/V121LegacyChainMigrationAcceptanceTests.cs#L218)): 6 pricings / 6 rates / 3 components after Up; after Down to `V121LegacySchemaCleanup` the pricings remain, the lines tables are empty, no root currency is set and the new objects are gone; after Up again there are 0 rates and 0 components. The existing tests named "…reversible…" concern the earlier rule-group and cleanup migrations and their schema shape only.

### Deleted development data — provenance

- Ordered by the Owner in the previous session ("all database data is test data … delete it"). Executed once, on 2026-10-09 at about 18:04, on dev `DotAirAncillary` only. Not repeated by this audit.
- Backup taken immediately before: `C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS\MSSQL\Backup\DotAirAncillary_before_final_wipe.bak` — copy-only, finished 2026-10-09 18:03:40, 18.3 MB, first LSN 42000000019100001 (`msdb.dbo.backupset`). During this audit it was restored into a scratch database, counted and the scratch database dropped.
- Before (from the restored backup, migration level `V121LegacySchemaCleanup`): `Ancillary` 53 tables / 759 rows, `ReadModel` 40 tables / 538 rows, `ReferenceData` 12 tables / 415 rows.

| Table | Ancillary | ReadModel |
|---|---|---|
| Suppliers | 4 | 4 |
| AncillaryServiceDefinitions | 31 | 31 |
| AncillaryProvisions | 99 | 99 |
| AncillaryPricings | 69 | 69 |
| AncillaryPricingLines | 97 | 97 |
| Provision rule rows (all non-empty rule and rule-child tables) | 450 | 238 |
| ProvisionRuleMigrationAudit | 9 | — |

  The rule rows in detail — Ancillary: PassengerEligibilityRules 37, PassengerTypes 43, SalesRestrictionsRules 7, PointsOfSale 6, CustomerTypes 1, GeographyRules 76, OriginAirports 29, DestinationAirports 3, RoutePairs 63, FlightApplicationRules 13, MarketingAirlines 3, OperatingAirlines 8, FlightNumbers 7, Aircraft 11, FareApplicationRules 16, FareFamilies 12, CabinClasses 19, Rbds 3, TravelDateRules 6, PermittedTravelPeriods 6, DayTimeApplicationRules 1, DayTimeWindows 3, AdvancePurchaseRules 26, BaggageApplicationRules 16, SeatApplicationRules 14, SeatCharacteristics 19, SeatNumbers 2. ReadModel holds the same child counts (rule headers are columns of the provision row there).
- After the delete and the four migrations: `Ancillary` 54 tables / 0 rows, `ReadModel` 41 tables / 0 rows, `ReferenceData` 415 rows. Same figures at the end of this audit.

### Completion report amended

`reports/V121-PHASE1-PHASE2-FINAL-COMPLETION-REPORT.md` now opens with an amendment block: pushed SHA, non-reversible `Down`, exact data figures and backup reference, the HTTP evidence, and the two superseded decisions; the affected body lines carry a pointer to it.

## 7. Delta of this audit (uncommitted)

Changed:

- `src/AeroTech.Ancillary.Query/AncillaryInventoryPolicyAggregate/Dto/BackofficeInventoryPolicyDto.cs`
- `src/AeroTech.Ancillary.Query/AncillaryInventoryPolicyAggregate/Dto/InventoryConfigurationSnapshotDto.cs`
- `src/AeroTech.Ancillary.Query/AncillaryInventoryPolicyAggregate/Queries/GetInventoryConfigurationSnapshot/GetInventoryConfigurationSnapshotService.cs`
- `src/AeroTech.Ancillary.Query/AncillaryInventoryPolicyAggregate/Queries/GetInventoryPolicyById/GetInventoryPolicyByIdService.cs`
- `src/AeroTech.Ancillary.Query/AncillaryInventoryPolicyAggregate/Queries/GetInventoryPolicyById/InventoryPolicyMapper.cs`
- `src/AeroTech.Ancillary.Query/AncillaryInventoryPolicyAggregate/Queries/GetInventoryPolicyByServiceIdentity/GetInventoryPolicyByServiceIdentityService.cs`
- `tests/AeroTech.Ancillary.Application.AcceptanceTests/Fixtures/InventoryScope.cs` (wiring of the three query services)
- `tests/AeroTech.Ancillary.Application.AcceptanceTests/Inventory/P2InventoryPolicyAcceptanceTests.cs` (snapshot shape pin gains `CurrentServiceDefinitionId`)
- `tests/AeroTech.Ancillary.Application.AcceptanceTests/Migration/V121LegacyChainMigrationAcceptanceTests.cs` (F05 test)
- `reports/V121-PHASE1-PHASE2-FINAL-COMPLETION-REPORT.md` (amendment)

Added:

- `tests/AeroTech.Ancillary.Application.AcceptanceTests/Inventory/V121PostPushAuditAcceptanceTests.cs` (F01, F03, F04)
- `reports/V121-POSTPUSH-CONFORMANCE-AUDIT.md`

No migration, no domain or application change, no change under `AncillaryReservationAggregate` (the frozen-hash tests `V12_C04_…` and `X20_…` pass), no schema touched outside the audit's own scratch databases.

## 8. Commands

```text
git rev-parse HEAD; git status --short; git rev-parse origin/feat/ancillary-v12.1-phase2-stock
sqlcmd … ReferenceData.Currencies (queries of section 2); msdb.dbo.backupset; RESTORE … DotAirAncillary_AuditRestore; DROP DATABASE DotAirAncillary_AuditRestore
curl https://aerotech-airinfo.dotair.stg.agidp.ir/service/v1/Financials/Currencies            (read-only)
git worktree add --detach <temp> 889d514; dotnet build; dotnet test (both projects); git worktree remove
dotnet build src/AeroTech.Ancillary.ServiceHost -o <side folder>; dotnet AeroTech.Ancillary.ServiceHost.dll (port 5299); 63 HTTP requests; host stopped
dotnet build AeroTech.Ancillary.sln -v q --nologo --no-incremental                             0 errors, 13 warnings
dotnet test tests/AeroTech.Ancillary.Domain.ConformanceTests --no-build                         232 passed, 0 failed, 0 skipped
dotnet test tests/AeroTech.Ancillary.Application.AcceptanceTests --no-build                     250 passed, 0 failed, 0 skipped
dotnet … ef.dll migrations has-pending-model-changes (both contexts)                            no changes
```

Not done, stated plainly:

- The HTTP run is a recorded scripted run, not a test inside the solution. An in-solution HTTP test needs a hosted test setup (`WebApplicationFactory`, a test authentication scheme, substitutes for RabbitMQ and Redis); neither this repository nor the Ordering reference has that pattern, so it was not introduced without a decision.
- 403 for a non-airline caller could not be exercised with the supplied token (unchanged from the Phase 2 smoke).
- The test reference fixture uses id 53 for GBP; the platform id is 50. Fixture-only, left as is.

## 9. Needs the Owner

1. F01: have AirInfo's `decimalPlaces` corrected (and decide IRR). Until then keep multi-currency pricing out of production.
2. Review and commit decision for the corrections of section 7.
3. Whether to rename the stored `serviceDefinitionId` in the policy read contract.
4. Whether an in-solution HTTP test setup should be added.
