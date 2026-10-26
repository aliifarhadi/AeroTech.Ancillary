# V11 Final Closure — Gate 5

Date: 2026-10-27. Branch `feat/ancillary-offers-v9`. Source: `296197a` plus the uncommitted Gates 1–5 and Hold contract R1 (nothing is committed or pushed; R09).
Authority: `docs/Pack-v11` 15, with 07 R01–R10 and §H. Owner decisions: 01 §I (Gate 0), §J (R1), §K (Gate 4), §L (Gate 5).
Verdict: **PASS for the supported scope**, with the GATED and NOT VERIFIED items listed in §10. The audit in §6 found no structural defect, so closure is not failed for code quality.

## 1. Source and file changes (since `296197a`)

| Area | Modified | New |
|---|---|---|
| `src/AeroTech.Ancillary.Application` | 27 | 31 |
| `src/AeroTech.Ancillary.Domain` | 10 | 10 |
| `src/AeroTech.Ancillary.Persistence` | 3 | 2 (the v11 migration and its designer) |
| `src/AeroTech.Ancillary.Query` | 4 | 19 |
| `src/AeroTech.Ancillary.RestApi` | 2 | 0 |
| `tests/…AcceptanceTests` | 10 | 5 |
| `tests/…Domain.ConformanceTests` | 6 | 2 |
| `docs/Pack-v11` | 15 | 0 |
| `reports/` | 2 | 11 (this file included) |

- Providers, ServiceHost and Consumers are unchanged.
- No test file was deleted and no new `Skip` was added (`git diff 296197a -- tests`).
- All 131 changed code files are CRLF.
- Freeze pins: 133 files, `3E998D04…E90`.

## 2. Migration (R01, R02)

- One v11 migration: `20261026190047_V11ReservationSelections`.
  - It is the last applied migration on dev (`dbo.__CommandsMigrationHistory`).
  - Down/Up was proven on dev at Gate 2 (`reports/V11-GATE2-TYPED-DOMAIN.md`).
- `migrations has-pending-model-changes` (2026-10-27): "No changes have been made to the model since the last migration."
- The migration SQL (`migrations script 20261026114827_V10ReservationAllocations 20261026190047_V11ReservationSelections`, 94 lines):
  - creates the 7 child tables `AncillaryReservationUnit{Flights, BaggageSelections, MealSelections, AssistedTravelSelections, AirportServiceSelections, ConnectivitySelections, UpgradeSelections}`;
  - adds `VariantCode` and `ServiceDefinitionRef`;
  - drops the JSON list column `CoveredFlightIds` and the two appointment columns.
- The three `nvarchar(max)` occurrences are EF's `DECLARE @var` lines for dropping default constraints, not columns. No JSON list column remains (R02).

## 3. Tests (R03, R04)

Final run 2026-10-27, logs in scratch `v11/full6`:

| Step | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 errors, 61 warnings |
| Domain conformance | 293 discovered, 293 passed |
| Shopping | 126 discovered, 125 passed, 1 skipped (existing skip, `V9PriceRevalidationTests`) |
| Application acceptance | 300 discovered, 300 passed |
| Aborted or crashed runs | none |

v11 test classes:
- **Acceptance:** `V11WireAcceptanceTests`, `V11TypedSelectionAcceptanceTests`, `V11LifecycleAcceptanceTests`, `V11VariantMatrixAcceptanceTests`, plus the v11 cases in `V10InventoryAllocationAcceptanceTests`.
- **Domain:** `V11HoldConformanceTests`, `V11InventoryConformanceTests`.

The gate reports give the ID-by-ID mapping:
- W/D rows: `V11-GATE1-WIRE.md`, `V11-GATE2-TYPED-DOMAIN.md`, `V11-HOLD-CONTRACT-R1.md`;
- I rows: `V11-GATE3-INVENTORY.md`;
- L rows: `V11-GATE4-SEAT-LIFECYCLE.md`.

**SQL concurrency (I11–I13, R04):**
- Each test run uses its own SQL Server Express database (`localhost\SQLEXPRESS`, `DotAirAncillary_Tests_{guid}`).
- Every racing attempt runs in its own `AncillaryScope`, so it has its own DbContext and SQL connection; all attempts start together (`Task.WhenAll`).
- The ledger takes `sp_getapplock` (Exclusive, owner Transaction, 20 s) per source in sorted order. Four orders race for the last unit, the last kilograms and the last slot person: exactly one hold commits, the others get 16417.
- I23 proves a real lock timeout from a separate connection: 16622 after 20 s, nothing written.

