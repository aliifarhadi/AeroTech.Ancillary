# 02 — Three Surface Contracts, Mapping, Samples & Evidence

## Normative benchmark evidence

| Evidence | Source | Binding design consequence |
|---|---|---|
| IATA NDC 21.3 ServiceListRQ/RS reference implementation (Condor) | https://docs.ndc.condor.com/docs/21_3/ndc_messages/IATA_ServiceListRQ_RS.html | Offer/Order context; `ALaCarteOffer`, `OfferItem`, `Eligibility`, separate `ServiceDefinition` with descriptions/rich content; explicit pax + flight associations. This is **NDC-inspired JSON**, not a claim of direct IATA-certified XML compliance. |
| IATA NDC 21.3 Offer/Order model | https://docs.ndc.condor.com/docs/21_3/offer-order-structure.html | Reuse service definition data; scoped service OfferItems, not one flattened universal product price. Seat groups/seat map are distinct. |
| IATA NDC 21.3 OfferPrice | https://docs.ndc.condor.com/docs/21_3/ndc_messages/IATA_OfferPriceRQ_RS.html | Selection of flight + service + seat offers precedes final priced order-ready commercial offer. Mere discovery quote cannot claim payable final total. |
| Sabre NDC API catalog | https://developer.sabre.com/product-collection/new-distribution-capability-ndc/v1/index.html | Separate Get Ancillaries, Get Seats, Reprice Order and Order Management. Sabre Get Ancillaries documentation describes **free-of-charge** NDC ancillaries, so do not infer that this one API sells every paid ancillary. |
| flydubai booking APIs | https://developer.flydubai.com/browse/api-doc-banner?apirefrence=try_out | `POST /pricing/services` for available/priced ancillary services; distinct priced-seat-map `SeatOffers`. |
| Lufthansa Developer — Seat Maps | https://developer.lufthansa.com/docs/read/api_details/offers/Seat_Maps | Seat row, seat number, characteristic codes, cabin details and physical layout are a separate sourced resource. Do not synthesize SeatMap from Ancillary definition metadata. |
| Lufthansa Customers/Orders reference | https://developer.lufthansa.com/docs/read/api_partner_privileged/orders/v2/Customers_and_Orders_Response | Passenger/segment association, baggage unit/dimension/code detail, service description, RFIC/RFISC and issued-document contexts. Reference shape, not proof of a public Lufthansa `/baggage/details` endpoint. |
| AEGEAN baggage policy | https://en.aegeanair.com/travel-info/travelling-with-aegean/baggage/add-baggage/ | Free allowance, first excess piece, overweight (23–32kg), oversize, route/fare/time and nonrefundability matter; baggage details cannot be a generic title and price alone. |
| Amadeus developers FAQ | https://admin.developers.amadeus.com/self-service/apis-docs/guides/developer-guides/faq/ | Self-Service additional checked bags via `additionalServices` on Flight Offers Price; not all NDC operations are exposed in the same product. Do not advertise direct Amadeus parity without conversion/spec tests. |

**Benchmark level:** Behavioral semantics and NDC-style data relationships. API is our REST/JSON product with stable versioning, NOT an unmodified Lufthansa/Amadeus/Sabre wire protocol. Provide a traceability table per field and explicit differences.

## Endpoints and inbound contracts (must be three separately declared DTO classes, even if fields match)

| Surface | Search route | Request DTO (exact) | Auth / context |
|---|---|---|---|
| BO | `POST /Backoffice/v1/AncillaryOffers` | `BackofficeAncillaryOffersRequest {offerId:string, options?: {customerId?:string,countryId?:long,pointOfSaleId?:long,salesDate?:DateTimeOffset,preferredCurrencyId?:int}}` | `SurfaceAuthorization.Backoffice`; source-style explicit options |
| Agency API | `POST /Api/v1/AncillaryOffers` | `ApiAncillaryOffersRequest {offerId:string}` | `SurfaceAuthorization.Ota`; agency, office, customer claims; X-Country |
| Agency panel | `POST /OtaPanel/v1/AncillaryOffers` | `OtaPanelAncillaryOffersRequest {offerId:string}` | `SurfaceAuthorization.OtaPanel`; source OTA context as on AirAvail; no Backoffice options |

