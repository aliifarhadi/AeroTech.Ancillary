# Deterministic domain semantics and invariants (design freeze for Phase 1)

## 1. Core matching contract; no hidden EAV or general rule language
Candidate ordering: published ACTIVE service definitions only, provision status ACTIVE, eligible sales window, sorted `Sequence ASC`, **first provision whose populated rule groups all match** is decisive; its `CommercialOutcome` decides Paid, Free or NotAvailable. If no provision matches, not saleable (NoMatch); a matching NotAvailable is explicit denial and must not fall through to a broader Paid provision. The executor is future shopping/Phase-3 interface, NOT written as a generic runtime evaluator in Phase 1.
- Empty rule group or empty allowed-values collection = no restriction from that group.
- OR among alternatives of same typed collection; AND between populated typed dimensions, and among different rule groups. A group with both FlightNumber and Aircraft must match at least one of each.
- The special negative types `ProvisionBlackoutPeriod` and `ProvisionDayTimeWindow(Effect=Deny)` veto their positive date/time groups.
- For other dimensions, to ban a matching passenger/flight/fare/customer while allowing all others: define a more specific, lower-sequence ACTIVE Provision with Outcome=NotAvailable, plus broader positive provision at higher sequence. Do NOT add a universal `Effect`/`Negate` to every selector.
- Absent runtime context for a **populated** required dimension means `UnsupportedContext` / NOT MATCH. Never silently treat absent fare, service-date, passenger age, POS or timezone as allowed. Empty restriction stays unrestricted. This is critical for nonflight services.

## 2. Travel date / blackout (single canonical model)
- `ProvisionPermittedTravelPeriod(StartDate,EndDate)` is inclusive absolute-date period; start<=end. A single-day date is start=end. 1000 consecutive days -> one period, not 1000 date rows. Many disjoint periods may be authored; adjacent and overlapping positive periods are semantically mergeable (canonicalize during Draft import/bulk edit).
- `ProvisionBlackoutPeriod` has the same shape and is ALWAYS a denial. Multiple blackout rows may overlap; merging to nonoverlapping minimal ranges is allowed only while Draft. Blackout has priority even within permitted range.
- If zero positive periods, any date satisfies the positive-date part; blackout may still exclude dates.
- No second `SeasonalPeriods` AND/OR selector. "Seasonality" is a **business label** for positive permitted periods; it is NOT a second logical dimension. Do not implement annual recurrence, month/day wildcards, holiday calendars or operator trees in Phase 1.
- Date evaluated at the ServiceDefinition.ServiceDateBasis: FlightDeparture/ServiceStart/CheckIn/CoverageStart/Activation. Date-time must be resolved to local service occurrence before date extraction. `DateOnly` stored independently of UTC. If basis/source timezone cannot be resolved, sale is unsupported, NOT guessed.
- Merge algorithm for Draft positive ranges: sort by StartDate, then repeatedly merge if `next.StartDate <= current.EndDate.AddDays(1)` (guard DateOnly.MaxValue). Keep administrative source-to-merged-row lineage for imported published history; do not change an already-published row graph in place.
- Example: permitted `[2027-04-01,2027-04-30]`, blackout `[2027-04-10,2027-04-12]`: Apr 09 ALLOW, Apr 11 DENY, Apr 13 ALLOW. No positive periods plus Dec25 blackout: Dec24 ALLOW, Dec25 DENY.

## 3. Day/Time Application (weekly recurring)
- Mask bits: Monday=1, Tuesday=2, Wednesday=4, Thursday=8, Friday=16, Saturday=32, Sunday=64. Valid mask 1..127. Every window applies to all days selected in mask; `Effect` is exactly Allow=1 / Deny=2.
- StartLocalTime inclusive, EndLocalTime exclusive; null start = start of day, null end = end of day (not an instant). Both null = all day; if both non-null require start < end; no explicit overnight interval in ONE row. Decompose cross-midnight into two adjacent-day windows (one with open end, one with open start). Example Sat 22:00 to Sun 02:00 -> `[Sat,22:00,null)` + `[Sun,null,02:00)`.
- If no Allow window is defined, positive day/time restriction is unconstrained. Otherwise at least one Allow window must match. If any Deny window matches, result is DENIED even if Allow matches. Duplicate semantically identical windows rejected; overlapping Allow windows can be canonically combined only for Draft. Contradictory fully-denied Allow windows: either emit a deterministic unreachable-rule error on publication or require owner acknowledgement; do not silently pretend it can match.
- Weekly Day/Time is combined with the absolute date rules by AND. `DateRule` and `DayTimeApplicationRule` are separate dimensions but use the SAME service occurrence and local timezone.
- For a flight crossing midnight, match scheduled departure origin-local time when DateBasis=FlightDeparture, never destination date or raw UTC. DateBasis=CheckIn uses property's local time. DST ambiguous/missing service time must fail closed until the consumer has an occurrence timezone policy.

