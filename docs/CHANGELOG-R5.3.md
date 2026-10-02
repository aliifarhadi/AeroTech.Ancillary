# Changelog — R5.2 → R5.3

**Date:** 2026-10-02. The seven closures asked by the R5.2 conformance review; nothing else was reopened.

| # | Closure | Where |
|---|---|---|
| 1 | `BLOCKED_OWNER_DECISION` removed. The "last coupon carries the whole base value" convention is **not** approved and no longer appears anywhere. | Ordering spec (header, §6, §11); End-to-End; `START-HERE.md` |
| 2 | **Phase 1 sells and documents Extra Baggage only where the occurrence covers exactly one flight.** The quote omits a wider occurrence in catalogue mode and answers `16305` to a selection of it; the product's scope stays `TravellerBound`. Ordering's EMD-A therefore has exactly one coupon in Phase 1 and checks it. | Master §3.4, §7.2, §7.7; Phase 1 §3, Q04, Q05, Q10, Q11, Q15; proof request 17; OpenAPI `coveredFlightRefs`; Ordering spec header, §1, §3.2, §5.2 step 8, §6.2–§6.5, §10 (A06, A06a, E03) ; End-to-End §1, §4, §5 |
| 3 | Kept: coupon value = service base value (`Ancillary` lines); taxes are document-level price links; `IssuedTotal` = base + tax; the EMD's passenger and profile revision come from the associated ticket; carrier facts from the ticket coupon's `IssuedSegment`. With one coupon, error `2912` (tickets disagreeing on the revision) cannot occur and is removed; `2911` now also covers "not exactly one covered air service". | Ordering spec §6.2–§6.4, §9, §10 |
| 4 | Extra baggage over several flights is recorded as a deferred capability: it needs an EMD valuation model in Ordering (fare calculation or an authoritative proration) before the one-flight limitation is lifted. | Master §7.2, §12; Ordering spec header |
| 5 | Automated conformance tests are required. Every phase has two steps: A — build and prove with the `.http` file; B — tests for every "Expected behaviour" row (domain, application, persistence, quote contract). A phase is closed only after step B. | `CLAUDE.md` §4, §5; `START-HERE.md`; Phase 1 §5 |
| 6 | "No further benchmarking is needed" replaced: the Master records the benchmark baseline; later phases do not redesign from zero, but every external contract and repository fact a phase relies on is verified again against current source before implementation. | Master header, §14; `START-HERE.md` |
| 7 | The package is one directory tree with no nested archive. | `START-HERE.md` |

Also updated: AirAvail `ac84040` is recorded as the verified current head; the facts read today in IATA's *Airline Guide to EMD Implementation* (§4.2.4.3, §4.2.5, §5.1.1.2) are added to Master §13; the ATPCO list's file name is recorded for project evidence.

## Conformance matrix

```text
C1 Coupon Value vs Document Total            PASS
C2 ET passenger revision association         PASS
C3 ATPCO RFIC authority wording              PASS
C4 Multi-flight value allocation             CLOSED — not approved; Phase 1 limited to one covered flight
C5 Automated conformance tests required      PASS (step B of every phase)
C6 Benchmark / re-verification wording       PASS
C7 Single canonical package tree             PASS
Ordering source verification (cd50a2a)       PASS
```
