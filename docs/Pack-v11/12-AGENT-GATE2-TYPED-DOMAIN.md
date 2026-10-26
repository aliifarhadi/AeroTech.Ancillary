# Coding Agent — Gate 2 — typed AncillaryReservationUnit and durable historical selections

**Precondition:** Gate1 approved. Authority `03`/`04`, supported Variant list from Gate0. Scope Domain + Persistence + Query GET only. Preserve v10 Root status/idempotency and InventoryLedger.

1. Evolve existing `AncillaryReservationUnit` with immutable `VariantCode` derived Definition and minimum **typed choice** of the supported families; choose bounded VO/Owned child representation consistent with source and `CLAUDE.md` relational-only rule. No 24 aggregates/entities or JSON blob. Mandatory actual supported families must reconstruct precisely, particularly A01/A05, A11 fixed Meal, A15 SSR, A20 appointment/guests, A13 Pet if source valid.
2. Keep existing `ServiceDefinitionId` and `CoverageScope` internally, but **derive** from loaded Provision and Definition, not REST input. Ensure `Quantity`, `TravellerId`, `CoveredFlightIds` derive mapped Unit/Selected; never query Ordering.
3. Snapshot fixed fulfillment data only when semantically needed (MealCode, MenuItemRef, PlanCode, Facility/Service components, etc.) and uniquely sourced; no guessing missing business facts. If selected Menu/Package/Equipment variation cannot be distinguished by current Price/Resolve, **mark variant GATED**; do not add guessed Hold fields.
4. Persist `Selected.Options` typed Family data and needed fixed facts from Domain into normalized child tables. No duplicate nullable columns for unrelated variants on Root. Lists relational child tables. Audit existing primitive `CoveredFlightIds` SQL representation and migrate without loss if it breaches relational-only rule.
5. Implement GET Reservation typed projection *only from saved Reservation plus children*, never current Active Definition/Price or old SelectionSnapshot, even after expiration/retirement. Preserve Cancel/Released/Expired units' details.
6. Migration additive, preserve v10 rows and keys; legacy missing details reported as unknown, **never backfilled with fabricated current values**. Test migration on isolated DB and actual generated SQL; no deletion or DB reset.

## Acceptance
D01–D20, R01–R02 and W17–W18; SQL exact roundtrip for all active variants; change/retire source after Hold, GET identical; no new Ordering calls. Return report `reports/V11-GATE2-TYPED-DOMAIN.md` with ER diagram, typed class/field definitions, migration SQL, tests executed/discovered, discrepancy inventory. Do not commit/push.
