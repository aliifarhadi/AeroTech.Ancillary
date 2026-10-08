# 01 - Source audit, verified benchmark and baseline economics

## 1. Verified pins
- Ancillary `k8s-stg@6b0a80ff708815ce3a6a957fbef3380b83a3f4cd` and baseline `843cf7ccda45a1e16b1bd537172346fcdd76ae35`: GitHub compare returned `ahead_by=4`, 300 changed files (293 added, 6 modified within comparison listing). From `e2a8c9f...` to current HEAD: one commit, 176 changed files, 125 added, 51 modified, no changed reservation paths. The GitHub comparison counts are not a cost model or time estimate.
- AirPrice `k8s-stg@1b41f08e9a22d3c27b807d5ea275120f0f7273ad` was fetched successfully on 2026-10-08. This supersedes prior assertion that it could not be accessed. Source root: `AeroTech.AirPrice/AirPrice/src/`.

## 2. Actual AirPrice code - model precedent, not an identical schema
- `Domain/AirFareAggregate/AirFare.cs` owns typed `DayTimePermissionRule`, `SeasonalityRule`, `BlackoutsRule`, `FlightApplicationRule`, `SalesRestrictionsRule` and other specific rule VOs, without a generic EAV rule definition.
- `ValueObjects/Seasonality/SeasonalityRule.cs` holds `List<PermittedTravelDateRange>`; each range has StartDate and StopDate (`DateOnly`).
- `ValueObjects/Blackouts/BlackoutsRule.cs` holds `List<TravelBlackoutDateRange>`; each range has Type, StartDate and StopDate (`DateOnly`).
- `ValueObjects/DayTime/DayTimePermissionRule.cs` holds permitted weekdays and multiple `DayTimePermissionTimeRange` values (StartTime, StopTime `TimeOnly`).
- `Persistence/AirFareAggregate/AirFareEntityTypeConfiguration.cs` maps seasonality, blackouts and day/time via `OwnsOne` and `OwnsMany` into normalized relational tables (`AirFareSeasonalityPermittedTravelDates`, `AirFareBlackoutsTravelDates`, `AirFareDayTimePermissionsTimes`); weekdays use a converted collection. This is a useful store pattern, **not** proof that all AirPrice conditions are independent domain Entities.
- `Domain/AirFareAggregate/ChainHandlers/Reservation/{SeasonalityChecker,BlackoutChecker,DayTimeChecker}.cs` checks date ranges and weekdays/time during fare reservation validation. **Do not copy these runtime checkers to Phase 1**. Their use of UTC flight time is source implementation detail, not a safe global time-zone policy for airport-local ancillary sales.
- `Domain/AirChargeAggregate/AirCharge.cs` and `Entities/AirChargeCondition.cs` use an aggregate with explicit include/exclude child conditions; supports narrow, typed criteria instead of DSL. Source `AirFare` validation methods include empty validations and a Blackout equality implementation anomaly (StartDate absent): **copy the pattern, not code/bugs**.
- Reuse canonical `AeroTech.Messages.AirPrice.Enums.{PassengerTypeCode,AirFareType,TimeUnit,WeightUnit}` from current Ancillary Contracts. AirPrice source includes other enums; reuse only after namespace/value check and without cross-repo editing.

## 3. Actual Ancillary v11 code
- `AncillaryProvision.cs`: ServiceDefinitionId, Sequence, Draft/Active/Suspended/Retired, `PassengerCriteria`, `SalesCriteria`, `TravelCriteria`, `FareCriteria`, AdvancePurchase, Quantity, Application, Outcome, Fee, PriceLines, Settlement, Availability, frozen Fulfillment.
- `AncillaryProvisionConfiguration.cs`: EF primitive collections in flat columns; `ProvisionRoutePair` is a normalized child; PriceLines belong directly to Provision.
- `TravelCriteria.cs`: only one TravelFrom/TravelTo and TimeFrom/TimeTo plus list fields. No 1000 separately managed date records or independent Blackout/Seasonality child records.
- `ProvisionPriceLine.cs`: Category=Ancillary/Tax/Fee, Code?, Name?, CountryId?, StationAirportId?, UnitAmount decimal(18,2). `FeeDefinition.cs`: CurrencyId, FeeApplicationUnit. These **must be migrated without loss**.
- `ServiceDefinition.cs`: stable identity/classification, booking/document metadata, status and revision; no PricingUnit.
- `AncillaryReservation.cs`/`AncillaryReservationUnit.cs`: existing Hold/Confirm with overall reference and per-unit OrderServiceId/TravellerId/flight coverage, plus dormant StockPoolId and ProviderUnitRef. These are **frozen** in Phase 1 and Phase 2; no interpretation of their future semantics.
- Prior agent report: 92 domain and 76 acceptance tests claimed passed. No independently checked CI run was found for this commit. A successful report is not a substitute for new local build/tests.

## 4. ATPCO reference hierarchy (official)
1. https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/what_are_optional_services.htm : Services Record identifies WHAT, Provisions Record specifies passenger/travel/geography/carrier/flight/fare/sales and fees. This is the applicable ancillary industry benchmark.
2. https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/Create_and_Update_Provisions.htm : Travel Date Table allows multiple travel dates per Provisions row; separate travel first/last, passenger/age and fee fields.
3. https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/View_Provisions_Details.htm : PTC/age, booking/document options, Free/NotAvailable, fixed amount/currency, applies-per and MustCheckAvailability.
4. https://atpco.net/single-blog/what-are-atpco-fare-rules-categories : Fare Categories 2 Day/Time, 3 Seasonality and 11 Blackouts are **analogies**; they are not the same physical ATPCO S7 file design.

## 5. Decisions resulting from benchmark
- Model WHAT (`ServiceDefinition`), WHEN/WHO/WHERE (`Provision`), filed amounts (`Pricing`) as separate commercial ARs in our product; ATPCO describes S5 and S7 but does **not** prescribe this precise three-AR DDD split.
- Directly borrow typed rule objects, nested relational collections and half-open / inclusive invariants as documented in v12, not an AirPrice runtime checker or its broad rules catalog.
- Normalization means dedicated child row and FK per selector; no single CSV/JSON list of 1000 dates. Do not over-model each primitive as an aggregate.
- Avoid duplicate application pricing engines and conflicting rate statuses. Fees are fixed per selected rate for Phase 1.

## 6. Cost comparison (qualitative, no invented effort numbers)
- RESET from `843cf7c`: must recreate Supplier/ServiceDefinition/Provision APIs, tests, CQRS, migrations and baseline Hold/get/confirm scaffolding, though can reuse framework. Stronger clean-slate schema but much greater redo and operational risk.
- EVOLVE `6b0a80f`: keep validated domain identity, HTTP authoring and query infrastructure, Suppliers, source contracts, extensive scenario tests; deliberately replace flat Provision persistence and split old PriceLines into Pricing, with audited command/query migrations. Most economical if existing production data can be migrated and no incompatible legacy consumer discovered.
- **Preferred EVOLVE**. Before deleting legacy columns, stage reversible backfill and report counts, nulls and unmappable `PricingUnit` values. If deployed production compatibility is material, stop before destructive migration; owner decides rollout mode. Never infer PerRoom/PerVehicle merely from service subcode.
