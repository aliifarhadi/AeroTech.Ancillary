# Grouped Ancillary — final two fixes

| Item | Value |
|---|---|
| Branch / head | `feat/ancillary-grouped-api-gate-a` @ `017d241`; SeatMap baseline `49e9209`; changes UNCOMMITTED (no commit, no push) |
| Status | `ANCILLARY_CODE_READY`; `ORDERING_INTEGRATION_PENDING` (`NotConnectedOrderCommercialContextReader` is still the production reader); HTTP verified live on the OtaPanel and Service surfaces; Backoffice grouped list not verified with a Backoffice-priced offer (AirAvail staging cannot price one). Not `PRODUCTION_READY`. |
| Changed paths | 13 (7 production, 6 tests); 119 insertions, 13 deletions; no new file |
| Freeze | `git diff --name-only 49e9209` over the SeatMap Query and RestApi folders: empty. No migration added. No route added (the route pin test is in the green acceptance run). Frozen-file hash pin moved to `D2377BB0…501B` because `HoldGroupedSelectionsService` changed; still 91 files. |

## FIX 1 — no automatic opt-in (A23)

**Before.** `DirectSelection.TryBuild` set every required Boolean field to `true`. A fully authored A23 item came back from the grouped list with a purchasable `selectionId` carrying `optIn: true`, although the customer had not chosen anything.

**After.**
- `DirectSelection.TryBuild` no longer fills a Boolean field. An item that needs one returns no direct selection, so the list gives `selectionId = null`.
- `DirectSelection.RequiresConsent` sets `requiresDetails = true` on the grouped list item only. The legacy `OfferInlineOptions.Interaction` is not changed. The existing `{Surface}/v1/AncillaryOffers/PriorityServices/Details` route is the details endpoint.
- `DirectSelection.HasConsent` requires every required Boolean field to be explicitly `true`. It gates id issuing in the grouped list and in the typed details (missing or false adds the existing `SelectionFieldRequired` reason). It also makes `ResolveGrouped` return a non-Valid entry, and the hold verifier refuse with 16402 `…:SelectionFieldRequired`. That way a hand-built id with `optIn:false` cannot pass later either.

| Test | Before | After |
|---|---|---|
| `GRP_B11` A23 in the grouped list | id issued with `optIn:true` | `selectionId = null`, `requiresDetails = true`, `detailsEndpoint = /Backoffice/v1/AncillaryOffers/PriorityServices/Details` |
| `GRP_B11` details with `{ optIn: true }` | id | id; its selection is exactly `{ optIn: true }`; `ResolveGrouped` → `allValid = true` |
| `GRP_B11` details with `{ optIn: false }`, `{}` or no selection | id | no id; `SelectionFieldRequired` in the reasons |
| `GRP_B11` id re-encoded with `optIn:false` sent to `ResolveGrouped` | Valid | not Valid, `SelectionFieldRequired` |
| `GRP_B11` / `GRP_B02` A24 single-plan Wi-Fi | direct id | direct id, unchanged |
| `GRP_B10` grouped list | — | the 4 A23 items carry no id and `requiresDetails = true`; the read bound is unchanged |

## FIX 2 — an idempotent replay keeps the stored family

**Before.** `Replayed` matched the stored units by order-service id and selection id only. A Baggage unit sent again under the same key in `meals[]` was replayed, and `Project` reported the stored Baggage as a Meal.

**After.**
- The replay check still runs before any Ordering or verifier call.
- For each grouped entry it now also reads the stored unit's definition through the existing `IAncillaryServiceDefinitionRepository.GetAsync(unit.ServiceDefinitionId)`. The definition's `Profile` must equal the array the entry was sent in.
- Any difference, or a stored unit without `SelectionId` (a hold stored by the legacy endpoint), is the existing `AncillaryHoldIdempotencyKeyReused` 409 / 16403.
- No schema, aggregate, codec or Application→Query dependency was added. The order commercial version is not compared, so a replay after the order moved still works.
- The lock behaviour and the single unit of work of the first creation are unchanged.