Each surface also owns `POST /{surface}/v1/AncillaryOffers/Details` with request `{offerId, offerItemRef}` plus BO's `options?`; and `POST /{surface}/v1/AncillaryOffers/Selections/Evaluate` with `{offerId, offerItemRef, selection}` plus BO's `options?`. Same policies, no cross-surface token reuse. These are read-only operations; no claim of accepted payment/physical reservation.

## Output mapping law

1. Shared canonical model: source itinerary/travellers, Engine candidate, status/price/quantity/availability/confirmation and source provenance. Do not drop facts upstream just because one surface is compact.
2. BO response: operational screen. Rich journey, fare basis, allowance, category counts, `offers[]`, `blockedOffers[]`, diagnostics/missing-source facts, full selection schema, tax/fee breakdown and readiness. Existing BO JSON may be retained via new **BO-owned** DTO types, with compatibility snapshot tests. BO must *not* read Query generic presentation types.
3. API response: NDC-inspired, small + normalized. `dataLists.passengers`, `dataLists.paxSegments`, `dataLists.serviceDefinitions`, `aLaCarteOffers[].offerItems[]`. Each OfferItem has `offerItemRef`, `serviceDefinitionRef`, `eligibility.passengerRefs[]`, `eligibility.segmentRefs[]`, `price.unit`, priceStatus, quantity constraints, selection requirements, service/document codes, availability/readiness, reasons when not confirmed, and no raw internal DB numeric ids or private company diagnostics. `dataLists` entries must be deduplicated while every offer item has explicit associations. Do not advertise NDC XML compatibility or fabricate `OfferItemId` with booking authority.
4. OtaPanel response: agent-friendly, grouped by traveller and bound/flight, one selection row per service with clear price and current status, count and included baggage prominently displayed. No internal technical diagnostics dump. Separate typed DTO, even if composition uses private field-formatting helpers.
5. All DTOs preserve `currencyId` and resolved `currencyCode` only if verified, and money precision. `price.status` + `unitTotal:null` for ExternalQuote/incomplete; `total:null` if quantity or cross-unit fee basis is not established. Displaying price from the flight offer must be separately labeled, not summed into ancillary amount. Avoid PNR or EMD for pre-order.
6. Free service = `free`, not a positive charge; included benefits not to be charged a second time. `OfferReadiness.Selectable` does not guarantee or authorize purchase. Product `Unavailable/NotEligible` excluded from normal API sales list; BO can include blocked catalog for operator investigation. For API, include actionable `requirements` for NeedsInput/NeedsQuote and omit or separately flag unverified options, never claim confirmed availability.

## Wire examples — illustrative shape ONLY, invented IDs/prices (must be turned into working golden snapshots)

### Backoffice request
```json
{"offerId":"<priced-AirAvail-offer-id>","options":{"customerId":"501","countryId":98,"pointOfSaleId":211,"salesDate":"2026-10-11T10:00:00Z","preferredCurrencyId":70}}
```

### Backoffice response (abbreviated)
```json
{
 "data":{
  "offerId":"<priced-AirAvail-offer-id>",
  "scope":{"ownerAirlineId":1,"pointOfSaleId":"211","shoppingStage":"PreOrder"},
  "journey":{"portions":[{"portionRef":"B1","flights":[{"flightRef":"B1:F1","flightId":"1001","originAirportId":2,"destinationAirportId":6}]}]},
  "travellers":[{"travellerRef":"ADT-1","passengerTypeCode":"ADT","includedBaggageByFlight":[{"flightRef":"B1:F1","checkedPieces":1,"checkedWeight":23,"checkedWeightUnit":"Kg"}]}],
  "categories":[{"code":"Baggage","offerCount":1}],
  "offers":[{"offerItemRef":"opaque-selection-ref","service":{"variantCode":"A01","commercialName":"Extra checked bag"},"coverage":{"travellerRefs":["ADT-1"],"flightRefs":["B1:F1"]},"pricing":{"priceStatus":"Complete","currencyId":70,"baseAmount":5000000,"addedTaxes":[{"code":"VAT","amount":450000}],"unitTotal":5450000,"orderTotalComplete":true},"readiness":"Selectable","canPurchase":false}],
  "blockedOffers":[],"diagnostics":[],"completeness":{"flightOfferVerified":true,"shoppingContextComplete":false,"liveSupplierAvailabilityChecked":false}
 }
}
```

