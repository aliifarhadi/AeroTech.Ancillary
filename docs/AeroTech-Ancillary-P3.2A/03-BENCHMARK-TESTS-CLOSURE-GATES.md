# 03 — Real-world benchmark, conformance and strict exit gates

## 1. Benchmark: evidence vs AeroTech design choice
| Benchmark | Observed, authoritative source | Design implication for Backoffice OfferId endpoint | Not evidenced / MUST NOT claim |
|---|---|---|---|
| **flydubai developer API** | `POST /pricing/services` AncillaryOffers returns services for selected flights with price/inventory; separate `POST /pricing/seats` priced seat map. Booking AddToCart and PNR/pay are separate (official dev portal) | return flight-linked per-traveller service options with quantity and price status; seat candidates require a separate verified seat-map/selection step. Price/Availability != booking | AirAvail `FlightOffers/Details` implicitly provides seat-map stock: false |
| **flydubai modify flow** | Retrieve PNR → optional repricing → Ancillary API for bags/meals, Seat API/hold/assign for seats → ModifyPNR → Commit → payment | later Order-based adapter and modification workflow independent of this pre-order Backoffice search | This endpoint changes an Order or holds seats: false |
| **Sabre NDC** | `/offers/getAncillaries` ancillary listing, `/offers/getSeats` seat map; `/orders/change` handles adding ancillary to Order; service prices/fulfilment separated | keep ancillary listing separate from seat detail, reservation and documents. Canonical offer coverage and passenger applicability required | all Sabre GetAncillaries service types are paid or guaranteed: false (docs note free-of-charge format) |
| **Sabre Digital Connect** | `/products/ancillaries` GET/POST and `/products/seats` GET/POST; seat map includes availability, reserved/free/paid statuses | Frontend projection can group offer options; must expose flags and avoid implicit availability for a seat without map | shopping candidate itself reserves a particular seat: false |
| **Amadeus Altéa/NDC** | `ServiceList` for ancillary catalogue, `OfferPrice`/Offer Items and IDs, `OrderCreate`, `OrderChange/Cancel` separated; published integration examples show ServiceList after price before OrderCreate | preserve offer/candidate identity, coverage by traveler/flight/portion, price and document metadata separately; no Booking on search | user's OfferId is an authenticated token or guaranteed immutable price: false |
| **Lufthansa** | Local pinned **three airline example JSON files** (`samples/lufthansa-services-configuration.json`, `samples/lufthansa-services-by-order.json`, `samples/lufthansa-one-booking-v2-purchase-orders.json`) contain category/typed selector schemas, per traveler and flight service entries, quota status, sometimes no price | return `SelectionKind`+typed fields; price null for unknown, quantity min/max; `QuotaStatus` not guaranteed stock; coverage array for multi-flight services. Include docs metadata, but no issued EMD inferred from purchase row | samples prove public PSS internal data/guarantee: false. Not all 114 sample offer entries have price |
| **AEGEAN official booking / additional services** | while booking and under My Booking, users can choose seat, baggage, fast track, special meal, pet, Wi-Fi/upgrade; fare benefits and route matter | output should group by recognizable service profile; show fare-included allowance to avoid selling included benefit again; eligibility per traveler/flight | there is a published AEGEAN third-party JSON service-list wire identical to ours: **no source found** |
| **Lufthansa official booking** | fare-dependent bag and seat additions, changes after booking, services with distinct chargeability and conditions | price/eligibility depends on fare and route; Backoffice display needs FareBasis, fare family text, included baggage and exact terms | a universal 0 cost for missing ancillary price: false |

Official benchmark URLs:
- flydubai developer: https://developer.flydubai.com/browse/api-doc-banner?apirefrence=try_out
- flydubai modify: https://developer1.flydubai.com/browse/api-doc-banner?apirefrence=modify_flow
- Sabre NDC: https://developer.sabre.com/product-collection/new-distribution-capability-ndc/v1/index.html
- Sabre Digital Connect Post-Booking: https://developer.sabre.com/rest-api/digital-connect-post-booking/4.3.81/index.html
- Amadeus Altéa NDC summary: https://amadeus.com/documents/en/travel-industry/sales-sheet/amadeus-altea-ndc.pdf
- Amadeus scalable NDC patterns: https://amadeus.com/documents/en/pdfs/suggestions-for-ndc-adoption-at-scale.pdf
- AEGEAN booking methods: https://en.aegeanair.com/plan/booking-info/booking_methods/
- AEGEAN service catalogue: https://en.aegeanair.com/travel-info/additional-flight-services/
- Lufthansa fare services: https://www.lufthansa.com/iq/en/europa-tarife-economy-class
- Lufthansa booking management: https://www.lufthansa.com/no/en/overview-manage.html

**Evidence classification:** `PRODUCER_SOURCE_VERIFIED` (AirAvail current code), `CONSUMER_SOURCE_VERIFIED` (Ordering), `BENCHMARK_OFFICIAL` (official sites), `SAMPLE_OBSERVED` (Lufthansa fixture), `AEROTECH_DESIGN` (new API projection), `EXTERNAL_BLOCKED` (missing reference). Never write that Lufthansa/Amadeus return precisely our DTO.

