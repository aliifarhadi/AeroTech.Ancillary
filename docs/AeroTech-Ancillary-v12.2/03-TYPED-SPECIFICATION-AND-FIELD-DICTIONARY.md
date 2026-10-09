# 03 — Authoritative typed profiles, variant identifiers and field-level contracts

## 0. Type discipline (mandatory)
The named records below are **specification value objects/owned children**, never 24 independent ARs. Define `AncillaryProfile` enum with explicit stable numeric values: Baggage=1, Seat=2, Upgrade=3, Meal=4, Pet=5, AssistedTravel=6, AirportService=7, Priority=8, Connectivity=9. Define `ServiceVariant` stable codes A01..A24 in a typed source-controlled catalog, **not 24 additional domain aggregates**. `Definition.Profile` must equal `Variant.Profile`. Do not reuse numeric enum values of pre-existing `ProvisionApplicationType` (Standard=1,Baggage=2,Seat=3). Prefer typed code `ServiceVariantCode:string(8)` for future extensibility with a closed v12.2 registry; strong VO validates membership before creation. Carrier-defined new variants require code + typed contract registration, not a dynamic rules engine.

All types below specify fields **owned by airline authoring**. `?` means optional with exact context-dependent activation rules, NOT `null` because unfinished feature. Monetary fields ALWAYS in Pricing and not duplicated in Spec. `OffsetDateTime` means DateTimeOffset UTC canonical time; dimensions in decimal centimeters (18,3) with positive components; `WeightKg:decimal(18,3)`; durations in positive minutes/hours. Existing global AirportId/CabinId/Fare ids reused; don't create parallel catalog entities.

### 1. Baggage (A01–A06)
`BaggageSpecification` one per Definition:
| Member | Type | Mandatory when | Invariant |
|---|---|---|---|
| `ChargeKind` | **existing** `ExtraPiece|WeightPackage|Overweight|Oversize|SpecialEquipment` | always | A05 CabinBag uses Variant=A05 + ExtraPiece, not a new numeric ChargeKind value; preserve existing values 1–5 |
| `AllowanceConcept` | `Piece|Weight` | A01,A02,A05 | No global assumed Piece/Weight; actual fare/route authority at evaluation |
| `PackageWeightKg` | decimal(18,3)? | A02 | >0; e.g. 5kg,10kg sold as `PerItem`; choose units explicitly |
| `MaxKgPerPiece` | decimal(18,3)? | A01,A03,A04,A05,A06 when advertised | Max actual physical piece limit, not capacity |
| `WeightFromExclusiveKg / WeightToInclusiveKg` | decimal? | A03 | coherent positive bracket; exact acceptance based on measured baggage |
| `MaxSize` | `DimensionsCm?` | A04/A05/A06 when advertised | positive L/W/H; if linear-sum constraint also used see `MaxLinearSumCm` |
| `MaxLinearSumCm` | decimal? | A04/A06 if airline terms use linear dimensions | >0; do not confuse with individual dimensions |
| `EquipmentKind` | enum/code? | A06 | Valid industrial code or carrier-defined whitelisted kind (BIKE/SKI/GOLF/etc.) |
| `BaggageChargeCombination` | `Separate|Combined|MutuallyExclusive`? | A03/A04 when overlapping | Avoid charging overweight+oversize twice unless actual airline rule explicitly permits it |

Keep existing `ProvisionBaggageApplicationRule` for conditions (FreePieces, excess piece ordinal ranges, TravelApplication, PurchaseApplication, RuleDeference, applicable weight/charge kind); **single commercial source** for `ChargeKind` is Spec; existing Provision `ChargeKind` must be treated as a bounded override only if evidenced and with equality/explicit predicate, otherwise migrate/derive, not duplicate editable fields. `FirstExcessPiece`/`LastExcessPiece` permit different filed prices for first/second/etc.; owner approved separate POS provisions. External base Fare allowance is resolved at future Offer time, not manually re-filed as inferred number from SKU label. `QuantityRule` applies to purchased units per selection, not remaining aircraft capacity.

### 2. Seat (A07–A09)
`SeatSpecification`:
| Member | Type | Rule |
|---|---|---|
| `SeatPurpose` | `Standard|Preferred|ExtraSeat` | required; A07/A08/A09 |
| `SeatCharacteristicCodes` | `set<string(25)>` | optional for Standard, required for Preferred when category defined |
| `ApplicableCabinIds` | `set<int>` | eligible cabin, if restricted |
| `RequiresExitRowEligibility` | bool | true only if exit-row option |
| `RequiresAdjacentSeat` | bool | A09 only when selling an adjacent extra seat |
| `ExtraSeatPurpose` | `PassengerComfort|CabinBaggage`? | A09 required |
| `ExtraOccupiedSeatCount` | int? | A09 >=1; actual available seats checked by FlightFlow in P3 |
| `RequiresExternalTicketAction` | bool | A09 true when airline EXST/CBBG document policy requires ticketing rather than EMD; no actual Ticket issued in P1/2 |

