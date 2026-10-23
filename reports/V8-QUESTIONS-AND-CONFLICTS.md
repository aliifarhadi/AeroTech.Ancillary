# Pack v8 — questions before code

Baseline: `k8s-stg@49e9209`. Pack v8 verified (`SHA256SUMS.txt` OK, `VERIFY-PACK.py` PASS). Owner rulings of 2026-10-23 already applied and not repeated here: no `offerId` in Resolve, JWS for `r_`/`d_`, `SelectedJson nvarchar(max)` allowed, Backoffice `{offerId}` only, family arrays on all three surfaces, old Details / Evaluate / EvaluateBatch / Resolve deleted, 16xxx codes, TTL by configuration.

Only the four points below change the design. Each has the source evidence and my recommendation.

## 1. How the code is laid out today

| Concern | Where it lives now | File |
|---|---|---|
| Engine, coverage units, price, quantity, profile rules | Shopping project | `src/AeroTech.Ancillary.Shopping/Engine/*` |
| Active catalog read ports + SQL readers | Shopping project | `Shopping/Reading/*`, `Shopping/Reading/Sql/*` |
| Offer loading, sales scope, POS eligibility | Query project | `Query/Shopping/Common/*` |
| Shopping orchestration per operation (list, detail, evaluate, batch, resolve) | Query project | `Query/Shopping/AncillaryOffers/Queries/*` |
| Selection token (JWS HS256) | Query project | `Query/Shopping/Common/PricedSelections/PricedSelectionTokens.cs` |
| Write that needs shopping proof (seat hold) | Application command + port `ISeatSelectionVerifier`, implemented in Query | `Application/AncillaryReservationAggregate/Services/ISeatSelectionVerifier.cs`, `Query/Shopping/SeatMaps/Queries/VerifySeatSelections/SeatSelectionVerifier.cs` |

References: Query → Shopping, Application, Providers. Shopping → Domain, Persistence. Application → Domain.

Siblings: Ordering.Final `Query` only reads read models (`GetBackofficeOrderByIdService` reads and maps). AirAvail has no Query project; its business lives in `AeroTech.AirOffer.Services`.

## 2. Questions

### Q1. Where each piece of the new flow lives

Your rule: Query only reads the database, Shopping holds the shopping business. One hard constraint: the frozen SeatMap uses `Query/Shopping/Common` (offer source, sales scope), so that code cannot move without editing SeatMap files.

Proposal:

| Piece | Layer |
|---|---|
| Signed ids `r_`/`d_` (JWS codec moved from Query, purposes, expiry, bindings) | Shopping, new folder `Shopping/Selections/` |
| Classification Direct / Quantity / Typed, `quantityOptions` enumeration, issuance gate (eligible, complete price, active rate, PTC/POS of the candidate) | Shopping |
| Typed form catalog (GET Options) and typed validation rules (POST Options) | Shopping |
| Resolve rules (verify id, load active Rate→Pricing→Provision→Definition, build the resolved item) | Shopping |
| New read ports (pricing by rate id, snapshot by id) | Shopping `Reading/` + `Reading/Sql/`, the existing Shopping pattern |
| Snapshot entity + invariants + repository port | Domain |
| Snapshot EF configuration, repository, migration | Persistence |
| POST Options (a write) | Application: one service + thin Backoffice / OtaPanel / Api commands, like every command here; proof through an Application port implemented in Shopping (same shape as `ISeatSelectionVerifier`, implementation in Shopping instead of Query). Needs a new reference Shopping → Application |
| GET list, GET Options, Resolve: MediatR query + handler + DTO + mapper per surface | Query: the handler loads the offer and sales scope with the existing `Common` code, calls the Shopping service, maps the DTO; no rule in Query |

Recommendation: accept this table.

Answer: accepted as recommended (Owner, 2026-10-23).

### Q2. Free for A11 and A15 — the Domain forbids Paid

Pack: every sellable item carries a real `PricingRateId`; Free is authored as Paid + Filed with a zero rate.
Source: `AncillaryProvision.Constraints.cs:12-15` allows only `Free` and `NotAvailable` for A11 (FreeSpecialMeal) and A15 (Wheelchair). Paid is impossible for them.
Dev DB today: Active provisions with `PriceOrigin = Free` exist for A03, A06, A11 (8), A15 (3), A16 (3), A18, A20, A21, A22, A23, A24. Without a rate none of them can get an id, so they would disappear from Shopping.

