# Provision behavior — realistic eligibility + stages + request-confirmation

## Preserve typed v12.1 rules
Candidate order: active Definition, active Provisions sorted `(Sequence ascending)`, evaluate typed group whitelist OR within same dimension, AND across different populated dimensions; `Blackout` and DayTime `Deny` veto; `NotAvailable` is an explicitly matching negative Provision and wins at its sequence; missing required context => Unknown/Unsupported, not true. No generic EAV, rule DSL, hidden matching engine or shopping endpoint in Phase 1.

POS: **D04 separate Provisions**; `ProvisionSalesRestrictionsRule.ProvisionPointOfSale` defines POS keys and Agency/Corporate Customer restrictions. One matched Provision then selects rate by currency/PTC/age. Example POS=DE and POS=TR can select separate prices despite same ServiceDefinition and service. A general fallback provision should use a higher Sequence than specific POS.

## Small extra commercial descriptors
- `PurchaseStage` at Provision: `PreOrder`, `PostTicketed`, `Both`, `LegacyUnspecified`. `LegacyUnspecified` is migration-safety only, not a valid newly published authoring selection. Sales stages describe eligibility, NOT workflow processing; they do not override rules/codes or create new Order APIs. Example Iberia baggage may be PreOrder+PostTicketed; some special equipment/priority after ticketing only by documented carrier policy, NOT globally hardcoded for all airlines.
- `ConfirmationRequirement` on ServiceDefinition Booking VO: `Immediate` vs `SubjectToConfirmation`. `SubjectToConfirmation` is required when service is requestable and supplier/operations may deny later. `Immediate` only means no separate confirmation step is defined in the authoring metadata; it never guarantees physical availability. Future Consumer must return `OnRequest` rather than guaranteed availability; no Phase 3 state or adapter now. Applies WCHC/PETC/UMNR and other service if carrier policy demands it, not all SSR automatically.
- `ProvisionAdvancePurchaseRule`: existing MinimumPeriod plus optional MaximumPeriod (same Unit); semantic `minimum lead <= timeUntilService <= maximum lead` when Maximum exists. Zero min allowed; prevent negative, min>max, unsupported unit, unresolved ServiceDateBasis timezone; no need for dynamic time expressions elsewhere.
- `ProvisionBaggageApplicationRule.ChargeKind`: ExtraPiece|WeightPackage|Overweight|Oversize|SpecialEquipment (typed descriptor, not new Pricing aggregate); optional `AllowanceConcept` Piece|Weight, derived from **source-backed travel baggage policy**, never generic per airline. When `ChargeKind=WeightPackage` the provision must require Weight allowance concept; when `ChargeKind=ExtraPiece`, require Piece, unless a verifiable carrier-specific exception exists. Unknown applicable travel concept in future matching must fail closed. If actual allowance concept is unavailable for a restricted product, future sellability=UnsupportedContext, not guessed. `Weight` on package represents per-unit **entitlement kg**, *not* kg of physical aircraft stock.

## Family mapping examples (source-backed patterns, not product price assertions)
1. Qatar 10kg bundle: ServiceDefinitionRef `XBAG_10KG`; PricingUnit PerItem; QuantityUnit Each; Baggage ChargeKind WeightPackage, Weight=10kg, AllowanceConcept Weight only for qualified routes; Quantity Max defined by carrier policy; no Local FlightWeight by default.
2. Iberia 15kg/23kg/32kg piece: separate Definitions, PricingUnit PerPiece, QuantityUnit Piece, Weight per piece capped; one person/segment eligibility; do not infer all 3 at all flights.
3. Sport bicycle: separate Definition, `ChargeKind=SpecialEquipment`; eligibility route/aircraft, purchase stage carrier-specific; `ConfirmationRequirement` if applicable. No invented quota.
4. WCHR/WCHS/DPNA: Outcome Free; Booking Method SSR, ConfirmationRequirement SubjectToConfirmation as appropriate; no Paid Price, no fake stock count.
5. Seat: commercial paid/Free Provision, seat traits, no local seat occupancy. FlightFlow delegated inventory (only when verified).
6. Priority boarding: Product paid or Free via Fare, perhaps PostTicketed-only for documented channel; per passenger/sector, no fake time-slot.
7. Lounge: supplier access may require capacity check even without locally tracked count; `MustCheckAvailability` true does not imply Local.

## Mandatory authoring invariants
- `BookingDefinition.SubjectToConfirmation` does not mean an entire paid transaction should be immediately treated Confirmed; no Issue/EMD execution.
- Free can require booking/SSR and possibly EMD, irrespective of zero Price; NotAvailable never books/issues.
- Published Definition/Provision/Price historical versions immutable; amendments via new Draft and atomic publish/switch.
- PricingUnit and QuantityRule.Unit compatibility: PerPassenger/PerRoom/PerItem/PerVehicle/PerSeat -> Each; PerPiece -> Piece; PerKilogram -> Kilogram. Fixed 10kg package PerItem -> Each is valid; no contradictory 'PerItem but QuantityKilogram'. If legacy SKU has mismatch: explicit migrate/revise, never automatically reprice.
- ServiceDateBasis FlightDeparture vs ServiceStart vs CheckIn vs CoverageStart vs Activation remains actual context; no global assumption all products tied to one flight.
- All 27 existing typed child rows retain IDs and field types; the four minimal descriptor changes above are the ONLY authorized Phase1 semantic additions beyond Pricing.
