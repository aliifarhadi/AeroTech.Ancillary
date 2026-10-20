# AeroTech.Ancillary — Stage 2 Implementation Conformance (v1.1)

| | |
|---|---|
| Date | 2026-10-20 |
| Base | `k8s-stg` @ `7efee95` — all work below is **uncommitted** (no commit, push or merge was made) |
| Authority | Owner scope correction v1.1 (D1–D8) + Owner answers of 2026-10-20 (one new 503 code; Backoffice may preview a customer-bound offer) |
| Verdict | **`S2_INTERNAL_CONTRACT_READY`** — the Ancillary side of Stage 2 is built and tested against fakes. Operational end-to-end with Ordering is **not** claimed: Ordering has no endpoint for the Order commercial context (section 6). Not `S2_CLOSED`. |

## 1. Endpoints

| Route | Surface | State |
|---|---|---|
| `POST /{Backoffice,Api,OtaPanel}/v1/AncillaryOffers` | List | unchanged response; additive input `orderId` (exactly one of `offerId` / `orderId`) |
| `POST /{S}/v1/AncillaryOffers/Details` | Details | unchanged response; additive input `orderId` |
| `POST /{S}/v1/AncillaryOffers/Selections/Evaluate` | Single evaluate | **unchanged** request and response (D5) |
| `POST /{S}/v1/AncillaryOffers/Selections/EvaluateBatch` | Batch | **new**, typed per surface (D5) |
| `POST /{S}/v1/SeatMaps` | Seat map | unchanged route, DTO, status strings and seat-id hash; additive input `orderId` |
| `POST /Service/v1/Ancillaries/Selections/Resolve` | S2S | **new**, anonymous like the existing `Service-Holds` routes (D8) |

There are no `/Orders/...` routes (D6). The frozen `Service-Holds` controller is untouched. Resolve lives in its own `V1/AncillaryOffers/Controllers/ServiceController.cs`. The route pin moved from 150 to 154 routes.

**Batch request** (`{S}EvaluateBatchQuery`): `offerId | orderId`, `selections[]` of `{clientSelectionId, kind: 1=Ancillary|2=SeatMap, offerItemRef + selection | seatMapSelectionId}`. Limits: 1–50 lines, unique `clientSelectionId`, well-formed kind; otherwise 16801/422.

