# Order-based Shopping - Questions and Conflicts

Date: 2026-10-27
Base: `feat/ancillary-offers-v9@0f9dfbb` + Q1 replay change (uncommitted)
Goal (Owner): every step done today with an `offerId` (AncillaryOffers, Price) also works with an `orderId`. Ancillary reads the order over HTTP from Ordering (`GET /Service/v1/Orders/{orderId}/AncillaryContext`, in development), builds the same canonical shopping context, and follows exactly the offer path. The output shape stays the same, except `offerId` becomes `orderId`.

No code has been written. This file waits for the Owner's answers.

## 1. How it works today

### 1.1 The offerId path

| Step | Route (per surface `S` = Backoffice, OtaPanel, Api) | Source |
|---|---|---|
| List | `POST {S}/v1/AncillaryOffers {offerId}` | `GetAncillaryOffersService` → `IFlightOfferShoppingSource.LoadAsync(offerId)` (AirAvail `Service/v1/FlightOffers/Details`) → `FlightOfferShoppingContextMapper` → engine per point of sale → `IAncillaryOfferListing`. `PricedOfferScopeGuard` checks channel and customer. Answer `AncillaryOffersDto(OfferId, CurrencyId, <family arrays>)`. |
| Price | `POST {S}/v1/AncillaryOffers/Price {offerId, selections}` | `SelectionVerifier` loads the offer again (`SelectionVerifier.cs:46`). It writes one `SelectionSnapshot`: `OfferId` nvarchar(512), required (`SelectionSnapshot.cs:12,33,49`). Answer `{selectionId}`. |
| Read priced | `GET {S}/v1/AncillaryOffers/Price/{selectionId}` | re-shops with the stored `OfferId` (`GetAncillaryOffersService.ExecuteAsync(SelectionSnapshot…)`) |
| Resolve | `POST Service/v1/AncillaryOffers/Resolve {selectionId}` | reads the snapshot only, with no source call |
| Signed ids | `d_` / `r_` | bound to the offer by `SelectionBindings.Offer(offerId, trustedSourceVersion)` (claim `ob`) |

### 1.2 Order parts that already exist

These were built in S2 v1.1 stage 3 and are still in the tree:

- **Port and context records** (`Application/Orders/`):
  - `IOrderCommercialContextReader.FindAsync(orderId)`
  - `OrderCommercialContext(OrderId, int CommercialVersion, AcceptsChanges, TicketedAtUtc, FlightOffer Air, Travellers, Services)`
  - `OrderTravellerFacts(TravellerRef, OrderTravellerId, DateOfBirth, InfantOfTravellerRef)`
  - `OrderServiceFacts(OrderServiceId, TravellerRef, ServiceRef, PortionRef, FlightRefs, Quantity, CommercialState)`
- **Placeholder reader:** `Providers/Orders/NotConnectedOrderCommercialContextReader` always answers 16815 / 503 "not connected".
- **Order source** (`Query/Shopping/Common/Orders/OrderShoppingSource`), in this order:
  1. an unknown order answers 403;
  2. `AcceptsChanges = false` answers 422;
  3. `Air.OfferId` is required;
  4. the context is built from `order.Air` through the same `FlightOfferShoppingSource.ContextualizeAsync`, plus `OrderSourceFingerprint` and `OrderShoppingContextMapper`.
- **Source dispatcher:** `ShoppingSource` (`offerId` XOR `orderId`). Only `GetSeatMapService` uses it today; `ISeatMapQuery` already has `OrderId`. `SourceIdentityContext` of the engine already has `OrderId`.

### 1.3 Ordering

- `AeroTech.Ordering.Final@cd50a2a` is equal to its remote `k8s-stg`.
- `RestApi/V1/OrderAggregate/Controllers/ServiceController.cs` (`Service/v1/Bookings`) has **no action**.
- An order can be read only through:
  - `GET Backoffice/v1/Orders/{orderId}` (Backoffice token);
  - `GET OtaPanel/v1/Bookings/{orderId}` and `GET Api/v1/Bookings/{orderId}` (agency token, own orders only).
