# Scenario-first conformance matrix - design acceptance

This is an executable-spec checklist for later agent work, NOT a claim that these tests already pass in the pushed v12 repository. Scenarios marked P1 are required before Phase-1 closure; P2/P3 deliberately deferred. Test names map to acceptance tests, domain tests, API and command/query projection. Build an actual failing test FIRST for each v12 failure, then implement minimally.

### A. Travel dates, blackout, day/time, advance purchase
| ID | Phase | Given / When | Expected |
|---|---|---|---|
| D01 | P1 | 2027-01-01..2029-09-26 (1000 consecutive dates) | **ONE** permitted range row, exact endpoints; not 1000 date records |
| D02 | P1 | 1000 sparse non-adjacent dates | 1000 1-day ranges (correct sparse representation); batch persistence without N+1 |
| D03 | P1 | two adjacent permitted intervals | merge to one canonical Draft interval; semantic union unchanged |
| D04 | P1 | two overlapping permitted intervals | canonical union, no duplicated matching dates |
| D05 | P1 | a single permitted date | one Start=End row |
| D06 | P1 | permitted Apr01-30, blackout Apr10-12 | Apr09 allowed, Apr11 denied, Apr13 allowed |
| D07 | P1 | only blackout Dec25, no permitted range | Dec24 allowed, Dec25 denied (not deny-all) |
| D08 | P1 | no permitted range and no blackout | no date restriction |
| D09 | P1 | missing EndDate or EndDate < StartDate | reject before save |
| D10 | P1 | repeated same blackout / permitted range | reject exact duplicate; overlap canonicalize only in Draft |
| D11 | P1 | boundary Start=End=DateOnly.MaxValue | safe canonicalizer; no overflow |
| D12 | P1 | date condition on CheckIn basis for a hotel | uses hotel check-in local calendar, not flight date |
| D13 | P1 | insurance CoverageStart or eSIM Activation | applies its own date basis, never invents a flight |
| T01 | P1 | Allow Mon-Fri 09:00..17:00 | one weekday-mask window; Mon 09:00 allow; Sat 10:00 deny |
| T02 | P1 | Allow Mon 08:00..12:00 + Deny Mon 09:00..10:00 | 08:30 allow, 09:30 deny, 12:00 not allowed |
| T03 | P1 | Deny Friday whole day, no Allow windows | Friday deny, Saturday allowed |
| T04 | P1 | Sat 22:00 through Sun 02:00 | represented by Sat window with open end + Sun window with open start; 01:00 Sun allowed |
| T05 | P1 | mask = 0 or mask >127, or Start>=End if both finite | reject |
| T06 | P1 | overlapping Allow/Deny window | Deny wins; deterministic output |
| T07 | P1 | exact local flight departure is next UTC day | match origin-local departure time/date |
| T08 | P1 | required timezone missing or DST local instant ambiguous | UnsupportedContext, no guess |
| T09 | P1 | no Allow with two Deny windows | allow all other weekly windows |
| T10 | P1 | arbitrary invalid effect enum or duplicate window | reject |
| A01 | P1 | advance purchase=24 hours, sales 23h before occurrence | ineligible; verify elapsed duration |
| A02 | P1 | advance purchase=1 local day | calendar-day semantics distinct from 24 elapsed hours |
| A03 | P1 | SameTimeAsTicketed on standalone SIM | publication rejects incompatible ticket context |

