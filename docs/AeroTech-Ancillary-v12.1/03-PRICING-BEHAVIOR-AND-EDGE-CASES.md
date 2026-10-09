# Pricing specification and truth tables — complete Phase 1 repair

## Purpose, lifecycle and currency
`AncillaryPricing` is a versioned authored **pricebook for ONE matching Provision**. `AncillaryPricingRate` is an alternative charge per passenger selector and selling currency, not an additional charge. `AncillaryPriceComponent` is additive Tax/Fee *within* the selected rate. A Provision has at most one Active Pricing revision. The requested selling currency must match an authored Rate; **no FX conversion**. Separate POS commercial differences => separate Provision (D04).

`Money`: amount decimal(19,6), currency int positive and exists in existing `ReferenceData.Currencies`; authored scale <= `CurrencyReadModel.DecimalPlaces`, negative prohibited, no silent rounding. Rate Base strictly >0 for Paid; fee/tax components >=0. The monetary components are all tied to the SAME currency and SAME selector but **their FeeApplicationUnit may differ**. Therefore `UnitTotal = Base + additive Taxes + Fees whose application really is per accepted item/unit`; a Fee per Ticket/OneWay/RoundTrip/Sector is **not** multiplied into this per-unit Total and remains a separately filed component awaiting travel coverage at future quote. When any non-unit Fee exists, do not publish an unconditional final Total for all quantities: return `TotalForOneUnit?` only for homogeneous unit-scoped amounts, or include `UnappliedFees` separately, clearly annotated. Checked decimal arithmetic, no overflow; currency display scale from reference. Do not sum arbitrary fee application scopes as if all were per item.

```text
Provision #21: POS=DE, Outcome=Paid, PricingUnit=PerPassenger
Pricing v3 ACTIVE:
  Rate A: EUR / ADT / age unrestricted: Base EUR 40.00; Tax EUR 4.00 [Code XT]; Fee EUR 1.00 [Code SVC, FeeApplicationUnit=Item] => EUR 45.00
  Rate B: EUR / CHD / age unrestricted: Base EUR 22.00; Tax EUR 2.00 => EUR 24.00
  Rate C: USD / ADT / age unrestricted: Base USD 49.00; Tax USD 1.50 => USD 50.50
  Rate D: USD / CHD / age unrestricted: Base USD 25.00 => USD 25.00
Provision #22: POS=TR, own active pricebook and conditions (not Rate.PointOfSaleId).
```

The rate key is `(CurrencyId, PassengerTypeCode?, AgeFromInclusive?, AgeToExclusive?)`. For PricingUnit != PerPassenger, PTC/Age are null and **exactly one base rate PER currency**; this is an important correction of legacy “exactly one base” overall. For PerPassenger, within one currency either all selectors use PTC or none; within each PTC age bands disjoint, do not mix unbounded generic with bounded, prevent ambiguous generic fallback. Alternatives never sum; multiple currency rates never sum.

## Source-inclusive tax and fees
D05: Only **net** base and separately additive tax/fee components are authoritative. Every newly published Fee component MUST specify its application unit; a legacy Fee whose old root FeeApplicationUnit was null is **quarantined for explicit assignment** rather than assumed per item. `TaxIncludedInSource` if retained means explanatory source provenance only; it does **not** change any total and is not a second 'tax-inclusive gross' pricing mode. A gross-inclusive figure with no verified net/tax decomposition must NOT be saved as net plus extra tax. Reject that input or first normalize with a documented upstream tax breakdown; do not invent tax percentages or extract VAT on faith. Preserve codes/country/station and fee unit where known.

