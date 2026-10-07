# v11.0 Phase 1 — Commercial Authoring — Completion Report

`ANCILLARY_V11_PHASE1_AUTHORING_COMPLETE_READY_FOR_OWNER_AUDIT`

Date: 2026-10-08. Authority: `docs/AeroTech-Ancillary-FINAL-v11.0/` (all files read in the order of 09 §1, hashes verified against the manifest). Scope built: Phase 1 only. Nothing of Phase 2 was started.

**One thing needs the owner:** the work is **not committed** (standing owner rule: no commit or push without explicit approval). Everything below is in the working tree on top of the baseline.

## 1. Source control

| Item | Value |
|---|---|
| Repository / branch | `AeroTech.Ancillary` / `k8s-stg` |
| Baseline verified before editing and again at closure | `git rev-parse HEAD` = `e2a8c9f96ec486fbe429e2e1d9947dd83ca3b9c8` |
| New commit SHA(s) | none — uncommitted, awaiting approval |
| Diff against baseline | 51 tracked files modified (`+1829 / −130`), 110 files added, 0 deleted, 0 non-`.cs` files touched |
| Pushed | no |

The v11 pack folder under `docs/` is the owner's and is untracked; no document was created or changed there.

## 2. Files changed

161 `.cs` files. All are CRLF, carry no comments, and follow the namespace style of their neighbours (scripted scan over the 161 files: 0 findings).

| Layer | Files | Content |
|---|---|---|
| Contracts (`Contracts/AeroTech.Messages/Ancillary/Enums`) | 5 | `ProvisionApplicationType` + `Baggage = 2`, `Seat = 3`; new `RoutePairDirection`, `BaggageTravelApplication`, `BaggagePurchaseApplication`, `BaggageRuleDeference`. No other contract folder touched. |
| Domain | 19 | `AncillaryProvision` (typed blocks, `Change`, `Suspend`, `Reactivate`, `Retire`), value objects `PassengerCriteria`, `SalesCriteria`, `TravelCriteria`, `FareCriteria`, `AdvancePurchaseCriteria`, `ProvisionApplication`, `BaggageApplication`, `SeatApplication`; child entity `ProvisionRoutePair`; `ProvisionPriceLine` + `CountryId` / `StationAirportId`; `AncillaryServiceDefinition` (`Change`, `Suspend`, `Reactivate`, `Retire`, `Revise`); `Supplier.Retire`; error codes 16103, 16306; read-model snapshots. |
| Application | 87 | Ten new use-case folders in the existing per-use-case layout: `RetireSupplier`; `Change` / `Suspend` / `Reactivate` / `Retire` / `Revise` `AncillaryServiceDefinition`; `Change` / `Suspend` / `Reactivate` / `Retire` `AncillaryProvision`. Criteria inputs and validators, `ProvisionInputMapper`, result factories, projection factories, DI registrations. |
| Persistence | 6 | `AncillaryProvisionConfiguration`, `ProvisionRoutePairConfiguration`, repository includes, migration + designer + snapshot. |
| Query + Synchronizer | 18 + 2 | Provision read model, route-pair read model, detail DTO and mapper, list filter, definition read model / DTO / mapper, both synchronizers, migration + designer + snapshot. |
| API (`RestApi`) | 6 | Three Backoffice controllers, `DefineProvisionRequest`, new `ChangeProvisionRequest`, `ChangeServiceDefinitionRequest`. |
| Tests | 18 | 13 new test / fixture files; 5 baseline fixture files adjusted so the M1 tests compile against the new `Define` signature (`M1Fixtures`, `M1Commands`, `TestDatabase` EUR seed, `AncillaryScope`, and one line of `M1ProvisionConformanceTests`, which now reads the same expected value through `provision.Application.Type` instead of `provision.ApplicationType`). No expected value of an M1 test was changed. |

Not touched (empty `git diff e2a8c9f` and no untracked file): `Framework/`, `src/AeroTech.Ancillary.ReferenceData`, `ServiceHost`, `Providers`, `Consumers`, every contract folder other than `Ancillary/`.

## 3. Migrations

Normal additive migrations on top of the baseline history.

