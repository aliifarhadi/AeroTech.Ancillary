# Mandatory contract tests for 15 service families; no unverifiable success claims

## Status labels
Tests specified here are **TO BE IMPLEMENTED/EXECUTED** by Coding Agent. This design pack does not claim any new tests have passed. Report `PASS`, `FAIL`, `BLOCKED_NO_REAL_EVIDENCE`, `DEFERRED_PHASE3`, `NOT_RUN` per scenario; do not convert deferred into green.

## A. Price and currency 25 cases
Run `PR01..PR25` verbatim from doc 03, plus SQL proofs that filtered unique active index and `(PricingId,CurrencyId,PTC,age bands)` uniqueness are effective. Verify old↔new rates and historical total amounts line-for-line. Rejected fields cannot sneak through alternate DTO validators. Verify currency `DecimalPlaces=0,2,3` with `CurrencyReadModel` fixture, no decimal(18,2) truncation in EF.

## B. Family scenario matrix: 15 * 4 = 60 documented checks
For each family row 01..15 of doc 06, implement:
- `Fxx_DEF`: ServiceDefinition and typing, Document/Booking policy, DateBasis, PricingUnit validation.
- `Fxx_RULE`: relevant Provision rules, booking stage/confirmation, eligibility/limits/blackout/sequence; no production shopping engine.
- `Fxx_PRICE`: Paid Rate multi-currency/selector or Free no active Pricing, Amount+Currency round trip.
- `Fxx_POLICY`: InventoryAuthority configuration without inventing quotas, verify reference gating and truthful `IsGuaranteed=false`.
Use at least: weight baggage package 2 x 10kg per-person; piece 15/23/32; overweight; bicycle; pet subject-to-confirmation; paid seat FlightFlow authority; free VGML; free WCHR; paid UMNR; priority stage; lounge supplier/verified slot; CIP timed slot; Wi-Fi Unlimited; insurance Supplier; eSIM Supplier/unlimited. Never assert a particular airline offers all products/markets.

## C. Core cross-phase invariants 20 cases
| ID | Proof |
|---|---|
| X01 | `Unlimited` and `MustCheckAvailability=true` can coexist with no local count, but `guaranteed=false` |
| X02 | `NotConfigured` != Unlimited |
| X03 | No physical FlightCount/FlightWeight seeded from baggage sale type |
| X04 | Seat never has local duplicate occupancy |
| X05 | Facility unknown => Local AirportSlot Activation rejected explicitly, Draft can persist |
| X06 | Source missing => Local FlightCount/Weight Activation rejected, not bypassed |
| X07 | Immutable adjustment replay same correlation idempotent |
| X08 | Same correlation with different payload conflicts |
| X09 | 100 concurrent expectedVersion writes to one source yield one winner |
| X10 | 100 overlapping slots same facility yield one winner and no overlapping persisted active slots |
| X11 | Adjacent `[start,end)` slots permitted |
| X12 | 100 independent sources without global lock hotspot are permitted |
| X13 | Product version change keeps one Policy id and physical resources; no stale identity |
| X14 | Two service SKUs share true count/weight resource only with explicit typed binding |
| X15 | Count+Weight binding only one each, not arbitrary list |
| X16 | PassengerUsageLimit is not physical total or an actual cross-order ledger |
| X17 | Deferred Daily/Room Night/Assigned Asset cannot be activated |
| X18 | SourceUnavailable never mapped to unlimited or Available |
| X19 | Published v12.1 Rules and Pricing historical versions unchanged by capacity updates |
| X20 | Domain/SQL migrations leave existing M1 Reservation, Hold/Get/Confirm API unchanged |

## D. Authorization/migration tests
- Verify Backoffice caller owner/Actor against actual caller context (403/404), no user-supplied audit actor trust. Use real SQL Server for unique/overlap/rowversion tests (SQLite/in-memory not valid proof).
- New SQL schema on empty and realistic cloned existing database; pre/post all Rate amounts+totals+currency per selector and exact lifecycle/history, old columns cleanup only after backup. Query projection equality after commit.
- Build solution and full Domain/Acceptance suites; audit ID/type values with actual code and avoid rewriting unrelated tests to force pass.
- Agent produces a scenario index with links to exact test names and evidence, plus `DOMAIN_COMPLETE_P1_P2_READY_FOR_OWNER_AUDIT` marker ONLY if all domain-owned/authoring gates passed. Operational Local activation whose evidence source is absent remains BLOCKED truthfully and must not be marked complete in deployment.

## E. Non-goals
Do NOT run Phase 3 hold/confirm allocation stress tests as if Phase2 completed them. Cross-order consumption/available-to-sell math, provider API acceptance and EMD remain future. Do NOT redefine "workable" as promising 10 slots without a verified facility.
