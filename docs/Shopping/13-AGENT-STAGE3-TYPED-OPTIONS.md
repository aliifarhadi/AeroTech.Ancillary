# Agent Stage 3 — Typed Options ONLY + one temporary Snapshot table

**Pack Authority (must be locally present):** `00-START-HERE.md`, `01-OVERVIEW-AND-BUSINESS-CONTRACT-FA.md`, `02-EXACT-API-DTO-AND-FIELD-CONTRACT.md`, `03-A01-A24-VARIANT-DICTIONARY.md`, `04-SIGNED-IDS-TYPED-SNAPSHOT-AND-SECURITY.md`, `05-ORDERING-HANDOFF-AND-TESTS.md`, `06-BENCHMARK-SOURCE-DECISIONS.md`, `07-RELEASE-READINESS-AND-UNRESOLVED.md`. **If any file is absent or `python3 VERIFY-PACK.py` fails, STOP before modifying code. Do not consult earlier Pack versions.**


**Prerequisite:** Owner approves Stage2. Keep requests specialized to Variant/Family; no generic business Form DSL.

## Tasks

1. Introduce GET/POST `/{Surface}/v1/AncillaryOffers/{Family}/{reference}/Options` on 3 Surfaces, **only for Typed**. Use `r_` HMAC auth and expiry from Stage1; no OfferId/POS/RateId in POST Body.
2. `GET` reads actual options catalog for Variant: fields `{control,required,choices?}` + real constraints. No fabricated supplier choices, SSR, medical min/max.
3. `POST` variant-specific DTO from Dictionary03, validation from `04`: verify MAC/purpose/expiry/salesBinding/current active rate and chosen fields; no business revalidation in Resolve. Required unknown proof ⇒ no snapshot, fail closed; quote/approval ⇒ gated.
4. Create **one** `SelectionSnapshot` table exactly 04, smallest EF migration, no new Aggregate or token inventory table. ID `s_` random 128-bit, full typed selection in allowlisted selectedJson with no unnecessary PII. `expiresAt` and rate+unit+bindings persist. No physical TTL purge job in this stage.
5. Add Owner-approved D2 fields (`oxygenFlowRateLitersPerMinute:decimal?`, `frequentTravellerMedicalCardNumber:string?`) to the appropriate **existing A17 Domain/DTO/authoring contracts** with source-grounded semantics and no invented clinical limits; include necessary targeted Domain migration if persistence requires it. A17 with FREMEC must remain disabled until platform-approved encrypted at-rest storage; no homegrown encryption or logging. A18 gated if trusted infant/guardian refs cannot be read/verified, A19 gated on source mismatch, A09 FlightFlow evidence missing, Insurance nonexistent; no hidden acceptance.
6. POST response exactly `{selectionId,expiresAtUtc}`; no accidental selectedJson medical echo. Retry POST may create new s_ ID, no redundant fingerprint uniqueness layer; GET/Resolve later read same existing ID deterministically.

## Tests

- O01–O12, invalid signature, expired, wrong Actor, wrong domain SSR; failed POST creates 0 rows. Typed+Qty stores count; Qty-only produces 0 rows. Check DB migration and rollback; no SeatMap changes.

**Report:** `reports/V8-STAGE3-TYPED-SNAPSHOT.md`, no Commit/Push.
