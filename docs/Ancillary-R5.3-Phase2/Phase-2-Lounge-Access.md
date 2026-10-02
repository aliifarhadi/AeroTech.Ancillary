# Phase 2 — Lounge Access

**Authority:** `Ancillary-Domain-Master.md` (R5.4). This document only says which part of it is built now and gives concrete examples.
**Precondition:** Phase 1 closed (its 70 behaviour rows are covered by passing tests).
**Business outcome:** on an existing order, the airline sells lounge access per traveller per flight, at the airport the flight departs from, documented by an EMD-S (not associated with a ticket coupon). AirOffer asks which lounge accesses can be sold and for how much, and receives everything Ordering needs to record the sale and issue the EMD-S.

Everything Phase 1 built stays as it is. This phase only adds — with the one adjustment of §5.

## 1. What is built

| Master section | Added in Phase 2 |
|---|---|
| §3.1–§3.3 | `TravellerSegment`, `EmdStandalone` |
| §3.4 | the `LoungeAccess` row, and the combination check (`16105`), run first (Master §4.2, "Order of the product checks") |
| §4.1 | field `Lounge` |
| §4.2 | value object `LoungeDetail`; classification row for `LoungeAccess` (`Rfic E`, `GroupCode LG`, any service type letter) |
| §4.5 | industry reference entry `0BX` (`LG`, `LOUNGE ACCESS`, RFIC `E`, document `EmdStandalone`, exactly 1) |
| §7 | flight-scoped occurrences; lounge applicability (§7.2 rule 2); occurrence identity (§7.2); item order (§7.4); `lounge` block on items |
| §9 | `AncillaryProductType.LoungeAccess = 2`, `AncillarySalesScope.TravellerSegment = 2`, `AncillaryDocumentType.EmdStandalone = 3`, `AncillaryQuantityUnit.Each = 3` |
| Contract | `Contracts/ancillary-quotes-v1.openapi.yaml` is already at version `R5.4-phase2` (adds `lounge`); the implementation must match it |

No new aggregate, operation, error code or condition. The only new storage is what `LoungeDetail` needs.

Not built: `Traveller` and `Order` scopes, `None`, `Fee`, `Quota`, `SeatMap`, `Kilogram`, any other product type or reference entry; no message; no call to another service.

## 2. Backoffice operations

Unchanged routes. Product bodies (`POST` and `PUT`) gain one member, `lounge`, which is `null` for a bag:

```json
{
  "ownerAirlineId": 10,
  "productRef": "LNGTHR",
  "type": "LoungeAccess",
  "name": "Lounge access",
  "description": null,
  "salesScope": "TravellerSegment",
  "quantity": { "unit": "Each", "min": 1, "max": 1 },
  "document": { "type": "EmdStandalone", "rfisc": "0BX" },
  "codes": { "serviceTypeCode": "F" },
  "terms": { "refundable": false, "commissionable": null, "reusable": null, "formOfRefundCode": null, "interlineSettlementAllowed": null },
  "inventoryControl": "Unlimited",
  "baggage": null,
  "lounge": { "airportIds": [1] }
}
```

The sub code `0BX` is enabled with airline and code only (`{ "ownerAirlineId": 10, "code": "0BX" }`); the service fills in `rfic E`, `groupCode LG`, `commercialName LOUNGE ACCESS`. `GET AncillaryProducts/{id}` and the paginated list return `lounge` (with `airportIds` in the order sent) and `baggage`, each `null` when not applicable.

Shape validation (HTTP 400): `lounge.airportIds`, when `lounge` is present, is a list of integers. Every other rule is a domain rule (§4).

## 3. Quote

Unchanged route. Additions:

- A `TravellerSegment` product gives one candidate per traveller × each flight in his `flightRefs`. Its item has `flightRef` = that flight, `boundRef` = the bound containing it, `coveredFlightRefs` = `[flightRef]`.
- A lounge occurrence is applicable only when its flight's `originAirportId` is in the product's `Lounge.AirportIds` (in addition to rules 1, 3 and 4 of Master §7.2).
- `OriginAirportIds` and `DestinationAirportIds` are compared with that flight's own origin and destination.
- `existing` entries of a lounge product are matched by product + traveller + `flightRef`.
- The item carries `lounge: { airportIds }`; `baggage` is `null`. A bag item carries `lounge: null`.
- `inventory = { "control": "Unlimited", "remaining": null }`.
- Item order (Master §7.4): traveller → bound → the bag items of that bound (by `productRef`) → for each flight of the bound in travel order, the flight-scoped items of that flight (by `productRef`).

### Golden example

Data: airline 10 has `0CC` and `0BX` enabled; Product X (`XBAG1`, Phase 1) with Rule R (35.00 + VAT 3.50) and Product L (§2) with Rule RL (priority 1, currency 978, one `Ancillary` line 20.00, no conditions), all Active.

Request — two one-flight bounds, `F1` departs airport 1, `F2` departs airport 2:

```json
{
  "currencyId": 978,
  "asOf": "2026-10-01T14:00:00+00:00",
  "travellers": [ { "ref": "T1", "passengerTypeCode": "ADT", "flightRefs": ["F1", "F2"] } ],
  "bounds": [
    { "ref": "B1", "flights": [
      { "ref": "F1", "flightId": "100", "originAirportId": 1, "destinationAirportId": 2,
        "departureDateTime": "2026-10-10T08:00:00+03:30", "marketingAirlineId": 10, "operatingAirlineId": 10 } ] },
    { "ref": "B2", "flights": [
      { "ref": "F2", "flightId": "101", "originAirportId": 2, "destinationAirportId": 1,
        "departureDateTime": "2026-10-15T08:00:00+03:30", "marketingAirlineId": 10, "operatingAirlineId": 10 } ] } ]
}
```

Response `data.items`, in this order: `XBAG1` for `T1/B1`; `LNGTHR` for `T1/F1`; `XBAG1` for `T1/B2`. No lounge item for `F2`. The lounge item (`{ruleIdRL}` = id of Rule RL):

```json
{
  "ownerAirlineId": 10, "productRef": "LNGTHR", "productVersion": 1,
  "type": "LoungeAccess", "name": "Lounge access", "description": null,
  "salesScope": "TravellerSegment",
  "travellerRef": "T1", "boundRef": "B1", "flightRef": "F1", "coveredFlightRefs": ["F1"],
  "unit": "Each", "minQuantity": 1, "maxQuantity": 1, "quantity": 1,
  "codes": { "serviceTypeCode": "F", "groupCode": "LG", "subGroupCode": null, "description1Code": null, "description2Code": null },
  "baggage": null,
  "lounge": { "airportIds": [1] },
  "terms": { "refundable": false, "commissionable": null, "reusable": null, "formOfRefundCode": null, "interlineSettlementAllowed": null },
  "document": { "type": "EmdStandalone", "rfic": "E", "rfisc": "0BX" },
  "inventory": { "control": "Unlimited", "remaining": null },
  "priceRuleId": "{ruleIdRL}",
  "priceLines": [ { "category": "Ancillary", "code": null, "name": "Lounge access", "unitAmount": 20.00, "amount": 20.00 } ],
  "unitTotal": 20.00, "total": 20.00
}
```

The two bag items are exactly as in Phase 1 plus `"lounge": null`.

## 4. Expected behaviour

Data used by the rows, all for airline 10 unless a row says otherwise:

