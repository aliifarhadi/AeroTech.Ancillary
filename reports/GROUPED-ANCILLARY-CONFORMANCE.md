# Grouped Ancillary — implementation report (list, details, selection id, ResolveGrouped, grouped hold)

| Field | Value |
|---|---|
| BASE_SHA | `49e9209a40e046fe71c7913cc1fcf8a8fca6799d` (`k8s-stg`, rollback baseline) |
| IMPLEMENTED_SHA/WORKTREE | branch `feat/ancillary-grouped-api-gate-a` @ `f4b5145` (Gate A, Owner commit) + UNCOMMITTED working tree, 104 paths (44 modified, 60 new); no commit, no push |
| Authority | `AeroTech-Ancillary-FINISH-EXECUTION-PROMPT-2026-10-22.md` under `CLAUDE.md` |
| SEATMAP_FREEZE_DIFF | `git diff --name-only 49e9209 -- …SeatMaps…` = empty; `P32B_SRF_02` / `P32B_SRF_05` unchanged |
| ORDERING_READER_REAL_OR_PENDING | PENDING: `NotConnectedOrderCommercialContextReader` still throws 503 / 16815; the grouped hold was proven only with a test double of `IOrderCommercialContextReader` |
| UNSUPPORTED_INVENTORY_TYPES | `Supplier` (no provider reservation), `Local` (no atomic consumption), `NotConfigured`, any `RequiresCheckAtReserve` → the whole grouped hold is refused 422 / 16402 before any row; only `Unlimited` without reserve-time check is held |
| BLOCKERS | B1 real Ordering reader + `AcceptedSelectionId`/`AcceptedCurrencyId`/`AcceptedTotal` on the order service (handoff file); B2 no live HTTP run (no token; host not started); B3 A09 extra seat stays informational (`ExtraSeats/Details` answers, no id, hold entry refused) |
| VERDICT | `GROUPED_SHOPPING_RESOLVE_READY`, `GROUPED_HOLD_INTERNAL_VERIFIED`, `ORDERING_INTEGRATION_PENDING` (not `GROUPED_HOLD_PRODUCTION_READY`) |

## ENDPOINTS

| Method | Route | Note |
|---|---|---|
| POST | `{Backoffice|OtaPanel}/v1/AncillaryOffers/Grouped` | Gate A + `quantity {min,max,unit}` on every item with a Quantity field, `selectionId` issued for deterministic choices (engine-evaluated, `CanPurchase`), `detailsEndpoint` always a real route when `selectionId` is null |
| POST | `{Backoffice|OtaPanel}/v1/AncillaryOffers/{Baggages|Meals|Pets|AssistedTravel|AirportServices|PriorityServices|Connectivity|Upgrades|ExtraSeats}/Details` | 9 thin aliases per surface over one `IGetGroupedOfferDetailService`; body `{ offerId | orderId, offerItemRef, selection?, options? (Backoffice) }`; answer `{ offerItemRef, variantCode, readiness, quantity?, requiredFields[], options[], selected?, price, selectionId, reasonCodes }`; `selected` is a typed per-family DTO; a foreign family's ref is 404 / 16811; A09 answers without an id |
| POST | `Service/v1/Ancillaries/Selections/ResolveGrouped` | `{ offerId | orderId + expectedOrderCommercialVersion, seats[], 9 family arrays }`; 1–50 ids, no duplicates; seats through the frozen R2 resolver; non-seat ids decoded, re-shopped under their recorded scope through `IEvaluateBatchService`, statuses Valid/Stale/Rejected/NeedsQuote/NeedsVerification/Unavailable; `service` = existing `ServiceResolvedServiceDto` only when Valid; no write |
| POST | `Service/v1/Ancillaries/Service-Holds/Grouped` | header of the seat hold; `seats[]` = existing `SeatHoldSelectionInput`; each family array `[{ orderServiceId, selectionId }]`; `extraSeats` refused; one `AncillaryReservation`, one save, same order lock and idempotency; typed projection `{ holdId, …, seats[], baggages[] … }`; lifecycle = existing `Service-Holds/{holdId}` routes |

Legacy routes, DTOs, SeatMap ids and the Api surface are untouched. Route register 160 → 180.

## Selection id (`NonSeatSelectionIdCodec`)

`Base64Url(canonical JSON)`, max 4096: `v`, `source {kind, ref, orderCommercialVersion}`, `scope {channel, pointOfSaleId, airlineOfficeId, travelAgencyId, customerId, countryId}`, `offerItemRef`, `profile`, `variantCode`, `selection {chosen fields only, ordinal order, invariant numbers, UTC instants, sorted code sets}`. Decode re-encodes and compares, so any whitespace, order, extra member, unknown field or version change is rejected. No catalog, provision, pricing, supplier, price, currency or document text inside.

