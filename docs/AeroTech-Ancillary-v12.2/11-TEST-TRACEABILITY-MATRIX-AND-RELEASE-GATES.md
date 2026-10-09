# 11 — Executable domain conformance contract: 24x6 + cross-cutting

## 0. Proof standard
Every test in this document must be backed by a real implementation test method with exact `Axx-*` ID and actual `PASS/FAIL` result, not merely scenario prose. For P1/2 tests do **not** mark product shopping/allocations/real supplier confirmation as PASSED. `P3-DESIGN_ACCEPTED` means contract described; runtime `DEFERRED_P3`. Real AirInfo/Facility/FlightFlow source unavailable => `BLOCKED_SOURCE` and the given behavior fails closed in configured host. Do not invent fake source refs in real config. Test-only fakes permitted and explicitly labeled.

## 1. Every variant test group `A01`..`A24`
Implement **six** tests per 24 variant = **144 named scenario test groups**:
- `Axx-D`: Define/Change Draft + typed specification, only correct profile fields, mandatory constraints; invalid field rejection.
- `Axx-R`: Provision eligibility/one POS per published rule, coverage, booking stage, age/date/fare/cutoff, paid/free outcome; family rule compatible.
- `Axx-P`: price mode (`Filed|ExternalQuote|Free|NotAvailable`), correct unit and Money, tax/fee basis; provider quote not claimed implemented.
- `Axx-I`: correct InventoryAuthority + commercial usage cap, no fabricated quota; local pool only source evidence; configuration snapshot truthful.
- `Axx-Q`: EF Command/QueryDb round-trip `Define → Change → Publish → GET`, `Revise`/immutable Active and projection, no unrelated fields exposed.
- `Axx-N`: one or more negative boundary examples from chapter 04 (exact rejection business code, not 500).

For every group map to source class/line in report and SQL integration test; use `Theory` rows only if reporting preserves distinct Axx status. **All 144 groups must PASS for true P1/2 Domain DONE**, except explicitly evidence-dependent Activate integration marked `BLOCKED_SOURCE` with correct refusal; don't claim operational readiness.

## 2. Cross-cutting contracts
| Test | Required executable evidence |
|---|---|
| CT-01 | all 24 variant codes map exactly one of 9 closed profiles; no unknown spec publication |
| CT-02 | two profile details attached to one Definition rejected |
| CT-03 | wrong-family Provision Baggage/Seat/Medical rules rejected |
| CT-04 | Active Definition immutable, Revise copies entire single profile spec with correct child rows |
| CT-05 | same stable product ref cannot mutate Profile/Variant in place; new identity required |
| CT-06 | one Active Definition version per stable owner+ref and one Active Pricing per Provision via SQL |
| CT-07 | one v12.2 published Provision = exactly one POS; no cross-POS applicability leak |
| CT-08 | sale item accepted qty>=1, UI 0 means absent; maximum of IBAG 6 not physical remaining |
| CT-09 | Bound/Portion covering 2 flights charged once, not per flight |
| CT-10 | different outbound/inbound A01/A13 eligibility, no assumed symmetry |
| CT-11 | A02 2*10kg package counts purchased 2 items and consumption equivalent 20kg only if defined |
| CT-12 | A03/A04 combined surcharge respects ChargeCombination, no unintended double billing |
| CT-13 | Pet declared weight/dimension/allowed species contract input schema round-trips, no PII buyer values in Catalog |
| CT-14 | Child age/guardian input schema required on UMNR, but zero actual guardian data in published Spec |
| CT-15 | WCHR Free SSR publishes no Active Pricing, `MustCheckAvailability=true` possible with Unlimited |
| CT-16 | WCHC `Pending` vs `Guaranteed` treated as future offer state, not local capacity balance |
| CT-17 | Meal family mutually exclusive per traveller/flight configuration; actual cross-order consumption deferred |
| CT-18 | A10 Paid ExternalQuote without fake zero Money, publisher needs valid quote authority |
| CT-19 | A07/A09 Seat/EXST physical authority FlightFlow only; no local Count seat stock |
| CT-20 | CIP Airport/Facility/ServiceWindow/IANA DST invalid instant rejected in authoring/evidence validation |
| CT-21 | included tax vs added tax = no double addition; per Ticket Fee separate UnappliedFee |
| CT-22 | currency scales 0,2,3 preserved and invalid scale fails closed with test fixture |
| CT-23 | invalid real currency source returns blocked error and does not overwrite ReferenceData |
| CT-24 | malformed profile DTO HTTP 400/422; correct real JWT requests pass through serializer, model binding and mapper |
| CT-25 | same stable physical resource bound to 2 SKUs has one inventory counter |
| CT-26 | Policy version current as only Active/current Definition; retired versions not OR'ed for RequiresAvailabilityCheck |
| CT-27 | stored `ServiceDefinitionId` and read-only `CurrentServiceDefinitionId` distinct and truthful |
| CT-28 | absent policy != Unlimited; `Unlimited`+availability check != guaranteed |
| CT-29 | missing Local source => `SourceUnavailable`, no Activate / no fake capacity |
| CT-30 | Draft/Active/Retired lifecycle and 10 shared Rule groups retained intact |
| CT-31 | legacy v12.1 priced rows and IDs preserved through migration with documented ambiguous exceptions |
| CT-32 | CommandDb and QueryDb model snapshots current; no leftover `AncillaryPricingLine` new mappings |
| CT-33 | Tenant/Airline isolation of family fields, Stock source and Pricing data |
| CT-34 | no changes in Frozen Reservation SHA/files, no P3 runtime new operations |
| CT-35 | 9 specialist Backoffice actual HTTP smoke happy + negative against live bound host |
| CT-36 | all 24 authored Specifications persist/read correctly on SQL Server with real EF mapping |
| CT-37 | `PurchaseStage.OnBoard=5` only when relevant (A24), all old numeric values stable |
| CT-38 | enum code names and numeric values unchanged for `ServiceCoverageScope` and `PricingUnit` |
| CT-39 | no P3 buyer values in product definitions and no sensitive PII in logs |
| CT-40 | contract examples for future `serviceId + travelerId + quantity` include typed selection/TTL binding, no unsourced live P3 claim |

