# AeroTech.Ancillary - Seat Backoffice Authoring Contract v1.0

**Status:** Proposed owner-directed Backoffice/API increment; NOT an existing deployed contract.
**Repository:** https://github.com/aliifarhadi/AeroTech.Ancillary
**Source branch / inspected commit:** `k8s-stg` / `d8dbeef303e990449a89a2c643588a62fd5c7647` (`Remove UnUsed Things!`, 2026-10-13).
**Scope:** Ancillary Backend only, specifically Seat A07 / A08 / A09 Backoffice authoring. No other service or surface changes.
**Authority:** (1) Owner's present three-level Backoffice requirement; (2) `docs/AeroTech-Ancillary-v12.2/00-START-HERE-AND-AUTHORITY.md` and v12.2 chapters 02, 03, 04, 05, 06, 07, 08, 17; (3) latest real `k8s-stg` code, with changed-HEAD recheck required; (4) benchmarks for background only.

## 1. Purpose and non-negotiable aggregate boundaries

UI hierarchy: `Seat Definition (A07/A08/A09)` -> `AncillaryProvision(s) scoped to exact Definition version` -> `AncillaryPricing revision(s) scoped to exact Provision`.

- `AncillaryServiceDefinition`, `AncillaryProvision` and `AncillaryPricing` remain separate, existing aggregate roots. Seat-specific HTTP DTOs are **not** separate domain aggregates.
- `AncillaryInventoryPolicy` remains an independent aggregate keyed by `OwnerAirlineId + stable ServiceDefinitionRef` (not by the Pricing id and not as a child of a Provision). Actual seat map, occupancy and hold/assignment belong to FlightFlow; no new seat-stock table or fake remaining-seats count.
- No generic JSON/EAV/dynamic rule definition, no copy of the three underlying aggregates, no new inventory engine, no Order/EMD/reservation implementation, no changes to shopping surfaces.
- Active Definition/Provision/Pricing is not directly editable. Preserve version/revise and publication semantics. Reuse `IUnitOfWork`, repositories, projections, MediatR, validators and existing Backoffice authorization/presentation conventions.

## 2. Verified CURRENT routes (prefix: `/Backoffice/v1`)

| Level | Existing operation | Current API |
|---|---|---|
| Seat Definition | paginated GET | `/AncillaryServiceDefinitions/Paginated?Profile=Seat` (server uses `AncillaryProfile` enum; verify actual HTTP query enum conversion) |
| Seat Definition | create | `POST /AncillaryServiceDefinitions/Seat` |
| Seat Definition | edit draft | `PUT /AncillaryServiceDefinitions/Seat/{serviceDefinitionId}` |
| Seat Definition | typed form read | `GET /AncillaryServiceDefinitions/Seat/{serviceDefinitionId}` |
| Seat Definition | variants | `GET /AncillaryServiceDefinitions/Variants?Profile=Seat` |
| Provision | paginated children | `GET /AncillaryProvisions/Paginated?ServiceDefinitionId={serviceDefinitionId}&PageNumber=1&PageSize=10` |
| Provision | generic create | `POST /AncillaryProvisions` |
| Provision | generic edit draft | `PUT /AncillaryProvisions/{provisionId}` |
| Provision | generic form/details | `GET /AncillaryProvisions/{provisionId}/Form`, `GET /AncillaryProvisions/{provisionId}` |
| Provision | lifecycle | `POST /AncillaryProvisions/{id}/Activate`; `POST /AncillaryProvisions/{id}/Publish` with `PricingId`; `POST /AncillaryProvisions/{id}/SwitchActivePricing`; Suspend/Reactivate/Retire |
| Pricing | paginated children | `GET /AncillaryPricings/Paginated?AncillaryProvisionId={provisionId}&PageNumber=1&PageSize=10` |
| Pricing | create/edit/read | `POST /AncillaryPricings`, `PUT /AncillaryPricings/{pricingId}`, `GET /AncillaryPricings/{pricingId}` |
| Pricing | lifecycle | Activate/Suspend/Reactivate/Retire/Revise under `/AncillaryPricings/{id}/...` |
| Inventory | by stable identity | `GET /AncillaryInventoryPolicies/ByServiceIdentity?serviceDefinitionRef={ref}` |
| Inventory | availability/configuration | `GET /AncillaryInventoryConfigurationSnapshots?ServiceDefinitionRef={ref}[&FlightId=...]` |

