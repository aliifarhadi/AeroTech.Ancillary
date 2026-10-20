# S2 standalone real-HTTP E2E — conformance

## Verdicts

- **`S2_STANDALONE_HTTP_E2E_PASSED`**: C1–C5 pass through real HTTP TestServers. 30 tests were discovered and 30 executed, with 0 failed, 0 skipped and no aborted line. Order additions are verified by a separate GET to the Ordering simulator, and no external service is reached.
- **`PRODUCTION_ORDERING_CONNECTOR: NOT_YET_IMPLEMENTED`**: the production registration is still `NotConnectedOrderCommercialContextReader`, which answers 503 / 16815.

| Item | Value |
|---|---|
| Base | `k8s-stg@f8e29ae5111af9a85e945be845a87349f7c059f6` "G0". It is the pinned HEAD, unchanged and not advanced. |
| Commit | none; nothing is committed or pushed |
| Order documents | `00-OWNER-EXECUTION-DIRECTIVE.md`, `01-IMPLEMENTATION-INSTRUCTIONS.md` |
| Missing document | `02-E2E-ACCEPTANCE-AND-HANDOFF.md` was referenced but not supplied and is not on disk. The negative list of directive §5 was used in its place. |

**Run command (one line):**
```
dotnet test tests/AeroTech.Ancillary.Stage2.EndToEndTests/AeroTech.Ancillary.Stage2.EndToEndTests.csproj --no-restore
```

---

## 1. Production files modified

| File | Change | Why |
|---|---|---|
| `src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Dto/Service/ServiceResolvedServiceDto.cs` | +1 member `string? FulfillmentProviderKey` | 01 §2.2.3 and §4: Ordering requires a provider key on every `OrderService` (`Ordering.Final OrderService.cs:45`). The value already exists on the candidate. |
| `src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Queries/ResolveSelections/Service/ServiceSelectionResolutionMapper.cs` | +1 line `candidate.Confirmation.FulfillmentProviderKey` | sourced from the provision's `Fulfillment` definition; no default and no constant |
| `AeroTech.Ancillary.sln` | adds the new test project | — |

The change is additive and S2S only. Nothing else changed:
- no new route, table or migration;
- no change to Identity, auth or `ICallerContext`;
- no change to the SeatMap DTO or the seat-id algorithm;
- no change to `pricedSelectionRef`;
- no Stage 1 or Stage 3 change.

## 2. Test project

The project is `tests/AeroTech.Ancillary.Stage2.EndToEndTests`: 40 C# files, with no code comments.

**Ancillary host.** `Host/AncillaryTestHost.cs` is a `WebApplicationFactory<Program>`.
- It boots the real `src/AeroTech.Ancillary.ServiceHost/Program.cs` in environment `E2E`, so `appsettings.Development.json` is not loaded.
- Configuration is supplied only by test settings (`Host/Stage2Settings.cs`), including a test-only `Shopping:SelectionSigningKey`.

**What runs as in production.** Routing, versioning, controller binding, the `ApiResultWrapperFilter`, the exception middleware, the `SurfaceAuthorization` policies, `ClaimsCallerContext`, MediatR handlers, the Query services, the canonical `AncillaryShoppingEngine`, the pricing, the three surface mappers, `GetSeatMapService`, `PricedSelectionTokens` and `ResolveSelectionsService`.

**Replaced in test DI only.** Each port below was replaced after the production registration, in `ConfigureTestServices`.

| Port | Test implementation |
|---|---|
| authentication | `E2EAuthenticationHandler`. It issues a `ClaimsPrincipal` with exactly the claims `ClaimsCallerContext` and the surface policies read, for three distinct identities (`Host/E2EIdentities.cs`). The production policies run unchanged. |
| AirAvail S2S detail | The **production** `FlightOfferProvider` and its mapper stay in the pipeline. Only the primary handler of its typed `HttpClient` is replaced, by an in-process stub (`AirAvailHandler`/`TestAirAvail`) serving the captured real response `Fixtures/Wire/flight-offer-details.json`. It also simulates unknown offers (404) and outages (503). |
| `IOrderCommercialContextReader` | `TestOrderCommercialContextReader` over the simulator's `TestOrderStore` |
| `IAircraftSeatMapProvider` | `SeatMapDesk.FixedSeatMaps` (existing fixture) |
| `IOccupiedSeatProvider` | `TestOccupiedSeats` (taken seat, missing flight, outage) |
| catalog readers, currency, rates of exchange, airline codes | `ShoppingLab`/`InMemoryCatalog`, built from real Domain aggregates |
| airport, air-fare and point-of-sale conditions | existing `TestOnly*` fakes and `TestPointOfSaleConditions` |
| operator airline | `TestOperatorAirline` |
| Api references | `TestApiOfferReferencesReader`, the same logic as production without SQL |
| `IClock` | fixed at `2026-10-11T09:00Z` |
| `IShoppingSource` (C3_04 only) | `SeatMapDesk.VerifiedExitRows`, a **fake-verified exit-row fact**. Production has no source for it. |

