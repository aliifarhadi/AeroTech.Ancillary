# Ancillary v12.1 Owner Review Pack

**Approval requested. This is NOT an instruction to write or change code.**

Files:
- `00-AUTHORITY-AND-DECISIONS.md` - source baseline, decisions, approval gates
- `01-BENCHMARK-TRACEABILITY.md` - official ATPCO S5/S7 and AirPrice verified comparison
- `02-ENTITY-CATALOG.md` - 3 Phase-1 commercial aggregate roots, 10 rule-group entities, 27 child row entities, pricing lines and future-phase visibility
- `03-RULE-SEMANTICS-AND-INVARIANTS.md` - exact allow/deny, range/time, selector, precedence, charging invariants
- `04-PRICING-AND-PUBLICATION.md` - money/rate/version lifecycle, atomic publish and outcome
- `05-SCENARIO-MATRIX.md` - 98 scenario checks across P1/P2/P3, positive/negative/boundaries, 17 product-family cases
- `06-MIGRATION-AND-BACKOFFICE.md` - safe v12->v12.1 transition, contracts, deletion and AirAvail gap
- `07-THREE-PHASE-PLAN-AND-GATES.md` - commercial authoring, stock, reservation as 3 owner-controlled stages
- `08-OWNER-APPROVAL-CHECKLIST.md` - exact approve/amend decisions

Do NOT hand these files as a coding-agent task yet. First owner confirms the catalog and rule semantics; then issue a separate explicit Phase-1 implementation prompt tied to verified Git SHA.
