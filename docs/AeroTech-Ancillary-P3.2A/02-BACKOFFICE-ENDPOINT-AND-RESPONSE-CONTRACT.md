# 02 — Backoffice AncillaryOffers API: request, canonical projection and DTO contract

## 1. Route and inbound contract
```
POST /Backoffice/v1/AncillaryOffers/
Authorization: Bearer <authenticated-backoffice-token>
Content-Type: application/json

{"offerId":"<priced-AirAvail-OfferId>"}
```
One and only one public business input: **OfferId**. No `ownerAirlineId`, `pointOfSaleId`, `customerId`, fare, price, age or `isAvailable` accepted from body. Resolve commercial scope server-side. `GET` is not used because OfferId can be lengthy and exposes commercial identifiers in URLs. Route may be canonicalized by ASP.NET Core; accept requested trailing slash and test it. Existing framework may wrap response `BaseResult<T>`; conform after source audit, do not create a parallel response envelope.

Request DTO: `BackofficeGetAncillaryOffersRequest { OfferId:string required }` with bounded validated size based on actual `JourneyOfferIdCodec` use and existing validation style; no guessed arbitrary destructive length restriction. Prefer `POST` query/handler conventions even though operation is read-only.

## 2. Shape for airline operators, not a raw domain dump
```text
BackofficeAncillaryOffersResponse
  OfferId:string
  EvaluatedAtUtc:DateTimeOffset
  Currency:{CurrencyId:int, CurrencyCode:string?}
  FlightOffer:{LastTicketingDate:DateTimeOffset?, SourcePricedAt:DateTimeOffset?, BaseAmount:decimal?, TotalAmount:decimal?}
  Scope:{OwnerAirlineId:int, PointOfSaleId:long, ShoppingStage:"PreOrder"}
  Journey:{Portions:[{PortionRef,Sequence,Direction?,OriginAirportId,DestinationAirportId,Flights:[{FlightRef,FlightId,FlightVersion,FlightNumber,MarketingAirlineId,OperatingAirlineId,OriginAirportId,DestinationAirportId,DepartureAt,ArrivalAt,CabinClassId?,RbdId?,AircraftId?,Legs?}]}]}
  Travellers:[{TravellerRef,TravellerIndex,PassengerTypeCode,IncludedBaggageByFlight:[{FlightRef,CheckedPieces?,CheckedWeight?,CheckedWeightUnit?,CabinPieces?,CabinWeight?,CabinWeightUnit?}], FareByFlight:[{FlightRef,AirFareId?,FareBasis?,FareFamilyText?,FareFamilyId?,CabinClassId?,RbdId?}]}]
  Categories:[{Code:AncillaryProfile,Label,OfferCount}]
  Offers:BackofficeAncillaryOffer[]
  BlockedOffers:BackofficeAncillaryOffer[] // actionable diagnostics, NOT sellable
  Diagnostics:[{Code,Severity,MissingFacts?,Source?}]
  Completeness:{FlightOfferVerified:bool,ShoppingContextComplete:bool,HasBlockedProducts:bool,LiveSupplierAvailabilityChecked:bool}

BackofficeAncillaryOffer
  CandidateIdentity:{DefinitionVersionId,ProvisionId,CoverageScope,TravellerRef,CoverageRef,PricingRevisionId?} // technical re-evaluation key, NOT purchase token
  Service:{ServiceDefinitionRef,DefinitionVersion,Profile,VariantCode,CommercialName,Description?,ServiceTypeCode,ServiceSubCode,RFIC?,RFISC?}
  Coverage:{Scope,TravellerRefs,FlightRefs,PortionRefs}
  Eligibility:{Status,ReasonCodes[],MissingContextFields[]}
  Selection:{Kind,RequiredFields:[{Name,Type,Required}],ZeroQuantityMeansNoSelection}
  Quantity:{Unit,MinPerSelection,MaxPerSelection,CumulativeLimits?}
  Pricing:{Origin:Filed|Free|ExternalQuote|NotAvailable,Status,CurrencyId?,BaseAmount?,CompleteUnitTotal?,AddedTaxes[],IncludedTaxes[],AppliedUnitFees[],UnappliedFees[],IsOrderLevelTotalComplete,QuoteProviderKey?}
  Availability:{Authority?,ConfigState,CheckedState,IsGuaranteed:false,RequiresCheckAtReserve:true|false,ConsumptionPreview?,ReasonCodes[]}
  Confirmation:{BookingMethod,ConfirmationRequirement,ProviderConfirmationStatus}
  Document:{Routing,DocumentType,RFIC?,RFISC?,DocumentRequired}
  OfferReadiness:existing P3.1 enum
  DisplayState:"AvailableForSelection"|"NeedsInput"|"QuoteRequired"|"NeedsVerification"|"Unavailable"|"NotEligible"
  CanPurchase:false // P3.2.A has no purchase-bound token or reservation protocol
  ReasonCodes:[string]
```

