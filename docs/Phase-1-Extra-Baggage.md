# Phase 1 — Extra Baggage

**Authority:** `Ancillary-Domain-Master.md` (R5.3). This document only says which part of it is built now and gives concrete examples.
**Business outcome:** an airline user enables a baggage sub code, defines an "extra checked bag" product on it and its price, with any separate tax. For an existing order, AirOffer asks which bags can be sold to which traveller on which bound and for how much, and receives everything Ordering needs to record the sale and issue an EMD-A.

## 1. What is built

| Master section | Built in Phase 1 |
|---|---|
| §4 `AncillaryProduct` | every field, value object, operation and invariant marked P1 |
| §4.5 `ServiceSubCode` | all of it, including the industry reference with its one entry `0CC` |
| §5 `AncillaryPriceRule` | every field, operation and invariant marked P1; conditions `PassengerTypes`, `OriginAirportIds`, `DestinationAirportIds`; `Ancillary` and `Tax` lines |
| §7 Quote | everything marked P1: bound-scoped occurrences, `existing`, catalogue and selection modes |
| §9 Enumerations | only the members marked P1 |
| §10 Operations | the rows marked P1 |
| §11 Errors | the rows marked P1 |

In Phase 1 the only product shape is: `ExtraBaggage`, `TravellerBound`, `EmdAssociated`, `Unlimited`, unit `Piece`, with a `Baggage` detail. A request naming any other value (`TravellerSegment`, `EmdStandalone`, `Quota`, `Each`, `LoungeAccess`, `Fee`, …) is refused as malformed (HTTP 400), because those values do not exist yet.

Not built: anything marked P2 or later; no ordering of bags (first, second, third); no message is published; no other service is called.

Before starting, delete the superseded file `docs/AeroTech-Ancillary-Master-Domain-ADR-PRD-v1.1-FINAL.md` that came with the repository skeleton.

## 2. Backoffice operations

All under `Backoffice/v1`.

### 2.0 Service sub codes

`POST ServiceSubCodes` — an **industry** code (begins with a digit) is enabled with the airline and the code only:

```json
{ "ownerAirlineId": 10, "code": "0CC" }
```

The service takes the attributes from its industry reference: `rfic C`, `groupCode BG`, `subGroupCode null`, `description1Code B1`, `description2Code null`, `commercialName FIRST EXCESS BAG`.

`POST ServiceSubCodes` — a **carrier-defined** code (begins with a letter) carries its attributes:

```json
{
  "ownerAirlineId": 10,
  "code": "XBG",
  "rfic": "C",
  "groupCode": "BG",
  "subGroupCode": null,
  "description1Code": null,
  "description2Code": null,
  "commercialName": "EXTRA BAG"
}
```

| Operation | Success | Result body (`data`) |
|---|---|---|
| `POST ServiceSubCodes` | 200 | `{ id, ownerAirlineId, code, source, status }` |
| `POST ServiceSubCodes/{id}/Retire` · `/Reactivate` | 200 | same |
| `GET ServiceSubCodes/{id}` | 200 / 404 (`16110`) | every field of Master §4.5 |
| `GET ServiceSubCodes/Paginated` | 200 | filters `ownerAirlineId`, `code`, `source`, `status`; default order `code` |

### 2.1 Products

`POST AncillaryProducts` takes the body below. `PUT AncillaryProducts/{id}` takes the same body **without** `ownerAirlineId`, `productRef` and `type` (they are immutable and are not members of the `PUT` request type).

```json
{
  "ownerAirlineId": 10,
  "productRef": "XBAG1",
  "type": "ExtraBaggage",
  "name": "First extra bag 23kg",
  "description": null,
  "salesScope": "TravellerBound",
  "quantity": { "unit": "Piece", "min": 1, "max": 1 },
  "document": { "type": "EmdAssociated", "rfisc": "0CC" },
  "codes": { "serviceTypeCode": "C" },
  "terms": { "refundable": false, "commissionable": null, "reusable": null, "formOfRefundCode": null, "interlineSettlementAllowed": null },
  "inventoryControl": "Unlimited",
  "baggage": { "pieces": 1, "weight": 23, "weightUnit": "Kg" }
}
```

