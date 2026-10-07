# 01 — FINAL Industry Benchmark & Traceability v9.1

**Status:** implementation evidence and decision traceability.  
**Supersedes:** every earlier Ancillary benchmark document.  
**Rule:** this document does not authorize concepts that are absent from `00` and `02`. Industry features that AeroTech cannot source correctly today stay in the Gap Register and do not become nullable/dead fields.

---

## 1. Benchmark hierarchy

The benchmark is intentionally layered:

1. **ATPCO Optional Services / FareManager** — primary filing and optional-service commercial semantics.
2. **IATA NDC + EMD** — offer/service/document association vocabulary and accountable document semantics.
3. **Amadeus Altéa / Sabre airline retailing** — PSS behavior and separation of ancillary/seat shopping from fulfillment.
4. **flydubai public developer API** — real airline API evidence for ancillary shopping, priced seats and commitment identifiers.
5. **Lufthansa public developer API** — additional evidence for static/dynamic seat separation, service-provider identity and settlement/document semantics.
6. **Navitaire / Radixx / Hitit / IBS** — carrier PSS breadth and merchandising boundary checks.
7. **Accelya / Datalex / PROS** — modern airline product-catalog, merchandising and Backoffice operational benchmark.
8. **Current AeroTech source** — the implementation constraint and integration vocabulary that the agent must preserve.

A lower item never overrides a higher semantic authority merely because its public API is easier to access.

---

# 2. ATPCO Optional Services / FareManager

## 2.1 Public references

- `https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/what_are_optional_services.htm`
- `https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/Create_and_Update_Services.htm`
- `https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/Create_and_Update_Provisions.htm`
- `https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/View_Provisions_Details.htm`
- `https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/Search_for_Provisions.htm`

## 2.2 Industry semantics confirmed

ATPCO separates the identity/classification of an optional service from the commercial conditions under which that service is available or charged. The public Optional Services material exposes the same families of dimensions used by AeroTech:

```text
service type / subcode
service group / subgroup / descriptions
RFIC / document behavior
SSR / booking behavior
sequence / precedence
passenger type
geography / route
carrier / flight / equipment
travel dates and day/time
cabin / RBD
fare / ticketed-fare qualifiers
advance purchase
PADIS seat characteristics
baggage application
availability
fee application
settlement / refund / commission concepts
```

ATPCO also supports dimensions such as age, passenger occurrence and frequent-flyer status. Those are **benchmark-supported future dimensions**, not v9.1 implementation fields, because AeroTech does not yet have the authoritative runtime inputs required to evaluate them safely.

## 2.3 AeroTech mapping

| ATPCO concept | AeroTech v9.1 |
|---|---|
| Service identity / S5-like classification | `AncillaryServiceDefinition` |
| Commercial provision / S7-like sequence | `AncillaryProvision` |
| Carrier/industry service subcode | `ServiceSubCode` + `SubCodeSource` |
| Industry subcode semantics | read-only `IndustryServiceSubCodeReference` |
| Sequence | `AncillaryProvision.Sequence` |
| PTC | `PassengerCriteria.PassengerTypeCodes` |
| Geography | `TravelCriteria.RoutePairs`, origin/destination/via |
| Flight/equipment | `FlightIds`, `FlightNumbers`, `AircraftIds` |
| Date/day/time | `TravelCriteria` |
| Fare | `AirFareIds`, `AirFareTypes`, `FareFamilyIds`, `FareBasisCodes` |
| Cabin/RBD | `CabinClassIds`, `RbdIds` |
| Seat characteristics | `SeatApplication.SeatCharacteristicCodes` |
| Exact carrier seat exceptions | `SeatApplication.SeatNumbers` constrained by aircraft applicability |
| Baggage application | `BaggageApplication` |
| Fee applies per | typed `FeeApplicationUnit` |
| Availability result | `CommercialOutcome.Available/NotAvailable/Free` |
| Effective dates | service/provision sales/travel dates |
| Age / occurrence / FF tier | **Gap only; absent from v9.1 code** |