- The new route `GET /Service/v1/Orders/{orderId}/AncillaryContext` is not on disk.
- The S2 pack (`docs/ancillary_s2_execution_pack/01-…MASTER…md` §05.3) describes `POST /Service/v1/Orders/AncillaryShoppingContext` with a JSON body. All of its fields are marked "proposed".

### 1.4 What I will copy without asking

- **HTTP adapter:** the AirAvail provider pattern.
  - Wire DTOs in `Providers/Orders/Wire/`;
  - a reader `Providers/Orders/Services/OrderCommercialContextReader` that replaces the not-connected one;
  - `AddHttpClient<IOrderCommercialContextReader, …>` reading `Ordering:BaseUrl`, failing fast when the setting is missing;
  - the S2S call is anonymous (memory: S2S needs no auth).
- **Ordering errors:**
  - 404 from Ordering becomes `null`, which the order source turns into 403 (unchanged);
  - any other failure becomes 16815 / 503.
- **Engine and listing:** unchanged. Only the source changes.

## 2. Questions

### Q1. What does `GET /Service/v1/Orders/{orderId}/AncillaryContext` return?

The engine needs the same facts the offer gives it:

- flights: flight, capacity, airports, airlines, times, aircraft, cabin, RBD, booking class;
- fare components: AirFareId, fare basis, fare family;
- tickets and coupons;
- baggage allowance;
- the channel, customer and country of the sale.

From the order it additionally needs:

- the commercial version and whether the order accepts new services;
- the ticketing time;
- the travellers, with the mapping order traveller id ↔ offer traveller ref, the PTC and the date of birth;
- the existing services (order service id, traveller, service, flights, quantity, state).

Options:

- **(a) Ordering returns all of these facts from the order itself.** Ancillary maps them to its `FlightOffer` model and `OrderCommercialContext`, as the existing order source expects. AirAvail is not called.
- **(b) Ordering returns only the order facts plus the source offer id.** Ancillary loads the itinerary and fares from AirAvail with that offer id. This is less work for Ordering, but the facts are then the offer's, not the order's. An old priced offer id may no longer resolve, or may price differently from what was sold.

Recommendation: **(a)**. After booking, the order is the source of truth.

If Ordering's developer already has a response shape, please send it. Otherwise I will write the exact JSON Ancillary needs as a handoff note for them; please also say where it should go: `E:\Projects\DotAir\handoffs\` or `reports/`.

Answer:(a)

### Q2. Route names for the order-based endpoints

You asked for similar endpoints that take an `orderId`.

**Proposal (a), on each of Backoffice, OtaPanel and Api:**

| New route | Body |
|---|---|
| `POST {S}/v1/AncillaryOffers/Orders` | `{orderId}` |
| `POST {S}/v1/AncillaryOffers/Orders/Price` | `{orderId, selections}`, with selections exactly as in the offer Price |

Unchanged routes: `GET {S}/v1/AncillaryOffers/Price/{selectionId}` and `POST Service/v1/AncillaryOffers/Resolve`, because the stored selection remembers its source.

**Alternative (b):** keep today's routes and accept `offerId` XOR `orderId` in the body. This was the S2 v1.1 D6 decision, and the `ShoppingSource` dispatcher already does it for SeatMaps.

Recommendation: **(a)**, because it follows your wording "separate endpoints that take orderId". Please confirm the route names or give others.

Answer: (a)

### Q3. Surfaces and who may shop an order

| Surface | Proposal |
|---|---|
| Backoffice | any order |
| OtaPanel and Api | only an order sold to the caller: the order's customer or agency must equal the caller's token claims, otherwise 403. This is the same rule `PricedOfferScopeGuard` applies to an offer, and it needs the order's sales owner in the Q1 response. |

Recommendation: all three surfaces, with that check.

Answer:all three surfaces, with that check.

### Q4. Output shape

`AncillaryOffersDto` starts with `offerId`.