## 4. Live HTTP on the real host and dev DB (R05)

- My own host was side-built from this source (scratch `v11/g4host`, Development) on `http://localhost:5291` against `DotAirAncillary`. Your :4046 was not in use and was not touched. The host was stopped afterwards, port 5291 was confirmed free, and its log has 0 error lines.
- Raw evidence: scratch `v11/g5_live.json` (run 3, 53/53).
- Earlier attempts:
  - Run 1 was 52/53: my check expected A07, but the real seat is A08.
  - Run 2 stopped at step 16, because run 1 had already retired its throwaway product.

**Catalog (Owner Q6, G5-Q1)** — created through my host's Backoffice routes with your token, each copied from an existing product (`g5_catalog.py`, every call 200):

| Reference | Definition | Provision | Policy |
|---|---|---|---|
| `ASSIST_WCHR_IMM` (A15, Immediate) | 1564390653905862656 | 1564390659794665472 | Unlimited, Active |
| `LOUNGE_THR_LOCAL` (A20, local supplier, `RequiresSpecificAppointment = true`) | 1564390684859826176 | 1564390687393185792 | Unlimited, Active |
| `ASSIST_WCHR_RET`, `ASSIST_WCHR_RET2` (throwaway A15, G5-Q3) | 1564390680816517120, 1564392025501990912 | retired by the run | — |

**Seat (G5-Q2):**
- AirAvail `service/v1/FlightOffers/Details` and `POST OtaPanel/v1/SeatMaps` on your offer both answered 200.
- Seat 7A on IKA→IST 1124 (2026-10-28 08:15) was AVAILABLE for ADT-1. Its `seatMapSelectionId` is used exactly as the SeatMap answer gave it.

**Flow (run 3):**

| # | Request | Status | Result |
|---|---|---|---|
| 01 | `POST Service-Holds` A01 ×2 + A15 WCHR + A20 appointment | 201 | Receipt has only `holdId, reference, status, expiresAt, units[unitId, orderServiceId, status]` |
| 02 | same create again | 201 | same hold and unit ids |
| 03 | same key, other `guestCount` | 409 | 16403 |
| 04 | `POST SeatMaps/Holds` append 7A | 201 | same hold, 4 units |
| 05 | same append again | 201 | same unit ids |
| 06 | `GET` | 200 | families `baggages`, `assistedTravel`, `airportServices`, `seats`; options exactly as sent; the seat's variant and reference equal its definition in SQL (`A08` / `ECO_SEAT_PREFERRED`); no `pricingId` |
| 07 | `DELETE ?unitIds=<A15>` | 200 | only A15 Released |
| 08–09 | Confirm, Confirm again | 200 | Confirmed; A15 stays Released |
| 10–11 | Cancel A20 with `CUSTOMER_REQUEST`, then replay | 200 | A20 Cancelled |
| 12 | Cancel A20 with another reason | 409 | 16413 |
| 13 | release a Confirmed unit | 409 | refused |
| 14 | `GET` | 200 | selected values identical to step 06 after SQL reload; 0 allocation rows |
| 15 | `coverageScope`, `serviceDefinitionId`, `inventoryFacts`, `services[]` | 400 ×4 | strict input |
| 16–21 | throwaway A15 held, provision and definition retired, `GET`, replay | 201/200 | GET byte-identical after the Definition is retired; replay returns the stored hold |
| 22–26 | 60 s hold, the real poller, `GET`, Confirm | 201/200/409 | SQL `Status = 4` on hold and unit; GET Expired with the selection unchanged; 16407 |

**GET snapshot (step 14):**

