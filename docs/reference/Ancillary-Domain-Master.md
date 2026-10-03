# AeroTech.Ancillary — Domain and Boundary Master

**Revision:** R6, 2026-10-03. Changes are listed in `reference/CHANGELOG.md`.
**Status:** the implementation reference for the whole project. It is updated in place when a decision changes; there is never a second copy. Paths in this document are relative to the `docs/` folder.
**Checked against (2026-10-03):** Ancillary `9bbd385`, Ordering.Final `cd50a2a`, FlightFlow `d2180b2`, AirAvail/AirOffer `ac84040` (all heads of `k8s-stg`); IATA Airline Guide to EMD Implementation; ATPCO Optional Services reference manual, baggage migration guide and the industry sub-code list revised 30 March 2026; IATA NDC and ONE Order; Sabre and Amadeus ancillary/EMD material. The benchmark baseline is recorded in §13. A later phase does not redesign from zero, but every external contract and repository fact it relies on is verified again against current source before it is implemented.

## 0. What this document is

This document defines **what the Ancillary service is**: its domain model field by field, the behaviour of every operation, and exactly what crosses its boundary with AirOffer, Ordering, FlightFlow and the backoffice user.

It does not define how the code is organised, stored, tested or deployed. Those follow the repository's conventions (`CLAUDE.md`) and the platform's existing standards. Platform-wide concerns are out of its scope.

The model is complete for the whole horizon and is built in phases. Every item carries the phase that first builds it (**P1, P2, P2B, P4, P5, P6**). The phase plan and the state of each phase are in `phases/README.md`; each phase's folder says exactly which part is built in it.

Rules for whoever implements it:

1. Build only items of the active phase. An item of a later phase must not exist yet.
2. Do not add a domain type, field, enum member, operation, endpoint or error code that is not in this document.
3. Do not rename, re-type or remove anything an earlier phase built. Later phases only add.
4. If something needed is missing or contradictory, stop and ask. Do not choose a plausible value.

---

## 1. The service in one page

Ancillary is the airline's **catalogue of sellable extra services, the authority for their prices, the place where every ancillary sale is reserved and confirmed, and the keeper of limited stock**. Towards the rest of the platform it plays, for non-flight services, the part that FlightFlow and AirPricing play for flights.

| It owns | Aggregate | Phase |
|---|---|---|
| Which optional-service sub codes the airline uses, and what each means | `ServiceSubCode` | P1 |
| What an ancillary product is | `AncillaryProduct` | P1 |
| What it costs, under which conditions | `AncillaryPriceRule` | P1 |
| The answer to "which products apply here, at what price" | Quote (no state) | P1 |
| The reservation of every ancillary service sold on an order, and its confirmation, release and cancellation | `ServiceReservation` | P2B |
| How many units of a limited product are left on a flight | `StockPool` | P4 |
| The published read model that AirOffer reads | read models (§15) | P2B |

It never owns or creates:

- an **offer** (identity, validity, composition) — AirOffer is the only offer authority;
- an **order**, an order service, an accepted price history, an **EMD**, a ticket, a document number — Ordering;
- a flight, a seat map, a seat hold — FlightFlow;
- a payment;
- a currency conversion.

### Vocabulary

| Term | Meaning |
|---|---|
| Service sub code | The three-character code of an optional service (also the RFISC printed on an EMD coupon). An industry code has the meaning ATPCO publishes; a carrier-defined code has the meaning its airline gives it. An airline enables each code it uses once. |
| Product | A sellable ancillary definition of one airline, e.g. "extra checked bag 23 kg". |
| Product version | One immutable published state of a product. Sales reference `ProductRef + Version`. |
| Sales scope | What one purchase is attached to: a traveller on a bound, a traveller on a flight, … |
| Occurrence | One concrete thing that can be bought: product × traveller × bound (or flight). |
| Coverage | The flights an occurrence covers. For a bound-scoped occurrence: every flight of the bound. |
| Quantity | How many units of the product are bought in one occurrence. |
| Price rule | One priced condition set of a product. The best matching rule gives the price. |
| Price line | One amount of a rule: the ancillary price itself, or a tax. |
| Quote | The stateless computation of applicable occurrences and their prices for a given context. |
| Catalogue mode | A quote that lists everything applicable. |
| Selection mode | A quote that prices exactly the occurrences the customer chose. |
| Reservation | Ancillary's record that specific occurrences are held, and later confirmed, for one order. Every ancillary sale has one, whatever the product. |
| Stock pool | The counted stock of one limited product on one flight. |

---

## 2. Boundaries

### 2.1 Who talks to Ancillary

```text
Backoffice user ─► Ancillary                     registers sub codes; defines products, price rules, stock quotas
AirOffer        ─► Ancillary's published read model   reads catalogue and prices by SQL — no API call          (P2B)
Ordering        ─► Ancillary                     reserves / confirms / releases / cancels every ancillary service (P2B)
FlightFlow      ─► Ancillary                     "flight cancelled" fact                                         (P4)
```

Ancillary calls no other service. Sales channels never call Ancillary.

This is the same arrangement the platform already has for flights: AirOffer reads FlightFlow's and AirPricing's read models and builds the offer itself; Ordering calls FlightFlow to hold and confirm.

### 2.2 AirOffer ↔ Ancillary — reading, not calling

| | |
|---|---|
| What AirOffer reads | the published read model of §15: Active products with their copied codes, Active price rules and their lines |
| How | SQL against Ancillary's query database, kept as a cached snapshot that is refreshed when the stamp of §15 changes — the pattern AirOffer already uses for reference data and point-of-sale rules |
| What AirOffer does with it | builds the ancillary offer items itself, by the rules of §7, and gives each item an opaque offer-item id (`reference/Ancillary-Edge-Contract.md`) |
| What Ancillary guarantees | the read model's tables and columns change only by addition; every change of a product or rule moves the stamp |

§7 is therefore a specification with two implementations: Ancillary's own evaluator and AirOffer's. The behaviour rows of the phase documents are their common test; both must give the same answer for the same data and context.

A snapshot can be some seconds old. What AirOffer shows is an offer, not a commitment. The commitment is the reservation (§2.3), where Ancillary checks the accepted item again.

`POST Service/v1/AncillaryQuotes` remains in Ancillary as the reference implementation of §7, for tests and for backoffice preview. AirOffer does not call it.

There are two selling situations. AirOffer serves both from the same snapshot:

- **On an existing order** (P1, P2): Ordering sends the order's context to AirOffer.
- **While shopping for flights, before an order exists** (P3): AirOffer builds the context from its own flight offer.

### 2.3 Ordering ↔ Ancillary — one interface for every service

Every ancillary service of an order — whatever its product, its supplier or its inventory control — goes through the same steps at Ancillary (§8):

```text
Reserve  →  Confirm            the sale is committed
Reserve  →  Release            given up before confirmation (or left to expire)
Reserve  →  Confirm  →  Cancel   withdrawn after confirmation
```

Ordering uses its existing reservation machinery with one more provider, `Ancillary`, exactly as it uses FlightFlow for seats. It needs no knowledge of what happens behind the interface.

**Reserve is the authoritative check.** Ordering sends the accepted selections (product, version, price rule, traveller, bound or flight, quantity) with the order's context. Ancillary evaluates them by §7 at that moment — product still Active in that version, same price rule still selected, occurrence still applicable, quantity still allowed, and from P4 stock available — and only then creates the reservation. A selection that AirOffer offered from an old snapshot is refused here.

What crosses to Ordering, and what never does:

