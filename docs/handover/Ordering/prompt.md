# Prompt — Ordering (Ordering.Final repository)

**Before giving it:**
1. In the Ordering repository, create `docs/ancillary/` and copy into it, keeping these relative paths, from Ancillary's `docs/`:
   - `handover/Ordering/spec.md`
   - `handover/AirOffer/spec.md`
   - `reference/Ancillary-Domain-Master.md`
   - `reference/Ancillary-Edge-Contract.md`
2. Approve the list in §12 of `handover/Ordering/spec.md` (changes to Ordering's own model).
3. Ancillary Phase 2B and the AirOffer work must be done; add `Ancillary:BaseUrl` and `Fulfillment:AncillaryHoldMinutes` to the local configuration.

```text
You are adding ancillary selling to Ordering for an existing ticketed order: list, add, reserve
and confirm at the Ancillary service, and issue the EMD-A.

All paths below are under docs/ancillary/. Read completely, in this order:
1. handover/Ordering/spec.md
2. reference/Ancillary-Edge-Contract.md
3. reference/Ancillary-Domain-Master.md — §2.3, §7.4, §8
4. handover/AirOffer/spec.md — §6
Then read Ordering's own authority (CLAUDE.md, the Master sections the spec names) and the code
the spec lists in its §2.

Build exactly §3 to §9 of the spec, following the existing local ticket-issue and reservation
code as the pattern. Then write one test per row of its §11, named with its row id, using the
existing test fixtures, with AirOffer and Ancillary replaced by fakes that follow their contracts.
Run the whole suite: green.

Rules:
- Take no domain decision. If the spec contradicts Ordering's source or Master, stop and ask.
- Ordering never reads Ancillary's catalogue and never asks Ancillary for a price; it calls
  AirOffer for the list and the details, and Ancillary only to reserve, read, confirm, release.
- Do not change existing behaviour for orders without ancillary services.
- Do not commit or push.

Report: what was built by layer; migrations; the table "row -> test"; the test run result;
open questions; deviations (there should be none).
```