**Isolation guard.** `Host/IsolationGuard.cs` records any escape, and each test fails on dispose unless the escape was declared.
- Every `HttpClient` from `IHttpClientFactory` gets a blocking primary handler, and so does the JwtBearer back-channel.
- All three DbContexts get a connection interceptor that throws.
- Hosted services are removed: MassTransit, `OutboxPublisher` and `MessageRetentionPoller`.
- C1_06 proves the SQL guard on a real SQL-backed Backoffice route.

**Known limit.** The Framework creates its Redis multiplexer eagerly in `Program`, before test DI runs (`Framework/AeroTech.Framework.Infrastructure/DependencyInjection.cs:32`, `abortConnect=false`).
- The test points it at loopback `127.0.0.1:1` with `connectRetry=0`, so no external host is contacted. It is disposed after each test.
- Avoiding this attempt entirely would need a production change, which was not done.

**Ordering simulator.** `Ordering/OrderingSimulator.cs` is a separate ASP.NET Core TestServer, built only in the test project. Its routes:
- `POST /__e2e/orders/from-offer`;
- `GET /__e2e/orders/{id}`;
- `POST /__e2e/orders/{id}/ancillaries`;
- `POST /__e2e/orders/{id}/mutations`.

The Add (`OrderingAddService`) works like this:
1. It checks the version and the idempotency key atomically, under a per-order gate.
2. It calls the real `POST /Service/v1/Ancillaries/Selections/Resolve` **over HTTP**.
3. It validates beneficiary, flight, capacity, seat uniqueness, price evidence and provider key.
4. It applies every line at once with exactly one version step, or inserts nothing.

Air services take the provider `FlightFlow`, mirroring `Ordering.Final Order.Create.cs:275`. An included seat inherits its air service's provider (`Order.Create.cs:404`). Ancillary services use only the `fulfillmentProviderKey` returned by Resolve; there is no fallback.

## 3. Checkpoints

| Gate | Evidence (tests) | Result |
|---|---|---|
| C1 real host, offer List/Details/Evaluate/Batch, authorization, no external call | C1_01 … C1_06 | passed |
| C2 Order source through the test reader; production registration unchanged | C2_01 … C2_04 | passed |
| C3 SeatMap / Batch / signing; free vs paid; duplicate, occupied and missing flight; exit row | C3_01 … C3_04 | passed |
| C4 separate Ordering HTTP host; Add → HTTP Resolve → GET; idempotency and atomicity | C4_F1, C4_F2, C4_F3, C4_F4, C4_05 | passed |
| C5 three surfaces; failures and state changes | C5_F5, C5_01 … C5_10 | passed |

**Mapping to the directive.**

| Directive item | Tests |
|---|---|
| §1 offer path | C1_02, C1_03 |
| §2 offer → test Order → Add | C4_F1 |
| §3 existing Order, pre-issue and post-ticket | C4_F2, C4_F3 |
| §4 SeatMap with included and paid seat | C4_F4, C3_02, C3_04 |
| §6 three surfaces | C5_F5, C1_04, C2_04 |

**§5 negative paths.**

| Negative | Tests |
|---|---|
| bad scope | C1_04, C2_04 |
| stale version | C5_01, C4_05 |
| price change | C5_02 |
| taken seat | C5_07 |
| duplicate seat | C3_02 |
| mixed basket | C5_03 |
| shared fee | C5_04 |
| mismatched traveller | C5_05, C5_09 |
| replay and double Add | C4_05 |
| outage | C1_05, C2_03, C3_03, C5_08 |
| tampered and expired refs | C5_06 |
| Order-scope consumer gap | C5_10 |

In every negative path, the simulator GET shows that the test Order did not change.

## 4. Scenario results (generated from the run's trx and HTTP transcripts)

