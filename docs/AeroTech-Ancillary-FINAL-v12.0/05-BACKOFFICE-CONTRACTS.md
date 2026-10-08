# 05 - Phase 1 Backoffice APIs, DTOs and publication transactions

**Only Backoffice API families are authorized.** Reuse repository `Backoffice/v1` route, application service, validators, CQRS, enum DTO, pagination and projection conventions. No second v2 API solely for new schema; adjust current v1 only if no published external consumer depends on the old shape; otherwise use backward-compatible additive fields or report breaking-change migration to Owner.

## Suppliers (existing supporting AR)
`POST /Backoffice/v1/Suppliers`, `GET /Paginated`, `GET /{id}`, `POST /{id}/Retire` in the existing route family. Preserve Supplier attributes and lifecycle from current code. No adapter, price or inventory methods on Supplier.

## AncillaryServiceDefinitions (AR #1)
`POST /Backoffice/v1/AncillaryServiceDefinitions`; `GET /Paginated`; `GET /{id}`; `PUT /{id}` (Draft only); `POST /{id}/Activate`; `Suspend`, `Reactivate`, `Retire`, `Revise`. Keep current route casing and actions. Add one `PricingUnit` enum property to define/edit/detail/list; on revise carry it forward. Server refuses a PricingUnit change once any version of commercial identity has been activated (query by OwnerAirlineId, ServiceDefinitionRef). Filters: OwnerAirlineId, SupplierId, ServiceSubCode, GroupCode, Status, search, Page, PageSize. Existing identity/classification, BookingDefinition and DocumentDefinition fully round-trip.

## AncillaryProvisions (AR #2)
`POST /Backoffice/v1/AncillaryProvisions`; `GET /Paginated`; `GET /{id}`; `PUT /{id}` (Draft-only full coherent authoring); `POST /{id}/Activate`; `Suspend`, `Reactivate`, `Retire`. Maintain current source Contract shape when possible, but move `Fee/PriceLines` to Pricing. New draft Provision can be authored before pricing exists. Filter by ServiceDefinitionId, SupplierId, Status, Sequence, SalesEffectiveAt?; detail returns typed criteria with stable row IDs. Pagination list only summary and counts, not all 1000 dates per row.

Example *illustrative* create (not an unversioned wire authority):
```json
{
  "serviceDefinitionId": 9101,
  "sequence": 10,
  "salesEffectiveFrom": "2026-10-08T00:00:00+00:00",
  "coverageScope": "Sector",
  "outcome": { "disposition":"Paid", "documentRequired":true, "bookingRequired":true },
  "passengerTypes": ["ADT"],
  "seasonalPeriods": [{"startDate":"2026-12-01", "endDate":"2026-12-31"}],
  "blackoutPeriods": [{"startDate":"2026-12-24","endDate":"2026-12-25"}],
  "travelDates": [{"travelDate":"2026-12-02"},{"travelDate":"2026-12-04"}],
  "dayTimeRestrictions": [{"dayOfWeek":"Monday","startTime":"08:00:00","endTime":"12:00:00","effect":"Allow"}],
  "flightIds":[81234], "fareFamilyIds":[5]
}
```
The complete request also carries existing `Quantity`, `Application`, `AdvancePurchase`, `Settlement`, `Availability` and frozen legacy `Fulfillment` as defined in doc 03. Actual enum JSON representation must match current project serialization conventions (the above is conceptual only).

### Row-level authoring for date/time without 1000-element PUT
For Draft provision only, typed row endpoints within the existing Provision controller or per-use-case handlers:
- `POST /{provisionId}/TravelDates` with `TravelDate`; `PUT /{provisionId}/TravelDates/{rowId}`; `DELETE /{provisionId}/TravelDates/{rowId}`.
- Same exact named route shapes for `SeasonalPeriods`, `BlackoutPeriods`, `DayTimeRestrictions` with their typed fields.
- For other selector rows, full Draft PUT is acceptable. No generic `/{dimension}/{id}` metaprogrammed condition controller; no `/Bulk`, `/Preview`, `/Simulate`, `/Evaluation` endpoints.
- Each mutation checks provision Draft, parent identity/tenant, duplicate key, row belongs to same provision, optimistic concurrency where framework supports it; updates command/query model in one unit of work. Return domain conflict instead of leaking DB unique errors.
- Accept an array of 1000 date rows in CREATE / full Draft UPDATE if request size permits, with provider-parameter-safe batching. Row-level endpoints are for subsequent independent edits.

