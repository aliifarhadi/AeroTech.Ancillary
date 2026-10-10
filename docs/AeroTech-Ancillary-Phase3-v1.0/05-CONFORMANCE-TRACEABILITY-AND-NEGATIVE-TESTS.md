# 05 - Phase3 Traceability, A01-A24 scenarios, integration and stress matrix

**Test status in this pack = NOT EXECUTED.** These are acceptance criteria. Agent must run source-level tests and report exact commands/results, never copy Phase2 historical counts as Phase3 evidence.

## 1. Definition of Done per section

P3.1 `PASS_ENGINE`: complete typed Context, 10-rule traceability, active/current version selection, 24-variant candidate/schema, accurate price/tax/fees, truthful inventory/usage limits and deterministic results, no source DTO coupling.

P3.2 `PASS_ADAPTERS`: only after gate approval; producer-verified or honestly source-blocked mappings, authenticated Shopping/Quote HTTP, bound tokens/TTL/revalidation, negative security and version tests; no Order mutation.

P3.3 `PASS_RESERVATION`: only after gate approval; actual allocation/provider outcome and per-unit consistency under SQL concurrency, safe retry/read/compensation, no double-flight-seat hold, cancellation separate from refund and issuance. If provider/resource capability absent => `BLOCKED_EXTERNAL_REFERENCE`, NOT `PASS_RESERVATION`.

Every variant row includes both initial `Shop` candidate evaluation and `EvaluateSelection` typed input. An incomplete mandatory input returns `NeedsSelection`, not fictional immediate confirmed/held stock.

## 2. Exhaustive profile/scenario tests (P3.1)

| Test ID | Variant / test story | Assert |
|---|---|---|
| E-A01 | One traveller checked bag; 1 free piece and distinct first/second paid piece tier provisions | correct ordinal/portion, one rate, cannot sell duplicate included free allowance, min qty positive |
| E-A02 | Traveller buys two 10-Kg packages; `PricingUnit=PerItem` | price=2 x package; quantity limit uses kg entitlement only when configured, never 20 copies of item price |
| E-A03 | Verified baggage 27 kg inside `(23,32]`; outside bracket | typed form + bracket match and reject outside; no implicit combined surcharge |
| E-A04 | Oversize with dimensions and optional A03 combinability | dimensions required, commercial combination policy respected, approval can be pending |
| E-A05 | Cabin allowance known vs unknown; additional cabin bag | no double charge included cabin bag; unknown => insufficient evidence |
| E-A06 | Sports equipment code, size/weight and supplier | typed selection; no fake physical equipment capacity, supplier delegated status |
| E-A07 | Standard Seat at selected flight and traveller | SeatMapSelection; price or Free as authored; no confirmed seat assignment from metadata |
| E-A08 | Preferred exit-row with child traveller | eligibility/required safety terms and verified aircraft seat characteristic; child blocked as policy demands |
| E-A09 | EXST additional occupied seats | never treat as ordinary preferred seat; extra seat count + ticket/exchange contract required, reservation capability blocked if unproven |
| E-A10 | Cabin upgrade external quote | original/target cabin eligibility; amount=null, QuoteRequired and TicketOrExchange metadata as authored |
| E-A11 | Free WCHR? no, this is free special meal SSR | Free no filed pricing; correct PTC, catering cutoff and meal exclusivity, potential pending confirmation |
| E-A12 | Paid preorder meal/menu | required menu item, correct per-item rate, excludes incompatible menu/time selections |
| E-A13 | Cabin cat/dog/weight/dimensions across outbound+return | evaluated for both flights, subject to supplier approval, never accept quota based on one flight alone |
| E-A14 | Hold pet size/weight bracket + required docs | correct variant/supplier + named size bracket, no phantom quota |
| E-A15 | Wheelchair WCHR/WCHS/WCHC | Free SSR, right assistance code, no EMD paid line, confirmation separate from eligibility |
| E-A16 | BLND/DEAF/DPNA | typed code and communication restrictions, no arbitrary medical data requirement |
| E-A17 | MEDA/AOXY/STCR | evidence refs/oxygen amount as applicable, medical/supplier Pending, no real medical info retained in logs |
| E-A18 | Bassinet and associated adult | infant+guardian+flight, real availability unknown without bassinet source, no miscount as regular seat |
| E-A19 | UMNR traveller age + handoff/pickup schema + connection rules | multi-flight coverage and lead time, guardian form requirements, supplier may reject/pending |
| E-A20 | Lounge venue/terminal/time/guest count | ServiceStart date basis; unknown facility time zone fails closed, no imaginary room capacity |
| E-A21 | FastTrack local appointment and DST ambiguity | location/slot schema; DST gap/duplicate instant handled with authoritative zone, unknown source blocked |
| E-A22 | CIP package includes Lounge element | one package charge (no automatic double charge constituent products), supplier/slot proof required |
| E-A23 | Priority paid vs included in fare | included benefit not billed twice; verified duplicate entitlement per flight if available |
| E-A24 | On-board WiFi plan and aircraft capability | OnBoard context separate from P3 `Both`, source verified; unknown supplier availability not guaranteed |

## 3. Ten Provision rule groups - must be individually exercised

