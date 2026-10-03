# Prompt — Phase 2B (Ancillary repository)

The proof file needs a real Backoffice token, an airline with no Ancillary data and a currency in its header; without them the proof is skipped and reported as not run.

```text
You are implementing Phase 2B (reservations and the published read model) of AeroTech.Ancillary.
Phases 1 and 2 are complete and tested. In this one session you adapt the tests to the new
document layout, build Phase 2B, prove it, and write its conformance tests. Work through all
parts without stopping; stop early only for a reason in "When to stop".

PART 0 — Clean up and adapt
1. The Owner has unzipped the new documents over the repository. Delete every old document that
   is not part of the new layout:
     - in the repository root: START-HERE.md, CHANGELOG-R5.3.md, CHANGELOG-R5.4.md
     - in docs/: everything except README.md, reference/, phases/ and handover/
       (this removes docs/Ancillary-Domain-Master.md, docs/Ancillary-Edge-Contract.md,
       docs/AirOffer-Ancillary-Implementation-Spec.md, docs/Ordering-P1-Ancillary-Implementation-Spec.md,
       docs/Phase-1-End-to-End.md, docs/proof/, docs/Ancillary-R5.3-Phase2/, any *.zip, and the old
       flat files in docs/phases/ such as docs/phases/Phase-1-Extra-Baggage.md)
     - in docs/phases/: everything except README.md and the folders P1-Extra-Baggage, P2-Lounge-Access,
       P2B-Reservations, P4-Stock, P5-Paid-Seat, P6-Pricing-Depth
2. In both tests/*/Fixtures/RepositoryFiles.cs, change the paths and nothing else:
     docs/phases/Phase-1-Extra-Baggage.md  -> docs/phases/P1-Extra-Baggage/phase.md
     docs/phases/Phase-2-Lounge-Access.md  -> docs/phases/P2-Lounge-Access/phase.md
     docs/proof/phase-1.http               -> docs/phases/P1-Extra-Baggage/proof.http
     docs/proof/phase-2.http               -> docs/phases/P2-Lounge-Access/proof.http
   and every caller that passes a proof file name ("phase-1.http", "phase-2.http") so that it
   resolves to the new path. The OpenAPI file stays at Contracts/ancillary-quotes-v1.openapi.yaml.
3. Run the whole test suite. It must be green before you change anything else.

PART 1 — Read, completely and in this order
CLAUDE.md; docs/README.md; docs/reference/Ancillary-Domain-Master.md (all of it; §2, §7.6, §8
and §15 with special care); docs/phases/P2B-Reservations/phase.md;
docs/phases/P2B-Reservations/proof.http.

PART 2 — Build
Build exactly phase.md §1–§3, in the order of phase.md §4, following the code already in the
repository as the pattern. In particular:
- Reserve evaluates its units with the existing AncillaryQuoteEvaluator in selection mode, with
  the request's context. Do not write a second evaluator and do not change the quote operation.
- A reserve request with a known idempotencyKey is answered from the stored reservation without
  evaluating anything; "content" is compared exactly as Master §8.3 defines it.
- Two simultaneous Reserve requests with the same key must both end with the same single
  reservation (unique index on IdempotencyKey; the loser re-reads and returns it).
- Expiry is a view rule only (Master §8.2 rule 5): no job, no stored change, no message.
- Build nothing of Master §8.4 (stock pools), no outbox message, no consumer, no call to
  another service.
- LastUpdateTime is written by the product and price-rule synchronizers on every projection,
  from the clock; the migration fills existing rows with CreatedAt.

PART 3 — Prove
Build with no errors and no new warnings. Add and apply the migrations. If the token in
docs/phases/P2B-Reservations/proof.http is real, start the service and run that file top to
bottom (request 21 includes a SQL check — run it against the development database), then
docs/phases/P2-Lounge-Access/proof.http and docs/phases/P1-Extra-Baggage/proof.http with their own
airlines. Every request must return its expected result; a failure is a code defect. If a token
is the placeholder, do not run that file and say so.

PART 4 — Conformance tests
Every row of phase.md §5 — V01–V20, M01–M06, G01–G02 — is covered by at least one test named
P2B_<rowId>_…, using the existing fixtures and the same split as before (domain tests without a
database; application and persistence tests on SQL Server). V11 and V19 are real concurrency
tests against the database. M01 reads the column list from the database catalogue. G01 is the
whole suite green with no change to any Phase-1 or Phase-2 test other than the paths of PART 0.
No new package and no new project reference.

PART 5 — Update the status
In docs/phases/README.md set P2B to "built — awaiting Owner review" and fill in the date.
Change nothing else in docs/.

PART 6 — Report (final message; create no document)
1. PART 0: the files deleted, and the test files and lines changed.
2. What was built, by layer; migrations.
3. Build result.
4. Proof: request number -> status and body for each file run, or "not run: placeholder token".
5. Table: P2B row id -> test name(s) -> kind. All 28 rows.
6. Full test run result (counts per project).
7. Open questions; deviations (there should be none).

When to stop before the end
- The suite is not green after PART 0.
- SQL Server is unreachable or a migration cannot be applied.
- A row of phase.md §5 contradicts the Master, or two documents disagree.
- Something cannot be done without a new package or project reference.
In every other case, continue until all parts are done.

Do not commit or push.
```
