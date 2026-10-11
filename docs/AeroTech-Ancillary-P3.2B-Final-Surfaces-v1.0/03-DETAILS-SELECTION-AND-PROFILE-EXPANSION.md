# 03 — Details, Baggage Expansion, Seat Maps and Selection Preview

## Business distinction

**Search = discovery** of applicable offers and prerequisites. **Details = reliable descriptive product data + selectable parameters**. **Selection Evaluate = apply input and recompute candidate/price if enough trusted evidence**. **Booking = separate future P3.3.** These are separate operations sharing the same canonical engine/context, not surface-dependent business rules.

For all 3 surfaces, implement:

- `POST /{Surface}/v1/AncillaryOffers` (Search; already exists for Backoffice).
- `POST /{Surface}/v1/AncillaryOffers/Details` (Detail, generic dispatch over current Ancillary profile). Body `{offerId,offerItemRef}`; BO optionally `options` in identical AirAvail layout. The response is surface-specific DTO. The Detail MUST re-evaluate the offer candidate and verify caller, source offer and POS; no direct `GET /Definitions/{id}` is sufficient.
- `POST /{Surface}/v1/AncillaryOffers/Selections/Evaluate` (read-only selection). `{offerId,offerItemRef,selection}` + BO options. Uses canonical typed `AncillarySelection` after strict validation; no arbitrary JSON dictionaries. Returns `Accepted` / `Incomplete` / `Rejected` as P3.1; `Accepted` means engine-evaluated input, **not** sale, reservation or provider stock guarantee.

A separate `POST /{Surface}/v1/AncillaryOffers/SeatMaps` is reserved and should be implemented with a **real trusted seat-map provider** when available. If no provider is connected in this phase, return explicit typed `SEAT_MAP_SOURCE_UNAVAILABLE` and do not manufacture seat rows or pretend seats are available. Show the endpoint as unavailable in capabilities, rather than faking seat-map completion. Optional profile routes (e.g. `/Baggage/Details`) are **not** required if generic Details returns a strongly typed baggage payload. Do not add nine redundant controllers.

## Canonical detail contract

`CanonicalAncillaryOfferDetail` (Application/Shopping read model, not a domain aggregate):

- `SourceIdentity`: offer ID, evaluated instant, authorized selling scope; `Candidate`: canonical engine candidate and its identity, current readiness and reason codes;
- `Definition`: published service identity, profile/variant, commercial name, full description, service code/subcode/RFIC/RFISC, booking and EMD routing **from existing authoritative product**;
- `Coverage`: traveler(s), portions, flight refs and supported scope;
- `Terms`: published sales / service applicability, cancellation/refund/waiver and provider confirmation **only where modeled and sourced**. Absent commercial terms are `null` or explicit `NotProvided`, never made up;
- `Options`: selection requirements and constraints already specified in service definition/provision, typed Profile detail (Baggage/Seat/Meal/Pet/AssistedTravel/AirportService/Priority/Connectivity/Upgrade). No untyped catch-all dict;
- `Pricing`: exact canonical `PriceAssessment`, price unit and tax/fee line breakdown; quote/verification state; no recomputation;
- `Availability`: exact canonical state, no invented counts, guarantees or seat map;
- `Completeness`: source-backed, unknown and source-unavailable fields, rather than synthetic defaults.

**Important:** The current engine candidate includes a category/summary and selection contract but not all physical specification fields. Do not bloat every Search candidate with 24 profile-specific details; the **canonical Details projector** retrieves the authorized published Definition/Provision/Price/Inventory rows and returns a fully typed detail model, still source-agnostic. This is *enrichment*, not a second commercial rules engine. Engine decision always wins.

## Baggage Detail: A01–A06 authoritative mapping

Source is existing `AncillaryServiceDefinitionAggregate/Specifications/BaggageSpecification.cs` and selected Provision `BaggageApplication` / `QuantityRule`, trusted included baggage per passenger/flight from AirAvail coupon. Required fields by source availability:

- `chargeKind`: ExtraPiece | WeightPackage | Overweight | Oversize | CabinBag | SpecialEquipment; `allowanceConcept`: Piece | Weight when published;
- `includedAllowance`: checked/cabin pieces, weight, unit by traveler+segment. Show `known=false` if producer does not prove the value; zero allowance is a known zero only when source marks it;
- `packageWeightKg`, `maxKgPerPiece`, `weightFromExclusiveKg`, `weightToInclusiveKg`, `maxSizeCm` (L/W/H), `maxLinearSumCm`, `equipmentKind`, `chargeCombination` **without inventing limits**;
- `quantity`: min/max/step, valid selection field(s), tier range (`FirstExcessPiece`/`LastExcessPiece` where published), coverage and any cumulative count/weight limits;
- `pricing`: Filed/free/quote, per Piece/Item/Kilogram as published, base/added/included tax, fee application unit (Item/Ticket/OneWay/Sector/etc), currency and price-readiness; exact line amounts; do not charge included tax twice;
- `restrictions`: permitted traveller types, fares/fare family, travel dates, route/carrier, deadlines; include only terms available in domain/authoritative data. A booking cutoff in AEGEAN website is a **benchmark**, not a universal literal rule in our database;
- `fulfillment`: selection prerequisites, method, supplier confirmation required, document/RFIC/RFISC if defined; no issued EMD on read;
- `pendingFacts`: e.g. unknown local timezone, approved bag dimensions, usage ledger or supplier inventory. Do not mark free allowance and purchased ancillary quantity as interchangeable.