The sub code `0CC` must already be enabled and Active for the airline. `rfic`, `groupCode`, `subGroupCode`, `description1Code` and `description2Code` are not part of the request: the service copies them from the sub code and returns them on `GET`.

| Operation | Success | Result body (`data`) |
|---|---|---|
| `POST AncillaryProducts` | 200 | `{ id, ownerAirlineId, productRef, version, status }` |
| `PUT AncillaryProducts/{id}` | 200 | same |
| `POST AncillaryProducts/{id}/Activate` | 200 | same, plus `retiredVersionId` (`long?`) when a previous version was retired |
| `POST AncillaryProducts/{id}/Suspend` | 200 | same |
| `POST AncillaryProducts/{id}/Retire` | 200 | same |
| `POST AncillaryProducts/{id}/Revise` | 200 | the new Draft: `{ id, ownerAirlineId, productRef, version, status }` |
| `GET AncillaryProducts/{id}` | 200 / 404 (`16101`) | every P1 field of the product (Master §4.1) |
| `GET AncillaryProducts/Paginated` | 200 | page of the same DTO; filters `ownerAirlineId`, `productRef`, `type`, `status`; default order `productRef`, then `version` descending |

A malformed body (missing member, unknown value name, wrong length or format) is HTTP 400. Every other rule of Master §4 is `16106`, except the sub-code rule, which is `16109`.

### 2.2 Price rules

`POST AncillaryPriceRules` takes the body below. `PUT AncillaryPriceRules/{id}` takes the same body **without** `ownerAirlineId` and `productRef`.

```json
{
  "ownerAirlineId": 10,
  "productRef": "XBAG1",
  "priority": 1,
  "currencyId": 978,
  "lines": [
    { "category": "Ancillary", "code": null, "name": null, "amount": 35.00 },
    { "category": "Tax", "code": "VAT", "name": "Value added tax", "amount": 3.50 }
  ],
  "salesFrom": null,
  "salesTo": null,
  "travelFrom": null,
  "travelTo": null,
  "conditions": { "passengerTypes": null, "originAirportIds": null, "destinationAirportIds": null }
}
```

| Operation | Success | Result body (`data`) |
|---|---|---|
| `POST AncillaryPriceRules` | 200 | `{ id, ownerAirlineId, productRef, priority, currencyId, status }` |
| `PUT AncillaryPriceRules/{id}` | 200 | same |
| `POST AncillaryPriceRules/{id}/Activate` · `/Suspend` · `/Retire` | 200 | same |
| `GET AncillaryPriceRules/{id}` | 200 / 404 (`16201`) | every P1 field of the rule (Master §5.1) |
| `GET AncillaryPriceRules/Paginated` | 200 | filters `ownerAirlineId`, `productRef`, `currencyId`, `status`; default order `productRef`, `currencyId`, `priority` |

`passengerTypes` items are names of `PassengerTypeCode` (e.g. `"ADT"`); an unknown name is HTTP 400.

## 3. Quote — `POST Service/v1/AncillaryQuotes`

Wire contract: `contracts/ancillary-quotes-v1.openapi.yaml`. Meaning: Master §7.

Phase-1 specifics:

- Only bound-scoped products exist, so every item has `flightRef = null`. An extra bag is offered only where the traveller flies exactly one flight of the bound (Master §7.2), so `coveredFlightRefs` always has exactly one entry.
- The quote reports price lines and their total and nothing about documents' values. What Ordering later puts on an EMD (coupon value = the `Ancillary` line; taxes at document level) is Ordering's rule and changes nothing here.
- `inventory = { "control": "Unlimited", "remaining": null }` on every item.
- `salesContext` and the flight facts no Phase-1 rule uses (`flightCapacityId`, `operatingAirlineId`, `aircraftId`, `cabinClassId`, `rbdId`) are accepted and ignored.

### Golden example

Data: the sub code `0CC` is enabled; the product of §2.1 and the rule of §2.2 are `Active`. `{ruleId}` is the id of that rule.

Request:

```json
{
  "currencyId": 978,
  "asOf": "2026-10-01T14:00:00+00:00",
  "salesContext": { "channel": "BackOffice", "customerId": null, "travelAgencyId": null, "countryId": null },
  "travellers": [ { "ref": "T1", "passengerTypeCode": "ADT", "flightRefs": ["F1"] } ],
  "bounds": [
    { "ref": "B1",
      "flights": [
        { "ref": "F1", "flightId": "100", "flightCapacityId": "5001",
          "originAirportId": 1, "destinationAirportId": 2,
          "departureDateTime": "2026-10-10T08:00:00+03:30",
          "marketingAirlineId": 10, "operatingAirlineId": 10,
          "aircraftId": 7, "cabinClassId": 2, "rbdId": "5" } ] } ],
  "selections": [
    { "productRef": "XBAG1", "productVersion": 1, "priceRuleId": "{ruleId}",
      "travellerRef": "T1", "boundRef": "B1", "flightRef": null, "quantity": 1 } ]
}
```

Response `data`:

```json
{
  "currencyId": 978,
  "asOf": "2026-10-01T14:00:00+00:00",
  "items": [
    {
      "ownerAirlineId": 10, "productRef": "XBAG1", "productVersion": 1,
      "type": "ExtraBaggage", "name": "First extra bag 23kg", "description": null,
      "salesScope": "TravellerBound",
      "travellerRef": "T1", "boundRef": "B1", "flightRef": null, "coveredFlightRefs": ["F1"],
      "unit": "Piece", "minQuantity": 1, "maxQuantity": 1, "quantity": 1,
      "codes": { "serviceTypeCode": "C", "groupCode": "BG", "subGroupCode": null, "description1Code": "B1", "description2Code": null },
      "baggage": { "pieces": 1, "weight": 23, "weightUnit": "Kg" },
      "terms": { "refundable": false, "commissionable": null, "reusable": null, "formOfRefundCode": null, "interlineSettlementAllowed": null },
      "document": { "type": "EmdAssociated", "rfic": "C", "rfisc": "0CC" },
      "inventory": { "control": "Unlimited", "remaining": null },
      "priceRuleId": "{ruleId}",
      "priceLines": [
        { "category": "Ancillary", "code": null, "name": "First extra bag 23kg", "unitAmount": 35.00, "amount": 35.00 },
        { "category": "Tax", "code": "VAT", "name": "Value added tax", "unitAmount": 3.50, "amount": 3.50 }
      ],
      "unitTotal": 38.50, "total": 38.50
    }
  ]
}
```

The same request without `selections` returns the same item. Amounts are compared numerically, not as text.

## 4. Expected behaviour

These rows state precisely how the service must behave. Data used by the rows, all for airline 10 unless a row says otherwise:

| Name | Definition |
|---|---|
| Sub code S | the industry code `0CC`, enabled as in §2.0 |
| Sub code G | the carrier-defined code `XBG` of §2.0 |
| Product X | the §2.1 example: `XBAG1` on `0CC`, quantity 1–1 |
| Product G | `XBAGG`, name `Extra bag 23kg`, on `XBG`, quantity `Piece` 1–2, otherwise like Product X |
| Rule R | the §2.2 example, for `XBAG1` |
| Rule RG | for `XBAGG`: priority 1, currency 978, one `Ancillary` line 30.00 |

A sub code is enabled before a product naming it is defined.

### Service sub codes

