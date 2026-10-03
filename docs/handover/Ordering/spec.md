# Ordering — Ancillary Implementation Spec

**Date:** 2026-10-03
**For:** the Ordering service. It states exactly what Ordering builds so that an extra bag can be added to an existing ticketed order, reserved and confirmed at Ancillary, and documented with an EMD-A. It is a companion contract: it becomes binding for Ordering when the Owner adds it to Ordering's own authority set.
**Checked against:** `AeroTech.Ordering.Final` `cd50a2a` (head of `k8s-stg`); Ancillary `reference/Ancillary-Domain-Master.md` (R6); `reference/Ancillary-Edge-Contract.md`; `handover/AirOffer/spec.md`; IATA Airline Guide to EMD Implementation.
**Paths** in this document are relative to Ancillary's `docs/` folder; in the Ordering repository the same files sit under `docs/ancillary/` (see `handover/README.md`).
**Names:** the command, entity and field names are the ones Ordering's Master v2.0 already uses (§6.4, §6.7, §6.8, §17, §18, §26).

Every choice below is fixed. Whoever implements it takes no domain decision; if something is missing or contradicts Ordering's source, stop and ask the Owner.

**How the pieces fit.** Ordering asks **AirOffer** what can be added and what the chosen items are (list, details). It asks **Ancillary** to reserve and confirm every ancillary service, through its existing reservation machinery and one more provider. It issues the document itself. It never reads Ancillary's catalogue and never asks Ancillary for a price.

**Phase-1 limitation.** An extra bag is sold and documented only when it covers exactly one flight, so its EMD-A has exactly one coupon. AirOffer and Ancillary enforce this (Ancillary Master §7.2); this spec relies on it and checks it. A bag over several flights needs a way for an EMD to carry a fee that spans several coupons — a fare calculation or an authoritative proration — which Ordering does not model; it is deferred until that is designed.

Payment is outside this spec, as it is outside Ordering's core: the order, the service and the EMD hold no payment state. Whatever financial approval a production sale needs is enforced by the orchestration that calls these commands.

---

## 1. Scope

| In Phase 1 | Not in Phase 1 |
|---|---|
| Listing the services that can be added to an order (NDC `ServiceList`, post-sale) | Selling before the order exists (Phase 3) |
| `AddAncillaryFromOffer` — accepting selected offer items on a ticketed order (NDC `OrderChange`) | Cancelling an ancillary; voiding, refunding or exchanging an EMD |
| Reserving and confirming every ancillary service at Ancillary, through the existing Reserve and Confirm commands | Extra baggage that covers more than one flight; EMD-S |
| `IssueAncillaryDocument` — issuing the EMD-A locally | OTA, OtaPanel, IBE surfaces (the shapes are already the channel shapes; only the Backoffice surface is built) |
| Backoffice surface only | Payment |

---

## 2. What the source has today (`cd50a2a`)

