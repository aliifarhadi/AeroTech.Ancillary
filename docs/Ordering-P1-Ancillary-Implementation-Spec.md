# Ordering — Phase 1 Ancillary Implementation Spec (companion to Ancillary R5.3)

**Date:** 2026-10-02
**For:** the Ordering service. It states exactly what Ordering builds so that an extra bag can be added to an existing ticketed order and its EMD-A issued. It is a companion contract: it becomes binding for Ordering when the Owner adds it to Ordering's own authority set.
**Checked against:** `AeroTech.Ordering.Final` `cd50a2a` (head of `k8s-stg` on 2026-10-02); Ancillary `Ancillary-Domain-Master.md` R5.3; IATA Airline Guide to EMD Implementation.
**Names:** the command, entity and field names are the ones Ordering's Master v2.0 already uses (§6.4, §6.7, §6.8, §17, §18, §26).

Every choice below is fixed. Whoever implements it takes no domain decision; if something is missing or contradicts Ordering's source, stop and ask the Owner.

**Phase-1 limitation.** An extra bag is sold and documented only when it covers exactly one flight, so its EMD-A has exactly one coupon. Ancillary enforces this in the quote (Master §7.2); this spec relies on it and checks it. A bag over several flights needs a way for an EMD to carry a fee that spans several coupons — a fare calculation or an authoritative proration — which Ordering does not model; it is deferred until that is designed, and no convention for placing or splitting the amount is used meanwhile.

Payment is outside this spec, as it is outside Ordering's core: the order, the service and the EMD hold no payment state. Whatever financial approval a production sale needs is enforced by the orchestration that calls these commands.

---

## 1. Scope

| In Phase 1 | Not in Phase 1 |
|---|---|
| Listing the ancillaries that can be added to an order | Selling before the order exists (Phase 3) |
| `AddAncillaryFromOffer` — adding extra baggage to a ticketed order | Reserving an ancillary (`ReserveAncillary`, Phase 4) |
| `IssueAncillaryDocument` — issuing the EMD-A locally | Cancelling an ancillary, voiding, refunding or exchanging an EMD; extra baggage that covers more than one flight |
| Backoffice surface only | OTA, OtaPanel, IBE surfaces; EMD-S; payment |

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

`OrderAncillaryService` is an abstract `OrderService`. `OrderBaggageService` is the concrete type for `ExtraBaggage`. One accepted quote item becomes **one `OrderItem` and one `OrderBaggageService`**.

Base `OrderService` values:

| Field | Value |
|---|---|
| `TravellerId` | the selection's traveller |
| `ServiceType` | `BaggageCharge` |
| `FulfillmentProviderKey` | `LocalDocumentAuthority` (because `inventory.control = Unlimited`) |
| `CommercialStatus` | `Active` |
| `CreatedByChangeId` | the change of §5 |
| `ResponsibleAirlineId` (`int?`, Master §6.4, materialized now on the base type; null for existing services) | `ownerAirlineId` |

`OrderAncillaryService` fields (all set once, at creation, from the quote item; never changed):

| Field | Type | Null | Source |
|---|---|---:|---|
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

### 3.3 No-reservation provider

Register a reservation provider with `ProviderKey = LocalDocumentAuthority` whose capability for any service is `ReservationMode.None`. Its hold, confirm, release and cancel operations are never called; if one is, it throws. Effect: `RequiresReservation` is false for an ancillary service, `ReserveOrder` skips it, and nothing in the reservation flow changes for air services.

### 3.4 `ElectronicMiscDocument`, `EmdCoupon`, `EmdPriceLink`

Materialized as Ordering's Master §17 defines them, as their own aggregate (not inside the ticket aggregate, and with their own statuses), with these Phase-1 values. `EmdCoupon.IssuanceValue` stays a required decimal.

