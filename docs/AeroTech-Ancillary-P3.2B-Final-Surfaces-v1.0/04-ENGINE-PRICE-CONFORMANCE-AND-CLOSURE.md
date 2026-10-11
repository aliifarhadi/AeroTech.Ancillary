# 04 — Engine-First Audit, Commercial Pricing and P3.2B Closure Gates

## Mandatory execution order

**Gate A: Engine facts and pricing first, before writing three mappers.** Run and independently document P3.1 R1 regression/24 variant tests. Verify traceability from published Domain catalog -> matching provision sequence -> pricing rate -> price components -> status. Fix **only proved business blockers** in engine, with explicit source and expected outcome; no mass rewrite. The following matrix must pass as executable fixtures:

1. Eligibility: PTC, age (unknown does not silently pass), fare basis/id/type, fare family ID unknown, airport/country, marketing vs operating airline, cabin/RBD, travel dates, timezones, day/time and advance purchase boundaries, multiple connections, currency, and all 10 shared rule groups.
2. Provision: deterministic `Sequence`/first matching rule, NotAvailable vs Free vs Filed vs ExternalQuote, selection changes provisions, NotEligible/Unavailable/NeedsSelection/NeedsQuote/NeedsVerification/Selectable statuses. No rule should be skipped because of surface DTO.
3. Inventory and supplier: `NotConfigured`, Unlimited, Supplier, FlightFlow, local count/weight/slot, cumulative usage, no invented remaining stock or guaranteed availability. If `MustCheckAvailability=true` and no trusted check, never promise stock. If policy genuinely says Unlimited, honor it without inventing a count.
4. Price selection: CurrencyId + PTC/age selectors select exactly one published rate; absence/ambiguity explicit. Preserve `BaseAmount`, AddedTaxes, IncludedTaxes, Item Fees, non-Item Fees, decimal scale; `UnitTotal=base+added tax+item fees`, with included tax reported only. Free and ExternalQuote must not fabricate money. Never mix flight fare and ancillary amount.
5. Per-piece/package/kg: A01 quantity x per-piece; A02 quantity x package-price (do NOT multiply by kg in package); A03 `PerKilogram` requires verified billable weight semantics and no double charge; A04 charge-combination must prevent double collecting oversize/overweight where configured. If actual billable delta cannot be derived, do not return a false total. Fees per Ticket/OneWay/etc need correctly scoped quote, not per-item multiplication.
6. Missing facts: AirAvail does not provide DOB, fare family numeric ID, IANA time zone or seat map; fail closed **only for rules dependent on the absent facts**, not globally; `salesDate` of request (commercial) versus current verification instant are distinguished without changing AirAvail contract semantics.
7. Authorized POS-set parity: single named POS, zero eligible, multiple eligible, price difference between POSs, same definition with different pricing, no guessed defaults and no unsafe dedup. Cross-surface canonical decisions identical when commercial contexts are equal.

## Gate B: shared orchestration & three surface contracts

- One fetch of `Service/v1/FlightOffers/Details` per inbound request (unless fresh source data requires a documented re-fetch); one trusted offer model; validate nonempty/join consistency as existing P3.2A.
- Internal canonical result returned independently of Backoffice, Api, OtaPanel assemblies. No `AncillaryOffersDto` in shared orchestration return signature. `CanonicalAncillaryOfferResult` and P3.1 engine remain intact except proven engine defects.
- Three typed request + three typed response classes and three mappers; DI and routes/policies independently pinned. Exact parity of Backoffice options; API & Panel only `{offerId}`. Caller extracts agency claim + X-Country as AirAvail, authorized POS **set**.
- API normalized `serviceDefinitions` + `offerItems` + pax/segment associations, price availability and status; no internal IDs. BO full diagnostics; Panel readable grouped output. Golden JSON snapshots and OpenAPI schemas generated and compared; enums and numeric precision checked.
- All surface DTO transformations are deterministic and side-effect-free; test metamorphic equivalence on the same canonical result (same amount, currency, allowed scope, reason and status).

## Gate C: Details & selection

- Baggage Details for A01-A06 source-backed: allowance, dimensions, weight/piece, tier ordinal where applicable, fees/tax, time/carrier/fare restrictions, required selection fields, fulfilled document metadata only if present.
- Seat Details A07-A09 source-backed metadata; physical seat map actual if provider exists, else explicit unavailable. Test forged `offerItemRef` and cross-surface reuse. `EvaluateSelectionAsync` called for any price-changing selection; no customer verified seat/age/availability.
- Search, Details, Evaluate on all three surfaces use same canonical engine/selected provision. Tests for changed fare, ambiguous pricing, stale source, stale provision/price revision, unavailable inventory and quantity out of bound.

## Gate D: real execution evidence, not green mocks alone

1. **Same live priced OfferId** from staging AirAvail; each of Backoffice / Api / OtaPanel with valid credentials/claims; authenticated request, real producer HTTP, live published Ancillary DB, one actual successful candidate and at least one missing-fact candidate. If staging lacks a specific surface token or POS eligibility service, explicitly `BLOCKED_EXTERNAL` for that row; do not report full close.
2. Real 1 ADT+1 CHD flight with 2 bounds, multi-sector and coupon fare IDs. Assert no cross-traveller/fare leakage.
3. Multi-POS integration fixture using a real POS eligibility implementation/service or verified dataset, not stub-only; explicit POS vs implicit set matches AirAvail behavior. Check catalog POS IDs actually belong to point-of-sale ID namespace.
4. Verified price fixture: base 5,000,000 + added VAT 450,000 = 5,450,000, included tax excluded from addition; Wi-Fi base 900,000 + added VAT 81,000 = 981,000, plus Ticket fee 100,000 *once per ticket* only in an order-level quote, never multiplied per service. Negative tests for missing FX, age, kg basis and currency scale.
5. Test real Details/Selection for A01 extra bag and A07 seat metadata; genuine SeatMap if connection exists, otherwise typed `SEAT_MAP_SOURCE_UNAVAILABLE` and no claim of seat-map closure.
6. Verify no writes to Reservation/Order/EMD or migrations, no route breaking changes in Backoffice, security policies do not cross, verify doc samples match compiled serializer and OpenAPI.
7. Include evidence: branch/head, exact commands/build counts, test IDs, golden JSON files for **three surfaces**, raw HTTP status (redacted), actual-source vs fixture flag, unresolved external references/limitations. No copied fake commercial values represented as live.

## Closure labels

- `P3_2B_ENGINE_GATED`: Gate A independently tested; no false amounts or availability.
- `P3_2B_THREE_SURFACES_GATED`: Gates A+B, distinct schemas and verified parity; source access for agency resolver verified.
- `P3_2B_DETAILS_GATED`: Gates A–C, baggage details source-backed and selection verification functional, seat-map source truthfully marked.
- **`P3_2B_CLOSED` ONLY if Gates A–D pass** for the promised scope. Otherwise state `P3_2B_PARTIAL_EXTERNAL_BLOCKED` with exact missing dependency. No stock or physical booking promised by this Phase 3.2 closure; those are Phase 3.3.

### Strict change-control boundaries

No new seller policy in Ancillary inconsistent with AirAvail. No financial rounding invention. No generic reflection/dynamic JSON rules. No 3 implementations of eligibility or pricing. Do not create Domain aggregates for presentation concepts. Do not change AirAvail, Ordering, FlightFlow or AirPrice repos without explicit Owner approval. Report cross-service defects and exactly which common contract should change, separately. Minimize migrations and follow current repo conventions. Preserve Phase 1/2, P3.1 and reservation acceptance tests.