```json
{"holdId":"1564392064504823808","orderId":"17930505321","reference":"ORD-17930505321","status":"Confirmed","expiresAt":"2026-10-26T22:05:32+00:00",
 "baggages":[{"variantCode":"A01","products":[{"provisionId":"1561355739027472384","serviceDefinitionRef":"BAG_EXTRA_23","units":[{"unitId":"1564392064509018112","orderServiceId":"179305053211","status":"Confirmed","unit":{"travellerId":"1001"},"coverage":{"flightIds":["1558420526873968816"]},"selected":{"quantity":2}}]}]}],
 "assistedTravel":[{"variantCode":"A15","products":[{"provisionId":"1564390659794665472","serviceDefinitionRef":"ASSIST_WCHR_IMM","units":[{"unitId":"1564392064509018113","orderServiceId":"179305053212","status":"Released","unit":{"travellerId":"1001"},"coverage":{"flightIds":["1558420526873968816"]},"selected":{"quantity":1,"options":{"assistedTravel":{"assistanceSsrCode":"WCHR"}}}}]}]}],
 "airportServices":[{"variantCode":"A20","products":[{"provisionId":"1564390687393185792","serviceDefinitionRef":"LOUNGE_THR_LOCAL","details":{"airportId":1},"units":[{"unitId":"1564392064509018114","orderServiceId":"179305053213","status":"Cancelled","cancellationReasonCode":"CUSTOMER_REQUEST","unit":{"travellerId":"1001"},"coverage":{"flightIds":["1558420526873968816"]},"selected":{"quantity":1,"options":{"airportService":{"timeWithOffset":"2026-10-28T06:00:00+03:30","guestCount":1}}}}]}]}],
 "seats":[{"variantCode":"A08","products":[{"provisionId":"1560040712915910656","serviceDefinitionRef":"ECO_SEAT_PREFERRED","units":[{"unitId":"1564392068237754368","orderServiceId":"179305053214","status":"Confirmed","unit":{"travellerId":"1001"},"coverage":{"flightIds":["1558420526873968816"]},"selected":{"quantity":1},"seatNumber":"7A","seatMapSelectionId":"QURULTF8MTU1ODQyMDUyNjg3Mzk2ODgxNnwxNTYwMDQwNzEyOTE2NjEwNjU3fDdB"}]}]}]}
```

## 5. Contract against the Offers and Resolve source (pack 15 item 5)

The Hold keeps the Offers/Resolve names (family arrays, `variantCode`, `products`, `units`, `unit`, `coverage`, `selected`, `options` with the family key). Three leaves differ on purpose; they are not literally identical:

| Offers / Resolve | Hold | Why |
|---|---|---|
| `unit.travellerRef` (string, offer-scoped) | `unit.travellerId` (long) | the Order's real traveller id; the caller maps it |
| `coverage.flightRefs` (string[]) | `coverage.flightIds` (long[]) | real flight ids for the stock sources |
| `Resolve.provision.provisionId` (flat item) | `products[].provisionId` (grouped) | the Hold groups by provision, as Offers groups products |

- The Hold `selected.options` is typed per family (R1). The Offers and Resolve output still use the six-key union (`AncillaryUnitOfferDto.Options`, `SelectedUnitDto`). Fixing that is the Owner's R1-Q5, waiting for an order.
- For every valid request, the JSON is the same as what Resolve writes.

## 6. Audit (pack 15 item 4)

| Check | Result | Evidence |
|---|---|---|
| New or removed route | none | `git diff 296197a -- src/AeroTech.Ancillary.RestApi` has no `[Http…]`/`[Route]` change |
| New provider or host change | none | Providers, ServiceHost, Consumers unchanged |
| Ordering reader or FlightFlow seat path from any reservation operation | none | `V10_S10_S11_V11_L15_L17_L18…` (reachability from Hold, SeatMaps/Holds, Confirm, Release, Cancel, Expire, GET) |
| Several orchestrators | no; one service per operation | `V11_L21…` |
| Empty selection tables | none; each of the 6 selection tables stores a chosen or fixed fact of a holdable variant (Priority has none, Owner Gate 2 Q3) | Gate 2 report |
| Ad hoc DTO fields | removed by R1 (per-family `selected`; seat fields only on seat units) | `V11_W29…` |
| Copy of Offer or Pricing | none; the GET reads only the reservation | `V11_L09…`, live steps 16–21 |
| `JsonObject` or JSON persistence | none in reservation Domain/Persistence | scan scratch `v11/g5_unused.py` |
| Generic nullable value object | the unit has one optional owned part per family, used one at a time (Owner Gate 2 Q1); not a 24-type bag | Domain `V11_D20…` |
| Duplicated stock counters | none; usage is computed by the ledger from allocation rows | scan |
| Obsolete old API records | none; the only types without a name reference are framework-discovered (validators, EF configurations, the `ToReceipt` extension class) | scan |

## 7. Variant matrix A01–A24 (pack 15 item 6)

