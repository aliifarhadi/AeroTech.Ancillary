# AirOffer — Ancillary Implementation Spec

**Date:** 2026-10-03
**For:** the AirOffer service (repository `AeroTech.AirAvail`, solution `AeroTech.AirOffer.sln`). It states exactly what AirOffer builds so that ancillaries can be listed and priced for an existing order. It becomes binding for AirOffer when the Owner places it in that repository.
**Checked against:** `AeroTech.AirAvail` `ac84040` (head of `k8s-stg`, read on 2026-10-03); Ancillary `9bbd385`.
**Paths** in this document are relative to Ancillary's `docs/` folder; in the AirAvail repository the same files sit under `docs/ancillary/` (see `handover/README.md`).

---

## 1. The decision this spec implements

AirOffer does not call Ancillary. It reads Ancillary's **published read model** by SQL, keeps it as a cached snapshot, and works out the applicable ancillaries and their prices itself — the way it already reads FlightFlow's and AirPricing's read models and evaluates fares and point-of-sale rules.

Consequences that are part of the decision:

1. The evaluation rules of Ancillary Master §7 are implemented in AirOffer. Ancillary keeps its own implementation; the Quote rows of the Phase-1 and Phase-2 documents are the common test. For the same data and context both must agree.
2. A snapshot can be a little old. What AirOffer returns is an offer. Ordering has Ancillary check each accepted item again when it reserves it; AirOffer is not the last line of defence.
3. AirOffer never writes to Ancillary's database and reads only the tables and columns of Master §15.

## 2. What exists today (`ac84040`)

| Fact | Where |
|---|---|
| Other services' data is read with Dapper scripts against their `ReadModel` schemas (`FlightCapacities`, `AirFares`, `PointOfSales` …); connection strings `FlightFlowQueryDbContext`, `AirPricingQueryDbContext`, `CoreServiceQueryDbContext`, `BasicInfoQueryDbContext`. | `Persistence/Scripts`, `Persistence/Repositories`, `ServiceHost/appsettings.json` |
| A whole rule set is loaded as a snapshot and evaluated in AirOffer: point-of-sale rules. | `Scripts/GetPointOfSaleSnapshot.sql`, `Services/PointOfSaleEligibilityService.cs` |
| A snapshot cache refreshed by a cheap stamp probe (`MAX(LastUpdateTime)`, row count) with a probe interval and a maximum age. | `Scripts/GetReferenceDataStamp.sql`, `Services/Shared/ReferenceDataCache.cs`, `Options/ReferenceDataCacheOptions.cs` |
| Service-to-service controller, route `Service/v{version}`, one operation `POST FlightOffers/Details`, no authorization attribute. | `RestApi/V1/AirOffers/Controllers/Service2Service/ServiceController.cs` |
| `BaseResult<T>` responses; a `BusinessException` is written as HTTP 400 with its code. Codes in use: 6001–6027. | `Framework`, `Services/Exceptions/ExceptionFactory.cs` |
| The flight offer id is a plain-text encoding, not stored. | `Services/Offers/JourneyOfferIdCodec.cs` |
| No ancillary operation exists; only commented-out placeholders. | channel controllers |
| Test project with xUnit, NSubstitute, FluentAssertions. | `AirOffer/test/AeroTech.AirOffer.Services.Tests` |

## 3. Scope

| Now | Later, each with its own document |
|---|---|
| Reading and caching the ancillary catalogue | Listing and selecting ancillaries while shopping, inside the flight offer (Phase 3) |
| Evaluating Extra Baggage and Lounge Access for an order's context | Stock figures (Phase 4), seats (Phase 5), further conditions (Phase 6) |
| `POST Service/v1/AncillaryOffers` — the service list | Channel-surface operations |
| `POST Service/v1/AncillaryOffers/Details` — pricing the selected items | |
| Offer and offer-item ids | |

Nothing existing changes: no existing endpoint, DTO, codec or script.

## 4. Reading the catalogue