## 3. SQL Server race and migration tests
- `SQL-C01`: exactly one success for 100 concurrent adjustment requests using same expected Version for one source; 99 version conflicts; no lost changes.
- `SQL-C02`: replay same `CorrelationId` and same adjustment returns same result, no 2nd adjustment; different amount same correlation gets business conflict.
- `SQL-C03`: exactly one of 100 nonidentical overlapping AirportSlot intervals for same facility wins; adjacent `[09:00,10:00)`, `[10:00,11:00)` allowed; no overlapping active records.
- `SQL-C04`: 100 different sources do not encounter a global serial lock hotspot; all succeed.
- `SQL-C05`: two valid policies bind same ResourceId on same flight, count source remains one; CountUnit inconsistent binding refused.
- `SQL-C06`: `decimal(18,3)` CapacityKg and `decimal(19,6)` Amount round trip on actual SQL Server, not in-memory.
- `SQL-C07`: migrations Up on v12.1 seeded source DB, all surviving PK/Price/Children/History reconciled; QueryDb synchronized.
- `SQL-C08`: Down test tells truth about restore/loss; backup or forward repair where needed; no deceptive "reversible" PASS.
- `SQL-C09`: filtered unique current policy key + active Pricing versions robust against double activation.
- `SQL-C10`: concurrency same Portfolio Ref across two new Definition revisions one Active; no type drift.

## 4. Architecture proof budget
**Final mandatory matrix:** 144 A-group checks + 40 cross-contract + 10 SQL stress/migration + minimum 18 real HTTP smoke (9 positive,9 negative) = **212 documented checks**. Some test methods may cover multiple checkpoints, but report their separate IDs individually and link exact methods. No meaningless multiplication of assertion counts. No test is considered "PASS" solely because the fixture creates a Default Spec then ignores its specialized fields.

## 5. Report format and closure labels
Create `reports/V12.2-PHASE1-PHASE2-IMPLEMENTATION-AND-CONFORMANCE.md` with:
- branch, base SHA and final unpushed working SHA, `git diff --stat`, commit status;
- per-24 scenario `D/R/P/I/Q/N` PASS/FAIL with test method names; `CT` and `SQL-C` status;
- DTO/EF/DB snapshots and migration commands/output; Count of tests discovered/executed/passed/skipped;
- list of actual external evidence source binding and feature deactivation due to unavailable provider;
- exact classification of any RefData data problem, no overrides;
- show `PHASE1_PHASE2_DOMAIN_READY_FOR_OWNER_AUDIT` **only** if all P1/2 check groups pass and deficits are accurately blocked as source-dependent; no `PRODUCTION_READY` if external currencies/stock source unresolved;
- Phase3 labelled `DESIGN_ONLY_NOT_IMPLEMENTED`;
- test run environment/database details; no CI claims without CI evidence; stop for Owner audit without push.
