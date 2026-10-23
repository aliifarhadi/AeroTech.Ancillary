# Pack v8 — Ordering handoff contract for ResolveGrouped

This is the contract the Ordering service adopts later under its own gate. It describes what Ancillary returns and what Ordering must check itself. No Ordering code is written in this stage.

## 1. Call

```http
POST /Service/v1/Ancillaries/Selections/ResolveGrouped
Content-Type: application/json

{"ids":["d_<signed-A01-qty2>","s_<typed-A15>"]}
```

| Rule | Behaviour |
|---|---|
| Body | `ids` only; any other member is refused with 400 |
| Ids | 1–50, non-blank, each once; otherwise 422 (16801) |
| `d_` | signature, purpose `d`, expiry; quantity from the signed payload |
| `s_` | snapshot row by id; expiry from the row; selected values from the row |
| `r_` or any other text | 422 (16816), the purpose is not accepted |
| Active chain | Rate → Pricing (Active) → Provision (Active) → Definition (Active); otherwise 409 (16820) |
| Batch | all or nothing: one bad id fails the whole call |
| Side effects | none: read-only, retry-safe, the snapshot is never consumed |

## 2. Answer

The answer holds the ten family arrays: `baggages`, `meals`, `pets`, `assistedTravel`, `airportServices`, `priorityServices`, `connectivity`, `upgrades`, `extraSeats`, `travelInsurance`. Each array item looks like this:

```json
{
  "selectionId": "d_<signed-A01-qty2>",
  "variantCode": "A01",
  "selectionScope": "PerTravellerPerPortion",
  "unit": { "travellerRef": "ADT-1", "portionRef": "B1" },
  "coverage": { "travellerRefs": ["ADT-1"], "portionRefs": ["B1"], "flightRefs": [] },
  "sourceBinding": { "offerHash": "<43 chars>", "salesHash": "<43 chars>" },
  "definition": { "definitionVersionId": "<id>", "serviceDefinitionRef": "<ref>", "variantCode": "A01", "serviceTypeCode": "<code>", "serviceSubCode": "<code>" },
  "provision": { "provisionId": "<id>", "coverageScope": "Portion", "purchaseStage": "<stage>", "bookingMethod": "<method>", "bookingRequired": true, "confirmationRequirement": "Immediate", "documentRouting": "<routing>", "documentType": "<type>", "documentRequired": true, "requiresCheckAtReserve": false },
  "pricing": { "pricingId": "<id>", "pricingRateId": "<id>", "pricingVersion": 1, "currencyId": 70, "quantity": 2, "unitAmount": 0, "totalAmount": 0, "pricingUnit": "PerPiece", "baseAmount": 0, "taxLines": [], "feeLines": [] },
  "details": { "chargeKind": "ExtraPiece" }
}
```

A typed item (`s_`) adds `selected` with exactly the family body the customer sent to POST Options, for example `{"assistanceSsrCode":"WCHS"}`. All values above are placeholders. Real values come from the Domain, and long ids are written as strings by the platform JSON settings.

| Field | Source |
|---|---|
| `unit`, `selectionScope` | the signed or stored unit: Order, Traveller+Journey, Traveller+Portion, Traveller+Flight |
| `coverage` | only the refs the unit carries; a Portion unit has `flightRefs: []` and a Flight unit has `portionRefs: []` |
| `sourceBinding.offerHash` | `SHA256("AERO-OFFER-v1|" + B64(offerId) + "|" + B64(trustedSourceVersion))`, Base64Url |
| `sourceBinding.salesHash` | `SHA256("AERO-SALE-v1|" + ownerAirlineId + "|" + pointOfSaleId + "|" + customerIdOrDash)`, Base64Url (your ruling: no channel, agency, office or country) |
| `pricing.unitAmount` | `AncillaryPricingRate.UnitTotal` of the Domain: base plus applied components, included taxes excluded |
| `pricing.totalAmount` | `unitAmount × quantity` |
| `taxLines`, `feeLines` | the rate components by category |

## 3. What Ordering must check (pack 05 §A)

| Check | How |
|---|---|
| Offer binding | recompute `offerHash` from the Order's own offer id and trusted version; reject on mismatch |
| Sales binding | recompute `salesHash` from the Order's verified owner, point of sale and customer; reject on mismatch |
| Traveller | `unit.travellerRef` must be a traveller of the Order's offer, with the PTC the Order holds |
| Portion | expand `portionRef` to the Order's own flights; never charge twice for its legs |
| Duplicates | refuse the same `selectionId`, and the same commercial service for the same unit, more than once |
| Price | record the returned pricing as the accepted price of the Order line |
| Not a hold | a resolved item is neither reserved nor ticketed; reservation is a later step |

## 4. Scenario map (pack 05 §C5)

| Case | Covered by |
|---|---|
| R01 mixed `d_` + `s_` | one call, items grouped by the definition's family |
| R02 no Order/Offer/POS/Country | body is `ids` only |
| R03 active chain | §1 |
| R04 zero price | the real zero rate gives `unitAmount: 0` with a real `pricingRateId` |
| R05 quantity 2 | quantity from the signed `d_`; total = unit × 2 |
| R06 typed values | `selected` from the snapshot row |
| R07 duplicate id | 422 for the whole call |
| R08 repeated call | same answer, no write |
| R09 Portion | `flightRefs: []` |
| R10–R12 | Ordering checks in §3 |
| R13, R14 | 409 for the whole call when any parent is not Active |
