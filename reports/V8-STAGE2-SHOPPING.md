# Pack v8 — Stage2: shopping lists on three surfaces

| Item | Value |
|---|---|
| Branch | `feat/ancillary-offers-v8`, HEAD `851ff5a`; Stage1 and Stage2 both uncommitted |
| Authority | pack `docs/Shopping` v8 (`12-AGENT-STAGE2-SHOPPING.md`, `02`, `03`), your rulings, Q1–Q4 |
| Endpoints | `POST Backoffice/v1/AncillaryOffers`, `POST OtaPanel/v1/AncillaryOffers`, `POST Api/v1/AncillaryOffers`, body `{offerId}` only |
| Removed endpoints | `Details`, `Selections/Evaluate`, `Selections/EvaluateBatch` on the three surfaces; `Service/v1/Ancillaries/Selections/Resolve` |
| Database | no migration, no write; Stage2 reads only |
| Tests | none, on your order; the proof is the clean build |
| SeatMap | only `ResolveSeatMapSelectionsService.cs` changed, the three lines of Q3 |

## 1. What was built, by layer

| Layer | Change |
|---|---|
| Contracts | enum `SelectionScope` (`PerOrder`, `PerTraveller`, `PerTravellerPerPortion`, `PerTravellerPerFlight`) |
| Shopping `Offers/` | `AncillaryOfferListing`: runs the engine once per point of sale, reads the active definitions and provisions of the candidates, classifies each row and signs its ids. `CustomerInputs`: per-variant rules of pack 03 §2/§4. Records `AncillaryOfferRow`, `AncillaryQuantityPolicy`, `AncillaryQuantityOption` |
| Shopping `Selections/` | `SelectionSigner.SignDirect` now takes the provision, see §2 |
| Shopping `Engine/` | the engine's own "unverifiable" check moved into `AncillaryShoppingEngine.IsUnverifiable`; `Readiness` calls it, behaviour unchanged |
| Query | one service `GetAncillaryOffersService` (load offer, existing scope guard, call Shopping), three thin queries and handlers, two mappers, the DTOs, `Common/SelectionLimits.MaximumIds = 50` |
| RestApi | the three controllers keep only `POST`; the Service selections controller is deleted |

The Query service has no rule. It loads the offer through the existing `IFlightOfferShoppingSource`, calls the existing `PricedOfferScopeGuard`, and passes the point-of-sale contexts to Shopping.

Each request class refuses unknown body fields with `[JsonUnmappedMemberHandling(Disallow)]`, as the definition inputs already do. A blank `offerId` gives the existing `FlightOfferIdIsRequired` error.

## 2. Classification and issuance (Shopping)

A row appears only when at least one id was signed. The Stage1 gate still applies: Filed and complete price, real rate and revision, no FX, no quote, no unapplied fee, not per kilogram, Immediate confirmation, one unit traveller, no Seat, A17 or A19.

| Kind | When | Output |
|---|---|---|
| Typed | the variant asks the customer for an input (table below) | `reference` `r_` and `optionsEndpoint` |
| Quantity-only | no input; selection kind is `QuantityChoice` and provision max > min | `quantityOptions`, one signed `d_` per allowed count, `totalAmount = count × unit price` |
| Direct | no input otherwise | `selectionId` `d_`; quantity = provision min for `QuantityChoice`, else 1 |
| Not issued | a value the definition must fix is missing (A06 equipment kind, A11 meal code, A12 menu item), or A10 is not `FixedAncillary` | no row |

A count is allowed only inside the provision's min and max and inside every verified cumulative limit remaining. That is the engine's own `QuantityAssessor` formula, applied to each count.

| Variant | Customer input, so Typed |
|---|---|
| A01, A02, A05, A10 fixed, A11, A12 | never: quantity or fixed values only |
| A03, A04, A13–A16, A18, A23 | always |
| A06 | when the definition has max size, max linear sum or max kg |
| A20, A21, A22 | when an appointment is required or `MaxGuestsPerPrimary` > 0 |
| A24 | when `MaxDevices` is set |

Signer change, needed by pack 03: without a selection the engine returns `NeedsSelection` for every typed-form variant, even when all its fields are fixed (A11, A20–A22, A24). It returns `Selectable` for the A23 opt-in.