### B. Passenger, fare, flight, sale, geography, conflict semantics
| ID | Phase | Given / When | Expected |
|---|---|---|---|
| E01 | P1 | only ADT whitelist | CHD does not satisfy positive allow-list |
| E02 | P1 | two age bands [0,65), [65,+inf) | at 64 selects first; at 65 selects second; no overlap |
| E03 | P1 | age-based rule missing DOB/service-start context | UnsupportedContext, fail closed |
| E04 | P1 | origin THR and destination IST | correct directional itinerary matches; reverse does not unless BothDirections |
| E05 | P1 | service location IKA lounge without itinerary | service-location selector works, does not require flight origin |
| E06 | P1 | SIM covers country TR | coverage country restriction, not arbitrary sale POS country |
| E07 | P1 | Allowed POS=1, customerType=Agency | both must match, not either |
| E08 | P1 | FareFamily=5 AND RBD=2 AND Cabin=Y | all populated fare dimensions must match |
| E09 | P1 | FlightNumber=A123 AND OperatingAirline=4 | both must match |
| E10 | P1 | no Fare condition for independent hotel | no AirFare context required |
| E11 | P1 | condition references Fare for standalone service with no fare-context contract | reject publication or explicit UnsupportedContext; no guessing |
| E12 | P1 | one invalid reference code or duplicate route/reversed BothDirections | reject |
| E13 | P1 | NotAvailable seq=10 for flight 123, Paid seq=100 default | flight 123 blocked; flight 124 Paid; no fall-through |
| E14 | P1 | NotAvailable seq=10 for a customer, Paid seq=100 all | named customer blocked; others get Paid |
| E15 | P1 | only a broad NotAvailable matches | explicit deny, not NoMatch |
| E16 | P1 | no active Provision matches | NoMatch, no invented default price |
| E17 | P1 | same sequence active across one ServiceDefinition | filtered unique rejects race |
| E18 | P1 | missing country/age/flight context for populated restriction | UnsupportedContext; empty restriction needs none |

### C. Pricing and charging units
| ID | Phase | Given / When | Expected |
|---|---|---|---|
| P01 | P1 | PerPassenger insurance ADT [0,65)=20, ADT [65,+inf)=40 | distinct selectable rates, NEVER sum 60 |
| P02 | P1 | shared transfer ADT=18, CHD=9, INF Free | separate Free INF Provision or approved equivalent; no zero Paid base |
| P03 | P1 | PerVehicle price=80, qty=2 vehicles | quantity unit Each compatible; no PTC rate |
| P04 | P1 | PerRoom price=45, qty=2 rooms | compatible; no implicit per-night charging without approved duration rule |
| P05 | P1 | PerItem SIM price=12 | each item, no PTC/age |
| P06 | P1 | PerSeat price=15 and seat number but no aircraft context | reject inconsistent seat authoring |
| P07 | P1 | PerPiece baggage with Quantity.Unit=Kilogram | reject conflicting units |
| P08 | P1 | PerKilogram baggage with Quantity.Unit=Piece | reject conflicting units |
| P09 | P1 | PerPassenger with Quantity.Unit=Kilogram | reject conflicting units |
| P10 | P1 | one base 25, tax 2.5, fee 1, same selector | 28.5 UnitTotal; retain code and jurisdiction |
| P11 | P1 | duplicate base, orphan tax, duplicated tax code, age overlap | reject each separately |
| P12 | P1 | amount 10.123, negative tax, zero Paid base, missing CurrencyId | reject |
| P13 | P1 | two Active prices under one Provision under concurrent requests | DB uniqueness + domain conflict |
| P14 | P1 | atomically switch price v1 -> v2 under active Paid Provision | no gap and no double active; stale expected old ID conflicts |
| P15 | P1 | Free wheelchair requiring SSR booking or free benefit requiring EMD | Free can still have booking/document policy without active Pricing |
| P16 | P1 | same service identity revised PerPassenger -> PerVehicle | reject; require new ServiceDefinitionRef |
| P17 | P1 | two seasonal price periods | two correctly conditioned Provisions with own price versions, not two simultaneous active prices for same Provision |
| P18 | P1 | SameTimeAsTicketed but independent service | reject unsupported connection |

