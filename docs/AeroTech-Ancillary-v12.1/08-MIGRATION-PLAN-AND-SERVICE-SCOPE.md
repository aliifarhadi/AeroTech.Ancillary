# Safe migration and API evolution — only AeroTech.Ancillary

## Baseline
Feature commit `1abf7a53e0efb9eb892097977327718c61486158` is the sole mandatory starting diff to audit. Existing Phase2 migration creates 8 stock tables; feature also includes `V121LegacySchemaCleanup` (command/query) that drops historical tables/columns. Current version's pricing `CurrencyId` is on `AncillaryPricing`, `Amount` on `AncillaryPricingLine`; its Line mixes Base/Tax/Fee. Preserve all current amounts, statuses and historical prices.

## Execution plan
**M0 Read-only inventory:** record SHA, SHA of unchanged Reservation and other frozen paths, complete DB schema and actual row counts, pricebook and Provision price status distribution, migration history, effective v12.1 source docs and reference currency decimal settings. Tests first. No other repository query or patch needed.

**M1 Expand:** add `AncillaryPricingRates`, `AncillaryPriceComponents`, `Money` owned columns and necessary indexes/FK in Command + ReadModel contexts; add Provision `PurchaseStage`, `Booking.ConfirmationRequirement`, `AdvancePurchase.MaximumPeriod?`, baggage typed descriptors; add row-version/indexes where missing. Existing tables remain physically unchanged at this checkpoint.

**M2 Backfill:** transform each old legacy Pricing group by `(PricingId, PTC, AgeFrom, AgeTo)` into one Rate at parent CurrencyId; copy components with base currency. Build deterministic lineage map old LineId -> new RateId/ComponentId, legacy PricingId unchanged. Treat source tax as additive under current semantics. If data violates uniqueness or Money scale, **report exception and halt activation/migration for affected Pricing**, don't drop currency/round prices silently. Existing Active Paid/Free/NotAvailable status and price must reconcile exactly.

**M3 Reconcile:** compare all original IDs, old/new selectors, line totals, taxes, fees, rates, currencies, statuses, PriceVersion, effective quote unit per Provision, existing ServiceDefinitionRef identities and unique indexes. Verify sample 31 seeded product definitions where present; no assumptions about actual production data. Test SQL on clean DB and clone/backup, including migration both directions **schema** while acknowledging data written after Up cannot be reconstructed automatically by Down.

**M4 Cutover:** switch Domain/DTO/Backoffice to new PricingRate and PriceComponent. Remove old mixed `AncillaryPricingLine` authoring path. Ensure GET detail returns nested rates with Money pairs; list paginated details clearly identify available authored currencies, while obsolete one-currency DTO is not quietly reused. Versioned Draft edits never mutate Active price.

**M5 Cleanup (D14):** audit already-pushed `V121LegacySchemaCleanup` column and table drop list *inside Ancillary only*. If backup/reconciliation shows no loss of current or historical Ancillary meaning, accept as separate explicit existing cleanup. After new Pricing migration and one complete production-like verification, remove its old columns/table only when lineage and restoration are provable. No cleaning unrelated tables, no SQL migration against any other service.

**M6 Verify:** all old Phase1/2 tests (modify only tests intentionally enforcing the replaced Pricing shape), new ~15 family scenario suite, Data/SQL concurrency, read-model parity, authorization, no new 2nd active Pricing, no accidental external adapter, full solution build and EF pending-model-change check. Report exact `dotnet build/test`, counts PASS/FAIL, migration SQL, database clone reconciliation and SHA. No self-certified closure if unexecuted.

## Compatibility rules
- Old flat Line DTO is deprecated/replaced in a documented breaking **Backoffice** contract. Agent must update only Ancillary-owned callers/tests. Other apps are not to be edited.
- No foreign API schema is invented. If no existing internal compatible API, store old read DTO as an explicit compatibility projection only if unambiguous (one currency) and report multi-currency conflict; do not silently return first arbitrary currency.
- Currency code resolves through ReferenceData currency reference inside this repo; published Money uses immutable authored CurrencyId. DecimalPlaces validated at publication; an unavailable reference blocks new publish rather than coercing.
- Concurrency: filtered unique Active Pricing per Provision continues; status switch atomic; new rates/components persist behind root; avoid child orphan and ambiguous selector uniqueness.

## Phase2 fixes without inventing new physical resource infrastructure
- Remove cross-domain `Unlimited` vs MustCheck refusal (D10) with migration-safe logic.
- Policy identity resolves via `OwnerAirlineId+ServiceDefinitionRef` even if ServiceDefinition revised; stale `ServiceDefinitionId` never becomes sole controlling identity.
- Reference NotConnected behavior retained (D11), clear NOT_VERIFIED in admin API. No fake provider data, no Local activation bypass.
- Do not auto seed stock from baggage weight/seat/SSR code. Existing 31 policies stay NotConfigured until explicitly authored, not forced Unlimited.
