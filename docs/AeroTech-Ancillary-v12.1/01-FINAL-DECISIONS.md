# Final decisions — CLOSED by domain architect using user-approved boundaries

| ID | Final decision | Implementation consequence |
|---|---|---|
| D01 | Keep `AncillaryPricing` aggregate root; create `AncillaryPricingRate` and child `AncillaryPriceComponent`; retire mixed old line after migration | One clear rate containing Base Money and components |
| D02 | Money = `(Amount,CurrencyId)` on Rate base and each component | No authoritative root CurrencyId; no ambiguous amount |
| D03 | Multiple authored currencies inside one Active Pricing revision | No FX; a requested unsupported selling currency is unavailable, NOT converted |
| D04 | **Separate Provision for each POS** | POS is a typed Provision sales selector; **NO POS selector on PricingRate** |
| D05 | Base means net amount, tax and fee components additive; gross derived | TaxIncluded only optional source/provenance flag; inclusive sourced price without net breakdown cannot masquerade as additive base |
| D06 | FeeApplicationUnit only on the applicable Fee component | No universal root fee scope; Tax components have none |
| D07 | decimal(19,6) storage; validate currency scale using `CurrencyReadModel.DecimalPlaces` | JPY 0, EUR 2, KWD 3, no silent rounding or FX |
| D08 | **Fixed baggage weight packs as separate service SKUs/definitions**, e.g. 5kg/10kg/20kg as repository already has; `QuantityRule` remains integer min/max, no StepQuantity DSL. Add `MaximumPeriod?` to existing AdvancePurchase rule only to model bounded presale windows | A 10kg pack quantity 2 means 20kg entitlement. For arbitrary kilo sales use PerKilogram SKU; the concept comes from the fare/route, not a stock unit. No generic tariff matrix |
| D09 | Baggage is a **commercial carriage entitlement**, not automatically finite flight stock | Local Count/Weight sources apply only when a factual airline-owned quota has been authored and validated |
| D10 | `MustCheckAvailability` is NOT proof of a local numerical quota. **Remove ban on Unlimited+MustCheckAvailability** | Policy may have no local quota while commercial/operational confirmation is required; no automatic available/guaranteed claim; an actual delegated authoritative stock source still uses Supplier/FlightFlow |
| D11 | **KEEP CURRENT strict evidence gating** per Owner | `NotConnected...` reference ports remain fail-closed; Local Flight/Facility sources cannot become Active without source proof. No fake local registry/attestation bypass, no other-service integration |
| D12 | DailyCount, RoomNight and AssignedAsset remain schema/semantics **documented, unimplemented and explicitly unsupported for activation** | 3 proven local ARs only; supplier-managed accommodation and transfers supported at policy level |
| D13 | No invented discount, agency commission, percent of fare, mileage, merchant markup, FX or dynamic formulas | Existing net Base + fixed Tax/Fee is complete for this release; list unsupported formulas honestly |
| D14 | Clean obsolete Ancillary-owned schema only after audit; retain/restore historical semantics | Review already pushed `V121LegacySchemaCleanup` and new Pricing migration; no other-service changes |

## Extra minimal domain commitments needed for observed commercial variants
1. `BookingDefinition.ConfirmationRequirement: Immediate | SubjectToConfirmation` — only describes whether provider/operations may have to confirm request. Existing `BookingMethod/SSR/SSIM` remain authoritative. A listed SSR is not guaranteed stock (Iberia SpecialNeeds).
2. `AncillaryProvision.PurchaseStage: PreOrder | PostTicketed | Both` — condition for sell-stage selection; does NOT process tickets or orders, and maps to documented pre-/post-sale restrictions. Default migration of legacy rows to `Both` only if their recorded service policy can support both; otherwise keep Draft/quarantine for explicit catalog classification rather than silently widening. For existing published records without stage evidence, prefer `LegacyUnspecified` (not saleable by stage-specific future consumers) with migration lineage.
3. `ProvisionAdvancePurchaseRule.MaximumPeriod?` measured with same approved unit as `MinimumPeriod`; if defined require Maximum >= Minimum; limit values without a proven temporal interpretation are non-publishable. Absolute SalesRestrictons dates do not replace relative booking lead-time.
4. `ProvisionBaggageApplicationRule.ChargeKind` typed minimal value (`ExtraPiece`, `WeightPackage`, `Overweight`, `Oversize`, `SpecialEquipment`) plus optional `AllowanceConcept` (`Piece`, `Weight`) only when documented for this product. Do not infer universal baggage concept from SKU name or route. Existing `FreePieces/FirstExcessPiece/LastExcessPiece/Weight/...` retained. Pet service remains its own product family; no phantom universal baggage rate table.

**These four additions are minimal typed authoring descriptors** grounded in public airline offers/SSR/booking windows. They do not authorize new ARs, runtime matcher, reservation state, supplier call or EMD.

## Deliberately rejected
- New generic `StockPool`; arbitrary rule DSL; clone pricing per POS plus POS-in-rate; three competing tax representations; one class per ancillary family; tax/fee percentage formulas without requirement; treating `ServiceDefinition.ServiceSubCode` as stock key; granting limited stock sellability before Phase 3.
