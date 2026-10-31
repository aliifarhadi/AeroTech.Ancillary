# Stage 6A.2 R4-A: Ancillary Resolve source contract

Authority: `AeroTech.Ordering.Final/docs/AeroTech-Stage6A2-R4-Verified-Benchmark-And-Execution-Prompts/01-ANCILLARY-COMPANION-EXECUTION-PROMPT-R4.md`.

Baseline: `k8s-stg@c6e783b`, equal to origin, clean tree. Work is uncommitted: the commit is the Owner's order (repository rule 8), so no final SHA exists yet.

## 1. Status

| Gate | Meaning | Code and tests | Live HTTP |
|---|---|---|---|
| AC01 | General Resolve header | PASS | PASS, Offer source |
| AC02 | General price completeness | PASS | PASS |
| AC03 | Seat Resolve header and count | PASS | PASS, Offer source |

Live HTTP ran on 2026-10-31 on the Owner's host :4046 (started 13:33:14 with this code) with an OtaPanel token and an agency-priced offer:

| Step | Result |
|---|---|
| `OtaPanel/v1/SeatMaps/Price`, 4 seats on 2 flights | 200, 7.48 s |
| Seat Resolve | 200, `sourceKind` Offer, exact `sourceReference`, `selectedLineCount` 4, 4 rows with the same ids |
| `OtaPanel/v1/AncillaryOffers/Price`, 1 line | 200, 2.83 s |
| General Resolve | 200, `sourceKind` Offer, exact `sourceReference`, `selectedLineCount` 1, `isUnitTotalComplete` true |
| Point of sale changed in a seat id | 409 / 16820 |

Not run live: the Order source, because it needs a running Ordering and an order with future flights. T02 and T07 cover it in tests only.

## 2. Contract added

General `POST Service/v1/AncillaryOffers/Resolve`, additive:

| Member | Value |
|---|---|
| `sourceKind` | `Offer` or `Order`, a plain name |
| `sourceReference` | the exact stored offer id, or the order id as invariant text, with no prefix |
| `expiresAtUtc` | the snapshot expiry |
| `selectedLineCount` | the number of stored lines |
| `pricing.isUnitTotalComplete` | `Rate.IsUnitTotalComplete`, per line |

Seat `POST Service/v1/Ancillaries/SeatMaps/Resolve`, additive: the same four header members. `sourceKind` stays a plain name in the seat header, because Ordering reads it with `ShoppingSourceKind?` and a string converter, although the enums inside a seat are `{value,name,title}`.

## 3. Decisions on the prompt text

- **Cardinality guard not added.** The nine profiles are eight mapped families plus Seat, and `SelectionResolver` refuses a Seat line with 409. So `selectedLineCount` already equals the mapped rows. T04 pins it.
- **"Active" snapshot.** A snapshot has no status column. Active means not expired, which `FindOpenAsync` already enforces.
- **No Domain or Persistence change.** `SourceReference` is the small Query helper `Shopping/Common/SelectionSource.cs`, used by both mappers.

## 4. Files

Changed:
- `src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Dto/Service/ServiceResolvedSelectionsDto.cs`
- `src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Dto/Service/ServiceResolvedPricingDto.cs`
- `src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Queries/ResolveAncillarySelection/Service/ServiceResolvedSelectionsMapper.cs`
- `src/AeroTech.Ancillary.Query/Shopping/SeatMaps/Dto/Service/ServiceSeatMapResolutionDto.cs`
- `src/AeroTech.Ancillary.Query/Shopping/SeatMaps/Snapshots/SeatMapResolutionSnapshot.cs`
- `src/AeroTech.Ancillary.Query/Shopping/SeatMaps/Queries/ResolveSeatMapSelections/ResolveSeatMapSelectionsService.cs`
- `src/AeroTech.Ancillary.Query/Shopping/SeatMaps/Queries/ResolveSeatMapSelections/Service/ServiceSeatMapResolutionMapper.cs`
- `reports/ORDERING-ANCILLARY-INTEGRATION-GUIDE.md`