Note: **do not fabricate nested Pricing endpoints**. Existing paginated, create, edit and read Pricing contracts already meet the requested third level. No new Pricing controller is authorized in this increment.

## 3. Four proposed NEW Seat Provision routes (exact published URL)

All carry `[Authorize(SurfaceAuthorization.Backoffice)]`, the current `Backoffice/v{version:apiVersion}` convention, real MediatR handlers, current ApiResult/error conventions, and must appear in the real published OpenAPI.

```http
GET  /Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId:long}/ProvisionForm
POST /Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId:long}/Provisions
GET  /Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId:long}/Provisions/{provisionId:long}/Form
PUT  /Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId:long}/Provisions/{provisionId:long}
```

No new `GET Paginated` is necessary: retain the existing Provision filtered list. No new `GET Pricing/{id}` is necessary: retain the existing generic GET. Do not rename/delete existing generic routes, or the typed `Seat/{id}` Definition routes.

### 3.1 GET create form contract (new)

Returns a compact **closed typed Seat editor descriptor** resolved from the persisted exact Definition (NOT user-supplied profile or guessed variant). Recommended wire DTO name: `BackofficeSeatProvisionCreateFormDto`. Its fields are:

- `serviceDefinitionId: long`, `variantCode: string` (exactly A07/A08/A09), `profile: EnumValueDto` (Seat), `definitionStatus: EnumValueDto`, `pricingUnit: EnumValueDto`, `serviceDateBasis: EnumValueDto`.
- `applicationType: EnumValueDto` from the authoritative `AncillaryVariant.ApplicationType` (Seat); **read-only**, never client-settable.
- `allowedRuleSections: string[]`: canonical casing matching the Seat form's actual allowed nested members. Include shared sections, including `TravelDate`/`DayTimeApplication`; never include `BaggageApplication`, `PetRule`, `AssistedTravelRule`, `AirportServiceRule`.
- `constraints`: a small **hard-coded typed** descriptor resolved from existing `AncillaryVariant`, `SeatSpecification` and current Domain guards: `quantityUnit` compatible with `PricingUnit.PerSeat` (existing map => `Each`), `maxQuantity: 1` for A07/A08, `maxQuantity: null` for A09 (no invented upper bound), `requiresSeatNumberOrCharacteristic: true`, `seatNumberFilterRequiresAircraft: true`, `publishedPointOfSaleCount: 1`, `requiresPreferredCharacteristic: true` for A08 at *Definition spec* level. Do not assert that every A09 has the same actual purchase quantity as `ExtraOccupiedSeatCount`; these are distinct domain values.
- `editable: bool` derived from current Domain lifecycle (Draft for subsequent Provision edit; Definition status is a separate fact); `form`: **omit** for create rather than creating fake POS, aircraft, supplier, rate, fare, document or monetary defaults. Client builds only user-selected values using this descriptor.

This is a proposed DTO shape, to be implemented and proven in OpenAPI; do not describe it as existing code. The descriptor is UI advice plus a documented reflection of real server invariants, NOT a new rule engine. If exact DTO names must follow a pre-existing project pattern, reuse that pattern and report the final name/wire format before frontend integration.

### 3.2 POST/PUT Seat Provision input contract (new)

Use one typed `SeatProvisionInput` (or equivalent project-convention name), composed of **existing** typed `Provision*Input` records, without adding extraneous persisted fields:

