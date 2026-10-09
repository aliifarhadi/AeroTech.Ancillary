# AeroTech Ancillary v12.1 — Phase 1 + Phase 2 final completion report

Date: 2026-10-09. Authority: `docs/AeroTech-Ancillary-v12.1/` files 00–10 (FINAL IMPLEMENTATION AUTHORITY). Companion: `reports/V121-P1P2-FINAL-GAP-REPORT.md`.

## 1. Verdicts

| Subject | Verdict |
|---|---|
| Domain (Phase 1 pricing + descriptors, Phase 2 corrections) | **PASS** |
| Local operational evidence (Local FlightCount / FlightWeight / AirportSlot, FlightFlow delegation) | **BLOCKED** — no source of truth is connected (D11 kept); proven only with test-supplied references |
| Production limited-stock sell | **NOT_A_PHASE2_GATE** |

`ANCILLARY_V12_1_PHASE1_PHASE2_DOMAIN_READY_FOR_OWNER_AUDIT`

Nothing is committed, pushed or merged. Phase 3 was not started. No other repository was read for this work or written to.

## 2. Baseline and scope

- Branch `feat/ancillary-v12.1-phase2-stock`, HEAD `1abf7a53e0efb9eb892097977327718c61486158` (verified with `git rev-parse HEAD`); base `k8s-stg@933b7b7`. All work is in the working tree: 110 modified, 7 deleted and 56 untracked paths (`git status --short`); 38 of the untracked paths are under `src/`, `tests/` and `Contracts/`, the other 18 are the Owner's pack files in `docs/` and the two reports of this work.
- Frozen: `**/AncillaryReservationAggregate/**` and the two M1 reservation test files — 36 files, combined hash `6974ABA2C62BAFE7CDE159BDCE8A4969F1CAECCF25368012F30D1BE7DDA4E486`, unchanged (`git status` shows no entry under those paths; tests `V12_C04_…byte_stable` and `X20_…byte_stable` recompute the hash).
- Not touched: `Framework/`, the ReferenceData module, contract folders other than `Contracts/AeroTech.Messages/Ancillary/`, documents in `docs/`.

## 3. Commands and results

