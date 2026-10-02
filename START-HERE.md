# Start here

This package (R5.3) is one directory tree. Its content is copied as it is into the root of `aliifarhadi/AeroTech.Ancillary`, on a working branch created from `k8s-stg` (`843cf7c`). There is no nested archive; these files are the authority.

```text
CLAUDE.md
START-HERE.md
CHANGELOG-R5.3.md
contracts/ancillary-quotes-v1.openapi.yaml
docs/Ancillary-Domain-Master.md
docs/Ordering-P1-Ancillary-Implementation-Spec.md
docs/Phase-1-End-to-End.md
docs/phases/Phase-1-Extra-Baggage.md
docs/phases/Phase-2-Lounge-Access.md
docs/phases/Phase-4-Stock.md
docs/phases/Phase-5-Paid-Seat.md
docs/phases/Phase-6-Pricing-Depth.md
docs/proof/phase-1.http
```

## For the Owner

| Step | What |
|---|---|
| 1 | Create the branch and copy the tree above into the repository root. |
| 2 | Make sure the local configuration lets the service start (connection strings, broker, cache, identity). |
| 3 | Give Claude Code the step-A prompt. |
| 4 | Review as described in `docs/phases/Phase-1-Extra-Baggage.md` §5. |
| 5 | Give Claude Code the step-B prompt. The phase is closed when its tests pass. |
| 6 | For the cross-service run, hand `docs/Phase-1-End-to-End.md` §3 to the AirOffer team and `docs/Ordering-P1-Ancillary-Implementation-Spec.md` to the Ordering team. |

## Prompt for Claude Code — Phase 1, step A (build and prove)

```text
You are implementing Phase 1 of AeroTech.Ancillary in this repository. This session is step A.

Read, in this order and completely, before writing any code:
1. CLAUDE.md
2. docs/Ancillary-Domain-Master.md
3. docs/phases/Phase-1-Extra-Baggage.md
4. contracts/ancillary-quotes-v1.openapi.yaml
5. docs/proof/phase-1.http

Then build Phase 1 exactly as CLAUDE.md §5 orders it.

Rules:
- Build only what is marked P1. Nothing of a later phase may exist in the code.
- Take no domain decision. If a fact is missing or two documents disagree, stop and ask.
- This is step A: production code only. Tests are step B, in a later session.
- Do not commit or push.
- docs/Phase-1-End-to-End.md and docs/Ordering-P1-Ancillary-Implementation-Spec.md describe other
  services; build nothing from them here.

Finish by starting the service, running docs/proof/phase-1.http from top to bottom, and reporting as
CLAUDE.md §4 says.
```

## Prompt for Claude Code — Phase 1, step B (conformance tests)

```text
Phase 1 of AeroTech.Ancillary is built and reviewed. This session is step B.

Read CLAUDE.md §4 (step B), docs/Ancillary-Domain-Master.md and docs/phases/Phase-1-Extra-Baggage.md.

Write the conformance tests of Phase 1: every row of "Expected behaviour" (S, C, R and Q rows) is
covered by at least one automated test named with its row id, in the kinds CLAUDE.md §4 lists
(domain, application, persistence, quote contract).

Rules:
- Do not change behaviour, routes or contracts. A defect a test exposes is fixed in the code and
  listed in the report. A row that seems wrong is a question for the Owner, not a changed test.
- docs/proof/phase-1.http must still pass unchanged.
- Do not commit or push.

Finish with the table "row id -> test name", the test run result, and the list of production
code changes.
```

## Later phases

Each later phase uses the same two prompts with its own phase document. Phase 3 has no work in this repository. Before a phase starts, the "Before starting" table of its document must be satisfied, and the facts it relies on in other repositories are checked again against their current source.