The run used the command above with `--logger trx`; outcomes come from the trx file. Transcripts are written to `bin/Debug/net10.0/e2e-transcripts/<test>.json`. "Isolation" is the guard verdict at dispose; C1_06 declares its one SQL escape on purpose.

| Test | HTTP calls | Test Order at first GET | Test Order at last GET | Isolation | Result |
|---|---|---|---|---|---|
| `C1_01_the_real_host_lists_offer_candidates_through_the_api_route_without_any_external_call` | 1 | - | - | isolated | Passed |
| `C1_02_an_inline_plan_needs_no_details_a_typed_product_opens_details_on_click_and_single_evaluate_prices_the_choice` | 3 | - | - | isolated | Passed |
| `C1_03_an_offer_batch_is_a_priced_preview_that_is_never_ready_and_carries_no_reference` | 2 | - | - | isolated | Passed |
| `C1_04_the_surface_policies_and_the_offer_scope_refuse_a_missing_identity_a_foreign_surface_a_foreign_agency_and_a_foreign_country` | 6 | - | - | isolated | Passed |
| `C1_05_an_unknown_offer_and_an_unavailable_airavail_fail_honestly_through_the_real_provider_adapter` | 2 | - | - | isolated | Passed |
| `C1_06_the_sql_guard_stops_a_route_that_would_reach_sql_server_so_no_shopping_route_can_hide_a_database_call` | 1 | - | - | isolated | Passed |
| `C2_01_production_keeps_the_not_connected_ordering_reader_and_only_the_test_host_reads_the_simulator_store` | 3 | - | - | isolated | Passed |
| `C2_02_an_order_bound_batch_is_ready_and_signs_a_reference_for_every_line_after_details_on_the_order_candidate` | 4 | - | - | isolated | Passed |
| `C2_03_an_unreachable_ordering_an_unknown_order_and_an_order_closed_for_changes_are_refused_honestly` | 6 | - | - | isolated | Passed |
| `C2_04_an_agency_order_is_not_shopped_by_another_agency_or_by_the_airline_backoffice_on_another_channel` | 3 | - | - | isolated | Passed |
| `C3_01_the_seat_map_of_an_order_prices_a_category_seat_frees_an_uncategorised_seat_and_closes_taken_and_other_cabin_seats` | 2 | - | - | isolated | Passed |
| `C3_02_a_paid_and_a_free_seat_on_the_order_are_signed_together_and_the_same_seat_twice_signs_nothing` | 4 | - | - | isolated | Passed |
| `C3_03_a_missing_flight_occupancy_and_an_unreachable_flightflow_block_the_seat_map_instead_of_showing_free_seats` | 3 | - | - | isolated | Passed |
| `C3_04_an_exit_row_seat_is_signed_only_for_a_test_verified_adult_who_accepts_the_terms` | 9 | v2 services=1 seats=0 | v2 services=1 seats=0 | isolated | Passed |
| `C4_05_a_repeated_add_replays_its_first_result_a_reused_key_with_another_payload_conflicts_and_an_old_version_is_refused` | 10 | v2 services=1 seats=0 | v2 services=1 seats=0 | isolated | Passed |
| `C4_F1_an_offer_preview_becomes_an_order_whose_new_candidates_are_signed_added_through_resolve_and_read_back` | 9 | v1 services=0 seats=0 | v2 services=2 seats=0 | isolated | Passed |
| `C4_F2_an_existing_pre_issue_order_adds_two_pieces_for_one_traveller_with_its_exact_price_and_one_version_step` | 6 | v2 services=1 seats=0 | v2 services=1 seats=0 | isolated | Passed |
| `C4_F3_a_ticketed_order_with_open_coupons_adds_a_service_and_a_used_or_unknown_coupon_blocks_without_mutation` | 11 | v3 services=1 seats=0 | v3 services=1 seats=0 | isolated | Passed |
| `C4_F4_a_paid_and_an_included_seat_are_assigned_to_the_right_passenger_flight_and_capacity` | 6 | v2 services=1 seats=1 | v2 services=1 seats=1 | isolated | Passed |
| `C5_01_a_commercial_change_after_signing_makes_resolve_stale_and_the_add_inserts_nothing` | 7 | v2 services=0 seats=0 | v2 services=0 seats=0 | isolated | Passed |
| `C5_02_a_price_change_after_signing_makes_resolve_stale_and_the_add_inserts_nothing` | 6 | v1 services=0 seats=0 | v1 services=0 seats=0 | isolated | Passed |
| `C5_03_a_basket_with_one_invalid_line_signs_nothing_and_an_add_without_references_is_refused` | 5 | v1 services=0 seats=0 | v1 services=0 seats=0 | isolated | Passed |
| `C5_04_a_per_ticket_shared_fee_keeps_the_basket_unready_and_signs_nothing` | 3 | - | - | isolated | Passed |
| `C5_05_an_unlinked_traveller_signs_nothing_and_a_reference_of_another_order_inserts_nothing` | 11 | v1 services=0 seats=0 | v1 services=0 seats=0 | isolated | Passed |
| `C5_06_a_tampered_reference_is_rejected_and_an_expired_reference_is_refused_without_insertion` | 8 | v1 services=0 seats=0 | v1 services=0 seats=0 | isolated | Passed |
| `C5_07_a_seat_taken_after_signing_is_stale_at_resolve_and_the_whole_seat_basket_inserts_nothing` | 6 | v1 services=0 seats=0 | v1 services=0 seats=0 | isolated | Passed |
| `C5_08_an_ordering_outage_during_resolve_inserts_nothing_and_the_order_path_never_calls_airavail` | 8 | v2 services=1 seats=0 | v2 services=1 seats=0 | isolated | Passed |
| `C5_09_a_traveller_link_changed_after_signing_changes_the_order_source_so_resolve_is_stale_and_inserts_nothing` | 7 | v1 services=0 seats=0 | v1 services=0 seats=0 | isolated | Passed |
| `C5_10_an_order_scoped_service_resolves_valid_but_the_ordering_simulator_refuses_it_because_ordering_has_no_traveller_less_service` | 6 | v1 services=0 seats=0 | v1 services=0 seats=0 | isolated | Passed |
| `C5_F5_the_three_surfaces_show_the_same_canonical_offer_through_their_own_shapes_and_the_same_seat_map` | 12 | - | - | isolated | Passed |