Reuse ProvisionSeatApplicationRule (seat characteristics / number filters). **Do not store seat occupancy or seat-map availability in Ancillary**. Cabin upgrade is NOT SeatSpec; A10 below.

### 3. Upgrade (A10)
`UpgradeSpecification`: `FromCabinId:int`, `ToCabinId:int`, `AllowedUpgradeKind:FixedAncillary|DynamicQuote|TicketReprice`, `RequiresTicketExchange:bool` (policy derived from documented issuer contract; if unknown mark RequiresExternalQuote rather than fake EMD), `EligibleFareFamilyIds:set<long>`. `FromCabinId != ToCabinId` and class hierarchy validation from real cabin reference; no local flight seats. Dynamic quote mode uses `PriceOrigin=ExternalQuote` and requires trusted provider quote later; no 0.00 amount in `AncillaryPricingRate`.

### 4. Meal (A11–A12)
`MealSpecification`: `MealKind:SpecialRequest|PaidPreorder`, `MealCode:string(4)?` (mandatory for standardized SSR meal), `MenuItemRef:string(40)?` (mandatory paid menu SKU), `DietaryCode:string(20)?`, `CateringLeadTimeMinutes:int>=0`, `ExclusiveMealFamilyCode:string(30)?` (one selection per traveller/segment if configured). Menu variants are separate Definition versions/products where meaning/price differs, not generic arbitrary form JSON. Free SSR A11 uses `PriceOrigin=Free`; A12 filed paid (or supplier quote with explicit origin). Catering supplier checks are not physical stock unless backed by verified allotment.

### 5. Pet (A13–A14)
`PetSpecification`:
| Member | Type | Rule |
|---|---|---|
| `TransportMode` | `Cabin|Hold` | required |
| `AllowedAnimalTypes` | non-empty set `Cat|Dog|RegisteredOther(code)` | country/route restrictions via Provision; real carrier code validated |
| `MaxCombinedWeightKg` | decimal(18,3) | >0; include carrier when the product specifies combined limit |
| `CarrierDimensionsMaxCm` | `DimensionsCm` | every component >0 |
| `MinAnimalAgeWeeks` | int? | >=0; route-specific exception in Provision via explicit typed override |
| `RequiredDocumentCodes` | set<string(30)> | nonempty if travel terms require; codes are airline-approved documentation types |
| `AllowedHoldAnimalSizeBrackets` | set<typed bracket> | A14 if different priced medium/large categories; brackets disjoint |
| `AcceptanceRequirement` | `SubjectToConfirmation` | default for A13/A14 unless authority explicitly proves immediate confirmation |

**P1/2 input schema**: `AnimalType:enum required`, `CombinedWeightKg:decimal required`, `CarrierDimensionsCm:Dimensions required`, `DocumentAcknowledgementCodes:codes[] required if configured`; actual values future P3 only. `quota=2` shown by Lufthansa is NOT shopper's max and NOT authorable unconditional active stock; v12.2 can bind verified animal-carrier FlightCount.

### 6. AssistedTravel (A15–A19) — tagged variants, never one all-null medical entity
`AssistedTravelSpecification`: `AssistanceKind:Wheelchair|DisabilityAssistance|MedicalEquipment|Bassinet|UnaccompaniedMinor` required; **exactly one** typed detail matching kind:
- `WheelchairDetails { AllowedSsrCodes:set<WCHR|WCHS|WCHC>, AssistanceLevelCode?, LeadTimeMinutes:int>=0 }` (A15); WCHR/WCHS/WCHC meanings not merged.
- `DisabilityAssistanceDetails { AllowedSsrCodes:set<BLND|DEAF|DPNA|...>, RequiredCommunicationMethod?:enum }` (A16); do not collect protected health details as catalog fields.
- `MedicalEquipmentDetails { MedicalServiceCode:AOXY|MEDA|STCR|approved-code, RequiresMedicalApproval:bool, EquipmentKind:enum, OxygenUnits?:decimal, EvidenceTypeCodes:set<string> }` (A17); when pricing requires supplier quotation use ExternalQuote.
- `BassinetDetails { MaxInfantWeightKg:decimal?, MaxInfantAgeMonths:int?, CompatibleSeatGroups:set<string>, RequiresInfantAndGuardian:bool=true }` (A18); physical bassinet stock only verified; seat-linked resource belongs external FlightFlow when assignment is required.
- `UnaccompaniedMinorDetails { MinAgeYears:int, MaxAgeYearsExclusive:int, GuardianContactRequired:bool=true, ConnectionPolicy:DirectOnly|ApprovedConnections, AllowedTransitAirportIds?:set<int> }` (A19); guardians' actual identities are P3 buyer inputs, not product configuration. Ages `0<=min<max`.

