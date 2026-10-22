# Grouped Ancillary — closure audit (bounded fixes on `5d85304`)

| Item | Value |
|---|---|
| Branch / head | `feat/ancillary-grouped-api-gate-a` @ `5d85304` "Gate A Finish"; baseline `49e9209`; corrections UNCOMMITTED (no commit, no push) |
| Status | `ANCILLARY_CODE_READY` for the grouped shopping, resolve and hold code; `ORDERING_INTEGRATION_PENDING` for the end-to-end hold; `HTTP_NOT_VERIFIED` (no live HTTP run this round). Never `PRODUCTION_READY`. |
| Changed paths | 17 (8 production, 9 tests) + 1 new production file `Query/…/ResolveGroupedSelections/GroupedHoldCapability.cs`; `git diff --stat`: 342 insertions, 107 deletions |
| Migration | none new; `20261022112545_GroupedSelectionId` (additive, from the finish round) is the only schema change of the feature |

## Freeze diff (reservation folders vs `49e9209`)

22 files, 437 insertions, 31 deletions. Pre-existing files touched: `HoldAncillaryServices/{AncillaryHoldHeader (new), IHoldAncillaryServicesService (+1 overload), HoldAncillaryServicesService (core split, behaviour unchanged)}`, `AncillaryReservation` (occurrence key), `AncillaryReservationUnitArgs` and `AncillaryReservationUnit` (`SelectionId`). Every other path is new under `HoldGroupedSelections/` and `Services/`. This round changed only `HoldGroupedSelectionsService.cs` inside the frozen set. Frozen hash pins moved from `493A5801…7AF5` to `6A18D7BF1D64403BBCFD7AD6648271CDCFF73AF3D8592D9C339DE3BF9707C3D3` (91 files, unchanged count) in the three pin tests. Routes 180, command services 59, SeatMap and legacy hold untouched, one engine, one aggregate.

## MUST FIX 1 (P0) — Order-issued id must still hold after the order mutates

- Repro: an id issued at order v1 was verified at hold by re-deriving the item against the current source; after Ordering accepted the service the order is v2 and its source fingerprint (version + services) differs, so the old hash never matched.
- Fix: `OrderServiceFacts` gained `AcceptedServiceDefinitionId`, `AcceptedProvisionId`, `AcceptedPricingRevisionId`; `GroupedSelectionVerifier` binds each entry to the accepted snapshot (definition, provision, traveller, flights, portion, pricing revision, currency, total, quantity), reopens the order at its current version without the batch's own services, re-offers and re-prices through the engine, and accepts an Order-issued id whose version is at or below the current one.
- Tests: Shopping `GRP_B07` (Offer→resolve→accepted on order v2→hold verifies; resolve at v2 is `Stale`; verification at v1 refused) and `GRP_B06` (Offer-issued id on a v1 order). Result: pass.

## MUST FIX 2 (P0) — ResolveGrouped never says `allValid` for what the hold cannot take

- Repro: a Supplier Wi-Fi plan, a Local-stock bag or an A09 extra seat resolved `Valid`; the hold then refused them.
- Fix: new `GroupedHoldCapability.Blocker(candidate)` (Seat → `SeatSelectionNotOffered`; inventory authority null → `InventoryNotConfigured`, Supplier → `InventoryDelegatedCheckRequired`, Local → `InventorySourceNotConfigured`; reserve-time check → `AvailabilityMustBeChecked`); `GroupedSelectionRevalidation` returns `Unavailable` with that code and the verifier refuses with the same code. Existing reason codes only.
- Tests: `GRP_B08` (Supplier Wi-Fi, forged Local bag id, forged A09 id → not `Valid`; Unlimited Wi-Fi → `allValid`). Result: pass.

## MUST FIX 3 (P1) — validate the accepted order service

