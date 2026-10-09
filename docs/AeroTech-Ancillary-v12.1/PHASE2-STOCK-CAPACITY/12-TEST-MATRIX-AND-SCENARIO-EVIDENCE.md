# Mandatory scenario matrix and closure protocol

> **Project authority:** AeroTech Ancillary **v12.1 — Phase 2: Stock & Capacity**. Continuation of the approved Phase 1 v12.1; not a separately versioned specification. Phase 3 remains closed.


## 62 scenario IDs from approved Pack 06
Do not mark IDs as passed without executing actual tests. Reuse all 62 unique IDs verbatim from `06-SCENARIO-MATRIX-AND-EXIT-GATE.md`. Every ID must have a test name or an EXPLICIT `DEFERRED_PHASE3`, `DEFERRED_PROVIDER_EVIDENCE`, or `DEFERRED_ASSET_EVIDENCE` entry and rationale; no untracked drop.

| Family | IDs | Phase2 expectation |
|---|---|---|
| Configuration / ownership | C01-C07 | mandatory, except C04 no remote availability adapter; assert no mirror |
| Flight count | F01-F10 | mandatory with domain + SQL uniqueness, F10 asserts Provision remains owner |
| Weight | W01-W06 | W01-W05 mandatory; W06 deferred Phase3 operational obligations |
| Airport slots | A01-A08 | mandatory authoring, race, half-open ranges and timezone; A03 actual multi-slot occupancy deferred Phase3 |
| Daily count | D01-D04 | domain schema contract mandatory; persistence/runtime conditional provider evidence |
| Hotel room night | H01-H07 | domain schema contract mandatory; persistence/runtime conditional verified local allotment; booking H02/H03 Phase3 |
| Passenger usage | P01-P05 | policy/identity contract mandatory; cross-order real enforcement deferred Phase3 |
| Assigned asset | R01-R03 | design only, no runtime until operator evidence |
| Cross-cutting | X01-X12 | mandatory where Phase2; X04 obligations deferred Phase3, X12 delegated no-mirror guard |

## Strong test methods (suggested exact names)
- `P2_C02_unmapped_legacy_product_is_not_configured_not_unlimited`
- `P2_C01_unlimited_policy_creates_no_stock_rows`
- `P2_F02_pet_products_share_one_physical_flight_count_key`
- `P2_F09_meal_variants_share_one_catering_quota`
- `P2_W01_fixed_5kg_bundle_uses_5kg_in_binding_even_when_price_per_item`
- `P2_W04_count_and_weight_binding_is_closed_two_resource_contract`
- `P2_A04_concurrent_overlapping_facility_slot_creation_has_single_winner_sql`
- `P2_A05_adjacent_utc_slots_are_accepted`
- `P2_A06_dst_local_time_must_have_authoritative_timezone_and_utc_resolution`
- `P2_D03_date_range_authoring_must_map_to_independent_daily_capacity_buckets` (only if local Daily activated)
- `P2_H04_same_room_type_different_rate_plans_have_one_room_night_stock` (domain/persistence only if local hotel signed)
- `P2_P02_two_orders_same_traveller_require_stable_identity` (defer actual enforcement)
- `P2_X01_same_expected_version_capacity_adjustment_has_one_winner_sql`
- `P2_X07_no_cross_repository_or_phase3_changes`
- `P2_X08_snapshot_cannot_claim_booking_guarantee`
- `P2_X09_legacy_definition_authority_is_not_inferred`

## Fail-fast stress tests
1. 100 parallel independent writes to SAME existing FlightCount total with identical expected version: one commit, 99 stable conflicts; exactly one adjustment row.
2. Same for FlightWeight `decimal(18,3)`: no double adjustment and exact precision.
3. 100 concurrent AirportSlot overlapping-but-different `[start,end)` creations in SAME facility: one success, rest conflict; database MUST prove no overlapping live intervals.
4. 100 simultaneous distinct FlightId sources: all accepted, no global lock bottleneck.
5. 100 concurrent duplicated Policy activation for same `(OwnerAirlineId,ServiceDefinitionRef)`: at most one current active.
6. Kill or retry adjustment after ambiguous response: stable correlation key should allow idempotent replay of same payload; mismatching payload conflicts, no duplicate ledger row.
7. Query projection parity: fields, timestamps, effective statuses and adjustment counts equal command DB; compare after save, not eventually assumed.
8. Phase1 regression: domain + acceptance tests, including original M1 Hold/Confirm, with unchanged reservation file checksums.

## Phase3-specific tests — intentionally NEVER claim completed now
- 100 concurrent flight holds for quota 10 -> <=10 held/confirmed.
- 50 parallel 5kg holds for weight 100kg -> <=100kg leased.
- CountPlusWeight partial failure -> no leaked holds.
- 90-minute lounge occupancy locks all intersecting slots atomically.
- Multi-night hotel stay atomically consumes each night or fails entirely.
- passenger cross-order usage counters with stable beneficiary identity.
- concurrent confirm-vs-expire, retry, release exactly once.
- supplier reservation race and stale snapshot / FlightFlow seat provider checks.

## Evidence requirement
Agent must output a scenario table of all 62 IDs, `test class.method`, `executed PASS/FAIL/DEFERRED`, and why deferred; actual `dotnet test` command, pass/fail/skip counts, migrations names, SQL unique/transaction evidence, build log; no claim of 62 passing tests if some deferred.
