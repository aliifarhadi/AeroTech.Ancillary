# 09 - Phase 1 scenario matrix, conformance and exit criteria

**Every scenario is an actual test, not merely a narrative or agent assertion.** Run clean solution build, domain conformance and SQL-backed acceptance tests with command/read migrations. Test names use `V12_*`; report mapping ScenarioId => file.method and actual result. Existing v11 13-family tests must stay effective or be replaced by strictly stronger equivalent tests.

## 13 family authoring tests
| ID | Service family | Minimum proof (all create/read/Draft edit/activate/list) |
|---|---|---|
| FAM01 | prepaid/extra baggage | separate PerPiece/PerKilogram definitions, route/flight season & paid units |
| FAM02 | sports/special baggage | own service identity, permitted dates, allowance/price |
| FAM03 | wheelchair/special assistance | appropriate SSR code, Free/NotAvailable, no fabricated industry subcode |
| FAM04 | meal | PTC ADT/CHD, flight/cabin, ServiceDefinition Booking metadata |
| FAM05 | travel insurance | PerPassenger, age `[0,65)` and `[65,+inf)` distinct rates, policy date qualification |
| FAM06 | paid seat selection | PerSeat, aircraft-specific seat characteristic; FlightFlow untouched |
| FAM07 | airport lounge | industry `0BX` only if verified reference, otherwise CarrierDefined; PTC, Tax/fee breakdown |
| FAM08 | priority boarding | PerPassenger or item according to actual product identity, day/time |
| FAM09 | fast track | sales/POS & seasonal and blacklist |
| FAM10 | Wi-Fi | PerItem, no activation workflow invented |
| FAM11 | pet service | service definition, booking metadata, route/flight, fixed price |
| FAM12 | Meet & Assist / CIP | PerPassenger or PerItem as approved, POS/airport/time |
| FAM13 | UMNR/special handling | child eligibility, booking meta, multiple rule dates |

## Normalization/semantics regression
- `V12_N01`: 1,000 distinct explicitly allowed date rows, unique real child IDs, no JSON / delimited column, round-trip through SQL and paginated Backoffice; edit/delete one date preserving other 999 and identity.
- `V12_N02`: 3 disjoint seasonal windows, 10 blackouts, singleton blackout date, reversed/duplicate ranges rejected; date restrictions preserved after activate.
- `V12_N03`: Allow Monday 08:00-12:00, Deny Monday 09:00-10:00; boundary rows/range validation and timezone semantics documented, no Phase 1 runtime evaluator present.
- `V12_N04`: passenger, POS, Customer, CustomerType, route/direction, marketing/operating carrier, FlightId/Number, aircraft, Fare family/type/basis/cabin/RBD IDs survive command->query->Backoffice and Draft mutation.
- `V12_N05`: empty dimensions unconstrained; no generic `ConditionType`, JSON DSL, expression evaluator, preview/simulate routes in source.
- `V12_N06`: Active Provision child data cannot be mutated or deleted, new Draft provision can supersede via approved sequence and lifecycle.

## Pricing invariants
- `V12_P01`: ServiceDefinition PricingUnit immutable once published and across revisions of same reference; wrong-unit pricing rejected.
- `V12_P02`: one Provision owns many Draft/Retired price versions and max ONE Active; SQL filtered index tested with two connections racing to activate different versions.
- `V12_P03`: Paid Active Provision exactly one Active price; Free/NotAvailable no Active payable price; illegal deactivation rejected.
- `V12_P04`: PerPassenger age 0-64 and 65+ lines (exclusive upper) + ADT/CHD/INF selectors with non-overlapping bands; duplicate/overlap rejected.
- `V12_P05`: PerRoom/PerVehicle/PerItem/PerSeat/PerPiece/PerKilogram one currency per price; non-passenger selectors rejected.
- `V12_P06`: exactly one base Ancillary monetary component per selector; tax/fee lines with category, code, name, country and station; missing base, duplicate tax component, mixed generic/specific fallback rejected.
- `V12_P07`: amount decimal(18,2), negatives/duplicate lines rejected; per-rate total calculated without summing alternative PTC/age bands.
- `V12_P08`: atomic active Pricing switch preserves old snapshot, no transient price gap for Active Paid Provision; stale expected-old-id conflicts deterministically.

## Compatibility and hard source boundaries
- `V12_C01`: Existing Supplier definition/register/list/detail/Retire still work; ServiceDefinition identity/subcode/SSR/booking/document/revision preserved; only CarrierDefined codes for unknown industry.
- `V12_C02`: v11 existing allowed credentials/pagination/source contracts remain compatible or breaking change explicitly listed for Owner. 13 old family cases still pass.
- `V12_C03`: clone v11 DB migrated to v12: all existing rule selectors, PriceLines amounts/categories and statuses preserved, command/query schema consistent; before/after counts and mappings reported.
- `V12_C04`: no changed source/tests/routes under AncillaryReservation/Hold/Get/Confirm; no other repo edits; no new provider adapter, StockPool, evaluator, FX, EMD issuing or AirAvail/Ordering/FlightFlow contracts.
- `V12_C05`: clean `dotnet build` and `dotnet test` all Ancillary projects; fresh DB migration up; existing-db migration up; no pending EF model changes on command/query contexts; no failures hidden by skips.
- `V12_C06`: concurrency test for same `(ServiceDefinitionId,Sequence)` active Provision; unique index and domain 409 conflict.

## Phase 1 exit report - mandatory format
```
PHASE_1_V12_READY_FOR_OWNER_AUDIT
Repository: ...
Base SHA: ...
Implementation SHA: ...
Changed files by Module: ...
Migration names: ...
Legacy->new mapping counts & exceptions: ...
Scenario tests: N passed / N failed / N skipped (method map attached)
Build: command, exit code, relevant log
CommandDb / QueryDb pending migration/model changes: ...
No Reservation/Hold/Get/Confirm changes: diff evidence
No other repo changes: evidence
Remaining gap register: ...
GO / NO_GO / GO_WITH_CORRECTIONS
```
Then STOP; no Phase 2 code until Owner authorizes.
