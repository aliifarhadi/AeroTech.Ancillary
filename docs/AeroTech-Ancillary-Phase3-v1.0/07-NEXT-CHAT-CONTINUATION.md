# Continuation prompt for next chat - Ancillary Phase3

The owner approved **the decomposition** of AeroTech Ancillary Phase3 into:
1. Phase3.1 independent canonical Ancillary Shopping Engine receiving its own typed canonical context, matching existing active Definition/Provision/Pricing/InventoryPolicy with full eligibility/price/schema/availability truth, returning canonical offer candidates.
2. Phase3.2 adapters: AirAvail FlightOffer Details, authorized Ordering Order snapshot and verified direct input into exactly that Context, shopping/selection/quote HTTP and bound opaque ServiceOfferId issuance.
3. Phase3.3 real Ancillary reservation Hold/Confirm/Release/Expire/Cancel/Recover with genuine capacity or supplier evidence, no double FlightFlow hold and no EMD issuance by Ancillary.

The pack `AeroTech-Ancillary-Phase3-v1.0` is authoritative for NEW Phase3 boundaries, under the v12.2 approved Phase1/2 contracts. Agent starts ONLY P3.1 and stops for owner audit; P3.2/P3.3 are forward design until separately authorized. Do not reopen P1/P2 or implement fake inventory/hold. No other repository may be modified.

Verified baseline 2026-10-10:
- Ancillary `feat/ancillary-v12.2-phase1-phase2-rebuild@bee6ad67237be4f576e425686ce501f23c277952`.
- Ordering `k8s-stg@cd50a2a5fd18a372f32f2d4efac08dee0510f6ba`, FlightFlow `k8s-stg@d2180b2e4c07789abde75e712e870ece8d2c776b` (read only).
- AirAvail GitHub inaccessible (404), no live authenticated FlightOffers/Details response verified. Ordering's real OfferProvider POST route and wire DTO were inspected. Do not claim full producer contract verification.
- P2: `PHASE2_AUTHORING_CLOSED=yes`, `PHASE2_FULLY_CLOSED=no`; unresolved Inventory Resource, Airport Facility, Counting Family, FlightFlow delegation authority. No manufactured reference records.
- Actual `AncillaryReservation.Hold` still stores Held without local/provider allocation. FlightFlow Hold genuinely consumes flight capacity; Ordering already groups Air + Seat services per passenger/flight, thus Ancillary must not double reserve.
- Lufthansa fixtures: 22 display categories / 48 codes, by-order 114 services, six purchase order service examples; sample statuses are not authoritative AeroTech stock.

Authority docs: `docs/AeroTech-Ancillary-v12.2/` 00..17, especially 01,02,03,04,05,06,07,09,17; this pack 00..08. Agent prompt is `06-CODING-AGENT-EXECUTION-PROMPT.md`.

On next agent result, audit actual code vs contracts field by field, all 24 variants, all ten rules, status/price/tax/currency truth, no cross-scope changes, tests and actual source evidence. Only owner can authorize next phase.
