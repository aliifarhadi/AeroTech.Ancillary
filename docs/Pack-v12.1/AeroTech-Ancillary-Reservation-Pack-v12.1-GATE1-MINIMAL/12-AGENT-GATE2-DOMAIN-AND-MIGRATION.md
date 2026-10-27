# Gate 2 — Coding Agent — Typed Domain / SQL Rehydration (Conditional)

## Precondition
Only execute on explicit Owner approval **and** when Gate0/1 show a source-proven gap in Domain/Persistence. If no gap: report `NO CODE CHANGE REQUIRED` with proof and test output; do not refactor because Pack has this Gate.

## Target and no-go
Preserve `AncillaryReservation` Root, `AncillaryReservationUnit`, `AncillaryReservationUnitAllocation`, owned relational selections, existing `V11ReservationSelections` migration. No 24 aggregates, no general JSON column, no duplicate Stock Pool, no Domain-to-Ordering dependency, no Price snapshot reader. `CLAUDE.md`: lists in child table.

## Audit then correct only proven faults
1. `AncillaryHoldFamilies` and `HoldSelectionVerifier` pass all selected dynamic Options; `TypedSelectionForm.WithFixedChoices` captures correct `ServiceDefinitionRef`, `EquipmentKind`, `MealCode`, `MenuItemRef`, `ToCabinId`, `AirportId`, `FacilityId` at Hold time.
2. `AncillaryReservationUnit` creates one typed family selection max, keeps FlightIds in relational child, preserves Qty/decimal/offset and normalized SeatNumber. Verify all existing owned configurations and actual precision.
3. `AncillaryHoldMapper` GET reconstructs from Reservation SQL alone after Definition/Provision retirement; no current Catalog/Offer lookup, no fabricated provider ref.
4. Canonical replay compares effective selections not just Unit ID; changed BagRef/SSR/Slot fails 409; same idempotency after Retire still returns historical same selection without access to catalog.
5. GATED variants remain GATED. If source cannot prove a field, STOP with exact File/Line and recommendation instead of inventing.
6. Existing migration applied? No schema change. Migration missing/defective? propose *minimal* EF migration SQL + data impact and wait for Owner permission to run. Never delete DB rows or run Down/Up on owner environment unasked.

## Tests
D01–D12, W02/W03, GL schema checks, owned precision, Add/Save/Reload/GET with new DbContext, retirement history; compare source-bound choices and after-state. Existing tests not removed/skipped.

## Deliverable
`reports/V12-GATE2-TYPED-REHYDRATION.md`: keep/refactor table per file; migration status, actual tests, no new features; STOP for Owner review. No commit/push/merge.
