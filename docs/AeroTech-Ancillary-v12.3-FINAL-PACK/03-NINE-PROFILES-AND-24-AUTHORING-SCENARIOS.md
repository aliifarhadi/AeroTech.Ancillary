# 03 — Exact A01–A24 commercial authoring and P2 shopping coverage

**Uniform contract for every row:** one commercial Definition typed `Specification` + shared metadata, any applicable Provision rules (not obligatorily POS-bound), either filed Pricing/ExternalQuote/Free/NotAvailable, an existing P2 Shopping offer/selection evaluation, and a test for foreign fields. No P3 allocation implementation.

| Variant | Profile | Definition fields / policy | Sale/selection/price scope | InventoryPolicy in Ancillary? |
|---|---|---|---|---|
| A01 Extra checked bag | Baggage | ChargeKind=ExtraPiece; allowance concept Piece; MaxKgPerPiece? | Paid per piece, first/last excess ordinal in Provision | Explicit Unlimited, Supplier or justified local resource |
| A02 Extra weight package | Baggage | WeightPackage, PackageWeightKg (e.g. 5/10), Weight concept | Purchased package qty PerItem; quantity ≠ kg | Explicit Unlimited/Supplier, not fake plane weight capacity |
| A03 Overweight | Baggage | Overweight; exclusive/inclusive weight band | Weight/per-piece surcharge, measured bag predicate | Supplier/verified local only if real physical quota |
| A04 Oversize | Baggage | dimensions/linear sum; charge combination | PerPiece; overlap with A03 controlled | Same strict nonSeat inventory options |
| A05 Cabin bag | Baggage | CabinBag variant + existing ExtraPiece kind; cabin dimensional terms | PerPiece | Not inferred as physical aircraft stock |
| A06 Special equipment | Baggage | EquipmentKind/max weight and size | PerItem/PerPiece; equipment selector | Supplier or genuine Count/Weight resource |
| A07 StandardSeat | Seat | SeatPurpose=Standard; optional characteristic/cabins/exit row | PerSeat Filed or Free, traveller+flight, optional specific seat selection | **NO — not applicable** |
| A08 PreferredSeat | Seat | SeatPurpose=Preferred; **nonempty characteristic codes**; cabin/exit row | PerSeat, traveller+flight, matching actual characteristic only if trusted | **NO — not applicable** |
| A09 ExtraSeat | Seat | Purpose EXST/CBBG, RequiresAdjacentSeat?, ExtraOccupiedSeatCount>=1, ticket-routing declaration | PerSeat; actual seat allocation EXTERNAL and future P3 | **NO — not applicable** |
| A10 Cabin upgrade | Upgrade | from/to cabin distinct, fixed/quote/reprice, exchange metadata | Filed only for real fixed product; else trusted ExternalQuote | Never fake Seat stock; nonSeat policy only if genuine independent quota |
| A11 Free special meal | Meal | SSR meal code, cutoff, exclusive family | Free, no rate, supplier approval possible | Explicit Unlimited+check or Supplier; no invented count |
| A12 Paid preorder meal | Meal | MenuItemRef, dietary code, lead time | Paid PerItem/PerPassenger Filed or supported quote | Supplier/verified local Count if genuine |
| A13 PETC | Pet | species, combined weight, carrier dimensions, age, documents | PerItem; external confirmation | Supplier/true carrier-flight Count |
| A14 PET in hold | Pet | species/weight/size bracket, docs, mode Hold | PerItem or valid quote | Supplier or real Count+Weight |
| A15 Wheelchair | AssistedTravel | SSR WCHR/WCHS/WCHC distinct levels, cutoff | Free, confirmation possibly required | Unlimited+check / supplier; not guaranteed |
| A16 Disability assistance | AssistedTravel | allowed SSR codes/communication | Free or approved filed | Unlimited+check / supplier |
| A17 Medical equipment | AssistedTravel | equipment kind, MEDA/AOXY/STCR, medical docs, approval | Free/Filed/ExternalQuote based on actual airline | Supplier/real equipment quota |
| A18 Bassinet | AssistedTravel | infant weight/age/guardian, compatible groups | Free or Filed | Verified bassinet quota / Supplier; seat map itself external |
| A19 UMNR | AssistedTravel | age range, handoff, connection restrictions | charge policy by child/route | Supplier/true capacity or explicit nonfinite check |
| A20 Lounge | AirportService | airport, facility/terminal, window, guest policy | per passenger | Supplier or actual AirportSlot |
| A21 Fast track | AirportService | airport lane/facility, direction, time | per passenger | Supplier or actual AirportSlot |
| A22 CIP meet-and-assist | AirportService | included components, airport/facility, guest quota | package item/passenger, avoid double charge | Supplier or actual AirportSlot |
| A23 Priority boarding/checkin | Priority | zone/group, fare inclusion predicate | PerPassenger filed/free | Explicit Unlimited if no finite resource |
| A24 Onboard Wi-Fi | Connectivity | plan kind/duration/data/devices/aircraft/OnBoard | PerItem/Passenger; onboard only authoring supported | Supplier/capability source, no fake bandwidth |

## Typed family fields, not generic blobs
Baggage: `ChargeKind`, `AllowanceConcept?`, `PackageWeightKg?`, `MaxKgPerPiece?`, `WeightFromExclusiveKg?`, `WeightToInclusiveKg?`, `MaxSize?`, `MaxLinearSumCm?`, `EquipmentKind?`, `ChargeCombination?`.
Upgrade: `FromCabinId`, `ToCabinId`, `AllowedUpgradeKind`, `RequiresTicketExchange`, `EligibleFareFamilyIds[]`.
Meal: `MealKind`, `MealCode?`, `MenuItemRef?`, `DietaryCode?`, `CateringLeadTimeMinutes`, `ExclusiveMealFamilyCode?`.
Pet: `TransportMode`, `AllowedAnimalTypes[]`, `MaxCombinedWeightKg`, `CarrierDimensionsMaxCm`, `MinAnimalAgeWeeks?`, `RequiredDocumentCodes[]`, `AllowedHoldAnimalSizeBrackets[]`.
AssistedTravel: tagged `AssistanceKind`, exactly one of Wheelchair/DisabilityAssistance/MedicalEquipment/Bassinet/UnaccompaniedMinor subdetails, no all-null union.
AirportService: `Kind`, `AirportId`, `TerminalRef?`, `FacilityId?`, `Direction`, `ServiceWindowStart?`, `ServiceWindowEnd?`, `IanaTimeZone?`, `VisitDurationMinutes?`, `MaxGuestsPerPrimary?`, `IncludedComponentCodes[]`, `RequiresSpecificAppointment`.
Priority: `Kind`, `PriorityZoneCode?`, `PriorityGroupCode?`, `FareBenefitRef?`, `AirportIds[]`.
Connectivity: `PlanKind`, `DurationMinutes?`, `IncludedDataMb?`, `MaxDevices?`, `EligibleAircraftIds[]`, `DeliveryStage`, `FulfillmentProviderRef?`.

All nine family specs preserve current EF owned/typed associations and validated A01–A24 registry. Do not introduce a tenth profile or flatten to JSON/EAV. Tests for each variant: define, change, get exact shape, activate, pricing publication/evaluation, negative foreign-field rejection. Availability management tests for **A07/A08/A09** are specifically *no-policy* tests; for nonSeat scenarios they prove truthful source behavior.