**Decision:** `ServiceDefinition + Provision` is a deliberately simplified typed implementation of proven optional-service filing behavior. It is not a generic rules engine.

---

# 3. IATA NDC retailing model

## 3.1 Relevant pattern

NDC service retailing consistently uses the following separation:

```text
Offer / ALaCarteOffer
  OfferItem
    passenger association
    journey / segment association
    price
  ServiceDefinition reference

Order / servicing
  selected service
  passenger / segment association
  fulfillment/document state
```

This supports AeroTech's split:

```text
Ancillary     = commercial authority + supplier/quota fulfillment
AirAvail      = offer evaluation and retailing composition
Ordering      = accepted sale + servicing orchestration + accountable documents
FlightFlow    = physical seat state
```

## 3.2 Internal IDs are not NDC public identifiers

NDC reference IDs are message identifiers. They do **not** require AeroTech to use a string `ServiceDefinitionRef + Version` as an internal foreign key. AeroTech v9.1 uses numeric internal IDs while presenting stable offer/service reference IDs at API boundaries.

---

# 4. IATA EMD

Industry EMD semantics remain unchanged from previous packs:

```text
EMD-A = associated document
EMD-S = standalone document
RFIC  = Reason For Issuance Code
RFISC = Reason For Issuance Sub-Code / service subcode semantics
```

AeroTech rule:

- `AncillaryServiceDefinition.Document` records what document behavior the service requires.
- Some real supplier integrations may eventually require a supplier-side activation/reference step, but v9.1 does not invent such an operation without a concrete supplier contract.
- **Ordering owns accountable EMD issuance, number allocation, coupon state, void/refund history and audit.**
- Ancillary never becomes the financial/document source of truth merely because it owns the sold service definition.

Current Ordering source already contains `ElectronicMiscDocumentIssued`, EMD coupon contracts, `EmdType`, and `ProviderInteractionType.IssueEmd`; this source evidence reinforces the boundary.

---

# 5. Amadeus Altéa / Amadeus NDC

Public Amadeus/Altéa retailing material consistently separates:

```text
AirShopping / flight offer
ServiceList / ancillary services
SeatAvailability / seat map
OrderCreate / OrderRetrieve / servicing
```

Relevant consequences for AeroTech:

1. Seat shopping may be presented together in one retailing UX but requires seat-specific physical state.
2. ServiceList-style ancillary shopping belongs in the offer/retailing composition layer, not in fulfillment APIs.
3. Accepted services are associated to passengers and itinerary components.
4. Ticket/EMD fulfillment is a distinct concern from discovery.

No Amadeus-only opaque internal structure is copied.

---

# 6. Sabre

Public Sabre airline/NDC materials expose the same high-level behavior:

- get ancillaries / ancillary catalog in a shopping or servicing flow;
- seat availability as a seat-specific operation;
- post-booking servicing;
- PNR/order context as the basis for later shopping and commitment.

The legacy `RetrieveSeatAvailabilityList` page previously supplied by the user was not reliably retrievable and therefore no hidden schema from it is treated as authority. Publicly verifiable Sabre behavior is used only at the semantic level.

---

# 7. flydubai Developer API — concrete airline API benchmark

## 7.1 Source

Public developer portal / OpenAPI exposed at `developer.flydubai.com`, including the current try-out/OpenAPI definition.

The inspected API includes these real operations:

```text
POST /pricing/flightswithfares
POST /order/cart
POST /pricing/services
POST /pricing/seats
POST /cp/summaryPNR
POST /cp/commitPNR
POST /order/payment/processFOP
POST /cp/RetrievePNR
```

## 7.2 Ancillary shopping

`POST /pricing/services` is an ancillary-shopping operation. Its public description and schema show that ancillary shopping returns service/SSR price and inventory/rule information after the flight/cart context is known.

Observed service-quote concepts include:

```text
code
categoryID
currency
amount
ruleID
ruleDesc
maxCountFlightLevel
maxCountServiceLevel
quantity
cutoffHours
```