- A `d_` is now signed for `Selectable`, or for `NeedsSelection` when eligibility is `Eligible` (all rules decided, only the form pending) and the engine's unverifiable check is false for that provision.
- An `r_` is signed for `Selectable` or `NeedsSelection`. POST Options in Stage3 re-runs the engine with the real selection.

## 3. Response

Families: `baggages`, `meals`, `pets`, `assistedTravel`, `airportServices`, `priorityServices`, `connectivity`, `upgrades`; `extraSeats` and `travelInsurance` are always `[]`.

Each row has `variantCode`, `title`, `description` (omitted when absent), `selectionScope`, `unit`, `coverage`, `unitPrice {currencyId, amount}`, `quantityPolicy {selectable, min, max, unit}` and `details`. Exactly one of `selectionId`, `quantityOptions`, or `reference` + `optionsEndpoint` is written. Every null field is left out.

| Coverage of the unit | `selectionScope` | `unit` |
|---|---|---|
| Order | `PerOrder` | `{scope:"Order"}` |
| Journey | `PerTraveller` | `{travellerRef}` |
| Portion | `PerTravellerPerPortion` | `{travellerRef, portionRef}` |
| Sector | `PerTravellerPerFlight` | `{travellerRef, flightRef}` |

`optionsEndpoint` is `/{Surface}/v1/AncillaryOffers/{Family}/{reference}/Options` with the family names of pack 02 §1.

`details` per family follows 02 §3 plus the extra fields of 03 §3, all read from the definition. Airport services use the provision rule first and the definition second for terminal, facility, direction, service window and guests, as the engine does.

OtaPanel and Api return the same DTO, as Ordering and AirAvail do for their agency surfaces. Backoffice has its own DTO in which every enum is `{value, name, title}`, the platform rule for Backoffice and the shape of the old Backoffice offers DTO. Its mapper reuses the shared details and only wraps the enums.

## 4. Deleted

| Area | Files |
|---|---|
| `Query/Shopping/AncillaryOffers`: old DTOs, snapshots, `EvaluateBatch`, `EvaluateSelection`, `GetAncillaryOfferDetail`, `ResolveSelections`, old list mappers, `OfferInlineOptions`, Api references reader | 174 |
| `Query/Shopping/Common/PricedSelections`, `OfferItemRef.cs`, `OfferPurchase.cs` | 9 |
| `RestApi/.../AncillaryOffers/Controllers/ServiceController.cs` | 1 |

In total 184 files under `src/` are deleted and 48 code files are new in Stage2. The `ExceptionFactory` methods that only the deleted code used are left in place.

## 5. Proof

| Step | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 errors, 57 warnings |
| SeatMap freeze (48 files, method that reproduces the Gate0 value `2cab2be6…25fd` on `49e9209`) | new value `cf4211192fe359c43897814dc9716406e442b84578e75ea6daabfd1c2896e39d`; changed file: `ResolveSeatMapSelectionsService.cs` only |
| Comments in changed `.cs` files | none |
| `Dictionary<string, object` in Shopping, `Core.Enums.Channel` in changed files | none |
| Line endings | 87 files checked, 0 with a lone LF |

Not proven in this stage: no test (your order), no live HTTP call and no performance number. Scenarios H01–H12 and S01–S16 of pack 05 are therefore not yet shown.

## 6. Points for your review

Points 1–5 are S2-1 to S2-5 in `reports/V8-QUESTIONS-AND-CONFLICTS.md` §3, each with what was applied. S2-6 there covers a fixed quantity above 1. Point 6 is information only.

1. AssistedTravel `details`: pack 02 §3 nests the variant object (`assistanceKind` + `wheelchair{…}`), pack 03 §3.4 is flat with `kind`. Built as 02, the same nesting as the definition DTO of this repository.
2. Backoffice enums as `{value, name, title}` while pack 02 says enum string.
3. `medicalEquipment` and `unaccompaniedMinor` sub-objects not built: A17 and A19 are gated and never reach the list.
4. Optional `includedBaggage` of 03 §3.1 not built.
5. `extraSeats` uses `ExtraSeatDetailsDto` of 03 §3.9; `travelInsurance` uses an empty `TravelInsuranceDetailsDto`; both lists are always empty.
6. `OfferOptionCatalog` was deleted with the Details code. Gate0 said it moves to Shopping; Stage3 rebuilds the options catalog there from the domain and pack 03 §4.
