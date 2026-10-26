# Gate 1 — Coding Agent — Domain, DTO، Migration کوچک و Unit lifecycle

**Precondition:** Gate 0 audit report accepted. Authority: `01-MASTER-RESERVATION-CONTRACT-FA.md`, implementation specifics `02,03,06,07`؛ source not other historical packs.

## Approved work
1. **Reuse and evolve** `Domain/AncillaryReservationAggregate/AncillaryReservation.cs` و `Entities/AncillaryReservationUnit.cs`, `Arguments/AncillaryReservationUnitArgs.cs`.
2. Add small immutable child `AncillaryReservationUnitAllocation` per `03`: Id, UnitId, ResourceKind existing enum, InventorySourceId, Amount decimal(19,3). Owned relation under Unit, **no standalone aggregate**.
3. Add Unit nullable immutable InventoryFacts (`AcceptedWeightKgTotal`, `AppointmentTimeWithOffset`, `GuestCount`) and optional `AppendIdempotencyKey`. Add corresponding DTO only for Generic Hold and optional `holdId` on SeatMap Hold request; no other optional “24 family” fields.
4. Keep `AncillaryReservationStatus` and Unit enum 1..5 unchanged. Implement domain `ReleaseUnit`, `CancelUnit`, `AppendSeatUnits`, revise `Confirm`, `Release`, `Cancel`, `Expire` to operate **only on eligible active Unit statuses** preserving historical Released/Cancelled statuses.
5. Fix old `Definition+Traveller+Flights` uniqueness guard: should not forbid a valid second Provision/Variant; duplicate OrderServiceId and live duplicate Seat/flight inside same Hold remain guards.
6. Canonical idempotency comparison includes all three inventory facts, append key and seat identities. No new global Idempotency Entity/table.
7. Add EF additive migration CommandDb: one Allocation table, nullable Fact/AppendKey columns, FK and Indexes. Respect existing DB style and actual decimal precision. Test migration with existing rows; never wipe user DB.
8. `RequestedExpiresAt` required for **new** Hold; no time of infinite stock; defaulting expiry to now+arbitrary TTL prohibited.
9. Preserve current SeatMap Validator/Resolve commercial contract. `StockPoolId` legacy left untouched and not used as inventory authority.

## Strict not allowed
No Shopping/Price/Resolve changes؛ no FlightFlow/Ordering calls; no Supplier adapters, Pending/Mixed/Failed enums, EMD/Refund, new stock Aggregate or background job.

## Tests & evidence
- Domain exact transitions all 5 states and numeric values (`L01`..`L23` in `07` applicable)
- Unit-level Released does not get re-confirmed or re-cancelled.
- Sequence two Provision different same Definition and same Coverage allowed when legitimate.
- Same Create idempotency data vs changed `acceptedWeightKgTotal` conflicts.
- Add Seat appendix to existing Held preserves same root Id and append retry state (pure Domain test, no real verifier yet).
- EF migration + `has-pending-model-changes`: PASS in test DB.
- Negative test unknown/extra InventoryFacts.

## Deliverable
`reports/V10-GATE1-DOMAIN-AND-SCHEMA.md` with file diff, ER model, full changed field/type list, migrations generated SQL, all test commands and results, idempotency proof, unresolved conflicts.

**Important:** After this Gate, existing Hold code may still not guarantee stock; **do not deploy or claim P3 closed**. Continue only when Gate1 reviewed.
