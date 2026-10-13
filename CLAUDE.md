# AeroTech.Ancillary - working rules

Rules set by the Owner. They apply to every task in this repository and win over any pack or prompt document that conflicts with them.

1. **The sibling pattern wins over a document.** Before designing a surface, a use case or an integration, open the matching AirPrice source (and Ordering where AirPrice has no equivalent) and copy its shape. When a document asks for something the sibling does not do, stop and ask.
2. **One write path per aggregate.** An aggregate has one POST, one PUT of the whole draft, one GET by id, a paginated list and its lifecycle actions. No endpoint and no use case per child entity, rule group or row. A profile-specific Backoffice route is a typed facade over that same use case, never a second implementation, and is added only when the Owner orders it.
3. **Relational storage only.** No JSON or nvarchar(max) list column; a list is a child table.
4. **No business rule in infrastructure.** ReferenceData, Providers, Persistence and the ServiceHost adapters only read, write or call. Rules live in the Domain, or in an Application or Query service.
5. **The aggregate guards its invariants when it is authored**, at Define and at Change, not only at publication.
6. **Open points are asked, not decided.** Placement, naming, response shape, error code, configuration behaviour and anything else a prompt leaves open is asked before coding, with a recommendation and the source evidence.
7. **Before any new work** give two short lists: how the sibling does it, and what is open. Code after the answer. Work in small steps the Owner can review.
8. **No commit, push or merge** without the Owner's explicit order for that action. No code comments. Reports go to `reports/`.
9. **Every claim is backed by a command run in the same turn.** A test run is green only when the executed count equals the discovered count and nothing aborted.