| Name | Definition |
|---|---|
| Sub code S | `0CC`, enabled (Phase 1) |
| Sub code L | `0BX`, enabled with airline and code only |
| Sub code GL | carrier-defined `XLG`: `rfic E`, `groupCode LG`, `commercialName LOUNGE` |
| Product X, Rule R | as in Phase 1 |
| Product L | the §2 example on `0BX` |
| Product LG | `LNGXLG` on `XLG`, `LoungeAccess`, `TravellerSegment`, `EmdStandalone`, `Unlimited`, quantity `Each` 1–2, `lounge.airportIds [1, 5]` |
| Rule RL | for `LNGTHR`: priority 1, currency 978, one `Ancillary` line 20.00 |
| Rule RLG | for `LNGXLG`: priority 1, currency 978, one `Ancillary` line 15.00 |

### Service sub codes

| # | Behaviour |
|---|---|
| S01 | Register `0BX` with only airline and code → `Active`, `source Industry`; `GET` returns `rfic E`, `groupCode LG`, `subGroupCode null`, `description1Code null`, `description2Code null`, `commercialName LOUNGE ACCESS`. |
| S02 | Register `0BX` with any attribute member present → `16112`. |
| S03 | `0CD` is still not in the reference → `16114`. |

### Catalog

| # | Behaviour |
|---|---|
| C01 | Define, activate and read Product L → every field as sent; `lounge.airportIds [1]`; `baggage null`; copied `document.rfic E`, `codes.groupCode LG`. Product X read again → `lounge null`. |
| C02 | Combination (`16105`), on Define and Change: `LoungeAccess` with `TravellerBound`, with `EmdAssociated`, or with `Piece`; `ExtraBaggage` with `TravellerSegment`, with `EmdStandalone`, or with `Each`. |
| C03 | The combination check runs first: `LoungeAccess` with `TravellerBound` **and** no `lounge` **and** a sub code that is not enabled → `16105` (not `16106`, not `16109`). |
| C04 | Lounge detail (`16106`): `LoungeAccess` without `lounge`; with `airportIds` empty, repeated, or containing `0`; with `baggage` set. `ExtraBaggage` with `lounge` set → `16106`. |
| C05 | Lounge classification (`16106`): `LoungeAccess` naming `0CC` (`rfic C`, `groupCode BG`); naming a carrier-defined code registered with `rfic E` but `groupCode BG`, or with `groupCode LG` but `rfic C`. Any service type letter is accepted for a lounge. |
| C06 | Industry code constraints for `0BX`: quantity `1–2` → `16106`; quantity `1–1` accepted. (An `0BX` product with `EmdAssociated` is a combination failure, `16105`.) Product LG on carrier-defined `XLG` with quantity `1–2` is accepted. |
| C07 | `terms.interlineSettlementAllowed = true` on Product LG (carrier-defined, but not baggage) → accepted. |
| C08 | Change a Draft lounge product → `lounge` replaced; codes copied again from the named sub code. Revise an Active Product L → the new Draft carries the same `lounge`. |
| C09 | `ExtraBaggage` naming carrier-defined `XLG` (`rfic E`, `groupCode LG`) → `16106` (bag classification, unchanged from Phase 1). |

### Quote

Products X, L, LG and their rules are Active unless a row says otherwise.

