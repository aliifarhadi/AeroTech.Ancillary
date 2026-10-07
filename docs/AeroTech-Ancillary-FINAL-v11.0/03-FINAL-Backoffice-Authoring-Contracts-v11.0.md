# 03 — FINAL Backoffice Authoring Contracts v11.0

## 1. Goal

At Phase 1 exit, an analyst can create, inspect, search, edit Draft data and manage lifecycle for the complete ancillary catalog without direct database access.

No shopping/evaluation endpoint is part of this document.

## 2. Existing route families to preserve

```text
Backoffice/v1/Suppliers
Backoffice/v1/AncillaryServiceDefinitions
Backoffice/v1/AncillaryProvisions
```

Use repository conventions; avoid parallel `v2` routes merely to introduce new request fields.

## 3. Supplier Backoffice

Existing required surface:

```text
POST  /Suppliers
GET   /Suppliers/Paginated
GET   /Suppliers/{supplierId}
```

Phase 1 MUST add exactly one Supplier lifecycle action: `POST /Suppliers/{supplierId}/Retire`. `Retired` is terminal in Phase 1. Do not add Reactivate, adapter or configuration endpoints.

List/detail must expose current source fields including owner airline, name, typed fulfillment classification/key and status. The presence of fulfillment metadata does not authorize supplier calls in Phase 1.

## 4. ServiceDefinition Backoffice

Required:

```text
POST  /AncillaryServiceDefinitions
GET   /AncillaryServiceDefinitions/Paginated
GET   /AncillaryServiceDefinitions/{id}
PUT/PATCH Draft edit using repository convention
POST  /{id}/Activate
POST  /{id}/Suspend
POST  /{id}/Reactivate
POST  /{id}/Retire
POST  /{id}/Revise          // required: create Version + 1 as Draft with a new numeric Id
```

Do not invent additional workflow states.

### List filtering

Support practical filters without a generic query language:

```text
OwnerAirlineId?
SupplierId?
Status?
ServiceSubCode?
GroupCode?
CommercialName/search text?
Page/PageSize
```

### Detail

Return every authored ServiceDefinition field, including classification, booking/document metadata, sales dates, status and version.

## 5. Provision Backoffice

Required:

```text
POST  /AncillaryProvisions
GET   /AncillaryProvisions/Paginated
GET   /AncillaryProvisions/{id}
PUT/PATCH Draft edit using repository convention
POST  /{id}/Activate
POST  /{id}/Suspend
POST  /{id}/Reactivate
POST  /{id}/Retire
```

Active provision mutation is forbidden. To replace an active rule, author a new Draft row and activate it according to the service's sequence/version policy.

### Provision request blocks

```text
serviceDefinitionId
sequence
salesEffectiveFrom / salesDiscontinueAt
coverageScope
passenger
sales
travel + routePairs
fare
advancePurchase
quantity
application { type, baggage?, seat? }
outcome
fee
priceLines
settlement
availability
legacy fulfillment (preserve baseline contract unless owner changes it later)
```

### List filtering

Practical filters:

```text
ServiceDefinitionId?
Status?
CoverageScope?
SalesEffectiveAt?   // optional simple convenience filter if cheap
Page/PageSize
```

Do not implement runtime passenger/flight/fare matching in the list query.

## 6. No Preview/Simulate/Bulk in v11 Phase 1

Previous packs added Preview/Bulk/Simulate and then used them to justify an evaluator inside Ancillary. v11 explicitly removes that requirement.

Do not create:

```text
/Simulate
/AncillaryEvaluations
/Preview rule engine
/Bulk pricing matrix persistence
```

A future owner decision may add authoring conveniences after the simple base works.

## 7. Reference data

Do not duplicate AirInfo/AirPrice/AeroCore reference masters inside Ancillary.

Backoffice request models store canonical IDs.

If the current repository already has a read-only ReferenceData module, it may be reused as-is. Do not expand Phase 1 into a large gateway/proxy project and do not modify the source reference services.

## 8. Read model requirement

Every typed criterion persisted in command storage must round-trip to the query/read model and Backoffice detail/list where relevant.

No field may exist only in the command aggregate while disappearing from Backoffice reads.

## 9. Validation

Validate structure, not runtime eligibility:

- IDs positive where required;
- enum values defined;
- list values distinct;
- date ranges valid;
- time bounds supplied as a pair where appropriate;
- route pair origin != destination;
- baggage ranges coherent;
- seat selectors structurally valid;
- Paid/Free/NotAvailable price invariants;
- Industry sub-code must exist in the read-only industry reference when `SubCodeSource=Industry`.

Do not validate a Provision by fetching a concrete flight/passenger/fare and running a matcher. That is outside Phase 1.
