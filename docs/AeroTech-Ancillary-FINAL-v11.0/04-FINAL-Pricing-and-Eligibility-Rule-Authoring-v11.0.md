# 04 — FINAL Pricing and Eligibility Rule Authoring v11.0

## 1. Purpose

This document defines **what analysts can author**. It does not authorize an evaluator in Ancillary.

## 2. Rule composition semantics — frozen for later consumer

Within one list field: OR.

Across different populated fields: AND.

Example authored Provision:

```text
PTC = [ADT, CHD]
AND RoutePairs = [THR -> IST]
AND FareFamilyIds = [5, 6]
AND TravelFrom = 2026-12-20
AND TravelTo   = 2026-12-31
```

Later consumer meaning:

```text
(ADT OR CHD)
AND THR->IST
AND (FareFamily 5 OR 6)
AND date in range
```

The Phase 1 code only stores/validates/returns the rule.

## 3. Price differentiation by passenger type

Do not create `AdultPrice`, `ChildPrice`, `InfantPrice` columns.

Use separate Provisions:

```text
ServiceDefinition: Lounge Access

P10: PassengerTypeCodes=[ADT] -> 25 EUR
P20: PassengerTypeCodes=[CHD] -> 15 EUR
P30: PassengerTypeCodes=[INF] -> Free or NotAvailable
```

This supports arbitrary future PTC values without adding price columns.

## 4. Price differentiation by travel/flight

Example:

```text
ServiceDefinition: Extra Bag 20KG

Provision 10
  FlightIds = [81234]
  TravelFrom = 2026-12-20
  TravelTo   = 2026-12-31
  Price      = 30 EUR

Provision 20
  RoutePair = THR->IST
  FareFamilyIds = [5]
  Price = 25 EUR

Provision 100
  broad/default rule
  Price = 20 EUR
```

Phase 1 proves all three rows can be authored, persisted and read back exactly.

It does **not** assert which row wins for an actual passenger/flight because runtime evaluation belongs to a later owner-authorized phase.

## 5. Sales window vs travel window

Keep distinct:

```text
SalesEffectiveFrom / SalesDiscontinueAt
```

= when the rule may be sold.

```text
TravelFrom / TravelTo
```

= travel dates for which the authored rule applies.

The Backoffice must display both distinctly.

## 6. Fare dimensions

Phase 1 authors exact conditions for:

```text
AirFareId
AirFareType
FareFamilyId
FareBasis
CabinClassId
RbdId
```

No wildcard fare-basis DSL and no display-name identity.

## 7. Sales dimensions

Phase 1 authors:

```text
PointOfSaleIds
CustomerIds
CustomerTypes
```

Do not add PCC/office/channel duplicates without an authoritative source decision.

## 8. Fixed price

Phase 1 price is filed fixed money.

```text
UnitTotal = sum authored price lines
```

The authoring system does not calculate selling-currency FX.

If a business wants a different filed price for another market/currency, author a distinct Provision with appropriate sales criteria.

## 9. Tax/Fee evidence

Price lines may carry:

```text
Category = Ancillary | Tax | Fee
Code
Name
CountryId?
StationAirportId?
UnitAmount
```

Do not invent tax jurisdiction logic in Phase 1.

## 10. Sequence

`Sequence` is an explicit analyst-controlled priority metadata field.

The later intended matcher is lowest sequence first. There is no specificity score.

Phase 1 should prevent duplicate active `ServiceDefinitionId + Sequence` rows if the existing persistence pattern can enforce it cleanly, but it must not implement matching.

## 11. No hidden evaluator

The following are forbidden in Phase 1 even as an internal service:

```text
IAncillaryCommercialEvaluator
ProvisionCriteriaMatcher
EvaluationContext
FlightContext/FareContext for matching
EvaluateAncillariesService
Backoffice Simulate
```

Tests must not require them.