| Aggregate field | Phase-1 value |
|---|---|
| `OriginalOrderId`, `CurrentServicingOrderId` | the order |
| `TravellerId`, `TravellerProfileRevisionId` | those of the associated ticket (§6.2) — never the traveller's current profile revision |
| `IssueFulfillmentTaskId` | the task of §6 |
| `DocumentNumber` | from the EMD document stock |
| `Type` | the service's `DocumentType` (`Associated`) |
| `ReasonForIssuanceCode` | the service's `ReasonForIssuanceCode` |
| `IssuanceContext` | built exactly as for a ticket in `IssueOrderService.IssuanceContextOf` |
| `IssuedAt` | the clock |
| `IssuedTotal` | the document total of §6.3: base value plus taxes. It is not the coupon value when the service has tax lines. |
| `CurrencyId` | the order's currency |
| `ProviderReference`, `PredecessorEmdId` | null |
| `StatusSummary` | `Issued` |
| `DocumentVersion` | 1 |
| `RefundRecords`, `ExchangeRecords` | empty |

Coupons and price links: §6.4.

---

## 4. Listing what can be added

`GET Backoffice/v1/Orders/{orderId}/AncillaryOffers`

1. Load the order; apply the same authorization as the other Backoffice order operations. Unknown order → `2500`.
2. The order's status must be `Ticketed`; otherwise `2901`.
3. Build the quote request (§4.1) without `selections`, call AirOffer `POST Service/v1/OrderAncillaryOffers`, and return its items unchanged, in its order.
4. AirOffer unreachable, timed out or answering with a server error → `2905`. A business error of Ancillary → `2904`, carrying the upstream code and message.

Nothing is stored.

### 4.1 Building the quote request