| Fact | Where |
|---|---|
| `OrderService` requires a non-empty, immutable `FulfillmentProviderKey`. | `Domain/OrderAggregate/Entities/OrderService.cs` |
| `ReservationProviderResolver.Resolve` throws `NoFulfillmentAdapterRegistered` (2505) for a key with no registered provider; only the FlightFlow provider is registered. | `Application/FulfillmentReservationAggregate/Services/ReservationProviderResolver.cs`, `Providers/DependencyInjection.cs` |
| Provider keys: `FlightFlow`, `LocalDocumentAuthority`. | `Domain/Providers/FulfillmentProviderKeys.cs` |
| `OrderServiceType`: `AirTransportation = 1`, then implicit values up to `ExtraSeat` (22). `BaggageAllowance` exists; no purchased-baggage member. | `Contracts/.../OrderServiceType.cs` |
| `ProductType` has `Baggage`. | `Contracts/.../ProductType.cs` |
| `OrderPricingReason`: `InitialSale 1 … Cancel 8`. `OrderPricingLineSubCategory`: `BaseFare 1, Tax 2, VAT 3, ServiceFee 4, Promo 5`. `OrderPricingLineCategory.Ancillary = 8` exists. | `Contracts/.../Enums` |
| `OrderChangeType.AddProduct = 2` exists. A change is created with `CommercialVersion + 1`, and the order takes that version. | `Order.Cancellation.cs` |
| Root status `Ticketed` is decided only by ticketable air services (`MarkTicketed`). | `Order.Issuance.cs` |
| Local ticket issue: order lock → outstanding services (none → return what exists) → unresolved-task check → stock check → stock lock → `FulfillmentTask` (`LocalDocumentAuthority`, key `issue-ticket:{taskId}`) → `stock.Allocate(taskId, role)` → document → `MarkIssued` → task targets → one `SaveChanges`. | `Application/OrderAggregate/Commands/IssueOrder/IssueOrderService.cs` |
| A ticket's `IssuedTotal` is the sum of its coupons' `IssuanceValue`. A ticket holds `TravellerId` and the `TravellerProfileRevisionId` used when it was issued; a traveller's current revision can differ later. | `Domain/ElectronicTicketAggregate/ElectronicTicket.cs`, `OrderTraveller.cs` |
| A ticket coupon keeps an `IssuedSegment` snapshot (marketing and operating airline, airports, times, booking class). | `ElectronicTicketAggregate/ValueObjects/IssuedSegmentSnapshot.cs` |
| Ordering's Master defines `EmdPriceLink.EmdCouponId` as optional and `EmdCoupon.IssuanceValue` as required. | Master §17.2, §17.3 |
| Ticket coupon: `CurrentOrderServiceId`, `FinancialStatus` (`Open, Used, Void, Exchanged, Refunded, Suspended`), `ControlStatus` (`Local, External, ReleasePending, Unknown`). | `TicketCoupon.cs`, enums |
| `OrderFulfillmentTaskType.IssueEmd = 6`, `AccountableDocumentKind.ElectronicMiscDocument = 2`, `FulfillmentTargetKind.ElectronicMiscDocument = 5`, `EmdCoupon = 6`, `ElectronicMiscDocumentType { Associated = 1, Standalone = 2 }`, `EmdCouponPurpose.Service = 1`, `EmdCouponStatus.OpenForUse = 1` exist. | `Contracts/.../Enums` |
| Integration contracts `ElectronicMiscDocumentIssued` and `ElectronicMiscDocumentIssuedCoupon` exist (coupon `IssuanceValue` is a non-null decimal); nothing publishes them yet. | `Contracts/.../IntegrationEvents/V1` |
| Reservation goes through `IReservationProvider` (capability, plan units, prepare, reserve, read, release, confirm, cancel-confirmed). `ReserveService` groups the services to reserve by provider key and mode, creates one `FulfillmentReservation` per group, and maps the provider's answer (`ProviderOperationRef`, per unit `UnitCorrelationKey`, `ProviderUnitRef`, status). | `Domain/Providers/Reservation`, `Application/FulfillmentReservationAggregate/Commands/Reserve/ReserveService.cs` |
| `Order.EnsureReservable` accepts only `Created`, `Confirmed`, `ReservationUnconfirmed`, `ReserveFailed`; `EnsureNewReservationAllowedAt` refuses after the last ticketing date. A `Ticketed` order can therefore not reserve anything today. | `Order.Reservation.cs` |
| Hold deadlines are enforced by Ordering's own poller, which releases due holds at the provider; Ordering consumes no provider expiry message. | `Consumers/Jobs/ReservationDeadlinePoller.cs`, `ReservationDeadlineService.cs` |
| Backoffice reservation routes: `POST Orders/{orderId}/Reservations`, `…/Reservations/Services`, `…/Reservations/{reservationId}/Release`, `…/Reservations/Confirmations`. | `RestApi/V1/FulfillmentReservationAggregate/Controllers/BackofficeController.cs` |
| No ancillary or baggage service entity, no EMD aggregate, no add-to-order command exist in code. Ordering's Master defines them for its ancillary stage. | Master §6.7, §6.8, §17 |
| Error codes in use: 2001–2822. | `ExceptionFactory.cs` |

---

## 3. Model additions

### 3.1 Enum members

Appended at the end of each enum; no existing value changes.

| Enum | New member | Value |
|---|---|---:|
| `OrderServiceType` | `BaggageCharge` | 23 |
| `OrderPricingReason` | `AddService` | 9 |
| `OrderPricingLineSubCategory` | `Ancillary` | 6 |

`BaggageAllowance` keeps meaning the free allowance; a purchased bag is `BaggageCharge`.

### 3.2 `OrderAncillaryService` and `OrderBaggageService`

`OrderAncillaryService` is an abstract `OrderService`. `OrderBaggageService` is the concrete type for `ExtraBaggage`. One accepted item becomes **one `OrderItem` and one `OrderBaggageService`**.

Base `OrderService` values:

| Field | Value |
|---|---|
| `TravellerId` | the selection's traveller |
| `ServiceType` | `BaggageCharge` |
| `FulfillmentProviderKey` | `Ancillary` — for every ancillary service, whatever its inventory control (§3.3) |
| `CommercialStatus` | `Active` |
| `CreatedByChangeId` | the change of §5 |
| `ResponsibleAirlineId` (`int?`, Master §6.4, materialized now on the base type; null for existing services) | `ownerAirlineId` |

`OrderAncillaryService` fields (all set once, at creation, from the item AirOffer returns in §5; never changed):

| Field | Type | Null | Source |
|---|---|---:|---|
| `AcceptedOfferItemId` (new) | `string` | No | the `offerItemId` the seller selected |
| `ServiceDefinitionRef` | `string` | No | `productRef` |
| `ServiceDefinitionVersion` (new) | `int` | No | `productVersion` |
| `DocumentType` (new) | `ElectronicMiscDocumentType` | No | `document.type`: `EmdAssociated` → `Associated` |
| `ReasonForIssuanceCode` (new) | `string` | No | `document.rfic` |
| `ServiceSubCode` | `string` | No | `document.rfisc` |
| `ServiceTypeCode` | `string` | No | `codes.serviceTypeCode` |
| `GroupCode` | `string` | No | `codes.groupCode` |
| `SubGroupCode` | `string?` | Yes | `codes.subGroupCode` |
| `CommercialName` | `string` | No | `name` |
| `SsrCode` | `string?` | Yes | always null in Phase 1 |
| `Quantity` | `decimal` | No | `quantity` |
| `UnitCode` | `string` | No | `unit` |
| `CoveredAirServiceIds` | list of `long` | No | the traveller's Active `OrderAirTransportService` on the segment named by `coveredFlightRefs`; in Phase 1 exactly one |
| `Refundability` | `RefundabilityRule` | No | `terms.refundable`: `true` → `Refundable`, `false` → `NonRefundable` |
| `Reusable` | `bool?` | Yes | `terms.reusable` |
| `FormOfRefundCode` | `string?` | Yes | `terms.formOfRefundCode` |
| `Commissionable` | `bool?` | Yes | `terms.commissionable` |
| `InterlineSettlementAllowed` | `bool?` | Yes | `terms.interlineSettlementAllowed` |

