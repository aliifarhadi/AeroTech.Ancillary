# Pack v8 — Final closure

| Item | Value |
|---|---|
| Branch | `feat/ancillary-offers-v8`, HEAD `851ff5a` (pack + Gate0), baseline `k8s-stg@49e9209` |
| State | Stages 1–5 uncommitted; nothing committed, pushed or merged |
| Recommendation | **GO** for your review of the Ancillary-only milestone, with one open condition: a live Backoffice run on a Backoffice-priced offer (AirAvail staging's Backoffice search answered 500). **NO-GO** for production use until Ordering adopts the handoff contract and the gated items close |

## 1. Build and suites (final tree)

| Command | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 errors, 61 warnings. 4 are new NuGet advisories (NU1902, NU1903) from the restored Shopping test project; Gate0 had 63 with that project present. No compiler warning comes from a changed file |
| `dotnet test tests/AeroTech.Ancillary.Shopping.Tests` | 74 discovered, 74 passed |
| `dotnet test tests/AeroTech.Ancillary.Domain.ConformanceTests` | 264 discovered, 264 passed |
| `dotnet test tests/AeroTech.Ancillary.Application.AcceptanceTests` | 232 discovered, 232 passed (1 min 25 s) |

Existing tests changed because an approved stage changed the rule they pin:

- `P1BoundaryConformanceTests` names `SelectionSnapshot` and `ISelectionSnapshotRepository` as allowed "Snapshot" types, the same way it allows `ProvisionCancellationRule`.
- `V12BoundaryAcceptanceTests` lists the new routes (170 instead of 158) and counts 59 command services instead of 58.

The POST Options routes now use literal templates (`"Baggages/{reference}/Options"`), like every other controller. The route pin reads them that way.

## 2. The 78 cases

PASS 71, GATED 7, FAIL 0. Test names are in `tests/AeroTech.Ancillary.Shopping.Tests` (`V8ScopeTests`, `V8SurfaceTests`, `V8SecurityTests`, `V8OptionsTests`, `V8ResolveTests`).

| Group | PASS | GATED | Evidence |
|---|---|---|---|
| S01–S16 scope × quantity | S01, S03–S16 | S02 | `S..` tests. S02: the engine builds an Order unit only for A22 and A24 (`CoveragePlanner.AllowsOrderScope`), and neither has a quantity field; `S02_OrderUnitIsNotBuiltForAQuantityVariant` proves no fake row |
| H01–H13 surfaces | all 13 | – | `H..` tests; H01–H03 serialize the real Backoffice, OtaPanel and Api DTOs; H03 also live (§3) |
| T01–T19 security | all 19 | – | `T..` tests; tampering is done on the real JWS payload; T15/T16 at the exact expiry second |
| O01–O12 options | O01–O03, O05–O07, O09, O10, O12 | O04, O08, O11 | O04: the Domain forces pets to be subject to confirmation, so the sell gate never issues an `r_`; O05 signs one directly and gets 422 for a weight above the maximum. O08: a definition holds one menu item, so A12 is never typed. O11: A17 is gated by design |
| R01–R14 resolve | R01–R09, R13, R14 | R10–R12 | R10–R12 are Ordering's checks; the Ancillary half (offer hash, unit traveller, sales hash recomputed with the same algorithm) is tested; Ordering itself is not touched |
| N01–N04 non-regression | all 4 | – | N01: SeatMap code and DTO hash unchanged except the Q3 constant; N02: Application suite with the seat hold tests 232/232, reservation code unchanged; N03: scope diff §4; N04: §4 |

## 3. Live HTTP (my own host on port 4146 against `DotAirAncillary`, then stopped)

Your Api token and the offer you sent. The same 17 steps were run again on OtaPanel with your token of 2026-10-24; all answered the same statuses (shopping 200 in 6.5 s, Resolve 200 in 3.3 s and 0.1 s repeated, same family counts and prices).

Backoffice with your token of 2026-10-24 answered 403 (16809) for this offer. That is the existing `PricedOfferScopeGuard`: the offer was priced on the agency channel and the Backoffice scope is the BackOffice channel. A Backoffice-priced offer could not be made: AirAvail staging `POST Backoffice/v1/FlightOffers/Grids` answered 500 with an empty body for IKA–IST on 2026-10-28 and 2026-10-30.

Steps of the Api run:

| Step | Status | Note |
|---|---|---|
| `POST Api/v1/AncillaryOffers` | 200 in 9.3 s | baggages 4, assistedTravel 18, airportServices 10, upgrades 4, others 0, currencyId 70 |
| same with `orderId` added | 400 | unknown member |
| `GET …/AssistedTravel/{r_}/Options` | 200 in 0.8 s | A16: `assistanceSsrCode` select, choices `BLND` |
| `POST …` `{"assistanceSsrCode":"XXXX"}` | 422 | 16821, no row written |
| `POST …` with an extra member | 400 | unknown member |
| `POST …` `{"assistanceSsrCode":"BLND"}` twice | 200, 200 | two `s_` ids, two rows |
| `POST Service/…/ResolveGrouped` `[d_ A21, s_ A16]` | 200 in 5.9 s, repeat 0.1 s | same answer twice; A21 12,600,000 IRR; A16 0 IRR with real rate `1561354423463851266`; `selected {"assistanceSsrCode":"BLND"}` |
| tampered `d_`, `r_` in Resolve, `d_` in Options | 422, 422, 422 | 16816 |
| duplicate ids, 51 ids | 422, 422 | 16801 |
| unknown member in Resolve | 400 | |
| unknown `s_` | 404 | 16819 |
| offers without a token | 401 | |

Two pack demo items were not shown live. This offer lists no quantity row and no A15. A15 is explained: the dev A15 definitions are subject to confirmation, so the gate drops them. Why the 11 dev A01 provisions give no row for this offer was not traced. The quantity path is proven in memory (S04, S05, S11, H05, R05).

## 4. Diff audit

| Check | Result |
|---|---|
| SeatMap freeze (48 files) | `cf4211192fe359c43897814dc9716406e442b84578e75ea6daabfd1c2896e39d`; only `ResolveSeatMapSelectionsService.cs` differs from `49e9209` (Q3) |
| Reservation code | unchanged |
| Migrations | one new: `20261023195016_SelectionSnapshots` (CreateTable / DropTable of one table); no Direct or Quantity token table |
| New Domain types | `SelectionSnapshot` (plain class, not an aggregate root) and its repository port |
| Changed areas | Contracts enums, Domain (A11/A15 Paid, D2 fields, errors, snapshot), Persistence (snapshot), Shopping, Query Shopping, Application snapshot use case, RestApi offers controllers, ServiceHost wiring; nothing in Ordering or another repository |
| Comments, `Core.Enums.Channel`, `Dictionary<string, object` in Shopping, banned names | none in 181 changed `.cs` files |
| Line endings | 190 files checked, 0 with a lone LF |

## 5. SQL proofs (dev DB)

| Proof | Value |
|---|---|
| Active zero rates with a real id | 41 |
| Active non-seat Free provisions left | 0 |
| ReferenceData currencies | 47 EUR, 70 IRR, 155 USD |
| Snapshot rows of A17 | 0 (A17 cannot reach POST) |
| Tables named like Token / Quantity snapshot / Selection | 1 (`SelectionSnapshots`) |
| Rows written by the live runs | 4 (2 Api, 2 OtaPanel), all `{"assistanceSsrCode":"BLND"}`; the refused POSTs wrote none |

## 6. Limitations (not built in this phase)

- **A09:** no FlightFlow source for extra seats; `extraSeats` is always `[]`.
- **A17:** no approved encryption at rest. The D2 fields exist in the contract and engine, but the variant never sells.
- **A19:** the guardian contact model (text versus boolean) is unresolved; gated.
- **Insurance:** no variant exists in the Domain; `travelInsurance` is always `[]`.
- **Out of scope:** after-order shopping, holds and reservation for these ids, EMD, snapshot purge job, the `s_` collision retry (S3-3) and Ordering integration.

## 7. Risks

| Risk | Note |
|---|---|
| Response time | first Api shopping 9.3 s, first Resolve 5.9 s (warm 0.1 s); not profiled |
| Data | this dev offer shows few families; A01/A02/A12 dev provisions are not offered for it and the cause is not traced |
| Open review points | S2-1..S2-6, S3-1..S3-6, S4-1..S4-5, S5-1..S5-3 in `reports/V8-QUESTIONS-AND-CONFLICTS.md` |