| Context | Migration | `Up` operations |
|---|---|---|
| `AncillaryDbContext` | `20261007202914_V11Phase1CommercialAuthoring` | 37 `AddColumn` (35 on `AncillaryProvisions`, 2 on `ProvisionPriceLines`), `CreateTable ProvisionRoutePairs`, 1 `CreateIndex` |
| `AncillaryQueryDbContext` | `20261007202918_V11Phase1CommercialAuthoringQuery` | 42 `AddColumn` (`AncillaryProvisions`, `AncillaryProvisionPriceLines`, 2 on `AncillaryServiceDefinitions`), `CreateTable AncillaryProvisionRoutePairs`, 1 `CreateIndex` |

- `ef migrations has-pending-model-changes` on a fresh side build: "No changes have been made to the model since the last migration." for both contexts.
- Applied to the dev database `DotAirAncillary` (`database update` run for all three contexts; `ReferenceDbContext` had nothing pending). History now: command `InitialAncillary` + `V11Phase1CommercialAuthoring`; query `InitialAncillaryQuery` + `V11Phase1CommercialAuthoringQuery`. The three sample provisions are intact (`ApplicationType = 1`, every list column `[]`).
- No table or column of the reservation aggregate appears in either migration (asserted by test).

## 4. Typed criteria implemented

Each member exists in Domain, Application input + validator, command storage, read-model projection, Query model, Backoffice detail DTO and the Draft update path.

| Block | Members |
|---|---|
| `PassengerCriteria` | `PassengerTypeCodes` (`AirPrice.Enums.PassengerTypeCode`) |
| `SalesCriteria` | `PointOfSaleIds`, `CustomerIds`, `CustomerTypes` (`Core.Enums.CustomerType`) |
| `TravelCriteria` | `OriginAirportIds`, `DestinationAirportIds`, `ViaAirportIds`, `TravelFrom`, `TravelTo`, `DaysOfWeek`, `TimeFrom`, `TimeTo`, `MarketingAirlineIds`, `OperatingAirlineIds`, `FlightNumbers`, `FlightIds`, `AircraftIds` |
| `RoutePairs` (child rows) | `OriginAirportId`, `DestinationAirportId`, `Direction` (`Directional` / `BothDirections`) |
| `FareCriteria` | `AirFareIds`, `AirFareTypes` (`AirPrice.Enums.AirFareType`), `FareFamilyIds`, `FareBasisCodes`, `CabinClassIds`, `RbdIds` |
| `AdvancePurchaseCriteria?` | `Period`, `Unit` = `AeroTech.Messages.AirPrice.Enums.TimeUnit` (reused, no Ancillary copy) |
| `ProvisionApplication` | `Type` (`Standard` / `Baggage` / `Seat`), `Baggage?`, `Seat?` |
| `BaggageApplication` | `FreePieces?`, `FirstExcessPiece?`, `LastExcessPiece?`, `Weight decimal(9,2)?`, `WeightUnit` = `AirPrice.Enums.WeightUnit`, `TravelApplication?`, `PurchaseApplication`, `RuleDeference?` |
| `SeatApplication` | `SeatNumbers`, `SeatCharacteristicCodes` (at least one; exact seat numbers require `AircraftIds`) |
| Price line | existing `Category`, `Code`, `Name`, `UnitAmount decimal(18,2)` + `CountryId?`, `StationAirportId?` |

Kept as in the baseline: `Quantity`, `Outcome`, `Fee`, `Settlement`, `Availability`, and the `Fulfillment` value object (untouched semantically). `Sequence` is stored precedence metadata only.

Storage: every list criterion is its own typed EF primitive-collection column (the baseline already stores `CoveredFlightIds` this way); route pairs are child rows. There is no rule document, no EAV table.

Validation is structural only (positive ids, defined enum values, distinct lists, `TravelFrom ≤ TravelTo`, time bounds as a pair, route origin ≠ destination, coherent baggage ranges, seat selectors, Paid / Free / NotAvailable invariants). Nothing fetches a flight, passenger or fare.

## 5. Backoffice lifecycle implemented

