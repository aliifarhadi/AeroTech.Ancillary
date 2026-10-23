# Pack v8 — Stage3: typed options and the selection snapshot

| Item | Value |
|---|---|
| Branch | `feat/ancillary-offers-v8`, HEAD `851ff5a`; Stages 1–3 uncommitted |
| Authority | pack `docs/Shopping` v8 (`13`, `04` §3–4, `02` §4, `03` §4–5), your rulings, Q1–Q4 |
| New endpoints | `GET {Surface}/v1/AncillaryOffers/{Family}/{reference}/Options` and `POST {Surface}/v1/AncillaryOffers/{Family}/{reference}/Options` on Backoffice, OtaPanel and Api |
| Database | migration `20261023195016_SelectionSnapshots`: one table `Ancillary.SelectionSnapshots`; applied to the dev DB |
| Tests | none, on your order; proof is the clean build |
| SeatMap | unchanged since Stage2 (hash `cf4211…e39d`, only the Q3 file differs from `49e9209`) |

## 1. Flow

GET and POST start the same way:

1. The `r_` is read with the Stage1 codec, purpose Reference only. A bad MAC gives 422 (16816) and an expired one gives 410 (16817).
2. The offer is reloaded with the `offerId` query parameter (S3-8; before 2026-10-24 it was signed inside the `r_`), through the existing offer source, the caller's sales scope and `PricedOfferScopeGuard`.
3. The point-of-sale context whose sales binding equals the signed one is selected. No match gives 403 (16818). If the loaded offer's binding does not equal the signed `ob` (another or a changed offer), the result is 409 (16820).
4. Rate → Pricing → Provision → Definition are read through the Active-only readers. A missing link gives 409. A definition of another family than the route gives 404 (16819). A product that takes no customer input gives 409.

Then:

- **GET** returns `{variantCode, fields:{name:{control, required, choices?, unit?, min?, exclusiveMin?, max?}}}` (`exclusiveMin` added with your approval on 2026-10-24, see S3-7) built by `Shopping/Options/TypedSelectionForm` (pack 03 §4). Values fixed by the definition are not asked again (03 §2).
- **POST** takes the family body, fills the fixed values from the definition (A06 equipment kind, A20–A22 airport and facility, A24 plan), and runs the engine's `EvaluateSelectionAsync` on the same candidate (Q4). It accepts only an `Accepted` evaluation with the same rate that passes the Stage1 sell gate. Then it stores one snapshot and returns `{selectionId, expiresAtUtc}`.
- **POST errors:** field issues give 422 (16821); any other refusal gives 409 (16820). A23 refuses an `optIn` that is not `true` with 422, because the engine checks only that the field is present.

`expiresAtUtc = min(r_ expiry, now + Shopping:SelectionMaxAge)`. The `s_` id is `s_` + Base64Url of 16 random bytes, 24 characters.

## 2. Form per variant (GET) and body per family (POST)

| Variant | Fields asked |
|---|---|
| A03 | `bagRef` text R; `measuredWeightKg` number R, kg, `exclusiveMin` = tier lower bound (or 0), max = tier upper bound |
| A04 | `dimensions` R, cm |
| A06 | `dimensions` R when max size or linear sum is defined; `weightKg` number R, `exclusiveMin` 0, max per piece, when defined |
| A13, A14 | `animalType` select R; `otherAnimalCode` select when RegisteredOther is allowed; `animalAgeWeeks` number, min = provision override or definition, when defined; `combinedWeightKg` number R, `exclusiveMin` 0, max = provision override or definition; `carrierDimensions` R; `documentAcknowledgements` multi-select R when codes exist; `sizeBracket` select R when brackets exist |
| A15, A16 | `assistanceSsrCode` select R from the allowed SSR codes |
| A18 | `infantRef` select R (INF travellers linked to the unit traveller); `guardianRef` select R (the unit traveller), as the engine's bassinet rule requires |
| A20–A22 | `timeWithOffset` date-time R when an appointment is required; `guestCount` number, 0..max, when guests are allowed |
| A23 | `optIn` checkbox R |
| A24 | `deviceCount` number 1..max when `MaxDevices` is set |

POST bodies: `Baggages {bagRef, measuredWeightKg, dimensions, weightKg}`, `Pets {animalType, otherAnimalCode, animalAgeWeeks, combinedWeightKg, carrierDimensions, documentAcknowledgements, sizeBracket}`, `AssistedTravel {assistanceSsrCode, infantRef, guardianRef}`, `AirportServices {timeWithOffset, guestCount}`, `PriorityServices {optIn}`, `Connectivity {deviceCount}`. Unknown members are refused with 400.

Stage2 now asks the same catalog whether a variant takes input, so the list and the options can no longer disagree.

## 3. Snapshot table (pack 04 §3)

| Column | Type |
|---|---|
| Id | varchar(24) PK |
| Version | smallint (1) |
| PricingRateId | bigint |
| CoverageScope | smallint |
| TravellerRef | nvarchar(100) null (null for Order) |
| CoverageRef | nvarchar(100) |
| OfferBinding, SalesBinding | char(43) |
| Quantity | int (1) |
| SelectedJson | nvarchar(max): only the family body the customer sent, nulls left out |
| CreatedAtUtc, ExpiresAtUtc | datetimeoffset |

`SelectionSnapshot` (Domain) guards these rules at creation. It does not derive from `AggregateRoot`, because that base would add `RowVersion`, `LastUpdateTime` and `LastUpdatedBy`, which pack 04 does not list. The repository has only `AddAsync`.

## 4. Layers

| Layer | Content |
|---|---|
| Domain | `SelectionSnapshot`, `ISelectionSnapshotRepository`, errors 16816–16822; D2 fields on the A17 Oxygen contract: `OxygenFlowRateLitersPerMinute` (required, > 0), `FrequentTravellerMedicalCardNumber` (optional) |
| Persistence | configuration, repository, `DbSet`, migration |
| Shopping `Options/` | `TypedSelectionForm` (fields and fixed values), `TypedSelectionService` (read reference, binding and active chain, form, proof), descriptor records |
| Shopping `Selections/` | the sell gate moved from the signer into `SellableCandidate`, used by the signer and the proof |
| Shopping engine | D2 fields in the selection record and the field map; flow rate > 0 |
| Application | `SubmitSelectionOptions`: six family inputs, one service, one validator base, thin Backoffice / OtaPanel / Api command, handler and validator; port `ITypedSelectionVerifier` |
| Query | `TypedSelectionVerifier` (port implementation: scope by surface, offer reload, call Shopping), `GetSelectionOptions` service with thin per-surface queries and handlers, DTOs, `Common/OfferFamilyRoutes` |
| RestApi | the three controllers: one GET and six POST actions each |

## 5. Proof

| Step | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 errors, 57 warnings |
| Migration on the dev DB | `20261023195016_SelectionSnapshots` is the latest entry; the table has the 12 columns of §3 with those exact types |
| SeatMap freeze | `cf4211192fe359c43897814dc9716406e442b84578e75ea6daabfd1c2896e39d`, same as Stage2 |
| Comments, `Channel`, dictionary-of-object in Shopping, banned file names | none in 149 changed `.cs` files |
| Line endings | 154 files checked, 0 with a lone LF |

Not proven: no test (your order), no live HTTP call, no rollback run. The migration's `Down` drops the table. Scenarios O01–O12 are therefore not yet shown.

## 6. Points for your review

They are S3-1 to S3-6 in `reports/V8-QUESTIONS-AND-CONFLICTS.md` §4, each built as stated.
