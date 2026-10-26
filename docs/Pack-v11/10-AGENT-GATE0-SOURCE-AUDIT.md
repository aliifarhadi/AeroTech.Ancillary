# Coding Agent — Gate 0 — Read-only source verification

**Status:** done 2026-10-26 (`reports/V11-GATE0-SOURCE-AND-CONTRACT-AUDIT.md`); the Owner's answers are merged into 01–08 and 10–15 (01 §I).

**ONLY REPORT. NO CODE EDIT, NO MIGRATION, NO COMMIT/PUSH/MERGE.** Owner must approve Gate 0 before Gate 1. Read `00`–`08` entirely first. Commit pinned `7a74055c774b2466fe239b8307ec93b15264e50c`; when working HEAD differs, record exact SHA and diff.

1. Read `CLAUDE.md`, Reservation Root/Unit/Allocations/UnitConfiguration, REST Hold/GET/SeatMaps requests, Application Hold/Allocator, InventoryLedger, actual `AncillaryOffersDto`/`AncillaryVariantGroupDto`/`AncillaryProductGroupDto`/`AncillaryUnitOfferDto`, `SelectionOptionsInput`, six `*SelectionOptions`, Resolve DTOs and mapper, `CustomerSelectionContract`, `AncillaryVariant` registry.
2. Produce **exact CLR field/type/nullable/serializer table** for every path in `02`. Check wire names/representations of `variantCode, products, units, unit, coverage, selected` and Family names. Do NOT assume sample enum numeric values or `SeatHoldFlightInput` field types. Record all different fields with file path and line number.
3. For each A01–A24 make a table: `variant | family | selected variable fields source | fixed fulfillment fields source | required inventory facts | supported/gated | reason | test`. **A02/A06/A12/A24** check whether customer-selected field comes from Price/Resolve vs fixed Definition; don't invent missing source.
4. Verify `InventoryResourceKind`, `ServiceCoverageScope`, `AncillaryReservationStatus` and Unit enum numeric values; source schema for `CoveredFlightIds` (JSON? relational?), constraints/migration baseline/legacy Reservation count, DDL. Read `reports/V10-GATE2-LOCAL-INVENTORY.md` and closure but treat their test claims as reported only.
5. Identify exact 6 reservation API endpoints; ensure 0 Ordering outgoing calls and 0 physical SeatHold call. Check `IOrderCommercialContextReader` injection or Provider client calls in Reservation use cases.
6. Audit `flightRefs` string vs numeric `flightIds`: **Caller supplies mapped ids**; Ancillary does not map via Ordering. `provisionId` from Resolve; productKey not API identity.
7. Produce report `reports/V11-GATE0-SOURCE-AND-CONTRACT-AUDIT.md` containing discrepancies, evidence, proposed *smallest* change, migration impact, total source/types list and decision table. No code until approval. If an existing field name contradicts doc, **report** rather than silently rename/create a new business concept.

**No artificial Architecture:** no new offer resolver, no Identity integration, no Generic typed-DTO generator or reflected form engine. Benchmark FlightFlow only at lifecycle principles. New code exactly within Reservation.