`OrderBaggageService` adds: `Pieces` (`int?`), `Weight` (`decimal?`), `WeightUnit` (`BaggageWeightUnit?`: `Kg` → `Kg`, `Lbs` → `Lbs`) from `baggage.*`. They describe **one unit**; the purchased amount is `Quantity`. `BaggageCategoryCode` and `PrepaidIndicator` stay null. `codes.description1Code` and `description2Code` are not stored; the sub code identifies them.

In Phase 1 the nullable fields of Master §6.7 that are marked "No" above are required because the service always comes from Ancillary.

### 3.3 The `Ancillary` reservation provider

Add the provider key `Ancillary` to `FulfillmentProviderKeys` and register one more `IReservationProvider` with that key, next to FlightFlow's. Every ancillary service uses it, so every ancillary sale goes through Reserve and Confirm at Ancillary (Ancillary Master §8).

Capability, for any ancillary service:

| Member | Value |
|---|---|
| `Mode` | `HoldThenConfirm` |
| `BatchResultMode` | `AtomicAllOrNothing` |
| `PreConfirmationReleaseScope` | `Operation` |
| `PostConfirmationCancelScope` | `Unit` |
| `SupportsExtend`, `SupportsSplit` | `false` |
| `ProvidesUnitReference` | `true` |
| `SupportsReadBack` | `true` |
| `ExpiresAutomatically` | `true` |
| `SupportsSafeConfirmReplay` | `true` |

The adapter's calls are in §6.

### 3.4 `ElectronicMiscDocument`, `EmdCoupon`, `EmdPriceLink`

Materialized as Ordering's Master §17 defines them, as their own aggregate (not inside the ticket aggregate, and with their own statuses), with these Phase-1 values. `EmdCoupon.IssuanceValue` stays a required decimal.

| Aggregate field | Phase-1 value |
|---|---|
| `OriginalOrderId`, `CurrentServicingOrderId` | the order |
| `TravellerId`, `TravellerProfileRevisionId` | those of the associated ticket (§7.2) — never the traveller's current profile revision |
| `IssueFulfillmentTaskId` | the task of §6 |
| `DocumentNumber` | from the EMD document stock |
| `Type` | the service's `DocumentType` (`Associated`) |
| `ReasonForIssuanceCode` | the service's `ReasonForIssuanceCode` |
| `IssuanceContext` | built exactly as for a ticket in `IssueOrderService.IssuanceContextOf` |
| `IssuedAt` | the clock |
| `IssuedTotal` | the document total of §7.3: base value plus taxes. It is not the coupon value when the service has tax lines. |
| `CurrencyId` | the order's currency |
| `ProviderReference`, `PredecessorEmdId` | null |
| `StatusSummary` | `Issued` |
| `DocumentVersion` | 1 |
| `RefundRecords`, `ExchangeRecords` | empty |

Coupon and price links: §7.3, §7.4.

---

## 4. Listing what can be added

`GET Backoffice/v1/Orders/{orderId}/ServiceList`

1. Load the order; apply the same authorization as the other Backoffice order operations. Unknown order → `2500`.
2. The order's status must be `Ticketed`; otherwise `2901`.
3. Build the context (§4.1) and call AirOffer `POST Service/v1/AncillaryOffers`. Return its `data` — the service list of `reference/Ancillary-Edge-Contract.md` §2 — unchanged.
4. AirOffer unreachable, timed out, or answering 5xx → `2905`. AirOffer answering 400 with one of its codes `6028`–`6033` → `2904`, carrying that code and message.

Nothing is stored.

### 4.1 Building the context

