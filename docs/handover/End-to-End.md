# End to end across AirOffer, Ancillary and Ordering

**Audience:** the Owner and the AirOffer and Ordering teams.
**Authorities:** `reference/Ancillary-Domain-Master.md` for Ancillary; `handover/AirOffer/spec.md` for AirOffer; `handover/Ordering/spec.md` for Ordering; `reference/Ancillary-Edge-Contract.md` for what a seller sees. This document only connects them and states how they are checked together.
**Checked against source (heads of `k8s-stg` on 2026-10-03):** Ancillary `9bbd385`, Ordering.Final `cd50a2a`, FlightFlow `d2180b2`, AirAvail/AirOffer `ac84040`.

## 1. Who does what

| Service | Today | Needed |
|---|---|---|
| Ancillary | Phases 1 and 2 built and tested: catalogue, price rules, the reference evaluator. | Phase 2B: reservations, and the published read model with its stamp. |
| AirOffer | No ancillary operation. Reads other services' read models by SQL. | Read Ancillary's read model, evaluate, and offer `AncillaryOffers` and `AncillaryOffers/Details`. |
| Ordering | Ticket issuance and the reservation machinery exist. No ancillary service, no EMD. | List, add, reserve and confirm through the new `Ancillary` provider, issue the EMD-A. |
| FlightFlow | — | nothing |

Selling is on an **existing, ticketed order** only, and a bag only where it covers **one flight**. Selling while shopping for flights is Phase 3.

## 2. Flow

```text
Seller ─► Ordering   GET  Orders/{id}/ServiceList
          Ordering ─► AirOffer  POST Service/v1/AncillaryOffers            (order context)
                      AirOffer  reads its snapshot of Ancillary's read model, evaluates, mints offer items
Seller ◄─ service list: service definitions + offer items (opaque ids, prices, expiry)

Seller ─► Ordering   POST Orders/{id}/Services   (requestKey, offerRefId, selected offerItemRefIds + quantity)
          Ordering ─► AirOffer  POST Service/v1/AncillaryOffers/Details    (context + selection)
          Ordering   records the services and their pricing lines        — provider key: Ancillary

Seller ─► Ordering   POST Orders/{id}/Reservations/Services
          Ordering ─► Ancillary POST Service/v1/ServiceReservations         (context + units)
                      Ancillary checks every unit again with its own evaluator, then holds
Seller ─► Ordering   POST Orders/{id}/Reservations/Confirmations
          Ordering ─► Ancillary POST …/ServiceReservations/{id}/Confirmations

Seller ─► Ordering   POST Orders/{id}/Ancillaries/Issuance
          Ordering   issues the EMD-A for the confirmed services, from what it stored
```

Three checks protect the sale, each by the service that owns the fact:

| Check | Where |
|---|---|
| the offer item is genuine, not expired, still what the catalogue says | AirOffer, at Details |
| the accepted product version, price rule, applicability and quantity are still valid | Ancillary, at Reserve |
| the ticket coupon to associate with is open and locally controlled | Ordering, at issue |

## 3. Manual run

Preconditions: Ancillary Phase 2B done, with sub code `0CC`, product `XBAG1` and its rule (35.00 + VAT 3.50) Active; AirOffer and Ordering have built their specs; an EMD document stock exists.

| # | Step | What must be seen |
|---|---|---|
| 1 | Create, reserve, confirm and issue an order for one adult on one bound of one flight. | Order `Ticketed`; one ticket coupon, `Open` and `Local`. |
| 2 | Get the service list. | One service definition (`rfic C`, `rfisc 0CC`, "First extra bag 23kg") and one offer item: unit price 35.00 + tax 3.50 = 38.50, quantity rule 1–1, an opaque `offerItemId`, an expiry. |
| 3 | Add that item (quantity 1) with a new `requestKey`. | One order item of kind `Baggage`, one baggage service of type `BaggageCharge` with provider `Ancillary`; pricing lines 35.00 and 3.50 with reason `AddService`; order total +38.50; commercial version +1; status still `Ticketed`; `OrderAncillaryAdded` published. Ancillary was not called. |
| 4 | Send exactly the same request again. | The same result; nothing new. |
| 5 | Reserve the new service, then confirm. | At Ancillary: one reservation, unit `Held` then `Confirmed`, `total 38.50`. In Ordering: a reservation with provider `Ancillary`, unit `Confirmed`; status still `Ticketed`. |
| 6 | Get the service list again; try to add the old item with a new `requestKey`. | The list no longer offers the bag for that traveller and bound. The add is refused (`2904` carrying AirOffer's `6031`). |
| 7 | Issue the ancillary document. | One EMD-A: RFIC `C`; passenger = the passenger of the associated ticket; one coupon, RFISC `0CC`, value 35.00, associated with the ticket coupon; document total 38.50; the 3.50 tax linked to the document; `ElectronicMiscDocumentIssued` published. Neither AirOffer nor Ancillary was called. |
| 8 | Issue again. | The same EMD is returned. |
| 9 | On a second ticketed order: list and add the bag; then, in Ancillary, retire the rule and activate a new one at 40.00; then reserve. | Ancillary refuses the reservation (`16309`); the service stays on the order, unreserved; issuing finds nothing to issue. A new list shows 43.50 with a new offer item. |
| 10 | On a ticketed order whose bound has two flights, list. | No bag is offered. |
| 11 | Stop AirOffer and list; start it, stop Ancillary, and reserve an added service. | `2905` for the list; the reservation attempt stays unresolved and succeeds when Ancillary is back and the reserve is repeated. The order is consistent throughout. |

If all eleven hold, the design is confirmed across the three services. If one fails, the contract is corrected before any further work.
