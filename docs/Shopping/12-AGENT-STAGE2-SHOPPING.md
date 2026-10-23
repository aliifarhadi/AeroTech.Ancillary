# Agent Stage 2 — Shopping 3 Surfaces + Quantity Options (no DB write)

**Pack Authority (must be locally present):** `00-START-HERE.md`, `01-OVERVIEW-AND-BUSINESS-CONTRACT-FA.md`, `02-EXACT-API-DTO-AND-FIELD-CONTRACT.md`, `03-A01-A24-VARIANT-DICTIONARY.md`, `04-SIGNED-IDS-TYPED-SNAPSHOT-AND-SECURITY.md`, `05-ORDERING-HANDOFF-AND-TESTS.md`, `06-BENCHMARK-SOURCE-DECISIONS.md`, `07-RELEASE-READINESS-AND-UNRESOLVED.md`. **If any file is absent or `python3 VERIFY-PACK.py` fails, STOP before modifying code. Do not consult earlier Pack versions.**


**Prerequisite:** Owner closes Stage1. Goal: Front receives correct rows and signed choices before Typed Options exists.

## Tasks

1. Keep common current `IAncillaryShoppingEngine` and service; change only Surface mappers/controller contracts to `POST /{Surface}/v1/AncillaryOffers {offerId}` for `Backoffice`, `OtaPanel`, `Api`.
2. Surface JSON must use family arrays of `02`. Per item include only common fields + matching typed `details`; unknown fields omitted (not 30 nulls).
3. Generate rows from `CoveragePlanner` actual `Sector/Portion/Journey/Order` and traveller binding; no fake `PerFlight` no Traveller; no duplicate per Leg; `unit` determines ID cardinality.
4. If no selectable fields: provide `selectionId` d_. If **only integer Qty** selectable: provide `quantityOptions[{quantity,selectionId,totalAmount}]` directly, signed per actual Quantity; **NO Snapshot, GET or POST**. For Typed: `reference` r_ and actual family optionsEndpoint; no final ID yet.
5. Filter Quote/Approval required, incomplete totals, unknown-option required, FX converted, unapplied order fees, gated A09/A17/A19/Insurance. Apply active-rate and actual PTC/POS eligibility **before issuing**. No fake Insurance from screenshots.
6. For consistent DX use names and field types exactly 02/03. Internal currencyId from Catalog, not ISO 978. Browser may group visually like SingaporeAir but Backend rows are unit-resolved.

## Tests

- H01-H12, S01-S16; 3 real HTTP JSON response fixtures; no duplicate Unit IDs; Qty1/Qty2 different HMAC values; no DB Snapshot row; free zero Rate.
- SeatMap frozen hashes and tests.
- Report exact diff and performance; no extra tables, no cross-service reads in Resolve (does not exist yet).

**Report:** `reports/V8-STAGE2-SHOPPING.md`. No Commit/Push without Owner.
