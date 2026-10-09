# Backoffice contract, source migration and deletion audit

## 1. Reuse scope vs changes
**Keep** current repository, ServiceDefinition CRUD/version/lifecycle, Supplier, Framework, domain errors, Pricing AR/Line, price switch and indexes, existing tests and family fixtures, immutable published snapshots. **Refactor** AncillaryProvision domain hierarchy and relational mappings; rewrite specifically the wrong TravelDate/SeasonalPeriod test assertions. **Do not delete or alter** operational AncillaryReservation/Unit, Hold/Get/Confirm route behavior, Ordering/FlightFlow/AirAvail code or currently published historical data.

## 2. Proposed Backoffice request shapes (field contracts; no premature implementation)
Use existing `/Backoffice/v1/AncillaryServiceDefinitions`, `/AncillaryProvisions`, `/AncillaryPricings`. Preserve existing DTOs with an explicit compatibility strategy; do not silently repurpose old field names. New normalized authoring form is conceptual:
```json
{
  "serviceDefinitionId": 123,
  "sequence": 20,
  "coverageScope": "Sector",
  "passengerEligibility": {"allowedPassengerTypes": ["ADT"], "allowedAgeBands": [{"ageFromInclusive":0,"ageToExclusive":65}]},
  "salesRestrictions": {"salesEffectiveFrom":"2027-01-01T00:00:00+00:00","salesDiscontinueAt":null,"allowedPointOfSaleIds":[13]},
  "geography": {"allowedRoutePairs":[{"originAirportId":1,"destinationAirportId":2,"direction":"Directional"}]},
  "flightApplication": {"allowedFlightIds":[12345]},
  "fareApplication": {"allowedFareFamilyIds":[5]},
  "travelDate": {"permittedPeriods":[{"startDate":"2027-06-01","endDate":"2027-08-31"}],"blackoutPeriods":[{"startDate":"2027-07-10","endDate":"2027-07-12"}]},
  "dayTimeApplication": {"windows":[{"daysOfWeekMask":31,"startLocalTime":"09:00:00","endLocalTime":"17:00:00","effect":"Allow"}]},
  "advancePurchase": {"minimumPeriod":24,"unit":"Hour","sameTimeAsTicketed":false},
  "quantity": {"unit":"Each","minQuantity":1,"maxQuantity":2},
  "outcome": {"disposition":"Paid","documentRequired":false,"bookingRequired":true}
}
```
The example is explanatory: specific JSON enum serialization follows current repository patterns and canonical IDs must be verified, not blindly taken from placeholder numbers. Supplemental existing `Settlement`, `Availability`, `Fulfillment` properties retain established contract until an approved migration adapter is written.

Dedicated draft-row endpoints should be named by RULE scope, not a generic condition controller:
- `POST/PUT/DELETE /AncillaryProvisions/{id}/TravelDate/PermittedPeriods/{rowId?}`
- `POST/PUT/DELETE /AncillaryProvisions/{id}/TravelDate/BlackoutPeriods/{rowId?}`
- `POST/PUT/DELETE /AncillaryProvisions/{id}/DayTimeApplication/Windows/{rowId?}`
- For other groups, prefer a typed `PUT /AncillaryProvisions/{id}/{RuleGroup}` to replace the Draft group, stable IDs retained for unchanged rows. This avoids 27 x 3 bespoke routes while keeping typed domain entities and no JSON column in persistence.
- Existing `Publish`, `Activate`, `SwitchActivePricing` remain orchestration applications; no preview/simulate/ITIN matcher in Phase 1.

