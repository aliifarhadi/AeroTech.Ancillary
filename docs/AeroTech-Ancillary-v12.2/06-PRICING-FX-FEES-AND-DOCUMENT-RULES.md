# 06 — Exact Pricing & document semantics for Phase1+2

## Current retained v12.1 aggregate (do not redo old broken Flat Pricing)
`AncillaryPricing` (AR): `Id:long`, `AncillaryProvisionId:long`, `Version:int`, `PricingUnit`, `Status`, audit timestamps. `AncillaryPricingRate` child: `Id:long`, `(CurrencyId:int,PassengerTypeCode?,AgeFromInclusive?,AgeToExclusive?)`, `BasePrice:Money`. `AncillaryPriceComponent` child: `Category:Tax|Fee`, `Code:string(10)`, `Name:string(100)?`, `CountryId:int?`, `StationAirportId:int?`, `Amount:Money`, `FeeApplicationUnit:enum?`, `TaxIncludedInSource:bool?`. Money: `Amount:decimal(19,6)>=0`, `CurrencyId:int` with currency reference decimal scale; one rate's base/components same currency. One active Pricing revision per Provision. Keep `AncillaryPricingLine` removed; *no root CurrencyId*.

## Required v12.2 corrections (the smallest diff)
1. **Explicit price origin:** `Paid+Filed` / `Paid+ExternalQuote` / `Free` / `NotAvailable` consistent with existing Outcome. Filed requires priced published Rates; Quote-based does not publish a fake zero-price record. `QuoteProviderKey:string(50)?` must resolve to authorized Supplier/owner quote capability, or activation `BLOCKED_REFERENCE`. A reference not configured may remain Draft but never be advertised as guaranteed sale.
2. **Charge basis per monetary component:** Fee `Item|Ticket|OneWay|RoundTrip|SectorOrPortion` (reuse existing enum). Rate's `UnitTotal` includes Base + taxes and Fee only if actually per Item and tax semantics reconciled; `UnappliedFees[]` shown separately and `IsUnitTotalComplete=false` when order-level basis cannot be evaluated. Never apply per-Ticket Fee once per traveller; no automatic % fee or FX without sourced rule.
3. **Tax included/excluded correctness:** `TaxIncludedInSource` currently informational. v12.2 must add **unambiguous `TaxTreatment=IncludedInBase|AddedToBase`** or (if backward compatible) validate `TaxIncludedInSource` to mean inclusion *only with documented data definition*. In rate total formula, `IncludedInBase` tax is informative and **must not** add to base again; `AddedToBase` tax adds. Require explicit TaxTreatment on newly authored Tax components, migrate existing rows as `Unknown` and block new publication until classified. No invented statutory tax rates; country/station references reused.
4. **Rounding/scale:** `Money.Amount` retains up to 6 dp stored; writing rate validates authoritative `DecimalPlaces` (0/2/3). Do not round silently; no hardcoded default 2, no double, no currency conversion from a different authorized rate. Report external `ReferenceData.Currencies` wrong decimals and fail closed; do not edit AirInfo outside this repository.
5. **Per-piece tiers:** `A01` uses Provision `FirstExcessPiece/LastExcessPiece` and separate Rate attached to matching rule; require consistent coverage, no ambiguous simultaneous winner. A02 package weight captured in definition, price per package. A04 combination policy prevents mistaken doubles. A10 quote-based by default; only authenticated Provider quote in P3 can give accepted Money.
6. **One POS/Provision:** `CurrencyId` on Rate represents authored offered currency, *not* POS; the Provision holds the POS. For an airline serving two POS with same rate, author two Provisions (UI may offer Copy-as-new-draft, not hidden shared pricing inheritance).
7. **Historical correctness:** Active price immutable; Revision copies every Rate+Component Money + selector with newly generated IDs, with identifiable lineage. Every movement in Definition/Provision/Pricing published status is auditable. Historic Orders/EMDs later use accepted snapshot not mutable catalog.

## Structured calculations and tests
- `GrossUnit = Base + SUM(Tax.Amount WHERE treatment=AddedToBase) + SUM(Fee.Amount WHERE feeUnit=Item)`.
- `IncludedInBase` Taxes reported and reconciled as included amount; included portions may not exceed Base and should not be added a second time; unknown treatment **cannot create a misleading Complete total**.
- Non-unit fees returned as `UnappliedFees`, with basis; in P3 quote/order-level total computes these once at appropriate scope.
- Quotes: `Authoring PriceOrigin=ExternalQuote` allows publication only with a credible provider authorizer; `Public Amount=null`, not zero. `Order service selection` later must use quote TTL, accepted currency/price and idempotent repricing.
- Real LH raw `prices.total=12000` EUR uses a provider representation whose scale is not proved from the JSON alone. Never use `12000` as authored major EUR units nor claim it is `120` unless upstream explicit minor-unit contract is checked.

| Test ID | Proven property |
|---|---|
| PR-01 | EUR 10.50 stored with `decimal(19,6)`, correct reference 2dp |
| PR-02 | KWD 10.125 valid only when source says 3dp; reject when source wrong and fail closed |
| PR-03 | JPY 100.5 rejected, 100 accepted with source 0dp |
| PR-04 | EUR and USD different authored rates, never auto-FX |
| PR-05 | Base 100 + Added Tax 9 + Included Tax 5 = Gross 109; report included 5 separately |
| PR-06 | Base 100 + Ticket Fee 7 = UnitTotal 100; fee unapplied, not 107 |
| PR-07 | 2 travellers/3 items with one Ticket Fee: do not multiply fee per Item |
| PR-08 | one active Pricing revision / provision, stale expectedVersion rejected |
| PR-09 | overlapping PTC/age bands same currency rejected; different currency allowed |
| PR-10 | component currency differs from rate rejected |
| PR-11 | Paid+Filed cannot publish without valid Active Pricing |
| PR-12 | Free/NotAvailable cannot publish with active paid Pricing |
| PR-13 | Paid+ExternalQuote requires verified quote authority, doesn't need fake Active Pricing |
| PR-14 | PriceOrigin/CommercialOutcome incompatible combination rejected |
| PR-15 | unknown tax inclusion treatment cannot publish as accurate UnitTotal |
| PR-16 | legacy migration round-trip rate/component and tax flag without reinterpretation |
| PR-17 | idempotent price activation switch with actual SQL uniqueness |
| PR-18 | 24 variants have valid price mode; no forced EMD assumptions for EXST / Upgrade / SSR |

## Documents / fulfilment (authoring only)
Retain `DocumentDefinition(Type,RFIC?,RFISC?)` and add the small `DocumentRouting(NoAncillaryDocument|Emd|TicketOrExchange)` metadata described in chapter 17, without claiming the existing `AncillaryDocumentType` has a Ticket value. Retain `BookingDefinition(Method, SSR?, SSIM?, ConfirmationRequirement)` from v12.1. For document-required purchasable service, airline authoring must state valid correct EMD-A/EMD-S/Ticket/NoDocument policy **only when supported**; never infer EMD from service group alone. `SSR WCHR` may be Free and no EMD, `EXST` may need Ticket, `Upgrade` may require Ticket exchange. Phase3 does the *actual issue*. P1+2 only validates metadata and refusal of contradictory combinations.
