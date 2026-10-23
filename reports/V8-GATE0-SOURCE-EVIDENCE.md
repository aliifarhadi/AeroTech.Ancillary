# Pack v8 — Gate0 source evidence (read only)

| Item | Value |
|---|---|
| Baseline | `k8s-stg` = `origin/k8s-stg` = `49e9209a40e046fe71c7913cc1fcf8a8fca6799d` (same as the pack baseline) |
| Pack | `docs/Shopping` v8, `sha256sum -c SHA256SUMS.txt` all OK, `VERIFY-PACK.py` PASS (15 documents, 78 scenarios) |
| Code changed | none; no migration; no branch created |
| Proposed branch | `feat/ancillary-offers-v8` from `k8s-stg@49e9209`, after your approval |
| Verdict | **GO after the answers to Q1–Q4** in `reports/V8-QUESTIONS-AND-CONFLICTS.md` |

## 1. Frozen SeatMap set

48 files: `Query/Shopping/SeatMaps/**`, `Shopping/SeatMaps/**`, `Shopping/Engine/AncillaryShoppingEngine.SeatMaps.cs`, `RestApi/V1/SeatMaps/**`.
Combined SHA-256 (sorted paths, CR removed): `2cab2be6a14d0095ca41fe27880f0eb2c476f125aeb09b10005c8dd381e125fd`.
One dependency on code to be deleted: `ResolveSeatMapSelectionsService.cs:2,46-47` uses `EvaluateBatchService.MaximumLines` (Q3).

## 2. Pack concept → current source

| Pack concept | Current source | Gap |
|---|---|---|
| 4 selection units | `Shopping/Engine/CoveragePlanner.cs:20-35`: Sector×Traveller (ref `{BoundId}:F{n}`), Portion×Traveller (ref `BoundId`), Journey×Traveller (`JOURNEY`), Order (`ORDER`) | none; non-traveller Sector/Portion/Journey not produced and not to be built |
| Enum mapping | `ServiceCoverageScope` Sector/Portion/Journey/Order → `PerTravellerPerFlight` / `PerTravellerPerPortion` / `PerTraveller` / `PerOrder` | DTO mapping only |
| Rate per PTC | `PriceAssessor.cs:85` takes rates whose PTC is null or equals the traveller's | none; `PriceAssessment.PricingRateId` already on the candidate |
| Active rate chain | Rate is a child of `AncillaryPricing` (status Draft/Active/Suspended/Retired); readers `FindActivePricingsAsync(pricingIds)` only | new read by rate id needed |
| Free = real zero rate | `PriceAssessor.cs:49` Free has no `PricingRateId`; Domain accepts a zero base (`AncillaryPricingRate`); **`AncillaryProvision.Constraints.cs:12-15` forbids Paid for A11/A15** | Q2 |
| Quantity | `QuantityAssessment(MinPerSelection, MaxPerSelection, cumulative limits)` | enumerate min..max inside Shopping |
| Typed form choices | `OfferOptionCatalog.Build(definition, provision, candidate, context)`; A18 infant/guardian and A19 child need the offer context | Q4 |
| Signed ids | `PricedSelectionTokens.cs` (JWS HS256, `Shopping:SelectionSigningKey`, fail closed) in Query | reuse with `purpose` per your ruling; move to Shopping (Q1) |
| Source expiry | `FlightOfferShoppingContextMapper.cs:62` `ValidUntilUtc` = clock + `Shopping:SourceContextTtl` (30 min) | present; `exp = min(ValidUntilUtc, now + 15 min)` |
| Sales binding facts | `AncillarySalesScope` (owner, channel, POS set, office, agency, customer, country); candidate POS per engine run | none |
| Currency | `ReferenceData.Currencies`: EUR 47, IRR 70, USD 155 | matches the pack |

## 3. Gated items (evidence)

| Item | Evidence | Status |
|---|---|---|
| A09 | no FlightFlow occupancy contract for extra seats | GATED, `extraSeats: []` |
| A17 | no encryption-at-rest mechanism in `src` or `Framework` (search for DataProtection / encrypt found nothing) | GATED (D2 fields may be added, endpoint closed) |
| A18 | infant/guardian choices come from the offer context (`OfferOptionCatalog.cs:51-52`) | open with Q4; gated under Q4 b |
| A19 | `CustomerSelectionContract.cs:107-108` text fields vs `AncillarySelection.cs:61-63` booleans | GATED |
| Insurance | no variant in `AncillaryVariant` | `travelInsurance: []` |

## 4. What is deleted (your ruling)

`Query/Shopping/AncillaryOffers/Queries/{GetAncillaryOfferDetail, EvaluateSelection, EvaluateBatch, ResolveSelections}` with their DTOs, routes (`Details`, `Selections/Evaluate`, `Selections/EvaluateBatch` on three surfaces, `Service/v1/Ancillaries/Selections/Resolve`) and tests; `Common/PricedSelections` after the codec moves. Only other dependent besides the SeatMap line of Q3: `Query/DependencyInjection.cs`. `OfferOptionCatalog` moves to Shopping (it becomes the GET Options catalog).

## 5. Smallest diff per stage (after the answers)

1. Stage1: JWS codec with purposes `r`/`d` in Shopping; issuance gate; rate-by-id reader; Free per Q2; tests T01–T19 on the codec.
2. Stage2: three surface list contracts + family DTOs, `quantityOptions`, deletion of the old routes; S01–S16, H01–H13.
3. Stage3: snapshot entity + table + migration, GET/POST Options per Q1/Q4; O01–O12.
4. Stage4: ResolveGrouped; R01–R14.
5. Stage5: 78-case closure, live HTTP on three surfaces + Service.

## 6. Baseline build and tests

Command: `dotnet build AeroTech.Ancillary.sln --no-incremental`, then `dotnet test` per project with `--list-tests` counts.

| Step | Result |
|---|---|
| Clean build | 0 errors, 63 warnings (pre-existing) |
| Domain.ConformanceTests | 264 discovered, 264 passed |
| Shopping.Tests | 368 discovered, 368 passed |
| Application.AcceptanceTests | 232 discovered, 232 passed |

SeatMap regression command: the same three suites (SeatMap tests live in Shopping.Tests and Application.AcceptanceTests) plus the combined hash of section 1.