### Transcript per test

#### C1_01_the_real_host_lists_offer_candidates_through_the_api_route_without_any_external_call (Passed, isolated)

1. `api->ancillary` `POST /Api/v1/AncillaryOffers` -> **200** count=12

#### C1_02_an_inline_plan_needs_no_details_a_typed_product_opens_details_on_click_and_single_evaluate_prices_the_choice (Passed, isolated)

1. `api->ancillary` `POST /Api/v1/AncillaryOffers` -> **200** count=12
2. `api->ancillary` `POST /Api/v1/AncillaryOffers/Details` -> **200** 
3. `api->ancillary` `POST /Api/v1/AncillaryOffers/Selections/Evaluate` -> **200** 

#### C1_03_an_offer_batch_is_a_priced_preview_that_is_never_ready_and_carries_no_reference (Passed, isolated)

1. `api->ancillary` `POST /Api/v1/AncillaryOffers` -> **200** count=12
2. `api->ancillary` `POST /Api/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=false refs=0/2 basket=['OrderBindingRequired']

#### C1_04_the_surface_policies_and_the_offer_scope_refuse_a_missing_identity_a_foreign_surface_a_foreign_agency_and_a_foreign_country (Passed, isolated)

1. `anonymous->ancillary` `POST /Api/v1/AncillaryOffers` -> **401** 
2. `otapanel->ancillary` `POST /Api/v1/AncillaryOffers` -> **403** 
3. `backoffice->ancillary` `POST /Api/v1/AncillaryOffers` -> **403** 
4. `api-foreign-agency->ancillary` `POST /Api/v1/AncillaryOffers` -> **403** error 16809
5. `api->ancillary` `POST /Api/v1/AncillaryOffers` -> **403** error 16809
6. `backoffice->ancillary` `POST /Backoffice/v1/AncillaryOffers` -> **403** error 16809

#### C1_05_an_unknown_offer_and_an_unavailable_airavail_fail_honestly_through_the_real_provider_adapter (Passed, isolated)

1. `api->ancillary` `POST /Api/v1/AncillaryOffers` -> **422** error 16804
2. `api->ancillary` `POST /Api/v1/AncillaryOffers` -> **502** error 16806

#### C1_06_the_sql_guard_stops_a_route_that_would_reach_sql_server_so_no_shopping_route_can_hide_a_database_call (Passed, isolated)

1. `backoffice->ancillary` `GET /Backoffice/v1/Suppliers/Paginated` -> **500** error None

#### C2_01_production_keeps_the_not_connected_ordering_reader_and_only_the_test_host_reads_the_simulator_store (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
3. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed

#### C2_02_an_order_bound_batch_is_ready_and_signs_a_reference_for_every_line_after_details_on_the_order_candidate (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
3. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Details` -> **200** 
4. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=2/2 basket=[]