| Ordering stores (from AirOffer's item, confirmed by the reservation) | Stays inside Ancillary |
|---|---|
| product reference and version, name, codes, terms, detail, price lines, document instruction, the reservation reference | who supplies the service and on what terms; how rules are authored; how stock is counted; how a supplier is asked to deliver |

Documents stay with Ordering. Ancillary gives the **instruction as data** — `document` says which document to issue and with which codes — and Ordering issues it after the reservation is confirmed. The price lines keep their category all the way to the document: on the EMD, the coupon value is made of the `Ancillary` line only, `Tax` lines stay at document level, and the document total is the item's `total` (`handover/Ordering/spec.md`).

`inventory.control` tells where the stock is held:

| `inventory.control` | Reserved at |
|---|---|
| `Unlimited` | Ancillary (nothing is counted; the reservation is still made and checked) |
| `Quota` (P4) | Ancillary (counted) |
| `SeatMap` (P5) | FlightFlow holds the seat; Ancillary only prices it |

### 2.4 FlightFlow ↔ Ancillary

Ancillary never calls FlightFlow. Flight identity (`flightId`) is only a key supplied by the caller. From P4, Ancillary reacts to the fact "this flight is cancelled" by closing that flight's stock pools (the exact message is settled when P4 starts; see the Phase-4 document). Seats and their holds stay entirely in FlightFlow; from P5 Ancillary only prices a seat that the caller describes.

### 2.5 Sales channels

What a channel sees — the list of services, the offer items, how it selects them and how it adds them to an order — follows the IATA NDC and ONE Order structure and is specified in `reference/Ancillary-Edge-Contract.md`. Those endpoints belong to AirOffer and Ordering. Nothing in Ancillary's own API is a channel API.

---

## 3. Product taxonomy

A product is described by three **independent** properties. None is derived from another or from the product type.

### 3.1 Sales scope — `AncillarySalesScope`

| Member | Value | One occurrence is | Phase |
|---|---:|---|---|
| `TravellerBound` | 1 | one traveller on one bound, covering all its flights | P1 |
| `TravellerSegment` | 2 | one traveller on one flight | P2 |
| `Traveller` | 3 | one traveller, no flight | later |
| `Order` | 4 | the whole order | later |

### 3.2 Document type — `AncillaryDocumentType`

| Member | Value | Ordering must issue | Phase |
|---|---:|---|---|
| `EmdAssociated` | 2 | an EMD-A, associated with the ticket coupons of the covered flights | P1 |
| `EmdStandalone` | 3 | an EMD-S, not associated with a ticket coupon | P2 |
| `None` | 1 | nothing | P6 |

`EmdAssociated` is possible only for a scope that has flights.

### 3.3 Inventory control — `AncillaryInventoryControl`

| Member | Value | Meaning | Phase |
|---|---:|---|---|
| `Unlimited` | 1 | any number can be sold; nothing is reserved | P1 |
| `Quota` | 2 | a counted number per flight, kept by Ancillary Stock; a sale needs a hold | P4 |
| `SeatMap` | 3 | an identified seat kept by FlightFlow; Ancillary only prices it | P5 |

### 3.4 What is sold in each phase

| `Type` | `SalesScope` | `Document.Type` | `InventoryControl` | `Quantity.Unit` | Detail | Phase |
|---|---|---|---|---|---|---|
| `ExtraBaggage` | `TravellerBound` | `EmdAssociated` | `Unlimited` | `Piece` | `Baggage` | P1 |
| `LoungeAccess` | `TravellerSegment` | `EmdStandalone` | `Unlimited` | `Each` | `Lounge` | P2 |
| `PetInCabin` (the first limited product; the Owner may name another before P4 starts) | `TravellerSegment` | `EmdAssociated` | `Quota` | `Each` | — | P4 |
| `Seat` | `TravellerSegment` | `EmdAssociated` | `SeatMap` | `Each` | `Seat` | P5 |

A product whose combination is not a row of this table, for the phases already built, is rejected (`16105`).

The table says what is sold now. It is not a permanent property of a type: a later phase may add a row for an existing type. Current limitations: Extra Baggage is sold per bound, and in Phase 1 only where the traveller's part of that bound is exactly one flight (§7.2); a product is sold only on flights marketed by its own airline (no codeshare or interline selling).

---

## 4. Aggregate `AncillaryProduct`

One instance is **one version** of a product. All versions of a product share `OwnerAirlineId + ProductRef`.

### 4.1 Fields

| Field | Type | Null | Phase | Meaning and rules |
|---|---|---:|---|---|
| `Id` | `long` | No | P1 | Identity of this version. |
| `OwnerAirlineId` | `int` | No | P1 | The airline that sells the product. `> 0`. Never changes. |
| `ProductRef` | `string` | No | P1 | The product's business identity. 2–20 characters, `A–Z` and `0–9` only. Never changes. Never reused within the airline, even after the product is retired. |
| `Version` | `int` | No | P1 | 1 for a new product; each revision gets the highest existing version + 1. Never changes. |
| `Type` | `AncillaryProductType` | No | P1 | Business classification (§9). Fixed for all versions of the product. |
| `Name` | `string` | No | P1 | Commercial name shown to the customer and printed as the service name. 1–100 characters. |
| `Description` | `string?` | Yes | P1 | Free text, up to 500 characters. |
| `SalesScope` | `AncillarySalesScope` | No | P1 | §3.1. |
| `Quantity` | `QuantityPolicy` | No | P1 | How many units one occurrence may contain. |
| `Document` | `DocumentPolicy` | No | P1 | Which document Ordering issues, with which codes. Names the product's service sub code. |
| `Codes` | `IndustryCodes` | No | P1 | Industry classification, carried to Ordering unchanged. |
| `Terms` | `SalesTerms` | No | P1 | Sale conditions, carried to Ordering unchanged. |
| `InventoryControl` | `AncillaryInventoryControl` | No | P1 | §3.3. |
| `Baggage` | `BaggageDetail?` | Yes | P1 | Present exactly when `Type = ExtraBaggage`. |
| `Lounge` | `LoungeDetail?` | Yes | P2 | Present exactly when `Type = LoungeAccess`. |
| `DefaultQuotaPerFlight` | `int?` | Yes | P4 | Present (`≥ 0`) exactly when `InventoryControl = Quota`. |
| `Seat` | `SeatDetail?` | Yes | P5 | Present exactly when `Type = Seat`. |
| `Status` | `AncillaryProductStatus` | No | P1 | `Draft`, `Active`, `Suspended`, `Retired`. |
| `CreatedAt` | `DateTimeOffset` | No | P1 | When this version was created. |
| `ActivatedAt` | `DateTimeOffset?` | Yes | P1 | When this version first became `Active`. |
| `RetiredAt` | `DateTimeOffset?` | Yes | P1 | When this version became `Retired`. |

### 4.2 Value objects

**`QuantityPolicy`** — P1

| Member | Type | Rule |
|---|---|---|
| `Unit` | `AncillaryQuantityUnit` | What is counted. |
| `Min` | `int` | Smallest quantity of one purchase. `≥ 1`. |
| `Max` | `int` | Largest total quantity of one occurrence, counting what was already bought. `Min ≤ Max ≤ 99`. |

**`DocumentPolicy`** — P1

| Member | Type | Entered or copied | Rule |
|---|---|---|---|
| `Type` | `AncillaryDocumentType` | entered | §3.2. |
| `Rfisc` | `string?` | entered | The product's service sub code = the Reason For Issuance Sub Code of its EMD coupons. It must be the `Code` of an `Active` `ServiceSubCode` of the same airline (§4.5). |
| `Rfic` | `string?` | copied | Reason For Issuance Code. Copied from that `ServiceSubCode`; never typed on a product. |

When `Type` is an EMD type, `Rfisc` is required. When `Type = None` (P6), `Rfisc` and `Rfic` are empty.

**`IndustryCodes`** — P1

| Member | Type | Entered or copied | Rule |
|---|---|---|---|
| `ServiceTypeCode` | `string` | entered | Optional Services service type: one letter. Required. |
| `GroupCode` | `string` | copied | Optional Services group. |
| `SubGroupCode` | `string?` | copied | Optional Services sub-group. |
| `Description1Code` | `string?` | copied | Optional Services description 1. |
| `Description2Code` | `string?` | copied | Optional Services description 2. |

The copied members come from the `ServiceSubCode`.

**How the codes are governed.**

1. A product never states what a sub code means. It names an enabled `ServiceSubCode`; `Rfic`, `GroupCode`, `SubGroupCode`, `Description1Code` and `Description2Code` are copied from it into the product version whenever the version is defined or changed. The attributes of a sub code never change (§4.5), so every product with the same sub code carries the same classification.
2. **Classification by product type** — the sub code must fit the type:

| `Type` | `Rfic` | `GroupCode` | `ServiceTypeCode` | Phase |
|---|---|---|---|---|
| `ExtraBaggage` | `C` (baggage) | `BG` | `C` or `P` | P1 |
| `LoungeAccess` | `E` (airport services) | `LG` | any letter | P2 |
| `PetInCabin` | `C` (baggage) | `PT` | any letter | P4 |
| `Seat` | `A` (air transportation) | `SA` | any letter | P5 |

3. **Industry code constraints** — when the sub code is an industry code, the product must respect its reference entry (§4.5): `Document.Type` must be the entry's accepted document type, and where the entry says "exactly 1" the product's `Quantity` must be `Min = 1`, `Max = 1`. A code such as `0CC` names one specific item (the *first* excess bag); a second item is a different code and a different product.
4. **Carrier-defined baggage is not interline** — a product whose sub code is `CarrierDefined` and whose `Type` is `ExtraBaggage` must not have `Terms.InterlineSettlementAllowed = true`.
5. The sub code must be `Active` whenever a version is defined, changed or activated. Otherwise `16109`.

Rules 2, 3 and 4 are checked on Define, Change and Activate (`16106`).

**Order of the product checks.** On Define, Change and Activate the combination of §3.4 (`Type`, `SalesScope`, `Document.Type`, `InventoryControl`, `Quantity.Unit`) is checked first (`16105`). Only a product whose combination is a row of §3.4 is then checked against the other rules of §4 (`16106`, `16109`).

No SSR code exists on a product in the phases of this document. Booking-method, SSR and SVC semantics are added only when a real host or provider booking contract is introduced, together with their behaviour; until then nothing in this service stores or sends them.

**`SalesTerms`** — P1

| Member | Type | Meaning |
|---|---|---|
| `Refundable` | `bool` | Whether the sold service may be refunded. |
| `Commissionable` | `bool?` | Settlement term, if the airline defines it. |
| `Reusable` | `bool?` | Whether an unused service may be reused, if defined. |
| `FormOfRefundCode` | `string?` | Up to 10 characters, if defined. |
| `InterlineSettlementAllowed` | `bool?` | Settlement term, if defined. |

These are facts recorded at sale time. They are not a refund decision.

**`BaggageDetail`** — P1. What **one unit** of the product grants.

| Member | Type | Rule |
|---|---|---|
| `Pieces` | `int?` | `≥ 1`. |
| `Weight` | `decimal?` | `> 0`. |
| `WeightUnit` | `AncillaryWeightUnit?` | Required exactly when `Weight` is present. |

At least one of `Pieces` and `Weight` must be present.

**`LoungeDetail`** — P2: `AirportIds` — the airports where the lounge is offered; at least one, no duplicates. The product applies to a flight that departs from one of them.

**`SeatDetail`** — P5: `SeatCharacteristicCodes` — the seat characteristics this product prices; at least one, no duplicates. The codes are the seat-characteristic codes of the seat map that FlightFlow serves; the caller sends a seat's codes with the seat (§7.8).

### 4.3 Lifecycle

```text
          Activate                Suspend
Draft ───────────────► Active ───────────────► Suspended
  │                      ▲ │                       │
  │                      │ └───────────────────────┘ Activate
  │ Retire               │ Retire          Retire  │
  └────────────────► Retired ◄─────────────────────┘
```

| Operation | Allowed when | What happens | Errors |
|---|---|---|---|
| **Define** | no version of `OwnerAirlineId + ProductRef` exists | A new product: `Version = 1`, `Draft`, `CreatedAt = now`. The codes are copied from the named sub code. All field rules are checked. | `16102` reference already used; `16105`; `16106`; `16109` |
| **Change** | `Draft` | Every field except `Id`, `OwnerAirlineId`, `ProductRef`, `Version`, `Type`, `Status` and the timestamps is replaced by the new values. The codes are copied again from the named sub code. All field rules are checked. | `16103` not a Draft; `16105`; `16106`; `16109` |
| **Activate** | `Draft` or `Suspended` | Status becomes `Active`. `ActivatedAt = now` if it was empty. All field rules are checked again. The sub code must be `Active` (governance rule 5). If the version was a `Draft` and another version of the same product is `Active` or `Suspended`, that other version becomes `Retired` (`RetiredAt = now`) at the same moment: the two changes succeed or fail together. | `16104`; `16105`; `16106`; `16109` |
| **Suspend** | `Active` | Status becomes `Suspended`. The product is no longer offered; its price rules and stock are untouched. | `16104` |
| **Retire** | `Draft`, `Active`, `Suspended` | Status becomes `Retired`, `RetiredAt = now`. Final. | `16104` |
| **Revise** | `Active` or `Suspended` | A new `Draft` version is created: same `OwnerAirlineId`, `ProductRef`, `Type`; `Version` = highest existing + 1; every other field copied; `CreatedAt = now`. The source version is unchanged. | `16107` a Draft of this product already exists; `16108` source is `Draft` or `Retired` |

Any operation on an unknown id is `16101`.

### 4.4 Invariants

1. For one `OwnerAirlineId + ProductRef`: at most one version is `Active` or `Suspended`; at most one is `Draft`; no two versions have the same `Version`.
2. A version that has left `Draft` never changes again, except `Status`, `ActivatedAt` and `RetiredAt`.
3. No version is ever deleted. Every version stays readable, because sales refer to it.
4. Only an `Active` version is offered by the quote and usable for stock. Retiring a sub code does not change or withdraw a version that is already `Active`; it only blocks defining, changing and activating versions until the sub code is reactivated.
5. Two requests that try to activate the same Draft, or to revise the same product, at the same time: exactly one succeeds.

### 4.5 Aggregate `ServiceSubCode`

The sub codes an airline has enabled. It is the single place where a sub code's attributes are held; products only refer to it.

Industry practice it follows (ATPCO Optional Services): a sub code is three alphanumeric characters and is one of two kinds.

- **Industry-defined** — maintained by ATPCO, always beginning with a digit, with standard attributes that are the same for every airline. An airline does not define or alter them.
- **Carrier-defined** — defined by one airline and valid only for it.

Codes beginning with `98` or `99` are reserved for fees outside optional services and are not accepted.

#### Industry reference

The service ships a read-only reference of the industry codes it supports. No operation creates, changes or removes its entries; an entry is added only by a new revision of this document.

What the reference holds, and on whose authority:

- **Classification** — `GroupCode`, `SubGroupCode`, `Description1Code`, `Description2Code`, `CommercialName`. These are ATPCO's reference attributes of the industry code and are the same for every airline. Nobody in AeroTech defines or alters them.
- **Accepted RFIC and document type** — the RFIC and EMD type with which AeroTech sells the code. ATPCO's list gives a *recommended* RFIC for each industry code, and ATPCO's service record lets a carrier set EMD type and RFIC; so these two values are an AeroTech product constraint, chosen from that source data, not a claim that ATPCO fixes them for every carrier. In this service they are fixed per entry.
- **Quantity per occurrence** — an AeroTech constraint that follows from what the code means.

| `Code` | `GroupCode` | `SubGroupCode` | `Description1Code` | `Description2Code` | `CommercialName` | Accepted `Rfic` | Accepted document type | Quantity per occurrence | Phase |
|---|---|---|---|---|---|---|---|---|---|
| `0CC` | `BG` | — | `B1` (First Bag) | — | `FIRST EXCESS BAG` | `C` | `EmdAssociated` | exactly 1 | P1 |
| `0BX` | `LG` | — | — | — | `LOUNGE ACCESS` | `E` | `EmdStandalone` | exactly 1 | P2 |
| `0BT` | `PT` | `PC` (In Cabin) | — | — | `PET IN CABIN` | `C` | `EmdAssociated` | exactly 1 | P4 |
| `0B5` | `SA` | — | — | — | `PRE RESERVED SEAT ASSIGNMENT` | `A` | `EmdAssociated` | exactly 1 | P5 |

An entry exists in the service only from its phase. Source of every row: ATPCO, *Optional Services Industry Sub Codes*, revised 30 March 2026 (entries 203, 563, 668, 686). `0CD` (second bag) and the following bag codes are separate industry codes and are not supported; no "first, second, third bag" logic exists in any phase.

#### Fields

| Field | Type | Null | Phase | Meaning and rules |
|---|---|---:|---|---|
| `Id` | `long` | No | P1 | Identity. |
| `OwnerAirlineId` | `int` | No | P1 | The airline that enables the code. `> 0`. |
| `Code` | `string` | No | P1 | Three characters, `A–Z` and `0–9`. Must not begin with `98` or `99`. |
| `Source` | `ServiceSubCodeSource` | No | P1 | Not entered: `Industry` when `Code` begins with a digit, `CarrierDefined` when it begins with a letter. |
| `Rfic` | `string` | No | P1 | Reason For Issuance Code: one letter `A–Z`. For an industry code: the accepted RFIC of its reference entry. For a carrier-defined code: the airline's own choice. |
| `GroupCode` | `string` | No | P1 | Two characters `A–Z`, `0–9`. |
| `SubGroupCode` | `string?` | Yes | P1 | Two characters. |
| `Description1Code` | `string?` | Yes | P1 | Two characters. |
| `Description2Code` | `string?` | Yes | P1 | Two characters. |
| `CommercialName` | `string` | No | P1 | 1–30 characters: letters, digits and spaces. |
| `Status` | `ServiceSubCodeStatus` | No | P1 | `Active`, `Retired`. |
| `CreatedAt` | `DateTimeOffset` | No | P1 | |

No field except `Status` ever changes after registration.

#### Operations

| Operation | Allowed when | What happens | Errors |
|---|---|---|---|
| **Register** | the airline has no sub code with this `Code`, in any status | A new sub code, `Active`. **Industry code:** the request carries only the airline and the code; every attribute is taken from the industry reference. A request that also carries an attribute is refused, and a code that is not in the reference is refused. **Carrier-defined code:** the request carries `Rfic`, `GroupCode`, `CommercialName` and optionally the sub-group and description codes. | `16111` already registered; `16112` invalid; `16114` industry code not in the reference |
| **Retire** | `Active` | Status becomes `Retired`. Products that already carry its values are not touched; no product can be defined, changed or activated with it while it is retired. | `16113` |
| **Reactivate** | `Retired` | Status becomes `Active` again, with the same attributes. | `16113` |

Any operation on an unknown id is `16110`.

Register checks, in this order, and stops at the first failure: the code's form and the `98`/`99` rule (`16112`); for an industry code, no attribute supplied (`16112`); for an industry code, the code is in the reference (`16114`); for a carrier-defined code, its attributes are valid (`16112`); the airline does not already have the code (`16111`).

#### Invariants

1. Per airline there is at most one sub code per `Code`, ever. A retired code is brought back only by Reactivate, never by a second registration, so a code can never acquire a different meaning.
2. The attributes of an industry code are always exactly those of the industry reference. A carrier-defined code is local to its airline and never acquires industry meaning, whatever its name says.
3. No sub code is ever deleted.

### 4.6 Example

The airline enables the industry sub code `0CC` by sending only the code; the service fills in group `BG`, description `B1`, name `FIRST EXCESS BAG` and RFIC `C` from its reference. It defines `XBAG1` (v1, Draft, quantity 1–1) naming `0CC`; the product version now carries `Rfic C`, `GroupCode BG`, `Description1Code B1`. It activates the product and sells it for a month. It then wants to rename it. It revises v1 → v2 (Draft), changes the name, activates v2. At that moment v1 becomes `Retired` and v2 is the offered version. Orders sold earlier still show `XBAG1` version 1, and v1 can still be read.

---

## 5. Aggregate `AncillaryPriceRule`

A rule prices one product. It refers to the product by `OwnerAirlineId + ProductRef`, so it applies to every version of that product.

### 5.1 Fields

| Field | Type | Null | Phase | Meaning and rules |
|---|---|---:|---|---|
| `Id` | `long` | No | P1 | Identity. |
| `OwnerAirlineId` | `int` | No | P1 | `> 0`. Never changes. |
| `ProductRef` | `string` | No | P1 | The priced product. Never changes. |
| `Priority` | `int` | No | P1 | `≥ 1`. When several rules match, the lowest number wins. |
| `CurrencyId` | `int` | No | P1 | `> 0`. The currency of every line. |
| `Lines` | list of `PriceLine` | No | P1 | The amounts. See below. |
| `SalesFrom` | `DateTimeOffset?` | Yes | P1 | The rule applies to sales at or after this instant. |
| `SalesTo` | `DateTimeOffset?` | Yes | P1 | The rule applies to sales before this instant. If both are set, `SalesFrom < SalesTo`. |
| `TravelFrom` | `DateOnly?` | Yes | P1 | The rule applies to travel on or after this date. |
| `TravelTo` | `DateOnly?` | Yes | P1 | The rule applies to travel on or before this date. If both are set, `TravelFrom ≤ TravelTo`. |
| `Conditions` | `PriceRuleConditions` | No | P1 | Further restrictions. See below. |
| `Status` | `AncillaryPriceRuleStatus` | No | P1 | `Draft`, `Active`, `Suspended`, `Retired`. |
| `CreatedAt` | `DateTimeOffset` | No | P1 | |

**`PriceLine`**

| Member | Type | Rule |
|---|---|---|
| `Category` | `AncillaryPriceLineCategory` | `Ancillary` = the price of the service itself; `Tax` = a tax shown separately; `Fee` (P6). |
| `Code` | `string?` | Up to 10 characters. Required for `Tax` (and `Fee`). |
| `Name` | `string?` | Up to 100 characters. |
| `Amount` | `decimal` | A fixed amount for **one unit** in one occurrence, in the rule's currency. `> 0`. It uses exactly the money representation the platform already uses for prices (the one Ordering's pricing lines use — two fractional digits at the Ordering commit this document was checked against); an amount that this representation cannot hold exactly is refused (`16206`), never rounded. A currency that needs more fractional digits is therefore not supported; changing that is a platform decision outside this document. |

