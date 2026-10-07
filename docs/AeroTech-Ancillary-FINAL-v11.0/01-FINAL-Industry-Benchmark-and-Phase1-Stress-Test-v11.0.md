# 01 — FINAL Industry Benchmark and Phase 1 Stress Test v11.0

## 1. Benchmark conclusion

The simplest robust structure is the industry-standard separation between **service identity/definition** and **provisions/rules**.

ATPCO Optional Services describes:

- a **Services Record** that identifies what the optional service is and classifies it by sub-code/group/sub-group/description and booking/document characteristics;
- a **Provisions Record** that specifies travel, passenger, geography, carrier, flight, fare and sales requirements plus applicable fees.

That maps cleanly to:

```text
AncillaryServiceDefinition ~= service/S5-like concept
AncillaryProvision         ~= provision/S7-like concept
```

This is the central v11 design. Do not add a second Product/PricingRule domain, generic DSL or EAV structure.

## 2. Shopping boundary benchmark

Flydubai exposes:

```text
POST /pricing/flightswithfares   -> flight shopping
POST /pricing/services           -> ancillary shopping
POST /pricing/seats              -> priced seat-map shopping
```

Its public `AncillaryOffers` description says it provides the list of ancillaries for selected flights with price and available inventory.

v11 uses this only to freeze the boundary:

> Ancillary authors/publishes definitions and filed commercial rules. A later shopping service such as AirAvail applies the concrete itinerary/passenger/fare context and builds saleable offers.

No runtime shopping evaluator is built in Ancillary Phase 1.

## 3. IATA alignment

IATA Service Definition describes a separately sellable/deliverable non-flight product/service such as bag, seat or meal. v11 preserves a stable numeric `ServiceDefinitionId` for technical identity and commercial classification fields for airline/industry meaning.

## 4. Phase 1 stress test — representative families

The v11 generic model must prove these families without dedicated aggregates for each family.

| ID | Family | Modeling shape | Required proof in Phase 1 |
|---|---|---|---|
| FAM01 | Extra/prepaid baggage | ServiceDefinition + BaggageApplication + Provision | piece/weight package, paid/free/not-available, route/flight/date/PTC pricing |
| FAM02 | Sports/special baggage | ServiceDefinition + BaggageApplication or Standard where no baggage-specific field is needed | different service definitions/SKUs and fixed prices |
| FAM03 | Wheelchair/special assistance | Standard + BookingDefinition/SSR | WCHR/WCHS/WCHC-style definitions can be represented; free or paid outcome configurable |
| FAM04 | Meal | Standard + booking metadata | different meal definitions; price by PTC/flight/route/date if desired |
| FAM05 | Travel insurance | Standard | separate plan definitions; journey/order coverage; ADT/CHD pricing and sales/travel windows |
| FAM06 | Paid seat selection | SeatApplication | price rules by aircraft/seat number or seat characteristic; no live-seat state |
| FAM07 | Airport lounge | Standard; industry `0BX` when applicable | supplier-specific lounge definitions and prices; airport/route/PTC/date conditions |
| FAM08 | Priority boarding | Standard | paid/free by fare family/cabin/PTC/POS |
| FAM09 | Fast track | Standard | airport/route/date/POS/fare rules |
| FAM10 | Wi-Fi | Standard | flight/aircraft/cabin/fare-family pricing |
| FAM11 | Pet service | Standard + booking metadata | sector/journey applicability and fixed price |
| FAM12 | Meet & Assist / CIP | Standard | airport/route/PTC/POS pricing |
| FAM13 | UMNR / special handling style service | Standard + booking metadata | passenger-type/sales/travel authoring without age-rule invention |

## 5. What this stress test deliberately does NOT invent

The following are not added merely to make every conceivable supplier product fit:

```text
age bands
frequent-flyer tier
traveller occurrence index
insurance underwriting schema
SIM activation schema
supplier credential/configuration schema
arbitrary JSON attributes
generic keyword rules
nested boolean rule expressions
external quote pricing
dynamic pricing
percentage-of-fare math
live seat inventory
```

If a real supplier later proves one is necessary, the owner can authorize it in a later phase.

## 6. Industry sub-code rule

The current source contains a deliberately small read-only `IndustryServiceSubCodeReference` with known entries such as `0BX` lounge and `0CC` first excess bag.

Phase 1 MUST NOT invent ATPCO industry codes for the other fixtures.

Rule:

- use `Industry` only when the code is in the authoritative read-only reference dataset;
- otherwise use the existing `CarrierDefined` path with explicit carrier classification fields;
- do not create a mutable ServiceSubCode master aggregate;
- do not hard-code unverified industry semantics to make a test pass.

## 7. References

- ATPCO Optional Services overview: https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/what_are_optional_services.htm
- ATPCO Create/Update Services: https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/Create_and_Update_Services.htm
- ATPCO Provisions: https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/Create_and_Update_Provisions.htm
- flydubai booking APIs: https://developer.flydubai.com/browse/api-doc-banner?apirefrence=try_out
- IATA NDC / Offers & Orders: https://www.iata.org/en/programs/airline-distribution/retailing/ndc
