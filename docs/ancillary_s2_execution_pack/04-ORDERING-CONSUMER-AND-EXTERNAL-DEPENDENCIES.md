# S2 — Ordering Consumer Contract / external dependencies (READ ONLY outside Ancillary)

**این فایل یک Contract قابل تحویل به تیم Ordering است، نه مجوز تغییر آن مخزن.** از Master `01-S2-OWNER-DESIGN-MASTER-v1.0.md` §§03,05,09–11,17 منشأ می‌گیرد. وضعیت واقعی مخزن Ordering باید با Source همان روز VERIFY شود؛ نام URLهای TARGET را live فرض نکن. آخرین cross-repo audit: Ordering.Final `k8s-stg@cd50a2a5fd18a372f32f2d4efac08dee0510f6ba` (PRIOR-AUDIT).

## 1. گردش پیش از Issue — Ordering مالک سفارش از لحظه ایجاد

```text
Client -> Ordering.CreateOrderFromOffer(OfferId,Travellers,Contact)
Ordering -> Client: OrderId + trusted Order commercial state / version
Client -> Ancillary: POST /{S}/v1/Orders/{orderId}/AncillaryOffers
Ancillary -> Ordering S2S: Read trusted OrderAncillaryContext
Ancillary -> Client: List with inline options and optional Details
Client -> Ancillary: POST /{S}/v1/AncillaryOffers/Selections/Evaluate (batch Order source)
Ancillary -> Client: per-line quoteReady + protected short-lived pricedSelectionRefs
Client -> Ordering: POST /{S}/v1/Orders/{orderId}/Ancillaries (TARGET)
Ordering -> Ancillary S2S Resolve(OrderId,OrderVersion,TokenRefs)
Ancillary -> Ordering: allValid + full ResolvedSelections with Price/Provider/Seat/FlightCapacity
Ordering: check OrderVersion again, atomic OrderChange/OrderService persist, no silent subtotal change
Ordering -> Client: Order change result, next Payment/Reserve/Issue steps owned elsewhere
```

After issue identical client-facing Shopping/Evaluate/Resolve mechanics with trusted `PostTicketed` Context and strict Coupon/Existing Service eligibility. Payments, refund, Ticket/EMD document issuance and actual Supplier hold are **not** accomplished by S2.

## 2. REQUIRED trusted Order context read — target S2S

**Proposed**, not verified deployed: `POST /Service/v1/Orders/AncillaryShoppingContext` `{orderId:long}`. May adapt to real sibling Ordering convention **only after code verification** (then update contract docs). It must require Caller service identity and authorized sales/Order access. No client-controllable agency/owner scopes.

| JSON/context field | Type | Null/Source guard | Required for what |
|---|---|---|---|
| `orderId` | int64 | >0 | all Order shopping |
| `orderCommercialVersion` | string | nonempty, changes on commercial updates | immutable accepted quote/optimistic concurrency |
| `status`, `purchaseStage` | canonical enums | sourced from current lifecycle, not request flag | PreOrder vs PostTicketed and eligibility |
| `originPricedOfferId?` | string | actual link or null; never manufactured | mapping Offer SeatMap ID; frozen Order-based SeatMap DTO |
| `ownerAirlineId` | int | true Order owner | catalogue and auth |
| `sales.channel` | enum label/value with explicit mapping | Source Order/authorization | POS restrictions, no numeric blind casts |
| `sales.customerId?`, `travelAgencyId?`, `airlineOfficeId?` | int64? | required by authenticated channel | leak/cross-buyer prevention |
| `sales.pointOfSaleId`, `countryId?`, `currencyId` | int64/int | validated and fixed for quote | price/scope |
| `travellers[]` | `{orderTravellerId,sourceTravellerRef?,ptc,dateOfBirth?/verified?,associatedAdultRef?,seatEntitlement?}` | source verified; refs unique | per pax and Infant/Guardian |
| `portions[]` | `{portionRef,flightRefs[]}` | existing Order portions | journey/bound coverage |
| `flights[]` | `{flightRef,flightId,flightCapacityId,flightVersion,cabinClassId?,aircraftId?,rbdId?,operatingAirlineId?,marketingAirlineId?,departureAt,arrivalAt,origin/destination/timezone}` | actual Order + Flight source; missing critical means unknown | eligibility, SeatMap, physical holds |
| `coupons[]` | `{orderTravellerId,flightRef,couponStatus,fareBasis?,fareFamily?,rbd?,isUpgradable?,ticketDocumentRef?}` | per coupon, including Used/Flown/Refunded/Voided; not all true by default | PostTicketed, fare application |
| `allowances[]` | per pax-flight checked/cabin weights/pieces | source complete? | baggage/upsell |
| `existingServices[]` | `{orderServiceId,variantCode,beneficiary,coverage,quantity,commericalStatus,fulfillmentStatus,documentStatus?,countingFamilyCode?,seatNumber?}` | authoritative active/pending/used/refunded | non-duplication, caps and seat reassignment |
| `usageEvidence[]` | verified family/coverage used+remaining | null/unknown != zero | cumulative limits |
| `fareBenefits[]` | explicit source proof & coverage | `fareBenefitsComplete` true only if genuinely verified | free/entitled vs upsell |
| `payments/documents/seatAssignments` | minimal IDs/state as required | from respective authoritative services via Ordering | post-issue sales/coupon and no double charge |
| `completeness` | flags: travellers,flights,coupons,allowances,existingServices,usageEvidence,fareBenefits,seatEvidence | true only if source actually complete | honest NeedsVerification |

