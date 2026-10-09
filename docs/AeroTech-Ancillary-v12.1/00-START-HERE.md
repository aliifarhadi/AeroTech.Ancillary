# AeroTech.Ancillary v12.1 — FINAL DOMAIN COMPLETION PACK (PHASE 1 + PHASE 2)
**Status:** Domain design and implementation authority for correction on existing `feat/ancillary-v12.1-phase2-stock`, **not code already implemented**. Date: 2026-10-09.

## Goal
Deliver a realistic airline Ancillary catalog + provisions + price authoring + capacity policy and capacity administration for 15 mainstream families, consistent with documented Amadeus, flydubai, Iberia NDC, ATPCO examples, with the **smallest coherent domain**. Preserve the existing v12.1 framework and source. No external service changes. No Phase 3 Hold/Confirm/Release/EMD or real shopping evaluator.

## Source of truth
- Repo: `aliifarhadi/AeroTech.Ancillary`, feature branch `feat/ancillary-v12.1-phase2-stock`, pushed SHA `1abf7a53e0efb9eb892097977327718c61486158`; `k8s-stg@933b7b7a793b426dbcb6362bbf8519886edf9080` still has Phase 1.
- User-approved D01–D14 from 2026-10-09: D01–03, D05–07, D09, D12–13 Recommended; **D04 different Provision per POS**; **D11 keep current**; **D14 cleanup**. User delegated design selection on unknown D08 and D10: this pack selects the least complex benchmark-supported interpretation and documents why.
- **New definitive authority:** docs 01–10 of THIS pack for Phase 1+2 corrections > approved v12.1 business contract for unchanged concepts > current source > older donor packs. No rename or new domain object beyond documented delta.
- Canonical existing Phase 1: Supplier; AncillaryServiceDefinition; AncillaryProvision with ten typed Rule children; AncillaryPricing; Phase 2 AncillaryInventoryPolicy, FlightCountInventory, FlightWeightInventory, AirportSlotInventory. Existing `AncillaryReservation` frozen.

## Scope and production truth
**Phase 1** author/publish a service, its eligibility conditions, prices and documentary/booking metadata.
**Phase 2** author/activate inventory authority, local capacity administrative totals and immutable adjustments where real source evidence exists. The new design is **authoring-ready** and tests real SQL concurrency. It does **not** claim flight safety capacity or permit guaranteed limited-stock selling. A missing physical reference remains explicitly unavailable per Owner D11.
**Phase 3** future only: shopping evaluation, supplier approval, Sell/Hold/Confirm/Release, expiry, cross-order passenger consumption, stock allocations, EMD. Do not implement.

## Reading order
01 decisions → 02 complete graph/fields → 03 Pricing contract → 04 Provision contract → 05 Inventory contract → 06 15 real families → 07 future OTA contract boundary → 08 migration → 09 acceptance matrix → 10 Coding Agent instruction → 11 traceability.

## Stop conditions
- No changes to AirAvail, FlightFlow, AirPrice, Ordering, JetPay, or their contract copies beyond existing Ancillary-owned contracts as separately specified.
- No undocumented industry RFISC/SSR codes or fake provider availability.
- No destructive migrations without Ancillary-owned pre/post reconciliation, backup/recovery proof.
- Do not merge or close without actual test logs and Owner audit.