| Request field | Value |
|---|---|
| `currencyId` | `Order.CurrencyId` |
| `asOf` | the clock, at the moment of the call |
| `salesContext` | `channel` = the name of `Order.SalesContext.Channel`; `travelAgencyId` = `Order.SalesContext.TravelAgencyId`; `customerId` and `countryId` null (the order's sales context does not hold them) |
| `bounds[]` | every journey of the order that has at least one segment with an Active air service: `ref` = journey id as text; in journey sequence |
| `bounds[].flights[]` | every segment of that journey with at least one Active air service: `ref` = segment id as text; `flightId`, `originAirportId`, `destinationAirportId`, `marketingAirlineId`, `operatingAirlineId`, `aircraftId` from the segment; `departureDateTime` = `SoldDeparture`; `flightCapacityId`, `cabinClassId`, `rbdId` omitted; in segment sequence |
| `travellers[]` | every Active traveller with at least one Active air service: `ref` = traveller id as text; `passengerTypeCode` = the name of his `PassengerType`; `flightRefs` = the segments on which he has an Active air service; in traveller index order |
| `existing[]` | every Active `OrderAncillaryService` of the order: `productRef` = `ServiceDefinitionRef`, `travellerRef`, `boundRef` = the journey of its covered air services, `flightRef` null, `quantity` = `Quantity`. When the context is built for a Reserve call (§6), the services being reserved in that call are left out. |

---

## 5. `AddAncillaryFromOffer`

`POST Backoffice/v1/Orders/{orderId}/Services`

### 5.1 Request

The shape of `reference/Ancillary-Edge-Contract.md` §4:

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

| Field | Rule |
|---|---|
| `requestKey` | Required, 1–64 characters, chosen by the caller, unique per attempt to add. |
| `offerRefId` | Required. |
| `selectedOfferItems` | Required, at least one. No `offerItemRefId` twice. |
| `quantity` | `≥ 1`. |

A malformed request is HTTP 400. Ordering does not read the ids; they are opaque to it.

### 5.2 Steps

All steps run under the same order lock that `IssueOrderService` takes (`IReservationLock.AcquireAsync(orderId)`).

1. Load the order (`2500` if unknown); authorize.
2. **Replay.** If an `OrderChange` of this order has `SourceSystem = "AddAncillary"` and `SourceReference = requestKey`, compare the request with the services that change created: every selected item must match exactly one of those services, and every one of those services exactly one selected item, on `offerItemRefId` = `AcceptedOfferItemId` and `quantity` = `Quantity`. If they match, return the result of that change (§5.3) without calling anyone and without changing anything; otherwise `2907`.
3. Status must be `Ticketed` (`2901`).
4. No unresolved fulfillment task of the order of type `IssueTicket`, `IssueEmd`, `VoidTicket`, `CancelConfirmed`, `ReleaseReserved`, `ReserveInventory` or `ConfirmInventory` (`2778`).
5. Build the context of §4.1, add the request's `acceptSelectedOffer`, and call AirOffer `POST Service/v1/AncillaryOffers/Details` once.
6. AirOffer failure → `2905`. AirOffer refusing with `6028`–`6033` (for example an expired offer or an item that is no longer available) → `2904` with that code and message.
7. **Check the answer.** It must contain exactly one item per selected item, in the same order, with the same `offerItemId` and `quantity`; `currencyId` must be the order's currency; each item's `travellerRef` must be an Active traveller of the order; each `coveredFlightRefs` must have exactly one entry, a segment on which that traveller has an Active air service; `type` must be `ExtraBaggage`; `document.type` must be `EmdAssociated`; `total` must equal the sum of the item's line amounts. Anything else → `2906`.
8. **Commit**, as one unit of work:
   - one `OrderChange`: `ChangeType = AddProduct`, `CommercialVersion = Order.CommercialVersion + 1`, `ActorContext` = the caller's sales context (as `CancelOrder` does), `SourceSystem = "AddAncillary"`, `SourceReference = requestKey`, `ReasonCode`, `ReasonText`, `WaiverCode` null, `IsInvoluntary = false`, `CommittedAt` = the clock;
   - per item, in order: one `OrderItem` (`Kind = ProductType.Baggage`, `AcceptedTotal = total`, Active, created by the change) and one `OrderBaggageService` (§3.2);
   - per item line: one `PricingLine` and one allocation (§5.4);
   - `Order.CommercialVersion` = the change's version; `Order.CustomerTotal` increases by the sum of the items' `total`;
   - `Order.Status` is **not** changed: it stays `Ticketed`;
   - the read model is projected and `OrderAncillaryAdded` (§8) is written to the outbox in the same unit of work.

Either all of step 8 is stored or none of it. A failure in steps 1–7 stores nothing.

After this command the services exist on the order but are not yet reserved at Ancillary. They are reserved and confirmed by §6, and only then documented by §7.

### 5.3 Result

```json
{
  "orderId": "9001", "orderChangeId": "9100", "commercialVersion": 3,
  "orderStatus": "Ticketed", "currencyId": 978, "customerTotal": 538.50,
  "services": [
    { "orderItemId": "9101", "orderServiceId": "9102", "travellerId": "7001", "journeyId": "8001",
      "acceptedOfferItemId": "A1~978~1790000000~XBAG1~1~501~7001~B~8001",
      "rfic": "C", "rfisc": "0CC", "name": "First extra bag 23kg",
      "quantity": 1, "total": 38.50, "coveredAirServiceIds": ["6001"] }
  ]
}
```

The amounts in the example are illustrations.

### 5.4 Pricing lines

For each `priceLines[]` entry of an item, one `PricingLine` created by the change:

| `PricingLine` field | `Ancillary` line | `Tax` line |
|---|---|---|
| `Reason` | `AddService` | `AddService` |
| `Scope` | `OrderService` | `OrderService` |
| `Category` | `Ancillary` | `Tax` |
| `SubCategory` | `Ancillary` | `Tax` |
| `Direction` | `Credit` | `Credit` |
| `Treatment` | `CustomerPrice` | `CustomerPrice` |
| `Code` | `code` | `code` |
| `Description` | `name` | `name` |
| `Reference` | `priceRuleId` | `priceRuleId` |
| `Amount`, `EquivalentAmount` | `amount` | `amount` |
| `CurrencyId`, `EquivalentCurrencyId` | the order's currency | the order's currency |
| `ExchangeRateSnapshot` | null | null |
| `Refundability` | the service's `Refundability` | the service's `Refundability` |

Each line has exactly one allocation, for its full amount, to the new `OrderBaggageService`. A `Tax` line is never changed into `VAT` from its code or name. No fare pricing unit, fare component or fare pricing atom is created.

### 5.5 Duplicates

- The same `requestKey` again → the first result, no second sale (step 2).
- A different `requestKey` for something already on the order → AirOffer no longer offers it and refuses the old item (`2904`), because the context carries what the order already holds; and Ancillary refuses it again at Reserve. Ordering adds no rule of its own.

---

## 6. Reserving and confirming at Ancillary

No new command. The existing commands are used:

| Step | Existing operation |
|---|---|
| Reserve the new ancillary services | `POST Backoffice/v1/Orders/{orderId}/Reservations/Services` with their service ids |
| Confirm | `POST Backoffice/v1/Orders/{orderId}/Reservations/Confirmations` |
| Give up before confirming | `POST Backoffice/v1/Orders/{orderId}/Reservations/{reservationId}/Release` |

`ReserveService` groups the services by provider key and mode, so the ancillary services of one call become one `FulfillmentReservation` with provider `Ancillary`.

### 6.1 Two guards that change

| Guard today | Change |
|---|---|
| `Order.EnsureReservable()` refuses a `Ticketed` order. | When **every** service selected for the call is an `OrderAncillaryService`, `Ticketed` is also accepted. For any other selection the guard is unchanged. |
| `Order.EnsureNewReservationAllowedAt(now)` refuses after the last ticketing date. | Not applied when the order is `Ticketed` and every service to reserve is an `OrderAncillaryService`. |

The order's root status does not change through reserving or confirming an ancillary on a `Ticketed` order (`SummarizeReservation` already leaves a non-reservable status alone).

### 6.2 The adapter

| `IReservationProvider` member | Behaviour |
|---|---|
| `CapabilityFor` | §3.3 |
| `PlanUnits` | one unit per ancillary service: `UnitCorrelationKey` = the service id as text; `OrderServiceIds` = that service |
| `PrepareAsync` | `RequestedExpiresAt` = the clock + `Fulfillment:AncillaryHoldMinutes` (new required option; the host does not start without it); `ValidationEvidence` = null — Ancillary validates inside Reserve |
| `ReserveRequestFor` / `ReserveAsync` | `POST {Ancillary}/Service/v1/ServiceReservations` (Ancillary Master §8.3): `idempotencyKey` and `reference` from the intent; `expiresAt` = `RequestedExpiresAt`; `context` = §4.1 with `asOf` = the clock and `existing` without the services of this call; one `unit` per planned unit: `unitReference` = `UnitCorrelationKey`, `productRef` = `ServiceDefinitionRef`, `productVersion` = `ServiceDefinitionVersion`, `priceRuleId` = the `Reference` of the service's pricing lines, `travellerRef` = traveller id, `boundRef` = the journey of the covered air service, `flightRef` null, `quantity` = `Quantity` |
| `ReadRequestFor` / `ReadAsync` | `GET …/ServiceReservations/{ProviderOperationRef}` |
| `ConfirmRequestFor` / `ConfirmAsync` | `POST …/ServiceReservations/{ProviderOperationRef}/Confirmations` |
| `ReleaseRequestFor` / `ReleaseAsync` | `POST …/ServiceReservations/{ProviderOperationRef}/Releases` |
| `CancelConfirmedRequestFor` / `CancelConfirmedAsync` | `POST …/ServiceReservations/{ProviderOperationRef}/Cancellations` with the units' `ProviderUnitRef` as `unitRefs` (not used in Phase 1; implemented for completeness of the interface) |

Mapping Ancillary's answer to the outcome:

| Ancillary | Outcome |
|---|---|
| reservation returned | success; `ProviderOperationRef` = `reservationId`; `EchoedIdempotencyKey`, `EchoedCorrelationReference` = `idempotencyKey`, `reference`; `ExpiresAt`; per unit `UnitCorrelationKey` = `unitReference`, `ProviderUnitRef` = `unitRef`, status by name (`Held`, `Confirmed`, `Released`, `Expired`, `Cancelled`) |
| a unit's `total` differs from the service's accepted total (the sum of its Active customer-price pricing lines) | definitive failure of the operation; the adapter releases the reservation it just received; the services stay unreserved |
| HTTP 409 or 422 with an Ancillary code (`16301`–`16309`, `16405`–`16412`) | definitive refusal, carrying the code and message |
| HTTP 400 without a code | definitive refusal: Ordering built a malformed request (a defect) |
| 5xx, timeout, no connection | unknown outcome; the existing recovery applies — read back by `ProviderOperationRef` when known, otherwise repeat the reserve with the same idempotency key, which is safe |

Configuration: `Ancillary:BaseUrl` (required), `Fulfillment:AncillaryHoldMinutes` (required).

A refusal at Reserve means the accepted item is no longer valid at Ancillary (changed version or price, quantity already taken). The services stay on the order, unreserved and undocumented, with the provider's refusal recorded by the existing reservation flow. Removing them is the cancel-ancillary operation, which is not part of Phase 1; until then they are simply never documented.

---

## 7. `IssueAncillaryDocument`

`POST Backoffice/v1/Orders/{orderId}/Ancillaries/Issuance`

Request: `{ "emdDocumentStockId": "5001" }`

It issues one EMD-A, with one coupon, for **every** outstanding ancillary service of the order, in one all-or-nothing operation, following the local ticket-issue pattern step for step. Ancillary and AirOffer are not called.

**Outstanding ancillary service:** an Active `OrderAncillaryService` with `DocumentType = Associated`, whose latest reservation unit at Ancillary is `Confirmed` (§6), and for which no `ElectronicMiscDocument` with `StatusSummary = Issued` has a coupon whose `CurrentOrderServiceId` is that service. A service that is not yet confirmed at Ancillary is never documented.

### 7.1 Steps

1. Acquire the order lock; load the order (`2500`); authorize.
2. Determine the outstanding ancillary services, in order of creation.
   - None, and the order has EMDs: return the existing EMDs with the task id of the latest one (replay; nothing changes).
   - None, and no EMD exists: `2908`.
3. No unresolved fulfillment task of the order of type `IssueEmd`, `IssueTicket`, `VoidTicket` or `CancelConfirmed` (`2778`).
4. For each outstanding service, find the ticket coupon of its covered air service and the passenger identity of the EMD (§7.2). Any failure → `2909`; nothing is issued for any service.
4a. For each outstanding service, compute its values (§7.3). A service whose pricing cannot be reconciled → `2911`; nothing is issued for any service.
5. Load the stock (`DocumentStockNotFound`); `stock.EnsureCanIssue(AccountableDocumentKind.ElectronicMiscDocument, number of outstanding services)`.
6. Acquire the stock lock; reload the stock; repeat step 5's check.
7. Create one `FulfillmentTask`: `TaskType = IssueEmd`, `FulfillmentProviderKey = LocalDocumentAuthority`, no reservation, idempotency key `issue-emd:{taskId}`, correlation reference `order:{orderId}:issue-emd:{taskId}`, targets = the outstanding services (`FulfillmentTargetKind.OrderService`, action `Issue`); start its attempt.
8. Per service, in order: `stock.Allocate(taskId, "EMD:{orderServiceId}", …)`. If any allocated number already belongs to an EMD → `DocumentNumberIsAlreadyIssued`.
9. Per service: create the `ElectronicMiscDocument` (§3.4) with its coupons and price links (§7.3, §7.4).
10. Mark each allocation issued; add the task targets `DocumentStockAllocation`, `ElectronicMiscDocument`, `EmdCoupon` (action `Issue`); complete the attempt as `Succeeded`.
11. Store the task and the EMDs, project the read models of the EMDs and the stock, and save — one unit of work. Each EMD raises its issued event, published as `ElectronicMiscDocumentIssued` (§8).

The order's root status and `CommercialVersion` do not change. The last-ticketing-date check of ticket issue is not applied.

A failure before step 11 stores nothing: no task, no allocation, no EMD.

### 7.2 Which ticket coupon, and whose passenger identity

An outstanding service must have exactly one covered air service; otherwise `2911`. For that air service there must be **exactly one** ticket coupon with all of:

| Condition | Value |
|---|---|
| `CurrentOrderServiceId` | the covered air service |
| the ticket's `TravellerId` | the ancillary service's traveller |
| `FinancialStatus` | `Open` |
| `ControlStatus` | `Local` |

No such coupon, or more than one → `2909`. A ticket of another traveller never qualifies. This is the Phase-1 rule for locally issued documents. It is narrower than what IATA allows (IATA also permits association with a checked-in coupon, for example); widening it is a later decision.

**Passenger identity of the EMD.** An EMD-A carries the passenger of the ticket it is associated with, as he was when that ticket was issued: the EMD's `TravellerId` and `TravellerProfileRevisionId` are those of the ticket that owns the coupon above. The traveller's `CurrentProfileRevisionId` is never used for an EMD-A, even when it differs from the ticket's revision.

**Carrier of the coupon.** Wherever the EMD-A coupon needs the operating or marketing carrier of its flight, the value comes from the associated ticket coupon's `IssuedSegment` snapshot — never from the order's current segment or from FlightFlow.

### 7.3 The coupon and the amounts

**The three values of a service**, computed from its Active pricing lines with `Treatment = CustomerPrice` that are allocated to it:

| Value | Definition |
|---|---|
| `ServiceBaseValue` | the sum of the lines with `Category = Ancillary` |
| `ServiceTaxValue` | the sum of the lines with `Category = Tax` |
| `DocumentTotal` | `ServiceBaseValue + ServiceTaxValue` |

If the service has an Active customer-price line of any other category, or a line that is not allocated in full to this service, the values cannot be reconciled and the issue is refused (`2911`).

The EMD has exactly one `EmdCoupon`:

| Field | Value |
|---|---|
| `CouponNumber` | 1 |
| `Purpose` | `Service` |
| `OriginalOrderServiceId`, `CurrentOrderServiceId` | the ancillary service |
| `PricingLineId` | null |
| `ReasonForIssuanceSubCode` | the service's `ServiceSubCode` |
| `ServiceSubCode` | the same value |
| `AssociatedTicketCouponId` | the coupon of §7.2 |
| `IssuanceValue` | `ServiceBaseValue` |
| `CurrencyId` | the order's currency |
| `Status` | `OpenForUse` |
| `ProviderCouponStatusCode`, `PredecessorEmdCouponId`, `ExternalValueReference` | null |

and `IssuedTotal = DocumentTotal`. One `EmdAssociationHistory` entry: `Action = Associate`, the ticket coupon, the change that created the service, `OccurredAt` = issue time.

The coupon value is the base value of the service; it never contains tax. Taxes belong to the document. Example — `Ancillary` 35.00 and `Tax` 3.50: coupon value 35.00, `IssuedTotal` 38.50, and the tax is a document-level price link of 3.50. With no tax line: the coupon value and `IssuedTotal` are both 35.00.

**Basis.** IATA's guide derives an EMD's base fare amount either from its fare calculation or from the sum of its coupon values, the two being mutually exclusive, and keeps taxes and the total document amount as separate elements. With one coupon, the coupon value is the base amount and nothing has to be split or placed.

### 7.4 Price links

One `EmdPriceLink` per pricing line of the service, with `PricingLineId`, `PricingAllocationId` = that line's allocation, `AttributedValue` = the line's amount, `CurrencyId`:

| Pricing line | `EmdCouponId` |
|---|---|
| `Category = Ancillary` | the coupon |
| `Category = Tax` | null — a document-level link |

Both reconciliations must hold exactly, otherwise the issue is refused (`2911`):

- the sum of all links' `AttributedValue` = the EMD's `IssuedTotal`;
- the sum of the coupon-linked links' `AttributedValue` = the coupon's `IssuanceValue`.

### 7.5 Result

```json
{
  "orderId": "9001", "orderStatus": "Ticketed", "issueFulfillmentTaskId": "9200",
  "documents": [
    { "electronicMiscDocumentId": "9201", "orderServiceId": "9102", "travellerId": "7001",
      "documentNumber": "0001234567890", "type": "Associated", "reasonForIssuanceCode": "C",
      "status": "Issued", "issuedAt": "2026-10-02T10:00:00+00:00", "issuedTotal": 38.50, "currencyId": 978,
      "coupons": [
        { "emdCouponId": "9202", "couponNumber": 1, "reasonForIssuanceSubCode": "0CC",
          "associatedTicketCouponId": "4001", "issuanceValue": 35.00, "status": "OpenForUse" } ],
      "priceLinks": [
        { "pricingLineId": "9110", "emdCouponId": "9202", "attributedValue": 35.00 },
        { "pricingLineId": "9111", "emdCouponId": null, "attributedValue": 3.50 } ] }
  ]
}
```

---

## 8. Integration events

| Event | When | Contract |
|---|---|---|
| `OrderAncillaryAdded` | step 8 of §5.2 | **new**, in `AeroTech.Messages.Ordering.IntegrationEvents.V1`, extending `BaseIntegrationEvent` |
| `ElectronicMiscDocumentIssued` | step 11 of §7.1, once per EMD | **existing** contract, used unchanged |

`OrderAncillaryAdded`: `OrderId`, `OrderChangeId`, `CommercialVersion`, `CurrencyId`, `CustomerTotalAfter`, `CommittedAt`, `Services[]` — each `OrderItemId`, `OrderServiceId`, `TravellerId`, `ServiceDefinitionRef`, `ServiceDefinitionVersion`, `ServiceSubCode`, `Quantity`, `Total`, `CoveredAirServiceIds[]`.

`ElectronicMiscDocumentIssued` values: `ElectronicMiscDocumentId`; `OrderId`; `TravelerId` = the EMD's traveller; `OperationId` = `IssueFulfillmentTaskId` (as the ticket event does); `DocumentNumber`; `Type`; `ReasonForIssuanceCode`; `IssuerCarrierId`, `IssuingOfficeId` from the issuance context; `Authority = Local`; `CurrencyId`; `IssuedTotal`; `DocumentVersion`; `Coupons[]` — `EmdCouponId`, `CouponNumber`, `Purpose`, `OrderServiceId` = `CurrentOrderServiceId`, `AssociatedTicketCouponId`, `IssuanceValue`.

Both are written through the existing transactional outbox, in the same unit of work as the data they describe. A replay (§5.2 step 2, §7.1 step 2) publishes nothing.

---

## 9. Guard on existing operations

Cancelling an ancillary and voiding an EMD are not part of Phase 1. Until they exist, `CancelOrder` and `VoidElectronicTickets` are refused with `2910` when the order has an Active `OrderAncillaryService`, so that no bag or EMD is left attached to a cancelled flight or a voided ticket.

---

## 10. New error codes

Block `29xx` is unused at `cd50a2a`.

| Code | Name | HTTP | Raised when |
|---|---|---:|---|
| 2901 | `OrderIsNotTicketedForAncillary` | 409 | the order's status is not `Ticketed` (§4, §5) |
| 2904 | `AncillaryOfferWasRejected` | 422 | AirOffer refused with one of its codes `6028`–`6033`; the message carries that code and text |
| 2905 | `AncillaryOfferProviderIsUnavailable` | 503 | AirOffer could not be reached or answered 5xx (including its `6034`) |
| 2906 | `AncillaryOfferAnswerIsInconsistent` | 502 | the answer fails a check of §5.2 step 7 |
| 2907 | `AncillaryRequestKeyWasUsedForAnotherRequest` | 409 | same `requestKey`, different selection |
| 2908 | `OrderHasNoAncillaryToIssue` | 409 | no outstanding ancillary service and no EMD |
| 2909 | `AncillaryTicketCouponIsNotEligible` | 409 | §7.2 fails for the covered air service |
| 2910 | `OrderHasActiveAncillary` | 409 | §9 |
| 2911 | `AncillaryDocumentCannotBeFormed` | 409 | the service does not have exactly one covered air service, or the reconciliation of §7.3 / §7.4 fails |

Codes 2902 and 2903 are not used. Existing codes reused as they are: `2500`, `2505`, `2778`, `DocumentStockNotFound`, `DocumentStockKindMismatch` (2786), `DocumentNumberIsAlreadyIssued`, and the existing reservation errors.

---

## 11. Expected behaviour

Each row is one test when tests are written. Amounts: `Ancillary` 35.00, `Tax` 3.50 unless a row says otherwise.

### Listing and adding

| # | Behaviour |
|---|---|
| A01 | List on a ticketed order → AirOffer's service list returned unchanged. |
| A02 | Add one selected bag item → one item of kind `Baggage`, one `OrderBaggageService` of type `BaggageCharge` with provider `Ancillary`, `AcceptedOfferItemId` stored; pricing lines with reason `AddService`; `CommercialVersion + 1`; status `Ticketed`; `OrderAncillaryAdded` published once. No call to Ancillary. |
| A03 | The same `requestKey` again → the same result; nothing new stored or published. The same `requestKey` with another selection → `2907`. |
| A04 | AirOffer refuses (`6030` expired, `6031` no longer available, `6032` quantity) → `2904` carrying that code; the order is unchanged. |
| A05 | AirOffer answers with a different quantity, another `offerItemId`, another currency, an item count that differs from the selection, or an item covering more than one flight → `2906`; nothing stored. |
| A06 | Order not `Ticketed` → `2901`. Unresolved fulfillment task → `2778`. AirOffer down → `2905`. |
| A07 | `CancelOrder` or a ticket void on an order with an Active ancillary service → `2910`. |

### Reserving and confirming

| # | Behaviour |
|---|---|
| R01 | `Reservations/Services` with the new bag service on a `Ticketed` order → one `FulfillmentReservation` with provider `Ancillary`, unit `Held`; Ancillary received the unit with the service's product reference, version, price rule, traveller, journey and quantity, and a context whose `existing` does not contain this service. Order status still `Ticketed`. |
| R02 | `Confirmations` → unit `Confirmed`. Order status still `Ticketed`. |
| R03 | The same call selecting an air service together with the ancillary on a `Ticketed` order → refused as today (`OrderIsNotReservable`). |
| R04 | Ancillary refuses with `16309` (the rule was replaced after the add) → the reservation is recorded as rejected with that code; the service stays Active and unreserved; it is not outstanding for §7. |
| R05 | Ancillary returns a unit whose `total` differs from the service's accepted total → the adapter releases that reservation; the operation is a definitive failure. |
| R06 | Timeout on Reserve, then a retry → the same reservation at Ancillary (same idempotency key); one `FulfillmentReservation`. |
| R07 | The hold is not confirmed before `Fulfillment:AncillaryHoldMinutes` → the existing deadline poller releases it; the service is unreserved again and can be reserved anew. |
| R08 | An order without ancillary services reserves, confirms and issues exactly as before. |

### Issuing

| # | Behaviour |
|---|---|
| E01 | Confirmed bag, one covered flight, no tax line → one coupon, value 35.00; `IssuedTotal` 35.00; one price link (coupon-linked). |
| E02 | Confirmed bag with tax → one coupon, value 35.00; `IssuedTotal` 38.50; price links: 35.00 on the coupon, 3.50 with no coupon. |
| E03 | A bag service that is on the order but not confirmed at Ancillary → not outstanding: nothing is issued for it; with no other outstanding service and no EMD → `2908`. |
| E04 | A service with a customer-price line of another category, a line not fully allocated to it, or more than one covered air service → `2911`; nothing issued. |
| E05 | The ticket's `TravellerProfileRevisionId` equals the traveller's current revision → issued with that revision. |
| E06 | The traveller's current revision is newer than the ticket's → the EMD carries the ticket's revision, not the current one. |
| E07 | The EMD's coupon carrier facts are those of the ticket coupon's `IssuedSegment`, even after the order's segment has changed. |
| E08 | The only `Open`, `Local` coupon for the covered air service belongs to a ticket of another traveller, or the coupon is `Used`, `Void`, `Exchanged`, `Refunded`, `Suspended`, or its control is not `Local` → `2909`; nothing issued. |
| E09 | Two outstanding services, the second fails any check → nothing issued for either; no stock number consumed. |
| E10 | Issue again when nothing is outstanding → the existing EMDs are returned; nothing new. |
| E11 | The published `ElectronicMiscDocumentIssued` carries `IssuedTotal` 38.50 and one coupon with value 35.00; `OperationId` = the issue task. |
| E12 | Issuing calls neither AirOffer nor Ancillary, and changes neither the order's status nor its `CommercialVersion`. |

---

## 12. What the Owner signs off with this spec

These are changes to Ordering's own model and Master; approving this spec approves them.

1. The three enum members of §3.1 and the provider key `Ancillary`.
2. The new fields `AcceptedOfferItemId`, `ServiceDefinitionVersion`, `DocumentType`, `ReasonForIssuanceCode` on the ancillary service, and `ResponsibleAirlineId` materialized on the base service.
3. Every ancillary service is reserved and confirmed at Ancillary before it is documented; an unconfirmed service is never documented.
4. The two guard changes of §6.1, so that an ancillary can be reserved on a `Ticketed` order.
5. The order's root status staying `Ticketed` when an ancillary is added, reserved, confirmed and documented.
6. Phase 1 documents only a bag that covers one flight: one EMD-A, one coupon, coupon value = base value, taxes at document level.
7. `Open` + `Local` as the only eligible ticket-coupon state in Phase 1.
8. The guard of §9.
9. The new event `OrderAncillaryAdded`, and the channel shapes of `reference/Ancillary-Edge-Contract.md` for listing and adding.

The prompt for this work is `handover/Ordering/prompt.md`.