```
Suppliers                    POST  | GET Paginated | GET {id} | POST {id}/Retire
AncillaryServiceDefinitions  POST  | GET Paginated | GET {id} | PUT {id} (Draft edit)
                             POST {id}/Activate | Suspend | Reactivate | Retire | Revise
AncillaryProvisions          POST  | GET Paginated | GET {id} | PUT {id} (Draft edit)
                             POST {id}/Activate | Suspend | Reactivate | Retire
```

- Supplier: `Active → Retired`, terminal (second retire → 16103). No Reactivate.
- ServiceDefinition: `Draft → Active` (active supplier required), `Active → Suspended`, `Suspended → Active`, `Draft / Active / Suspended → Retired`. Edit only in Draft (16203 otherwise). `Revise` creates `Version + 1` as a new Draft with a new numeric Id and leaves the source version untouched.
- Provision: same four transitions; edit only in Draft (16303 otherwise), the edit replaces the whole authored content including route pairs and price lines.
- No new status value and no second workflow: the existing `ServiceDefinitionStatus` / `ProvisionStatus` / `SupplierStatus` enums are used as they are.
- List filters: suppliers (owner airline, fulfillment kind, status, search); definitions (owner airline, supplier, reference, sub code, type code, group code, status, search); provisions (service definition, supplier, status, coverage scope, sequence, sales date).
- Industry sub codes are accepted only when `IndustryServiceSubCodeReference` has them (`0BX`, `0CC`); everything else is `CarrierDefined`. No sub-code CRUD or master was added.

## 6. Scenario → test map

Domain tests: `tests/AeroTech.Ancillary.Domain.ConformanceTests/P1*.cs`. Acceptance tests (application services against a real SQL Server database per run): `tests/AeroTech.Ancillary.Application.AcceptanceTests/{Suppliers,ServiceDefinitions,Provisions,Families,Boundary}/P1*.cs`.

