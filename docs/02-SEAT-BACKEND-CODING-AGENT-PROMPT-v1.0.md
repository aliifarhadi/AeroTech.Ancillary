# Coding Agent Prompt - Implement Seat-Specific Backoffice Provision Authoring

**Read first:** `01-SEAT-BACKOFFICE-BACKEND-CONTRACT-v1.0.md` attached to this prompt. It is the exact approved scope, route inventory, field matrix, risks, and acceptance contract. This prompt alone is not a substitute for it.

## Role / guardrails

You are the **coding agent**, not the domain architect. Implement code only for the approved Seat A07/A08/A09 authoring increment inside `AeroTech.Ancillary`. The Owner approves business decisions. Do not invent domain abstractions, change unrelated stages, or implement another family. Existing P1/P2/Shopping must remain intact. Do not perform work in frontend, Ordering, AirPrice, FlightFlow or AirAvail repositories.

Source repository: `https://github.com/aliifarhadi/AeroTech.Ancillary`; baseline `k8s-stg@d8dbeef303e990449a89a2c643588a62fd5c7647`. **FIRST fetch live `k8s-stg` HEAD and compare to baseline**; if advanced, report the diff and merge only compatible changes, never silently assume the old HEAD, rebase, or overwrite. Use project coding conventions; no duplicated aggregate/service/repository. No unilateral push or merge; provide the patch plus evidence for owner approval.

Authority order: latest Owner instruction and the companion Seat contract -> `docs/AeroTech-Ancillary-v12.2/00-START-HERE-AND-AUTHORITY.md` + chapters `02/03/04/05/06/07/08/17` -> actual latest code, migrations, tests -> evidence-based benchmark only. Latest Owner narrows scope to **Seat authoring API**, no P3 reopening.

## Required implementation

### A. Verify/read actual implementation before editing

Read:

1. `src/AeroTech.Ancillary.RestApi/V1/AncillaryServiceDefinitionAggregate/Controllers/BackofficeController.cs` and Seat definition request.
2. `src/AeroTech.Ancillary.RestApi/V1/AncillaryProvisionAggregate/Controllers/BackofficeController.cs`, generic `DefineProvisionRequest`, `ChangeProvisionRequest`.
3. `src/AeroTech.Ancillary.Query/AncillaryProvisionAggregate/Dto/BackofficeProvisionFormDto.cs`, FormMapper, ProvisionMapper, read/paginated services.
4. `src/AeroTech.Ancillary.Domain/AncillaryServiceDefinitionAggregate/AncillaryVariant.cs`, `Specifications/SeatSpecification.cs`.
5. `src/AeroTech.Ancillary.Domain/AncillaryProvisionAggregate/{AncillaryProvision.cs,AncillaryProvision.Publication.cs,AncillaryProvision.Rules.cs,Entities/ProvisionSeatApplicationRule.cs}`.
6. `src/AeroTech.Ancillary.Application/AncillaryProvisionAggregate/Commands/{DefineAncillaryProvision,ChangeAncillaryProvision,PublishAncillaryProvision,ActivateAncillaryProvision}` and corresponding validators/DI.
7. `src/AeroTech.Ancillary.RestApi/V1/AncillaryPricingAggregate/Controllers/BackofficeController.cs` and source Pricing input; `src/AeroTech.Ancillary.Domain/AncillaryPricingAggregate/AncillaryPricing.cs`.
8. `src/AeroTech.Ancillary.RestApi/V1/_Shared/SurfaceAuthorization.cs`, actual response envelopes, JSON settings, OpenAPI filters, tenant/airline scope and error conventions.
9. `tests/AeroTech.Ancillary.Application.AcceptanceTests/Backoffice/P32CSeatContractAcceptanceTests.cs` and existing Provision/Pricing acceptance tests.

The last commit `d8dbeef3` intentionally removed 199 controller lines and 155 per-rule Application files. **Do not recreate** per-rule CRUD or removed handlers. All rules are edited through the aggregate's full Draft PUT.

