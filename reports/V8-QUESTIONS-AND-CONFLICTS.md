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

Answer:

### Q2. Free for A11 and A15 — the Domain forbids Paid

Pack: every sellable item carries a real `PricingRateId`; Free is authored as Paid + Filed with a zero rate.
Source: `AncillaryProvision.Constraints.cs:12-15` allows only `Free` and `NotAvailable` for A11 (FreeSpecialMeal) and A15 (Wheelchair). Paid is impossible for them.
Dev DB today: Active provisions with `PriceOrigin = Free` exist for A03, A06, A11 (8), A15 (3), A16 (3), A18, A20, A21, A22, A23, A24. Without a rate none of them can get an id, so they would disappear from Shopping.

Options:
- a) Allow Paid for A11 and A15 (that one rule and its tests), then turn the dev Free provisions into Paid + a zero-rate pricing by SQL, as was done for seats on 2026-10-21.
- b) Keep the Free disposition and let a Free provision carry a zero-rate pricing (bigger Domain change: Free provisions have no pricing today).

Benchmark (my recollection, not verified in this turn): ATPCO S7 marks free services with a no-charge indicator rather than a zero amount, so a) is an AeroTech choice, consistent with the seat decision.
Recommendation: a).

Answer:

### Q3. The frozen SeatMap Resolve uses `EvaluateBatchService.MaximumLines`

`Query/Shopping/SeatMaps/Queries/ResolveSeatMapSelections/ResolveSeatMapSelectionsService.cs:2,46-47` imports the EvaluateBatch namespace and reads `EvaluateBatchService.MaximumLines` (50). Deleting EvaluateBatch breaks it.

Recommendation: move the constant to one shared place (`Query/Shopping/Common/SelectionLimits.MaximumIds = 50`, also used by ResolveGrouped) and change those three lines of the SeatMap file. Behaviour is identical; the SeatMap hash changes and is recorded.

Answer:

### Q4. How POST Options validates a typed choice

The existing engine checks typed choices with the offer context: pet weight/age, bassinet infant and guardian, airport time windows, etc. (`Shopping/Engine/ProvisionEvaluation.ProfileRules.cs`). The `r_` of the pack carries only a hash of the offer, so POST Options cannot reload the offer, and the engine cannot run.

Options:
- a) Put `offerId` inside the signed `r_` payload (never in the POST body, never in Resolve). POST Options reloads the offer like Shopping does and runs the existing `EvaluateSelectionAsync` on the same candidate. One rule set, no duplicate validators; costs one AirAvail call per POST.
- b) Write context-free validators per variant in Shopping (field shape + specification limits only). Rules that need the offer (A18 infant age, pet age, appointment windows) cannot run, so those variants stay gated.

Recommendation: a).

Answer:
