# Ancillary — Edge Contract

**Date:** 2026-10-03
**What it is:** how ancillaries look to whoever sells them — sales channels through Ordering and AirOffer — and how that maps to the industry standards and to the two reference systems. The endpoints described here belong to AirOffer and Ordering; Ancillary itself has no channel API.
**Authority for the domain behind it:** `reference/Ancillary-Domain-Master.md`.

The shapes below follow the structure of IATA NDC (`ServiceList`, `OfferPrice`, `OrderChange`) and use NDC's element names in camelCase wherever an NDC element exists. Members that have no NDC counterpart are marked "AeroTech".

---

## 1. Mapping to the standards and to the reference systems

| Step | IATA NDC / ONE Order | Amadeus | Sabre | AeroTech | Phase |
|---|---|---|---|---|---|
| List the services that can be added to an order | `ServiceList` in the post-sale flow (by order) | `Service_IntegratedCatalogue` | `GetAncillaryOffers` | Ordering `GET …/Orders/{orderId}/ServiceList` → AirOffer `POST Service/v1/AncillaryOffers` | now |
| Price the chosen services | `OfferPrice` with the selected a-la-carte offer items | `Service_IntegratedPricing` | priced ancillary items | AirOffer `POST Service/v1/AncillaryOffers/Details` (called by Ordering) | now |
| Add them to the order | `OrderChange` accepting the selected offer items | add the service to the PNR (SSR) | add the ancillary item to the reservation | Ordering `POST …/Orders/{orderId}/Services` | now |
| The provider confirms the service | ONE Order: the delivery provider reports the service's status | SSR status confirmed (`HK`) | ancillary item confirmed | Ordering Reserve → Confirm at Ancillary (Master §8) | now |
| Issue the document | ticketless in ONE Order; EMD in today's world | pricing record + EMD issuance | EMD issuance | Ordering `IssueAncillaryDocument` (EMD-A / EMD-S) | now |
| List services while shopping | `ServiceList` in the prime booking flow (by offer) | `Service_StandaloneCatalogue` | `GetAncillaryOffers` | AirOffer, on its channel surfaces | P3 |
| Select them with the flights | `OfferPrice` / `OrderCreate` with a-la-carte items | priced with the itinerary | — | AirOffer price operation and `OfferId`; Ordering `CreateOrderFromOffer` | P3 |
| Choose a seat | `SeatAvailability` (a-la-carte offer item per seat) | seat map + chargeable seat | seat map + ancillary | AirOffer seat map with offer items | P5 |
| Cancel, refund | `OrderReshop`, `OrderCancel` | EMD void / refund | EMD void / refund | Ordering's later servicing stage | later |

The identity of a service for a seller is its RFIC and RFISC (the ATPCO sub code) and its name — never the internal product reference or any id.

---

## 2. Service list

Returned by AirOffer and passed on unchanged by Ordering.

```json
{
  "aLaCarteOffer": {
    "offerId": "A1~978~1790000000",
    "ownerAirlineId": 10,
    "offerExpirationDateTime": "2026-10-03T12:30:00+00:00",
    "currencyId": 978,
    "offerItems": [
      {
        "offerItemId": "A1~978~1790000000~XBAG1~1~501~7001~B~8001",
        "serviceDefinitionRefId": "SD1",
        "eligibility": { "paxRefIds": ["7001"], "paxJourneyRefId": "8001", "paxSegmentRefIds": ["8101"] },
        "quantityRule": { "minimumQty": 1, "maximumQty": 1 },
        "unitPrice": {
          "baseAmount": 35.00,
          "taxSummary": { "totalTaxAmount": 3.50, "taxes": [ { "taxCode": "VAT", "descText": "Value added tax", "amount": 3.50 } ] },
          "totalAmount": 38.50
        }
      }
    ]
  },
  "dataLists": {
    "serviceDefinitionList": [
      {
        "serviceDefinitionId": "SD1",
        "ownerAirlineId": 10,
        "name": "First extra bag 23kg",
        "descText": null,
        "rfic": "C",
        "rfisc": "0CC",
        "serviceType": "ExtraBaggage",
        "classification": { "serviceTypeCode": "C", "groupCode": "BG", "subGroupCode": null, "description1Code": "B1", "description2Code": null },
        "bookingInstructions": null,
        "documentType": "EmdAssociated",
        "unit": "Piece",
        "baggage": { "pieces": 1, "weight": 23, "weightUnit": "Kg" },
        "lounge": null,
        "terms": { "refundable": false, "commissionable": null, "reusable": null, "formOfRefundCode": null, "interlineSettlementAllowed": null }
      }
    ]
  }
}
```