| Scenario | Test(s) |
|---|---|
| A01 | not a unit test — sibling repository state, §11 |
| A02 | `P1_A02_the_domain_has_no_evaluation_namespace_or_runtime_matching_type`, `P1_A02_A05_K07_no_evaluator_matcher_simulate_preview_or_bulk_source_exists` |
| A03 | `P1_A03_the_reservation_use_cases_are_still_exactly_hold_get_and_confirm` + `git diff`, §10 |
| A04 | `P1_A04_no_integration_contract_for_another_module_is_added` |
| A05 | `P1_A05_K07_the_rest_api_exposes_exactly_the_authoring_routes_and_the_frozen_service_routes` |
| B01 | `P1_B01_two_suppliers_own_separate_definitions_of_the_same_sub_code`, `P1_PS3_B01_…` |
| B02–B04 | `P1_B02_B03_B04_the_supplier_keeps_its_baseline_shape_without_secrets_or_transport_settings`, `P1_REQ_no_stock_pool_quota_or_supplier_adapter_is_added_outside_the_frozen_baseline` |
| C01 | `P1_C01_an_industry_sub_code_must_exist_in_the_read_only_reference` |
| C02 | `P1_C02_a_carrier_defined_service_authors_its_own_classification` |
| C03 | `P1_C03_activation_and_reactivation_require_an_active_supplier`, `P1_C03_a_definition_of_a_retired_supplier_cannot_be_activated_or_reactivated` |
| C04 | `P1_C04_a_draft_can_be_edited`, `P1_C04_a_definition_that_left_draft_rejects_semantic_mutation`, `P1_C04_an_edit_is_validated_like_a_definition_and_a_refused_edit_changes_nothing`, `P1_C04_an_edit_is_refused_once_the_definition_left_draft_or_when_it_is_malformed`, `P1_C04_C07_a_draft_definition_is_edited_and_every_field_round_trips` |
| C05 | `P1_C05_K06_the_lifecycle_follows_the_existing_status_values_only`, `P1_C05_a_draft_and_a_suspended_definition_can_be_retired_directly`, `P1_C05_K06_the_lifecycle_moves_a_definition_through_the_existing_states`, `P1_C05_a_suspended_version_cannot_be_reactivated_while_another_version_is_active` |
| C06 | `P1_C06_a_revision_is_a_new_draft_with_a_new_id_and_the_next_version`, `P1_C06_only_a_published_definition_can_be_revised_and_the_version_must_grow`, `P1_C06_revise_creates_the_next_version_as_a_new_draft_and_keeps_the_published_version` |
| C07 | `P1_C07_booking_and_document_definitions_hold_every_authored_value`, `P1_C04_C07_…` |
| D01, D02 | `P1_D01_D02_separate_provisions_of_one_definition_carry_the_passenger_type_prices`, `P1_PS1_D01_D02_…`, `P1_PS1_D02_an_infant_can_instead_be_free_through_its_own_provision` |
| D03 | `P1_D03_no_adult_child_or_infant_price_member_exists` |
| D04, E04 | `P1_D04_E04_no_unauthorized_criterion_exists_in_the_provision_model` |
| E01–E03 | `P1_E01_E02_E03_sales_lists_hold_their_values_and_reject_malformed_ones`, `P1_REQ_different_points_of_sale_customers_and_customer_types_carry_their_own_filed_price_and_currency` |
| F01, F03, F06, F08, F09 | `P1_F01_F03_F06_F08_F09_travel_id_lists_hold_their_values_and_reject_malformed_ones` |
| F02 | `P1_F02_route_pairs_are_directional_or_both_directions_between_two_different_airports` |
| F04 | `P1_F04_travel_dates_hold_their_values_and_a_reversed_range_is_refused` |
| F05 | `P1_F05_days_of_week_and_a_time_window_hold_their_values_and_the_window_needs_both_bounds` |
| F07 | `P1_F07_flight_numbers_are_stored_trimmed_and_upper_cased` |
| G01–G06 | `P1_G01_G06_every_fare_dimension_holds_its_values_and_they_coexist_in_one_provision` |
| G07 | `P1_G07_a_fare_family_is_a_numeric_identity` |
| H01–H03 | `P1_H01_an_advance_purchase_period_must_be_positive`, `P1_H02_the_advance_purchase_unit_is_the_canonical_air_price_time_unit`, `P1_H03_the_advance_purchase_block_is_data_without_eligibility_arithmetic` |
| I01, I02 | `P1_I01_a_paid_provision_requires_a_fee_and_at_least_one_price_line`, `P1_I02_a_free_or_not_available_provision_cannot_carry_a_payable_amount` |
| I03 | `P1_I03_amounts_are_stored_as_decimal_18_2_and_the_baggage_weight_as_decimal_9_2` |
| I04–I06 | `P1_I04_a_price_line_can_carry_code_country_and_station_evidence`, `P1_I05_the_price_model_has_one_filed_currency_and_no_conversion_member`, `P1_I06_a_fee_unit_without_a_frozen_formula_can_be_authored_but_not_activated` |
| J01–J05 | `P1_J01_a_standard_application_has_no_family_specific_child_object`, `P1_J02_the_baggage_application_holds_exactly_the_typed_descriptors`, `P1_J02_every_typed_baggage_descriptor_is_stored_and_none_blocks_activation`, `P1_J03_a_seat_application_requires_seat_numbers_or_characteristic_codes`, `P1_J04_exact_seat_numbers_require_aircraft_ids`, `P1_J05_the_seat_application_stores_no_live_seat_state` |
| K01 | `P1_K01_K06_a_supplier_is_retired_once_and_stays_listed_and_readable` (+ baseline `M1PaginatedAcceptanceTests`) |
| K02 | `P1_K02_definitions_are_filtered_by_status_supplier_sub_code_group_and_name` |
| K03 | `P1_K03_provisions_of_a_definition_are_filtered_by_status_coverage_sequence_and_sales_date` |
| K04 | `P1_K04_every_typed_criterion_round_trips_through_the_command_store`, `P1_K04_the_backoffice_detail_returns_every_typed_criterion`, `P1_K04_a_provision_without_criteria_round_trips_as_unrestricted`, `P1_REQ_criteria_are_typed_columns_in_both_stores_and_no_generic_rule_document_exists` |
| K05 | `P1_K05_a_draft_update_round_trips_every_field`, `P1_K05_a_draft_is_replaced_as_a_whole_by_a_change`, `P1_K05_a_provision_that_left_draft_is_immutable`, `P1_K05_a_refused_change_leaves_the_draft_untouched`, `P1_K05_an_edit_is_refused_once_the_provision_left_draft_or_when_it_is_malformed` |
| K06 | `P1_K06_the_lifecycle_moves_a_provision_through_the_existing_states`, `P1_K06_the_provision_lifecycle_uses_only_the_existing_status_values`, `P1_K06_a_draft_and_a_suspended_provision_can_be_retired_directly`, `P1_K06_a_suspended_provision_cannot_be_reactivated_while_its_sequence_has_another_active_row`, `P1_K06_a_supplier_is_retired_once_and_retired_is_terminal` |
| K07 | `P1_A02_A05_K07_…`, `P1_A05_K07_…` |