Request context includes flight/route/date/carrier/channel/currency facts.

### AeroTech consequence

This directly supports:

```text
context -> ancillary offers
commercial rule evaluation -> price + max quantity
accepted service -> later commitment
```

It also supports the choice that AirAvail, not Ancillary fulfillment endpoints, composes the customer-facing price.

## 7.3 Seats are a distinct priced retailing operation

flydubai exposes `POST /pricing/seats` separately from generic `pricing/services`.

Observed seat-offer concepts include:

```text
seat number
serviceCode
amount
currency
assigned
ruleID
ruleDesc
blocked / pre-blocked state
```

### AeroTech consequence

AeroTech keeps a distinct `SeatOffers` retailing operation while sharing the same commercial ServiceDefinition/Provision authority. Physical seat availability remains FlightFlow-owned.

## 7.4 Commitment uses technical identity, not a business-key reconstruction

In flydubai PNR summary/commit data, selected special services carry a numeric `ServiceID` together with flight/passenger/amount/currency/status facts.

This is important evidence for the v9.1 simplification:

```text
ServiceDefinitionId / ProvisionId / OrderServiceId
```

are the internal commitment identities. AeroTech does **not** reconstruct a sold service later from:

```text
ServiceDefinitionRef + Version
```

The business reference/version can remain display/audit metadata.

## 7.5 What AeroTech deliberately does not copy

AeroTech does not copy flydubai's exact session/cart/PNR schema, field names, or provider-specific booking mechanics. The benchmark is used for the behavioral facts above only.

---

# 8. Lufthansa Developer API — additive benchmark only

Lufthansa is used only where it strengthens an already established design decision.

## 8.1 Static aircraft/seat reference versus dynamic seat offer

Lufthansa public APIs distinguish reference resources such as aircraft/seat details from dynamic seat-map/offer information for a concrete flight/date/cabin.

### AeroTech consequence

This corroborates the already source-verified boundary:

```text
AirInfo      = aircraft/static layout/reference authority
FlightFlow   = concrete flight seat state/availability
AirAvail     = priced seat offer composition
Ancillary    = commercial applicability/price rule authority
```

No seat inventory is moved into Ancillary.

## 8.2 Service provider and settlement identity

Lufthansa's public Orders/Services response model exposes service-level concepts such as:

```text
ServiceID
ValidatingCarrier
Settlement.Method
InterlineSettlementValue
FeeOwner
Present To / service-provider identity
RFIC / RFISC
service group/subgroup/type
```

Document/settlement methods include distinctions corresponding to direct settlement and associated/standalone EMD behavior.

### AeroTech consequence

This is additive evidence that the commercial owner/validating airline and the **actual provider/supplier of a service do not have to be the same concept**.

Therefore v9.1 restores explicit `Supplier` identity and does not limit Ancillary to airline-internal fulfillment.

This does **not** mean Lufthansa exposes an AeroTech-style `Supplier` aggregate, and the Pack does not claim that it does. The benchmark supports the separation of provider/settlement identity.

---

# 9. Navitaire

Navitaire/New Skies is used as a boundary benchmark:

- airline flight ancillaries such as seats/bags/priority are native airline-retailing concepts;
- broader Travel Commerce can include cars/hotels/insurance/transportation/activities with materially different inventory/product semantics.

AeroTech v9.1 conclusion:

- third-party **supplier-backed ancillary** is allowed inside Ancillary;
- this does not force hotel/car/general travel products into the Ancillary bounded context when they already have their own domain semantics.

The boundary follows product semantics, not the legal ownership of the supplier.

---

# 10. Radixx

Radixx is retained as evidence for LCC-style ancillary retailing, seat selection, bags and servicing. No inaccessible/private API field is treated as authority.

The useful behavioral benchmark is:

```text
shop ancillary / seat
associate passenger + flight
commit to reservation
service later
```

---

# 11. Hitit Crane

Hitit public product material supports:

- ancillary merchandising;
- configurable service sales;
- seat/baggage and post-booking servicing;
- distribution across airline/agency/digital surfaces.