## AncillaryPricings (AR #3) - new Backoffice family
`POST /Backoffice/v1/AncillaryPricings` (create Draft for Provision ID, currency and lines), `GET /Paginated?AncillaryProvisionId=&Status=&Page=&PageSize=`, `GET /{id}`, `PUT /{id}` (Draft-only), `POST /{id}/Activate` (only safe when parent outcome/status rules allow it), `/Suspend`, `/Reactivate`, `/Retire`, `/Revise` (new version + new ID Draft), `POST /AncillaryProvisions/{provisionId}/SwitchActivePricing` (transactional catalog operation; request `{expectedOldPricingId?,newPricingId}`) where appropriate in Provision or Pricing controller. Neither publish nor switching invokes live shopping or providers.
`GET detail`: Id, parent ID, Version, PricingUnit, CurrencyId, FeeApplicationUnit?, Status/timestamps, every line including selector PTC/age, category/code/name/country/station/amount, computed summary amounts **per rate key** for display, never cross-key sum. On define, PricingUnit is server-derived; client cannot override.

Example price version:
```json
{
  "ancillaryProvisionId": 5100,
  "currencyId": 978,
  "feeApplicationUnit": "Item",
  "priceLines": [
    {"passengerTypeCode":"ADT","ageFromInclusive":0,"ageToExclusive":65,"category":"Ancillary","amount":20.00},
    {"passengerTypeCode":"ADT","ageFromInclusive":65,"ageToExclusive":null,"category":"Ancillary","amount":40.00},
    {"passengerTypeCode":"ADT","ageFromInclusive":0,"ageToExclusive":65,"category":"Tax","code":"T1","name":"Filed tax","amount":2.00}
  ]
}
```
Example assumes original PricingUnit = PerPassenger and category/enum serialized as demonstrated; real contracts are generated from project's present serializer and canonical IDs, not manually incompatible JSON.

## Publication workflow and HTTP errors
1. Create/activate ServiceDefinition when Supplier active.
2. Create Draft Provision and rules; create Draft Pricing. For Paid use coordinated catalog publishing: one transaction validates/activates price and provision (without intermediate publicly visible inconsistent Active Provision). For Free/NotAvailable activate provision without price.
3. New price for active Paid Provision: create Draft version, then `SwitchActivePricing` atomic; old version Suspended or Retired, new Active. Never mutate old published rate lines.
4. New eligibility for active Provision: create a new Draft Provision with new Id and appropriate sequence, build prices, atomically replace published provision/sequence where needed. No hidden generic publication saga.
5. Validation: 400 invalid structural fields; 404 missing or wrong-parent ID as appropriate; 409 invalid state, overlapping selectors, uniqueness/race or stale expected active ID. Match project's existing error contract, do not invent an independent exception envelope.

## CQRS and security checks
- Current `IAncillaryProvisionQueryDbSynchronizer` and read models evolve, not duplicated. Add equivalent Pricing projection/synchronizer. All mutable child IDs, unit, dates, modifiers and statuses round-trip. Check pagination and numeric IDs preserve precision.
- AuthZ mirrors existing Backoffice surfaces. Do not trust client-supplied OwnerAirlineId/related ownership; verify ServiceDefinition/Supplier/Provision relationships server side. No customer private or office information leaks into public integration messages.
- Read-only reference lookups use current ReferenceData and canonical `AirPrice` contracts; never copy reference masters into Ancillary.