### B. Implement exactly four new routes

```http
GET  /Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId:long}/ProvisionForm
POST /Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId:long}/Provisions
GET  /Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId:long}/Provisions/{provisionId:long}/Form
PUT  /Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId:long}/Provisions/{provisionId:long}
```

Reuse existing Backoffice auth/ApiVersion/tags/mediator and appropriate controller organization. These are ADDITIVE; preserve all current existing Definition, generic Provision, generic Pricing, inventory and Shopping routes. Do not add new Pricing GET/POST/PUT because they already exist. The existing provision child list is `GET /Backoffice/v1/AncillaryProvisions/Paginated?ServiceDefinitionId=...`; pricing child list is `GET /Backoffice/v1/AncillaryPricings/Paginated?AncillaryProvisionId=...`.

### C. Strongly typed Seat provision facade

Build one Seat-specific request and form DTO, with the precise allowed field inventory in companion document section 3.2. Compose existing `Provision*Input` objects; do not create a new domain aggregate, rule engine, or table. `SeatProvisionInput` has only:

`Sequence, CoverageScope, PurchaseStage, PriceOrigin, QuoteProviderKey, Quantity, Outcome, Settlement, Availability, Fulfillment, PassengerEligibility?, SalesRestrictions?, Geography?, FlightApplication?, FareApplication?, TravelDate?, DayTimeApplication?, AdvancePurchase?, SeatApplication`.

Derive `ServiceDefinitionId` exclusively from route and `ApplicationType = ProvisionApplicationType.Seat` server side. Pass all forbidden other-family rule args as `null`. For `GET` editor form include exactly the above fields. Add strict deserialization for unknown members on the Seat-specific payload, including `ApplicationType`, `ServiceDefinitionId`, `BaggageApplication`, `PetRule`, `AssistedTravelRule`, `AirportServiceRule` and invented `Stock/Price` members. Use existing `JsonUnmappedMemberHandling.Disallow` convention and validate real ASP.NET JSON behavior. Ensure existing legacy/general routes remain unchanged.

Get/create metadata must be useful WITHOUT a Provision id; read persisted Seat Definition by exact id and produce the proposed `BackofficeSeatProvisionCreateFormDto` per companion contract: `ServiceDefinitionId`, `VariantCode`, `Profile`, `DefinitionStatus`, `PricingUnit`, `ServiceDateBasis`, read-only `ApplicationType`, closed `AllowedRuleSections` and typed constraints reflecting Domain guards. Do not put fake source data or commercial defaults into create response. Keep descriptors small and fixed, not dynamic user-input validation expressions.

Existing GET form for a provision must load the exact Definition/provision pair, reject mismatches, and map to Seat-only DTO non-destructively. Do not silently erase existing shared rule groups; `TravelDate` (including `BlackoutPeriods`) and `DayTimeApplication` remain supported and round-trip unchanged. Frontend may collapse advanced fields, but backend must preserve them. If irreconcilable legacy other-family rule data is found on an A07/A08/A09 Provision, return a clear diagnostic instead of hiding it and losing it on PUT.

**Key existing checks**: `AncillaryProvision.EnsureApplicationRules` requires nonempty SeatNumbers or SeatCharacteristicCodes, and explicit SeatNumbers require `FlightApplication` with aircraft refs. `AncillaryProvision.Publication.cs` caps A07/A08 `Quantity.MaxQuantity` at 1; A09 must not get an invented max. One POS per new published Provision; published active Definition and consistent pricing; no direct change of an Active aggregate. Source typed Seat specifications differ for A07 vs A08 vs A09. All other relevant checks still run in Domain/Application; don't repeat them inconsistently.

### D. Preserve lifecycle and independent aggregates

