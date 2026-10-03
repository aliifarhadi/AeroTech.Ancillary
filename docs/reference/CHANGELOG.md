# Changelog of the reference documents

Newest first. One entry per decision; details are in the documents themselves.

## R6 — 2026-10-03

- **AirOffer reads Ancillary's published read model directly** (SQL, cached snapshot, stamp probe) and evaluates it; no HTTP call to Ancillary. Master §2.2, §15. Reason: that is how AirOffer already reads FlightFlow and AirPricing.
- **Every ancillary service is reserved and confirmed at Ancillary** through one interface; Reserve re-checks the selection with Ancillary's evaluator. Master §2.3, §8 (`ServiceReservation`; stock pools now build on it in P4). Reason: Ordering's existing provider interface; ONE Order's delivery-provider model; Ancillary, not AirOffer, is the last check.
- **Seller-facing shape follows NDC:** service definitions, a-la-carte offer items with opaque ids and an expiry, selection by offer-item id. `Ancillary-Edge-Contract.md` (new).
- Benchmark facts on NDC, ONE Order and the platform added to Master §13; published read model §15; marketplace horizon §16.
- Documentation reorganised: `reference/`, `phases/<phase>/`, `handover/`.

## R5.4 — 2026-10-03

- Combination check (`16105`) runs before every other product rule. Occurrence identity: flight-scoped = product + traveller + flight. Phase-2 document and proof.

## R5.3 — 2026-10-02

- Extra Baggage sold and documented only when it covers exactly one flight (EMD-A has one coupon). No convention for splitting an amount over coupons. Every phase closes with conformance tests.

## R5.2 — 2026-10-02

- EMD coupon value = the `Ancillary` lines; taxes at document level. EMD-A passenger identity from the associated ticket. RFIC per industry code is an AeroTech constraint taken from ATPCO data. Later phases' industry codes recorded (`0BX`, `0BT`, `0B5`).

## R5.1 — 2026-10-02

- Industry sub codes come from a read-only reference (`0CC`: BG, B1, FIRST EXCESS BAG, RFIC C, exactly 1). Carrier-defined codes local to the airline. Ordering companion spec.

## Earlier — 2026-10-01 / 2026-10-02

- R1–R4.1: ownership (catalogue, pricing, stock in Ancillary; offers in AirOffer; orders and EMDs in Ordering; seats in FlightFlow), three product axes, stateless quote, `productVersion + priceRuleId` stale-selection guard, traveller flight coverage, sub-code register.