| # | Behaviour |
|---|---|
| S01 | Register `0CC` with only airline and code → `Active`, `source = Industry`; `GET` returns `rfic C`, `groupCode BG`, `subGroupCode null`, `description1Code B1`, `description2Code null`, `commercialName FIRST EXCESS BAG`. |
| S02 | Register `0CC` with any attribute member present (for example `rfic`, `groupCode` or `commercialName`), even with the reference's own value → `16112`. |
| S03 | Register `0CD`, or any other digit-leading code that is not in the industry reference → `16114`. |
| S04 | Register `XBG` as in §2.0 → `Active`, `source = CarrierDefined`; `GET` returns every field as sent. Without `rfic`, `groupCode` or `commercialName` → HTTP 400. |
| S05 | `code` `"98A"` or `"99Z"` → `16112`. `code` not three characters `A–Z0–9`, `rfic` not one letter, `groupCode` not two characters, `commercialName` longer than 30 characters or containing `/`, `-` or `.` → HTTP 400. |
| S06 | Register a code the airline already has, whether `Active` or `Retired` → `16111`. The same code for another airline succeeds (an industry code with the same reference attributes; a carrier-defined code with its own). |
| S07 | Retire → `Retired`. Retire again → `16113`. Reactivate → `Active` with exactly the same attributes. Reactivate an `Active` code → `16113`. Unknown id → `16110`. |
| S08 | There is no operation that changes the attributes of a sub code, and no way to give a retired code other attributes. |

### Catalog

| # | Behaviour |
|---|---|
| C01 | Define Product X → `Draft`, `Version = 1`; `GET` returns every field as sent, plus `document.rfic = "C"`, `codes.groupCode = "BG"`, `codes.subGroupCode = null`, `codes.description1Code = "B1"`, `codes.description2Code = null` copied from Sub code S. |
| C02 | Define a second product with the same `ownerAirlineId + productRef` → `16102`. The same `productRef` for another airline succeeds. |
| C03 | `productRef` `"xbag"`, `"X"`, `"X-BAG"` or 21 characters → HTTP 400. |
| C04 | Change a Draft → all mutable fields replaced and the codes copied again from the named sub code; `ownerAirlineId`, `productRef`, `type`, `version` unchanged. |
| C05 | Change an `Active`, `Suspended` or `Retired` version → `16103`. |
| C06 | A value that does not exist in Phase 1 — `salesScope = "TravellerSegment"`, `document.type = "EmdStandalone"`, `inventoryControl = "Quota"`, `quantity.unit = "Each"`, `type = "LoungeAccess"` → HTTP 400 on Define and on Change. |
| C07 | `baggage = null`; or `pieces` and `weight` both null; or `weight` without `weightUnit`; or `weightUnit` without `weight` → `16106`. |
| C08 | `document.rfisc` null → `16106`. `rfisc = "0c"` → HTTP 400. `codes.serviceTypeCode` missing → HTTP 400. A request that carries `rfic`, `groupCode`, `subGroupCode` or a description code has those members ignored. |
| C09 | `document.rfisc` naming a code the airline has not enabled, or a `Retired` one → `16109` on Define and on Change. |
| C10 | Classification of an extra bag: a carrier-defined sub code registered with `rfic E`, or with `groupCode LG` → `16106`. `serviceTypeCode = "F"` → `16106`; `"P"` is accepted. |
| C11 | Industry code constraints: a product on `0CC` with `quantity` `1–2`, or `2–2` → `16106` on Define, Change and Activate; quantity `1–1` is accepted. Product G on the carrier-defined `XBG` with `1–2` is accepted and remains a carrier-local code. |
| C12 | Interline: Product G (carrier-defined sub code) with `terms.interlineSettlementAllowed = true` → `16106`; with `false` or `null` → accepted. Product X (industry sub code) with `true` → accepted. |
| C13 | `quantity` of Product G with `min = 0`, `min > max` or `max = 100` → `16106`. |
| C14 | Activate a Draft → `Active`, `ActivatedAt` set from the clock. |
| C15 | Suspend an Active → `Suspended`; Activate it again → `Active`, `ActivatedAt` unchanged. |
| C16 | Suspend a Draft; Activate an Active; any operation on a Retired → `16104`. |
| C17 | Retire a Draft, an Active and a Suspended version → `Retired`, `RetiredAt` set. |
| C18 | Revise an Active v1 → a new Draft v2 with identical field values, including the copied codes; v1 unchanged and still `Active`. |
| C19 | Revise while a Draft of the product exists → `16107`. Revise a Draft or a Retired version → `16108`. |
| C20 | Activate Draft v2 while v1 is `Active` → v2 `Active` and v1 `Retired` together; result carries `retiredVersionId = v1.Id`. Same when v1 is `Suspended`. |
| C21 | After C20, `GET` of v1 still returns it with all fields; Paginated with `productRef` filter returns both versions, v2 first. |
| C22 | Two concurrent Activate calls on the same Draft → exactly one succeeds; one `Active` version exists. |
| C23 | Operation on an unknown id → `16101` (HTTP 404). |
| C24 | Sub code S is retired while Product X is a Draft → Activate → `16109`. After Reactivate of Sub code S → Activate succeeds. |
| C25 | Sub code S is retired while Product X is `Active` → Product X is unchanged and still offered. A Suspended Product X cannot be activated again until Sub code S is reactivated (`16109`). |

