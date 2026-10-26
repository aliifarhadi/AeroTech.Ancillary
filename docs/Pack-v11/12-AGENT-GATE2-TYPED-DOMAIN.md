# Coding Agent — Gate 2 — typed AncillaryReservationUnit and durable historical selections

**Precondition:** Gate1 approved. Authority `03`/`04`, supported Variant list from Gate0. Scope Domain + Persistence + Query GET only. Preserve v10 Root status/idempotency and InventoryLedger.

1. Evolve existing `AncillaryReservationUnit` with immutable `VariantCode` derived Definition and minimum **typed choice** of the supported families; choose bounded VO/Owned child representation consistent with source and `CLAUDE.md` relational-only rule. No 24 aggregates/entities or JSON blob. Mandatory actual supported families must reconstruct precisely, particularly A01/A05, A11 fixed Meal, A15 SSR, A20 appointment/guests, A02/A06/A10 fixed/A12/A24 fixed choices; A13/A14 stay GATED (Owner Q1).
2. Keep existing `ServiceDefinitionId` and `CoverageScope` internally, but **derive** from loaded Provision and Definition, not REST input. Ensure `Quantity`, `TravellerId`, `CoveredFlightIds` derive mapped Unit/Selected; never query Ordering.
3. Capture the fixed choices with the rule Price uses (`TypedSelectionForm.WithFixedChoices`) and validate the options against the static Definition/Provision rules (`TypedSelectionForm.Fields`) through one Application port implemented in Query, following `ISeatSelectionVerifier` (Owner Q2). Snapshot `ServiceDefinitionRef` and only the fixed facts of 04 §3 (Owner Q4).
4. Persist `Selected.Options` typed family data and the fixed facts from Domain into normalized child tables. No duplicate nullable columns for unrelated variants on Root. Lists go into relational child tables; `CoveredFlightIds` moves to `AncillaryReservationUnitFlights` (Owner Q5).
5. Implement GET Reservation typed projection *only from saved Reservation plus children*, never current Active Definition/Price or old SelectionSnapshot, even after expiration/retirement. Preserve Cancel/Released/Expired units' details.
6. Delete all reservation rows on dev first (Owner Q5). Plain migration: `VariantCode`, `ServiceDefinitionRef`, the typed selection tables and `AncillaryReservationUnitFlights` replacing the JSON column; no backfill. Check the generated SQL and prove Down/Up on a test database.

## Acceptance
D01–D23, R01–R02, W17–W18 and W25–W26; SQL exact roundtrip for all supported variants; change/retire source after Hold, GET identical; no new Ordering calls. Return report `reports/V11-GATE2-TYPED-DOMAIN.md` with ER diagram, typed class/field definitions, migration SQL, tests executed/discovered, discrepancy inventory. Do not commit/push.