| Request field | Value |
|---|---|
| `currencyId` | `Order.CurrencyId` |
| `asOf` | the clock, at the moment of the call |
| `salesContext` | `channel` = the name of `Order.SalesContext.Channel`; `travelAgencyId` = `Order.SalesContext.TravelAgencyId`; `customerId` and `countryId` null (the order's sales context does not hold them) |
| `bounds[]` | every journey of the order that has at least one segment with an Active air service: `ref` = journey id as text; in journey sequence |
| `bounds[].flights[]` | every segment of that journey with at least one Active air service: `ref` = segment id as text; `flightId`, `originAirportId`, `destinationAirportId`, `marketingAirlineId`, `operatingAirlineId`, `aircraftId` from the segment; `departureDateTime` = `SoldDeparture`; `flightCapacityId`, `cabinClassId`, `rbdId` omitted; in segment sequence |
| `travellers[]` | every Active traveller with at least one Active air service: `ref` = traveller id as text; `passengerTypeCode` = the name of his `PassengerType`; `flightRefs` = the segments on which he has an Active air service; in traveller index order |
| `existing[]` | every Active `OrderAncillaryService`: `productRef` = `ServiceDefinitionRef`, `travellerRef`, `boundRef` = the journey of its covered air services, `flightRef` null, `quantity` = `Quantity` |

---

## 5. `AddAncillaryFromOffer`

`POST Backoffice/v1/Orders/{orderId}/Ancillaries`

### 5.1 Request

```json
{
  "requestKey": "3f6c1c2e-5a0b-4d0e-9a55-7d0b8a1e2f10",
  "selections": [
    { "productRef": "XBAG1", "productVersion": 1, "priceRuleId": "501",
      "travellerId": "7001", "journeyId": "8001", "quantity": 1 }
  ]
}
```

| Field | Rule |
|---|---|
| `requestKey` | Required, 1–64 characters, chosen by the caller, unique per attempt to add. |
| `selections` | Required, at least one. No two with the same `productRef + travellerId + journeyId`. |
| `productRef`, `productVersion`, `priceRuleId` | The values of the catalogue item the user chose in §4. |
| `travellerId`, `journeyId` | An Active traveller and a journey of this order. |
| `quantity` | `≥ 1`. |

A malformed request is HTTP 400.

### 5.2 Steps

All steps run under the same order lock that `IssueOrderService` takes (`IReservationLock.AcquireAsync(orderId)`).

1. Load the order (`2500` if unknown); authorize.
2. **Replay.** If an `OrderChange` of this order has `SourceSystem = "AddAncillary"` and `SourceReference = requestKey`, compare the request with the services that change created: every selection must match exactly one of those services, and every one of those services exactly one selection, on `productRef` = `ServiceDefinitionRef`, `productVersion` = `ServiceDefinitionVersion`, `travellerId`, `journeyId` = the journey of the service's covered air services, `quantity` = `Quantity`, and `priceRuleId` = the `Reference` of the service's pricing lines. If they match, return the result of that change (§5.3) without calling anyone and without changing anything; otherwise `2907`.
3. Status must be `Ticketed` (`2901`).
4. No unresolved fulfillment task of the order of type `IssueTicket`, `IssueEmd`, `VoidTicket`, `CancelConfirmed`, `ReleaseReserved`, `ReserveInventory` or `ConfirmInventory` (`2778`).
5. Every `travellerId` is an Active traveller of the order (`2902`); every `journeyId` is a journey of the order on which that traveller has at least one Active air service (`2903`).
6. Build the quote request of §4.1 and add `selections`, one per request selection, in request order: `travellerRef` = `travellerId`, `boundRef` = `journeyId`, `flightRef` null. Call AirOffer once.
7. AirOffer failure → `2905`. Ancillary business error (for example `16309` selection no longer current, `16305`, `16306`) → `2904` with the upstream code and message.
8. **Check the answer.** It must contain exactly one item per selection, in the same order, with the same `productRef`, `productVersion`, `priceRuleId`, `travellerRef`, `boundRef` and `quantity`; `currencyId` must be the order's currency; `type` must be `ExtraBaggage`; `document.type` must be `EmdAssociated`; `inventory.control` must be `Unlimited`; `coveredFlightRefs` must have exactly one entry, and it must be a segment on which that traveller has an Active air service; `total` must equal the sum of the item's line amounts. Anything else → `2906`.
9. **Commit**, as one unit of work:
   - one `OrderChange`: `ChangeType = AddProduct`, `CommercialVersion = Order.CommercialVersion + 1`, `ActorContext` = the caller's sales context (as `CancelOrder` does), `SourceSystem = "AddAncillary"`, `SourceReference = requestKey`, `ReasonCode`, `ReasonText`, `WaiverCode` null, `IsInvoluntary = false`, `CommittedAt` = the clock;
   - per item, in order: one `OrderItem` (`Kind = ProductType.Baggage`, `AcceptedTotal = total`, Active, created by the change) and one `OrderBaggageService` (§3.2);
   - per item line: one `PricingLine` and one allocation (§5.4);
   - `Order.CommercialVersion` = the change's version; `Order.CustomerTotal` increases by the sum of the items' `total`;
   - `Order.Status` is **not** changed: it stays `Ticketed`. Whether an ancillary has its document is read from the service and the EMD, not from the order's root status;
   - the read model is projected and `OrderAncillaryAdded` (§7) is written to the outbox in the same unit of work.

Either all of step 9 is stored or none of it. A failure in steps 1–8 stores nothing.

### 5.3 Result

```json
{
  "orderId": "9001", "orderChangeId": "9100", "commercialVersion": 3,
  "orderStatus": "Ticketed", "currencyId": 978, "customerTotal": 538.50,
  "services": [
    { "orderItemId": "9101", "orderServiceId": "9102", "travellerId": "7001", "journeyId": "8001",
      "productRef": "XBAG1", "productVersion": 1, "serviceSubCode": "0CC",
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
- A different `requestKey` for something already on the order → Ancillary refuses it through `existing` (`16305` or `16306`, surfaced as `2904`). Ordering adds no rule of its own.

---

## 6. `IssueAncillaryDocument`

`POST Backoffice/v1/Orders/{orderId}/Ancillaries/Issuance`

Request: `{ "emdDocumentStockId": "5001" }`

It issues one EMD-A, with one coupon, for **every** outstanding ancillary service of the order, in one all-or-nothing operation, following the local ticket-issue pattern step for step. Ancillary and AirOffer are not called.

**Outstanding ancillary service:** an Active `OrderAncillaryService` with `DocumentType = Associated` for which no `ElectronicMiscDocument` with `StatusSummary = Issued` has a coupon whose `CurrentOrderServiceId` is that service.

### 6.1 Steps

1. Acquire the order lock; load the order (`2500`); authorize.
2. Determine the outstanding ancillary services, in order of creation.
   - None, and the order has EMDs: return the existing EMDs with the task id of the latest one (replay; nothing changes).
   - None, and no EMD exists: `2908`.
3. No unresolved fulfillment task of the order of type `IssueEmd`, `IssueTicket`, `VoidTicket` or `CancelConfirmed` (`2778`).
4. For each outstanding service, find the ticket coupon of its covered air service and the passenger identity of the EMD (§6.2). Any failure → `2909`; nothing is issued for any service.
4a. For each outstanding service, compute its values (§6.3). A service whose pricing cannot be reconciled → `2911`; nothing is issued for any service.
5. Load the stock (`DocumentStockNotFound`); `stock.EnsureCanIssue(AccountableDocumentKind.ElectronicMiscDocument, number of outstanding services)`.
6. Acquire the stock lock; reload the stock; repeat step 5's check.
7. Create one `FulfillmentTask`: `TaskType = IssueEmd`, `FulfillmentProviderKey = LocalDocumentAuthority`, no reservation, idempotency key `issue-emd:{taskId}`, correlation reference `order:{orderId}:issue-emd:{taskId}`, targets = the outstanding services (`FulfillmentTargetKind.OrderService`, action `Issue`); start its attempt.
8. Per service, in order: `stock.Allocate(taskId, "EMD:{orderServiceId}", …)`. If any allocated number already belongs to an EMD → `DocumentNumberIsAlreadyIssued`.
9. Per service: create the `ElectronicMiscDocument` (§3.4) with its coupons and price links (§6.3, §6.4).
10. Mark each allocation issued; add the task targets `DocumentStockAllocation`, `ElectronicMiscDocument`, `EmdCoupon` (action `Issue`); complete the attempt as `Succeeded`.
11. Store the task and the EMDs, project the read models of the EMDs and the stock, and save — one unit of work. Each EMD raises its issued event, published as `ElectronicMiscDocumentIssued` (§7).

The order's root status and `CommercialVersion` do not change. The last-ticketing-date check of ticket issue is not applied.

A failure before step 11 stores nothing: no task, no allocation, no EMD.

### 6.2 Which ticket coupon, and whose passenger identity

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

### 6.3 The coupon and the amounts

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
| `AssociatedTicketCouponId` | the coupon of §6.2 |
| `IssuanceValue` | `ServiceBaseValue` |
| `CurrencyId` | the order's currency |
| `Status` | `OpenForUse` |
| `ProviderCouponStatusCode`, `PredecessorEmdCouponId`, `ExternalValueReference` | null |

and `IssuedTotal = DocumentTotal`. One `EmdAssociationHistory` entry: `Action = Associate`, the ticket coupon, the change that created the service, `OccurredAt` = issue time.

The coupon value is the base value of the service; it never contains tax. Taxes belong to the document. Example — `Ancillary` 35.00 and `Tax` 3.50: coupon value 35.00, `IssuedTotal` 38.50, and the tax is a document-level price link of 3.50. With no tax line: the coupon value and `IssuedTotal` are both 35.00.

**Basis.** IATA's guide derives an EMD's base fare amount either from its fare calculation or from the sum of its coupon values, the two being mutually exclusive, and keeps taxes and the total document amount as separate elements. With one coupon, the coupon value is the base amount and nothing has to be split or placed.

### 6.4 Price links

One `EmdPriceLink` per pricing line of the service, with `PricingLineId`, `PricingAllocationId` = that line's allocation, `AttributedValue` = the line's amount, `CurrencyId`:

| Pricing line | `EmdCouponId` |
|---|---|
| `Category = Ancillary` | the coupon |
| `Category = Tax` | null — a document-level link |

Both reconciliations must hold exactly, otherwise the issue is refused (`2911`):

- the sum of all links' `AttributedValue` = the EMD's `IssuedTotal`;
- the sum of the coupon-linked links' `AttributedValue` = the coupon's `IssuanceValue`.

### 6.5 Result

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

## 7. Integration events

| Event | When | Contract |
|---|---|---|
| `OrderAncillaryAdded` | step 9 of §5.2 | **new**, in `AeroTech.Messages.Ordering.IntegrationEvents.V1`, extending `BaseIntegrationEvent` |
| `ElectronicMiscDocumentIssued` | step 11 of §6.1, once per EMD | **existing** contract, used unchanged |

`OrderAncillaryAdded`: `OrderId`, `OrderChangeId`, `CommercialVersion`, `CurrencyId`, `CustomerTotalAfter`, `CommittedAt`, `Services[]` — each `OrderItemId`, `OrderServiceId`, `TravellerId`, `ServiceDefinitionRef`, `ServiceDefinitionVersion`, `ServiceSubCode`, `Quantity`, `Total`, `CoveredAirServiceIds[]`.

`ElectronicMiscDocumentIssued` values: `ElectronicMiscDocumentId`; `OrderId`; `TravelerId` = the EMD's traveller; `OperationId` = `IssueFulfillmentTaskId` (as the ticket event does); `DocumentNumber`; `Type`; `ReasonForIssuanceCode`; `IssuerCarrierId`, `IssuingOfficeId` from the issuance context; `Authority = Local`; `CurrencyId`; `IssuedTotal`; `DocumentVersion`; `Coupons[]` — `EmdCouponId`, `CouponNumber`, `Purpose`, `OrderServiceId` = `CurrentOrderServiceId`, `AssociatedTicketCouponId`, `IssuanceValue`.

Both are written through the existing transactional outbox, in the same unit of work as the data they describe. A replay (§5.2 step 2, §6.1 step 2) publishes nothing.

---

## 8. Guard on existing operations

Cancelling an ancillary and voiding an EMD are not part of Phase 1. Until they exist, `CancelOrder` and `VoidElectronicTickets` are refused with `2910` when the order has an Active `OrderAncillaryService`, so that no bag or EMD is left attached to a cancelled flight or a voided ticket.

---

## 9. New error codes

Block `29xx` is unused at `cd50a2a`.

| Code | Name | HTTP | Raised when |
|---|---|---:|---|
| 2901 | `OrderIsNotTicketedForAncillary` | 409 | the order's status is not `Ticketed` |
| 2902 | `AncillaryTravellerIsNotActive` | 422 | a selection's traveller is not an Active traveller of the order |
| 2903 | `AncillaryJourneyIsNotAvailable` | 422 | a selection's journey is not a journey of the order with an Active air service of that traveller |
| 2904 | `AncillaryOfferWasRejected` | 422 | Ancillary refused the quote; message carries the upstream code and text |
| 2905 | `AncillaryOfferProviderIsUnavailable` | 503 | AirOffer could not be reached or failed |
| 2906 | `AncillaryOfferAnswerIsInconsistent` | 502 | the answer fails a check of §5.2 step 8 |
| 2907 | `AncillaryRequestKeyWasUsedForAnotherRequest` | 409 | same `requestKey`, different selections |
| 2908 | `OrderHasNoAncillaryToIssue` | 409 | no outstanding ancillary service and no EMD |
| 2909 | `AncillaryTicketCouponIsNotEligible` | 409 | §6.2 fails for a covered air service |
| 2910 | `OrderHasActiveAncillary` | 409 | §8 |
| 2911 | `AncillaryDocumentCannotBeFormed` | 409 | the service does not have exactly one covered air service, or the reconciliation of §6.3 / §6.4 fails |

Existing codes reused as they are: `2500`, `2505`, `2778`, `DocumentStockNotFound`, `DocumentStockKindMismatch` (2786), `DocumentNumberIsAlreadyIssued`.

---

## 10. Expected behaviour

Each row is one test when tests are written. Amounts: `Ancillary` 35.00, `Tax` 3.50 unless a row says otherwise.

### Adding

| # | Behaviour |
|---|---|
| A01 | Add one `XBAG1` (sub code `0CC`, quantity 1) to a ticketed order → one item of kind `Baggage`, one `OrderBaggageService` of type `BaggageCharge` with provider `LocalDocumentAuthority`; pricing lines with reason `AddService`; `CommercialVersion + 1`; status `Ticketed`; `OrderAncillaryAdded` published once. |
| A02 | The same `requestKey` again → the same result; nothing new stored or published. The same `requestKey` with another selection → `2907`. |
| A03 | A second add of `XBAG1` for the same traveller and journey with a new `requestKey` → `2904` carrying `16305`. A request for `XBAG1` with quantity 2 → `2904` carrying `16306`. |
| A04 | A product on an industry sub code that is not in Ancillary's reference cannot exist, so it can never be listed or added; a carrier-defined generic bag (`XBAGG`, 1–2) can be added with quantity 2 and stays a carrier-local code on the service. |
| A05 | The listed rule was replaced before the add → `2904` carrying `16309`; the order is unchanged. |
| A06 | AirOffer answers with a different quantity, traveller, currency, an item count that differs from the selections, or an item covering more than one flight → `2906`; nothing stored. |
| A06a | A journey on which the traveller flies two flights → the list offers no bag for it; a selection of it → `2904` carrying `16305`. |
| A07 | Order not `Ticketed` → `2901`. Unresolved fulfillment task → `2778`. |
| A08 | `ReserveOrder` on an order that has an ancillary service does not fail and does not create a reservation for it. |
| A09 | `CancelOrder` or a ticket void on an order with an Active ancillary service → `2910`. |

### Issuing

| # | Behaviour |
|---|---|
| E01 | One covered flight, no tax line → one coupon, value 35.00; `IssuedTotal` 35.00; one price link (coupon-linked). |
| E02 | One covered flight, with tax → one coupon, value 35.00; `IssuedTotal` 38.50; price links: 35.00 on the coupon, 3.50 with no coupon. |
| E03 | A service with two covered air services (which the add command never creates) → `2911`; nothing issued. |
| E04 | A service with a customer-price line of another category, or a line not fully allocated to it → `2911`; nothing issued. |
| E05 | The ticket's `TravellerProfileRevisionId` equals the traveller's current revision → issued with that revision. |
| E06 | The traveller's current revision is newer than the ticket's → the EMD carries the ticket's revision, not the current one. |
| E07 | The EMD's coupon carrier facts are those of the ticket coupon's `IssuedSegment`, even after the order's segment has changed. |
| E08 | The only `Open`, `Local` coupon for a covered air service belongs to a ticket of another traveller, or the coupon is `Used`, `Void`, `Exchanged`, `Refunded`, `Suspended`, or its control is not `Local` → `2909`; nothing issued. |
| E09 | Two outstanding services, the second fails any check → nothing issued for either; no stock number consumed. |
| E10 | Issue again when nothing is outstanding → the existing EMDs are returned; nothing new. No outstanding service and no EMD → `2908`. |
| E11 | The published `ElectronicMiscDocumentIssued` carries `IssuedTotal` 38.50 and one coupon with value 35.00; `OperationId` = the issue task. |
| E12 | Issuing calls neither AirOffer nor Ancillary, and changes neither the order's status nor its `CommercialVersion`. |

---

## 11. What the Owner signs off with this spec

These are changes to Ordering's own model and Master; approving this spec approves them.

1. The three enum members of §3.1.
2. The new fields `ServiceDefinitionVersion`, `DocumentType`, `ReasonForIssuanceCode` on the ancillary service, and `ResponsibleAirlineId` materialized on the base service.
3. The no-reservation provider under the key `LocalDocumentAuthority`.
4. The order's root status staying `Ticketed` when an ancillary is added and when its EMD is issued.
5. Phase 1 documents only a bag that covers one flight: one EMD-A, one coupon, coupon value = base value, taxes at document level. Bags over several flights wait for an EMD valuation model.
6. `Open` + `Local` as the only eligible ticket-coupon state in Phase 1.
7. The guard of §8.
8. The new event `OrderAncillaryAdded`.
