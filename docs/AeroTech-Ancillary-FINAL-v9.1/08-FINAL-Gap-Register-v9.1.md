# 08 — FINAL Gap Register v9.1

## Rule

No item in this table is permission to omit a CORE v9.1 scenario. Blocking implementation tasks such as J2 and FareFamilyId propagation are assigned to the v9.1 milestones and are **not open design gaps**.

| ID | Gap | Status | Blocks v9.1 close? | Rule |
|---|---|---|---|---|
| G01 | canonical PCC | source gap | No | do not infer from office/agency fields |
| G02 | loyalty FF status context | integration future | No | no v9.1 implementation field until an authoritative runtime contract exists |
| G02A | authoritative pre-order DOB/age | integration future | No | do not infer age from PTC; no v9.1 age criterion field until both shopping contexts carry authoritative DOB |
| G02B | stable cross-context traveller occurrence ordinal | integration future | No | no v9.1 occurrence criterion field; do not infer it from unstable request ordering |
| G03 | customer index score | integration future | No | no v9.1 implementation field until a canonical source exists |
| G04 | Keyword/SSR matching advanced semantics | integration future | No | no v9.1 generic keyword field; add only with a concrete contract |
| G05 | ticket designator | context future | No | no guessed parser |
| G06 | account code | context future | No | no guessed parser |
| G07 | tour code | context future | No | no guessed parser |
| G08 | tariff/rule qualifiers | context future | No | no guessed parser |
| G09 | ExternalQuote provider protocol | future pricing | No | FixedAmount closes v9.1 |
| G09A | per-kg/per-5kg excess-weight request/rounding semantics | commercial input future | No | fee units are retained but runtime calculation is inactive until exact excess-weight contract is frozen |
| G10 | AI/dynamic ancillary pricing | future pricing | No | add only with a concrete pricing-provider contract; no placeholder method now |
| G11 | interline concurrence execution | future | No | shape retained; no fake partner logic |
| G12 | paid-upgrade execution | future servicing | No | no v9.1 UpgradeApplication field; add only when availability/reprice/servicing contract is frozen |
| G13 | time-to-decide provider | future | No | do not invent provider key |
| G14 | multi-flight EMD value/proration | future document | No | no invented proration |
| G15 | non-air TravelCommerce closure | separate BC | No | Transfer/Car/Hotel/Insurance not Ancillary |
| G16 | exact inaccessible legacy Sabre page | evidence limitation | No | current public Get Seats is authority |
| G17 | exact inaccessible Jazeera Swagger | evidence limitation | No | do not infer private schema |
| G18 | exact inaccessible flydubai currency_converter page | evidence limitation | No | current AirAvail/AirPrice FX contract is authority |
| G19 | supplier-specific distinct post-confirm activation/issuance API | supplier-contract future | No | do not create a generic endpoint or no-op capability until a real Supplier API proves a distinct operation |
| G20 | first concrete external Supplier protocol/adapter | supplier integration future | No | v9.1 freezes the adapter/routing boundary; do not invent a production third-party wire protocol until a Supplier is selected |
| G21 | supplier lifecycle that cannot honor canonical Hold/Confirm/Read/Validate/Release/Cancel semantics | supplier-contract future | No | add a new capability only from a concrete Supplier contract; do not pre-build a generic capability matrix |

## Closed in v9.1 — no longer gaps

```text
Aircraft reference authority            CLOSED -> AirInfo
Cabin reference authority               CLOSED -> AirInfo
static seat map for authoring            CLOSED -> AirInfo DisplaySeatMap
live physical seat ownership             CLOSED -> FlightFlow
FareFamilyId canonical source            CLOSED -> AirPrice
FareFamilyId ancillary matching          CORE implementation
FareBasis exact matching                 CORE implementation
exact FlightId targeting                 CORE implementation
exact seat-number authoring              CORE implementation
pre-order ancillary shopping             CORE -> AirAvail Offer context
post-order ancillary shopping            CORE -> AirAvail Order context
ReservationMode ownership                CLOSED -> Ordering provider capability
EMD ownership                            CLOSED -> Ordering
```