Family proof — `Families/P1FamilyAcceptanceTests.cs`. Every test runs the full rule through `FamilyProof.ProveAsync`: create Draft definition and provision → read both details → both appear in the paginated lists → edit the Draft definition and the Draft provision and read the edited fields back → activate both → read the Active state (detail, list filtered by Active, read-model row). No test calls Hold or Confirm.

| Family | Test |
|---|---|
| FAM01 | `P1_FAM01_extra_prepaid_baggage_is_authored_as_piece_and_weight_packages_with_every_outcome` |
| FAM02 | `P1_FAM02_sports_and_special_baggage_are_separate_definitions_with_fixed_prices` |
| FAM03 | `P1_FAM03_wheelchair_assistance_uses_ssr_booking_metadata_with_free_and_paid_configurations` |
| FAM04 | `P1_FAM04_meals_are_separate_definitions_priced_by_passenger_type_flight_date_cabin_and_fare_family` |
| FAM05 | `P1_FAM05_insurance_plans_are_separate_definitions_with_journey_and_order_coverage` |
| FAM06 | `P1_FAM06_paid_seat_rules_are_authored_by_aircraft_seat_number_and_characteristic` |
| FAM07 | `P1_FAM07_airport_lounge_is_defined_by_several_suppliers_with_their_own_prices` |
| FAM08 | `P1_FAM08_priority_boarding_is_paid_free_or_not_available_by_fare_family_cabin_passenger_and_point_of_sale` |
| FAM09 | `P1_FAM09_fast_track_is_authored_by_airport_route_date_and_point_of_sale` |
| FAM10 | `P1_FAM10_wifi_is_priced_by_aircraft_flight_cabin_and_fare_family` |
| FAM11 | `P1_FAM11_pet_service_carries_booking_metadata_with_sector_and_journey_prices` |
| FAM12 | `P1_FAM12_meet_and_assist_is_authored_by_airport_route_passenger_point_of_sale_and_date_for_two_suppliers` |
| FAM13 | `P1_FAM13_unaccompanied_minor_handling_uses_passenger_type_and_booking_metadata_without_age_fields` |

Price-stress fixtures — `Families/P1PriceStressAcceptanceTests.cs`.

| Fixture (06) | Test |
|---|---|
| 1. Lounge ADT 25 EUR / CHD 15 EUR / INF NotAvailable | `P1_PS1_D01_D02_lounge_is_25_eur_for_an_adult_15_eur_for_a_child_and_not_available_for_an_infant` |
| 2. Baggage 30 / 25 / 20 EUR | `P1_PS2_baggage_has_a_flight_and_date_rule_a_route_and_fare_family_rule_and_a_default_rule` |
| 3. Two lounge suppliers | `P1_PS3_B01_two_lounge_suppliers_have_distinct_definition_ids_and_prices` |
| 4. Meal by PTC and cabin | `P1_PS4_a_meal_price_differs_by_passenger_type_and_cabin` |
| 5. Insurance by PTC and travel window | `P1_PS5_an_insurance_plan_price_differs_by_passenger_type_and_travel_window` |
| 6. Seat by aircraft and characteristic | `P1_PS6_a_seat_rule_differs_by_aircraft_and_characteristic` |
| 11 "Pricing scenarios" (dates, flights, routes, fare families, cabins, RBDs; POS, customer, customer type) | `P1_REQ_different_travel_dates_flights_routes_fare_families_cabins_and_rbds_carry_their_own_filed_price`, `P1_REQ_different_points_of_sale_customers_and_customer_types_carry_their_own_filed_price_and_currency` |

## 7. Build

```
dotnet build AeroTech.Ancillary.sln -v q --nologo --no-incremental
    0 Error(s)   13 Warning(s)
```

