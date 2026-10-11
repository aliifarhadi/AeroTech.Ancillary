# Phase 3.2.A — Backoffice `/Backoffice/v1/AncillaryOffers/` from AirAvail OfferId

**Authority:** This pack implements only one read-only Backoffice flight-offer-to-ancillary-shopping endpoint; source-agnostic Engine P3.1 is reused without a new rules system. No Reservation/Ordering mutations or future channel APIs.

1. `00-AUTHORITY-SOURCE-AUDIT.md`: pinned producer+consumer source verified and immutable scope.
2. `01-FLOW-CONTEXT-MAPPING-AND-BOUNDARIES.md`: full flow and field-by-field mapping, POS/tenant boundary.
3. `02-BACKOFFICE-ENDPOINT-AND-RESPONSE-CONTRACT.md`: API request/output and explicit statuses with illustrative JSON.
4. `03-BENCHMARK-TESTS-CLOSURE-GATES.md`: flydubai/Sabre/Amadeus/Lufthansa/AEGEAN benchmarks; integration tests, strict closure.
5. `04-CODING-AGENT-PROMPT.md`: complete directly executable instructions for coding agent.

**Most serious architectural blocker:** AirAvail OfferId does not contain the internal PointOfSaleId; Ancillary current Backoffice claims expose AirlineOfficeId, not POS. A verified POS resolution source is required for pricing/eligibility. If it does not exist, this endpoint MUST fail closed, not price in a guessed context.