### Agency API request
```json
{"offerId":"<priced-AirAvail-offer-id>"}
```

### Agency API response — NDC-inspired normalized JSON, not literal NDC schema
```json
{
 "data":{
  "sourceOfferId":"<priced-AirAvail-offer-id>",
  "currency":{"code":"IRR","id":70},
  "dataLists":{
   "passengers":[{"passengerRef":"ADT-1","ptc":"ADT"}],
   "paxSegments":[{"segmentRef":"B1:F1","flightId":"1001","originAirportId":2,"destinationAirportId":6}],
   "serviceDefinitions":[{"serviceDefinitionRef":"EXBAG23","name":"Extra checked bag","serviceCode":"C","subCode":"0CC","description":"One extra checked piece","baggage":{"pieces":1,"maxKgPerPiece":23}}]
  },
  "aLaCarteOffers":[{"offerItems":[{
   "offerItemRef":"opaque-selection-ref",
   "serviceDefinitionRef":"EXBAG23",
   "eligibility":{"passengerRefs":["ADT-1"],"segmentRefs":["B1:F1"]},
   "pricing":{"status":"Complete","unitPrice":{"amount":5450000,"currencyId":70},"base":{"amount":5000000,"currencyId":70},"taxes":[{"code":"VAT","amount":450000}],"additionalFees":[],"totalPrice":null},
   "quantity":{"unit":"Piece","min":1,"max":2},
   "selectionRequirements":[{"name":"Quantity","type":"Integer","required":true}],
   "availability":{"state":"NotChecked","guaranteed":false},
   "readiness":"Selectable","canPurchase":false
  }]}],
  "responseStatus":"NeedsSelectionOrRecheck"
 }
}
```

### OtaPanel request
```json
{"offerId":"<priced-AirAvail-offer-id>"}
```

### OtaPanel response — operational grouped UI, own DTO
```json
{
 "data":{
  "offerId":"<priced-AirAvail-offer-id>",
  "travellerGroups":[{"travellerRef":"ADT-1","passengerTypeCode":"ADT","flightGroups":[{"flightRef":"B1:F1","flightNumber":"XX101","includedCheckedBaggage":{"pieces":1,"weightKg":23},"services":[{"offerItemRef":"opaque-selection-ref","category":"Baggage","title":"Extra checked bag","unitPrice":{"amount":5450000,"currencyId":70},"quantityStep":1,"displayStatus":"AvailableForSelection","requiresRecheck":true}]}]}],
  "blockedCount":0,
  "currencyId":70
 }
}
```

**Example caution:** none of these synthetic airport/product codes or prices is claimed to originate from AEGEAN, Lufthansa, Sabre or actual users. Agent must produce real test fixtures captured from AirAvail with source data redacted and ensure serializer/envelope shape follows local framework. `canPurchase=false` until a real selection/price authority and reservation integration exist, despite the preview price.

## Required negative contract tests

- Exact BO optional fields; API & Panel reject or ignore forged `pointOfSaleId`, `customerId`, `preferredCurrencyId` in body as AirAvail contract requires. Ensure they never affect resolver.
- Three disjoint routes + policies and DTO types. Wrong surface token forbidden **before** invoking upstream provider.
- Same trusted context and eligible POS -> equivalent canonical candidate identities and amounts across 3 surfaces. Presentation shape intentionally differs.
- Two or more authorized POS identities -> no arbitrary first; results stay scoped, are not overwritten, not erroneously merged. No authorized POS -> proper source parity failure/no available products; unavailable reference -> typed failure.
- Multi passenger, two bounds, connection with two sectors, baggage allowance/fare code/units, tax included vs added, Ticket fee, quote required, unavailable, no DOB, no seatmap, missing IANA TZ.
- API must not leak `PricingRevisionId`, `ProvisionId`, internal ACL, raw office ID or diagnostic stack; BO operational diagnostic access only under BO policy.