- **(a)** A separate answer record per source, whose first member is `orderId` instead of `offerId`. The family arrays and all inner DTOs are shared, so they are byte-equal. Backoffice keeps its own `EnumValueDto` variant in the same way.
- **(b)** The same record with both `offerId` and `orderId` nullable. The memory rule "no nullable bags" argues against this.

Recommendation: **(a)**. Should the answer also show `orderCommercialVersion`? Recommendation: yes. Ordering then knows which version the offers were priced against.

Answer:(a)

### Q5. Storing an order-priced selection

`SelectionSnapshot.OfferId` is required. For an order source:

| Item | Proposal |
|---|---|
| New columns | `OrderId bigint NULL` and `OrderCommercialVersion int NULL` |
| `OfferId` | becomes nullable |
| Domain invariant | exactly one source |
| Migration | one plain migration for the command and query contexts, applied to dev (no backfill; dev data is test data) |
| Signed ids | bound to the order by `SelectionBindings.Order(orderId, orderFingerprint)` instead of the offer binding |

Recommendation: as proposed.

Answer:as proposed.

### Q6. When the order changes after Price

| Step | Proposal |
|---|---|
| `GET …/Price/{selectionId}` of an order-priced selection | re-reads the order. If the commercial version or the order fingerprint changed, it answers 409 / 16820, the same as a changed offer today. |
| Resolve | stays snapshot-only, as on the offer path, and adds `orderId` and `orderCommercialVersion` to its output, so Ordering can compare them with its own version before it stores the services |

Recommendation: as proposed.

Answer:as proposed.

### Q7. Which orders accept new services

The order source refuses an order whose `AcceptsChanges` is false.

- **(a)** Ordering returns `acceptsChanges` (Ordering owns its state machine).
- **(b)** Ancillary derives it from Ordering's status names.

Recommendation: **(a)**.

Answer:(a)

### Q8. SeatMaps

`SeatMaps` already accepts `offerId` XOR `orderId` through the dispatcher, but it is frozen.

- **(a)** Leave SeatMaps out of this first step.
- **(b)** Include it, since it gets the order reader for free once Q1 is built.

Recommendation: **(a)**, as a separate step.

Answer: remove `orderId` from existing endpoints, then add new endpoint with same signature that accepts `orderId` 

## 3. Round 2: Ordering's implementation

Ordering replied to `ORDERING-ANCILLARY-CONTEXT-HANDOFF.md`. Their route is built in `AeroTech.Ordering.Final` (local, uncommitted, not deployed). I read their code (read only):

- `OrderAncillaryContextDto.cs`
- `GetOrderAncillaryContextService.cs`
- the `ServiceController` diff, which moves the route from `Service/v1/Bookings` to `Service/v1/Orders`

### 3.1 What matches, and what I will copy without asking

| Item | Evidence |
|---|---|
| Route `GET Service/v1/Orders/{orderId:long}/AncillaryContext`, anonymous. Every field of the handoff header, travellers, journeys, segments, legs and `airServices`. | `OrderAncillaryContextDto.cs` |
| `journeys` and `segments` hold only the itinerary still covered by an Active air service | `GetOrderAncillaryContextService.cs` (`coveredSegmentIds`) |
| `couponOpenForUse` is `true` when a coupon's `FinancialStatus` is `Open`, `false` when coupons exist but none is open, and `null` when there is none. It maps to `FlightOfferCoupon.IsOpenForUse`. | same file, `CouponOpenForUse` |
| An unknown order is 404 (code 2500). The reader turns 404 into `null`, and the order source then answers 403, as today. | `ExecuteAsync` returns `null` |
| `passengerTypeCode` is Ordering's own `PassengerTypeCode`; `sales.channel` is `Shared.Enums.SalesChannel`; `commercialState` is `OrderServiceCommercialState`. All three enums are byte-equal to the copies already in this repo's `Contracts/AeroTech.Messages`. The wire DTO reads them with those enums. Passenger types map to the AirPrice `PassengerTypeCode` by member name. | `diff` of the three files: identical |
| Ordering's `cabinClassId` is a `long`; `FlightOfferFareComponent.CabinClassId` and `FlightOfferFlight.CabinClassId` are `int?`. Checked conversion; an overflow becomes 16815 / 503. | `FlightOfferFareComponent.cs`, `FlightOfferFlight.cs` |
| One fare component per distinct (`airFareId`, journey `boundId`). One ticket per traveller (`TravellerRef` = `travellerId`), with one coupon per air service. | `FlightOffer*` records |
| `FlightOfferBound.Direction` stays `null`. Ordering has no direction, and only `FlightOfferFingerprint` reads it. | `grep bound.Direction` finds only `FlightOfferFingerprint.cs:33` |