#### C2_03_an_unreachable_ordering_an_unknown_order_and_an_order_closed_for_changes_are_refused_honestly (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **403** error 16809
3. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **503** error 16815
4. `test->ordering` `POST /__e2e/orders/{id}/mutations` -> **200** 
5. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **422** error 16801
6. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **422** error 16801

#### C2_04_an_agency_order_is_not_shopped_by_another_agency_or_by_the_airline_backoffice_on_another_channel (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `api-foreign-agency->ancillary` `POST /Api/v1/AncillaryOffers` -> **403** error 16809
3. `backoffice->ancillary` `POST /Backoffice/v1/AncillaryOffers` -> **403** error 16809

#### C3_01_the_seat_map_of_an_order_prices_a_category_seat_frees_an_uncategorised_seat_and_closes_taken_and_other_cabin_seats (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `api->ancillary` `POST /Api/v1/SeatMaps` -> **200** 

#### C3_02_a_paid_and_a_free_seat_on_the_order_are_signed_together_and_the_same_seat_twice_signs_nothing (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `api->ancillary` `POST /Api/v1/SeatMaps` -> **200** 
3. `api->ancillary` `POST /Api/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=2/2 basket=[]
4. `api->ancillary` `POST /Api/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=false refs=0/2 basket=[]

#### C3_03_a_missing_flight_occupancy_and_an_unreachable_flightflow_block_the_seat_map_instead_of_showing_free_seats (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `api->ancillary` `POST /Api/v1/SeatMaps` -> **502** error 16814
3. `api->ancillary` `POST /Api/v1/SeatMaps` -> **502** error 16814

#### C3_04_an_exit_row_seat_is_signed_only_for_a_test_verified_adult_who_accepts_the_terms (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `api->ancillary` `POST /Api/v1/SeatMaps` -> **200** 
3. `api->ancillary` `POST /Api/v1/AncillaryOffers` -> **200** count=12
4. `api->ancillary` `POST /Api/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=1/1 basket=[]
5. `api->ancillary` `POST /Api/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=false refs=0/1 basket=[]
6. `api->ancillary` `POST /Api/v1/AncillaryOffers/Selections/EvaluateBatch` -> **422** error 16801
7. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **200** allValid=true ['Valid']
8. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **201** version=2 services=1 seats=0 replayed=false
9. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v2 services=1 seats=0

#### C4_05_a_repeated_add_replays_its_first_result_a_reused_key_with_another_payload_conflicts_and_an_old_version_is_refused (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
3. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=1/1 basket=[]
4. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=1/1 basket=[]
5. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **200** allValid=true ['Valid']
6. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **201** version=2 services=1 seats=0 replayed=false
7. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **201** version=2 services=1 seats=0 replayed=true
8. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **409** error=IdempotencyKeyReusedWithAnotherPayload
9. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **409** error=OrderVersionConflict
10. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v2 services=1 seats=0

#### C4_F1_an_offer_preview_becomes_an_order_whose_new_candidates_are_signed_added_through_resolve_and_read_back (Passed, isolated)

1. `api->ancillary` `POST /Api/v1/AncillaryOffers` -> **200** count=12
2. `api->ancillary` `POST /Api/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=false refs=0/1 basket=['OrderBindingRequired']
3. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
4. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v1 services=0 seats=0
5. `api->ancillary` `POST /Api/v1/AncillaryOffers` -> **200** count=12
6. `api->ancillary` `POST /Api/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=2/2 basket=[]
7. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **200** allValid=true ['Valid', 'Valid']
8. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **201** version=2 services=2 seats=0 replayed=false
9. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v2 services=2 seats=0

#### C4_F2_an_existing_pre_issue_order_adds_two_pieces_for_one_traveller_with_its_exact_price_and_one_version_step (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
3. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=1/1 basket=[]
4. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **200** allValid=true ['Valid']
5. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **201** version=2 services=1 seats=0 replayed=false
6. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v2 services=1 seats=0

