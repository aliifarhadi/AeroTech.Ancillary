# Pack v8 — Stage4: ResolveGrouped

| Item | Value |
|---|---|
| Branch | `feat/ancillary-offers-v8`, HEAD `851ff5a`; Stages 1–4 uncommitted |
| Authority | pack `docs/Shopping` v8 (`14`, `02` §5, `03` §6, `04` §5, `05` §A–C5), your rulings, Q1 and Q3 |
| Endpoint | `POST Service/v1/Ancillaries/Selections/ResolveGrouped`, body `{ids}` |
| Database | no migration, no write |
| Tests | none, on your order; proof is the clean build |
| Handoff contract | `reports/V8-STAGE4-ORDERING-HANDOFF.md` |

## 1. Flow

1. **Query** `ResolveGroupedSelectionsService` checks the request shape, as the SeatMap Resolve does: 1..`SelectionLimits.MaximumIds` ids, none blank, each once (422, 16801). Unknown body members are refused with 400.
2. **Shopping** `Resolution/SelectionResolver`:
   - an `s_` id is read from the snapshot table, in one query for all `s_` ids. A missing row gives 404 (16819) and an expired one 410 (16817);
   - any other id goes to the Stage1 codec with purpose `d`. An expired one gives 410; a bad MAC, wrong purpose or an `r_` gives 422 (16816);
   - the rates are read through the Active-only pricing reader (by rate id), then the Active provisions and Active definitions, one query set for the whole batch;
   - each item must still match: the rate's parent Pricing is Active, the Provision and Definition are Active, the Provision coverage equals the signed or stored one, the product is not a Seat, and an `s_` belongs to a product that takes customer input while a `d_` belongs to one that does not. Any failure gives 409 (16820) for the whole call.
3. **Query** `ServiceResolvedSelectionsMapper` writes the ten family arrays.

No offer, AirAvail, Ordering, FlightFlow or inventory call is made, and no price is recomputed. Quantity comes from the `d_` or the snapshot. `unitAmount` is the Domain's `AncillaryPricingRate.UnitTotal`. `totalAmount` is `unitAmount × quantity`. `selected` is the stored snapshot JSON, unchanged.

## 2. Reuse instead of new rules

| Need | Reused source |
|---|---|
| Unit price | `AncillaryPricingRate.UnitTotal` (Domain) |
| `requiresCheckAtReserve` | the engine's rule, moved into `AvailabilityAssessor.MustCheckAtReserve` and used by both |
| Confirmation requirement | the engine's expression: provision pet acceptance mode, otherwise the definition's |
| Typed or not | `CustomerInputs.AreRequested`, the same catalog as Stages 2–3 |
| Scope enum | `SelectionScopes.Of`, moved out of the listing and used by both |
| `details` | the Stage2 family mappers, now taking the definition and provision instead of a shopping row |
| `unit` DTO | the Stage2 unit mapper |

## 3. Proof

| Step | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 errors, 57 warnings |
| SeatMap freeze | `cf4211192fe359c43897814dc9716406e442b84578e75ea6daabfd1c2896e39d`, same as Stages 2–3 |
| Comments, `Channel`, dictionary-of-object in Shopping, banned file names | none in 169 changed `.cs` files |
| Line endings | 175 files checked, 0 with a lone LF |

Not proven: no test (your order) and no live HTTP call. Scenarios R01–R14 are mapped in the handoff contract §4 but not executed.

## 4. Points for your review

They are S4-1 to S4-5 in `reports/V8-QUESTIONS-AND-CONFLICTS.md` §5.