### R2-1. `acceptsChanges` is always `false`

Ordering fixed it to `false` until its Stage 6 (`private const bool AcceptsNewServices = false`). Under Q7 (a), `OrderShoppingSource` refuses every order with 422 "order does not accept new services". The new endpoints will therefore answer 422 for every real order until Ordering changes the constant.

- **(a)** Build as agreed. Proof is a clean build only, as your rule says. A live HTTP flow waits for Ordering Stage 6.
- **(b)** Ignore `acceptsChanges` until Stage 6. This departs from your Q7 answer.

Recommendation: **(a)**.

Answer:Ignore `acceptsChanges` until Stage 6. This departs from your Q7 answer.

### R2-2. Who may shop an order: channel and country

Two facts:

- `PricedOfferScopeGuard` compares `offer.SalesChannel` (`AeroTech.Messages.Core.Enums.Channel`, which you banned in new code) and `offer.SalesCountryId` with the caller's scope.
- Ordering stores no sales country. Ordering also uses a different enum, `Shared.Enums.SalesChannel`.

With the offer guard, every agency call on an order would be refused: `scope.CountryId` comes from the request-country header, and the order has no country to compare it with.

**Proposal:** a separate `OrderScopeGuard`, applying exactly your Q3 rule.

| Surface | Rule |
|---|---|
| Backoffice | any order |
| OtaPanel and Api | `order.sales.travelAgencyId` must equal the caller's `TravelAgencyId`. The customer is checked as the offer guard does it: the order's `customerId` must equal the caller's `CustomerId` unless the scope sells for any customer. Otherwise 403. |
| Channel | not compared |
| Country | not compared |

The order-built `FlightOffer` leaves `SalesChannel` and `SalesCountryId` `null`. The points of sale used for pricing are still resolved from the caller's request, as today.

Recommendation: as proposed.

Answer: as proposed (Owner, in chat). The Owner also stated that `AeroTech.Messages.Core.Enums.Channel` is replaced by `AeroTech.Messages.Shared.Enums.SalesChannel`.

### R2-3. Price facts that an order does not have

The offer fingerprint hashes members the order answer does not carry:

- `PricedAt`
- `BaseAmount`, `ChargeAmount`, `TotalAmount`
- `OrderCharges`, `RatesOfExchange`
- coupon `PriceLines`

Evidence: `FlightOfferFingerprint.cs:18-22,98,102,104`. No other Query code reads them (grep). `OrderSourceFingerprint.Of` hashes `FlightOfferFingerprint.Of(order.Air)` first.

- **(a)** `OrderSourceFingerprint` hashes the order facts directly: header, travellers, journeys, segments, legs, air services and ancillary services, and no longer hashes `FlightOfferFingerprint.Of(order.Air)`. The order-built `FlightOffer` fills the price members with neutral values:
  - `PricedAt` = the time Ancillary read the order;
  - the amounts are 0;
  - the lists are empty;
  - `LastTicketingDate` is `null`.
- **(b)** Keep the fingerprint as it is. The neutral values then enter the hash, and `PricedAt` changes on every read, so every `GET Price/{selectionId}` would answer 409.

Recommendation: **(a)**.

Answer:(a)