#### C4_F3_a_ticketed_order_with_open_coupons_adds_a_service_and_a_used_or_unknown_coupon_blocks_without_mutation (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `test->ordering` `POST /__e2e/orders/{id}/mutations` -> **200** 
3. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
4. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=1/1 basket=[]
5. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **200** allValid=true ['Valid']
6. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **201** version=3 services=1 seats=0 replayed=false
7. `test->ordering` `POST /__e2e/orders/{id}/mutations` -> **200** 
8. `test->ordering` `POST /__e2e/orders/{id}/mutations` -> **200** 
9. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v3 services=1 seats=0
10. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
11. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v3 services=1 seats=0

#### C4_F4_a_paid_and_an_included_seat_are_assigned_to_the_right_passenger_flight_and_capacity (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `api->ancillary` `POST /Api/v1/SeatMaps` -> **200** 
3. `api->ancillary` `POST /Api/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=2/2 basket=[]
4. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **200** allValid=true ['Valid', 'Valid']
5. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **201** version=2 services=1 seats=1 replayed=false
6. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v2 services=1 seats=1

#### C5_01_a_commercial_change_after_signing_makes_resolve_stale_and_the_add_inserts_nothing (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
3. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=1/1 basket=[]
4. `test->ordering` `POST /__e2e/orders/{id}/mutations` -> **200** 
5. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **200** allValid=false ['Stale(OrderVersionChanged)']
6. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **409** error=SelectionsNotCommittable
7. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v2 services=0 seats=0

#### C5_02_a_price_change_after_signing_makes_resolve_stale_and_the_add_inserts_nothing (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
3. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=1/1 basket=[]
4. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **200** allValid=false ['Stale(PriceChanged)']
5. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **409** error=SelectionsNotCommittable
6. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v1 services=0 seats=0

#### C5_03_a_basket_with_one_invalid_line_signs_nothing_and_an_add_without_references_is_refused (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
3. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=false refs=0/2 basket=[]
4. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **422** error=AddRequestIncomplete
5. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v1 services=0 seats=0

#### C5_04_a_per_ticket_shared_fee_keeps_the_basket_unready_and_signs_nothing (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
3. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=false refs=0/1 basket=['SharedFeeNotApplied']

#### C5_05_an_unlinked_traveller_signs_nothing_and_a_reference_of_another_order_inserts_nothing (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `test->ordering` `POST /__e2e/orders/{id}/mutations` -> **200** 
3. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
4. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=false refs=0/2 basket=[]
5. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7002 v1
6. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
7. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=1/1 basket=[]
8. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **200** allValid=false ['Rejected(PricedSelectionForAnotherOrder)']
9. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **409** error=SelectionsNotCommittable
10. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v1 services=0 seats=0
11. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v1 services=0 seats=0

#### C5_06_a_tampered_reference_is_rejected_and_an_expired_reference_is_refused_without_insertion (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
3. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=1/1 basket=[]
4. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **200** allValid=false ['Rejected(PricedSelectionNotAuthentic)']
5. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **409** error=SelectionsNotCommittable
6. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **200** allValid=false ['Expired']
7. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **409** error=SelectionsNotCommittable
8. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v1 services=0 seats=0

#### C5_07_a_seat_taken_after_signing_is_stale_at_resolve_and_the_whole_seat_basket_inserts_nothing (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `api->ancillary` `POST /Api/v1/SeatMaps` -> **200** 
3. `api->ancillary` `POST /Api/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=2/2 basket=[]
4. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **200** allValid=false ['Stale(SeatSelectionNotOffered)', 'Valid']
5. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **409** error=SelectionsNotCommittable
6. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v1 services=0 seats=0

#### C5_08_an_ordering_outage_during_resolve_inserts_nothing_and_the_order_path_never_calls_airavail (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
3. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=1/1 basket=[]
4. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **503** error 16815
5. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **502** error=AncillaryResolveFailed
6. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **200** allValid=true ['Valid']
7. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **201** version=2 services=1 seats=0 replayed=false
8. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v2 services=1 seats=0

#### C5_09_a_traveller_link_changed_after_signing_changes_the_order_source_so_resolve_is_stale_and_inserts_nothing (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
3. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=1/1 basket=[]
4. `test->ordering` `POST /__e2e/orders/{id}/mutations` -> **200** 
5. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **200** allValid=false ['Stale(SourceVersionChanged)']
6. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **409** error=SelectionsNotCommittable
7. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v1 services=0 seats=0