**Batch response**: lines in request order. Each line has `clientSelectionId`, `kind`, `status`, `reasonCodes`, `canPurchase`, `pricedSelectionRef`, `validUntilUtc`, plus either `ancillary` (the surface's existing single-evaluate result, carrying the same verdict) or `seat` (`OfferSeatSelectionDto`). The basket gives `allReady`, `currencyId`, `total`, `sharedFees`, `reasonCodes`.

**Resolve request**: `{orderId:long, expectedOrderCommercialVersion:int, pricedSelectionRefs:string[], correlationId?}`.

**Resolve response**: `{orderId, orderCommercialVersion, correlationId, allValid, results[]}`, with exactly one result per requested ref, in request order:
- `status`: `Valid | Stale | Expired | NeedsQuote | NeedsVerification | Unavailable | Rejected`;
- `reasonCodes`;
- `service`: a typed paid ancillary or paid seat, carrying its real Definition/Provision/Pricing revision, RFIC/RFISC, SSR, Order traveller ids, flight ids, seat and capacity, and exact price;
- or `includedSeat`: traveller, flight, capacity and seat, with fee 0.

Resolve makes no hold, takes no payment and does not change the Order.

## 2. v1.1 decisions as built

| D | How it is built |
|---|---|
| D1 | Stateless JWS (HS256) via `Microsoft.IdentityModel.JsonWebTokens` 8.0.1 (`Common/PricedSelections/PricedSelectionTokens.cs`). The payload `PricedSelection` covers: schema version, source kind, `orderId`, `orderCommercialVersion:int`, source version, signed sales scope, kind, `offerItemRef` or `seatMapSelectionId`, canonical selection or seat facts, coverage and traveller refs, definition/provision/pricing revision, currency, total and units. `jti`, `iat`, `nbf` and `exp` are standard claims. Nothing is stored: no table, no migration, no key ring. The reader rejects anything tampered, signed with another key, unsigned, malformed, over 8 KiB, read before its issue time, or of an unknown schema. Expiry is reported as `Expired`. |
| D2 | Port `Application/Orders/IOrderCommercialContextReader` returning an `OrderCommercialContext` (section 6). `orderCommercialVersion` is an `int` everywhere: `SourceIdentityContext.OrderCommercialVersion` changed from `string?` to `int?`, and the context validator requires it to be >0 for Order sources and absent otherwise. At runtime the reader is `Providers/Orders/NotConnectedOrderCommercialContextReader`, which fails closed with 16815/503. |
| D3 | The SeatMap rule is kept: an eligible seat with no category is `AVAILABLE` at `"0"`. These cases are now `BLOCKED` or an honest error: unknown booked cabin → BLOCKED; occupancy answer missing a requested flight → 16814/502; two points of sale pricing a seat differently → BLOCKED; category without a rate → BLOCKED. Paths, DTO, statuses and hash are unchanged. A free seat resolves as `includedSeat` with no Definition. |
| D4 | A19 is unchanged in shape. It reads as `NeedsVerification` (`GuardianContactRequired` / `VerifiedGuardianContacts`), so no token is ever issued. |
| D5 | A separate `EvaluateBatch` route with typed request and response per surface. The single route keeps its request: the three single-evaluate queries implement `IAncillaryOffersQuery.OrderId` explicitly (`=> null`), so the public request members are unchanged (pinned by `P32B_AGY_02`). |
| D6 | One dispatcher, `Common/ShoppingSource`: sending both ids → 16801/422; `orderId` → Order source; otherwise the Offer source as before. An `orderId` is never written into `flightOfferId`: the seat map's `flightOfferId` is the Order's source offer id, and a missing one is refused with 16801/422. |
| D7 | Delivered in four internal stages, each built and tested before the next. |
| D8 | `ICallerContext`, `SurfaceAuthorization`, JWT, policies and middleware are untouched. Resolve has no `[Authorize]`, like `Service-Holds`. Only the approved code 16815 is new. |

## 3. Source issues fixed (v1.1 section 4)

| # | Fix | Tests |
|---|---|---|
| 1 | `ChargedUnits` derives charged units from the selection contract and quantity rule. A typed-form service is one unit; a Quantity field is respected; PerKilogram stays without a line total. | `PRICE_01`, `PRICE_04`, `PRICE_06`, `E_A15` |
| 2 | Explicit free is 0 in the context currency. Quoted, refused and unknown prices stay null. | `PRICE_15`, `P32B_ENG_04` (renamed: free is now an explicit zero), `PRICE_05` |
| 3 | List `canPurchase` = Selectable + unit price complete + no required customer input. Details contract unchanged. | `CT_10`, goldens |
| 4 | Exact ordinal choices (A02 package ref, A24 plan, meal/menu/equipment codes). | `SEC_13`, `E_A06`, `E_A11` (renamed: the meal is priced at zero) |
| 5 | Fingerprint covers sale channel, customer, country, bounds, flights, legs, fare cabin/RBD/booking class and coupon usability. The guard refuses a foreign customer. Backoffice staff (office scope, no customer) may preview a customer-bound offer. | `VIEW_15`, `SEC_07` ×2 |
| 6 | SeatMap gaps closed (D3). | `SM_09`–`SM_12`, `SM_03` |
| 7 | A17 medical approval and A19 guardian contacts are `NeedsVerification`. A18 needs the real infant–guardian pair, a recorded link and the infant's age. | `E_A17`, `E_A19`, `S2_A18` |
| 8 | Non-Item fees are never folded into a line. The basket lists them as shared fees and is not ready. | `PRICE_12` |

Also built:
- Post-ticketed coupon state: `FlightOfferCoupon.IsOpenForUse` → `TravellerFareFacts.CouponOpenForUse` → engine `CouponUsability`. A used coupon is not eligible for that traveller and flight only; an unknown state leaves the candidate unverified. Tests: `FLOW_03_FLOW_10`, `FLOW_09`.
- A pet subject to confirmation is checked at reserve, without the "must check availability" claim. Test: `S2_A13`.

## 4. Per-variant results (A01–A24)

Source: `S2VariantMatrixTests` (25/25 green). Each row shows one positive selection on a lab catalogue at 100.00, and the same candidate with an empty selection. The empty selection is never accepted and never purchasable. A token is issued only when the result is purchasable and the source is an Order.

| Variant | Kind | Positive status | Line total | Token | Note (test row) |
|---|---|---|---|---|---|
| A01 | QuantityChoice | Accepted | 100.00 | yes | purchasable |
| A02 | QuantityChoice | Accepted | 100.00 | yes | package ref = authored definition ref |
| A03 | TypedForm | Accepted | 100.00 | yes | per piece; PerKilogram stays without total (`PRICE_06`) |
| A04 | TypedForm | Incomplete | — | no | fail-closed: charge combination not verified |
| A05 | QuantityChoice | Accepted | 100.00 | yes | purchasable |
| A06 | TypedForm | Accepted | 100.00 | yes | exact equipment kind |
| A07 | SeatMapSelection | Accepted | 100.00 | via seat map | bought by seat-map id in a batch |
| A08 | SeatMapSelection | Incomplete | — | no | exit-row terms not given (open point 1) |
| A09 | SeatMapSelection | Incomplete | — | no | fail-closed: adjacency not verified |
| A10 | ExternalQuote | Incomplete | — | no | NeedsQuote |
| A11 | TypedForm | Accepted | 0.00 | yes | explicit free |
| A12 | QuantityChoice | Accepted | 100.00 | yes | exact menu item |
| A13 | TypedForm | Incomplete | — | no | fail-closed: animal age not verified |
| A14 | TypedForm | Incomplete | — | no | fail-closed: animal age not verified |
| A15 | TypedForm | Accepted | 0.00 | yes | free assistance |
| A16 | TypedForm | Accepted | 0.00 | yes | free assistance |
| A17 | TypedForm | Incomplete | — | no | NeedsVerification: medical approval |
| A18 | TypedForm | Accepted | 0.00 | yes | verified infant–guardian pair |
| A19 | TypedForm | Incomplete | — | no | NeedsVerification: guardian contacts (D4) |
| A20 | TypedForm | Accepted | 100.00 | yes | one unit per passenger |
| A21 | TypedForm | Accepted | 100.00 | yes | one unit per passenger |
| A22 | TypedForm | Accepted | 100.00 | yes | per passenger; Order scope with a per-passenger rate is `BeneficiaryRequired` (`FLOW_07`) |
| A23 | SimpleOptIn | Accepted | 0.00 | yes | free opt-in |
| A24 | TypedForm | Accepted | 100.00 | yes | plan = authored definition ref |

## 5. Seat-ID demo (end to end, Order source)

`S2OrderSourceTests.SEAT_03` and `S2SelectionResolveTests.SEAT_03` run this flow on the captured AirAvail offer, served as Order 7001 at commercial version 4:

1. `SeatMaps {orderId: 7001}` returns the legacy 43-character ids: seat 3A for CHD-1 (paid category, 15.00) and seat 3B for ADT-1 (no category, `"0"`).
2. `EvaluateBatch {orderId: 7001}` with the bag line and both seat ids gives three Accepted lines. The basket is ready with total 115.00 and no reason codes. Every line gets a `pricedSelectionRef` that expires at evaluation time + `Shopping:SourceContextTtl`.
3. Reading each reference back gives `Valid` with Order 7001, version 4 and the signed totals 100 / 15 / 0. The paid seat carries traveller CHD-1, seat 3A, its seat-map id and its flight capacity id.
4. `Resolve {orderId: 7001, expectedOrderCommercialVersion: 4}` returns:
   - three `Valid` results;
   - the bag as `service` (`EXTRA_BAG`, Order traveller 101);
   - seat 3A as `service` (`SEAT_STANDARD`, Order traveller 102, flight capacity id);
   - seat 3B as `includedSeat` (Order traveller 101, fee 0, no service).

Negative paths are also tested:

| Case | Result | Tests |
|---|---|---|
| Seat taken since the batch, or seat category withdrawn | `Stale`, reason `SeatSelectionNotOffered` | `SEAT_07`, `SEC_10_SEC_11` |
| Same reference twice | `Rejected` (`DuplicateSelection`, `SeatSelectedTwice`) | `PRICE_20` |
| Invented 43-character id | not offered | `SEAT_07_SEAT_13` |
| One seat for two travellers, or two seats for one traveller | both lines rejected | `SEAT_05_SEAT_06` |

## 6. Ordering consumer contract and the external gap

**What Ancillary reads** (`OrderCommercialContext`). Each field is checked against Ordering.Final `cd50a2a`:

| Field | Ordering source |
|---|---|
| `OrderId`, `CommercialVersion:int` | `Order.cs:64` (`CommercialVersion`) |
| `AcceptsChanges` | derived by Ordering from `Order.Status` (`Order.cs:62`); Ordering owns the rule |
| `TicketedAtUtc` | `ElectronicTicket.IssuedAt` (`ElectronicTicket.cs:59`) |
| `Air.OfferId` | `Order.SourceOfferId` (`Order.cs:56`) |
| `Air.CustomerId` / `SalesChannel` | `Order.CustomerId` (`:50`) / `SalesContext.Channel` (`SalesContext.cs:47`). Ordering's `SalesChannel` numbers differ from Ancillary's `Channel`, so the adapter must translate. |
| `Air` bounds, flights, legs, fares, baggage, flags | `OrderJourney`, `OrderSegment`, `OrderSegmentLeg`, `OrderAirTransportService` (`FlightCapacityId:54`, `AirFareId:56`, `RbdId:66`, `CabinClassId:68`, `CheckedBaggageAllowance:70`, `IsUpgradable:78`), `OrderFareComponent` |
| `Air.Tickets[].Coupons[].IsOpenForUse` | `TicketCoupon` control/status (`TicketCoupon.cs:82–84`); Ordering's `CouponStatus.OpenForUse = 2` |
| `Travellers[]` `TravellerRef`, `OrderTravellerId`, `DateOfBirth`, `InfantOfTravellerRef` | `OrderTraveller.SourceTravellerRef:36`, `Id`, `TravellerProfileRevision.DateOfBirth:53`, `OrderTraveller.InfantParentTravellerId:42` |
| `Services[]` | `OrderService` (`TravellerId:41`, `CommercialStatus:47`). **Gap:** Ordering has no Ancillary `ServiceDefinitionRef` on a service yet. |

**Gap.** Ordering's `ServiceController` (`Service/v1/Bookings`) has no actions, so there is no endpoint to read the context. At runtime every `orderId` request therefore ends in **16815/503** by design (FLOW-13). Nothing about Ordering's URL or response was invented. These are Ordering-side requirements, not yet built:
1. a read endpoint that returns the fields above;
2. Add-to-Order calling `Resolve` and persisting `service` lines;
3. persisting `includedSeat` as a seat assignment without a fake SKU;
4. idempotency and concurrency on Add (SEC-19..21).

**How errors map for Order sources:**

| Case | Result |
|---|---|
| Unknown order, or another customer's order | 16809/403, alike (FLOW-16) |
| Order closed for changes, or without a source offer id | 16801/422 (FLOW-11, SEAT-18) |
| Ordering unreachable | 16815/503 |
| Both `offerId` and `orderId` sent | 16801/422 (FLOW-15) |

The flight-offer provider is never called for an Order source (FLOW-12).

## 7. Configuration

| Setting | Behaviour |
|---|---|
| `Shopping:SelectionSigningKey` (env `Shopping__SelectionSigningKey`) | At least 32 bytes. It is checked when a token is issued or read, not at startup. Without it, Order-bound batch and Resolve fail closed (500 `InvalidOperationException`); List, Details, single Evaluate and offer-based Batch keep working. The dev key lives only in the git-ignored `appsettings.Development.json`. Staging and production must set the environment variable. |
| `Shopping:SourceContextTtl` | Existing setting; also the token lifetime, through the context's `ValidUntilUtc`. |

## 8. Pack tests superseded by v1.1 (not failures)

| Pack test | Reason |
|---|---|
| SEC-15, SEC-16, SEC-17, SEC-18 (JWT, scope, allowlist on Resolve) | D8: Resolve follows the existing anonymous S2S pattern |
| SEC-02 key id / revocation | D1: no key ring. A foreign-key signature is still rejected (`SEC_02`). |
| SEAT-10 / SEAT-23 no-category → BLOCKED | D3: no-category stays AVAILABLE at 0. A category without a rate is BLOCKED (`SM_11`). |
| `/Orders/...` route tests | D6 |
| `NotAuthorized` resolve status | v1.1 section 3 status set |

## 9. Open points for the Owner (not decided in code)

1. **A08 exit-row seats in a batch.** A seat line carries only the seat-map id (v1.1), so `ExitRowTermsAccepted` cannot be given. A paid exit-row seat therefore stays Incomplete and gets no token. Candidate: an optional typed `selection` on seat lines.
2. **Shared fees** (Ticket, OneWay, RoundTrip, Sector). They are listed once per basket and never charged, and the basket is then not ready. Applying them per ticket or journey (PRICE-10/11) needs a rule decision.
3. **Seats already held in the Order** (`OrderSeatService`). They are not in the reader contract. Occupancy still comes from FlightFlow.
4. **Order not found is answered 403**, like a foreign order, so that no order leaks. No new 404 code was added (D8).
5. **Resolve failures are whole-request errors** when they hit the whole Order: closed, unknown, unreachable, or a guard failure (422/403/503). Per-reference statuses cover everything else.
6. **A priced drift appears as `Stale` with `OfferItemNotOffered`**, because item refs encode the pricing revision and the source version. The `PriceChanged` branch is kept defensively.

## 10. Other changes worth review

- **Goldens:** only item refs (from the widened fingerprint) and A01 `canPurchase` (`true` → `false`, since Quantity is still required) changed. No other DTO field changed.
- **Dead branch removed:** a passenger-type-only rate on an Order unit cannot exist. The domain allows passenger-type and age rates only with PerPassenger pricing, which already answers `BeneficiaryRequired`.
- **Services load once:** `GetAncillaryOffersService.ShopAsync`, `GetAncillaryOfferDetailService.DescribeAsync`, `EvaluateSelectionService.EvaluateAsync` and `GetSeatMapService.BuildAsync` let one batch load the source once and evaluate every line on one source version.

## 11. Commands and counts

```
dotnet build AeroTech.Ancillary.sln --no-incremental      -> 0 errors, 62 warnings (same as the stage-1 clean build)
dotnet test tests/AeroTech.Ancillary.Shopping.Tests        -> 317 discovered, 317 passed, 0 failed
dotnet test tests/AeroTech.Ancillary.Domain.ConformanceTests -> 259 discovered, 259 passed, 0 failed
dotnet test tests/AeroTech.Ancillary.Application.AcceptanceTests -> 224 discovered, 224 passed, 0 failed
```

No run reported "aborted". Discovered counts come from `dotnet test --list-tests`. No live HTTP run was made, and no database or migration was touched.

## 12. Changed files

**Modified (75)**

| Area | Files |
|---|---|
| Engine | `ChargedUnits` (new), `PriceAssessor`, `PriceAssessment`, `ProvisionEvaluation` (+`CouponUsability`), `ProvisionEvaluation.ProfileRules`, `AvailabilityAssessor`, `AncillaryShoppingEngine`, `AncillaryShoppingEngine.SeatMaps`, `ShoppingContextValidator`, `ShoppingReasonCodes`, `SourceIdentityContext`, `TravellerFareFacts` |
| Query | offer, detail, evaluate and seat-map services and interfaces; the three single-result mappers (verdict overload); `OfferSelectionResultsMapper`; `SelectionFieldValues`; `OfferPurchase`; `AncillarySalesScope`; `PricedOfferScopeGuard`; `FlightOfferFingerprint`; `FlightOfferShoppingSource`/`IFlightOfferShoppingSource`; `FlightOfferShoppingContextMapper`; `SeatMapSnapshot`; nine List, Details and SeatMaps queries (+`OrderId`); three single-evaluate queries (explicit `OrderId`); `DependencyInjection`; csproj |
| Application / Providers / Domain / RestApi | `FlightOfferCoupon`; Providers `DependencyInjection`; `ExceptionFactory`/`ExceptionMessages` (16815); three offer controllers (+`EvaluateBatch`) |
| Tests | `V12BoundaryAcceptanceTests` (routes, Providers pin), nine goldens, `AncillaryOffersTests`, `BackofficeSurfaceParityTests`, `EnginePriceConformanceTests`, `EnginePriceInventoryTests`, `EngineVariantTests`, `S201ContextCoverageTests`, `S2FinalShoppingTests`, `SeatMapTests`, `ThreeSurfaceTests`, `Fixtures/BackofficeDesk` |

**New (79)**

| Area | Files |
|---|---|
| Contracts | `SelectionLineKind`, `SelectionResolutionStatus` |
| Application | `Orders/` (reader port, context, traveller and service facts) |
| Providers | `Orders/NotConnectedOrderCommercialContextReader` |
| Query | `Common/PricedSelections/` (8), `Common/Orders/` (4), `Common/IShoppingSource` + `ShoppingSource`, `Queries/EvaluateBatch/` (canonical + three surfaces, 21), `Queries/ResolveSelections/` (6), snapshots (6), DTOs (13) |
| RestApi | `V1/AncillaryOffers/Controllers/ServiceController` |
| Engine | `ChargedUnits` |
| Tests | `S2PricingCorrectnessTests`, `S2OfferScopeTests`, `S2SelectionBatchTests`, `S2PricedSelectionTokenTests`, `S2OrderSourceTests`, `S2SelectionResolveTests`, `S2VariantMatrixTests`, `Fixtures/SeatMapDesk`, `Fixtures/SelectionDesk`, `Fixtures/OrderDesk` |

The Owner's untracked `docs/ancillary_s2_execution_pack/` and the pack zip were not modified.