Line rules: exactly one `Ancillary` line; any number of `Tax` lines, each with a different `Code`. (P6: `Fee` lines; an `Ancillary` amount of 0 becomes possible.)

What the amounts mean: there are no percentages and no formulas. The sum of all lines is everything the customer pays for one unit. No other service adds a tax to an ancillary. If a tax must be visible separately it is a `Tax` line; otherwise the `Ancillary` line alone is the full price.

**`PriceRuleConditions`** — each member is optional. An absent member means "no restriction". A present member must contain at least one value and no duplicates.

| Member | Type | Phase | The rule matches when |
|---|---|---|---|
| `PassengerTypes` | list of `PassengerTypeCode` | P1 | the traveller's passenger type is in the list |
| `OriginAirportIds` | list of `int` | P1 | the origin airport of the **first** covered flight is in the list |
| `DestinationAirportIds` | list of `int` | P1 | the destination airport of the **last** covered flight is in the list |
| `CabinClassIds` | list of `int` | P6 | every covered flight's cabin is in the list |
| `RbdIds` | list of `long` | P6 | every covered flight's booking class is in the list |
| `AircraftIds` | list of `int` | P6 | every covered flight's aircraft is in the list |
| `Channels` | list of `string` | P6 | `salesContext.channel` is in the list. The values are the names of the platform's sales-channel vocabulary as Ordering sends them: `BackOffice`, `IBE`, `PartnerAPI`, `AgencyPanel`, `GDS`, `System`. |
| `TravelAgencyIds` | list of `long` | P6 | the selling agency is in the list |
| `PointOfSaleCountryIds` | list of `int` | P6 | the country of sale is in the list |