New:
- `src/AeroTech.Ancillary.Query/Shopping/Common/SelectionSource.cs`
- `tests/AeroTech.Ancillary.Shopping.Tests/R4ResolveContractTests.cs`

No migration, table, enum, route, FlightFlow call or Ordering call was added.

## 5. Proof

| Check | Command | Result |
|---|---|---|
| Clean host build | `dotnet build src/AeroTech.Ancillary.ServiceHost -o <side folder> --no-incremental` | 0 errors, 34 warnings |
| Shopping tests | `dotnet test tests/AeroTech.Ancillary.Shopping.Tests` | 138 passed, 1 skipped, 139 discovered, none aborted |
| Boundary and seat hold tests | `dotnet test tests/AeroTech.Ancillary.Application.AcceptanceTests --filter "Boundary, SeatReservation, V10SeatAndLifecycle"` | 29 of 29, 29 discovered |

T01 to T09 are in `R4ResolveContractTests.cs` (13 test methods). T09 serializes with the host JSON options.

## 6. A18 (Bassinet) decision

Decision: **`BLOCKED_SOURCE` for Offer-based Create. Sellable on the Order path after one catalog correction. No code change.**

The `00` matrix should carry this status: A18 is not sellable at initial Create today.

Evidence from source:
- The domain sells the bassinet to the guardian adult and links the infant. `ProvisionEvaluation.Bassinet` requires `guardian.TravellerRef == _unit.TravellerRef`, an `INF` infant and a matching `AssociatedAdultRef`. `TypedSelectionForm` offers the infants linked to the unit traveller. The documents agree: `infantRef`, `guardianRef`, `flightRef` (v12.2 05), "Bassinet and associated adult" (Phase 3 E-A18).
- The catalog is wrong. The Staging seed gave the provision `types=("INF",)`, so the unit lands on the infant, `infantRef` has no choice and `guardianRef` is the infant. That is a seed error, not an engine error.
- The Offer path has no adult-infant link. `AssociatedAdultRef` is filled only by `OrderShoppingContextMapper` from Ordering's `InfantParentTravellerId`. The AirAvail offer carries none, so correcting the catalog alone would not make A18 priceable on an Offer.
- The Order path has the link, and `V9InlineOptionsTests` covers an adult with a linked infant.

Benchmark, public carrier pages only:
- Finnair assigns the bassinet to the infant on board when a suitable seat is booked for the adult.
- Cathay Pacific's agent guidance requests BSCT after the infant SSR is confirmed.
- China Southern offers BSCT for infants who do not occupy a seat.
- Lufthansa (BM10) limits it by capacity, age and aircraft.

Together these support a bassinet tied to an infant already linked to an adult. How NDC schemas express it is `UNVERIFIED`: no public schema page was found.

Rejected: selling A18 on the infant, or adding a client-supplied link at Offer time. Both invent a fact the source does not hold.

Open: the catalog correction on DB-TEST (provision passenger type `ADT`) is not applied.

Sources:
- https://easy.finnair.com/en/special-services-ssr/baby-cot
- https://www.cxagents.com/ta/en_MM/policies/passenger-assistance/children-and-infants/infants.html
- https://csair.com/en/tourguide/before_ready/special_assistance/infant
- https://www.lufthansa.com/md/en/fliegen-mit-baby

## 7. Known limits, not changed

- Seats do not pass `SellableCandidate`. A seat rate filed in another currency shows a converted price on the map, while Resolve answers the filed rate. Ordering refuses the mismatch, so wrong money cannot be stored, but that seat cannot be sold. The current catalog is in IRR.
- Prompt `02` section 5 lists `Booking.Method` and `Document.*` under `Provision`. On the seat wire they are under `ServiceDefinition`.
- A09, A17 and A19 gates are unchanged.
