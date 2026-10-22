# Grouped Ancillary API — Gate A preflight

| Item | Value |
|---|---|
| Authority | `AeroTech-Ancillary-Grouped-API-OWNER-DECISIONS-v1.1-2026-10-22.md` (chat attachment) under `CLAUDE.md` |
| Baseline | `k8s-stg@49e9209a40e046fe71c7913cc1fcf8a8fca6799d`, tree clean at preflight |
| Working branch | `feat/ancillary-grouped-api-gate-a` (local only, no commit) |
| Scope now | Gate A only: `POST {Backoffice|OtaPanel}/v1/AncillaryOffers/Grouped` |
| Code written | none |

## 1. How the sibling and this repo do it today

Source read: the four controllers under `RestApi/V1/AncillaryOffers/Controllers`, `RestApi/V1/SeatMaps/Controllers`, `Query/Shopping/AncillaryOffers/{Queries,Dto,Snapshots}`, `Query/Shopping/Common`, `Shopping/Results`, `Domain/AncillaryServiceDefinitionAggregate/{AncillaryVariant,AncillaryServiceDefinition.Profile,Specifications}`, the two boundary pins named in the spec.

1. **One shared read service, thin per-surface query + handler + mapper.** `IGetAncillaryOffersService.ExecuteAsync(query, scope, family)` returns `AncillaryShoppingSnapshot`; `Backoffice/` and `OtaPanel/` folders under `Queries/GetAncillaryOffers` each hold a MediatR query (`OfferId?`, `OrderId?`, Backoffice also `Options`), a handler that resolves the surface scope (`IBackofficeSalesScopeResolver.ResolveAsync(options)` / `IOtaPanelSalesScopeResolver.ResolveAsync()`) and a static mapper. Ordering uses the same shape (`Queries/<Op>/{Backoffice,OtaPanel}`).
2. **Family filter.** Both legacy list handlers call the service with `OfferFamilyFilters.Services` (every variant except A07/A08; A09 is included).
3. **offerId XOR orderId** is enforced in `ShoppingSource.LoadAsync` (422 / 16801). Query classes carry no FluentValidation validator.
4. **Controllers.** One class per surface and resource, `[Tags("Backoffice")]`, `[Route("Backoffice/v{version}/AncillaryOffers")]`, `[Authorize(SurfaceAuthorization.Backoffice)]`, one-line actions through `IMediator`. A second controller class on the same route prefix is an existing pattern (`{Profile}BackofficeController` next to `BackofficeController` in the provision aggregate).
5. **Snapshot content.** `AncillaryOfferItem` = `OfferItemRef`, `PointOfSaleId`, `Context`, `Candidate`, `OptionGroups`. The candidate carries profile, variant, name, coverage refs, readiness, price (`CompleteUnitTotal`, `CurrencyId`, `IsOrderLevelTotalComplete`), quantity range and the selection contract. The **definition specification is not in the snapshot**; the detail service reloads it with `IActiveAncillaryDefinitionReader.ListActiveAsync(owner)` and the OtaPanel detail mapper `OtaPanelServiceSpecificationMapper` already maps `ServiceSpecificationArgs` to typed per-profile DTOs.
6. **Direct/details signal.** `OfferInlineOptions.Interaction(candidate, groups)` gives `RequiresDetails` + reasons; `OfferPurchase.CanPurchase(candidate)` = Selectable + complete price + no required customer field. Neither treats a quantity range as "needs details".
7. **Shopping stage.** Offer source = constant `FlightOfferShoppingContextMapper.Stage` (PreOrder); the Order source sets the stage on each point-of-sale context (`AncillaryShoppingContext.ShoppingStage`).
8. **Flights.** `FlightOffer.Bounds[].Flights[].FlightId` (long) is available on the snapshot for the seat links.
9. **Enums on the wire.** Backoffice DTOs use `EnumValueDto {value,name,title}` (platform rule, R5.3 item 9); OtaPanel DTOs use enum names as strings.
10. **Pins.** `P32B_SRF_05` pins the three legacy controllers (untouched by a new class). `V12_C02_C04` pins the route count at 158 and lists every route.
11. **Tests.** `tests/AeroTech.Ancillary.Shopping.Tests` drives the real services with `BackofficeDesk` / `ShoppingLab` fixtures (no SQL), which is where a `GroupedOffersTests` class belongs.

## 2. Open points (answer before code)

| # | Point | Evidence | Recommendation |
|---|---|---|---|
| O1 | Backoffice request body: the spec allows only `offerId` / `orderId`; the legacy Backoffice list also takes `options {pointOfSaleId, countryId}` by your 2026-10-11 ruling (each surface mirrors AirAvail's input). | `BackofficeGetAncillaryOffersQuery.Options`, `IBackofficeSalesScopeResolver.ResolveAsync(options)` | Keep optional `options` on the Backoffice Grouped query, same as the legacy route; OtaPanel has none. |
| O2 | Enum fields (`coverageScope`, `readiness`, `shoppingStage`, `chargeKind` …) are strings in the spec; the Backoffice convention is `{value,name,title}`. | `BackofficeAncillaryOfferDto` vs `OtaPanelServiceDto` | Backoffice DTOs use `EnumValueDto`, OtaPanel DTOs use strings, exactly as the legacy pair does. |
| O3 | Route-count pin `V12_C02_C04` must move from 158 to 160 for Gate A (two new routes); the spec says no freeze test may change for an endpoint. | `V12BoundaryAcceptanceTests.cs:368` | Treat it as the route register, not a freeze: update 158 → 160 and add the two routes to its list; `P32B_SRF_02/05` stay untouched. |
| O4 | Which items go into the nine arrays: discoverable only (Selectable / NeedsSelection / NeedsQuote, as both legacy lists show) or every candidate including blocked ones? | `BackofficeAncillaryOfferMapper.IsDiscoverable`, `OtaPanelAncillaryOffersMapper.DisplayStatusOf` | Discoverable only; `readiness` still names the state. |
| O5 | `seats[]`: every flight of the offer/order, or only flights where the engine finds a seat candidate? | spec §4 "Flight قابل عرضه"; the engine would have to run with `OfferFamilyFilters.SeatMap` to know | Every flight of the source (one link per flight); no seat engine run in Grouped. |

Assumptions I will apply without a ruling (say so if wrong): typed fields come from the definition specification reloaded like the detail service (no change to the shared snapshot); A09 is left out of every array; `requiresDetails` = `Interaction.RequiresDetails` or `MinPerSelection != MaxPerSelection`; `price.unitTotal` = `CompleteUnitTotal`, `isTotalComplete` = `OfferPurchase.IsPriceComplete`; `selectionId` is always null in Gate A; `meta.shoppingStage` comes from the first point-of-sale context, else PreOrder; controllers `GroupedBackofficeController` / `GroupedOtaPanelController` in `RestApi/V1/AncillaryOffers/Controllers`; shared `IGetGroupedAncillaryOffersService` + per-surface query, handler, mapper under `Queries/GetGroupedAncillaryOffers/{Backoffice,OtaPanel}`; DTOs under `Dto/Backoffice` and `Dto/OtaPanel`, one type per file.

## 3. Freeze check at preflight

```
git diff --name-only 49e9209 -- src/AeroTech.Ancillary.RestApi/V1/SeatMaps src/AeroTech.Ancillary.Query/Shopping/SeatMaps src/AeroTech.Ancillary.Shopping/Engine/AncillaryShoppingEngine.SeatMaps.cs src/AeroTech.Ancillary.Shopping/SeatMaps
```
Output: empty.