- Keep **canonical result as source of truth**. Names/types of Engine canonical statuses retained without remapping to fabricated successful statuses. DisplayState is *pure* deterministic translation for Backoffice UI; exactly 1 mapping function covered by exhaustive enum tests.
- `Offers[]` includes `Selectable`, `NeedsSelection` and `NeedsQuote` **as discoverable catalog**, with `CanPurchase=false` while this endpoint lacks quote/selection/purchase authority; NeedsQuote is NOT a priced offer. A product requiring unavailable quote source may be categorized in BlockedOffers if readiness NeedsVerification. `BlockedOffers[]` includes `NeedsVerification` (and optionally rejected/unavailable if an existing diagnostic filter asks) so operators can see source gaps. Do not silently hide all products under blocked references.
- **Do not conflate** `Eligibility.Eligible` with `Availability.Guaranteed` or with an accepted Order. `Unlimited` is not `Guaranteed`. No fake remaining counts or date of provider check.
- Group `Categories` by existing 9 `AncillaryProfile` values for UI navigation; do not turn Lufthansa's 22 display categories into new DB aggregates. Profiles and Variants remain authoritative; presentation can later localize categories without changing Engine or pricing.
- Unit pricing, Quantity, Taxes, Fees, Document and booking conditions **must be retained per candidate**; do not flatten one blanket `Price` for multi-flight/multi-passenger offers. When price is `QuoteRequired` or incomplete, amount remains `null`, NOT 0. Free services have Free origin and **no fabricated 0 tax/price item**. `IsOrderLevelTotalComplete=false` when fee bases remain unapplied.
- Flights in `Journey` are supplied to help BackOffice display a usable operator screen. Preserve *original* `FlightOffer` flight totals distinctly from Ancillary price totals.
- Never echo OfficeId, internal ACL, customer secrets, payment token, raw travel documents or private provider payloads. Actor/Office used internally for authorization only.

