# Phase 1 — End to end across AirOffer, Ancillary and Ordering

**Audience:** the Owner and the AirOffer and Ordering teams. Nothing here is built inside Ancillary.
**Authorities:** Ancillary's side — `Ancillary-Domain-Master.md` (R5.4) §2 and §7. Ordering's side — `Ordering-P1-Ancillary-Implementation-Spec.md`. This document only connects the two and states how they are checked together.
**Checked against source (heads of `k8s-stg` on 2026-10-02):** Ordering.Final `cd50a2a`, FlightFlow `d2180b2`, AirAvail/AirOffer `ac84040`.

## 1. What exists today and what does not

| Service | Today (verified in source) | Needed for Phase 1 |
|---|---|---|
| Ancillary | skeleton only | everything in `phases/Phase-1-Extra-Baggage.md` |
| AirOffer | No ancillary operation (only commented-out placeholders). It already holds every flight fact the quote needs. | one new operation, §3 |
| Ordering | Ticket issuance exists. No ancillary or baggage service, no EMD, no way to add a product to an existing order. Document stock already knows the EMD kind, and the EMD integration contracts already exist. | `Ordering-P1-Ancillary-Implementation-Spec.md` |
| FlightFlow | — | nothing |

Phase 1 sells on an **existing, ticketed order** only, and only a bag that covers **one flight** (Ancillary Master §7.2): its EMD-A has one coupon. A bag over several flights is deferred until Ordering models how an EMD carries a fee spanning several coupons. Showing ancillaries while shopping for flights, and accepting them when the order is created, is Phase 3; a commercial release that needs that is not complete with Phase 1.

## 2. Flow

```text
Backoffice UI ─► Ordering   GET  Orders/{id}/AncillaryOffers
                 Ordering ─► AirOffer ─► Ancillary   quote, no selections
Backoffice UI ◄─ catalogue items (each with productVersion, priceRuleId, price)

Backoffice UI ─► Ordering   POST Orders/{id}/Ancillaries          (AddAncillaryFromOffer)
                 Ordering ─► AirOffer ─► Ancillary   quote with selections
                 Ordering   records the sale from the priced items

Backoffice UI ─► Ordering   POST Orders/{id}/Ancillaries/Issuance (IssueAncillaryDocument)
                 Ordering   issues the EMD-A from what it stored — no call to AirOffer or Ancillary
```

No price comparison is needed. A selection names the product version and the price rule the customer saw; Ancillary refuses it (`16309`) if either has changed. Published versions and Active rules never change, so an accepted selection is exactly what was shown.

## 3. AirOffer — one new operation

`POST Service/v1/OrderAncillaryOffers`. It does not exist today and must be built.

Its request body and its answer are exactly Ancillary's quote request and answer (`Contracts/ancillary-quotes-v1.openapi.yaml`). AirOffer forwards the request to Ancillary's `POST Service/v1/AncillaryQuotes` over HTTP and returns the answer unchanged, including the HTTP status and the business error codes of Ancillary. It stores nothing and adds nothing.

## 4. Ordering

Everything Ordering builds — the context it sends, the fields it stores, the add command, the issue command, the EMD shape, the events — is in `Ordering-P1-Ancillary-Implementation-Spec.md`. The points where the two services meet:

| Ancillary gives | Ordering does |
|---|---|
| `productRef`, `productVersion` | stores them as the service definition reference and version; sends them back in a selection |
| `priceRuleId` | sends it back in a selection; keeps it as the reference of the pricing lines |
| `coveredFlightRefs` | exactly one flight in Phase 1: stores the covered air service; the EMD has one coupon, associated with that flight's ticket coupon |
| `document.type`, `rfic`, `rfisc` | stores all three with the service at sale time, so issuing never asks Ancillary again |
| `codes`, `terms`, `baggage` | stores them with the service (description codes are not stored) |
| `priceLines[]`, `total` | one pricing line per line, allocated to the service; `total` is the sale amount and the EMD's document total. On the EMD the coupon value is the `Ancillary` line only; `Tax` lines stay at document level |
| `inventory.control = Unlimited` | uses its own no-reservation provider; no reservation step |
| `existing[]` (input) | sends the order's Active ancillary services so quantity limits hold across purchases |
| `travellers[].flightRefs` (input) | sends, per traveller, only the segments on which he has an Active air service |

## 5. Manual run

Preconditions: Ancillary's `phase-1.http` passed (sub code `0CC` enabled; product `XBAG1` and its rule Active); AirOffer has the operation of §3; Ordering has built its spec; an EMD document stock exists.

| # | Step | What must be seen |
|---|---|---|
| 1 | Create, reserve, confirm and issue an order for one adult on one bound of one flight. | Order `Ticketed`; one ticket coupon, `Open` and `Local`. |
| 2 | List what can be added. | One item: `XBAG1`, total 38.50, covering the flight, `maxQuantity 1`, with its `productVersion` and `priceRuleId`. |
| 3 | Add that item (quantity 1) with a new `requestKey`. | One order item of kind `Baggage` and one baggage service of type `BaggageCharge`; two pricing lines (35.00 and 3.50) with reason `AddService`; order total +38.50; commercial version +1; status still `Ticketed`; reference, version, document type, RFIC `C`, sub code `0CC`, group `BG`, terms and baggage description stored; no reservation attempted; `OrderAncillaryAdded` published. |
| 4 | Send exactly the same request again (same `requestKey`). | The same result; no second service, no second change, no event. |
| 5 | List again; then try to add the same item with a new `requestKey`. | The list no longer offers `XBAG1` for that traveller and bound. The add is refused (`2904` carrying `16305`); the order is unchanged. |
| 6 | Issue the ancillary document. | One EMD-A: RFIC `C`; passenger = the passenger of the associated ticket (its profile revision); one coupon with RFISC `0CC`, associated with the ticket coupon; coupon value 35.00; document total 38.50; the 3.50 tax linked to the document, not to the coupon; one number taken from the EMD stock; `ElectronicMiscDocumentIssued` published. Ancillary and AirOffer were not called. |
| 7 | Issue again. | The same EMD is returned; nothing new is issued. |
| 8 | In Ancillary: retire the rule and activate a new one at 40.00; revise and rename the product and activate version 2. | The order, its pricing lines and its EMD are unchanged. |
| 9 | On a second ticketed order: list, then in Ancillary replace the rule, then add the item that was listed. | Refused (`2904` carrying `16309`); the order is unchanged. Listing again shows the new price and `priceRuleId`. |
| 10 | On a ticketed order whose bound has two flights, list. Then on an order with two travellers on such a bound, where one of them no longer has an active air service on the second flight, list. | First order: no bag is offered. Second order: the traveller who flies only the first flight is offered the bag for it; the traveller who flies both is offered nothing. |
| 11 | Stop Ancillary and list. | `2905`; the order is unchanged. |

If all eleven hold, the Phase-1 design is confirmed across the three services. If one fails, the contract is corrected before any further work.