## Case vectors (all executable from domain + SQL)
| ID | Input | Expected |
|---|---|---|
| PR01 | EUR 40 base + EUR 4 tax + EUR 1 fee | EUR 45.00, one rate total |
| PR02 | EUR and USD ADT for same Provision and active pricebook | two authored Rate alternatives, no exchange/FX |
| PR03 | request GBP when only EUR/USD authored | `NoMatchingCurrency` (not fallback EUR, not FX) |
| PR04 | base JPY 1201.5, reference DecimalPlaces=0 | reject |
| PR05 | base JPY 1201 | valid |
| PR06 | base KWD 12.125, reference DecimalPlaces=3 | valid |
| PR07 | base KWD 12.1251 | reject |
| PR08 | USD component linked to EUR Base | reject currency mismatch |
| PR09 | two identical currency+PTC+age keys | reject duplicate |
| PR10 | EUR ADT generic + ADT age 60..∞ | reject ambiguous band |
| PR11 | EUR ADT 0..65 + EUR ADT 65..∞ | valid disjoint boundary |
| PR12 | NonPassenger PerPiece EUR and USD two rates | valid; precisely one per currency |
| PR13 | NonPassenger PerPiece CHD selector | reject |
| PR14 | Tax Code XT duplicated at same country/station in same Rate | reject unless distinct typed identity is actually sourced |
| PR15 | Fee charged per Ticket and other per Item within SAME Rate | valid; ticket fee stays Unapplied separately; per-unit total MUST exclude ticket fee |
| PR16 | Tax with FeeApplicationUnit or new Fee with missing FeeApplicationUnit | reject |
| PR17 | Paid zero base | reject; Free uses CommercialOutcome Free with no active Price |
| PR18 | Paid Provision active without active price | reject activation |
| PR19 | Free/NotAvailable Provision + active price | reject activation |
| PR20 | active price revision edit | reject; create Draft successor and atomic switch |
| PR21 | two concurrent price activations | one winner SQL filtered unique active + conflict |
| PR22 | Price read DTO returns Rate(Base Money,Components Money, optional UnitTotal Money and UnappliedFees) | no misleading total for mixed application units |
| PR23 | historical v12 legacy flat lines ADT/CHD with taxes and one currency | identical totals after migration per selector |
| PR24 | new pricing client sends old `Category=Ancillary` flat payload | explicit compatibility/migration rule, no silent misinterpretation |
| PR25 | fee absolute amount vs percent of fare | percentage unsupported explicitly; do not invent formula |

## Migration algorithm (no data loss)
For each old `AncillaryPricing` parent and every legacy selector `(PTC,AgeFrom,AgeTo)`:
1. Read parent `CurrencyId` as **legacy source currency**. There must be one Base Ancillary line; create ONE Rate with Base Money(old.Amount, old.CurrencyId).
2. Move old Tax and Fee lines with same selector into `AncillaryPriceComponent` in Rate, with Money(old.Amount, old.CurrencyId), codes/country/station/name preserved, old root `FeeApplicationUnit` assigned to **Fee** components only. Tax fee unit null. If Fee exists but root unit missing, quarantine and require an explicit evidenced migration decision per row, without silently treating it as PerItem.
3. Preserve original parent PriceVersion, Status, timestamps, ProvisionId, PricingUnit; preserve line-id -> (new Rate/Component-id) mapping in migration audit. Never mutate old active priced meaning.
4. Before switching reads: reconcile old and new rate-key set, Base, Tax, Fee and source component sums, currency, count by status and historical version. Only compare derived UnitTotal if old and new components genuinely have the same unit application. Quarantine anomalous/incomplete records and STOP switch for them; never convert an unmatched tax into 'valid' alternative.
5. Expand/add tables and dual-read only if needed for transitional proof; cut read/write to new model after tests, remove legacy tables/columns only after backup and verification of **Ancillary's own** data. Down schema does not restore post-up new writes magically; document recovery backup, not false reversibility.
6. Price switch and synchronization should be in one transaction where existing framework permits; if command and read model are sequential, report honest consistency lag/replay recovery, do not claim atomic cross-db commit.

## Economic boundaries
Authoring stores filed, explicit amounts. **No tax engine, conversion engine, currency exchange rate, supplier commission policy, quantity billing evaluator or agency markup** is implemented. Future OTA offer will select a single matching rate, multiply quantity according to definition unit, and form a priced snapshot, but that runtime is outside these two phases. Historical active prices never edited in place.