#### C5_10_an_order_scoped_service_resolves_valid_but_the_ordering_simulator_refuses_it_because_ordering_has_no_traveller_less_service (Passed, isolated)

1. `test->ordering` `POST /__e2e/orders/from-offer` -> **201** orderId=7001 v1
2. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
3. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=true refs=1/1 basket=[]
4. `ordering->ancillary` `POST /Service/v1/Ancillaries/Selections/Resolve` -> **200** allValid=true ['Valid']
5. `test->ordering` `POST /__e2e/orders/{id}/ancillaries` -> **409** error=SelectionsNotCommittable
6. `test->ordering` `GET /__e2e/orders/{id}` -> **200** v1 services=0 seats=0

#### C5_F5_the_three_surfaces_show_the_same_canonical_offer_through_their_own_shapes_and_the_same_seat_map (Passed, isolated)

1. `backoffice->ancillary` `POST /Backoffice/v1/AncillaryOffers` -> **200** listed
2. `backoffice->ancillary` `POST /Backoffice/v1/AncillaryOffers/Details` -> **200** 
3. `backoffice->ancillary` `POST /Backoffice/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=false refs=0/1 basket=['OrderBindingRequired']
4. `backoffice->ancillary` `POST /Backoffice/v1/SeatMaps` -> **200** 
5. `api->ancillary` `POST /Api/v1/AncillaryOffers` -> **200** count=12
6. `api->ancillary` `POST /Api/v1/AncillaryOffers/Details` -> **200** 
7. `api->ancillary` `POST /Api/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=false refs=0/1 basket=['OrderBindingRequired']
8. `api->ancillary` `POST /Api/v1/SeatMaps` -> **200** 
9. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers` -> **200** listed
10. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Details` -> **200** 
11. `otapanel->ancillary` `POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch` -> **200** allReady=false refs=0/1 basket=['OrderBindingRequired']
12. `otapanel->ancillary` `POST /OtaPanel/v1/SeatMaps` -> **200**

## 5. Behaviour the E2E made visible

None of these needed a production fix.

1. **The Order path never calls AirAvail.** `OrderShoppingSource` uses the Order's own `Air` snapshot (C5_08). Ordering therefore has to supply that snapshot (handoff §2).
2. **Unlinking a traveller after signing changes the Order fingerprint.** Resolve then answers `Stale/SourceVersionChanged` before the traveller check (C5_09). The B7 order of checks is correct.
3. **An airline Backoffice user cannot shop an agency Order** priced on PartnerAPI: 403 / 16809 (C2_04, `PricedOfferScopeGuard`).
4. **An Order-scoped A24 resolves `Valid`, but cannot be stored in Ordering**, because `OrderService.TravellerId` is non-nullable (C5_10). This is an external consumer issue; no fake traveller is invented.
5. **`PriceChanged` now has a test** (C5_02). G0 had reported that it was untested.
6. **The exit-row eligibility used in C3_04 is fake-verified.** Production still has no source for it.

## 6. Commands and counts

All commands were run in this working tree on f8e29ae.

| Command | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | Build succeeded, 0 errors, 66 warnings. That is the 62 warnings that already existed plus NU1902/NU1903 (`System.Security.Cryptography.Pkcs` 9.0.4) for the new project. The new project has 0 C# warnings. |
| Stage2 E2E (`--list-tests` / run) | discovered 30, executed 30, passed 30, failed 0, skipped 0, not aborted (18 s) |
| Shopping | discovered 360, executed 360, passed 360, failed 0, skipped 0 |
| Domain | discovered 259, executed 259, passed 259, failed 0, skipped 0 |
| Application acceptance (uses the local SQL Server of its own fixture) | discovered 224 / executed 224, passed 224, failed 0, skipped 0, not aborted (7 min 6 s); includes the boundary pins (route count unchanged) |

The new E2E count is independent of the older suites. No CI run was made or is claimed.

## 7. Not in scope / remaining

- Production Ordering connector and Ordering Add use case: see `S2-ORDERING-HANDOFF-CONTRACT.md`.
- Source gaps stay fail-closed and were not invented:
  - A02 weight concept;
  - authoritative A08 eligibility;
  - A10 quote;
  - A19 guardian contacts;
  - non-Item shared-fee settlement (C5_04).
