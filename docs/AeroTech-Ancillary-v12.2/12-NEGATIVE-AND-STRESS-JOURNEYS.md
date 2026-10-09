# 12 — Eight cross-domain scenario stress journeys (realistic PSS fidelity)

**P1/P2 executes commercial setup, validation, backoffice read and capacity authoring. P3 steps below are architectural acceptance criteria only.**

## J1 — LH-style asymmetric trip: FRA→DXB outbound vs DXB→FRA inbound
Fare includes different checked-bag allowance in each direction. Shopper sees IBAG extra piece in one leg/Bound and heavy additional KBAG another. Backoffice setups: A01/A03/A04 variants, separate Provision per POS and route/Fare; `0–6` UI stepper; rate EUR, fee/tax semantics, no invented `FlightCount`. P1/P2 asserts correct typed configs and no assumed symmetry. P3 later asserts `serviceId` bound to exact traveller+portion, 6 allowed if eligibility permits, seventh refused, zero no-op, duplicate order cap enforced.

## J2 — One traveller 5+10kg baggage package
Airline config A02 5kg/10kg products, cap 20kg per person/portion, two separate rates; purchase one of each uses 15kg commercial allowance, **two products**, no `FlightWeightInventory` unless externally validated actual quota. P1/P2 validates proper rate per package and cumulative scope/unit semantics. P3 later computes consumption across accepted orders and rejects >20kg.

## J3 — PETC outbound blocked, return scarce and pending
Definition A13 Cat/Dog combined max+dimensions, min age and document checklists. Outbound Provider Unavailable; return real authoritative quota 2 and one remaining, with actual provider confirmation pending. P1/P2 policy/supplier reference and limits correct; cannot fabricate availability. P3 later prevents simultaneous oversell, preserves Pending vs Confirmed and retains typed selection.

## J4 — WCHC and UMNR on two-segment connection
Free WCHC may be guaranteed on ST4 but pending on ST3; paid UMNR may be bound across ST1+ST2, with guardian fields and quota by bound/flight. P1/P2 Authoring supports both with distinct subtypes, no price on wheelchair and correct UMNR rate per traveller/portion; no 2x charge for 2 segments. P3 later validates exact traveller, handoff, connection rules and provider status.

## J5 — Seat standard/preferred, EXST/CBBG, aircraft swap
Author Seat profile Specs with exit-row constraints and EXST required adjacent seats. P1/P2 stores commercial characteristics and delegates to FlightFlow. P3 later FlightFlow seatmap must hold actual one or multiple seats; aircraft swap invalidates previous offered seat and triggers reprice/reaccommodation; no local seat pool.

## J6 — CIP combined Lounge/FastTrack at airport facility
Airline config airport/terminal/timezone/facility and shared slot source for A20–A22; package includes lounge+fasttrack with no double customer fee. P1/P2 source facility validity, timezone, overlap locks and slot config. P3 later reserves same capacity once per people count and releases on failure, with DST instant ambiguity refusal.

## J7 — CO2/simple opt-in thin POST from Lufthansa pattern
Simple offered service in future (not A01–A24 mandatory, but API stress) selected via `{serviceId,travelerId:'all',quantity:1}`; offer bound to current order travellers/POS/TTL and legal quantity. P1/P2 maintains generic future selection metadata contract with SimpleOptIn without adding CO2 aggregate. P3 later expands all travellers, idempotency + If-Match, no lastName-only authentication, no implied EMD.

## J8 — Provider quote expired / payment retry / refund and order history
A10 Upgrade requires source quote; A17 Medical approval; Pricing Authoring must reject bogus 0 price or fee multiplied per item. P3: expired quote reprice, verified FlightFlow quota, OrderChange with accepted snapshot, payment retry idempotent, ticket exchange OR EMD per document policy, release on decline, no duplicate issues, immutable order history. P1/P2 proves metadata can state which path and that capacity provider is sourced, **not** an issued document.

## Required negative tests across these journeys
- Underage exit row, underage/invalid UMNR guardian requirements, inconsistent SsrCode, unsupported variant/spec, service wrong POS, wrong cabin/fare, expired purchase cutoff, over-max quantity, missing required reference.
- Real impossible promise: SourceUnavailable != Guaranteed, Local no registered source != configured, `Unknown`/`Pending` != available, no remaining calculated before P3.
- Component tax included double addition, per Ticket Fee multiplied by traveller count, GBP/KWD wrongly accepted with 0-decimal reference without fail closed.
- 100 concurrent modifications of same Stock source/slot, idempotency adjustment collision, publisher double activation race.
- P3-only scenarios on stale OfferId / changed order version / same idempotency different payload; **document these but do not implement in v12.2**.