### R2-4. `ancillaryServices[]` shape

Ordering's shape differs from the handoff and from `OrderServiceFacts`:

| Ordering | Ancillary today |
|---|---|
| `coveredAirServiceIds` (air-service ids) | `FlightRefs` (`{boundId}:F{sequence}`) |
| `serviceDefinitionRef` nullable | `ServiceRef` required |
| `commercialState` Active, Replaced, Cancelled, Transferred | `ExistingServiceCommercialState` Active, Cancelled, Refunded |

The array is always empty today, and the meaning of `Replaced` or `Transferred` for an ancillary is Ordering's Stage 6 decision.

- **(a)** Declare the wire shape now. Until Ordering Stage 6 defines the meaning, a non-empty array answers 16815 / 503. The mapping is added in that stage.
- **(b)** Map it now: Active→Active, Cancelled→Cancelled, Replaced and Transferred skipped, a `null` ref skipped, and air-service ids turned into flight refs through `airServices`. This guesses Stage 6 meanings.

Recommendation: **(a)**.

Answer:(a)

### R2-5. Q4 open sub-question

Should the order answer of `AncillaryOffers/Orders` also return `orderCommercialVersion` next to `orderId`?

Recommendation: yes.

Answer:yes.

## 4. Round 3: before the order Price step

Built so far (uncommitted, clean build, Boundary 12/12, Shopping 125 passed + 1 existing skip of 126):

- Step 1: the Ordering reader.
- Step 2a: `POST {S}/v1/AncillaryOffers/Orders` on Backoffice, OtaPanel and Api, with `OrderScopeGuard`.
  - Signed ids are now bound to their source: `SelectionBindings.Offer(...)` for an offer and `SelectionBindings.Order(...)` for an order, chosen by `SelectionBindings.Source(context)`.
  - A changed order (version or fingerprint) therefore already fails the existing binding check with 409 / 16820.

Two points change the design of the next step (order Price and its snapshot migration).

### R3-1. `GET {S}/v1/AncillaryOffers/Price/{selectionId}` for an order-priced selection

Under Q2, this route stays shared and the stored selection remembers its source. But one route would then return two shapes: `AncillaryOffersDto` (`offerId` first) for an offer selection, and `OrderAncillaryOffersDto` (`orderId`, `orderCommercialVersion` first) for an order selection. Today each route has one response type, as in every sibling controller.

- **(a)** Add `GET {S}/v1/AncillaryOffers/Orders/Price/{selectionId}` for order selections. The existing GET serves offer selections only. Each route asked for the other kind of selection answers 404 / 16819 "selection not found". The order routes become `POST Orders`, `POST Orders/Price` and `GET Orders/Price/{selectionId}`, the same three as the offer routes.
- **(b)** Keep one GET. Its response type depends on the stored source.

Recommendation: **(a)**. It matches your Q8 answer for SeatMaps: a separate endpoint per source.

Answer:(a)

### R3-2. The name `OfferBinding` now also holds an order binding

The binding of a signed id is named "offer" in:

- the signed id claim (`ob`);
- `SignedSelection`, `SelectionProof`, `VerifiedSelection` and `ResolvedSelection` (`OfferBinding`);
- the column `SelectionSnapshotLines.OfferBinding`;
- the public Resolve output `sourceBinding.offerHash`.

For an order selection, it holds the order binding.

- **(a)** Rename to `SourceBinding` everywhere: claim `sb`, column renamed in the same migration as Q5, and Resolve output `sourceBinding.sourceHash`. The Resolve output is consumed by Ordering, so this changes its contract. Signed ids already issued stop working, which is acceptable because dev data is test data.
- **(b)** Keep `OfferBinding` / `offerHash` as they are. Nothing else changes.

Recommendation: **(a)** inside Ancillary (claim, records, column), but keep the public `offerHash` until Ordering consumes Resolve. If you prefer one name everywhere, rename it too.

Answer:(a)

