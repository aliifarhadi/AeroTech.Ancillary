# 15 — Evidence catalogue, sources and traceability grades

**Use industry/API evidence for observed behaviors only**. Neither public NDC APIs nor a Lufthansa browser screenshot proves provider's private `AggregateRoot` topology. The v12.2 aggregate design is an AeroTech architectural recommendation grounded in these observed interaction and field patterns.

## 1. Lufthansa owner-supplied primary data in current repo
`feat/ancillary-v12.1-phase2-stock@e23ba293b2578362005840967c3c07aa77dbd174`
- `samples/lufthansa-services-configuration.json` (**~73 KB**, 22 categories, 48 service-code configuration rows, 42 rich-content entries): category UI independent of service dictionary, `BAGGAGE=BOUND`, `PETS=BOUND`, `SEAT=SEGMENT`, `MEAL=SEGMENT`, `INSURANCE=SVC`, `PAXINFO=ITINERARY`. Example `IBAG` Stepper quantity `0–6`, `MBAG` `2–6`, `PETC` TYPE select Cat/Dog, `WCH*` service subtype, `SEAT` seatmap redirect. **Interpretation:** specialized buyer UI with shared service structure; not internal database inheritance evidence.
- `samples/lufthansa-services-by-order.json` (**~382 KB**, 114 services in one sample order): per `travelerIds`, `flightIds`, `quantity`, `quotaStatus`, optional `quota`, prices, SSR parameters, `reasonForIssuance`. Counts previously extracted: guaranteed 59, pending 17, unknown 32, no quotaStatus 6; 17 priced services, six quota fields, 87 `oncePerPassengerPerFlight=true`. A13 `PETC` has mandatory TYPE, length/width/height, weight nested input format; `UMNR` age; `WCHC` pending on one flight and guaranteed another; Rail&Fly `flightIds` may span two flights. **Interpretation:** quota status and physical quantity different; purchase parameters not product-definition fields.
- `samples/lufthansa-one-booking-v2-purchase-orders.json` (**~13 KB**): six purchased IBAG records, quantity=1, traveler and flight refs, status HK, code/subcode, price structure `value=12000,currencyCode=EUR` in raw source. **Do not infer universal minor unit scaling** without contract.
- Owner-supplied B2C screenshots 2026-10-09: cart has Rail&Fly, Car Rental, Pet, Baggage, Seat; baggage direction-dependent free and extra bags, 0..6 stepper, pet outbound unavailable and return "one space left", required acknowledgements.
- Owner-observed browser API call: `POST https://api.shop.lufthansa.com/one-booking/v2/purchase/orders/{orderId}/services?lastName=...`, request `{"services":[{"serviceId":"VAS.CO2.20-80","travelerId":"all","quantity":1}]}`. Request body authenticated context, price validity, response and downstream issuance are NOT contained in sampled 3 JSON files. Preserve this as **owner-observed request**, not an assertion that Lufthansa does it all synchronously.

## 2. Official industry and agency API sources
| Source | Official URL | Specific supported point (not private aggregate inference) |
|---|---|---|
| IATA NDC program | https://www.iata.org/en/programs/airline-distribution/retailing/ndc | Offer/Order data-exchange standard for distribution and airline sellers |
| IATA 21.3 Implementation Guide | https://guides.developer.iata.org/docs/21-3_ImplementationGuide.pdf | ServiceList returns applicable ancillary offerings to seller/flight context |
| Condor NDC 21.3 Offer/Order | https://docs.ndc.condor.com/docs/21_3/offer-order-structure.html | ALaCarteOfferItems and Eligibility, Seat group offers separate; order contains OrderItems/Services |
| Condor NDC methods | https://docs.ndc.condor.com/docs/21_3/access.html | `/shopping/serviceList`, `/shopping/seatmap`, `/offer/price`, `/order/create`, `/order/changeInquiry`, `/order/change` |
| Condor final price | https://docs.ndc.condor.com/docs/21_3/ndc_messages/IATA_OfferPriceRQ_RS.html | group selected offers and price with traveller/segment refs before order |
| Condor post-order change | https://docs.ndc.condor.com/docs/21_3/workflows/Add-ancillaries-to-existing-booking.html | optional changeInquiry simulates, actual change separate |
| flydubai OTA modify flow | https://developer1.flydubai.com/browse/api-doc-banner?apirefrence=modify_flow | separate Ancillary API bags/meals, Seat API seat-map/hold/assign, ModifyPNR and process payment |
| Lufthansa public excess baggage | https://www.lufthansa.com/us/en/uebergepaeck | Weight/oversize/piece commercial differentiation; compare to owner screenshots |
| Emirates additional allowance | https://www.emirates.com/us/english/before-you-fly/baggage/purchase-additional-baggage-allowance/ | pieces vs weight and 5kg bundles according applicable routes |
| Sabre NDC | https://developer.sabre.com/product-collection/new-distribution-capability-ndc/v1/index.html | Offer/Order and Ancillary/Seat distinct API flow (provider/version caveats) |
| Singapore Airlines NDC API | https://developer.singaporeair.com/ndc_api_catalogue | ServiceList and OrderChange servicing surface |
| Iberia NDC dev | https://agencias.iberia.com/en-es/ndc/ndc/para-desarrolladores/documentacion-apis/ | eligibility/ancillary sale and assistance channel support |
| Amadeus Flights guide | https://github.com/amadeus4dev/developer-guides/blob/master/docs/resources/flights.md | context-dependent flight offer ancillary info/pricing, product-specific restrictions |

**Source grading:** `[LH-S]` user samples; `[PUBLIC]` provider-published API/policy; `[DESIGN]` AeroTech model decisions; `[BLOCKED]` source-specific real money/ref/provider quota not verified. If external docs conflict with sample of another airline, never generalize a single provider's policy globally.

## 3. Evidence-to-design mapping
| Observed | v12.2 invariant | Covered Axx |
|---|---|---|
| LH `IBAG` stepper and direction-dependent baggage | separate PurchaseUnit vs physical resource; per-pos/route/portion provision | A01,A02,A03,A04,A05 |
| LH `KBAG`, `BIKE`, `XBGF` specialty | typed size/weight/equipment, potentially provider check | A04,A06 |
| LH PET TYPE/dims/weight/quota and directional sellability | Pet typed spec+buyer input contract; no fake local stock | A13,A14 |
| LH `WCHR/WCHC` `pending/guaranteed` and age of UMNR | AssistedTravel subtypes, confirmation distinct from pricing/stock | A15–A19 |
| LH meal SSR codes and seatmap | Meal and Seat variants and standalone specialized selection | A07–A09,A11,A12 |
| LH Rail&Fly multi-flight | `Portion` may cover several flights but one charged unit | extra integration proof, chapter 12 J4 |
| Condor `ALaCarteOfferItem`, group-based seat offers | future opaque service offer + typed context, not raw SKU | future P3 all |
| Condor changeInquiry/change | Add to Order ≠ final reservation/issue | future P3 |
| flydubai separate ancillary/seat/modify | provider and Ordering boundaries; existing UI may be specialized | A01–A24 / future P3 |

## 4. Findings that sources DO NOT establish
No direct evidence of Lufthansa internal AR inheritance, proprietary ledger, exact first-party request after Add for each service, deep Payment->EMD sequence, real PNR provider state transitions, currency storage scale of raw `12000`, universal operational quota for every PET/UMNR aircraft or one flight count limit for every baggage. The pack does not claim any of those.