| Command | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln -v q --nologo --no-incremental` | 0 errors, 13 warnings — the same count as the base build: 11 in Framework and in other services' contract files, 2 NuGet NU1510; none in a file changed here |
| `dotnet test tests/AeroTech.Ancillary.Domain.ConformanceTests --no-build` | 232 discovered (`--list-tests`), 232 executed, 232 passed; no aborted run |
| `dotnet test tests/AeroTech.Ancillary.Application.AcceptanceTests --no-build` (SQL Server Express, one database per run plus one per migration class) | 246 discovered, 246 executed, 246 passed; no aborted run |
| `ef migrations has-pending-model-changes` for `AncillaryDbContext` and `AncillaryQueryDbContext` | "No changes have been made to the model since the last migration." for both |
| `ef database update` for both contexts on dev `DotAirAncillary` | four migrations applied, see section 5 |

Baseline at `1abf7a5` before this work: build 0 errors, domain 200/200, acceptance 153/153 (gap report section 1).

One instability was seen and is reported as it happened: the existing test `V121_M02_dates_and_seasons_keep_their_intersection_dates_alone_or_seasons_alone_keep_their_union_and_nothing_is_widened` failed three times with a SQL command timeout (its date-expansion query against the legacy tables exceeds the 30 s default) while the machine was heavily loaded by other processes; a full acceptance run then took between 4 and more than 60 minutes. It passes alone (28 s) and in the final full run quoted above (2.2 minutes). The query was not changed by this work.

## 4. What changed

### 4.1 Pricing (D01–D07, D13)

- `AncillaryPricing` stays the root (Status, Version, AncillaryProvisionId, PricingUnit, one Active per provision). `AncillaryPricingLine` is replaced by `AncillaryPricingRate` (selector = CurrencyId + optional PassengerTypeCode / age band; base price) and 0..N `AncillaryPriceComponent` (Category Tax | Fee, Code, Name, CountryId, StationAirportId, `Money` amount, `FeeApplicationUnit` on Fee only, `TaxIncludedInSource` informational).
- `Money(Amount, CurrencyId)`: amount ≥ 0, at most six decimals, stored `decimal(19,6)`; addition refuses another currency. Every amount is checked against `CurrencyReadModel.DecimalPlaces` through `ICurrencyReference` at Define / Change and again at Activate / Reactivate / Switch / Publish; an unknown currency is refused.
- Several currencies live in one Active pricing; there is no root currency, no FX, no point of sale on a rate (D04: one provision per point of sale).
- Non-passenger units file exactly one rate per currency; PerPassenger rates are keyed by PTC / age band with disjoint bands per currency.
- Unit total = base + taxes + fees whose unit is `Item`. Fees per Ticket / OneWay / RoundTrip / SectorOrPortion are returned as `UnappliedFees` and `IsUnitTotalComplete = false`; percentage and per-kilogram fee units cannot be published (16305).
- New errors: 16511 currency not found, 16512 amount scale not allowed, 16513 component currency mismatch, 16514 flat base line not supported, 16515 fee application unit required (all 422).
- Backoffice contract (breaking, documented): `DefinePricingRequest(AncillaryProvisionId, Rates)`, `ChangePricingRequest(Rates)`; read DTO `BackofficePricingDto(…, Currencies, Rates[BasePrice, Components, UnitTotal, IsUnitTotalComplete, UnappliedFees])`; list columns Version, Pricing Unit, Currencies, Rates, Status, Created, Activated.

### 4.2 Provision and booking descriptors (doc 04)

- `BookingDefinition.ConfirmationRequirement` — `Immediate` (default) | `SubjectToConfirmation`.
- `AncillaryProvision.PurchaseStage` — `PreOrder` | `PostTicketed` | `Both`; `LegacyUnspecified` exists only as the value of migrated rows. It is refused as authoring input (16302) and a row that still carries it cannot be published (16316, 409).
- `ProvisionAdvancePurchaseRule.MaximumPeriod?` — same unit as the minimum, never below it (16302).
- `ProvisionBaggageApplicationRule.ChargeKind` (`ExtraPiece`, `WeightPackage`, `Overweight`, `Oversize`, `SpecialEquipment`) and optional `AllowanceConcept` (`Piece`, `Weight`): a weight package requires the Weight concept and a weight, an extra piece requires the Piece concept (16302); a baggage rule without a charge kind cannot be published (16317, 409). No StepQuantity.
- The ten rule groups and their 27 child row types are unchanged; the four descriptors flow through commands, validators (shape only), snapshots, read models, DTOs, mappers and synchronizers.

### 4.3 Phase 2 corrections (doc 05)

- D10: `EnsureUnlimitedIsCredible` is removed. An Unlimited policy activates whatever `MustCheckAvailability` says; the configuration snapshot now returns `RequiresAvailabilityCheck` beside `IsGuaranteed` (always false).
- Policy identity: activation resolves the current definition by `(OwnerAirlineId, ServiceDefinitionRef)` — the Active version, otherwise the highest non-retired version — and refuses when none exists (16617). `ServiceDefinitionId` on the policy is reconciled to that version at activation and is no longer the identity that is checked.
- D11 unchanged: the five reference ports are still not connected in the host, so FlightFlow and Local policies and every local source stop at 16608 `SourceUnavailable` outside tests. D12 unchanged: DailyCount, RoomNight and AssignedAsset cannot be activated (16607).
- No new capacity model, no allocation, no Hold / Confirm / Release.

## 5. Migrations and the development database

| Context | Migration | Content |
|---|---|---|
| Command | `20261009124006_V121FinalPricingRates` | create `AncillaryPricingRates`, `AncillaryPriceComponents` and their unique keys; copy existing lines (rate id = base line id, component id = tax / fee line id, root currency copied, root fee unit to Fee components only); drop `AncillaryPricingLines` and the root `CurrencyId`, `FeeApplicationUnit` |
| Query | `20261009124011_V121FinalPricingRatesQuery` | the same for the read model |
| Command | `20261009131917_V121FinalProvisionDescriptors` | `AncillaryProvisions.PurchaseStage` (existing rows = 4 LegacyUnspecified), `AncillaryServiceDefinitions.BookingConfirmationRequirement` (existing rows = 1 Immediate), `ProvisionAdvancePurchaseRules.MaximumPeriod`, `ProvisionBaggageApplicationRules.ChargeKind`, `.AllowanceConcept` |
| Query | `20261009132022_V121FinalProvisionDescriptorsQuery` | the same five columns on the read model |

On the Owner's instruction of 2026-10-09 ("all database data is test data — delete it instead of repairing it") the migration is plain: no guards, no retained legacy tables, no clone-and-reconcile run, and the `Down` methods restore schema only. This replaces steps M2–M3 and M5 of doc 08 as written.

Dev `DotAirAncillary` (local SQL Express), verified by query after the run:

- Backup taken first: `DotAirAncillary_before_final_wipe.bak` (default backup folder). Then every row of schemas `Ancillary` and `ReadModel` was deleted (759 and 538 rows; the 31 definitions / 99 provisions / 69 pricings of the sample catalog are gone). `ReferenceData` kept: 415 rows.
- Migration history ends at the four migrations above; `Ancillary` 54 tables, `ReadModel` 41 tables, 0 rows. `ReferenceData` unchanged at `20260923151353_AddCountries`.
- The existing legacy cleanup (`V121LegacySchemaCleanup`, D14) was reviewed inside this repository only and left as committed.

Migration behaviour on data is still proven by tests on seeded legacy databases: `V121_M04_M06_a_v11_database_migrated_through_v12_to_v121_keeps_every_selector_price_line_status_and_row_identity`, `PR23_…` (both schemas), `V121_M05_…leaves_the_database_exactly_as_the_model`, `V121_M12_…`.

## 6. Scenario index

Status values are from the runs in section 3.

### 6.1 Pricing PR01–PR25

| ID | Test (D = `V121FinalPricingConformanceTests`, A = `V121FinalPricingAcceptanceTests`) | Status |
|---|---|---|
| PR01 | D `PR01_one_rate_is_a_base_price_plus_its_own_tax_and_fee_in_one_currency`; A `PR01_PR02_one_active_price_keeps_every_currency_as_its_own_authored_rate_without_a_root_currency_or_a_conversion` | PASS |
| PR02, PR03 | D `PR02_PR03_two_currencies_are_two_authored_alternatives_and_a_currency_that_was_not_authored_is_never_converted` (PR03 by the test-only rate oracle: no shopping engine exists); A as PR01 | PASS |
| PR04–PR07 | D `PR04_PR05_PR06_PR07_an_amount_keeps_the_scale_of_its_currency_and_is_never_rounded`, `PR04_a_currency_that_the_reference_does_not_know_and_a_scale_beyond_storage_are_refused`; A `PR04_PR05_PR06_PR07_amounts_of_zero_two_and_three_decimal_currencies_are_stored_exactly_and_a_finer_scale_is_refused` | PASS |
| PR08 | D `PR08_a_component_in_another_currency_than_its_rate_is_refused`; A `PR08_PR09_PR13_…_unique_in_sql` | PASS |
| PR09–PR11 | D `PR09_PR10_PR11_a_rate_key_is_unique_and_age_bands_of_one_currency_and_passenger_type_never_overlap`; A `PR08_PR09_PR13_…` (unique index + duplicate insert refused by SQL error 2601) | PASS |
| PR12, PR13 | D `PR12_PR13_a_non_passenger_unit_files_exactly_one_rate_per_currency_and_no_passenger_selector`; A `PR08_PR09_PR13_…` | PASS |
| PR14 | D `PR14_a_component_is_unique_by_category_code_country_station_and_fee_unit_inside_its_rate` | PASS |
| PR15, PR22 | D `PR15_PR22_a_fee_per_ticket_stays_outside_the_unit_total_and_is_reported_as_unapplied`; A `PR15_PR22_the_read_model_returns_base_components_unit_total_and_the_fees_it_did_not_apply` | PASS |
| PR16 | D `PR16_a_tax_never_has_a_fee_unit_and_a_fee_is_not_published_without_one` | PASS |
| PR17 | D `PR17_a_paid_base_is_positive_and_no_amount_is_negative` | PASS |
| PR18, PR19 | A `PR18_PR19_a_paid_provision_is_not_published_without_an_active_price_and_a_free_one_never_takes_a_price` | PASS |
| PR20 | D `PR20_an_active_revision_is_never_edited_and_a_revision_copies_every_rate_into_a_new_draft`; A existing `V12_P08_the_active_price_is_switched_atomically_and_a_stale_expectation_conflicts` | PASS |
| PR21 | A `PR21_two_concurrent_price_activations_have_one_winner_behind_the_filtered_unique_index` | PASS |
| PR23 | A `PR23_every_legacy_flat_price_keeps_its_currency_and_its_base_components_and_total_after_the_migration` (Ancillary, ReadModel) | PASS |
| PR24, PR25 | D `PR24_PR25_the_flat_base_category_and_percentage_fee_units_are_refused_explicitly`; A `PR24_the_backoffice_contract_has_no_flat_price_line_or_root_currency_and_a_payload_without_rates_is_refused`, `PR24_PR25_a_flat_base_component_and_a_percentage_fee_unit_are_refused_by_code_and_never_reinterpreted` | PASS |

### 6.2 Families F01–F15 (60 checks)

`V121FinalFamilyAcceptanceTests`: four theories, each run once per family id (`family: "F01"` … `"F15"`):

- `Fxx_DEF_the_service_definition_states_its_unit_date_basis_booking_and_confirmation_and_refuses_another_quantity_unit`
- `Fxx_RULE_the_published_provision_keeps_its_stage_outcome_limits_and_typed_rules`
- `Fxx_PRICE_a_paid_rule_round_trips_every_rate_as_amount_and_currency_and_a_free_rule_has_no_price`
- `Fxx_POLICY_the_inventory_authority_is_authored_without_an_invented_quota_and_is_never_a_guarantee`

| # | Product authored | Rule highlights | Price | Policy and what the test observed | DEF / RULE / PRICE / POLICY |
|---|---|---|---|---|---|
| F01 | `XBAG_WEIGHT_10KG`, PerItem | Both, qty 1–2, route, advance 6–720 h, WeightPackage / Weight 10 kg | EUR 25, USD 27.5 | Unlimited: Active, state Unlimited, not guaranteed, 0 source rows | PASS / PASS / PASS / PASS |
| F02 | `XBAG_PIECE_23KG`, PerPiece, SSR XBAG | flight, fare family, pieces 1–3, ExtraPiece / Piece | EUR 30 + tax 2.7 + fee 1 (Item) = 33.7 | Unlimited, 0 source rows | PASS / PASS / PASS / PASS |
| F03 | `XBAG_OVERWEIGHT`, PerItem | aircraft, Overweight, 32 kg | USD 60 | Unlimited, 0 source rows | PASS / PASS / PASS / PASS |
| F04 | `SPORT_BICYCLE`, PerPiece, SSR BIKE, SubjectToConfirmation | PreOrder, advance 48 h, SpecialEquipment, must check | EUR 50 | Supplier `GroundHandlerA`: Active, DelegatedCheckRequired, check required | PASS / PASS / PASS / PASS |
| F05 | `PET_IN_CABIN`, PerItem, SSR PETC, SubjectToConfirmation | route both directions, booking required, must check | EUR 55 | Local FlightCount: 16608 without a source; with a test-supplied resource Active, no source row → NotConfigured | PASS / PASS / PASS / PASS |
| F06 | `SEAT_EXTRA_LEGROOM`, PerSeat | aircraft, seat 18A, characteristic L | EUR 18 | FlightFlow: 16608 without delegation; with test-supplied delegation Active, DelegatedCheckRequired, no local row | PASS / PASS / PASS / PASS |
| F07 | `MEAL_VGML`, PerPassenger, SSR VGML | Free, ADT / CHD, advance 24 h | none; a price is refused (16505) | Unlimited | PASS / PASS / PASS / PASS |
| F08 | `ASSIST_WCHR`, PerPassenger, SSR WCHR, SubjectToConfirmation | Free, booking required, must check | none (16505) | Unlimited + must check: Active, check required, not guaranteed | PASS / PASS / PASS / PASS |
| F09 | `UMNR_SERVICE`, PerPassenger, SSR UMNR, SubjectToConfirmation | PreOrder, age 5–12, must check | EUR 60 (5–8), EUR 50 (8–12) | Unlimited, check required | PASS / PASS / PASS / PASS |
| F10 | `PRIORITY_BOARDING`, PerPassenger | PostTicketed, fare families, blackout | EUR 8, GBP 7 | Unlimited | PASS / PASS / PASS / PASS |
| F11 | `LOUNGE_IKA`, PerPassenger, ServiceStart | ADT / CHD, service airport, daily window | ADT EUR 30, CHD EUR 15 | Supplier `LoungePartnerA`: DelegatedCheckRequired | PASS / PASS / PASS / PASS |
| F12 | `CIP_IKA`, PerPassenger, ServiceStart | service airport, advance 12 h | EUR 80, USD 88 | Local AirportSlot: 16608 without a facility source; with a test-supplied facility Active, no slot → NotConfigured | PASS / PASS / PASS / PASS |
| F13 | `WIFI_FULL`, PerItem | flight, aircraft, qty 1–4 | USD 9.99 | Unlimited | PASS / PASS / PASS / PASS |
| F14 | `INS_TRAVEL`, PerPassenger, CoverageStart | age 0–80, coverage country | EUR 12 (0–65), EUR 20 (65–80) | Supplier `InsurerA` | PASS / PASS / PASS / PASS |
| F15 | `ESIM_TR_5GB`, PerItem, Activation | coverage country, qty 1–5 | EUR 11, USD 12 | Supplier `TelecomA` | PASS / PASS / PASS / PASS |

Worked examples of doc 06:

| Example | Test | Status |
|---|---|---|
| A — 10 kg package per point of sale | `EXA_a_ten_kilogram_package_is_sold_per_point_of_sale_by_two_provisions_each_with_its_own_currencies_and_no_weight_pool` (second market filed in KWD + EUR: TRY is not in the test reference fixture) | PASS |
| B — 15 / 23 / 32 kg pieces | `EXB_fifteen_twenty_three_and_thirty_two_kilogram_pieces_are_three_definitions_with_their_own_flights_and_rates_and_no_piece_pool` | PASS |
| C — WCHR free | family F08 (all four checks) and `X01_…` | PASS |
| D — seat 18A | family F06 (all four checks) and `X04_…` | PASS |
| E — verified 12-person lounge slot | `EXE_a_verified_twelve_person_lounge_slot_refuses_an_overlap_accepts_an_adjacent_slot_and_records_an_immutable_adjustment_without_any_guarantee` | PASS |

The products, rules and amounts are fixtures that the domain must be able to express; none is a statement about what a named airline sells.

### 6.3 Cross-phase X01–X20

`V121FinalCrossPhaseAcceptanceTests` (SQL Server):

| ID | Test | Status |
|---|---|---|
| X01 | `X01_unlimited_and_must_check_availability_coexist_without_a_local_count_and_without_a_guarantee` | PASS |
| X02 | `X02_not_configured_is_never_reported_as_unlimited` | PASS |
| X03 | `X03_a_baggage_sale_type_never_seeds_a_flight_count_or_a_flight_weight_source` | PASS |
| X04 | `X04_a_seat_product_never_owns_a_local_occupancy_source` | PASS |
| X05 | `X05_an_unknown_facility_keeps_the_airport_slot_a_draft_and_refuses_its_activation_explicitly` | PASS |
| X06 | `X06_a_missing_source_of_truth_refuses_the_activation_of_a_flight_count_and_a_flight_weight_source` | PASS |
| X07, X08 | `X07_X08_an_adjustment_replays_idempotently_for_the_same_correlation_and_conflicts_when_the_payload_differs` | PASS |
| X09 | `X09_one_hundred_writes_with_the_same_expected_version_have_one_winner` | PASS |
| X10, X11 | `X10_X11_one_hundred_overlapping_slots_of_one_facility_have_one_winner_and_adjacent_slots_are_accepted` | PASS |
| X12 | `X12_one_hundred_independent_sources_are_all_accepted_concurrently` | PASS |
| X13 | `X13_a_new_product_version_keeps_the_one_policy_and_its_physical_source_and_the_pointer_is_reconciled_at_activation` | PASS |
| X14 | `X14_two_products_share_a_real_resource_only_through_their_explicit_typed_binding` | PASS |
| X15 | `X15_a_count_plus_weight_policy_binds_exactly_one_count_and_one_weight_resource` | PASS |
| X16 | `X16_a_passenger_usage_limit_is_an_entitlement_limit_and_never_a_physical_total_or_a_usage_ledger` | PASS |
| X17 | `X17_a_deferred_daily_room_night_or_assigned_asset_pattern_is_never_activated` (three patterns) | PASS |
| X18 | `X18_an_unavailable_source_is_never_reported_as_unlimited_or_available` | PASS |
| X19 | `X19_capacity_changes_leave_the_published_rules_and_prices_untouched` | PASS |
| X20 | `X20_the_reservation_source_and_its_hold_get_and_confirm_use_cases_are_byte_stable` | PASS |

Not run because they are not Phase 2: allocation, hold / confirm stress, available-to-sell arithmetic, provider acceptance, EMD — **DEFERRED_PHASE3**. Local activation against a real source of truth — **BLOCKED_NO_REAL_EVIDENCE** (only test-supplied references).

### 6.4 Descriptors

`V121FinalDescriptorConformanceTests` (domain): `D04_the_four_descriptor_enums_carry_exactly_the_approved_members`, `D04_a_provision_states_its_purchase_stage_and_keeps_it_through_publication`, `D04_the_legacy_stage_is_never_authored_and_a_migrated_row_is_not_published_until_a_stage_is_stated`, `D04_the_booking_states_whether_a_request_is_subject_to_confirmation_and_defaults_to_immediate`, `D08_an_advance_purchase_window_has_an_optional_maximum_that_is_never_below_the_minimum`, `D08_a_weight_package_is_a_weight_concept_with_its_entitlement_and_an_extra_piece_is_a_piece_concept`, `D08_overweight_oversize_and_special_equipment_take_the_concept_only_when_it_is_sourced`, `D08_a_baggage_rule_without_a_charge_kind_stays_a_draft` — PASS. (The `D04` / `D08` prefixes are the decision ids of doc 01.)

## 7. Concurrency evidence (SQL Server, not in-memory)

- One Active pricing per provision: filtered unique index `IX_AncillaryPricings_OneActivePerProvision ([Status]=(2))`; two activations → one winner, the loser gets 16507 (`PR21_…`, `V12_P02_…`).
- Rate key `(AncillaryPricingId, CurrencyId, PassengerTypeCode, AgeFromInclusive, AgeToExclusive)`: unique index `IX_AncillaryPricingRates_RateKey`; a duplicate inserted by raw SQL fails with error 2601 (`PR08_PR09_PR13_…`).
- 100 writers with the same expected version on one source → 1 success, 99 × 16605, one adjustment row (`X09_…`, `P2_X01_…`).
- 100 overlapping slots of one facility → 1 success, 99 × 16615, no overlapping row; adjacent slots accepted (`X10_X11_…`, `P2_A04_…`).
- 100 independent sources → 100 successes (`X12_…`, `P2_X03_…`).
- 100 concurrent policy definitions for one service identity → 1 success, 99 × 16604 (`P2_X10_…`).

## 8. API samples

These are the shapes of the Backoffice records as compiled; **no HTTP run was made in this iteration** (see section 12).

Define a pricing — `POST Backoffice/v1/AncillaryPricings`:

```json
{
  "ancillaryProvisionId": 1557000000000000001,
  "rates": [
    {
      "passengerTypeCode": "ADT", "ageFromInclusive": null, "ageToExclusive": null,
      "basePrice": { "amount": 40.00, "currencyId": 47 },
      "components": [
        { "category": "Tax", "code": "VAT", "name": null, "countryId": null, "stationAirportId": null, "amount": { "amount": 4.00, "currencyId": 47 }, "feeApplicationUnit": null, "taxIncludedInSource": null },
        { "category": "Fee", "code": "SVC", "name": null, "countryId": null, "stationAirportId": null, "amount": { "amount": 1.00, "currencyId": 47 }, "feeApplicationUnit": "Item", "taxIncludedInSource": null }
      ]
    },
    { "passengerTypeCode": "ADT", "ageFromInclusive": null, "ageToExclusive": null, "basePrice": { "amount": 44.00, "currencyId": 155 }, "components": [] }
  ]
}
```

Read a pricing (one rate shown):

```json
{
  "currencies": ["EUR", "USD"],
  "rates": [
    {
      "passengerTypeCode": { "name": "ADT" },
      "basePrice": { "amount": 40.00, "currencyId": 47, "currency": "EUR" },
      "components": [ { "category": { "name": "Tax" }, "code": "VAT", "amount": { "amount": 4.00, "currencyId": 47, "currency": "EUR" } } ],
      "unitTotal": { "amount": 45.00, "currencyId": 47, "currency": "EUR" },
      "isUnitTotalComplete": true,
      "unappliedFees": []
    }
  ]
}
```

Provision request — new members: `"purchaseStage": "PreOrder" | "PostTicketed" | "Both"`, `advancePurchase.maximumPeriod`, `baggageApplication.chargeKind`, `baggageApplication.allowanceConcept`. Service definition request — new member `booking.confirmationRequirement` (omitted = Immediate).

Inventory configuration snapshot — new member:

```json
{ "authority": { "name": "Unlimited" }, "state": { "name": "Unlimited" }, "isGuaranteed": false, "requiresAvailabilityCheck": true, "configuredCount": null, "configuredKg": null }
```

## 9. Git summary

- `git diff --shortstat`: 117 tracked files changed, 1017 insertions, 737 deletions. One of them is the Owner's own edit of `docs/AeroTech-Ancillary-v12.1/SHA256SUMS.txt`.
- Deleted (7): `PricingLineInput.cs`, `PricingLineInputValidator.cs`, `AncillaryPricingLineArgs.cs`, `AncillaryPricingLine.cs`, `AncillaryPricingLineConfiguration.cs`, `AncillaryPricingLineReadModelConfiguration.cs`, `AncillaryPricingLineReadModel.cs`.
- Untracked entries by project: Contracts 4 (the four enums), Domain 6, Application 3, Persistence 6, Query 8, ServiceHost 1, Domain.ConformanceTests 4, Application.AcceptanceTests 6.
- `git status --short -- "**/AncillaryReservationAggregate/**"` prints nothing.

## 10. What stays unavailable

- D11: FlightFlow-managed policies, Local policies and every local source cannot be activated in a deployed host (16608) until a real source for flight occurrence, inventory resource, airport facility with time zone, FlightFlow delegation and counting family is connected in a separately authorized step.
- D12: DailyCount, RoomNight, AssignedAsset.
- Phase 3: rate selection for a traveller and currency at sale time, quote and price snapshot, allocation, Hold / Confirm / Release against capacity, cross-order usage, EMD. Nothing in the repository consumes a pricing yet.
- D13: discounts, percentage fees, FX.

## 11. Decision register (choices made where the pack is silent)

1. Unit total = base + taxes + fees with unit `Item`; other fee units are reported, not added.
2. A rate stores scalar `CurrencyId` + `BaseAmount` (needed for the unique rate key) and exposes `BasePrice` as `Money`; a component owns a `Money`.
3. Component `Code` is nullable in storage for migrated rows; new authoring requires it and a pricing with a code-less component cannot be published or republished. A revision copies rates without re-validating them.
4. `PurchaseStage` is a required member of the provision Define / Change contract (breaking for Backoffice clients). `ConfirmationRequirement`, `MaximumPeriod`, `ChargeKind`, `AllowanceConcept` are optional members.
5. `ChargeKind` is nullable in storage (doc 02 lists it without `?`) so that migrated baggage rules keep loading; it is mandatory at publication.
6. 16316 and 16317 are raised by `Activate` (a new publication) only. `Reactivate` of a suspended migrated row is still allowed. There is no command that states a stage on an Active or Suspended migrated row; a Draft is corrected through the normal full edit.
7. Overweight, Oversize and SpecialEquipment accept either concept or none.
8. Current definition for a policy = Active version, otherwise highest non-retired version. The pointer changes only when the policy is activated; between a product revision and the next policy activation the read model still shows the previous version id.
9. `RequiresAvailabilityCheck` is computed at read time from the Active provisions of every version of the service identity; it is not stored on the policy.
10. Three existing shape pins were extended (`BookingDefinition` properties, `AncillaryProvision` properties, snapshot DTO properties) and two Phase 2 tests that pinned the removed D10 rule were rewritten to the new rule.

## 12. Deviations from the pack and open findings

Deviations:

- Data preservation, lineage tables, guarded migration and the clone reconciliation of doc 08 were dropped on the Owner's instruction; the dev data was deleted.
- Red-first was honoured only for the domain pricing tests; see gap report section 5.
- Example A uses KWD for the second market because TRY is not in the test reference fixture.
- No authenticated HTTP smoke was run for the changed Backoffice contracts (it needs a fresh Backoffice token). The acceptance tests call the application and query services directly, so model binding of the new request members is not exercised. The Phase 2 smoke of the same day found a defect of exactly that layer, so this gap matters.

Findings to raise:

- `ReferenceData.Currencies.DecimalPlaces` on dev is 0 for 168 of 170 currencies (only EUR and USD are 2). With D07 enforced, a real host refuses e.g. GBP 7.50 or KWD 7.125 until that reference data is correct. The ReferenceData module is not editable here.
- The legacy base line `Name` has no place in the rate model and was not carried.
- AirAvail's uncommitted M1 working copy reads read-model objects that no longer exist (not reviewed, not touched).
- An activation of a draft with a lower id than the Active version still relies on increasing ids for the save order (known since R5.3, unchanged).

## 13. Needs the Owner

1. Code audit of this working tree; commit / push decision (nothing was committed).
2. A fresh Backoffice token if an HTTP smoke of the new contracts is wanted before the audit closes.
3. Whether `Reactivate` of a migrated row should also demand a purchase stage (decision 6).
4. A trustworthy source for currency decimal places.