## 4. Passenger, Sales, Geography, Flight, Fare
- Passenger: allowed PTC codes and non-overlapping age ranges are separate AND dimensions. Age is completed years at the ServiceDateBasis occurrence, not calendar year or today's age. PTC and age pricing selectors are NOT inherited automatically from Provision passenger eligibility. `AgeFromInclusive >=0`, `AgeToExclusive > AgeFrom` when present, and null upper means infinity. For unsupported customer/passenger date context, fail closed.
- Sales: customer/POS/type selectors are allow-lists; SaleEffectiveFrom inclusive and SaleDiscontinueAt exclusive (UTC DateTimeOffset instant). ServiceDefinition-level sales dates also apply by AND, not replacement. Null upper bound means open-ended. No supplier/security framework invented here.
- Geography: typed route pairs preserve direction; `ProvisionServiceLocation` is actual service location (hotel, lounge, transfer), and `ProvisionCoverageCountry` is geographic benefit scope (eSIM/insurance). Do not treat airline route origin as hotel's location. Nonflight contexts cannot use flight-only constraints without an explicit upstream context contract.
- Flight: Marketing/Operating carrier are distinct. FlightId is canonical instance ID; FlightNumber is an optional number/code filter. Aircraft refers to existing canonical aircraft master; no seat inventory mirrored. No implicit flight-type match for an independent SIM.
- Fare: AirFareId, AirFareType, FareFamilyId, FareBasisCode, CabinClassId and RbdId are separate allow-lists from AirPrice; unknown fields fail closed when required. IDs and code normalization must match canonical masters; no wildcard FareBasis DSL.
- At activate, reject invalid empty/duplicate identity values, invalid Periods, disallowed reference enum values, incompatible rule/application context, missing canonical DateBasis, and incoherent positive/negative group structural constraints. Referenced catalog IDs are validated against read-only masters **if those masters are actually available**; otherwise report existence check as unresolved rather than silently accepting fabricated codes.

## 5. Advance purchase (Cat-5-inspired, limited)
- `MinimumPeriod:int >=0`, `Unit:canonical AirPrice.TimeUnit`; null Rule means no lead-time restriction. `SameTimeAsTicketed` is applicable only to ticket-associated services whose transaction can provide ticket issuance time; incompatible standalone authoring is rejected.
- Minimum lead is measured between sale instant and service occurrence. For MINUTE/HOUR use elapsed durations, for DAY/MONTH use local-calendar semantics from the referenced business timezone. If the actual canonical TimeUnit enum includes values without a proven implementation (or source contract), keep those values non-activatable rather than fabricating behavior.
- Do not add earliest booking, maximum lead, ticket-designator or fare tariff restrictions in Phase 1. Those are future explicit extensions.

## 6. Commercial Outcome and precedence
- `Paid`: exactly one active Pricing at the same time the Paid Provision is active. `Free`, `NotAvailable`: zero active Pricing. `NotAvailable` is an **explicit matching negative Provision**, even if all its selector groups are positive allow-lists. It is not a global service disable.
- BookingRequired and DocumentRequired are independent boolean metadata constrained by the product booking/document policy. A `Free` service may still require booking or EMD, as ATPCO S7 demonstrates. `NotAvailable` may not trigger booking or document issuance in future.
- An ACTIVE Provision and an ACTIVE Pricing are immutable as commercial historical snapshots. New price -> revise Pricing and atomic switch. New rule semantics -> new Draft Provision and atomic same-sequence publication; old Provision Retired.
- Unique filtered `(ServiceDefinitionId,Sequence)` on active Provision; unique filtered `(AncillaryProvisionId)` on active Pricing; unique `(ProvisionId,Version)` on price versions. Failures must produce domain conflict errors. Concurrency tests required.

## 7. Quantity/Pricing compatibility (must be checked at publication)
`PricingUnit` is fixed for the product; `Quantity.Unit` defines the order quantity measure; `FeeApplicationUnit` defines how a fee applies to travel/ticket coverage; PTC/Age pick alternate rates. Distinct concepts; never equate them.
| PricingUnit | Permitted Quantity.Unit | Example | Selector |
|---|---|---|---|
| PerPassenger | Each | insurance, lounge, shared transfer, meal | PTC/Age optional |
| PerRoom | Each | hotel room per chargeable period | no PTC/Age |
| PerItem | Each | SIM, Wi-Fi pass, pet crate | no PTC/Age |
| PerVehicle | Each | private transfer | no PTC/Age |
| PerSeat | Each | paid seat | no PTC/Age |
| PerPiece | Piece | extra checked bag | no PTC/Age |
| PerKilogram | Kilogram | excess baggage by weight | no PTC/Age |
All quantity values integer under existing `QuantityRule` (`Min>=1`, `Max>=Min`); no half-kilo until a separately approved contract supports precision. For hotel nights or insurance coverage days: amount is for a defined product/coverage unit, not silently "per night/day" without a documented application/duration rule. If variable duration billing is required, it needs separately approved PricingUnit or pricing duration rule, not an invented multiply-by-days algorithm.

## 8. Lifecycle and structural acceptance, without a Phase-1 shopping engine
Phase 1 MUST freeze truth tables for every supported typed rule in a **test-only reference oracle** based on explicit in-memory context (not a production shopping service): date, day/time, passenger, sales, geography, flight, fare, NotAvailable precedence, and missing required context. Rule-owned pure domain value predicates are permitted where they also enforce a structural invariant; no published `IAncillaryCommercialEvaluator`, no endpoint, no AirAvail/FlightFlow calls, no generic evaluation DSL. This approach verifies expected semantics now without implementing future external itinerary shopping. Phase 1 MUST test authoring round-trip, aggregation, structural invariants, reference-oracle truth tables, pricing validity and release behavior. Phase 3 must rerun these exact vectors through the real consumer to prove equivalence.