| Variant | Status | Proof |
|---|---|---|
| A01 extra checked bag | supported | live run (A01 ×2); `V11_W01…` |
| A02 weight package | supported | `V11_matrix…(A02)` |
| A03 overweight | supported | `V11_D01…`, `V11_L02_L03…`, `V11_L09…`, typed mixed test |
| A04 oversize | supported | `V11_matrix…(A04)` with dimensions |
| A05 cabin bag | supported | `V11_matrix…(A05)` |
| A06 special equipment | supported | `V11_D01…` (`equipmentKind` captured) |
| A07 standard seat | supported through SeatMaps/Holds | `V11_L04…` (A07 append) |
| A08 preferred seat | supported through SeatMaps/Holds | live run (7A priced by `ECO_SEAT_PREFERRED`) |
| A09 extra seat | **GATED** (no sale path) | `extraSeats` refused by strict JSON (`V11_W02…`) |
| A10 upgrade | supported for `FixedAncillary`; **GATED** with a quote | `V11_D01…`; Domain `V11_D14…` |
| A11, A12 meals | supported | `V11_D01…`, typed mixed test |
| A13, A14 pets | **GATED** (forced confirmation) | `pets` not a Hold member, 400 (`V11_W28…`, Gate 4 HTTP) |
| A15 wheelchair | supported | live run; `V11_D01…`, `V11_D23…` |
| A16 disability assistance | supported | `V11_matrix…(A16)` |
| A17 medical equipment | **GATED** | `V11_matrix…(A17)`: 16419, confirmation required |
| A18 bassinet | **GATED** | `V11_D22…`: 16419 even with Immediate |
| A19 unaccompanied minor | **GATED** | `V11_matrix…(A19)`: 16419 "variant A19" with Immediate |
| A20 lounge | supported | live run (appointment); `V11_D01…`; slot inventory tests |
| A21 fast track | supported | `V11_matrix…(A21)` |
| A22 CIP meet and assist | supported | `V11_matrix…(A22)` with appointment |
| A23 priority | supported | `V11_D01…`, `V11_D23…` (`optIn` must be true) |
| A24 Wi-Fi | supported | `V11_D01…`, `V11_D23…` |

## 8. Gate failures and corrections

| When | Failure | Correction |
|---|---|---|
| Gate 1 | family-parameterised validators broke DI on host start | parameterless validators, container test |
| Gate 1–2 | the Hold request and GET reused the six-key options union and a shared unit record (found by the Owner) | R1: per-family types (Offers generic pattern), seat unit record; pack 01 §J |
| R1 | I wrote "`unit:{}` → 422" into the pack | the platform answers 400; pack, report and questions file corrected |
| Gate 3 | the Retire guard made `V10_I19`'s retire race impossible | `V10_I19` keeps close/suspend; new `V11_I15_I23` race |
| Gate 4 | a "before" GET read from the writing scope (decimal scale) | read from a fresh scope |
| Gate 5 | the live check expected A07 for seat 7A | the check compares with the definition in SQL (A08) |
| Gate 5 | `V11_matrix…(A17)` could not author A17 as Immediate or as a quote without an external supplier | the test keeps A17's forced confirmation and a filed rule |
| Gate 5 | a Backoffice token was pasted into the questions report | moved to scratch `live/`, removed from the file (never committed) |

## 9. Dev data written by Gate 5

- The four catalog products in §4.
- The live runs' test holds: run 1, run 2 and run 3 orders `1793050…`, `1793050532x`. One Expired, one Confirmed, the others Held until their 30-minute deadline.
- All dev data is test data (Owner, 2026-10-09).

## 10. GATED and NOT VERIFIED

| Item | State |
|---|---|
| Local stock (FlightCount/Weight/AirportSlot) live on dev (R06) | **GATED** — no dev reference source for flights and resources; proven only by the SQL acceptance tests |
| Ordering as a real consumer end to end (R07) | **NOT VERIFIED** — no Ordering test was run; Ancillary calls Ordering nowhere |
| A09, A10 with a quote, A13, A14, A17, A18, A19 | **GATED** (§7) |
| Supplier stock, AcceptedWeight / per-kilo | unsupported, explicit refusal (16419) |
| Offers and Resolve output union (R1-Q5) | open, waiting for an Owner order |
| SeatMaps on the Api and Backoffice surfaces | not re-run live in v11 (only OtaPanel was used for the seat id) |