Implementation note: the claim `sb` proposed above is already the sales binding claim (`SelectionIdCodec.SalesBindingClaim = "sb"`). The source binding claim is therefore `so`. The public Resolve output keeps `sourceBinding.offerHash`.

## 5. Round 4: SeatMaps/Orders on order 1552605112424202240 (2026-10-28)

The Owner's call was `POST Backoffice/v1/SeatMaps/Orders` with body `{ orderId: 1552605112424202240, options: { countryId: 1, pointOfSaleId: 1 } }` against the Staging catalog. I reproduced it: 200, two seat maps, 290 seats each, 580 of 580 traveller-seat pairs `BLOCKED`, no seat id.

### 5.1 Why no seat is selectable (findings, no decision needed)

1. **First gate: no booked cabin.**
   - `AncillaryShoppingEngine.SeatMaps.cs:61` blocks every seat when the booked cabin (`fare.CabinClassId ?? flight.CabinClassId`) is null, before any product rule runs.
   - For this order, Ordering's AncillaryContext returns `airServices[].cabinClassId: null` on both segments, and Ancillary copies it as-is.
   - The fares of the order (`1469435125056929792`, `1469436464520495104`) do have `CabinClassId = 1` (Economy) and `FareFamilyId = 4` (Flex) in the synced `ReferenceData.AirFares`.
2. **Even with the cabin: the flights have departed.** The segments departed on 2026-09-27 and 2026-09-28. Every seat provision has a minimum advance purchase of 3 hours and a permitted travel period of 2026-10-01 to 2027-12-31, so every seat fails `AdvancePurchaseTooLate` and `TravelDateNotPermitted`. A test needs an order with future flights.

### 5.2 Where the order path still depends on the offer id

| Place | Dependency |
|---|---|
| `Providers/Ordering/Services/OrderingAncillaryContextMapper.cs:39` | the order-built `FlightOffer.OfferId` is Ordering's `sourceOfferId` |
| `Query/Shopping/Common/Orders/OrderShoppingSource.cs:28` | an order without `sourceOfferId` is refused (422 / 16801 "order … carries no source offer id") |
| `Query/Shopping/Common/FlightOffers/FlightOfferFingerprint.cs:20` | the order fingerprint (the source binding of signed ids) starts with that offer id |
| `Query/Shopping/SeatMaps/Queries/GetSeatMap/SeatMapMapper.cs:61` | every order seat map returns `flightOfferId` = the source offer id |
| `Query/Shopping/AncillaryOffers/Queries/GetAncillaryOffers/AncillaryOfferListingExtensions.cs:17` | the internal listing snapshot carries the offer id (not exposed by `AncillaryOffers/Orders`) |

Proposed correction:

- Ancillary no longer reads `sourceOfferId`, and the order source no longer requires it.
- The order-built air context is identified by the order id.
- The order fingerprint no longer contains an offer id.
- The order seat map answers `orderId` instead of `flightOfferId`, following the rule "outputs identical except offerId becomes orderId".

### R4-1. Shape of the SeatMaps/Orders answer

- **(a)** A separate answer for orders: each seat map carries `orderId` in place of `flightOfferId`; everything else is byte-equal to the offer seat map.
- **(b)** Same as (a), plus `orderCommercialVersion` next to `orderId`, as in `AncillaryOffers/Orders` (R2-5).
- **(c)** Keep `flightOfferId` and fill it with the order id.

Recommendation: **(b)**. Ordering then knows which order version the seat prices belong to, and the answer matches `AncillaryOffers/Orders`.

Answer:(a)

### R4-2. Missing `cabinClassId` in the order answer

- **(a)** Ordering fills `airServices[].cabinClassId` (it owns the booking); Ancillary keeps refusing to guess.
- **(b)** Ancillary falls back to `ReferenceData.AirFares.CabinClassId` of the air service's `airFareId` when Ordering sends null, the same way it already reads the fare family from that table.

Recommendation: **(a)**. The booked cabin is an order fact; a fare-table cabin can differ from the booked one after an upgrade.

Answer:(a)