### Example Baggage Detail (invented values, shape not captured from production)

```json
{
 "data":{
  "offerItemRef":"opaque-selection-ref",
  "service":{"name":"Extra checked bag","variantCode":"A01","serviceCode":"C","subCode":"0CC"},
  "coverage":{"passengerRefs":["ADT-1"],"segmentRefs":["B1:F1"]},
  "baggage":{
   "chargeKind":"ExtraPiece","allowanceConcept":"Piece",
   "includedAllowance":[{"segmentRef":"B1:F1","pieces":1,"weightKg":23,"verified":true}],
   "additionalPiece":{"maxWeightKg":23,"maxLinearSumCm":158},
   "applicableExcessPieceOrdinal":{"from":2,"to":3},
   "purchaseQuantity":{"min":1,"max":2,"step":1},
   "requiresMeasuredWeight":false
  },
  "pricing":{"origin":"Filed","unit":"PerPiece","status":"Complete","unitPrice":{"amount":5450000,"currencyId":70},"addedTaxes":[{"code":"VAT","amount":450000}],"unappliedFees":[],"orderTotalComplete":true},
  "availability":{"status":"NotChecked","guaranteed":false},
  "selectionRequirements":[{"name":"Quantity","required":true}],
  "readiness":"Selectable","canPurchase":false
 }
}
```

**Refusal:** when a required rule is unverifiable (e.g. baggage weight concept, linked earlier baggage purchase or supplier approval), return readable `NeedsVerification` and missing-fact list; do not silently set false or 0. When the selected product is not applicable to the traveler/segment/POS, do not reveal its private pricing via a guessed raw `DefinitionId`.

## Seat Details and seat map (distinct sources)

Seat definition supports `SeatPurpose`, `SeatCharacteristicCodes`, `ApplicableCabins`, `RequiresExitRowEligibility`, `RequiresAdjacentSeat`, `ExtraSeatPurpose`, `ExtraOccupiedSeatCount`, `RequiresExternalTicketAction`. These are **rules/traits**, NOT evidence of actual 12A availability. SeatMap additionally requires trusted flight/aircraft map with row/column/characteristics, actual occupied/blocked/free state and seat-specific price group; a selection of 12A is incomplete without evidence that 12A is valid and available to that traveler. If FlightFlow/seat-map provider is not connected, expose `NeedsVerification`/`SEAT_MAP_SOURCE_UNAVAILABLE` and never a mock seat. Lufthansa public Seat Maps and Condor NDC `SeatAvailability` are behavioral references (links in 02).

## Typed selection evaluation

- Input must be shaped and checked against each variant's `CustomerSelectionContract`; e.g. A01 `Quantity`, A03 measured weight and bag reference where required, A08 actual seat number + accepted exit row terms + verified map, A13 pet weight/dimensions/age proofs where supported. Do not accept customer-supplied `VerifiedSeatCharacteristics` or fake DOB/inventory.
- Return selected candidate changes and `ProvisionChangedBySelection`, current pricing revision and PriceAssessment; if rate changes on selection, recompute from Engine, do not use prior Search price. Decline/Incomplete when another chosen provision supersedes old one.
- `PerKilogram`: derive billable units from **approved commercial definition and verified measured weight**. Gross vs overweight delta vs package size must be explicitly sourced from the relevant rule. No blanket `Quantity` multiplication. If insufficient, `RequestedQuantityTotal=null`, proper reason. Non-Item fees stay unapplied until correctly allocated at order-level quote.
- Payment or Order price totals not part of this API. No signed purchase authority, reservation or document unless a later phase implements and validates those capabilities.

## Profile detail extensibility

Define typed detail components as actual fields already modeled in existing Phase 1/2 specifications, e.g. Meal menu code, Pet size/weight restrictions, AssistedTravel SSR, Lounge facility constraints, Upgrade cabin target, Connectivity plan. For unavailable provider/catalog specifics, return explicit unknown/unavailable; **do not invent 24 payload schemas with fictitious fields**. Tests must exercise A01–A06 and A07–A09 plus sampled A10–A24 using real defined fields. Ensure the API and Panel details have the same decision/pricing but different DTO layout.