It reinforces the need for surface-aware commercial applicability without introducing vendor-specific entities.

---

# 12. IBS iFly

IBS public airline-passenger-solution material supports:

- ancillary merchandising;
- paid seats and baggage;
- multi-channel retailing;
- post-booking servicing.

Again, this supports the behavioral scope, not copying private internals.

---

# 13. Jazeera public behavior

The previously referenced non-public/test Swagger was not reliably accessible. Public Jazeera sales flows still demonstrate airline extras such as seats, baggage, meals, lounge/priority and assistance.

v9.1 does not invent any Jazeera-private contract fields.

---

# 14. Accelya FLX ONE

Public references:

- `https://w3.accelya.com/products/flx-product-catalog/`
- `https://w3.accelya.com/products/flx-merchandising/`

Useful benchmark points:

- central airline product/catalog management;
- flight + ancillary merchandising;
- channel-aware distribution;
- ancillary sales before and after ticketing;
- seat/baggage merchandising;
- lifecycle separate from an individual shopping offer.

### AeroTech mapping

```text
stable commercial definition -> Ancillary
channel/context evaluation     -> AirAvail
accepted order snapshot        -> Ordering
fulfillment                    -> Ordering -> provider/Ancillary
```

---

# 15. Datalex

Datalex public merchandising/product material supports airline-user authoring of products, applicability and pricing rather than code-level configuration.

AeroTech consequence:

- Backoffice is operational, not decorative;
- bulk/matrix authoring may compile to Provisions;
- production behavior is tested with `POST Backoffice/v1/AncillaryOffers` in **AirAvail**;
- Ancillary does not get a second live evaluator merely to support Backoffice simulation.

---

# 16. PROS

PROS airline retailing/pricing material supports contextual offer optimization and dynamic retailing as an industry capability.

AeroTech v9.1 intentionally does **not** implement an inactive `ExternalQuote` method or dynamic-pricing placeholder. When a real provider/optimizer contract exists, it can be added as a concrete capability. Until then it remains a future gap rather than dead code.

---

# 17. Marketplace / supplier conclusion

The combined evidence does not support the v8 restriction “Ancillary means only services supplied by the airline itself.”

The final v9.1 rule is:

```text
OwnerAirlineId = carrier commercial owner / tenant
SupplierId     = party/system responsible for supplying the ancillary
```

Examples that are structurally valid:

```text
airline-operated lounge
contracted lounge provider A
contracted lounge provider B
airline-operated meal
third-party catering supplier
internal quota-based service
external provider-backed service
```

Different suppliers may require different reservation/confirmation/validation/cancellation behavior. A separate post-confirm supplier issuance/activation operation is **not** part of v9.1 CORE because no current real supplier contract requires it.

`Supplier` therefore belongs in the commercial/fulfillment model. Behavior classification is explicit (`Local | External`) and external routing uses a stable provider key; connector credentials and network configuration remain implementation/integration concerns.

---

# 18. Single evaluator conclusion

No benchmark requires two independent eligibility engines for one published ancillary catalog.

The final AeroTech rule is simpler:

```text
Ancillary -> owns authoritative definitions/provisions and validates their structure
AirAvail  -> owns the single runtime evaluator
```

Backoffice production-behavior simulation calls AirAvail. Any Ancillary-side preview is static authoring validation only.

This eliminates rule drift by construction.

---

# 19. Runtime identity conclusion

Industry public APIs routinely expose message/business references externally while using compact technical IDs internally. flydubai's numeric `ServiceID` is a concrete public airline example.

AeroTech v9.1 therefore freezes:

```text
ServiceDefinitionId long
ProvisionId         long
SupplierId          long
OrderServiceId      long
```

for internal runtime relationships.

`ServiceDefinitionRef` and `Version` remain useful metadata, not relationship keys.

---

# 20. Money / FX conclusion

This is a platform-source decision, not copied from a vendor PSS:

- AirPrice query source explicitly persists fare `Amount` as `decimal(18,2)`.
- AeroTech price/accepted-money fields use CLR `decimal` and platform monetary precision.
- ROE remains high precision where already required by the platform (`decimal(19,9)` in Ordering exchange-rate snapshot).
- AirAvail owns retail currency conversion.
- Ancillary Hold does not receive or validate converted selling money.

Therefore v9.1 removes the contradictory `AcceptedRevenue`/selling-currency validation from Ancillary fulfillment.

---

# 21. Industry subcode validation conclusion

ATPCO optional-service subcodes carry industry semantics and cannot be safely validated by string-length checks alone.

Final rule:

```text
SubCodeSource = Industry
  -> must resolve in organization-approved IndustryServiceSubCodeReference
  -> canonical semantics are validated/copied
  -> caller cannot redefine them

SubCodeSource = CarrierDefined
  -> caller supplies permitted carrier-defined semantics
  -> semantics freeze when the definition is published
```

The donor's two-row hard-coded `IndustrySubCodeReference` proves the intended validation mechanism but is **not** a production-complete dataset.

---

# 22. Traceability matrix — v9.1 corrections

| v9.1 decision | Industry / real-world evidence | AeroTech source evidence | Final rule |
|---|---|---|---|
| ServiceDefinition + Provision | ATPCO S5/S7-style optional-service filing | donor/catalog history | keep typed model |
| Marketplace suppliers | Lufthansa service-provider/settlement distinctions; real airline contracted service patterns | donor has Supplier aggregate | restore minimal `Supplier` root |
| Internal ID relationships | flydubai selected SSR uses numeric `ServiceID` | AeroTech domain uses long IDs broadly | ID-based runtime, refs are metadata |
| Shopping separate from commitment | flydubai `/pricing/services`, `/pricing/seats` vs PNR commit | AirAvail already has AncillaryOffers/Details | AirAvail shops; Ordering commits |
| Separate seat shopping | flydubai `/pricing/seats`; Amadeus/Sabre/Lufthansa seat operations | FlightFlow owns seat state | separate SeatOffers, shared commercial rules |
| Static vs live seat state | Lufthansa reference vs dynamic seat map; PSS benchmark | AirInfo aircraft layout + FlightFlow flight seat map | no seat inventory in Ancillary |
| Single evaluator | simple catalog/evaluation separation; no benchmark for duplicated evaluator | AirAvail already has `AncillaryOfferEvaluator` | only AirAvail evaluates live rules |
| Post-order one-way composition | order-servicing pattern | Ordering already owns order facts | Ordering -> AirAvail, never reverse fetch |
| Hold without FX | shopping/commit separation | AirAvail owns conversion; Ancillary has no FX | no amount/currency check in Hold |
| FareFamilyId | standard fare-family applicability | AirPrice has canonical ID | carry canonical ID end-to-end |
| Industry subcode reference | ATPCO optional-service semantics | donor has reference lookup mechanism | read-only authoritative reference dataset |
| EMD authority | IATA EMD | Ordering has EMD contracts and IssueEmd interaction type | Ordering issues accountable EMD |
| Supplier-specific fulfillment | provider/service separation | Ordering provider capability model + Ancillary supplier mapping | `SupplierId` identifies; `FulfillmentKind + FulfillmentProviderKey` route behavior |
| No dead qualifier fields | ATPCO proves concepts, not current input availability | current contexts lack reliable age/FF inputs | gap only until source exists |

---

# 23. Final benchmark verdict

`BENCHMARK_CONFORMANCE_GO`

v9.1 keeps the previous ATPCO/IATA/Amadeus/Sabre core and adds two concrete public-airline validations without changing those foundations:

1. **flydubai** validates shopping/commit separation, separate seat pricing and compact technical service identity.
2. **Lufthansa** reinforces static/dynamic seat separation and the distinction between commercial/validating carrier, service provider and settlement/document behavior.

No v9.1 CORE behavior exists only because it sounds generic or future-proof. Every CORE behavior is backed by industry semantics, a real airline/PSS pattern, current AeroTech source, or a necessary simplification that removes an already identified contradiction.
