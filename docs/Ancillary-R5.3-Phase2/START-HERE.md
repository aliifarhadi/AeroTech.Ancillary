# Start here

The documents (R5.4) live in the repository `aliifarhadi/AeroTech.Ancillary`, branch `k8s-stg`, at these paths:

```text
CLAUDE.md
START-HERE.md
CHANGELOG-R5.3.md
CHANGELOG-R5.4.md
Contracts/ancillary-quotes-v1.openapi.yaml
docs/Ancillary-Domain-Master.md
docs/Ordering-P1-Ancillary-Implementation-Spec.md
docs/Phase-1-End-to-End.md
docs/phases/Phase-1-Extra-Baggage.md
docs/phases/Phase-2-Lounge-Access.md
docs/phases/Phase-4-Stock.md
docs/phases/Phase-5-Paid-Seat.md
docs/phases/Phase-6-Pricing-Depth.md
docs/proof/phase-1.http
docs/proof/phase-2.http
```

## State

| Phase | State |
|---|---|
| 1 — Extra Baggage | closed in Ancillary (`467aa55`). The cross-service run waits for AirOffer and Ordering. |
| 2 — Lounge Access | next. Prompt below. |
| 3 | no work in this repository |
| 4 — Stock | waits for FlightFlow's cancellation message (Phase-4 document §0) |
| 5 — Paid seat | waits for AirOffer's seat-characteristic input (Phase-5 document §0) |
| 6 — Pricing depth | can follow Phase 2 |

## Prompt for Claude Code — Phase 2

Before giving it: put a real Backoffice token, an airline with no Ancillary data, and a currency in the header of `docs/proof/phase-2.http`.

```text
You are implementing Phase 2 (Lounge Access) of AeroTech.Ancillary. Phase 1 is complete and
tested. In this one session you build Phase 2, prove it, and write its conformance tests.
Work through all parts without stopping; stop early only for a reason in "When to stop".

PART 0 — Housekeeping
- Delete docs/Ancillary-R5.3.zip.

PART 1 — Read, completely and in this order
CLAUDE.md; docs/Ancillary-Domain-Master.md; docs/phases/Phase-2-Lounge-Access.md;
Contracts/ancillary-quotes-v1.openapi.yaml; docs/proof/phase-2.http. Then run the whole existing
test suite once and record the result: it must be green before you change anything.

PART 2 — Build (CLAUDE.md §6, steps 1–4)
Build exactly what docs/phases/Phase-2-Lounge-Access.md §1 lists, following the code already in
the repository as the pattern. In particular:
- The combination check of Master §3.4 runs before every other product rule (Master §4.2,
  "Order of the product checks"), on Define, Change and Activate.
- Flight-scoped occurrences, lounge applicability, occurrence identity (product + traveller +
  flight; the bound does not change it), existing-entry matching by flight, and the item order
  of Master §7.4 (bag items of a bound first, then each flight's flight-scoped items).
- The quote response matches Contracts/ancillary-quotes-v1.openapi.yaml exactly, including
  "lounge": null on bag items.
- Nothing of Phase 3 or later: no other enum member, reference entry, condition or field.

PART 3 — Phase-1 adjustment
Apply docs/phases/Phase-2-Lounge-Access.md §5 and nothing else: the Phase-1 C06 tests drop
TravellerSegment, EmdStandalone, Each and LoungeAccess and gain the listed still-missing names.
No other Phase-1 test changes. If any other Phase-1 test fails, that is a defect in the new code.

PART 4 — Prove
Build with no errors and no new warnings. Add and apply the migrations. If the token in
docs/proof/phase-2.http is real, start the service, run docs/proof/phase-2.http top to bottom,
then docs/proof/phase-1.http (with its own airline). Every request must return its expected
result; a failure is a code defect. If a token is the placeholder, do not run that file and
say so in the report.

PART 5 — Conformance tests
Every row of docs/phases/Phase-2-Lounge-Access.md §4 — S01–S03, C01–C09, Q01–Q16, G01–G02 —
is covered by at least one test named P2_<rowId>_…, using the existing fixtures and the same
split as Phase 1 (domain tests without a database; application and persistence tests on SQL
Server; contract test for Q01 reading the golden example of the Phase-2 document and the
OpenAPI file). G01 is the whole suite green. Then run the full suite: Phase 1 and Phase 2 green.
No new package and no new project reference.

PART 6 — Report (final message; create no document)
1. What was built, by layer; migrations.
2. Build result.
3. Proof: request number → status and body for each file run, or "not run: placeholder token".
4. Table: P2 row id → test name(s) → kind. All 30 rows.
5. Phase-1 test changes (only those of §5).
6. Full test run result (counts per project).
7. Open questions; deviations (there should be none).

When to stop before the end
- The existing suite is not green before you start.
- SQL Server is unreachable or a migration cannot be applied.
- A row of §4 contradicts the Master, or two documents disagree.
- Something cannot be done without a new package or project reference.
In every other case, continue until all parts are done.

Do not commit or push.
```