| Wire property | Source C# type | Seat authoring rule |
|---|---|---|
| `sequence` | `int` | positive; preserve sequence/version conflicts/selection semantics |
| `coverageScope` | `ServiceCoverageScope` | source enum, **Portion**, not invented Bound |
| `purchaseStage` | `PurchaseStage` | source enum; valid selling stages only |
| `priceOrigin` | `PriceOrigin` | `Filed`, `ExternalQuote`, `Free`, `NotAvailable` as source allows |
| `quoteProviderKey` | `string?` | only legitimate source, no fabricated supplier key |
| `quantity` | `ProvisionQuantityInput` | `Each` from PerSeat; A07/A08 max 1; A09 source guards |
| `outcome` | `ProvisionOutcomeInput` | paid/free/not available, document/booking flags must agree |
| `settlement` | `ProvisionSettlementInput` | existing settlement policy |
| `availability` | `ProvisionAvailabilityInput` | mustCheckAvailability; not equal guaranteed stock |
| `fulfillment` | `ProvisionFulfillmentInput` | valid `FulfillmentProviderKey` from source, never hardcode |
| `passengerEligibility` | `ProvisionPassengerEligibilityInput?` | types and age bands, if restricted |
| `salesRestrictions` | `ProvisionSalesRestrictionsInput?` | **exactly one real POS for activation/publication**, customer filters allowed |
| `geography` | `ProvisionGeographyInput?` | route/airport/country scope if configured |
| `flightApplication` | `ProvisionFlightApplicationInput?` | real flight, carrier, aircraft refs; aircraft mandatory with explicit seat numbers |
| `fareApplication` | `ProvisionFareApplicationInput?` | Fare, FareFamily, Cabin, RBD, FareBasis filters if configured |
| `travelDate` | `ProvisionTravelDateInput?` | permitted periods and optional **blackout periods**; do NOT drop stored blackouts |
| `dayTimeApplication` | `ProvisionDayTimeApplicationInput?` | optional existing time windows |
| `advancePurchase` | `ProvisionAdvancePurchaseInput?` | optional existing cutoff rules |
| `seatApplication` | `ProvisionSeatApplicationInput` | nonempty SeatNumbers OR SeatCharacteristicCodes, never physical seat inventory |

**ABSENT from Seat request:** `serviceDefinitionId` (trusted route id), `applicationType` (server sets `ProvisionApplicationType.Seat`), `baggageApplication`, `petRule`, `assistedTravelRule`, `airportServiceRule`, `pricing`, `inventoryPolicy`, `remainingSeats`, `seatOccupancy` and any dynamic/unknown member. Fail on unknown members for this Seat endpoint; do not silently ignore a foreign rule. A generic Provision command receives `ServiceDefinitionId` from the route and `ApplicationType.Seat` from fixed server mapping; disallowed domain rules are mapped as `null`.

Critical: The Seat-specific route is an **ACL/authoring facade** over the existing `DefineAncillaryProvisionService` / `ChangeAncillaryProvisionService`, existing commands and owned rule graph. It must not implement duplicate domain validation or persistence logic. Retain identical existing transaction, projection, error and result semantics.

### 3.3 GET Seat edit form contract (new)

`GET .../Seat/{definitionId}/Provisions/{provisionId}/Form` must:

1. Load both IDs from authoritative data; reject nonexistent ids, non-Seat Definition, A01/A10/other profile, and `provision.ServiceDefinitionId != definitionId` (no existence or details leakage across links).
2. Return the existing read model's header (`Id`, `ServiceDefinitionId`, `Status`, `Profile`, `VariantCode`, created/activated/suspended/retired timestamps) plus a `form` matching **exactly the POST/PUT Seat payload properties above** (no `ApplicationType`, no other family rules).
3. Round-trip every supported rule, especially permitted/blackout periods, day/time, POS/customer scope, cabin and seat characteristics. On subsequent PUT, preserve everything represented in GET unless the user explicitly changes it. Hidden/collapsed Advanced sections must remain in form state.
4. When domain has an invalid legacy/non-Seat rule payload, do NOT silently discard it in conversion: return a clear error/diagnostic rather than a destructive PUT form.
5. Expose the same closed typed metadata/constraints as GET create (directly or by an unambiguous link/query) without changing generic `GET /AncillaryProvisions/{id}/Form`.

### 3.4 PUT behavior

PUT accepts exactly the Seat input payload (same `form` as read) and changes only Draft Provision using existing application service. Reject wrong pair, another family, Active/Suspended/Retired mutation or malformed/unknown fields. No implicit automatic revise/clone. No destructive nulling of pre-existing TravelDate/Blackout or other shared rules.

## 4. Authoritative A07/A08/A09 distinctions

| Variant | Seat Definition typed specification (existing) | Provision-specific effect |
|---|---|---|
| A07 StandardSeat | `SeatCharacteristicCodes` optional; `ApplicableCabinIds`; `RequiresExitRowEligibility` | `Quantity.MaxQuantity = 1` on publication; `SeatApplication` must specify a characteristic or number |
| A08 PreferredSeat | `SeatCharacteristicCodes` **nonempty** in Definition; cabin and exit-row eligibility | `Quantity.MaxQuantity = 1`; eligible passengers/fare/seat characteristics; never infer real exit-row availability |
| A09 ExtraSeat | `RequiresAdjacentSeat`, required `ExtraSeatPurpose`, `ExtraOccupiedSeatCount >= 1`, `RequiresExternalTicketAction` per actual issuer policy | explicit relevant Seat filters, quantity per real source; EXST/CBBG may involve external Ticket/Exchange, **not invariably EMD** |