## 3. Safe v12 -> v12.1 semantic migration
- First read actual HEAD, EF model snapshot and current live database state; create a reproducible backup and migration-clone. Never assume v12 report values still apply to the new live database.
- Introduce typed rule-group identity, FK and child tables additively; write dual *read-compatible* migration projections where possible, NOT two authoritative concurrent writable representations.
- v12 has two positive date inputs with **AND**: exact `TravelDates` whitelist and `SeasonalPeriods` whitelist. Convert to ONE new permitted period collection by **set intersection** if both are populated; if only one is populated, use its union; if neither populated, no positive restriction. Compress consecutive resulting days into minimal inclusive periods (without expanding large continuous periods day by day). Keep v12 blackout union as denied periods. V12 DayTime Allow/Deny rows map to 7-bit day masks (one bit per original weekday) with the same authoring meaning.
- One v12 `ProvisionTravelDate` row may map into an aggregate range together with others. Preserve old row identity in read-only migration mapping/audit, NOT by fabricating a 1:1 new row ID. Record counts + semantic-equivalence tests, not row-count equality.
- Old v11 TravelFrom/TravelTo and primitive JSON list columns are kept as archived database history during transition. They are no longer read by new authoring/query models after verified cutover. A cleanup migration to drop archived columns is a separate explicit owner-approved deletion operation.
- Published historic Provisions that cannot be migrated without changing meaning remain readable in an immutable legacy projection; any new rule variant uses a successor Provision with new ID. Do not change historical accepted Order/Reservation references.
- New `ServiceDateBasis` is mandatory for new services. Existing published ServiceDefinitionRefs need a **reviewed mapping table** by actual family, not inferred from ATPCO codes or silent default. Block newer date-rules publication until basis known.
- Required integrity checks: Command vs Query comparable canonical intervals, original applicable date-set equivalence, blackout precedence, preserved price/currency/tax components, status, Provision IDs, published history, parent-child FK, unique indexes, no orphan references. Where date-basis or timezone or v12 time-window boundary semantics were not defined, log `MANUAL_MAPPING_REQUIRED` and do not make up a value.
- Failure rollback on clone restores v12 code+schema/data as a unit. Test twice: empty DB and current v12 clone with realistic data, plus one large 1000-day contiguous and one sparse migration dataset.

## 4. Reference and consumer compatibility audit
- Find source users of `AncillaryProvision.TravelDates`, `SeasonalPeriods`, `TravelFrom`, `TravelTo`, `DayTimeRestrictions`, `FeeCurrencyId`, `FeeApplicationUnit` and legacy `ReadModel.AncillaryProvisionPriceLines` including query SQL and AirAvail read contracts.
- v12 completion report explicitly flags **AirAvail direct SQL** to old readmodel price columns, which no longer stay fresh for newer rows. Block production GO until this is separately resolved under owner authority; no silent modifications to AirAvail.
- Old Backoffice API clients may use old `travelDates` individual DateOnly[] and `seasonalPeriods` side by side. Do NOT reinterpret old payloads as a union. Either versioned DTO adapter with exact old semantics and explicit incompatibility reporting, or a separately approved breaking contract / coordinated client migration. Do not expose two contradictory publicly writable rule sources.
- New readmodel returns typed RuleGroup summaries + row IDs + start/end ranges + effect; list endpoint provides counts, not 1000 nested rows; paginate child rows if practical.
- Proposed attributes `ServiceDateBasis`, `ServiceLocation`, `CoverageCountry` reference canonical AirInfo/AeroCore identifiers only, not replicated reference master tables.

## 5. Full deletion and migration-impact audit
1. Source-level delete obsolete v12 `ProvisionTravelDate` (single-day model), `ProvisionSeasonalPeriod` (duplicate permitted ranges), old flat `TravelCriteria` peers and obsolete endpoints **after** approved DTO migration; never leave a public second authoring path.
2. Keep `ProvisionBlackoutPeriod` but reparent under TravelDateRule. Rename/restructure `ProvisionDayTimeRestriction` -> `ProvisionDayTimeWindow` with weekday mask and explicit bounds; typed migration.
3. Old valid Quantity, Outcome, Settlement, CoverageScope, Baggage and Seat metadata are business fields, not erroneous legacy and remain.
4. Existing `FulfillmentDefinition` must be retained but marked Phase-3 operational ownership unresolved; phase-1 agent may not invent supplier credentials or new adapter protocols.
5. Produce a difference report against *approved* catalogs, source/DTO, EF snapshots, migrations, query tables and tests. Search removed identifiers globally before signoff. No stale DTO property or ghost command handler is acceptable after agreed deprecation.