## 3. Illustrative sample (DELIBERATELY synthetic; not a captured AirAvail response)
```json
{
  "data": {
    "offerId": "<priced-offer-id-from-airavail>",
    "evaluatedAtUtc": "2026-10-11T07:00:00Z",
    "currency": { "currencyId": 47, "currencyCode": "EUR" },
    "flightOffer": { "lastTicketingDate": null, "sourcePricedAt": null, "baseAmount": 250.00, "totalAmount": 285.00 },
    "scope": { "ownerAirlineId": 10, "pointOfSaleId": 211, "shoppingStage": "PreOrder" },
    "journey": { "portions": [{ "portionRef": "BOUND-1", "sequence": 1, "originAirportId": 101, "destinationAirportId": 202,
      "flights": [{ "flightRef": "BOUND-1:F1", "flightId": 991100, "flightVersion": 1, "flightNumber": "XX101", "marketingAirlineId": 10, "operatingAirlineId": 10,
        "originAirportId": 101, "destinationAirportId": 202, "departureAt": "2026-12-01T08:00:00+03:30", "arrivalAt": "2026-12-01T11:00:00+04:00" }] }] },
    "travellers": [{ "travellerRef": "PT1", "travellerIndex": 1, "passengerTypeCode": "ADT",
      "includedBaggageByFlight": [{ "flightRef": "BOUND-1:F1", "checkedPieces": 1, "checkedWeight": 23, "checkedWeightUnit": "Kilogram" }],
      "fareByFlight": [{ "flightRef": "BOUND-1:F1", "airFareId": 4321, "fareBasis": "EXAMPLE", "fareFamilyText": "LIGHT", "fareFamilyId": null }] }],
    "categories": [{ "code": "Baggage", "label": "Baggage", "offerCount": 1 }],
    "offers": [{
      "candidateIdentity": { "definitionVersionId": 801, "provisionId": 901, "coverageScope": "Portion", "travellerRef": "PT1", "coverageRef": "BOUND-1", "pricingRevisionId": 501 },
      "service": { "serviceDefinitionRef": "EXTRA_BAG", "definitionVersion": 1, "profile": "Baggage", "variantCode": "A01", "commercialName": "Extra checked bag", "serviceTypeCode": "C", "serviceSubCode": "0CC" },
      "coverage": { "scope": "Portion", "travellerRefs": ["PT1"], "flightRefs": ["BOUND-1:F1"], "portionRefs": ["BOUND-1"] },
      "eligibility": { "status": "Eligible", "reasonCodes": [], "missingContextFields": [] },
      "selection": { "kind": "QuantityChoice", "zeroQuantityMeansNoSelection": true, "requiredFields": [{ "name": "Quantity", "type": "Integer", "required": true }] },
      "quantity": { "unit": "Piece", "minPerSelection": 1, "maxPerSelection": 2, "cumulativeLimits": [] },
      "pricing": { "origin": "Filed", "status": "Complete", "currencyId": 47, "baseAmount": 35.00, "completeUnitTotal": 40.00,
        "addedTaxes": [{ "code": "TX", "amount": 5.00 }], "includedTaxes": [], "appliedUnitFees": [], "unappliedFees": [], "isOrderLevelTotalComplete": true },
      "availability": { "authority": "Unlimited", "configState": "Unlimited", "checkedState": "NotChecked", "isGuaranteed": false, "requiresCheckAtReserve": false, "reasonCodes": [] },
      "confirmation": { "providerConfirmationStatus": "NotRequested" },
      "document": { "routing": "Emd", "documentType": "EmdAssociated", "documentRequired": true },
      "offerReadiness": "Selectable", "displayState": "AvailableForSelection", "canPurchase": false, "reasonCodes": []
    }],
    "blockedOffers": [],
    "diagnostics": [],
    "completeness": { "flightOfferVerified": true, "shoppingContextComplete": false, "hasBlockedProducts": false, "liveSupplierAvailabilityChecked": false }
  }
}
```
**Sample caveats:** all IDs/names/prices above are invented EXAMPLE values, not Airbus/AEGEAN/Lufthansa tariffs or verified AirAvail IDs; the serialized enum object shape must match the real framework convention (`EnumValueDto` vs string). The JSON demonstrates semantics, NOT a promise that every sample value is active in the user's database. The sample intentionally marks `shoppingContextComplete=false` due to missing external inputs.

## 4. Error and status matrix
| Condition | Required behavior |
|---|---|
| No/invalid OfferId or search-only (unpriced) OfferId | 400/422 via project error conventions; no 200-empty |
| Unauthorized or wrong airline office/customer scope | 401/403, no leaked offers/customer context |
| POS unresolved/ambiguous | 409/422 typed `POS_CONTEXT_UNRESOLVED`, not fallback to arbitrary POS |
| Upstream AirAvail unavailable/timeout/500 or response envelope errors | typed 502/503 according to current framework; safe failure, log correlation, no fake success |
| Upstream returns no journey/flights/tickets or mismatched OfferId | treat inconsistent/not found with non-success result; no green empty list |
| Active catalog has no applicable published ancillary | 200 with valid journey, `offers=[]`, diagnostic `NO_APPLICABLE_PRODUCT` (not an upstream failure) |
| Candidate needs verification/quote/seat choice | 200 with proper `DisplayState`, `CanPurchase=false`; not silently 0 priced |
| Flight price invalidated or source context no longer authoritative | typed source/validation failure or NeedVerification, NEVER silent booking authority |
| Caller expects `ServiceOfferId` to buy | Not provided by this endpoint; future QuoteSelection/token gate only |

## 5. Scope protection
This API is a **Backoffice read-only discovery operation**. No DB migration, no provider hold/confirm, no direct `POST AirAvail/AncillaryOffers`, no mutation in Ordering, no EMD issuance. The Backoffice DTO is a projection of canonical Engine candidates and traveler journey context, not a separate rule engine.