| Item | Value |
|---|---|
| Connection string | `ConnectionStrings:AncillaryQueryDbContext` — Ancillary's query database. Required; the host does not start without it. |
| Scripts | `Persistence/Scripts/GetAncillaryCatalogSnapshot.sql` and `GetAncillaryCatalogStamp.sql` |
| Snapshot script | three result sets: (1) every row of `ReadModel.AncillaryProducts` with `Status = 2`; (2) every row of `ReadModel.AncillaryPriceRules` with `Status = 2`; (3) every row of `ReadModel.PriceLines` whose rule is in (2). Only the columns of Master §15. |
| Stamp script | `SELECT MAX(LastUpdateTime) AS MaxLastUpdate, COUNT_BIG(*) AS [Count]` over `AncillaryProducts` and `AncillaryPriceRules` together (all rows, not only Active) — the shape of `GetReferenceDataStamp.sql`. |
| Repository | `IAncillaryCatalogRepository` / `AncillaryCatalogRepository` in Persistence, written like `AirPricingRepository.GetPointOfSaleSnapshotAsync` and the reference-data stamp query. |
| Cache | `AncillaryCatalogCache` in `Services/Services/Shared`, written like `ReferenceDataCache`: a singleton holding the current snapshot in memory; warm on start-up; every `ProbeIntervalSeconds` compare the stamp and reload when it differs; reload unconditionally after `MaxAgeMinutes`. Local memory only. |
| Options | section `AncillaryCatalogCache`: `ProbeIntervalSeconds` (default 30), `MaxAgeMinutes` (default 60), `WarmOnStartup` (default true) — an options class with defaults, as `ReferenceDataCacheOptions`. |

Reading rules (Master §15): enumerated columns are the numbers of Master §9; list columns are JSON arrays of numbers, `NULL` meaning "no restriction"; a rule's lines are ordered by `PriceLines.Id`.

If the snapshot cannot be loaded at all (no successful load yet), the two operations fail with `6034`. After a first successful load, a failed reload keeps the previous snapshot and is logged.

## 5. Evaluating

`AncillaryOfferEvaluator` in Services implements Ancillary Master §7.2–§7.6 over the snapshot, for what Ancillary's Phases 1 and 2 define:

- occurrences: `TravellerBound` (traveller × bound, covering the traveller's flights of that bound) and `TravellerSegment` (traveller × each of his flights);
- applicability: marketing airline = product owner on every covered flight; Extra Baggage only when exactly one flight is covered; Lounge only when the flight's origin airport is in the product's lounge airports; a price rule is selected; quantity left after `existing`;
- rule selection: currency, sales window against `asOf`, travel window against the local departure date of the first covered flight, passenger type, origin airport of the first covered flight, destination airport of the last covered flight; lowest priority wins;
- the item of Master §7.4, with the line amounts and totals; item order of §7.4;
- occurrence identity and `existing` matching as Master §7.2.

Values that AirOffer does not know (an enum number that is not in Master §9 for Phases 1–2) make that product invisible; they never cause a failure.

## 6. Operations

Both on the existing `Service2Service/ServiceController`, no authorization attribute, `BaseResult<T>`.

### 6.1 `POST Service/v1/AncillaryOffers` — the service list

Request: the order's context — the members of Ancillary's quote request (Master §7.1) except `selections`: `currencyId`, `asOf`, `salesContext`, `travellers[]` (with `flightRefs`), `bounds[]` with `flights[]`, `existing[]`.

Response `data`: the service list of `reference/Ancillary-Edge-Contract.md` §2.

- `offerId`, `offerItemId`: as Edge Contract §3, with `issuedAt` = AirOffer's clock at the call. `offerExpirationDateTime` = `issuedAt` + `AncillaryOffer:ValidityMinutes`.
- One `offerItems` entry per applicable occurrence, in the order of Master §7.4. `quantityRule` = the item's `minQuantity` / `maxQuantity`. `unitPrice`: `baseAmount` = the `Ancillary` line's unit amount; `taxSummary.taxes` = the `Tax` lines (`taxCode` = line code, `descText` = line name, `amount` = unit amount); `totalAmount` = `unitTotal`.
- `eligibility`: `paxRefIds` = `[travellerRef]`; `paxJourneyRefId` = `boundRef`; `paxSegmentRefIds` = `coveredFlightRefs`.
- `serviceDefinitionList`: one entry per distinct product version among the items, in order of first use; `serviceDefinitionId` = `SD1`, `SD2`, …; its members from the product (Edge Contract §2). `bookingInstructions` is null.
- `ownerAirlineId` of the offer = the airline when all items have the same one, else null.
- No applicable occurrence → `offerItems` and `serviceDefinitionList` empty; still HTTP 200.

### 6.2 `POST Service/v1/AncillaryOffers/Details` — pricing the selected items

Request: the same context plus the selection of Edge Contract §4 without `requestKey`:

```json
{
  "currencyId": 978, "asOf": "…", "salesContext": { }, "travellers": [ ], "bounds": [ ], "existing": [ ],
  "acceptSelectedOffer": {
    "offerRefId": "A1~978~1790000000",
    "selectedOfferItems": [ { "offerItemRefId": "A1~978~1790000000~XBAG1~1~501~7001~B~8001", "quantity": 1 } ]
  }
}
```

Response `data`: `{ "offerId", "offerExpirationDateTime", "currencyId", "items": [ … ] }` — per selected item, in selection order, the item of Master §7.4 (every member, same names) plus `offerItemId`. `quantity`, line `amount`s and `total` are for the selected quantity.

Checks, per selected item in order; the first failure refuses the whole request:

| # | Check | Error |
|---:|---|---|
| 1 | The id decodes (Edge Contract §3), begins with `offerRefId`, and its currency equals the request's `currencyId`. | `6029` |
| 2 | `issuedAt` + validity has not passed. | `6030` |
| 3 | The traveller and the journey or segment of the id exist in the request's context. | `6031` |
| 4 | No earlier selected item names the same occurrence (Master §7.2 identity). | `6033` |
| 5 | Evaluating that occurrence now gives an item with the id's product reference, product version and price rule. (A product that is gone, another version, another rule, or an occurrence that is no longer applicable all fail here.) | `6031` |
| 6 | `quantity` is within the item's `minQuantity … maxQuantity`. | `6032` |

A request whose context is inconsistent (Master §7.1: repeated refs, unknown passenger type, a `flightRefs` entry that is not a flight of the request, a ref that is not letters and digits) is `6028`, in both operations.

### 6.3 Errors

AirOffer's convention applies: a `BusinessException`, HTTP 400, body `BaseResult` with the code.

| Code | Name | Ancillary's own code for the same condition |
|---|---|---|
| 6028 | `AncillaryRequestIsInconsistent` | `16301` |
| 6029 | `AncillaryOfferItemIsInvalid` | — (`16303` when the scope letter does not fit the product) |
| 6030 | `AncillaryOfferHasExpired` | — |
| 6031 | `AncillaryOfferItemIsNoLongerAvailable` | `16302`, `16304`, `16305`, `16309` |
| 6032 | `AncillaryQuantityIsNotAllowed` | `16306` |
| 6033 | `AncillaryOfferItemIsDuplicated` | `16307` |
| 6034 | `AncillaryCatalogIsUnavailable` | — |

`6034` is answered with HTTP 503 by the controller (not by the middleware, which would write 400).

### 6.4 Configuration

| Key | Meaning | Value |
|---|---|---|
| `ConnectionStrings:AncillaryQueryDbContext` | Ancillary's query database | per environment (Owner) |
| `AncillaryCatalogCache:ProbeIntervalSeconds` | stamp probe interval | 30 |
| `AncillaryCatalogCache:MaxAgeMinutes` | unconditional reload | 60 |
| `AncillaryCatalogCache:WarmOnStartup` | load at start | true |
| `AncillaryOffer:ValidityMinutes` | offer validity | 30 |

## 7. Not part of this work

- No HTTP call to Ancillary. No write to any Ancillary table. No table other than the three of Master §15.
- No change to `JourneyOfferIdCodec`, to `FlightOffers/Details`, to any channel controller or existing DTO. The commented-out placeholders stay.
- No currency conversion, no point-of-sale rule, no bundling with flights, no stock, no seat.
- No new NuGet package.

## 8. Expected behaviour

Each row is at least one test in `AeroTech.AirOffer.Services.Tests`, named with its id. The evaluator and the codec are tested without a database, on a snapshot built in the test.

### Conformance with Ancillary

| # | Behaviour |
|---|---|
| X01 | Every Quote row of `phases/P1-Extra-Baggage/phase.md` §4 (Q01–Q29) that concerns catalogue mode gives, in AirOffer, the same set of occurrences in the same order with the same quantities, lines and totals. |
| X02 | Every Quote row of `phases/P2-Lounge-Access/phase.md` §4 (Q01–Q16) that concerns catalogue mode gives the same result in AirOffer. |
| X03 | The rows of those documents that concern selections are reproduced through `AncillaryOffers/Details`, with the error codes of §6.3 in place of Ancillary's: stale version or rule → `6031`; not applicable → `6031`; quantity → `6032`; same occurrence twice (including a flight named with and without its journey) → `6033`. |

### Service list

| # | Behaviour |
|---|---|
| L01 | The golden example of `phases/P2-Lounge-Access/phase.md` §3 → three offer items in order bag `B1`, lounge `F1`, bag `B2`; two service definitions (`SD1` bag, `SD2` lounge); the bag items reference `SD1`. |
| L02 | Bag item: `unitPrice` 35.00 / tax `VAT` 3.50 / total 38.50; `eligibility` = one traveller, the journey, its one flight; `quantityRule` 1–1. Lounge item: 20.00, no taxes, `paxSegmentRefIds` = the flight, `paxJourneyRefId` = its journey. |
| L03 | Service definition of the bag: `rfic C`, `rfisc 0CC`, `serviceType ExtraBaggage`, classification with `groupCode BG` and `description1Code B1`, `documentType EmdAssociated`, `bookingInstructions null`, `baggage` filled, `lounge null`. |
| L04 | `offerId` = `A1~{currencyId}~{issuedAt}`; every `offerItemId` begins with it and decodes to the item's product, version, rule, traveller and journey or segment; `offerExpirationDateTime` = `issuedAt` + validity. |
| L05 | Nothing applicable → empty lists, HTTP 200. |
| L06 | A product row whose `Type` number is unknown to AirOffer is ignored; the other products are listed. |

### Details

| # | Behaviour |
|---|---|
| D01 | Selecting the bag item of L01 with quantity 1 → one item with every member of Master §7.4, `offerItemId`, `quantity 1`, `total 38.50`. |
| D02 | A changed or truncated id; an id of another `offerRefId`; an id whose currency differs from the request → `6029`. |
| D03 | An id older than the validity → `6030`. |
| D04 | An id whose traveller or journey is not in the context → `6031`. |
| D05 | After the snapshot shows a new product version or a new price rule, an id minted before → `6031`; a new list gives a new id that works. |
| D06 | A generic bag (1–2) with quantity 2 → `amount` doubled; quantity 3 → `6032`. With `existing` 1 → quantity 2 → `6032`. |
| D07 | Two selected items, the second invalid → the whole request fails with the second's error. |

### Catalogue cache

| # | Behaviour |
|---|---|
| S01 | The snapshot holds only Active products and Active rules with their lines in `Id` order; JSON list columns are read as lists, `NULL` as "no restriction". |
| S02 | When the stamp changes, the next probe reloads; when it does not, nothing is reloaded before `MaxAgeMinutes`. |
| S03 | Before the first successful load both operations answer 503 with `6034`; a failed reload keeps the previous snapshot. |
| S04 | All existing tests are unchanged and green. |

## 9. Manual proof

With Ancillary's development database holding the data of its `phases/P2-Lounge-Access/proof.http` run (airline with `XBAG1`, `LNGTHR` and their rules Active):

1. `POST Service/v1/AncillaryOffers` with the context of that file's request 21 → three offer items (bag `B1`, lounge `F1`, bag `B2`) with the prices 35.00, 20.00, 35.00 — the same occurrences and amounts Ancillary's own quote returns for that request.
2. `POST Service/v1/AncillaryOffers/Details` selecting the lounge item → its full item, total 20.00.
3. In Ancillary, retire the lounge rule and activate a new one at 25.00. Within the probe interval the list shows 25.00 and a new `offerItemId`; the old id is refused with `6031`.

The prompt for this work is `handover/AirOffer/prompt.md`.