Options:
- a) Allow Paid for A11 and A15 (that one rule and its tests), then turn the dev Free provisions into Paid + a zero-rate pricing by SQL, as was done for seats on 2026-10-21.
- b) Keep the Free disposition and let a Free provision carry a zero-rate pricing (bigger Domain change: Free provisions have no pricing today).

Benchmark (my recollection, not verified in this turn): ATPCO S7 marks free services with a no-charge indicator rather than a zero amount, so a) is an AeroTech choice, consistent with the seat decision.
Recommendation: a).

Answer: accepted as recommended (Owner, 2026-10-23).

### Q3. The frozen SeatMap Resolve uses `EvaluateBatchService.MaximumLines`

`Query/Shopping/SeatMaps/Queries/ResolveSeatMapSelections/ResolveSeatMapSelectionsService.cs:2,46-47` imports the EvaluateBatch namespace and reads `EvaluateBatchService.MaximumLines` (50). Deleting EvaluateBatch breaks it.

Recommendation: move the constant to one shared place (`Query/Shopping/Common/SelectionLimits.MaximumIds = 50`, also used by ResolveGrouped) and change those three lines of the SeatMap file. Behaviour is identical; the SeatMap hash changes and is recorded.

Answer: accepted as recommended (Owner, 2026-10-23).

### Q4. How POST Options validates a typed choice

The existing engine checks typed choices with the offer context: pet weight/age, bassinet infant and guardian, airport time windows, etc. (`Shopping/Engine/ProvisionEvaluation.ProfileRules.cs`). The `r_` of the pack carries only a hash of the offer, so POST Options cannot reload the offer, and the engine cannot run.

Options:
- a) Put `offerId` inside the signed `r_` payload (never in the POST body, never in Resolve). POST Options reloads the offer like Shopping does and runs the existing `EvaluateSelectionAsync` on the same candidate. One rule set, no duplicate validators; costs one AirAvail call per POST.
- b) Write context-free validators per variant in Shopping (field shape + specification limits only). Rules that need the offer (A18 infant age, pet age, appointment windows) cannot run, so those variants stay gated.

Recommendation: a).

Answer: accepted as recommended (Owner, 2026-10-23).

## 3. Stage2 — points applied, for your review

Each point is already built as stated and can be flipped without touching the rest.

| # | Point | Source | Applied |
|---|---|---|---|
| S2-1 | AssistedTravel `details` shape | 02 §3 nests the variant object (`assistanceKind` + `wheelchair{…}`); 03 §3.4 is flat with `kind`. 00 says not to pick one | 02, because it is the response authority and the definition DTO of this repository nests the same way (`WheelchairSpecificationDto(Wheelchair)`) |
| S2-2 | Backoffice enums | 02 says enum string; the platform rule and the old Backoffice offers DTO use `{value, name, title}` | `{value, name, title}` on Backoffice; plain strings on OtaPanel and Api, which share one DTO as in Ordering and AirAvail |
| S2-3 | `medicalEquipment`, `unaccompaniedMinor` sub-objects | 02 §3 lists them; A17 and A19 are gated | not built; they can never be emitted |
| S2-4 | `includedBaggage` in baggage details | 03 §3.1, optional, from the Offer | not built |
| S2-5 | Element types of `extraSeats` and `travelInsurance` | 03 §3.9 defines ExtraSeat details; 03 §3.10 says insurance has none | `ExtraSeatDetailsDto` as 03 §3.9, empty `TravelInsuranceDetailsDto`; both lists always `[]` |
| S2-7 | `optionsEndpoint` repeated the 509-character `r_` | 02 §2.2 "exact route returned to caller"; in a live answer `reference` and `optionsEndpoint` took 52% of 79,643 bytes | **decided by you 2026-10-24**: `optionsEndpoint` stays, as the template `/{Surface}/v1/AncillaryOffers/{Family}/{reference}/Options` with `{reference}` left literal |
| S2-6 | Fixed quantity above 1 | 02 §2.5 says min/max 1 for a fixed Direct | a `QuantityChoice` provision with min = max signs its min, and the policy shows that value; it is 1 in every case where min = 1 |

## 4. Stage3 — points applied, for your review