These fields live on the **Definition**'s typed Seat specification. They are NOT all new Provision fields. `SeatApplication` is limited to `SeatNumbers[]`, `SeatCharacteristicCodes[]`; it does not hold `SeatPurpose`, physical seat map or adjacency availability. Domain guard: explicit `SeatNumbers` requires `FlightApplication.AllowedAircraftIds` to be populated, and `SeatApplication` requires at least one number or characteristic when creating/updating a Seat Provision. No guarantee of available seat is inferred from any of these rules.

## 5. Blackout and advanced rule decision

**Never remove `TravelDate.BlackoutPeriods` from Domain, public Provision API, mapper, DB or Seat contract.** The v12.2 Pack explicitly allows blackout periods for commercial/seasonal restriction. Seat-map blocked/occupied seats are FlightFlow runtime facts, NOT `BlackoutPeriods`.

Frontend presentation: hide optional `travelDate.blackoutPeriods` and `dayTimeApplication` under **Advanced selling restrictions** by default. If a user enables them, show/edit them. If they were already populated, preserve/show an indicator, do not silently drop during PUT. This is a UI disclosure decision, not a domain removal.

## 6. Pricing, publication and stock boundary

Existing Pricing input:

```text
DefinePricingRequest { AncillaryProvisionId:long, Rates:IReadOnlyList<PricingRateInput> }
ChangePricingRequest { Rates:IReadOnlyList<PricingRateInput> }
PricingRateInput { PassengerTypeCode?, AgeFromInclusive?, AgeToExclusive?, BasePrice:MoneyInput, Components[] }
MoneyInput { Amount:decimal, CurrencyId:int }
PriceComponentInput { Category, Code, Name?, CountryId?, StationAirportId?, Amount:MoneyInput,
                      FeeApplicationUnit?, TaxIncludedInSource?, TaxTreatment? }
```

- Frontend must list all revisions/versions, distinguish Draft/Active/Retired; `PUT` only Draft; `POST /AncillaryPricings/{id}/Revise` for versions, `SwitchActivePricing` where applicable. An active price is not directly editable.
- `Paid + Filed`: create legitimate Pricing rates and use existing `POST /AncillaryProvisions/{id}/Publish` with `PricingId` if that is the selected flow. This existing endpoint validates matching Provision, price, active Definition and performs activation as designed.
- `Free + Free` and `NotAvailable + NotAvailable`: no fake 0.00 priced row. Use appropriate existing Provision Activate flow after verifying source `EnsureAllowedBy`, never force `Publish(PricingId)`.
- `Paid + ExternalQuote`: do not fabricate a Rate, amount, FX, provider quote, or assume actual future quote integration exists; verify real source activation requirements.
- `PricingUnit.PerSeat` is the Definition unit; `Quantity.Unit = Each` by the actual Domain mapping; no user-defined SKU rate multiplier.
- `InventoryPolicy` is **separate**: `GET /AncillaryInventoryPolicies/ByServiceIdentity?serviceDefinitionRef=...` and snapshot API. Seat stock delegated to FlightFlow when configured/verified. No auto-created local Seat inventory; no transaction across 3 aggregates in a single new UI POST.

## 7. Validation, authentication and compatibility

- Existing `SurfaceAuthorization.Backoffice`, active customer/tenant/airline identity, and existing authorization/owner filters apply unchanged. Never trust `serviceDefinitionId`, POS, supplier or OwnerAirlineId in body to escape owner scope. If existing code lacks a needed guard for these new routes, add it there only, documenting the authoritative source.
- Validation: profile/variant and exact parent-child match; Draft-only edits; unknown fields rejected; Variant-specific max Qty; `SeatApplication` characteristic or number required; aircraft required with SeatNumbers; publish one POS; actual real provider/catalog refs, no synthetic defaults. All **effective** server checks stay in Domain/Application, not only UI.
- Preserve current API response envelope and error shapes; do not invent error codes. Generate OpenAPI from actual typed requests, verify schema contains no forbidden Seat members and does not break existing generic routes.
- Preserve existing release/rollback and schema migrations; **no DB migration is expected**, as no aggregate shape changes. If agent discovers migration is truly required, stop and report before expanding scope.