**Identity mapping:** `sourceTravellerRef` from AirAvail must be mapped to actual `orderTravellerId` by verified Order relationship. A same-name passenger or same array index is **not** an identity proof. `FlightId` and `FlightCapacityId` are distinct; do not swap. If Ordering cannot establish source version or linked Airline/Agency/Order owner, fail-closed. For historical Orders with no origin OfferID, the existing frozen SeatMapsDto cannot honestly populate `flightOfferId`; OrderSeatMap must fail until a versioned alternative is explicitly approved.

## 3. TARGET Ordering Add API — contract only

Proposed on Ordering (not Ancillary):

`POST /{S}/v1/Orders/{orderId}/Ancillaries`

```json
{
  "expectedOrderCommercialVersion":"V17",
  "idempotencyKey":"<client-unique-key>",
  "pricedSelectionRefs":["<protected-T1>","<protected-T2>","<protected-seat>"]
}
```

- Reuse existing channel/buyer/user auth and Order lifecycle on real Ordering controllers. Route name is proposal until verified by Ordering owner.
- Transport-valid body may call Resolve but may not bypass it; no price/option/traveller trusted from client.
- All-or-none **commercial acceptance/persistence**, with per-line diagnostics; `allValid=false` => zero newly added services and no payment. A failure later during external Hold can be per-unit pending/partial in S3, not a silent partial S2 Add.
- Idempotency `(orderId,clientKey,canonicalPayloadHash)` same payload => same OrderChange; same key different payload => conflict; DB optimistic version checked again after Resolve and before Commit; no duplicate OrderService or Fee.
- Return actual OrderChange/OrderService IDs, new OrderVersion, prices/payment difference, status; do not return `Confirmed/EMDIssued` without event/document evidence.
- A22/A24 true Order-scoped services require `OrderService.TravellerId` nullable or a **real approved equivalence**; current Ordering in PRIOR-AUDIT had nonnullable `long`. No dummy passenger id = 0.

## 4. Ancillary Resolve S2S request/output

Request exact:

```json
{"orderId":12345,"expectedOrderCommercialVersion":"V17","pricedSelectionRefs":["<protected-token>"],"correlationId":"e7934519-8e56-47f4-a532-d7f077899e7b"}
```

Requires real `Service` policy + Ordering allowlist; Owner/Agency/Office/Customer and version re-check. Response requires per selection `status,reasonCodes,kind,serviceDefinitionRef,definitionVersionId,provisionId,pricingRevisionId,pricingRateId?,profile,variantCode,serviceTypeCode,serviceSubCode,fulfillmentProviderKey,bookingMethod,ssrCode?,confirmationRequirement,documentRouting,documentType,rfic?,rfisc?,beneficiaries(orderTravellerId),coverage(scope,portionRefs,flightRefs,flightIds,flightCapacityIds),selection normalized,price with all taxes/fees/FX,requiresCheckAtReserve,validUntilUtc,sourceFingerprint,sourceSelectionRefDigest` and `allValid/basketPricing`. Fields and precise JSON in Master §11. It NEVER holds/charges/issues.

## 5. Downstream handoff to existing Ancillary Stage3 — **after** Ordering creates OrderService

Current in Ancillary `HoldAncillaryServicesRequest`:

```text
IdempotencyKey:string
OrderId:long
Reference:string
RequestedExpiresAt:DateTimeOffset?
Services:AncillaryHoldServiceInput[]:
  OrderServiceId:long
  ServiceDefinitionId:long
  ProvisionId:long
  TravellerId:long? (genuine null permitted when genuinely Order-scoped)
  CoverageScope:ServiceCoverageScope
  CoveredFlightIds:long[]
  Quantity:int
```

Stage 3 current input **does not represent every future special typed field** (e.g., decimal chargeable weight, seat physical reference, external quote). That is an explicit S3 contract compatibility gap; do NOT pretend converting weight decimal to int solves it. In S2, publish complete ResolvedSelection and report the downstream need. Physical Seat Hold is FlightFlow-owned. BookingMethod/EMD routing is metadata; Ordering/Ticketing owns document/PNR/EMD.

## 6. Consumer-facing error/outcome responsibilities

| Event | Ancillary S2 | Ordering action |
|---|---|---|
| Token expired / Price or Order version changed | `Expired` or `Stale` + safe current-price advice if source trustworthy | no silent Add; ask UI to re-Evaluate/reapprove |
| Token from foreign Order/Agency/Office/POS | `NotAuthorized`/safe 404/403 | deny, no existence leak |
| Already consumed Coupon | `Rejected/NotEligible` | no OrderService addition |
| Missing Partner Quote/Capacity | `NeedsQuote/NeedsVerification` | do not charge/hold automatically |
| Basket one invalid line | per-line status, `allValid=false` | no partial commercial persist |
| External supplier fails after successful Add | out of S2 | Stage3 pending/compensation under Ordering control |
| Payment capture/EMD issue fails | out of S2 | Ordering/Payment/Ticketing retry & history |

## 7. External inventory — report status honestly

- `EXT-01`: Ordering Trusted Context + CommercialVersion / Ownership / Traveller map (external unavailable until deployed).
- `EXT-02`: Ordering AddAncillaries + idempotency/OrderChange/persistence.
- `EXT-03`: Ordering real Order-scoped nullable traveller support.
- `EXT-04`: AirAvail channel naming/priced source validity actual production parity.
- `EXT-05`: AirPrice fare benefits, tax/FX/precision.
- `EXT-06`: FlightFlow complete occupied rows / authoritative physical Hold.
- `EXT-07`: supplier external quote/appointment/chargeable KG/guardian evidence where source required.
- `EXT-08`: Ticketing/EMD/payment routing and events.

S2 Internal Contract tests with trusted fakes are allowed for `EXTERNAL_PENDING`; an actual end-to-end user journey is NOT certified unless services actually communicate. Do not edit any sibling repo under this task.