## 2. End-to-end and negative test matrix — beyond mocked happy paths
| ID | Full test | Expected |
|---|---|---|
| CT-01 | One **sanitized real** AirAvail priced OfferId and actual producer HTTP response; same request as Ordering | Upstream `FlightOffers/Details` returns expected envelope, non-empty flights/tickets; Backoffice maps without losing required fields |
| CT-02 | Backoffice authentication missing or wrong `authz_surface`/scope | 401/403; HTTP provider NOT contacted |
| CT-03 | OfferId without priced third commercial segment or corrupt | Producer refuses; no 200-empty success |
| CT-04 | Authoritative owner/office/POS match vs mismatch and POS ambiguous | One valid POS; mismatch denied; unresolved typed blocker; never first POS |
| CT-05 | Outbound + return, three flights (two in one bound) | One Portion candidate if provision is Portion; Sector candidates only as configured; no repeated fee |
| CT-06 | ADT/CHD/INF pricing coupon joins, FareComponent by bound/ref | Exactly correct price/rate/eligibility per passenger; no fare/coupon leakage |
| CT-07 | `BoundOfferId` absent/duplicated or PricingUnit points to nonexistent bound | typed source inconsistency; no guessed join |
| CT-08 | Broken Fare line ref, ambiguous Fare components | No arbitrary first fare; candidate requiring exact AirFare returns `NeedsVerification` or safe mapping refusal |
| CT-09 | `FareFamily="LIGHT"` string with numeric rule requiring `FareFamilyId` | Unknown/verification, NOT text-to-ID cast; other unrestricted services can still display |
| CT-10 | Cabin/checked bag allowance `0` with known unit vs no unit | Known zero preserved; missing unit = unknown, not guaranteed free allowance |
| CT-11 | Price including added tax, included tax and ticket-level fee | Correct unit value; incomplete order level total shown; separate flight price from ancillary price |
| CT-12 | Free SSR (WCHR) and dynamic quoted Upgrade | Free origin/selection+no money line; quote amount NULL with NeedsQuote/verification; no fake 0 |
| CT-13 | Seat product without seat-map/source & selected seat | `NeedsSelection`/`NeedsVerification`; `IsGuaranteed=false`; no double FlightFlow hold |
| CT-14 | UTC-origin departure with missing IANA timezone and route/date restriction | `NeedsVerification`, no `DateTimeOffset.Offset` substituted as IANA location |
| CT-15 | Currency referenced incorrectly or reference missing | price incomplete; no hardcoded 2-decimals workaround |
| CT-16 | External Inventory, counting family or usage ledger disconnected | truthful `NeedsVerification` in `blockedOffers`; not `Selectable` when evidence is required |
| CT-17 | Same flight reused as technical leg, multiple FlightIds, first/last AirportId | join deterministic; no duplicate air-service coupon for leg |
| CT-18 | Upstream timeout, 401, 404, 503, valid 200 with `data=null` or errors | typed safe errors, no `200` pretending no ancillary |
| CT-19 | Valid flight details but no catalog products for given owner/POS | 200 valid journey with `offers=[]` and clear no-product diagnostic |
| CT-20 | Published 24 variant fixtures run through same mapper/Engine/projection | All mapped variants have applicable typed selector/output; not all necessarily available on one flight |
| CT-21 | Cross-airline offer and unauthorized office/actor | No tenant leakage; owner matching enforced; errors do not include internal context |
| CT-22 | Controller HTTP body exactly `{offerId}` only; trailing slash | Correct endpoint contract; extra client commercial fields not trusted/used |
| CT-23 | Pricing unit and output fee basis PerPassenger/PerItem/PerPiece/PerKilogram | Correct `Quantity` and `PricingUnit`, no arbitrary multiplication |
| CT-24 | No DB product mutation, no Reservation call, no Order mutation | Read-only verified via spies/integration plus unchanged rowversions |
| CT-25 | Current Ancillary REST host boot + DI + SQL Server active catalog + stubbed AirAvail producer HTTP | Real HTTP 200 and response shape with SQL catalog; not only component fixture |
| CT-26 | **Staging authorized live E2E** against AirAvail with prepared active ancillary catalog | Verified real OfferId → real upstream HTTP → legitimate mapped context → actual Engine → Backoffice 200; record status without sharing token or customer data |

Tests CT-01 and CT-26 are materially different: capturing authoritative fixture proves wire fidelity; live E2E proves runtime connectivity. If staging access/credentials unavailable, explicitly mark CT-26 `NOT_RUN / BLOCKED_RUNTIME_ACCESS`; NEVER write `LIVE_E2E_PASS`.

## 3. Strict completion and stop rule
**Only `BACKOFFICE_OFFER_SHOPPING_CLOSED` if:** 1) source-to-context matrix each field audited and tested, 2) unique POS resolution proven by source/config, 3) real producer wire fixture and at least one validated trip, 4) response has actual price/selection/status matching Engine, 5) HTTP host/auth and SQL integration tests pass, 6) missing external reference facts display blocked honestly, 7) CT-26 live E2E passes or owner formally accepts `DOMAIN_AND_CONTRACT_CLOSED / LIVE_E2E_BLOCKED` as partial delivery. Do not mark full closure without real HTTP exercise.

If POS resolver missing, deliver compilable conditional architecture, tests proving failure closed, and one owner decision request — **NOT a mock global default POS**. If AirAvail returns different producer contract from source, stop and report a real sample (sanitized) and exact difference. If a strict Age/FareFamily/Supplier check lacks evidence, don't add fields to `FlightOffers/Details` in another repo: keep status `NeedsVerification` and demonstrate that other products still shop where possible.

Agent final evidence: exact baseline/current commit, paths changed, API sample redacted, SQL/HTTP test commands/results, 24-variant projection coverage, open dependency register, statuses `PASS_SOURCE|PASS_DOMAIN|PASS_SQL|PASS_HTTP|BLOCKED_EXTERNAL_REF|BLOCKED_RUNTIME|NOT_RUN`; do not claim a test was performed merely because it is documented.