## 8. Acceptance tests and evidence required

Minimum tests (new integration/contract tests plus existing suites):

1. A07/A08/A09 create-form metadata correctly reflects persisted Definition and correct variant rules; unknown/non-Seat ids refused.
2. A07/A08/A09 POST typed Seat Provision, GET new Seat form, exact PUT round-trip, existing generic GET equivalence (including nested child rows), persistence reload.
3. Seat POST/PUT reject Baggage/Pet/AssistedTravel/AirportService unknown sections rather than silently ignoring them.
4. Reject a provision id paired with another Seat definition id; reject cross-profile, cross-tenant/caller; no mutation on failure.
5. Reject A07/A08 quantity max >1 at publication; reject SeatApplication empty and specific SeatNumbers without aircraft; do not misapply A07/A08 bound to A09.
6. Advanced optional blackout/permitted/day-time rules round-trip non-destructively, including PUT where advanced inputs are untouched.
7. Pricing pagination correctly filters `AncillaryProvisionId`; POST/GET/PUT Draft remains valid on A07/A08/A09, revision history and active price protections.
8. `Paid+Filed` happy path: Active Definition, Seat Provision Draft, valid Pricing Draft, Publish with correct PricingId -> verify status and read; wrong PricingId or invalid status rejected.
9. `Free`, `NotAvailable`, `ExternalQuote` source-supported activation scenarios without fake price; if blocked by real provider reference, report `BLOCKED_EXTERNAL_REFERENCE` instead of faking PASS.
10. Existing Seat Definition typed A07/A08/A09 create/read/edit tests continue passing; generic Provision/Pricing endpoints and Agency/OtaPanel Shopping routes unchanged.
11. Real published OpenAPI conforms; authenticated real HTTP smoke (once service and reference data available) for create-form, create, read, PUT, child pagination, price read, and 401/403/mismatch controls. Unit/integration tests are **not** substitutes for live HTTP.

Agent handoff must include the exact HEAD, changed paths, negative deletion audit, build/test output (pass/fail/count), OpenAPI path/schema inventory, HTTP evidence and gaps. No unverified 'all green' report.

## 9. Source of truth index

- `docs/AeroTech-Ancillary-v12.2/{00,02,03,04,05,06,07,08,17}*.md`
- `src/AeroTech.Ancillary.Domain/AncillaryServiceDefinitionAggregate/{AncillaryServiceDefinition.cs,AncillaryVariant.cs,Specifications/SeatSpecification.cs}`
- `src/AeroTech.Ancillary.Domain/AncillaryProvisionAggregate/{AncillaryProvision.cs,AncillaryProvision.Publication.cs,AncillaryProvision.Rules.cs,Entities/ProvisionSeatApplicationRule.cs}`
- `src/AeroTech.Ancillary.RestApi/V1/AncillaryServiceDefinitionAggregate/{Controllers/BackofficeController.cs,Requests/SeatSpecificationRequest.cs}`
- `src/AeroTech.Ancillary.RestApi/V1/AncillaryProvisionAggregate/{Controllers/BackofficeController.cs,Requests/DefineProvisionRequest.cs,Requests/ChangeProvisionRequest.cs}`
- `src/AeroTech.Ancillary.RestApi/V1/AncillaryPricingAggregate/Controllers/BackofficeController.cs`
- `src/AeroTech.Ancillary.Query/AncillaryProvisionAggregate/{Dto/BackofficeProvisionFormDto.cs,Queries/GetAncillaryProvisionById/AncillaryProvisionMapper.cs,Queries/GetAncillaryProvisionForm/AncillaryProvisionFormMapper.cs}`
- `src/AeroTech.Ancillary.Query/AncillaryServiceDefinitionAggregate/Dto/SeatVariantSpecificationDtos.cs`
- `src/AeroTech.Ancillary.Application/AncillaryProvisionAggregate/Commands/{DefineAncillaryProvision,ChangeAncillaryProvision,PublishAncillaryProvision,ActivateAncillaryProvision}`
- `src/AeroTech.Ancillary.Application/AncillaryPricingAggregate/Commands/DefineAncillaryPricing/PricingInputs.cs`
- `tests/AeroTech.Ancillary.Application.AcceptanceTests/Backoffice/P32CSeatContractAcceptanceTests.cs`