`PassengerTypeCode` is the platform's existing passenger-type vocabulary (`ADT`, `CHD`, `INF`, …).

### 5.2 Lifecycle

| Operation | Allowed when | What happens | Errors |
|---|---|---|---|
| **Define** | a product with this `OwnerAirlineId + ProductRef` exists (any version, any status) | A new rule in `Draft`. All field rules are checked. | `16205` product not found; `16206` |
| **Change** | `Draft` | Every field except `Id`, `OwnerAirlineId`, `ProductRef`, `Status`, `CreatedAt` is replaced. | `16202`; `16206` |
| **Activate** | `Draft` or `Suspended` | Status becomes `Active`, provided no other `Active` rule has the same `OwnerAirlineId + ProductRef + CurrencyId + Priority`. | `16203`; `16204` priority already taken |
| **Suspend** | `Active` | Status becomes `Suspended`; the rule is no longer used. | `16203` |
| **Retire** | `Draft`, `Active`, `Suspended` | Status becomes `Retired`. Final. | `16203` |

Any operation on an unknown id is `16201`.

### 5.3 Invariants

1. A rule that has left `Draft` never changes again, except its `Status`. A new price is a new rule.
2. Among `Active` rules of one product and currency, `Priority` is unique.
3. No rule is ever deleted.

### 5.4 Example

`XBAGG` has two Active rules in EUR. Rule A: priority 1, 20.00, `PassengerTypes = [CHD]`. Rule B: priority 2, 35.00 plus a `Tax` line `VAT` 3.50, no conditions. A child pays 20.00 (rule A matches and has the lower number). An adult pays 38.50 (rule A does not match; rule B is used). To raise the adult price, the airline defines rule C with priority 2 at 40.00, suspends or retires rule B, then activates rule C.

---

## 6. Enum-like values on the boundary

Every enumerated value crosses the boundary as its **name** in a plain text field. A receiver must accept names it does not know yet. A request that names a value which does not exist in the built phases is refused as malformed (HTTP 400).

---

## 7. Quote

`POST Service/v1/AncillaryQuotes`

This section defines how applicable occurrences and their prices are worked out. It is implemented twice: by Ancillary (the quote operation below, and the check made by Reserve, §8) and by AirOffer over the published read model (§2.2, §15). The request and item shapes below are Ancillary's; AirOffer's channel-facing shapes are in `reference/Ancillary-Edge-Contract.md`.

The quote reads products and price rules (and, from P4, stock counters). It changes nothing, stores nothing, publishes nothing and calls nobody. The same request against the same data always gives the same answer.

### 7.1 Request

