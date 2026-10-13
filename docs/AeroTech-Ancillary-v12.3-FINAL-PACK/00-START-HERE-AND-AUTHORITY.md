# AeroTech Ancillary v12.3 — Single Project Authority

**State:** approved design to be implemented and verified by Backend Agent, not a claim that code is already deployed. **Date:** 2026-10-13. **Repo:** `aliifarhadi/AeroTech.Ancillary`; **branch:** `k8s-stg`; **reviewed HEAD:** `8fda5f1e8c2586a08fa995a0f8701e9299182986` (`Fix. Remove POS Mandatory`). Recheck real HEAD first. Work ONLY in this repository.

## Single authority
This COMPLETE v12.3 pack is self-contained, and replaces conflicting parts of v12.2 and all earlier Seat Inventory addenda. No need to consult previous packs for business decisions. The actual code determines retained API names, EF patterns and exact build/test mechanics; v12.3 determines desired behavior. No agent may silently substitute donor conventions for this pack. Owner approves architecture and scope; coding agent implements, tests, reports.

## Exact scope: existing phases, not new reservations
**P1:** typed authoring, Definition, Provision, Pricing, lifecycle and nine profiles A01–A24. **P2:** already-implemented Shopping/AncillaryOffers, selection-evaluation and inventory configuration for non-Seat products. **Both P1 and P2 are in scope for targeted correction and regression.** Previous v12.2 texts claiming all Shopping is merely future P3 are superseded. **P3:** Hold, Reservation, Confirm, Release, Order mutation, seat assignment, fulfilment, EMD issuance: OUT OF SCOPE; do not edit these flows or their contract.

## Architectural decision
- Seat A07/A08/A09: commercial Definition/Provision/Pricing and Seat-only Shopping price offers in Ancillary. No AncillaryInventoryPolicy, no Seat stock, no mandatory inventory, no FlightFlow provider key or call. Seat availability, layout and actual seat characteristics belong to external FlightFlow; composition is external to Ancillary.
- Shopping **has one existing canonical `AncillaryShoppingEngine`**, retained. Public/surface route outputs separated: `SeatOffers` for Seat commercial prices and `AncillaryOffers` for other services. **No second Shopping engine**; no duplicate rule/pricing implementations. Remove broken Ancillary `SeatMaps` stub. Preserve `Details` and `Selections/Evaluate` on the correct family surface, with ownership checks and no cross-family discovery.
- Other profiles: physical Count/Weight/Slot, Supplier authority and optional explicit Unlimited policies are still supported. An absent non-Seat policy != Unlimited. An absent Seat policy is **NotApplicable**, not NotConfigured/Unknown.
- There must be no direct **runtime source-code** reference/HTTP client/configuration for FlightFlow in Ancillary after change. Generic flight-identity/aircraft facts used in commercial rules are **not** the FlightFlow dependency; remove the hardcoded FlightFlow HTTP adapter but preserve correct external reference validation of non-Seat flight-based Count/Weight sources if authoritative replacement exists. Never fake `Verified`.

## Current source decisions to retain
`SalesRestrictions == null` or empty `PointsOfSale` means no POS restriction; nonempty list restricts matching POS. HEAD intentionally removed mandatory POS publication. Preserve `PricingUnit.PerSeat=5`, `ServiceCoverageScope.Portion=2`, all other existing enum values; do not reintroduce the old POS mandate.

## Greenfield assumption
Owner states **NO historical production data**. Agent may remove obsolete migrations, enum member `InventoryAuthority.FlightFlow=4`, obsolete schema/fixtures and stale tests and build a clean current-schema migration. **No data-preservation, legacy compatibility, dual-write or costly expand/contract work**. But do not delete working non-Seat Inventory and do not destroy unrelated tests: current SQL correctness, baseline smoke and compiler/contract compatibility still must pass.

## Deliverables and acceptance
- Real code changes in Ancillary only; migrations/schema and DI corrected, zero dangling FlightFlow dependencies in `src`.
- Seat Definition, Provision, Pricing, Pricing revisions, Shopping pricing and selection evaluation work without any Seat InventoryPolicy and without a FlightFlow connection.
- Three surfaces each expose working, properly authorized separated SeatOffers and non-Seat AncillaryOffers, no broken SeatMap proxy; real HTTP and SQL regression.
- All nine profiles / 24 scenarios and shared rules, fees/tax/currency, non-Seat inventories unaffected; concrete PASS/FAIL evidence, source blockers reported truthfully.
- No implementation of Hold/Confirm/Order. No code changes outside Ancillary repository. A final agent report with current SHA, file diff, test results, known blockers and exact endpoints.

## Read order
`01` decisions, `02` domain graph, `03` nine profiles and A01–A24, `04` Seat exact contract, `05` Provision, `06` Pricing, `07` non-Seat inventory, `08` Backoffice, `09` **existing Shopping and new SeatOffers route**, `10` DB, `11` tests, `12` exact source edits, `13` **BACKEND AGENT PROMPT**, `14` frontend contract, `15` continuation, `16` evidence, `17` enums. The full combined copy is `AeroTech-Ancillary-v12.3-COMPLETE.md`.
