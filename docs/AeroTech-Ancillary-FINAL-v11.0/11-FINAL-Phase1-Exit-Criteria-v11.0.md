# 11 — FINAL Phase 1 Exit Criteria v11.0

Phase 1 is not closed until every applicable checkbox below is proven by actual source/tests.

## Source control

- [ ] Implementation starts from or is functionally based on `e2a8c9f96ec486fbe429e2e1d9947dd83ca3b9c8`.
- [ ] No other repository changed.
- [ ] No functional `AncillaryReservationAggregate` file changed.
- [ ] No Hold/Get/Confirm operational behavior changed.

## Domain authoring

- [ ] Supplier current marketplace identity remains usable.
- [ ] ServiceDefinition classification/booking/document fields remain intact.
- [ ] ServiceDefinition Draft edit/lifecycle is manageable.
- [ ] Provision supports PassengerCriteria.
- [ ] Provision supports SalesCriteria.
- [ ] Provision supports TravelCriteria + RoutePairs.
- [ ] Provision supports FareCriteria.
- [ ] Provision supports AdvancePurchase authoring.
- [ ] Provision supports Standard/Baggage/Seat application types.
- [ ] Fixed filed price and price-line metadata round-trip.
- [ ] Draft edit/lifecycle is manageable.

## Simplicity/deletion audit

- [ ] No evaluator exists in Ancillary.
- [ ] No `AncillaryEvaluation` runtime namespace/service exists.
- [ ] No Preview/Bulk/Simulate engine exists.
- [ ] No generic JSON/EAV rule store exists.
- [ ] No family-specific aggregate explosion exists.
- [ ] No age/FF/occurrence/PCC future fields exist.
- [ ] No new supplier adapter or provider protocol exists.
- [ ] No new StockPool/quota implementation exists.

## Backoffice

- [ ] Supplier list/detail/pagination works.
- [ ] ServiceDefinition list/detail/pagination works with practical filters.
- [ ] ServiceDefinition Draft edit and lifecycle work.
- [ ] Provision list/detail/pagination works with practical filters.
- [ ] Provision detail returns all typed criteria.
- [ ] Provision Draft edit and lifecycle work.
- [ ] Industry sub-code validation uses read-only authoritative reference.
- [ ] CarrierDefined path works for services without authoritative industry code.

## Pricing scenarios

- [ ] ADT and CHD can have different prices via separate Provisions.
- [ ] INF can be Free or NotAvailable via a Provision.
- [ ] Different travel dates can have different authored prices.
- [ ] Different flights can have different authored prices.
- [ ] Different routes can have different authored prices.
- [ ] Different fare families/cabins/RBDs can have different authored prices.
- [ ] Different POS/customer/customer types can have different authored prices.
- [ ] Multiple Suppliers can define same broad service with different prices.

## Family proof

- [ ] FAM01 Extra/prepaid baggage
- [ ] FAM02 Sports/special baggage
- [ ] FAM03 Wheelchair/special assistance
- [ ] FAM04 Meal
- [ ] FAM05 Travel insurance
- [ ] FAM06 Paid seat selection commercial rule
- [ ] FAM07 Airport lounge
- [ ] FAM08 Priority boarding
- [ ] FAM09 Fast track
- [ ] FAM10 Wi-Fi
- [ ] FAM11 Pet service
- [ ] FAM12 Meet & Assist / CIP
- [ ] FAM13 UMNR/special handling style service

For each family: create Draft, read detail, list, update Draft and verify edited fields, activate, then read the Active state.

No Hold/Confirm is required for family proof.

## Engineering proof

- [ ] Solution builds cleanly except documented pre-existing warnings.
- [ ] Full test suite passes.
- [ ] Command and query migrations are consistent.
- [ ] No pending EF model changes.
- [ ] Scenario IDs map to concrete tests.
- [ ] Agent report includes exact commit SHA and file diff summary.

Only after every required criterion passes may the agent emit:

`ANCILLARY_V11_PHASE1_AUTHORING_COMPLETE_READY_FOR_OWNER_AUDIT`

The agent must then STOP. The owner decides the next phase.
