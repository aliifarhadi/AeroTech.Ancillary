# Grouped Ancillary API — Gate A conformance

| Item | Value |
|---|---|
| Authority | `AeroTech-Ancillary-Grouped-API-OWNER-DECISIONS-v1.1-2026-10-22.md` + Owner answers to the preflight (2026-10-22) |
| Baseline | `k8s-stg@49e9209a40e046fe71c7913cc1fcf8a8fca6799d` |
| Branch | `feat/ancillary-grouped-api-gate-a` (local), UNCOMMITTED, no push |
| Verdict | `SEATMAP_FROZEN_CONFIRMED`, `GROUPED_SHOPPING_READY` (Gate A only; `selectionId` is null, not ready to purchase, no frontend release) |

## 1. Endpoints

| Method | Route | Body | Answer |
|---|---|---|---|
| POST | `Backoffice/v1/AncillaryOffers/Grouped` | `{ offerId }` or `{ orderId }`, optional `options { pointOfSaleId, countryId }` (same as the legacy Backoffice list) | `BackofficeGroupedAncillaryOffersDto` |
| POST | `OtaPanel/v1/AncillaryOffers/Grouped` | `{ offerId }` or `{ orderId }` | `OtaPanelGroupedAncillaryOffersDto` |

Both sit in new controller classes `GroupedBackofficeController` / `GroupedOtaPanelController` beside the legacy ones, same route prefix, same `[Authorize]` policy and tag. Both-or-neither of offerId/orderId is refused by the existing `ShoppingSource` (422 / 16801). No Api route was added.

## 2. Response shape

`meta { offerId?, orderId?, currencyId, evaluatedAtUtc, shoppingStage }`, `seats[] { flightId, seatMapEndpoint }`, then nine arrays always present: `baggages`, `meals`, `pets`, `assistedTravel`, `airportServices`, `priorityServices`, `connectivity`, `upgrades`, `extraSeats` (A09, per your correction).

Every item carries the common fields first (`offerItemRef`, `variantCode`, `serviceDefinitionRef`, `name`, `travellerRefs`, `flightRefs`, `portionRefs`, `coverageScope`, `readiness`, `price { currencyId, unitTotal, isTotalComplete }`, `selectionId` = null, `requiresDetails`, `detailsEndpoint`) and then only the typed fields of its family, read from the definition specification. Enums are `{value,name,title}` on Backoffice and names on OtaPanel. `seats[]` lists every flight of the source in bound order; the seat engine is not run. Items shown = readiness Selectable, NeedsSelection or NeedsQuote. `requiresDetails` = the existing `OfferInlineOptions.Interaction` verdict (a quantity range alone does not require details). `detailsEndpoint` = `/{Surface}/v1/AncillaryOffers/{Family}/Details` when details are required; null for `extraSeats` because no A09 details route exists in the spec. `shoppingStage` comes from the shopping context (PreOrder / PostTicketed).

Samples (test fixture, captured offer, IRR): scratch `grouped/samples/backoffice-grouped.json`, `otapanel-grouped.json` (4 baggage, 4 connectivity, 4 extra-seat items, 2 seat links; A07 absent, A13 pet NeedsVerification absent).

## 3. Paths (37, all uncommitted)

- Query: `Snapshots/GroupedAncillaryOffersSnapshot.cs`, `Snapshots/GroupedOfferItem.cs`; `Queries/GetGroupedAncillaryOffers/{IGetGroupedAncillaryOffersService, GetGroupedAncillaryOffersService, GroupedOfferRoutes, GroupedOfferSpecifications}.cs`, `Backoffice/{Query, QueryHandler, Mapper}`, `OtaPanel/{Query, QueryHandler, Mapper}`; 14 DTO files under `Dto/Backoffice`, 14 under `Dto/OtaPanel` (one type per file); `DependencyInjection.cs` registration.
- RestApi: the two controllers.
- Tests: `tests/AeroTech.Ancillary.Shopping.Tests/GroupedOffersTests.cs` (GRP_A01–A06); `V12BoundaryAcceptanceTests` route register 158 → 160 with the two routes added. `P32B_SRF_02` and `P32B_SRF_05` untouched.
- Reports: this file and `GROUPED-ANCILLARY-PREFLIGHT.md`.

The shared service calls the existing `IGetAncillaryOffersService` with `OfferFamilyFilters.Services` and reloads the active definitions once (as the detail service does); no change to the legacy snapshot, mappers, engine or Seat code.

## 4. Proof (2026-10-22, final tree)

| Check | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 errors |
| Domain conformance | 264 listed, 264 passed |
| Shopping | 374 listed, 374 passed (368 + 6 new), no aborted |
| Application acceptance (SQL) | 232 listed, 232 passed, no aborted |
| Freeze diff (`git diff --name-only 49e9209 -- SeatMaps paths`) | empty |
| Line endings | 46 changed files checked, 0 lone LF; no comments |

Domain and Application ran on the tree before the last cosmetic change (JSON property order on the base DTO records + one test assertion); the clean build and the Shopping suite were repeated after it.

## 5. Not done / not in Gate A

- No live HTTP run (no token; Gate E).
- No OpenAPI capture.
- Gate B–E not started.
