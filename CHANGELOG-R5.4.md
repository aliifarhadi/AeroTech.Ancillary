# Changelog — R5.3 → R5.4

**Date:** 2026-10-03. Prepared after Phase 1 was closed in the repository (`467aa55`), for Phase 2. No domain decision of R5.3 changes.

| # | Change | Where |
|---|---|---|
| 1 | The quote contract's path is `Contracts/ancillary-quotes-v1.openapi.yaml` (the repository already has a `Contracts` folder; on Windows `contracts` and `Contracts` are the same). All references updated. | CLAUDE.md, START-HERE.md, Phase-1 document, End-to-End |
| 2 | Order of the product checks: the §3.4 combination (`16105`) is checked before every other product rule. | Master §4.2 |
| 3 | Identity of an occurrence: bound-scoped = product + traveller + bound; flight-scoped = product + traveller + flight, whatever `boundRef` says. Used by selection check 4 (`16307`). An `existing` entry without a flight has no effect on a flight-scoped occurrence. A flight-scoped selection for a flight the traveller does not fly fails check 5. | Master §7.2, §7.6 |
| 4 | Phase-2 document rewritten: build list, golden example, 30 behaviour rows, the planned adjustment of Phase-1 expectations. | `docs/phases/Phase-2-Lounge-Access.md` |
| 5 | New proof file. | `docs/proof/phase-2.http` |
| 6 | Quote contract at `R5.4-phase2`: `lounge` on items; open-enum lists written as "Known: …". | `Contracts/ancillary-quotes-v1.openapi.yaml` |
| 7 | Phase-1 golden response shows `"lounge": null`; Phase-1 C06 note; proof request 30 now uses `Quota` (still non-existent) instead of `LoungeAccess`. | Phase-1 document, `phase-1.http` |
| 8 | CLAUDE.md describes the repository as it is now and adds the Phase-2 build order; START-HERE has the Phase-2 prompt. | CLAUDE.md §2, §6; START-HERE.md |