| Field | Type | Null | Phase | Meaning |
|---|---|---:|---|---|
| `currencyId` | `int` | No | P1 | The currency the prices must be in. Ancillary does not convert: only rules in this currency are used. |
| `asOf` | `DateTimeOffset` | No | P1 | The instant the sale is priced at. Sales windows are compared with this value, never with Ancillary's own clock. |
| `salesContext` | object | Yes | P1 | `channel` (`string?`), `customerId` (`long?`), `travelAgencyId` (`long?`), `countryId` (`int?`). Accepted from P1; used by conditions from P6. |
| `travellers[]` | list | No | P1 | At least one. Each: `ref` (caller's key, unique), `passengerTypeCode` (a passenger-type name), `flightRefs[]` (the flights this traveller actually travels on; at least one; each must be a flight of the request). |
| `bounds[]` | list | No | P1 | At least one. Each: `ref` (caller's key, unique), `flights[]`. |
| `bounds[].flights[]` | list | No | P1 | At least one per bound, in travel order. Each flight: see below. |
| `existing[]` | list | Yes | P1 | What the same customer order already contains. Each: `productRef`, `travellerRef`, `boundRef?`, `flightRef?`, `quantity ≥ 1`. |
| `selections[]` | list | Yes | P1 | Absent or empty = catalogue mode. Each: `productRef`, `productVersion`, `priceRuleId`, `travellerRef`, `boundRef?`, `flightRef?`, `quantity ≥ 1`. `productVersion` and `priceRuleId` are the values of the catalogue item the customer chose. From P5 a seat selection also carries `seatNumber`. |
| `seats[]` | list | Yes | P5 | Seats to be priced: `flightRef`, `seatNumber`, `characteristicCodes[]` (§7.8). |

A flight:

| Field | Type | Null | Used in P1 for |
|---|---|---:|---|
| `ref` | `string` | No | Caller's key; unique in the whole request. |
| `flightId` | `long` | No | Identity of the flight (stock key from P4). |
| `flightCapacityId` | `long?` | Yes | — |
| `originAirportId` | `int` | No | Origin conditions. |
| `destinationAirportId` | `int` | No | Destination conditions. |
| `departureDateTime` | `DateTimeOffset` | No | Travel window. Its calendar date is taken in the offset it is given in (local departure date). |
| `marketingAirlineId` | `int` | No | Which airline's products apply. |
| `operatingAirlineId` | `int` | No | — |
| `aircraftId` | `int?` | Yes | — (P6 conditions) |
| `cabinClassId` | `int?` | Yes | — (P6 conditions) |
| `rbdId` | `long?` | Yes | — (P6 conditions) |

All `ref` values are the caller's own keys, 1–50 characters. Ancillary only echoes them back.

A request is refused with `16301` when a traveller `ref` or bound `ref` is repeated, a flight `ref` is repeated anywhere in the request, a `passengerTypeCode` is not a known passenger type, or a traveller's `flightRefs` names a flight that is not in the request.

`flightRefs` exists because travellers of one order do not always fly the same flights (for example after a partial cancellation). When an order is the source, it lists the flights on which that traveller has an active air service. When a flight offer is the source, it lists every flight for every traveller.

### 7.2 Occurrences and applicability

For every `Active` product, the candidate occurrences are:

- `TravellerBound`: each traveller × each bound in which the traveller has at least one flight. Coverage = the traveller's flights of that bound, in travel order.
- `TravellerSegment` (P2): each traveller × each of the traveller's flights. Coverage = that flight.

A traveller is never offered anything on a flight that is not in his `flightRefs`.

**Identity of an occurrence.** A bound-scoped occurrence is identified by product + traveller + bound; a flight-scoped occurrence by product + traveller + flight. For a flight-scoped occurrence the bound plays no part in its identity: the same flight named with and without its `boundRef` is the same occurrence.

A candidate is **applicable** when all of the following hold:

1. **Airline.** Every covered flight is marketed by the product's `OwnerAirlineId`.
2. **Type-specific rule.** Extra Baggage: the occurrence covers **exactly one flight** (see below). P2 Lounge: the flight departs from one of `Lounge.AirportIds`.
3. **Price.** A price rule is selected (§7.3).
4. **Quantity left.** `remaining = Quantity.Max − (sum of quantity of the existing entries of this same occurrence)` is at least `Quantity.Min`.
5. **Stock left** (P4, `Quota` products): the flight's remaining stock is at least `Quantity.Min`.

**Why Extra Baggage is limited to one covered flight.** An extra bag needs an EMD-A with one coupon per covered flight. For a single fee that spans several coupons, the industry does not put a value on each coupon; it carries the amount in the document's fare calculation and leaves the split to revenue-accounting proration. Ordering's EMD model has neither yet. So, until Ordering models that valuation, a bag is offered only where the traveller flies exactly one flight of the bound: in catalogue mode a wider occurrence is simply absent, and a selection of it is `16305`. The product's scope stays `TravellerBound`; lifting this limitation is a later change to this rule only (§12).

An `existing` entry belongs to the occurrence with the same product, traveller and bound (bound-scoped) or flight (segment-scoped). An entry that names no flight cannot belong to a flight-scoped occurrence and has no effect on it. An entry whose `travellerRef`, `boundRef` or `flightRef` is not in the request is an error (`16304`). An entry naming a product that is not `Active` is ignored.

### 7.3 Choosing the price rule

From the `Active` rules of the product (`OwnerAirlineId + ProductRef`), keep those for which **all** are true:

1. `CurrencyId` equals the request's `currencyId`.
2. `SalesFrom ≤ asOf` if `SalesFrom` is set; `asOf < SalesTo` if `SalesTo` is set.
3. The local departure date of the **first** covered flight is `≥ TravelFrom` if set and `≤ TravelTo` if set.
4. Every present condition matches (§5.1).

The remaining rule with the lowest `Priority` is the selected rule. If none remains, the occurrence is not applicable.

### 7.4 Result item

One item per applicable occurrence.

| Field | Type | Null | Phase | Value | Ordering uses it for |
|---|---|---:|---|---|---|
| `ownerAirlineId` | `int` | No | P1 | The product's airline. | Responsible airline of the service. |
| `productRef` | `string` | No | P1 | | Service definition reference. |
| `productVersion` | `int` | No | P1 | The `Active` version. | Stored with the reference; sent back in a selection. |
| `type` | name | No | P1 | `AncillaryProductType`. | Kind of order service. |
| `name` | `string` | No | P1 | Product name. | Commercial name. |
| `description` | `string?` | Yes | P1 | | Display. |
| `salesScope` | name | No | P1 | | — |
| `travellerRef` | `string` | No | P1 | | Beneficiary traveller. |
| `boundRef` | `string` | No | P1 | The occurrence's bound; for a flight-scoped item, the bound that contains the flight. | — |
| `flightRef` | `string?` | Yes | P1 | The flight of a flight-scoped item; null for a bound-scoped item. | — |
| `coveredFlightRefs` | list of `string` | No | P1 | The covered flights in travel order. Always a list. | Air services covered; one EMD-A coupon per covered flight, each associated with that flight's ticket coupon. |
| `unit` | name | No | P1 | `Quantity.Unit`. | Quantity unit. |
| `minQuantity` | `int` | No | P1 | `Quantity.Min`. | Display. |
| `maxQuantity` | `int` | No | P1 | `remaining` (§7.2 rule 4); from P4 not more than the remaining stock. | Display. |
| `quantity` | `int` | No | P1 | Catalogue mode: `minQuantity`. Selection mode: the selected quantity. | Purchased quantity. |
| `codes` | object | No | P1 | `serviceTypeCode`, `groupCode`, `subGroupCode`, `description1Code`, `description2Code`. | Service type, group, sub-group. |
| `baggage` | object | Yes | P1 | `pieces`, `weight`, `weightUnit` of one unit. | Baggage description. |
| `lounge` | object | Yes | P2 | `airportIds`. | Display. |
| `seat` | object | Yes | P5 | `number`, `characteristicCodes`. | Seat reference. |
| `terms` | object | No | P1 | The five `SalesTerms` members. | Refundability and settlement terms. |
| `document` | object | No | P1 | `type`, `rfic`, `rfisc`. | Stored with the service at sale time, so the document can be issued later without asking Ancillary: which document to issue, its RFIC, and the RFISC (= service sub code) of its coupons. |
| `inventory` | object | No | P1 | `control` (name); `remaining` (`int?`, only for `Quota`). | Whether and where to reserve before issuing (§2.3). |
| `priceRuleId` | `long` | No | P1 | The selected rule. | Source reference on its pricing lines; sent back in a selection. |
| `priceLines[]` | list | No | P1 | The `Ancillary` line first, then the other lines in the rule's order. Each: `category`, `code`, `name`, `unitAmount`, `amount`. | Its pricing lines. |
| `unitTotal` | `decimal` | No | P1 | Sum of `unitAmount`. | Display. |
| `total` | `decimal` | No | P1 | Sum of `amount`: what the customer pays for this occurrence. | Amount of the sale and of the document. |

Price line values: `unitAmount` = the rule line's `Amount`; `amount = unitAmount × quantity`; `name` = the line's `Name`, and for the `Ancillary` line the product `Name` when the line has none. Nothing is rounded.

The response contains `currencyId`, `asOf` and `items[]`.

**Order of items:** by traveller in request order; within a traveller, by bound in request order; within a bound, the bound-scoped items first, then the flight-scoped items in flight order; items of the same occurrence position by `productRef`.

### 7.5 Catalogue mode

No `selections`. Every applicable occurrence is returned, priced at its minimum quantity. Occurrences that are not applicable are simply absent. An empty list is a normal answer.

### 7.6 Selection mode

With `selections`. Exactly the selected occurrences are returned, in selection order, each priced at its selected quantity. The answer is all-or-nothing: selections are checked one after another in request order, each with the checks below in this order, and the first failure fails the whole request.

| # | Check | Error |
|---:|---|---|
| 1 | `travellerRef`, and `boundRef` / `flightRef` when given, exist in the request. | `16304` |
| 2 | Some `Active` product has this `productRef`. | `16302` |
| 3 | The references fit the product's scope. Bound-scoped: `boundRef` given, `flightRef` absent. Flight-scoped: `flightRef` given; `boundRef` absent or the bound containing that flight. | `16303` |
| 4 | No earlier selection in this request names the same occurrence (identity as in §7.2: product + traveller + bound, or product + traveller + flight). | `16307` |
| 5 | The occurrence is applicable by §7.2 rules 1–4. | `16305` |
| 6 | The selection is still what the customer saw: `productVersion` is the `Active` version and `priceRuleId` is the rule selected now. | `16309` |
| 7 | `quantity` is between `Quantity.Min` and `remaining`. | `16306` |
| 8 | (P4, `Quota`) the remaining stock covers `quantity`. | `16308` |

Check 6 pins a selection to exactly the catalogue item that was shown. A published product version and an Active price rule never change, so equal `productVersion` and `priceRuleId` guarantee the same description, the same codes and the same price. If either has changed, the caller asks for the catalogue again and shows the customer the new item.

A flight-scoped selection naming a flight that the traveller does not fly (not in his `flightRefs`) fails check 5.

For checks 3 and 5 the product is the `Active` product with this `productRef` owned by the marketing airline of the first flight the selection refers to (the `flightRef` flight, or the traveller's first flight in `boundRef`). If the traveller has no flight in `boundRef`, or no `Active` product with this `productRef` belongs to that airline, check 5 fails.

### 7.7 Worked example

Data: product `XBAGG` v1, airline 10, a generic extra bag on the carrier-defined sub code `XBG`, `TravellerBound`, quantity `Piece` 1–2, Active. Rule 501: priority 1, currency 978, lines `Ancillary` 35.00 and `Tax VAT` 3.50, Active.

Request: currency 978; bound `B1` with one flight `F1` (airport 1 → 2) marketed by airline 10; traveller `T1` (ADT, `flightRefs [F1]`); no `existing`.

- Catalogue mode → one item: `T1`, `B1`, `coveredFlightRefs [F1]`, `productVersion 1`, `priceRuleId 501`, `quantity 1`, `maxQuantity 2`, lines 35.00 and 3.50, `total 38.50`.
- Selection `XBAGG / version 1 / rule 501 / T1 / B1 / quantity 2` → the same item with `quantity 2`, line amounts 70.00 and 7.00, `total 77.00`.
- The same selection after the airline replaced rule 501 by rule 502 (40.00) → `16309`. A new catalogue shows rule 502.
- The same selection with `existing = [XBAGG / T1 / B1 / quantity 1]` → `16306`, because only 1 remains. With `quantity 1` → priced, `maxQuantity 1`.
- With `existing` quantity 2 → catalogue mode returns nothing for `T1/B1`; a selection gives `16305`.
- If `F1` is marketed by another airline → nothing for `T1/B1`.
- Request currency 840 → nothing (no rule in that currency).

A bound `B2` with two flights `F2`, `F3`:

- `T1` with `flightRefs [F2, F3]` → nothing for `T1/B2` in Phase 1 (two covered flights); a selection gives `16305`.
- `T2` with `flightRefs [F3]` only → one item for `T2/B2` with `coveredFlightRefs [F3]`; origin and destination conditions look at `F3`.

### 7.8 Seat pricing (P5)

Ancillary stores no seat, no seat availability and no seat hold, and never calls FlightFlow. It prices a seat only when the caller describes it.

1. The caller sends the candidate seats in `seats[]`: the flight, the seat number and the seat's characteristic codes, taken from FlightFlow's seat map.
2. A `Seat` product **matches** a seat when every code of the product's `SeatCharacteristicCodes` is among the seat's codes. If several Active products match, the one with the most codes wins; if still tied, the lowest `ProductRef`. A seat that no product matches is free: it produces no item.
3. For each traveller who flies the seat's flight, the matching product gives one flight-scoped occurrence for that seat, priced by the ordinary rule selection of §7.3. The item carries `seat { number, characteristicCodes }` and `inventory.control = "SeatMap"`.
4. In catalogue mode without `seats[]`, no seat item is returned.
5. A selection of a `Seat` product carries `flightRef` and `seatNumber`; the seat must be in `seats[]` (`16304` otherwise) and the selected product must be the one that matches it (`16305` otherwise).
6. A traveller can have at most one seat item selected per flight (`16307`).

---

## 8. Reservations (P2B) and stock (P4)

Every ancillary service sold on an order is reserved and confirmed here, through one interface, whatever the product. For an `Unlimited` product nothing is counted, but the reservation is still made: it is where Ancillary checks the accepted selection and records what it committed to. For a `Quota` product (P4) the reservation also takes units from a stock pool.

### 8.1 `ServiceReservation`

One reservation per reserve request of Ordering.

| Field | Type | Null | Meaning |
|---|---|---:|---|
| `Id` | `long` | No | Returned to Ordering as `reservationId`. |
| `IdempotencyKey` | `string` | No | Ordering's identity of this request. Unique. 1–128 characters. |
| `Reference` | `string` | No | Ordering's correlation text (the order). 1–128 characters. Not used for any decision. |
| `ExpiresAt` | `DateTimeOffset` | No | When an unconfirmed reservation lapses. Judged by Ancillary's clock. |
| `CreatedAt` | `DateTimeOffset` | No | |
| `Units` | list of `ServiceReservationUnit` | No | At least one. |

`ServiceReservationUnit` — one reserved occurrence:

| Field | Type | Null | Meaning |
|---|---|---:|---|
| `Id` | `long` | No | Returned as `unitRef`; Ordering names it when cancelling a confirmed unit. |
| `UnitReference` | `string` | No | Ordering's own key for the unit. Unique within the reservation. 1–128 characters. |
| `OwnerAirlineId`, `ProductRef`, `ProductVersion`, `PriceRuleId` | | No | The product version and price rule that were accepted. |
| `TravellerRef`, `BoundRef` | `string` | No | The occurrence, in the caller's keys. |
| `FlightRef` | `string?` | Yes | Set for a flight-scoped occurrence. |
| `CoveredFlightIds` | list of `long` | No | The `flightId` of every covered flight, in travel order. |
| `Quantity` | `int` | No | `≥ 1`. |
| `CurrencyId` | `int` | No | |
| `Total` | `decimal` | No | The amount Ancillary accepted for this unit: the item's `total` at reserve time. |
| `InventoryControl` | `AncillaryInventoryControl` | No | As the product had it at reserve time. |
| `Status` | `ServiceReservationUnitStatus` | No | See below. |

Unit states: `Held` → `Confirmed` → `Cancelled`; `Held` → `Released`; `Held` → `Expired`.

Nothing of a reservation changes after it is created except the units' `Status`. A later change of the product or of its price does not touch a reservation.

### 8.2 Behaviour rules

1. **All or nothing.** Every operation either changes all the units it addresses or changes nothing.
2. **Repeating is safe.** Repeating a request that already succeeded returns success without a second effect.
3. **Reserve checks the selection.** The units of a reserve request are evaluated exactly as the selections of §7.6, in request order, against the request's context. The first failure refuses the whole request with that check's error (`16301`–`16309`), and nothing is stored.
4. **Confirm does not check again.** What was accepted at reserve is what is confirmed.
5. **Expiry.** A `Held` unit whose reservation's `ExpiresAt` has passed counts as `Expired` for every read and every operation from that instant, whether or not that has been recorded. For an `Unlimited` product nothing else has to happen. (P4: for a `Quota` product the lapse is also recorded, to free the stock.)
6. Ancillary never calls Ordering and never changes anything outside this service.

What Ordering can rely on:

| Capability | Value |
|---|---|
| Idempotency identity | accepted on Reserve; Confirm, Release and Cancel are idempotent by state |
| Result of a repeated request | the same effect, never a second one |
| Result granularity | atomic per operation |
| Unit reference | given (`unitRef`) |
| Expiry | by Ancillary's clock, at the `ExpiresAt` Ordering asked for |
| Read-back | yes |
| Extend, split | no |

### 8.3 Operations (caller: Ordering)

**Reserve** — `POST Service/v1/ServiceReservations`

```json
{
  "idempotencyKey": "reserve-hold:9300",
  "reference": "order:9001",
  "expiresAt": "2026-10-03T12:30:00+00:00",
  "context": {
    "currencyId": 978,
    "asOf": "2026-10-03T12:00:00+00:00",
    "salesContext": { "channel": "BackOffice", "customerId": null, "travelAgencyId": null, "countryId": null },
    "travellers": [ { "ref": "7001", "passengerTypeCode": "ADT", "flightRefs": ["8101"] } ],
    "bounds": [ { "ref": "8001", "flights": [
      { "ref": "8101", "flightId": "100", "originAirportId": 1, "destinationAirportId": 2,
        "departureDateTime": "2026-10-10T08:00:00+03:30", "marketingAirlineId": 10, "operatingAirlineId": 10 } ] } ],
    "existing": []
  },
  "units": [
    { "unitReference": "9102", "productRef": "XBAG1", "productVersion": 1, "priceRuleId": "501",
      "travellerRef": "7001", "boundRef": "8001", "flightRef": null, "quantity": 1 }
  ]
}
```

`context` is the quote request of §7.1 without `selections`. `existing` lists what the order already holds **other than the units of this request**. A unit is a selection of §7.1 plus `unitReference`.

| Situation | Result |
|---|---|
| every unit passes §7.6 | A new reservation; all units `Held`. Returns the reservation (below). |
| the same `idempotencyKey` with the same content | The existing reservation with the current unit statuses. Nothing is evaluated again. |
| the same `idempotencyKey` with different content | `16405` |
| a unit fails a check of §7.6 | that check's error; nothing is stored |
| `expiresAt` not in the future, repeated `unitReference`, no units | `16412` |
| (P4) a pool lacks stock / is closed | `16403` / `16404`; nothing is stored |

"Content" means `reference`, `expiresAt` and, per unit, `unitReference`, `productRef`, `productVersion`, `priceRuleId`, `travellerRef`, `boundRef`, `flightRef`, `quantity`. The context is not compared.

The reservation as returned by Reserve and by Read:

```json
{
  "reservationId": "9400", "idempotencyKey": "reserve-hold:9300", "reference": "order:9001",
  "expiresAt": "2026-10-03T12:30:00+00:00",
  "units": [
    { "unitRef": "9401", "unitReference": "9102", "status": "Held",
      "ownerAirlineId": 10, "productRef": "XBAG1", "productVersion": 1, "priceRuleId": "501",
      "travellerRef": "7001", "boundRef": "8001", "flightRef": null, "coveredFlightIds": ["100"],
      "quantity": 1, "currencyId": 978, "total": 38.50, "inventoryControl": "Unlimited" }
  ]
}
```

Ordering compares each unit's `total` with the amount it accepted for that service.

**Read** — `GET Service/v1/ServiceReservations/{reservationId}`: the reservation with each unit's status as of now. Unknown: `16401`.

**Confirm** — `POST Service/v1/ServiceReservations/{reservationId}/Confirmations` (the whole reservation, no body)

| Units, as of now | Result |
|---|---|
| all `Held` | all become `Confirmed` |
| all `Confirmed` | success, no change |
| any `Expired` | `16406` |
| any `Released` | `16407` |
| any `Cancelled` | `16408` |
| `Held` and `Confirmed` mixed | `16409` |

**Release** — `POST Service/v1/ServiceReservations/{reservationId}/Releases` (the whole reservation, no body)

| Units, as of now | Result |
|---|---|
| all `Held` | all become `Released` |
| all `Released` | success, no change |
| any `Confirmed` | `16410` |
| any `Expired` | `16406` |
| any `Cancelled` | `16408` |
| any other mixture | `16409` |

**Cancel confirmed units** — `POST Service/v1/ServiceReservations/{reservationId}/Cancellations`, body `{ "unitRefs": ["9401"] }` (some or all units of the reservation)

| Selected units, as of now | Result |
|---|---|
| all `Confirmed` | they become `Cancelled` |
| all `Cancelled` | success, no change |
| any `Held` | `16411` |
| any `Released` | `16407` |
| any `Expired` | `16406` |
| `Confirmed` and `Cancelled` mixed | `16409` |
| a `unitRef` that is not in the reservation, or an empty list | `16412` |

An unknown `reservationId` is `16401` in every operation. A successful Confirm, Release or Cancel returns the reservation as Read does.

### 8.4 Stock pools (P4)

For a product with `InventoryControl = Quota`, a reservation unit also takes its quantity from the pool of each covered flight.

`StockPool` — one per `OwnerAirlineId + ProductRef + FlightId`:

| Field | Type | Null | Meaning |
|---|---|---:|---|
| `Id` | `long` | No | |
| `OwnerAirlineId`, `ProductRef`, `FlightId` | | No | The pool's key. |
| `Capacity` | `int` | No | How many units exist on this flight. Starts as the product's `DefaultQuotaPerFlight`; a backoffice user may override it. Never less than `Held + Confirmed`. |
| `Held` | `int` | No | Units currently held and not yet confirmed. |
| `Confirmed` | `int` | No | Units confirmed for orders. |
| `Status` | `StockPoolStatus` | No | `Open`, or `Closed` (no new reservation is accepted). |

Available = `Capacity − Held − Confirmed`. A pool comes into existence the first time it is needed (a reservation, or a capacity override); until then the product's `DefaultQuotaPerFlight` is its availability.

| Unit transition | Effect on the pool |
|---|---|
| created as `Held` | `Held += quantity` |
| `Held` → `Confirmed` | `Held −= quantity`, `Confirmed += quantity` |
| `Held` → `Released` | `Held −= quantity` |
| `Held` → `Expired` | `Held −= quantity` |
| `Confirmed` → `Cancelled` | `Confirmed −= quantity` |

Rules added in P4:

1. A unit's status and its pool's counters always change together. Concurrent reservations on one pool can never make `Held + Confirmed` exceed `Capacity`.
2. A lapsed `Held` unit of a `Quota` product is recorded — unit status, pool counter, and one `AncillaryReservationExpiredV1` message — either by a periodic job or by the next Reserve on the same pool, whichever comes first. A new reservation is therefore never refused because of stock that has already lapsed.
3. When the flight is cancelled, every pool of that flight becomes `Closed`. Existing reservations are not touched; Ordering decides what happens to its services.
4. Reserve refuses with `16403` when a pool lacks stock and with `16404` when a pool is closed.

Backoffice: read a pool by `ownerAirlineId + productRef + flightId` (when none exists yet, the answer shows the default quota and zero counters); set its `Capacity` (creates the pool if needed; refused with `16403` when below `Held + Confirmed`); close it (`16413` when the pool id is unknown).

In the evaluation of §7: `remaining` = the pool's Available (the default quota when no pool exists; 0 when the pool is `Closed`). The occurrence is applicable only if `remaining ≥ Quantity.Min`; `maxQuantity` is not more than `remaining`; `inventory` = `control "Quota"`, `remaining`. A selection asking for more than `remaining` is `16308`. In an offer the number is information only: the reservation is the binding decision.

Message published in P4 — `AncillaryReservationExpiredV1`: `ReservationId`, `IdempotencyKey`, `Reference`, `ExpiredAt`, `Units[]` (`UnitRef`, `UnitReference`, `OwnerAirlineId`, `ProductRef`, `CoveredFlightIds`, `Quantity`). Once per reservation, containing exactly the `Quota` units that lapsed.

---

## 9. Enumerations

Numeric values are fixed. A member exists from the phase shown.

| Enum | Members (value) — phase |
|---|---|
| `AncillaryProductType` | `ExtraBaggage` 1 — P1; `LoungeAccess` 2 — P2; `PetInCabin` 5 — P4; `Seat` 3 — P5. Reserved for later: `Meal` 4, `SportsEquipment` 6, `PriorityBoarding` 7, `FastTrack` 8, `Cip` 9, `UnaccompaniedMinor` 10, `InFlightConnectivity` 11, `InFlightEntertainment` 12, `ESim` 13, `Insurance` 14, `Other` 99. |
| `AncillarySalesScope` | `TravellerBound` 1 — P1; `TravellerSegment` 2 — P2. Reserved: `Traveller` 3, `Order` 4. |
| `AncillaryDocumentType` | `EmdAssociated` 2 — P1; `EmdStandalone` 3 — P2; `None` 1 — P6. |
| `AncillaryInventoryControl` | `Unlimited` 1 — P1; `Quota` 2 — P4; `SeatMap` 3 — P5. |
| `AncillaryQuantityUnit` | `Piece` 1 — P1; `Each` 3 — P2. Reserved: `Kilogram` 2. |
| `AncillaryWeightUnit` | `Kg` 1, `Lbs` 2 — P1. |
| `AncillaryPriceLineCategory` | `Ancillary` 1, `Tax` 2 — P1; `Fee` 3 — P6. |
| `ServiceSubCodeSource` | `Industry` 1, `CarrierDefined` 2 — P1. |
| `ServiceSubCodeStatus` | `Active` 1, `Retired` 2 — P1. |
| `AncillaryProductStatus` | `Draft` 1, `Active` 2, `Suspended` 3, `Retired` 4 — P1. |
| `AncillaryPriceRuleStatus` | `Draft` 1, `Active` 2, `Suspended` 3, `Retired` 4 — P1. |
| `ServiceReservationUnitStatus` | `Held` 1, `Confirmed` 2, `Released` 3, `Expired` 4, `Cancelled` 5 — P2B. |
| `StockPoolStatus` | `Open` 1, `Closed` 2 — P4. |

---

## 10. Operations exposed

| Operation | Caller | Route | Phase |
|---|---|---|---|
| Register sub code | Backoffice | `POST Backoffice/v1/ServiceSubCodes` | P1 |
| Retire / reactivate sub code | Backoffice | `POST Backoffice/v1/ServiceSubCodes/{id}/Retire` · `/Reactivate` | P1 |
| Read sub code; list sub codes | Backoffice | `GET Backoffice/v1/ServiceSubCodes/{id}` · `/Paginated` | P1 |
| Define product | Backoffice | `POST Backoffice/v1/AncillaryProducts` | P1 |
| Change product | Backoffice | `PUT Backoffice/v1/AncillaryProducts/{id}` | P1 |
| Activate / Suspend / Retire / Revise product | Backoffice | `POST Backoffice/v1/AncillaryProducts/{id}/Activate` · `/Suspend` · `/Retire` · `/Revise` | P1 |
| Read product; list products | Backoffice | `GET Backoffice/v1/AncillaryProducts/{id}` · `/Paginated` | P1 |
| Define price rule | Backoffice | `POST Backoffice/v1/AncillaryPriceRules` | P1 |
| Change price rule | Backoffice | `PUT Backoffice/v1/AncillaryPriceRules/{id}` | P1 |
| Activate / Suspend / Retire price rule | Backoffice | `POST Backoffice/v1/AncillaryPriceRules/{id}/Activate` · `/Suspend` · `/Retire` | P1 |
| Read price rule; list price rules | Backoffice | `GET Backoffice/v1/AncillaryPriceRules/{id}` · `/Paginated` | P1 |
| Quote (reference implementation of §7; tests and backoffice preview) | tests, backoffice tools | `POST Service/v1/AncillaryQuotes` | P1 |
| Reserve / read reservation | Ordering | `POST Service/v1/ServiceReservations` · `GET Service/v1/ServiceReservations/{reservationId}` | P2B |
| Confirm / release / cancel | Ordering | `POST Service/v1/ServiceReservations/{reservationId}/Confirmations` · `/Releases` · `/Cancellations` | P2B |
| Read pool / set capacity / close pool | Backoffice | `GET Backoffice/v1/StockPools` · `PUT Backoffice/v1/StockPools/Capacity` · `POST Backoffice/v1/StockPools/{id}/Close` | P4 |

There is no other operation, and no operation for sales channels. AirOffer uses no operation at all: it reads the published read model (§15).

---

## 11. Errors

A business error carries a numeric code and a message. A malformed request (missing field, wrong format, unknown value name) is HTTP 400 without a code.

| Code | Meaning | HTTP | Phase |
|---|---|---:|---|
| 16101 | Product not found | 404 | P1 |
| 16102 | Product reference already exists for this airline | 409 | P1 |
| 16103 | Product is not a Draft and cannot be changed | 409 | P1 |
| 16104 | Product status change not allowed | 409 | P1 |
| 16105 | Product combination is not sold (§3.4) | 422 | P2 |
| 16106 | Product is invalid (any other rule of §4) | 422 | P1 |
| 16107 | A Draft of this product already exists | 409 | P1 |
| 16108 | This version cannot be revised | 409 | P1 |
| 16109 | The product's sub code is not an Active sub code of the airline | 422 | P1 |
| 16110 | Sub code not found | 404 | P1 |
| 16111 | This code is already registered for the airline | 409 | P1 |
| 16112 | Sub code is invalid (any rule of §4.5) | 422 | P1 |
| 16113 | Sub code status change not allowed | 409 | P1 |
| 16114 | Industry sub code is not in the industry reference | 422 | P1 |
| 16201 | Price rule not found | 404 | P1 |
| 16202 | Price rule is not a Draft and cannot be changed | 409 | P1 |
| 16203 | Price rule status change not allowed | 409 | P1 |
| 16204 | Another Active rule has this priority | 409 | P1 |
| 16205 | The rule's product does not exist | 422 | P1 |
| 16206 | Price rule is invalid (any other rule of §5) | 422 | P1 |
| 16301 | Quote request is inconsistent | 422 | P1 |
| 16302 | Selected product not found | 422 | P1 |
| 16303 | Selection does not fit the product's scope | 422 | P1 |
| 16304 | A reference is not in the request | 422 | P1 |
| 16305 | Selected occurrence is not applicable | 422 | P1 |
| 16306 | Quantity not allowed | 422 | P1 |
| 16307 | Occurrence selected twice | 422 | P1 |
| 16308 | Not enough stock for the selection | 409 | P4 |
| 16309 | The selection is no longer current (product version or price rule changed) | 409 | P1 |
| 16401 | Reservation not found | 404 | P2B |
| 16403 | Not enough stock | 409 | P4 |
| 16404 | Stock pool is closed | 409 | P4 |
| 16405 | Idempotency key reused with a different request | 409 | P2B |
| 16406 | Reservation has expired | 409 | P2B |
| 16407 | Reservation was released | 409 | P2B |
| 16408 | Reservation was cancelled | 409 | P2B |
| 16409 | Reservation is in a mixed state | 409 | P2B |
| 16410 | Reservation is already confirmed | 409 | P2B |
| 16411 | Reservation is not confirmed | 409 | P2B |
| 16412 | Reservation request is invalid | 422 | P2B |
| 16413 | Stock pool not found | 404 | P4 |

In P1 each of scope, document type and inventory control has a single member, so `16105` cannot occur yet.

---

## 12. Phases

| Phase | Business slice | What Ancillary builds |
|---|---|---|
| P1 | Extra Baggage on an existing order: EMD-A, unlimited | `ServiceSubCode`, `AncillaryProduct`, `AncillaryPriceRule`, Quote |
| P2 | Lounge Access on an existing order: EMD-S, unlimited | flight-scoped occurrences, `LoungeDetail`, combination rule |
| P2B | The uniform interface: every ancillary sale is reserved and confirmed at Ancillary; AirOffer reads the catalogue directly | `ServiceReservation` (§8.1–§8.3); the published read model and its stamp (§15) |
| P3 | Showing both while shopping for flights and accepting them when the order is created | nothing (AirOffer and Ordering only) |
| P4 | Pet in cabin, the first limited product | `StockPool`, quota in reservations, lapse recording and its message (§8.4) |
| P5 | Paid seat | `Seat` type, `SeatDetail`, seat pricing in the quote |
| P6 | Pricing depth | `Fee` lines, remaining conditions, sales context, document `None` |

Each phase is built and proven by calling the running service and by one manual run across AirOffer and Ordering; the Owner reviews the domain, the operations and that run; then the phase's conformance tests are written, one or more for every expected-behaviour row of its phase document. A phase is closed only when those tests pass.

P1 and P2 sell only on an existing order. A commercial release that must sell ancillaries during flight booking needs P3 as well; closing P1 does not close that requirement.

### What later phases still need from outside this service

Nothing below is a benchmark question; each is a fact another service has to provide when that phase starts.

| Phase | Needed | Who |
|---|---|---|
| P3 | AirOffer carries the customer's selection inside its offer and returns the items on `FlightOffers/Details`; Ordering accepts them when the order is created. Ancillary builds nothing. | AirOffer, Ordering |
| P4 | A message from FlightFlow that states a flight is cancelled. At `d2180b2` FlightFlow publishes `FlightUpdated` on cancellation carrying only the flight id and version, with `Status` empty; it must state the cancelled status. | FlightFlow |
| P5 | AirOffer sends each candidate seat's characteristic codes from FlightFlow's seat map. | AirOffer |
| not scheduled | **Extra baggage over several flights.** Ordering first models how an EMD carries a fee that spans several coupons (a fare calculation for the EMD, or an authoritative proration). Only then is the one-flight limitation of §7.2 lifted. No convention for splitting or placing the amount is approved before that. | Ordering, revenue accounting |

Defaults the Owner may still change before the phase starts, without any other change to this document: the first limited product (`PetInCabin` on `0BT`, P4), and the seat matching rule of §7.8 (P5).

---

## 13. Record of the industry facts used

So that no phase needs new research, the facts this document relies on are recorded here with their source.

| Fact | Used for | Source |
|---|---|---|
| A sub code is three alphanumeric characters; industry sub codes are maintained by ATPCO and always begin with a digit; a carrier-defined sub code applies only to the carrier that defines it. | §4.5 | ATPCO, *Optional Services and Branded Fares Reference Manual* |
| The industry list gives, per code: group, sub-group, description 1, description 2, commercial name and a *recommended* RFIC. Codes beginning `98`/`99` are reserved for fees outside optional services. Commercial names are at most 30 characters, without `/`, `-`, `.`. | §4.5 | ATPCO, *Optional Services Industry Sub Codes*, revised 30 March 2026 (published by ATPCO as `Opt_Scvs_Industry-Sub-Codes-online_C.pdf`; a copy is kept with the project evidence) |
| `0CC` First Excess Bag (BG, B1, C); `0CD`, `0CE` … are the second, third … bag; `0BX` Lounge Access (LG, E); `0BT` Pet In Cabin (PT, PC, C); `0B5` Pre Reserved Seat Assignment (SA, A). | §4.5 | same list, entries 203–205, 563, 668, 686 |
| RFIC letters: A air transportation, B surface/non-air, C baggage, D financial impact, E airport services, F merchandise, G in-flight services, I individual airline use. | §4.2 | same list, RFIC table |
| A service record carries service type, sub code, SSR code, EMD type, RFIC and booking method; baggage uses service types A (allowance), C (charges), P (prepaid). Agreement between carriers (concurrence) exists only for industry sub codes. | §4.2 | ATPCO reference manual; ATPCO *Baggage Data Migration Guide* |
| An EMD-A value coupon is associated with exactly one ticket flight coupon; several EMD coupons may be associated with one flight coupon. One EMD has one RFIC; each coupon has an RFISC. | §2.3, Ordering spec | IATA, *Airline Guide to EMD Implementation*; ARC and Amadeus EMD guides |
| An EMD's base fare amount comes either from its fare calculation area or from the sum of its coupon values; the two are mutually exclusive; coupon values must add up to the base fare amount. Base fare amount, taxes/fees/charges and total document amount are separate data elements. When a fee is calculated over more than one coupon, the filed-fee elements are left blank on all coupons but the last of the component. | §7.2, Ordering spec | IATA, *Airline Guide to EMD Implementation* (2010), §4.2.4.3, §5.1.1.2 |
| The passenger name on an EMD-A must be exactly the name on the associated ticket; the operating carrier of an EMD-A coupon must be that of the associated ticket coupon; the ticket coupon must not be suspended or in a final status; excess baggage uses EMD-A. | Ordering spec | same guide, §4.2.4, §4.2.5 |
| The standard RFIC for baggage charges on an EMD-A is C; the EMD-A is lifted with its ticket coupon. | §4.2 | IATA, *Interline Considerations on Baggage Standards* |
| Some services are unlimited and some have a counted quota per flight; the quota lives with inventory, the sale needs a confirmed reservation before the document. | §3.3, §8 | Amadeus service catalogue; Sabre Inventory "Ancillary Availability" |
| An ancillary is not a separate offer for a seller: the catalogue comes with flight pricing, the selection travels in the flight offer, final availability is checked at booking; seat map is a separate read. | §2.2 | Amadeus Self-Service flight APIs |
| Product definition, offer management, stock keeping and order management are separate capabilities. | §1 | IATA, *The Product Catalog as Foundation for Modern Airline Retailing* (2026) |
| NDC `ServiceList` returns the applicable ancillary services both in the prime booking flow (from an offer) and after sale (from an order). The answer has an `ALaCarteOffer` (offer id, expiration, offer items each with an id, the eligible passengers and segments, a unit price with tax breakdown, and a reference to a service definition) and a list of service definitions (id, name, description, booking instructions; RFIC and RFISC identify the service). The seller sends the chosen offer item ids back in `OfferPrice`, `OrderCreate` or `OrderChange`. | §2.5, Edge contract | IATA NDC schemas 17.2–21.3; Amadeus Altéa NDC ServiceList implementation guides; airline NDC guides (IAG, British Airways, Malaysia Airlines) |
| In ONE Order the order management system tells delivery providers which services to deliver (`ServiceDeliveryNotif`) and providers report every status change (`ServiceStatusChangeNotif`). Delivery statuses: Ready to proceed, Ready to deliver, In progress, Delivered, Unable to deliver, Suspended, Removed, Not claimed, Expired. | §2.3, Edge contract | IATA Resolution 797; IATA Enhanced and Simplified Distribution implementation guides 18.2–21.3 |
| In the platform itself, AirOffer reads other services' `ReadModel` tables by SQL and evaluates fares and point-of-sale rules in its own code, with snapshot caches refreshed by a stamp probe; Ordering reserves through a provider interface (validate, reserve, read, release, confirm, cancel-confirmed). | §2.2, §2.3, §15 | AirAvail `ac84040`; Ordering.Final `cd50a2a` |

## 14. Source baseline

| Repository | Commit | What this document assumes about it |
|---|---|---|
| `AeroTech.Ancillary` | `9bbd385` | Phases 1 and 2 built and tested on the skeleton `843cf7c`. |
| `AeroTech.Ordering.Final` | `cd50a2a` | Everything listed in §2 of `handover/Ordering/spec.md`. |
| `AeroTech.AirAvail` (AirOffer) | `ac84040` | No ancillary operation exists. It reads FlightFlow's and AirPricing's read models by SQL (Dapper scripts), caches reference data and point-of-sale rules as snapshots, and re-prices `FlightOffers/Details` on every call from a plain-text offer id. |
| `Aerotech.FlightFlow` | `d2180b2` | Owns flights, capacity, seat map and seat holds; cancellation message as noted in §12. |

If a repository has moved on from its commit when a phase starts, the facts that phase relies on are checked again before coding.

---

## 15. Published read model (P2B)

AirOffer reads these tables of Ancillary's query database, schema `ReadModel`, directly. They are a contract: a table or column listed here is never removed, renamed or given another meaning; additions are allowed.

| Table | Rows AirOffer uses | Columns |
|---|---|---|
| `AncillaryProducts` | `Status = 2` (Active) | `Id`, `OwnerAirlineId`, `ProductRef`, `Version`, `Type`, `Name`, `Description`, `SalesScope`, `QuantityUnit`, `QuantityMin`, `QuantityMax`, `DocumentType`, `Rfic`, `Rfisc`, `ServiceTypeCode`, `GroupCode`, `SubGroupCode`, `Description1Code`, `Description2Code`, `Refundable`, `Commissionable`, `Reusable`, `FormOfRefundCode`, `InterlineSettlementAllowed`, `InventoryControl`, `BaggagePieces`, `BaggageWeight`, `BaggageWeightUnit`, `LoungeAirportIds`, `Status`, `LastUpdateTime` |
| `AncillaryPriceRules` | `Status = 2` (Active) | `Id`, `OwnerAirlineId`, `ProductRef`, `Priority`, `CurrencyId`, `SalesFrom`, `SalesTo`, `TravelFrom`, `TravelTo`, `PassengerTypes`, `OriginAirportIds`, `DestinationAirportIds`, `Status`, `LastUpdateTime` |
| `PriceLines` | lines of those rules | `Id`, `AncillaryPriceRuleId`, `Category`, `Code`, `Name`, `Amount` |

Conventions:

- Enumerated columns hold the numeric values of §9.
- List columns (`LoungeAirportIds`, `PassengerTypes`, `OriginAirportIds`, `DestinationAirportIds`) hold a JSON array of numbers; SQL `NULL` means "no restriction" (or, for `LoungeAirportIds`, "not a lounge"). `PassengerTypes` holds the numeric values of the platform's passenger-type vocabulary.
- The order of a rule's lines is the order of `PriceLines.Id`.
- `LastUpdateTime` (new in P2B, on `AncillaryProducts` and `AncillaryPriceRules`) is set to the clock every time the row is written, for any reason, including a status change.

**Stamp.** The pair `MAX(LastUpdateTime)` and `COUNT(*)`, taken over both tables together, changes whenever anything AirOffer could read has changed. AirOffer compares it with the stamp of its snapshot and reloads when it differs.

A later phase that adds something AirOffer must read (stock figures in P4, new condition columns in P6) adds it here first.

## 16. Horizon: Ancillary as a marketplace

Not scheduled and not built. Recorded so that today's boundaries are not broken later.

The catalogue may one day hold services of several suppliers — the airline's own and third parties' (airport services, connectivity, insurance, ground transport) — each with its own sale and price rules and its own way of being delivered. When that comes:

- A product gains a supplier and a delivery method. Rules that depend on the supplier stay inside Ancillary.
- Reserve, Confirm, Release and Cancel keep their shape. Behind them, Ancillary talks to the supplier where one exists; Ordering sees the same interface and the same statuses, and never the supplier.
- A service whose price cannot be tabulated in advance cannot be offered from the published read model; for such products AirOffer would need an answer computed by Ancillary at request time. That is the one case in which a runtime call from AirOffer to Ancillary would be introduced, and it would be decided then.
- Delivery status of a service, as ONE Order describes it, is reported by Ancillary to Ordering; Ordering stays the single record of the order.

Nothing in Phases 1–6 depends on this section.