| # | Behaviour |
|---|---|
| Q01 | Golden example (§3) with only Products X and L Active → exactly the documented items, in the documented order. |
| Q02 | One bound `B1` with flights `F1` (1 → 2) and `F2` (2 → 3); `T1` flies both; only Products X and L Active → one item: lounge for `F1`. No bag (two covered flights), no lounge for `F2`. |
| Q03 | Same bound; `T2` flies only `F2` → one item: bag for `T2/B1` covering `[F2]`; no lounge (`F2` departs airport 2). |
| Q04 | Item order with Products X, L and LG Active, one traveller, two one-flight bounds as in the golden example → `XBAG1/B1`, `LNGTHR/F1`, `LNGXLG/F1`, `XBAG1/B2`. With `F2` departing airport 5 instead → `XBAG1/B1`, `LNGTHR/F1`, `LNGXLG/F1`, `XBAG1/B2`, `LNGXLG/F2`. |
| Q05 | Lounge selection `flightRef F1`, `boundRef null` → priced, item `boundRef B1`. With `boundRef B1` → priced, identical item. |
| Q06 | Lounge selection with `boundRef B2` and `flightRef F1` → `16303`. Without `flightRef` (only `boundRef B1`) → `16303`. Bag selection with `flightRef` set → `16303` (unchanged). |
| Q07 | Lounge selection for `F2` (departs airport 2) → `16305`. For a flight that is in the request but not in the traveller's `flightRefs` → `16305`. For an unknown `flightRef` → `16304`. |
| Q08 | Two lounge selections for the same traveller and flight, one with `boundRef null` and one with `boundRef B1` → `16307`. The same flight for two different travellers → both priced. |
| Q09 | Product LG, selection quantity 2 → priced, `amount 30.00`; quantity 3 → `16306`. Product L quantity 2 → `16306`. |
| Q10 | `existing` = `LNGTHR`, `T1`, `flightRef F1`, quantity 1 → no `LNGTHR` item for `T1/F1`; selection → `16305`. `T1/F2` and other travellers unaffected. |
| Q11 | `existing` = `LNGXLG`, `T1`, `flightRef F1`, quantity 1 → `LNGXLG/F1` item with `maxQuantity 1`; selection of 2 → `16306`. |
| Q12 | `existing` = `LNGTHR`, `T1`, `boundRef B1`, no `flightRef` → no effect on any lounge item. |
| Q13 | A rule for Product L with `originAirportIds [2]` (priority 1) next to Rule RL (priority 2) → for `F1` (origin 1) Rule RL is used. A rule for Product L with `destinationAirportIds [2]` (priority 1) → used for `F1` (destination 2). |
| Q14 | A lounge flight marketed by another airline → no lounge item for it; selection → `16305`. |
| Q15 | Lounge selection carrying an old `productVersion` or `priceRuleId` → `16309` (same rule as Phase 1). |
| Q16 | A quote call changes no data and publishes nothing. |

### Regression

| # | Behaviour |
|---|---|
| G01 | The whole Phase-1 test suite passes, with only the adjustment of §5. |
| G02 | `docs/proof/phase-1.http` still passes (its request 30 now uses `Quota`). |

## 5. Adjustment of Phase-1 expectations

These Phase-1 expectations change because Phase 2 makes the values exist. This is a planned change, not a weakening:

| Phase-1 row | Before | From Phase 2 |
|---|---|---|
| C06 | `TravellerSegment`, `EmdStandalone`, `Each`, `LoungeAccess` → HTTP 400 | Those four cases leave the Phase-1 test and are covered by P2_C02 (`16105`). The test keeps `Quota` and adds `SeatMap`, `None`, `Traveller`, `Order`, `Kilogram`, `PetInCabin`, `Seat`, `Fee` — each still HTTP 400. |
| proof request 30 | `LoungeAccess` → 400 | `Quota` on a bag → 400 (already changed in `phase-1.http`). |
| Q01 golden response | the bag item has no `lounge` member | the bag item carries `"lounge": null`; the golden response in `Phase-1-Extra-Baggage.md` §3 already shows it. The Phase-1 contract tests read that document and the OpenAPI file, so they follow without change. |
| Open-enum names | the contract listed the names "Known in Phase 1" | the contract lists them as "Known: …" (all names that exist now). Tests that compare those lists with the code's enums follow without change. |

No other Phase-1 test may change.

## 6. Proof

Run `docs/proof/phase-2.http` from top to bottom against the development database, with an airline that has no Ancillary data yet (the file registers its own sub codes and products). Then run `docs/proof/phase-1.http` with its own airline; it must still pass.

## 7. Cross-service

Ordering's side of Phase 2 (selling a lounge access and issuing an EMD-S) is a separate Ordering document, written when Ordering starts its ancillary work. Phase 2 of Ancillary closes on its own proof and tests, as Phase 1 did.