**Deviation from §4 to report:** the id carries a `scope` block including `pointOfSaleId` and the office/agency ids. `OfferItemRef.Of` hashes channel, owner, agency, office, point of sale, offer id, source version and candidate identity; without those values no consumer can find the same `offerItemRef` again on the Service surface. Every value is re-validated: the offer's channel/customer/country are checked by `PricedOfferScopeGuard`, the point of sale must equal the hold header, and the candidate is re-shopped and re-priced.

## Hold verification order (`GroupedSelectionVerifier`, Query)

order found and version/acceptsChanges/currency/ticketedAt equal → every entry decoded, same profile, same point of sale, same source as the order (`Air.OfferId` for Offer ids, orderId+version for Order ids) → order service exists, `AcceptedSelectionId` equal, `AcceptedCurrencyId` equal and `AcceptedTotal` present → re-shop per source/scope with `IEvaluateBatchService` → `CanPurchase`, same `offerItemRef`, same re-encoded id, same currency and total, no shared fee, `Unlimited` without reserve-time check, traveller bound from the order facts → `AncillaryReservationUnitArgs` with `SelectionId` → one `HoldAsync(header, units)` in `HoldAncillaryServicesService` (legacy `HoldAsync(command)` maps to the same core).

Domain: `AncillaryReservationUnit.SelectionId` (varchar 4096, additive migration `20261022112545_GroupedSelectionId`, applied to dev, no pending model changes), part of `IsHeldBy` (replay vs conflict) and of the occurrence key (so two different selections of one definition on one traveller/flight coexist; seat and legacy keys unchanged). No child detail tables: the bound id retains the typed selection and nothing supported in this iteration needs more.

## SCENARIOS_PASSED

| Scenario (prompt §8) | Test |
|---|---|
| 1 SeatMap unchanged | freeze diff empty; `SeatSelectionVerifierTests`, `S2SeatMapResolveTests`, `SeatReservationAcceptanceTests` in the full runs |
| 2 real details routes, inline quantity, no dead A09 URL | `GroupedFlowTests.GRP_B02`, `GroupedOffersTests.GRP_A04/A06` |
| 3 Grouped → Details → id → ResolveGrouped on Offer and Order, same id, tamper/foreign/misfiled rejected | `GRP_B01` (codec), `GRP_B03`, `GRP_B04`, `GRP_B05` |
| 4 A01 variable quantity, A24 fixed plan, invalid/foreign field not ready | `GRP_B02`, `GRP_B03` (A02/A11/A12/A23 are NeedsVerification on an Offer source in this fixture: fare-benefit and existing-service facts are not supplied by an offer, so they cannot be direct there) |
| 5 mixed seat + Unlimited non-seat → one hold, replay, conflict, duplicate order service, zero rows on failure | `GroupedReservationAcceptanceTests.GRP_A01/A02/A04` (SQL), `GRP_B06` (verifier with trusted order double, fail-closed cases, 16815 when disconnected) |
| 6 lifecycle on a grouped hold, legacy contracts untouched | `GRP_A03`; legacy suites green |
| 8 pins | routes 180, command services 59 (7 reservation), folders + `HoldGroupedSelections`, frozen 91 files hash `493A580180E5C4CF0D245DC506B9CABEA730F9DD028A477F4CF895D7407C7AF5` |

Scenario 5 "two concurrent holds": proven as the lock contract (busy lock → 16416, zero rows; replay takes no lock), not as two real processes; cross-process exclusion is the Redis lock already used by the seat path.

## TEST_DISCOVERED/EXECUTED/FAILED (final tree, 2026-10-22)

| Suite | Listed | Executed | Failed | Aborted |
|---|---|---|---|---|
| clean sln build `--no-incremental` | — | 0 errors | — | — |
| Domain conformance | 264 | 264 | 0 | none |
| Shopping | 380 | 380 | 0 | none |
| Application acceptance (SQL) | 236 | 236 | 0 | none |

(Shopping 374 → 380: `GroupedFlowTests` 6; Application 232 → 236: `GroupedReservationAcceptanceTests` 4.)

## Not done

- No authenticated HTTP run and no OpenAPI capture (no token, host not started).
- No Ordering code; see `reports/GROUPED-ANCILLARY-ORDERING-HANDOFF.md`.
- Stock consumers / supplier adapters not built (fail closed).