### Pricing

| # | Behaviour |
|---|---|
| R01 | Define Rule R for existing Product X (any status) → `Draft`; `GET` returns every field as sent, both lines in the order sent. |
| R02 | Define a rule for a `productRef` that does not exist for that airline → `16205`. |
| R03 | No `Ancillary` line; two `Ancillary` lines; a `Tax` line without `code`; two `Tax` lines with the same `code` → `16206`. `category = "Fee"` → HTTP 400. A rule with one `Ancillary` line and no tax line succeeds. |
| R04 | `amount = 0` or negative on any line; `amount = 35.005`; `priority = 0`; `salesFrom ≥ salesTo`; `travelFrom > travelTo`; any condition list empty or with a duplicate → `16206`. |
| R05 | Change a Draft rule → replaced. Change a non-Draft rule → `16202`. |
| R06 | Activate a second rule with the same airline, product, currency and priority as an `Active` rule → `16204`. With a different currency or priority → succeeds. |
| R07 | Suspend the first rule, activate the second with the same priority → succeeds; re-activating the first → `16204`. |
| R08 | Suspend a Draft; Activate an Active; any operation on a Retired rule → `16203`. Unknown id → `16201`. |

### Quote

Product X with Rule R, and Product G with Rule RG, are `Active` unless a row says otherwise.