| # | Point | Source | Applied |
|---|---|---|---|
| S3-1 | Where the POST proof runs | Q1 says the port is implemented in Shopping; Q4 says POST reloads the offer, and the offer source is in `Query/Shopping/Common`, which Shopping cannot reference | port implemented in Query like `SeatSelectionVerifier` (reads scope and offer only); every rule is in `Shopping/Options/TypedSelectionService`; no new Shopping → Application reference |
| S3-2 | Control names | 02 §4 lists `select, text, number, checkbox, date-time`; 03 §4 also needs `dimensions`, a multi-select and a traveller select | the five of 02, plus `dimensions` and `multi-select`; traveller choice is a `select` of traveller refs |
| S3-3 | `s_` collision retry once | 04 §3 | not built: 128 random bits, and a retry needs a new path through the unit of work |
| S3-4 | Meals and Upgrades options | 02 §1 lists both families | no POST route: A10, A11 and A12 take no customer input in the current domain, so no `r_` exists for them; GET answers 409 |
| S3-5 | 404 versus 409 for a missing rate chain | 02 §6 | 409 for both, because the readers return Active rows only; a forged rate cannot pass the MAC |
| S3-7 | Exclusive lower bounds in the form | 02 §4 has only inclusive `min`; the engine refuses a weight at the lower bound | **approved by you 2026-10-24**: descriptor field `exclusiveMin` (a number, like JSON Schema `exclusiveMinimum`): A03 `measuredWeightKg` = tier lower bound or 0, A06 `weightKg` = 0, A13/A14 `combinedWeightKg` = 0 |
| S3-8 | offerId inside every `r_` | 04 §4 "the reference path is the only binding argument; no OfferId in POST body"; `oid` was about a third of each `r_` | **decided by you 2026-10-24**, rollback point commit `838e839`: `oid` removed from `r_`; GET and POST Options take `?offerId=`; the server loads that offer and accepts it only when its hash equals the signed `ob` (otherwise 409); a missing offerId gives 16803. `optionsEndpoint` = `…/{reference}/Options?offerId={offerId}`. Live: `r_` 509 → 334 characters |
| S3-6 | A18 bassinet | 04 §1 gates it while no trusted traveller source exists | open: the offer reload of Q4 is that trusted source; choices follow the engine (guardian = unit traveller, infants linked to it) |

## 5. Stage4 — points applied, for your review

| # | Point | Source | Applied |
|---|---|---|---|
| S4-1 | Service auth | 02 §1 and 14 say Service Auth; your ruling of 2026-09-30 says Service surfaces stay anonymous | anonymous, like the SeatMap Service controller |
| S4-2 | Error for a bad request shape | 02 §6 says 400 `INVALID_REQUEST` | unknown members 400 (framework); count, blank and duplicate ids 422 (16801), as the SeatMap Resolve does |
| S4-3 | `requiresCheckAtReserve` | 02 §5.2: "inventory/fulfillment policy; informational"; 04 §5 allows only the Rate → Definition chain | the engine's provision and definition rule (must check availability, or subject to confirmation); the inventory policy is not read |
| S4-4 | Coverage of a Flight unit | 02 §5 "for signed ID use Unit refs" | `portionRefs: []` for a Flight unit; the bound is not parsed out of the flight ref |
| S4-5 | Kind check | 04 §5.3 "typed ID belongs to known Variant" | an `s_` must be a product that takes input and a `d_` one that does not; otherwise 409 |

## 6. Stage5 — points applied, for your review

| # | Point | Source | Applied |
|---|---|---|---|
| S5-1 | Domain pin "no Snapshot type" | `P1BoundaryConformanceTests`; pack 04 names the table and Q1 put the entity in Domain | the two snapshot types are named exceptions, like `ProvisionCancellationRule` |
| S5-2 | Cases the Domain makes impossible | S02, O04, O08 | GATED with a test that proves no fake output: no Order unit for a quantity variant, pets always subject to confirmation, one menu item per definition |
| S5-3 | Live Backoffice | pack 15 §2 | Api, OtaPanel and Service Resolve ran live. Backoffice refuses the agency-priced offer (403, existing guard) and AirAvail staging's Backoffice search answered 500, so no Backoffice-priced offer exists to run it with; a Backoffice offerId from you closes this |
