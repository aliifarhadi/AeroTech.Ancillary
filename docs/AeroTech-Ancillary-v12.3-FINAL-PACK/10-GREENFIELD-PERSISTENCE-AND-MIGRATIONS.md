# 10 — Clean SQL and EF contract, no invented historic-data work

Owner explicitly confirms NO historical business data requiring preservation. Do not design compatibility schema for old Seat InventoryPolicies, no dual-write, migration archive, old ID remap, data-wiping caveat or stored-amount reconstruction. Rebuild the current dev database schema/migrations consistently where needed; do not reset someone else's external database without authorization. Run fresh SQL Server Up and both CommandDb/QueryDb migrations and verify generated model snapshots.

## Keep
- One Definition version per OwnerAirlineId+StableRef+Version, one current Active version by database constraint if present.
- One Active Pricing revision per Provision; real price rates and component associations with precision and stable tax treatment.
- For nonSeat, one current active/draft/suspended `AncillaryInventoryPolicy` per stable product identity; 3 true resource aggregates with resource uniqueness and optimistic version/concurrency, append-only typed adjustments, SQL overlap locking for airport slots.
- No seat stock tables, SeatMap cache, or seat-price tables separate from generic Pricing.
- Current no mandatory POS, optional SalesRestrictions and correct owner isolation.

## Change
- Drop obsolete FlightFlow-specific enum mapping/branch/provider evidence from runtime schema where actually stored; `Authority` storage supports Unlimited=1/Local=2/Supplier=3. Do not silently renumber existing others.
- Domain and command guards prevent Seat Policy creation; SQL integrity test proves fresh DB contains zero Seat Policy from normal calls. No need to preserve old Seat policies.
- `InventoryCapacityReadState.NotApplicable=8` if chosen must be present in solution-owned enum and serialized consistently. Avoid storing fake Seat InventoryPolicy merely to express the state.
- If no new DB columns/tables are needed after code cleanup, do not create decorative empty migrations. Existing correct migrations still run from a clean DB.

## SQL verification
- SQL-01 clean apply CommandDb migrations; SQL-02 QueryDb apply/projection consistency; SQL-03 nonSeat 100 simultaneous version conflicts produce one winner; SQL-04 correlation idempotency; SQL-05 overlapping AirportSlot race prohibited; SQL-06 seat no-policy/no tables and publish works; SQL-07 current owner isolation; SQL-08 Pricing decimal/tax/fee round trip; SQL-09 two POS unrestricted/restricted semantics; SQL-10 no dangling direct FlightFlow DI registration after application host builds.