- Contract (handoff §1): `CommercialState == Active`, `TravellerRef`/`PortionRef`/`FlightRefs` are the accepted coverage, `Quantity` = accepted `service.price.quantity` units (Ordering has no such column today; additive).
- Fix: verifier requires Active state, a currently offered candidate for the accepted definition/provision/traveller/flights, portion containment, `price.units == Quantity`, same pricing revision and total, no unapplied shared fee, linked order traveller.
- Negative tests in `GRP_B06`: wrong accepted id, wrong total, Cancelled state, quantity 1 vs 2, wrong pricing revision, wrong definition, wrong version, other point of sale, Supplier blocker, disconnected reader (503 / 16815). A service recorded on the other traveller or the other bound is held on that coverage (Ordering's record is the authority; asserted positively). Result: pass.

## MUST FIX 4 (P1) — codec never throws on malformed or oversized JSON

- Fix: `TryGetInt32` for `v` and order version, `TryGetInt64` for scope ids and quantity, `catch` of `JsonException | FormatException | InvalidOperationException | OverflowException` around parse and read; length > 4096 and non-base64url rejected before parsing.
- Test: `GRP_B09` (oversized `v`, string `v`, string/oversized point of sale, selection as array, oversized and decimal quantity, non-object root, truncated JSON, invalid UTF-8, 5000-char id → all `null`, no exception). Result: pass.

## Idempotency regression

`HoldGroupedSelectionsService` looks the key up before any verification: same batch → stored hold, different batch (other selection id or no seat) → 409 / 16403. Acceptance `GRP_A05` clears the fake verifiers after the first hold and proves the replay needs no verification; `GRP_A04` proves the replay takes no lock. Result: pass.

## Performance

Measured with the in-memory catalog read counter on the fixture (16 shown items, 12 evaluated for a direct id, 8 ids issued):

| Grouped list | catalog reads |
|---|---|
| before (finish round) | 81 |
| after | 57 |

Cause: `GroupedSelectionIdProvider.IssueAsync` called `DescribeAsync` (3 reads) before every evaluation only to get option groups the item already carries. Fix: `IEvaluateSelectionService.EvaluateAsync(shopping, item, selection)` evaluates straight from the shopping item; the detail overload delegates to the same private path. The remaining 4 reads per evaluated item are the engine's own catalog load in `EvaluateSelectionAsync`; a shared per-request catalog would be an engine change and is out of scope. `GRP_B10` pins `reads ≤ 9 + 4 × evaluated`.

## ID exposure (release limitation, no crypto rewrite)

The unsigned id reveals `pointOfSaleId`, `airlineOfficeId`, `travelAgencyId`, `customerId`, `countryId`. Recorded in the handoff §4 with the handling rule (opaque in logs, not in customer documents, cannot be replayed on another order, point of sale or source version).

## Fixture realism

`GroupedReservationAcceptanceTests.CatalogAsync` now authors a real `A07` seat and a real `A01` baggage through `V122Catalog.Define/Rule`, prices them in EUR and publishes the provisions; the fake verifier returns the Portion coverage of A01 (the domain check "unit coverage equals provision coverage" caught the earlier Lounge stand-in).

## Proof (final tree, same run)

| Step | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 errors, 64 warnings (pre-existing) |
| Domain.ConformanceTests | discovered 264, executed 264, passed 264 |
| Shopping.Tests | discovered 384, executed 384, passed 384 |
| Application.AcceptanceTests | discovered 237, executed 237, passed 237 |
| CRLF check | checked 15 changed files, bad 0 |
| Comment scan of changed files | none |

## Remaining dependencies

1. Ordering: store the accepted `service` block on the order service and fill the six `Accepted*` members plus `Quantity` (handoff §1); replace `NotConnectedOrderCommercialContextReader`.
2. Live HTTP proof of `ResolveGrouped` and `Service-Holds/Grouped` once Ordering is connected (`HTTP_NOT_VERIFIED`).
3. Owner review and commit of this round.