All 13 warnings are pre-existing and outside Ancillary-owned code: `Framework.Infrastructure.csproj` NU1510, `Framework.Core` `EnumTools.cs` CS8604 and `AuthorizationContextScopeKey.cs` CS0618, and the shared payment / wallet message files in `Contracts/AeroTech.Messages` (CS0108, CS8907). No warning points at `Contracts/AeroTech.Messages/Ancillary`, `src/AeroTech.Ancillary.*` or `tests/`.

## 8. Tests

```
dotnet test tests/AeroTech.Ancillary.Domain.ConformanceTests --no-build
    Passed!  Failed: 0, Passed: 92, Skipped: 0, Total: 92      (discovered by --list-tests: 92)
dotnet test tests/AeroTech.Ancillary.Application.AcceptanceTests --no-build
    Passed!  Failed: 0, Passed: 76, Skipped: 0, Total: 76      (discovered by --list-tests: 76)
```

No "aborted" or "crash" line in either run. Domain: 36 baseline M1 cases + 56 P1 cases. Acceptance: 26 baseline M1 tests + 50 P1 tests (3 supplier, 7 definition, 11 provision, 13 family, 9 price, 7 boundary). The per-run test database was dropped afterwards (only `DotAirAncillary` remains on the server).

## 9. Source scan — no evaluator

Case-insensitive scan of every `.cs` under `src/` and `Contracts/AeroTech.Messages/Ancillary` (excluding `obj` / `bin`):

| Terms | Hits |
|---|---|
| `AncillaryEvaluation`, `Evaluator`, `Matcher`, `EvaluationContext`, `FlightContext`, `FareContext`, `EvaluateAncillar`, `RuleEngine`, `Simulat`, `Preview`, `Bulk` | 0 (file content, file names, folder names) |
| `StockPool`, `Quota`, `Adapter` | only the baseline frozen `AncillaryReservationUnit.cs` and the EF snapshots of that baseline column; 0 elsewhere |
| age / date of birth / frequent flyer / loyalty / tier / occurrence / PCC | 0 (one unrelated baseline hit: outbox `TimeOfOccurrence`) |
| `AdultPrice` / `ChildPrice` / `InfantPrice` (and `…Amount`) | 0 |
| dynamic / formula / pricing provider / exchange rate / ROE / currency conversion | 0 |
| a second `TimeUnit` / `WeightUnit` / `PassengerTypeCode` / `AirFareType` / `CustomerType` enum, or an airport / airline / currency / fare-family / cabin / RBD / aircraft master class outside the baseline ReferenceData module | 0 |
| JSON rule document columns (`ToJson`, `json` column type, `RuleJson`, `CriteriaJson`) | 0 |

The same rules are fixed as tests (`P1_A02_…` ×2, `P1_REQ_no_stock_pool_…`, `P1_REQ_no_family_specific_aggregate_…`, `P1_D03_…`, `P1_D04_E04_…`). Domain aggregates are still exactly `Supplier`, `AncillaryServiceDefinition`, `AncillaryProvision`, `AncillaryReservation`. `src/AeroTech.Ancillary.Providers` still contains only `DependencyInjection.cs`.

## 10. Reservation code unchanged

```
git diff e2a8c9f --stat -- '*AncillaryReservationAggregate*'      → empty
git status --short -- '*AncillaryReservationAggregate*'           → empty
git diff e2a8c9f --stat -- reservation test files (Holds/, *Reservation*)  → empty
```

No file under `**/AncillaryReservationAggregate/**` or `**/RestApi/**/AncillaryReservationAggregate/**` was modified or added. The frozen Service routes are the baseline three (`POST Service-Holds`, `GET Service-Holds/{holdId}`, `POST Service-Holds/{holdId}/Confirmations`). The baseline Hold / Get / Confirm tests (13 domain, plus the acceptance hold tests) are unchanged and pass. No Cancel / Release / Issue / Split / History was added.

## 11. No other repository changed

Checked at closure. "Newer" = files modified after the v11 prompt file arrived (2026-10-07 23:41), excluding `.git`, `bin`, `obj`.

