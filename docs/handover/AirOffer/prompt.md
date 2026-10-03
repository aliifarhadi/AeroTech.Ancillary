# Prompt — AirOffer (AirAvail repository)

**Before giving it:**
1. In the AirAvail repository, create `docs/ancillary/` and copy into it, keeping these relative paths, from Ancillary's `docs/`:
   - `handover/AirOffer/spec.md`
   - `reference/Ancillary-Domain-Master.md`
   - `reference/Ancillary-Edge-Contract.md`
   - `phases/P1-Extra-Baggage/phase.md`
   - `phases/P2-Lounge-Access/phase.md`
2. Ancillary Phase 2B must be done (the read model needs `LastUpdateTime`).
3. Add `ConnectionStrings:AncillaryQueryDbContext` (Ancillary's query database) to the local configuration.

```text
You are adding ancillary listing and pricing to AirOffer (repository AeroTech.AirAvail, solution
AeroTech.AirOffer.sln). AirOffer reads Ancillary's published read model by SQL and evaluates it
itself; it makes no HTTP call to Ancillary.

All paths below are under docs/ancillary/. Read completely, in this order:
1. handover/AirOffer/spec.md
2. reference/Ancillary-Edge-Contract.md
3. reference/Ancillary-Domain-Master.md — §3, §4.2, §7, §9, §15
4. phases/P1-Extra-Baggage/phase.md and phases/P2-Lounge-Access/phase.md — their Quote rows and
   golden examples
Then read the existing code the spec points to in its §2: Persistence/Scripts and Repositories
(point-of-sale snapshot, reference-data stamp), Services/Services/Shared/ReferenceDataCache.cs,
Services/PointOfSaleEligibilityService.cs, Service2Service/ServiceController.cs,
Services/Offers/JourneyOfferIdCodec.cs, Services/Exceptions, ServiceHost/Configurations.cs.

Build exactly §4, §5 and §6 of the spec, following those existing classes as the pattern.
Build nothing of its §7. Then write the tests of its §8 (X01–X03, L01–L06, D01–D07, S01–S04),
named with their ids, without a database and without a new package. Run the whole suite: green.
If Ancillary's development database is reachable, do the manual proof of its §9.

Rules:
- Take no domain decision. Where the spec and the Master leave something open, or the source
  contradicts the spec, stop and ask.
- Read only the tables and columns of Master §15. Never write to that database.
- Do not change any existing endpoint, codec, DTO or script.
- Do not commit or push.

Report: files added and changed; scripts; configuration keys; the table "row -> test";
the test run result; the manual proof results (or "not run" and why); open questions.
```