| # | Behaviour |
|---|---|
| Q01 | Golden example, selection mode → exactly the documented response. |
| Q02 | Golden example without `selections`, with only Product X Active → the same single item, carrying `productVersion` and `priceRuleId`. |
| Q03 | Two travellers (`T1`, `T2`), two one-flight bounds (`B1`, `B2`), both travellers on both flights, catalogue mode, only Product X Active → four items in order `T1/B1`, `T1/B2`, `T2/B1`, `T2/B2`. With Product G also Active → eight items; within each traveller and bound, `XBAG1` before `XBAGG`. |
| Q04 | A bound with two flights `F1`, `F2`, traveller on both → no item for that traveller and bound in catalogue mode; a selection of it → `16305`. Other bounds and travellers unaffected. |
| Q05 | A one-flight bound whose flight has `marketingAirlineId ≠ 10` → no item for that bound; other bounds unaffected. |
| Q06 | Request currency 840 with only 978 rules → empty `items`, HTTP 200. |
| Q07 | Two rules for Product X, priority 1 (35.00) and priority 2 (20.00), both matching → priority 1 selected. |
| Q08 | Priority-1 rule with `passengerTypes = ["CHD"]`, priority-2 rule without conditions; traveller `ADT` → priority 2; traveller `CHD` → priority 1. |
| Q09 | Rule with `salesFrom = 2026-10-01T00:00Z`, `salesTo = 2026-10-02T00:00Z`: `asOf` at `salesFrom` → priced; at `salesTo` → not offered; one second before `salesFrom` → not offered. The service clock is set far away and has no effect. |
| Q10 | Rule with `travelFrom = travelTo = 2026-10-10`; departure `2026-10-10T23:30:00+03:30` → priced (local date 10 Oct); departure `2026-10-11T00:30:00+03:30` → not offered. |
| Q11 | One-flight bound `F1` (1 → 2). Priority-1 rule with `originAirportIds = [1]` and `destinationAirportIds = [2]` → selected. Priority-1 rule with `originAirportIds = [2]` → not matched; with `destinationAirportIds = [1]` → not matched; the priority-2 rule without conditions is used instead. |
| Q12 | Product `Draft`, `Suspended` or `Retired` → not offered. Rule `Draft`, `Suspended` or `Retired` → not used. |
| Q13 | Product X revised and v2 activated with a new name → catalogue item carries `productVersion = 2` and v2's name; the rule is still used. A selection still carrying `productVersion = 1` → `16309`. |
| Q14 | Rule R retired and a new rule activated (same or different price) → a selection still carrying Rule R's id → `16309`; a selection carrying the new rule's id → priced. |
| Q15 | Bound `F1`, `F2`: traveller `T2` with `flightRefs [F2]` → one item with `coveredFlightRefs [F2]`, and a rule with `originAirportIds` = the origin of `F1` does not match for him; traveller `T1` with `flightRefs [F1, F2]` → no item. A traveller with no flight in a bound gets no item for it. A `flightRefs` entry that is not a flight of the request → `16301`. |
| Q16 | Selection of unknown `productRef` → `16302`. |
| Q17 | Selection with `flightRef` set, or with `boundRef = null` → `16303`. |
| Q18 | Selection with unknown `travellerRef` or `boundRef` → `16304`. |
| Q19 | Selection for a bound of another marketing airline, or when no rule matches → `16305`. |
| Q20 | Product X: selection `quantity = 2` → `16306`. Product G: `quantity = 2` → priced, `amount = 60.00`; `quantity = 3` → `16306`. Any product: `quantity = 0` → HTTP 400. |
| Q21 | The same occurrence selected twice → `16307`. |
| Q22 | Two selections, second invalid → the whole request fails with the second's error; no partial response. |
| Q23 | Duplicate traveller `ref`, duplicate flight `ref` in two bounds, or `passengerTypeCode = "XXX"` → `16301`. |
| Q24 | A quote call changes no data and publishes nothing. |
| Q25 | `Ancillary` line with `name = "Bag fee"` → that name on the line; with `name = null` → the product name. A `Tax` line with `name = null` → `name = null`. |
| Q26 | Rule sent with the `Tax` line before the `Ancillary` line → response lists the `Ancillary` line first, then the tax line; `unitTotal` and `total` include both. |
| Q27 | `existing` = Product X, `T1`, `B1`, quantity 1 → no `XBAG1` item for `T1/B1` in catalogue mode; a selection of it → `16305`. `T2/B1` unaffected. |
| Q28 | `existing` = Product G, `T1`, `B1`, quantity 1 → the `XBAGG` item has `maxQuantity = 1`; selection of quantity 2 → `16306`; quantity 1 → priced. With `existing` quantity 2 → no `XBAGG` item for `T1/B1`. |
| Q29 | `existing` entry with an unknown `travellerRef` or `boundRef` → `16304`. Entry for a product that does not exist → ignored. A request without `existing` behaves as with an empty list. |

## 5. Proof and review

1. Start the service and run `docs/proof/phase-1.http` from top to bottom. Each request states its expected result. Report the actual status and body of every request.
2. The Owner then reviews, before anything else is built:
   - whether the sub-code, product and price-rule fields are what a backoffice user needs;
   - whether the quote answer is sufficient for AirOffer and Ordering (`docs/Phase-1-End-to-End.md` and `docs/Ordering-P1-Ancillary-Implementation-Spec.md`);
   - the manual run across the three services (`docs/Phase-1-End-to-End.md` §5), once AirOffer and Ordering have built their part.

After this review the phase's conformance tests are written (`CLAUDE.md` §4, step B): every row of §4 becomes at least one automated test. The phase is not closed before they pass.