| ID | Rule | Test cases |
|---|---|---|
| RULE-01 | PassengerEligibility | 2 PTCs, boundary ages, leap-year birthday, missing DOB when age band used, child exit row |
| RULE-02 | SalesRestrictions | POS A vs POS B, customer-specific vs customer type, exact from/to UTC boundary, no leaking unpublished provision |
| RULE-03 | Geography | origin/destination/via, route pair direction, coverage country, missing verified country, airport ID overflow |
| RULE-04 | FlightApplication | marketing vs operating carrier, flight number vs ID, aircraft match/mismatch, missing aircraft |
| RULE-05 | FareApplication | FareId, FareBasis, FareType, numeric FareFamilyId, cabin, RBD; one PTC different coupon fare; no string->id guess |
| RULE-06 | TravelDate | allowed inclusive start/end, blackout precedence, multi-flight differing travel dates |
| RULE-07 | DayTimeApplication | weekday masks, start-inclusive/end-exclusive, overlapping Deny/Allow, DST mismatch/source unavailable |
| RULE-08 | AdvancePurchase | minimum/maximum exact boundary with various real TimeUnit, SameTimeAsTicketed without actual ticket timestamp |
| RULE-09 | BaggageApplication | free pieces and excess ordinal; Piece vs Weight, purchase vs travel application, overlapping surcharge prevention |
| RULE-10 | SeatApplication | selected seat number and characteristics, absent seat map evidence, unavailable group, non-seat profile forbidden |

## 4. Core engine price, inventory and contract tests

| ID | Assert |
|---|---|
| PRICE-01 | EUR base 100 + added tax 9 + included tax 5 -> unit total 109; included 5 reported and NOT added again |
| PRICE-02 | 7 fee `PerTicket` appears unapplied; pricing calculation never multiplies by two travellers/three flight legs |
| PRICE-03 | FeeApplicationUnit enum 6..10 NOT implemented -> `Incomplete/Blocked`, not Item |
| PRICE-04 | EUR 2dp, JPY 0dp, KWD 3dp determined by authorized reference; unknown source or wrong decimals fails closed |
| PRICE-05 | Missing active Pricing for Paid+Filed not sold; Free no fake priced rate; Paid+ExternalQuote null amount/QuoteRequired |
| PRICE-06 | Ambiguous same-currency same-PTC/age price rates => reject, not take first |
| PRICE-07 | Correct refund/penalty metadata not calculated by shopping if source missing; document Routing matches Definition only |
| INVENTORY-01 | No Policy => NotConfigured; Unlimited => no local finite pool, not `Guaranteed` |
| INVENTORY-02 | Supplier/FlightFlow => DelegatedCheckRequired, no local seat bucket |
| INVENTORY-03 | Local ConfiguredNotGuaranteed vs ClosedForSale; missing resource => SourceUnavailable |
| INVENTORY-04 | 10kg package count for entitlement with Kg family vs purchased units; missing ledger -> remaining Unknown |
| INVENTORY-05 | One shared physical ResourceId across commercial variants, no independent remaining stock inferred |
| ENGINE-01 | Two-flight one Portion => one portion-priced service, and per-flight Sector candidate only when authored |
| ENGINE-02 | Repeated same canonical context yields same stable deterministic candidate content and ordering |
| ENGINE-03 | Definition Active but Provision Suspended => non-sellable; Active Pricing of unrelated version cannot be used |
| ENGINE-04 | Same key with different buyer Office/Customer cannot access same commercial terms; filter can't override POS scope |
| ENGINE-05 | `SelectionContract` fields/requiredness come from existing factory for all 24 variants; no foreign-profile fields |
| ENGINE-06 | Incomplete age/fare/baggage/DST data => insufficient evidence, never unconditional eligible |
| ENGINE-07 | Unauthorized internal `CandidateIdentity` is not a purchase token and cannot be used for booking |
| ENGINE-08 | No reservation/provider mutation, query-only DB assertions, no P3.1 HTTP routes |

## 5. P3.2 adapter and token tests (deferred)

`ADP-01..12`: source-proven mapping; BoundOfferId-vs-BoundId; per-coupon AirFareId; FareFamily string-vs-ID; long-to-int checked; PTC+DOB provenance; tickets/Offer expiration differences; post-create pre-ticketed stage blocked; POS/office forged scope; quote expired; token replay/tamper and sales currency manipulation; alternate SourceKind equivalence; no token secrets in logs; sanitized authentic AirAvail fixture or explicit `PRODUCER_NOT_VERIFIED`.

## 6. P3.3 reservation integration tests (deferred)

`RES-01` 100-way cap=1 -> exactly one valid local Hold; `RES-02` count+weight atomic rollback; `RES-03` same physical resource two commercial products; `RES-04` airport overlaps/slot DST; `RES-05` same idempotency key same digest returns same unit refs; `RES-06` same key different digest fails; `RES-07` supplier response lost-after-commit -> Unknown, recovered without duplicate allocation; `RES-08` supplier mixed Pending/Confirmed/Rejected per-unit; `RES-09` expired Hold vs Confirm concurrent under lock; `RES-10` Cancel confirmed subset and replay exactly once; `RES-11` multi-flight atomic unit/no half service; `RES-12` FlightFlow seat double-hold refused; `RES-13` A09 EXST blocked until multi-seat and document contract; `RES-14` A10 Upgrade blocked without quote/Exchange capability; `RES-15` no EMD/ticket issued by Ancillary; `RES-16` unknown provider outcome not auto-replayed with a NEW idempotency key; `RES-17` cross-office/airline/Order unit authorization denied; `RES-18` no false refund during capacity cancellation.

## 7. Evidence-report format (mandatory)

For every tested row emit:

```
Test ID | Scenario | Source evidence | Actual test method/file | Execution command
Expected result | Actual result | PASS/FAIL/BLOCKED_EXTERNAL_REFERENCE/NOT_RUN
```

Include `dotnet test ...` exact command and failing test names without invented totals; SQL integration tests distinguished from in-memory Domain tests. HTTP API smoke in P3.2/P3.3 only after authorized. A fixture that pretends missing provider references exist proves DOMAIN behavior **only**, not external integration or production readiness. `PASS_DOMAIN` and `BLOCKED_EXTERNAL_REFERENCE` are distinct.
