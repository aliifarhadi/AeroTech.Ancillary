# Agent Stage 4 — ResolveGrouped IDs-only + specialized output

**Pack Authority (must be locally present):** `00-START-HERE.md`, `01-OVERVIEW-AND-BUSINESS-CONTRACT-FA.md`, `02-EXACT-API-DTO-AND-FIELD-CONTRACT.md`, `03-A01-A24-VARIANT-DICTIONARY.md`, `04-SIGNED-IDS-TYPED-SNAPSHOT-AND-SECURITY.md`, `05-ORDERING-HANDOFF-AND-TESTS.md`, `06-BENCHMARK-SOURCE-DECISIONS.md`, `07-RELEASE-READINESS-AND-UNRESOLVED.md`. **If any file is absent or `python3 VERIFY-PACK.py` fails, STOP before modifying code. Do not consult earlier Pack versions.**


**Prerequisite:** Owner approves Stage3. No Ordering HTTP dependency.

## Tasks

1. `POST /Service/v1/Ancillaries/Selections/ResolveGrouped` exact JSON body `{ids:string[]}` 1..50 distinct; reject other fields. Use Service Auth existing policy.
2. Dispatch `d_` to signed codec (MAC/purpose/exp/Unit/Quantity), `s_` to DB snapshot (expiry/Unit/selected). Do not accept `r_`. Read local **active** Rate→Pricing→Provision→Definition; no external call, no refund/hold or reservation code.
3. Return exactly family arrays and field schemas from `02`, mapped by actual Definition family/variant; no general DTO of nullable options. `pricingRateId` valid for Free 0. Qty price computed from signed count, Typed from snapshot; no client-supplied price.
4. Include `sourceBinding.offerHash` / `salesHash` for Ordering's cross-Offer/actor validation. Do not require OfferId/OrderId/POS/Country in Resolve. Do not invent `flightRefs[]` for Portion using stale/absent data.
5. Reject invalid MAC/expired/**parent Pricing Retired or Suspended**/inactive Provision or Definition, mismatched declared scope, snapshot without matching Rate/Definition; no partial batch success. Repeated Resolve never consumes snapshot or writes DB.
6. Produce an **Ordering handoff contract fixture**, not speculative integration code. Coverage/identity/dedup from `05` must be explicit, so Ordering agent can adopt later under separate Owner gate.

## Tests

- R01-R12 + T01-T16, happy mixed d_+s_ and retry, A15 selected, A01 Qty2, A11 zero, Portion, malformed JSON, 51 IDs. Check SQL read counts and no new cross-service clients. Frozen SeatMap tests.

**Report:** `reports/V8-STAGE4-RESOLVE.md`. No Commit/Push.
