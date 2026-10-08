# ATPCO / AirPrice benchmark and scope selection

## Independently checked ATPCO official resources
1. ATPCO Optional Services introduction: https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/what_are_optional_services.htm . S5 describes WHAT is offered; S7 describes passenger/travel/geography/carrier/flight/fare/sales requirements and fees; S7 has a sequence ordering. S7 supports merchandise, flight-related and ticket-related services (subject to type-specific constraints).
2. ATPCO S7 authoring: https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/Create_and_Update_Provisions.htm . Fields actually include Travel First/Last, Travel Date Table, Day/Time first/last with Day/Range, Flight/Equipment, Passenger Type and age, Sales/Ticket dates, Advance Purchase, and Free/NotAvailable.
3. ATPCO Travel Date Table: https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/Travel_Date_Table.htm . **Each table row is First + Last (a date RANGE)**. Exact dates are a degenerate range. Its first/last travel fields must not be combined with the Travel Date Table at the same time. The table permits recurring month/day formats; we intentionally DEFER annual recurrence.
4. ATPCO S7 Results: https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/View_Provisions_Results.htm . Codes distinguish `Not Available`, `Free` with varying booking/EMD requirements. Fee Applies Per is independent of service classification; actual provision fee may have travel coverage dimension.
5. ATPCO S7 Details: https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/View_Provisions_Details.htm . Optional rule blocks are displayed under location, cabin, ticketed fare, application, travel, settlement etc.
6. Amadeus fare-rule category definitions (additional cross-check for Cat 2/3/4/5/11/15 terminology): https://amadeusdev.service-now.com/csm/en/fare-rules-category-description?id=kb_article&sysparm_article=KB0017983 . These fare-rule categories are **naming/semantics inspiration**; they are not Ancillary S7 category IDs.
7. ATPCO Fare Rules Category 5: https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Rules/Advance_Reservations_and_Ticketing_%285%29.htm . Difference between 24 elapsed hours and one calendar day must not be erased by `TimeUnit` matching.

## AirPrice exact code examples examined
- `.../AirFareAggregate/ValueObjects/DayTime/DayTimePermissionRule.cs` and `DayTimePermissionTimeRange.cs`: typed permitted weekday and time windows.
- `.../AirFareAggregate/ValueObjects/Seasonality/SeasonalityRule.cs` and `PermittedTravelDateRange.cs`: multiple positive start/stop periods.
- `.../AirFareAggregate/ValueObjects/Blackouts/BlackoutsRule.cs` and `TravelBlackoutDateRange.cs`: multiple denied periods.
- `.../AirFareAggregate/ValueObjects/FlightApplication/FlightApplicationRule.cs`: flight and aircraft restrictions.
- `.../AirFareAggregate/ChainHandlers/Reservation/SeasonalityChecker.cs`, `BlackoutChecker.cs`, `DayTimeChecker.cs`: execution only, not copied into Phase-1 Ancillary.
- `.../AirFareAggregate/AirFare.cs` and `.../Persistence/AirFareAggregate/AirFareEntityTypeConfiguration.cs`: typed rule blocks + relational `OwnsMany` rows.
- `.../AirChargeAggregate/Entities/AirChargeCondition.cs`: explicit typed geographical and fare-related fields (not a generic JSON rule DSL).
Canonical link prefix: https://github.com/aliifarhadi/Aerotech.AirPrice/tree/1b41f08e9a22d3c27b807d5ea275120f0f7273ad/AeroTech.AirPrice/AirPrice/src

## Naming crosswalk: NOT an ATPCO certification claim
| Fare Rules inspiration | Actual S7 analog | Adopted local name | Phase-1 scope |
|---|---|---|---|
| Cat 2 Day/Time | Day of week / Day-Range / time | `ProvisionDayTimeApplicationRule`, `ProvisionDayTimeWindow` | Allow and Deny windows with day mask; local service occurrence |
| Cat 3 Seasonality | Travel First/Last and Travel Date Table | `ProvisionTravelDateRule`, `ProvisionPermittedTravelPeriod` | multiple absolute inclusive date intervals |
| Cat 11 Blackout Dates | NotAvailable/conditional travel applicability patterns; fare-rule analog | `ProvisionBlackoutPeriod` | explicit exclusion windows taking precedence |
| Cat 4 Flight Application | CXR/Flight / equipment | `ProvisionFlightApplicationRule` | carrier/flight/equipment selectors |
| Cat 5 Advance Purchase | Advance Purchase Period/Unit / Same Time As Ticketed | `ProvisionAdvancePurchaseRule` | minimum lead time and optional ticket-purchase tie only |
| Cat 15 Sales Restrictions | Sales dates / POS/private distribution | `ProvisionSalesRestrictionsRule` | sales window and POS/customer allow lists |
| PTC/age | PSGR Type, Age Min/Max | `ProvisionPassengerEligibilityRule` | eligibility, separate from rate selector |
| Geographic qualifiers | LOC1, LOC2, Via and scope | `ProvisionGeographyRule` | selected airport routes + service location/coverage-country |
| Fare/booking | Carrier/Fare Class, Cabin, RBD | `ProvisionFareApplicationRule` | chosen existing AirPrice identifiers |
| Outcome and fee | S7 Free/NotAvailable/Chargeable + Fee Applies Per | `CommercialOutcome`, `AncillaryPricing` | preserve 3 distinct dispositions and booking/document flags |

## Explicitly not replicated
Fare-rule tariff/footnotes, ATPCO proprietary table numbers, annual recurring month/day seasons, private distribution security-table DSL, OW/RT travel construction algorithms, stopover algorithms, negotiated fare tables, upgrades, mileage formulas, price percentages, tax calculation, EMD issuance and concurrence. They are deferred until real contract/need, not silently approximated.

## Frequent Ancillary coverage proof
- Extra baggage (piece/weight), seat, meal, priority, fast track, wheelchair, pet, lounge, UMNR, airport assistance, Wi-Fi.
- Hotel/room, travel insurance (by age), SIM/eSIM, private vehicle transfer, shared passenger transfer.
- Itinerary-linked and nonflight service date basis are distinguished; never assume a flight departure exists for a hotel or SIM.