| Repository | Checkout | Uncommitted paths | Files newer than the v11 prompt |
|---|---|---|---|
| AirAvail | `fix/offerId@318f7be` | 48 (the earlier M1 work, unchanged) | 0 |
| AeroTech.Ordering.Final | `k8s-stg@cd50a2a` | 2 (old) | 0 |
| FlightFlow | `k8s-stg@d2180b2e` | 0 | 0 |
| AirPrice | `k8s-stg@1b41f08` | 0 | 0 |
| Core | `k8s-stg@f8204f7` | 0 | 0 |

Across every folder of `E:\Projects\DotAir` the only one with newer files is `AeroTech.Backoffice.V1` (165 files, all under `apps/backoffice-web/.next/dev`, the cache of a running frontend dev server; `git status` there is clean). Nothing was written there by this work.

## 12. Runtime smoke

The side-built host was started on port 5299 in Development and stopped again (port free before and after).

- Startup completed without an error line; dependency injection resolved.
- OpenAPI lists 35 operations: the 21 Backoffice routes of §5, the 3 frozen Service routes, 10 baseline Syncer routes and Ping.
- `GET api/v1/Ping` → 200; Backoffice calls without a token → 401.

Authenticated HTTP calls were **not** made: the stored Backoffice token is expired. The authoring proof therefore rests on the acceptance tests (application services, real SQL Server), not on live HTTP records.

## 13. Remaining gaps and decisions for the audit

Points where v11 is silent or where the baseline was preferred. None was resolved by adding scope.

1. **Commit.** Uncommitted on `e2a8c9f`; exit criterion "exact commit SHA" can only be completed after the owner approves a commit.
2. **Provision list needs `ServiceDefinitionId`.** 03 §5 marks it optional; the baseline test `M1PaginatedAcceptanceTests` asserts that a list without it returns no rows, so the baseline behaviour was kept. Making it optional is a one-line change plus that baseline assertion.
3. **Provision list columns unchanged.** The baseline test fixes the grid column set, so the typed criteria are returned by the detail, not as list columns. Only the `CoverageScope` filter was added.
4. **Request shape.** Price lines stay nested as `fee.priceLines` and the `fulfillment` block stays, as in the baseline contract; 03 §5 lists `priceLines` as its own block.
5. **Draft edit.** `PUT {id}` (sibling convention) replaces the whole Draft. The definition edit cannot change `OwnerAirlineId`, `ServiceDefinitionRef` or `Version`; the provision edit cannot change `ServiceDefinitionId`.
6. **Revise.** Allowed from Active or Suspended only. It copies the definition, not its provisions (they point at the old numeric Id). Activating the new version while the old one is Active is refused by the baseline rule 16204, so the analyst retires or suspends the old version first. v11 does not describe the switch-over or provision carry-over.
7. **Reactivate.** A definition needs an Active supplier (16206) and no other Active version (16204). A provision is refused when another Active row holds the same sequence (16306); it does not supersede it.
8. **No cascade.** Retiring a supplier or suspending / retiring a definition does not change the status of its definitions / provisions. v11 defines no cascade.
9. **Baggage descriptors** are stored and structurally validated only; none blocks activation. No tax-code-required rule exists on price lines (v11 has none).
10. **Read-model lifecycle timestamps.** The provision read model gained `ActivatedAt` / `SuspendedAt` / `RetiredAt` (the definition read model `SuspendedAt` / `RetiredAt`). For rows activated before the migration the new `ActivatedAt` is null until the row is projected again — true for the three dev sample provisions. No backfill was written because the command and query stores have separate connection strings.
11. **Lengths taken from sibling source**, as v11 gives none: flight number 16, fare basis 64, seat number 16 (Ordering), seat characteristic code 25 (Basic).
12. **Fixture codes are sample data.** Carrier-defined sub codes, group codes and seat characteristic codes in the tests are illustrative carrier classifications. The SSR codes used (WCHR, WCHS, WCHC, VGML, CHML, SPML, PETC, AVIH, UMNR, MAAS, SPEQ, XBAG) are IATA codes from recollection, not checked against a licensed source. Industry sub codes are only the two reference rows `0BX` and `0CC`.
13. **No live HTTP proof** (expired token, §12). A fresh Backoffice token is needed if live records are wanted.

Phase 1 stops here.