| Member | NDC element | Meaning |
|---|---|---|
| `aLaCarteOffer` | `ALaCarteOffer` | The container of optional offer items. One per answer. |
| `offerId` | `OfferID` | Identifies this answer. Opaque. |
| `ownerAirlineId` | `OwnerCode` | The offering airline (AeroTech uses its numeric airline id). When items of several airlines are returned, the member is on each service definition and this one is null. |
| `offerExpirationDateTime` | `OfferExpirationDateTime` | After this instant the offer items can no longer be accepted; the seller lists again. |
| `currencyId` | (currency of the amounts) | AeroTech uses its numeric currency id. |
| `offerItems[]` | `ALaCarteOfferItem` | One per sellable occurrence: one service, for one traveller, on one journey or one flight. |
| `offerItemId` | `OfferItemID` | Opaque. The seller sends it back unchanged to select the item. It must not be parsed or built by a seller. |
| `serviceDefinitionRefId` | `Service/ServiceDefinitionRefID` | The service definition this item sells. |
| `eligibility.paxRefIds` | `Eligibility/PaxRefID` | The traveller. Always exactly one. |
| `eligibility.paxJourneyRefId` | `…/PaxJourneyRefID` | The journey (bound) the item belongs to. |
| `eligibility.paxSegmentRefIds` | `…/PaxSegmentRefID` | The flights the item covers. For a journey-scoped item: the covered flights of that journey; for a flight-scoped item: that flight. |
| `quantityRule` | `ServiceItemQuantityRules` | How many can still be bought for this occurrence. |
| `unitPrice.baseAmount` | `UnitPrice/BaseAmount` | The service price for one unit, without tax. |
| `unitPrice.taxSummary` | `UnitPrice/TaxSummary` | The taxes for one unit, with their codes. |
| `unitPrice.totalAmount` | `UnitPrice/TotalAmount` | Base plus taxes, for one unit. |
| `dataLists.serviceDefinitionList[]` | `DataLists/ServiceDefinitionList/ServiceDefinition` | Each distinct service once, referenced by the items. |
| `serviceDefinitionId` | `ServiceDefinitionID` | Unique within the answer only. |
| `name`, `descText` | `Name`, `Desc/DescText` | Commercial name and description. |
| `rfic`, `rfisc` | reason-for-issuance code and sub code | What the service is, in industry terms. |
| `bookingInstructions` | `BookingInstructions` | SSR code and method. Null in all current phases (Master §4.2). |
| `serviceType`, `classification`, `documentType`, `unit`, `baggage`, `lounge`, `terms` | AeroTech | The facts Ordering records with the sale (Master §7.4). |

Rules:

1. In the post-sale flow the references are the order's own: `paxRefIds` are traveller ids, `paxJourneyRefId` a journey id, `paxSegmentRefIds` segment ids.
2. Items are ordered as Master §7.4 orders them.
3. Nothing is listed that cannot be bought now: an occurrence that is not applicable is simply absent.
4. The list is an offer. It is binding only after the reservation at Ancillary succeeds; until then a later step may refuse an item that has changed.

---

## 3. Offer and offer-item identifiers

AirOffer mints them without storing anything, in the manner of its flight offer id.

```text
offerId      =  A1 ~ {currencyId} ~ {issuedAtUnixSeconds}
offerItemId  =  {offerId} ~ {productRef} ~ {productVersion} ~ {priceRuleId} ~ {paxRef} ~ {B|F} ~ {journeyRef|segmentRef}
```

- `B` = journey-scoped item (the reference is the journey); `F` = flight-scoped item (the reference is the segment).
- `offerExpirationDateTime` = `issuedAt` + the configured validity. An item is refused after it.
- The quantity is not part of the id; the seller states it when selecting.
- References must consist of letters and digits only (1–50), so that `~` can never appear inside one.
- For a seller the ids are opaque. Only AirOffer writes and reads them.

In Phase 3 the same item payload travels inside the flight offer id; that is specified with Phase 3.

---

## 4. Selecting items — adding services to an order

Sent by the seller to Ordering (`POST …/Orders/{orderId}/Services`), in the shape of NDC `OrderChange` accepting selected offer items:

```json
{
  "requestKey": "3f6c1c2e-5a0b-4d0e-9a55-7d0b8a1e2f10",
  "acceptSelectedOffer": {
    "offerRefId": "A1~978~1790000000",
    "selectedOfferItems": [
      { "offerItemRefId": "A1~978~1790000000~XBAG1~1~501~7001~B~8001", "quantity": 1 }
    ]
  }
}
```

| Member | NDC element | Rule |
|---|---|---|
| `requestKey` | AeroTech | The seller's identity of this attempt (idempotency). |
| `offerRefId` | `SelectedPricedOffer/OfferRefID` | The `offerId` of the list the items came from. Every item must belong to it. |
| `offerItemRefId` | `SelectedOfferItem/OfferItemRefID` | An `offerItemId` of that list, unchanged. |
| `quantity` | `SelectedALaCarteOfferItem/Qty` | Within the item's `quantityRule`. |

What follows is Ordering's (`handover/Ordering/spec.md`): it has AirOffer price the selected items, records the services, reserves and confirms them at Ancillary, and issues the document.

---

## 5. Pricing the selected items (AirOffer, called by Ordering)

`POST Service/v1/AncillaryOffers/Details` takes the order's context and the selection of §4 and returns, per selected item and in the same order, the full item of Master §7.4 (all the facts Ordering records) together with its `offerItemId` and the selected quantity. It is the counterpart of NDC `OfferPrice` for a-la-carte items and of AirOffer's own `FlightOffers/Details`. It is not a channel operation.

---

## 6. Service status as a seller sees it

Ordering stays the single record of the order (ONE Order). Target mapping of an ancillary service's state to ONE Order's delivery status, for the order view:

| State in AeroTech | Shown as |
|---|---|
| service added, not yet confirmed at Ancillary | (not yet entitled — no delivery status) |
| reservation confirmed and document issued | `READY TO PROCEED` |
| later, reported by the delivering system | `READY TO DELIVER`, `IN PROGRESS`, `DELIVERED`, `UNABLE TO DELIVER`, `NOT CLAIMED`, `EXPIRED` |
| service cancelled | `REMOVED` |

Only the first two rows exist in the current phases; the delivery-side statuses arrive with departure control and supplier integration.

---

## 7. What a seller never sees

Product references and versions, price-rule ids, reservation ids, stock figures beyond "can be bought", suppliers, and any rule. They are inside the opaque ids or inside the services.
