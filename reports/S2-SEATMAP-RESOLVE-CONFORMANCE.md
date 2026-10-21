# S2 SeatMap Resolve — recoverable seat id and `POST /Service/v1/Ancillaries/SeatMaps/Resolve`

| Item | Value |
|---|---|
| Base | `k8s-stg@ca4242b` "Reports and E2E test Added" |
| Authority | Owner chat 2026-10-21 + `reports/S2-SEATMAP-RESOLVE-QUESTIONS-AND-CONFLICTS.md` (Q1–Q10 answered) + two AskUserQuestion answers (scope from offerId + provision; `expectedOrderCommercialVersion` required with `orderId`) |
| Commit | none; everything is in the working tree, uncommitted |
| Verdict | `S2_SEATMAP_RESOLVE_BUILT_AND_TESTED` — one Domain invariant changed beyond the brief (section 6, needs the Owner's confirmation) |

## R2 (2026-10-22) — replaces sections 3 and 4 for SeatMaps/Resolve

Owner order: the id stays `{travellerRef}|{flightId}|{pricingId}|{SEATNUMBER}`; the endpoint only reaches the pricing, its provision and its definition and returns them with the seat number, flight id and traveller ref, like Ordering reads AirAvail Details. Nothing from the offer or the order.

- **Input:** `POST /Service/v1/Ancillaries/SeatMaps/Resolve` `{ "ids": [ … ] }`. Removed: `offerId`, `orderId`, `expectedOrderCommercialVersion`, `seats[].exitRowTermsAccepted`, `correlationId`.
- **Output:** `{ "seats": [ { id, travellerRef, flightId, seatNumber, serviceDefinition{…}, provision{…}, pricing{…} } ] }` in request order. Each group is an object and each enum is `{ value, name, title }`:
  - `serviceDefinition`: `id, serviceDefinitionRef, version, supplierId, commercialName, description, profile, variantCode, serviceTypeCode, serviceSubCode, subCodeSource, groupCode, subGroupCode, description1Code, description2Code, pricingUnit, serviceDateBasis, booking{method, ssrCode, ssimCode, confirmationRequirement}, document{type, routing, rfic, rfisc}, seat{purpose, requiresExitRowEligibility, requiresAdjacentSeat, extraSeatPurpose, extraOccupiedSeatCount, requiresExternalTicketAction}`.
  - `provision`: `id, sequence, coverageScope, purchaseStage, priceOrigin, applicationType, quantity{unit, minQuantity, maxQuantity}, outcome{disposition, documentRequired, bookingRequired}, settlement{reissueRefund, formOfRefund, commissionable, interlineSettlement}, availability{mustCheckAvailability}, fulfillment{fulfillmentProviderKey}`.
  - `pricing`: `id, version, pricingUnit, rates[{ id, passengerTypeCode, ageFromInclusive, ageToExclusive, currencyId, baseAmount, unitTotal, isUnitTotalComplete, components[{ id, category, code, name, countryId, stationAirportId, amount, feeApplicationUnit, taxTreatment }] }]`. Every rate is returned; Ordering picks the one for its currency and passenger type.
- **Errors:** 422 / 16801 for no ids, more than 50, a repeated id, an unreadable id, the same seat of a flight twice, or two seats for one traveller on one flight. 404 / 16811 for an id whose pricing, provision or definition is not Active, or whose definition is not a Seat.
- **No longer checked here:** the seat map, occupancy, eligibility, the order version and exit-row terms. The seat map still shows only selectable seats; availability belongs to the reservation step.
- **Code:** `ResolveSeatMapSelectionsService` now depends only on the three catalog readers; new `IActiveAncillaryDefinitionReader.FindActiveDefinitionsAsync` (+ SQL reader); 15 nested DTOs under `Query/Shopping/SeatMaps/Dto/Service/`; removed `SeatMapSelectionRequest`, `ServiceSeatMapSelectionInput`, `ServiceSeatPriceDto`, `ServiceSeatPriceLineDto`.
- **E2E project deleted** on the Owner's order (`tests/AeroTech.Ancillary.Stage2.EndToEndTests`, removed from `AeroTech.Ancillary.sln`).
- **Proof (2026-10-22):** `dotnet build AeroTech.Ancillary.sln --no-incremental` 0 errors; Shopping tests 362 discovered / 362 passed, nothing aborted; live call on a side host (port 5299, stopped afterwards) with seat 15D's id answered 200 with `ECO_SEAT_STANDARD` / provision `1560040688677027840` / pricing `1560040688677727841`, one IRR rate with unit total 0.

## 1. What changed, in one paragraph

A seat's `id` in every SeatMaps answer is now `base64url(travellerRef | flightId | ancillaryPricingId | SEATNUMBER)`, not a hash. A seat has an id only when it is `AVAILABLE` with a priced category; a seat with no category is `BLOCKED`, so the "included seat" concept is gone and a free seat is a Paid provision whose Active pricing rate is 0. Seats are resolved by a new anonymous S2S route `POST /Service/v1/Ancillaries/SeatMaps/Resolve` (by `orderId` + `expectedOrderCommercialVersion`, or by `offerId`), which rebuilds the map from the current source, re-prices each id, and answers `Valid | Stale | Rejected` per seat with numeric catalog ids and a numeric price. Seat lines were removed from EvaluateBatch on the three surfaces and `includedSeat` from the selection Resolve; the old Resolve answers ancillaries only.

## 2. Owner decisions applied

| # | Decision | Where |
|---|---|---|
| Q1 = a | free seat = Paid provision + Active pricing with rate 0 | `AncillaryShoppingEngine.SeatMaps.cs` (a seat is Available only with a priced category); Domain rate guard (section 6) |
| Q2 | no category / Free-origin provision → BLOCKED, no id; overrides the 2026-10-14 "no category = free AVAILABLE" ruling | engine + `GetSeatMapService.cs` |
| Q3 | id shape like AirAvail `JourneyOfferIdCodec`, not signed | `Query/Shopping/SeatMaps/SeatMapSelectionId.cs`, `SeatMapSelection.cs` |
| Q4 | `flightRef` in the id = platform `flightId` (long) | `SeatMapSelectionId.Encode(travellerRef, flightId, pricingId, seatNumber)` |
| Q5 = a | seat lines out of EvaluateBatch; `includedSeat` out of Resolve | `SelectionLineRequest`, the three `*SelectionLineInput(+Mapper)`, `*SelectionLineDto`, `SelectionLineSnapshot`, `PricedSelection(+Mapper)`, `SelectionBasketRules`, `EvaluateBatchService`, `ResolveSelectionsService`, `ServiceResolvedServiceDto`, `ServiceResolvedSelectionDto`, `ServiceSelectionResolutionMapper`; deleted `SeatSelectionSnapshot`, `PricedSeat`, `OfferSeatSelectionDto`, `ServiceIncludedSeatDto` |
| Q6 | per-id `exitRowTermsAccepted`; acceptance is not eligibility | `SeatMapSelectionRequest`, `ResolveSeatMapSelectionsService` (eligibility stays in the engine's exit-row rule; unverified adult and child are BLOCKED on the map) |
| Q7, Q9 | output per seat with numeric `serviceDefinitionId`, `provisionId`, `pricingId`, `price { currencyId, baseAmount, taxes[], fees[], total }` | `Dto/Service/ServiceResolvedSeatDto.cs`, `ServiceSeatPriceDto.cs`, `ServiceSeatPriceLineDto.cs`, `ServiceSeatMapResolutionDto.cs`, `ServiceSeatMapResolutionMapper.cs` |
| Q8 | statuses `Valid | Stale | Rejected`; `allValid` = all Valid | `SeatMapResolutionSnapshot.cs` |
| Q10 | offer path: priced preview, `orderTravellerId` null | `ResolveSeatMapSelectionsService.ResolveAsync(orderBound: false)` |
| scope | channel/customer/country from the offer id, home airline from `IOperatorAirlineResolver`, POS from the provisions of the pricings in the ids, else every known POS | `ResolveSeatMapSelectionsService.ScopeAsync` |
| version | `expectedOrderCommercialVersion` required with `orderId` | `WellFormed` |

## 3. Contract before / after

**SeatMaps (three surfaces, frozen DTO):** only the value of `travelerPricing[].id` changed (43-char hash → base64url of four parts; length varies). A seat with no category now answers `BLOCKED` with `id = null` and `price = null`; previously it answered `AVAILABLE` at `"0"`.

**EvaluateBatch (Api, Backoffice, OtaPanel):** a line is `{ clientSelectionId, offerItemRef, selection, selectedOptionRefs? }`. Removed: `kind`, `seatMapSelectionId`, `exitRowTermsAccepted` on input; `kind` and `seat` on output. A line without `offerItemRef` is 422 / 16801.

**Selections/Resolve (Service):** `results[].service` lost `kind`, `seatNumber`, `flightCapacityId`; `results[].includedSeat` is gone. `SeatSelectionNotOffered` no longer appears here.

**SeatMaps/Resolve (Service, new):** request `{ offerId? | orderId?, expectedOrderCommercialVersion?, seats: [{ id, exitRowTermsAccepted? }] (1..50), correlationId? }`; response `{ offerId, orderId?, orderCommercialVersion?, correlationId?, allValid, seats: [{ seatMapSelectionId, status, reasonCodes[], travellerRef?, orderTravellerId?, flightId?, flightCapacityId?, seatNumber?, serviceDefinitionId?, provisionId?, pricingId?, fulfillmentProviderKey?, price? }] }`. Full field and reason-code tables: `S2-ORDERING-HANDOFF-CONTRACT.md` §1b; captured examples in its §4.

Wire note: the ids are `long` in the DTOs, as the Owner asked; the platform's JSON converter renders every `long` as a JSON string on the wire (the same way `orderId` is rendered today), so a consumer sees `"pricingId": "1031"` and `"total": 1500000`. I did not override the platform converter.

Errors: 16801 (422) malformed request / both or neither source / `orderId` without a version / 0 or >50 seats / empty or repeated id; 16809 (403) unknown order or no home airline; 16808 (409) no point of sale known for the pricing; 16815 (503) Ordering unreachable; 16814 (502) occupancy source failure (unchanged).

## 4. Behaviour of the seat Resolve

Per seat, in order: order version mismatch → `Stale OrderVersionChanged` (whole request, nothing priced); in-request duplicate (same flight + seat, or same flight + traveller) → `Rejected SeatSelectedTwice` / `TravellerSeatSelectedTwice`; undecodable id, or a flight/seat/traveller that is not on the rebuilt map → `Rejected SeatSelectionNotOffered`; seat not `AVAILABLE` or without a candidate → `Stale SeatSelectionNotOffered`; same seat offered under another pricing id → `Stale PriceChanged`; order path with a traveller lacking a positive `OrderTravellerId` → `Rejected OrderTravellerNotLinked`; the engine's `EvaluateSelectionAsync` with `{ seatNumber, exitRowTermsAccepted }` not purchasable → `Rejected` with the engine's reason codes (field issues included); an unapplied fee → `Rejected SharedFeeNotApplied`; else `Valid` with the candidate's ids, provider key and exact price.

The order path reads the order twice: once through `IOrderCommercialContextReader` to get the source offer id for the scope, then again inside `OrderShoppingSource`. Both reads hit the same reader; no second HTTP contract is involved. If the Owner prefers one read, the scope would have to be derived inside the source instead (not done, not asked).

Exit row: the Owner said "see how Amadeus handles exitRow, follow it". To my recollection the public Amadeus Seat Map Display marks exit-row seats with characteristic `E` and carries no acceptance field; eligibility is the airline's passenger rule. I did not verify this against Amadeus documentation during this task; it is labelled a recollection. The implementation therefore keeps acceptance as a per-id boolean in the Resolve request and keeps eligibility (`VerifiedExitRowEligible`, age) in the engine, so a child or an unverified adult never sees the seat as selectable regardless of acceptance.

## 5. Files

Production (`src/`): 4 new Query folders/files for the seat Resolve (`SeatMapSelection.cs`, `SeatMapSelectionId.cs`, `Snapshots/ResolvedSeatSnapshot.cs`, `Snapshots/SeatMapResolutionSnapshot.cs`, `Queries/ResolveSeatMapSelections/*` (request, interface, service, Service query/handler/mapper/input), `Dto/Service/*` (4 DTOs)), 1 new controller `RestApi/V1/SeatMaps/Controllers/ServiceController.cs`, `Query/DependencyInjection.cs` (+1 registration), `GetSeatMapService.cs` (id encoding), `AncillaryShoppingEngine.SeatMaps.cs` (no category → Blocked), the EvaluateBatch/Resolve files listed under Q5, reader ports `IActiveAncillaryPricingReader.FindActivePricingsAsync` and `IActiveAncillaryProvisionReader.FindActiveProvisionsAsync` with their SQL readers, `Common/OfferItemRef.cs` (seat prefix removed), and `Domain/AncillaryPricingAggregate/Entities/AncillaryPricingRate.cs` (section 6). Four files deleted. `git status`: 56 modified, 4 deleted, 11 untracked paths.

Tests: `Shopping.Tests` rewritten where seats were batch lines (`S2SelectionBatchTests`, `S2SelectionResolveTests`, `S2ExitRowSeatTests`, `S2OrderSourceTests` SEAT_03, `S2BasketCommitTests`, `S2OptionRefTests`, `S2ResolveHandoffTests`, `S2PricedSelectionTokenTests`, `SeatMapTests` SM_01/02/03/07/11/12), new `S2SeatMapResolveTests` (SEATID_01–02, SEATRES_01–13), fixtures `SelectionDesk.SeatResolveAsync` and `InMemoryCatalog`; `Domain.ConformanceTests` PR17, V12_P07, M1_I01 (zero base accepted); `Application.AcceptanceTests` route pin 154 → 155 and the frozen route list (+ `POST Service/Ancillaries/SeatMaps/Resolve`), `V12_P04_P07` (a zero base is now defined on a fresh provision instead of being refused); E2E `Stage2Catalog` (zero-rated `SEAT_STANDARD_MIDDLE` on `9` seats), `Stage2Flows` (`SeatRequest`, `SeatResolveAsync`, `AddAsync(seats)`), new `SeatResolveView`, `AddAncillariesRequest(+Seats)`, new `AddSeatRequest`, `OrderingAddService` (calls both Resolve routes, validates and stores seats), `TestOrderRecords`, `C3SeatMapResolveTests` (C3_01–C3_05), `C4_F4`, `C5_07`.

Reports: this file; `S2-ORDERING-HANDOFF-CONTRACT.md` §1.3, new §1b, §3 and §4 examples regenerated from the 2026-10-21 transcripts; `S2-STANDALONE-HTTP-E2E-CONFORMANCE.md` addendum.

## 6. Beyond the brief — needs the Owner

**Domain invariant changed.** `AncillaryPricingRate` required `BaseAmount > 0`; Q1 = a (a free seat is a pricing with rate 0) cannot be authored otherwise, so the guard became `>= 0` (`Money.Of` still refuses negatives). This relaxes rule PR17 "Paid zero base → reject" of `docs/AeroTech-Ancillary-v12.1/03-PRICING-BEHAVIOR-AND-EDGE-CASES.md` for every product, not only seats, because the pricing aggregate does not know the definition's variant. Tests PR17, V12_P07 and M1_I01 were changed accordingly. The doc row was not edited (PRD changes need permission). If the Owner wants zero rates limited to seat variants, the check must move to the Application `DefinePricing` use case, where the provision → definition is known.

**Not touched, by rule:** `docs/`, Ordering, Contracts, migrations (no schema change), commit/push.

## 7. Proof (commands run on 2026-10-21)

| Command | Result |
|---|---|
| `dotnet build src/AeroTech.Ancillary.ServiceHost --no-incremental` | 0 errors, 34 warnings (pre-existing) |
| `dotnet test tests/AeroTech.Ancillary.Shopping.Tests --no-build` | Passed 379, Failed 0, Skipped 0, Total 379, no aborted line |
| `dotnet test tests/AeroTech.Ancillary.Domain.ConformanceTests --no-build` | Passed 259, Failed 0, Skipped 0, Total 259, no aborted line |
| `dotnet test tests/AeroTech.Ancillary.Stage2.EndToEndTests --no-build` | Passed 31, Failed 0, Skipped 0, Total 31, no aborted line |
| `dotnet test tests/AeroTech.Ancillary.Application.AcceptanceTests --no-build` | Passed 224, Failed 0, Skipped 0, Total 224, no aborted line (2 m 58 s). A first run of the suite took 9 h while the machine was suspended and timed out two SQL-backed tests; both passed alone and in this full rerun. |
| `crlf_status_check.py` over every changed and new file | checked 78, 0 lone LF |
| `grep` for `//` and `/*` in changed `.cs` files | none |
