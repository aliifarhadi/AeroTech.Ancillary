# Agent Stage 1 — Minimal signed identity + real zero Rate

**Pack Authority (must be locally present):** `00-START-HERE.md`, `01-OVERVIEW-AND-BUSINESS-CONTRACT-FA.md`, `02-EXACT-API-DTO-AND-FIELD-CONTRACT.md`, `03-A01-A24-VARIANT-DICTIONARY.md`, `04-SIGNED-IDS-TYPED-SNAPSHOT-AND-SECURITY.md`, `05-ORDERING-HANDOFF-AND-TESTS.md`, `06-BENCHMARK-SOURCE-DECISIONS.md`, `07-RELEASE-READINESS-AND-UNRESOLVED.md`. **If any file is absent or `python3 VERIFY-PACK.py` fails, STOP before modifying code. Do not consult earlier Pack versions.**


**Prerequisite:** Owner accepts Gate0; no code otherwise. Work only on the new approved branch, not deleted grouped branch.

## Tasks

1. Implement **one** codec (not a token framework) for `r_` / `d_` as in `04` with `HMAC-SHA256`, purpose-separated domain string, canonical payload and expiry. Key from secret config; no hardcoded key / POS/Offer trust from client. Test encode/decode/constant-time verify/tampered qty, rate, pax, portion, exp and purpose. Limit payload/token length and reject invalid UTF8/duplicate fields.
2. Implement issuance gate reusable inside current Shopping path: only engine candidate already known eligible/complete/published, actual active Rate for Passenger/Unit/SalesScope and trusted Source ValidUntil. No extra engine or Order connector. Return no ID when unknown required evidence.
3. Fix **Authoring/publication of zero-cost Ancillary** using Owner-approved **Paid/Filed + real AncillaryPricing/AncillaryPricingRate(0)** model, and audit its real commercial disposition/document side effects; do not infer "free" automatically means no EMD or booking. Do not fake `PricingRateId` from Provision and do not add Free branch to Resolve or modify frozen SeatMap. Migration only when actual DB authoring change requires and documented.
4. Add smallest DB RateId lookup as needed through existing context/reader. Do not duplicate aggregate repositories.
5. `offerBinding` and `salesBinding` definitions must match `04` exactly; no raw external POS/Country parameters in Resolve, no sale authority derived from client.

## Gates / tests

- Existing build/test suites + SeatMap byte-for-byte frozen file audit.
- HMAC negative tests: forged CHD/ADT, cross POS, Pricing parent Retired/Suspended (no issue and no accepted Resolve), invalid auth, expiry, quantity tamper, purpose confusion.
- Free RateId>0 and price=0 supported by actual SQL fixture.
- Tests of 4 supported Unit scopes, Portion≠Flight.
- No Snapshot table in this stage, no endpoint changes, no Commit/Push.

**Report:** `reports/V8-STAGE1-SIGNED-IDENTITY.md` with diff and test commands, error codes, precise code paths; Owner review before Stage 2.