- Definition existing `GET/POST/PUT /AncillaryServiceDefinitions/Seat...` remains as-is.
- Pricing existing `GET/POST/PUT /AncillaryPricings...` and its version/activation/revision APIs remain as-is; no nested child aggregate create that writes Provision and Pricing in one request.
- `Paid + Filed`: use existing Pricing + Provision `Publish` semantics. `Free`, `NotAvailable`, `Paid + ExternalQuote`: preserve existing activation path and source restrictions; no fake paid pricing or provider quotes.
- `AncillaryInventoryPolicy` is keyed by stable `ServiceDefinitionRef`; keep its existing endpoints unchanged. Seat physical stock delegated to FlightFlow. This increment is not an inventory implementation.
- Do not alter Domain's `TravelDate.BlackoutPeriods` or its commercial semantics. Expose it as optional advanced Sale Restriction.

### E. Essential security/concurrency checks

- `[Authorize(SurfaceAuthorization.Backoffice)]` on all routes; never loosen established policies.
- Resolve parent Definition and guarantee `Profile=Seat`, `VariantCode` one of A07/A08/A09; exact child `ServiceDefinitionId` match before calling a mutation. Do not accept fake body serviceDefinitionId, owner, supplier, pointOfSale or pricing as trusted without existing checks.
- Preserve existing actor/tenant/airline ownership conventions. If another owner can use nested ids through the new routes, guard in the appropriate existing boundary without broad refactor, and add a negative test.
- Persist only through current UoW/projection and return existing `ApiResult` and typed error contract. Do not invent HTTP codes or error IDs.
- No concurrency bypass or silent automatic retries across mutable aggregate changes.

## Acceptance suite (must be implemented and actually run)

Use the existing test structure, plus new Seat facade tests. Minimum:

1. A07/A08/A09 GET create descriptor; correct profile, variant, restrictions, no fake defaults.
2. A07/A08/A09 POST -> GET Seat edit form -> exact PUT -> GET and persistent read equals (including POS, Fare, seat filters and advanced rules).
3. GET/PUT reject mismatched DefinitionId/ProvisionId and non-Seat profile; 401/403 and cross-tenant checks.
4. Unknown/foreign fields rejected on new Seat input; no unknown-field silent discard.
5. SeatApplication empty invalid; SeatNumbers without aircraft invalid; A07/A08 `MaxQuantity=2` rejected at publish/activate; A09 special fields remain on **Definition**.
6. Advanced `TravelDate.PermittedPeriods/BlackoutPeriods`, DayTime and AdvancePurchase survive the full edit cycle; no lost data.
7. Existing paginated Provision/Pricing nested filters return only selected parent rows; no regression in existing endpoints.
8. Paid+Filed full lifecycle through existing Pricing and Publish; Free/NotAvailable/ExternalQuote without fabricated Rates; invalid active edits denied.
9. OpenAPI generation: 4 routes, typed Seat-specific request schema, no unrelated family/stock fields, no breaking changes to existing Definition/Provision/Pricing public contracts.
10. Run full current Domain/Shopping/Application acceptance tests, relevant SQL integration and available HTTP smoke; separately report tests not run/blocked. Validate real request JSON and envelope, do not infer from C# types alone.

## Delivery and stop conditions

Deliver source patch plus `reports/SEAT-BACKOFFICE-AUTHORING-CONTRACT-REPORT.md` containing: live HEAD, exact changed file list, old/new routes, exact published OpenAPI request/response examples **captured from implementation**, DTO and validator decisions, test commands with actual output/counts, real SQL/HTTP evidence, untouched-route/deletion audit, precise known failures or external blockers, and frontend-ready payload references. No template/examples that pretend to be real responses.

STOP and report, rather than improvise, if: (i) latest HEAD has a material incompatible change, (ii) a necessary change violates v12.2 Authority, (iii) source enum/DTO semantics conflict with this prompt, (iv) cross-aggregate migration or external-service changes become necessary, (v) an immutable-active data model forces a rewrite outside this authorized scope. Do not expand work to nine profiles; Seat only.