**Buyer schema by kind:** Wheelchair = choose SSR level; disability assistance = service selection; Medical = approved minimum document refs and equipment quantities; Bassinet = infant+guardian relation; UMNR = traveller DOB, guardian hand-off contacts/pick-up contacts (security/retention policy P3). A17/A19 may be pending, never automatic confirmed due to Paid status.

### 7. AirportService (A20–A22)
`AirportServiceSpecification`:
`Kind:Lounge|FastTrack|CIP`, `AirportId:int`, `TerminalRef:string(30)?`, `FacilityId:long?` (mandatory for Local slot), `Direction:Departure|Arrival|Transfer|Any`, `ServiceWindowStart:TimeOnly?`, `ServiceWindowEnd:TimeOnly?`, `IanaTimeZone:string(64)` if service-local windows are defined, `VisitDurationMinutes:int?`, `MaxGuestsPerPrimary:int?`, `IncludedComponentCodes:set<string(30)>`, `RequiresSpecificAppointment:bool`. 
- A20 lounge: terminal/facility and guests; supplier capacity unless proven airline allotment.
- A21 FastTrack: lane/operating window and passenger.
- A22 CIP: package inclusions (lounge+fast track etc.) and guest surcharge **without duplicate charges**; chosen booking time must resolve an actual facility source. The service spec is not owner of `AirportSlotInventory`; Policy binds to actual facility if verified.
- UTC slot `[start,end)` resolved from IANA timezone; nonexistent/ambiguous DST times rejected without explicit offset. No negative/no-source-guaranteed.

### 8. Priority (A23)
`PrioritySpecification`: `Kind:Boarding|Checkin`, `PriorityZoneCode:string(20)?`, `PriorityGroupCode:string(20)?`, `FareBenefitRef:string(40)?`, `AirportScope:AirportId set?`; never sell an entitlement already included in fare. Buyer input may be simple opt-in per traveller/flight. No fake physical capacity.

### 9. Connectivity (A24)
`ConnectivitySpecification`: `PlanKind:Messaging|Time|Data|FullFlight`, `DurationMinutes:int?`, `IncludedDataMb:int?`, `MaxDevices:int?`, `EligibleAircraftIds:set<int>?`, `DeliveryStage:PreOrder|PostTicketed|OnBoard`, `FulfillmentProviderRef:string(50)?`. At least one plan dimension (duration/data/full-flight); mandatory fields by plan kind; no fabricated airplane bandwidth. If onboard-only, v12.2 enables authoring and excludes invalid preflight sell stage.

## Exactly nine profile-specific tables / owned graphs: constrained storage
Use one owned/child spec record corresponding to 9 profiles (with a small variant-specific subordinate VO/table only where it is really needed). Names: `BaggageSpecification`, `SeatSpecification`, `UpgradeSpecification`, `MealSpecification`, `PetSpecification`, `AssistedTravelSpecification`, `AirportServiceSpecification`, `PrioritySpecification`, `ConnectivitySpecification`. Existing `BaggageApplication/SeatApplication` stay owned by Provision. Child collection tables only for non-scalar `AllowedTypes`, `ApplicableCabins`, `DocumentCodes`, `IncludedComponents`, etc., with typed PK `(DefinitionId,Code)` and unique guards; avoid 24 main tables. Reuse existing owned JSON? **No**: use EF owned relational mapping. All strings length-limited. All decimals explicit precision. No uncontrolled `JsonDocument` or attribute-value bag.

## Variant/profile matching and activation validator
```
ServiceDefinition.Draft:
  profile ∈ 1..9; variant ∈ A01..A24; validated mapping; exactly one spec, family correct
  required characteristic fields complete and normalized
  Booking/Document/ServiceDateBasis/PricingUnit/PriceOrigin compatible
  no contradictions in applicable Rule groups, no mismatched BaggageApplication/SeatApplication
  references are present/valid or activation rejected; unknown source never defaulted
  then Activate/Publish
```

Existing ServiceDefinition and Provision publication logic must be **extended** not bypassed, including `ProvisionApplicationType` old enum compatibility and the `QuantityUnits` map. Do NOT map A10 Upgrade to bogus `Seat=3` simply because it uses flight availability.