### D. Thirteen airline-family authoring fixtures and nonflight family proof
| ID | Phase | Product | Required authoring proof |
|---|---|---|---|
| F01 | P1 | Extra baggage | PerPiece, route + flight date + prepaid descriptors; Paid/Free/NotAvailable |
| F02 | P1 | Sports/special baggage | aircraft restriction, weight/size descriptor, blackout |
| F03 | P1 | Wheelchair | SSR/booking metadata, Free and NotAvailable, no fabricated industry sub-code |
| F04 | P1 | Meal | PTC child fare/cabin/flight conditions, booking metadata |
| F05 | P1 | Travel insurance | CoverageStart, two age prices and upper-age exclusions |
| F06 | P1 | Paid seat | PerSeat, aircraft and seat characteristics; occupancy NOT owned here |
| F07 | P1 | Lounge | service location + applicable date/time + passenger price |
| F08 | P1 | Priority boarding | FlightDeparture, selected flight/date windows |
| F09 | P1 | Fast track | ServiceStart and POS + blackout |
| F10 | P1 | Wi-Fi | PerItem, supported flight/aircraft |
| F11 | P1 | Pet | PerItem, flight/route and booking metadata |
| F12 | P1 | Meet & Assist | ServiceStart and service airport + passenger charging |
| F13 | P1 | UMNR | CHD eligibility, Free/NotAvailable, local time period |
| F14 | P1 | Hotel room | PerRoom, CheckIn and service city, 2 room units, no forced flight |
| F15 | P1 | SIM/eSIM | PerItem, CoverageCountry and Activation basis, no forced flight |
| F16 | P1 | Private transfer | PerVehicle, service/route location, two vehicle units |
| F17 | P1 | Shared transfer | PerPassenger with ADT/CHD prices and explicit infant policy |

### E. Migration, lifecycle and API acceptance
| ID | Phase | Given / When | Expected |
|---|---|---|---|
| M01 | P1 | v12 1000 adjacent TravelDate rows | 1 new permitted interval preserving same matching date set; audit mapping preserved |
| M02 | P1 | v12 TravelDates + SeasonalPeriods both set | **intersection** preserved (prior v12 AND semantics), never naive union |
| M03 | P1 | v12 blackout and weekly Deny overlapping | deny precedence unchanged |
| M04 | P1 | historic active Provision in Orders / Reservation reference | ID/history still readable, no reassign or destructive collapse |
| M05 | P1 | old primitive JSON columns still present in DB | not read by new authoring after backfill; single source of truth; no auto-drop without verified evidence |
| M06 | P1 | command DB and query DB during backfill | equivalent eligibility, prices and statuses; no orphan rows |
| M07 | P1 | two clients edit Draft rule group | conflict/optimistic concurrency; stable IDs for untouched rows |
| M08 | P1 | writer edits Active Provision child | fails; author new Draft/successor |
| M09 | P1 | rate switch with concurrent Sell/Publish | atomic, historical read snapshots intact |
| M10 | P1 | REST read/list and update each typed group | complete round-trip; list summary not 1000 children |
| M11 | P1 | existing Hold/Get/Confirm files and tests | byte-level functional freeze; no changed behavior |
| M12 | P1 | build/migration/legacy dataset | clean build, tests, pending model check, reversible migration clone and audited exceptions |

### F. Deferred stages
| ID | Phase | Scenario | Required future outcome |
|---|---|---|---|
| S01 | P2 | finite airport lounge capacity by occurrence/date | availability and adjustments with optimistic concurrency |
| S02 | P2 | unlimited wheelchair service | no artificial finite counter |
| S03 | P2 | external hotel capacity | supplier-managed, not copied as authoritative local stock |
| S04 | P2 | airline seat | source of truth FlightFlow, not duplicate seat stock |
| R01 | P3 | partially confirmed multi-service order | per-unit outcome + provider refs + retry handling |
| R02 | P3 | hold expiry and concurrent release | no double allocation, idempotent/reconciled |
| R03 | P3 | cancel/issue/EMD and refunds | verified Ordering document/EMD contracts, not hypothetical |

## Definition of scenario completion
Each P1 scenario requires (a) a domain behavioral test or structurally precise truth table, (b) command/read-model persistence proof where data is stored, (c) API validation and realistic error code for failures, (d) data migration fixture where applicable. A green suite merely proving 1000 individual rows is NOT sufficient for D01. Review the test assertions themselves, not counts of green tests alone.