| Test | Before | After |
|---|---|---|
| `GRP_A06` same key, same batch, verifier fakes emptied | same hold | same hold id, one Baggage unit, one seat, no Meal |
| `GRP_A06` same key, same order service and selection id moved to `meals[]` or `priorityServices[]` | replayed and mislabeled | 409 / 16403; one row, stored unit unchanged |
| `GRP_A05` same key, other selection id or seat removed | 409 / 16403 | 409 / 16403 (unchanged) |
| `GRP_A05` new key, unknown selection | 422 / 16402 | 422 / 16402 (unchanged) |
| `GRP_A04` replay under a busy lock | no lock taken | no lock taken (unchanged) |
| `GRP_A07` key first used by the legacy `Service-Holds` endpoint, then sent to the grouped hold | would project the legacy unit | 409 / 16403; the legacy row and its unit are unchanged |

## Proof (final tree, same run)

| Step | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 errors, 62 warnings (pre-existing) |
| Domain.ConformanceTests | discovered 264, executed 264, failed 0 |
| Shopping.Tests | discovered 385, executed 385, failed 0 |
| Application.AcceptanceTests | discovered 239, executed 239, failed 0 |
| Aborted runs | none |
| CRLF check on changed files | 0 bad |
| Comment scan on changed files | none |

## Live HTTP run (Owner tokens, dev DB catalog, private host on port 5299, stopped afterwards)

Offer: AirAvail staging `OtaPanel/v1/FlightOffers` + `/Price`, IKA–IST 2026-10-25, 1 adult + 1 child, IRR, priced on the OtaPanel channel.

| Step | Result |
|---|---|
| `POST OtaPanel/v1/AncillaryOffers/Grouped` | 200; 32 items: baggages 10, assistedTravel 12, airportServices 6, upgrades 4; `seats[]` = 1 flight with `/OtaPanel/v1/SeatMaps` |
| every item has either a `selectionId` or a real `/OtaPanel/v1/AncillaryOffers/{Family}/Details` endpoint | pass |
| `{Family}/Details` without a selection for A16, A21 and A10 | 200; same `selectionId` as the list |
| `Baggages/Details` A03 with `{ bagRef, measuredWeightKg: 28 }` | 200, id issued |
| `Baggages/Details` A06 ski without dimensions | 200, no id, `SelectionFieldRequired` |
| `AirportServices/Details` A22 CIP without facility and time | 200, no id, `SelectionFieldRequired`, `SupplierConfirmationRequired`, `AvailabilityMustBeChecked`, `InventoryDelegatedCheckRequired` |
| `POST Service/v1/Ancillaries/Selections/ResolveGrouped` with the A03 id | 200, `allValid = true`, `Valid`, quantity 1, line total 52,500,000 IRR |
| ResolveGrouped with A16 (Unlimited) + A21 + A10 | A16 `Valid` with definition, provision and price; A21 and A10 `Unavailable` / `InventoryDelegatedCheckRequired`; `allValid = false` |
| ResolveGrouped with an id in another family, or a malformed id | 200, not valid |
| `POST Service/v1/Ancillaries/Service-Holds/Grouped` | 503 / 16815 (Ordering reader not connected) |
| `POST Backoffice/v1/AncillaryOffers/Grouped` with the OtaPanel offer and the new Backoffice token | 403 / 16809 from the existing channel guard; no token → 401 |
| AirAvail staging `Backoffice/v1/FlightOffers/Grids` and `Backoffice/v1/FlightAvailability/Price` (new Backoffice token) | 500 with an empty body; no Backoffice-channel offer can be priced, so the Backoffice grouped list stays unproven over HTTP |

No A23 product exists in the dev catalog, so FIX 1 is proven by tests only. The live list carried no A23 id.

## Open point for the Owner (not changed in this task)

The grouped list issues a `selectionId` for directly purchasable items whose stock is delegated to a supplier: live A21 Fast Track and A10 Economy→Premium, and the `WIFI_PARTNER` test fixture. `ResolveGrouped` then answers `Unavailable` / `InventoryDelegatedCheckRequired` for them, and the grouped hold refuses them. That is the behaviour the closure audit asked for. The frontend therefore sees an id it can never complete. Recommendation: withhold the id in the list and details when `GroupedHoldCapability.Blocker` is not null, so the item shows `selectionId = null` with its reason. This is a change to `GroupedSelectionIdProvider.Issue` only and needs your order.

## Known limitation kept

The unsigned, decodable id still exposes `pointOfSaleId`, `airlineOfficeId`, `travelAgencyId`, `customerId` and `countryId`. It stays documented in the Ordering handoff §4. There was no codec or encryption change in this task.
