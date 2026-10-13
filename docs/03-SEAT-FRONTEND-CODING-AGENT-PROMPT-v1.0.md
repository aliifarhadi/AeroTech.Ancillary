# Frontend Coding Agent Prompt - Backoffice Seat Authoring, 3-Level Hierarchy

## 0. Read-before-code / release gate

Implement the **Seat-only** Backoffice authoring experience for `AeroTech.Ancillary`:

**Level 1 Seat Service Definition -> Level 2 Provision -> Level 3 Pricing.**

This is frontend work, but it must match the independently owned Ancillary Backend Domain and its actual HTTP/OpenAPI contracts. First read the companion `01-SEAT-BACKOFFICE-BACKEND-CONTRACT-v1.0.md` and the Backend Agent's verified output (`reports/SEAT-BACKOFFICE-AUTHORING-CONTRACT-REPORT.md` once delivered). Relevant Backend source baseline: `https://github.com/aliifarhadi/AeroTech.Ancillary`, branch `k8s-stg`, inspected SHA `d8dbeef303e990449a89a2c643588a62fd5c7647`. Verify live HEAD, current Swagger/real response, and implemented new Seat routes rather than assuming the backend prompt equals deployed code.

**Frontend repository path, framework, router, state manager, API wrappers, auth middleware, styling, component library, permissions, RTL conventions and existing pages are NOT specified by Owner in this handoff.** Inspect the real frontend workspace and preserve its conventions. Do not invent filenames, URLs, npm dependencies, auth storage, design system, frontend directory structure, environment variables or external service endpoints. If new Seat Provision API is not yet deployed, implement against typed contracted interfaces and controlled mocks/tests only; do not label the result end-to-end complete or silently call the generic Provision mutation as a long-term substitute. Never alter the Backend repository.

## 1. User experience: one page, three expandable levels

```
Ancillary > Seat Management
  Level 1: Seat Definitions - server-paginated list (A07/A08/A09)
    [+ Create Seat] [Search] [Variant] [Status] [Page controls]

    Row: SEAT_STANDARD | v1 | A07 | Draft/Active | Supplier | ...
      [expand] [View/Edit] [Activate/Suspend/Revise/Retire as allowed]
      Level 2: Provisions for EXACT selected ServiceDefinitionId (independent pagination)
        [+ Add Seat Provision]
        Row: Seq 10 | POS (read from detail, if needed) | Scope | Paid/Free | Draft/Active | ...
          [expand] [View/Edit] [Activate/Publish/Switch Pricing... as allowed]
          Level 3: Pricing revisions for EXACT selected ProvisionId (independent pagination)
            [+ Add Pricing if Paid+Filed]
            Row: Version | Currencies | RateCount | Draft/Active | ...
              [View Details] [Edit Draft] [Revise] [Activate...] as allowed
```

Clicking a Seat Definition row expands its child Provision grid and lazily fetches it. Clicking a Provision row expands Pricing revisions and lazily fetches them. The row's edit action must not accidentally expand it; use accessible separate controls. Keep per-parent page, loading, error, empty state and refetch state **independent**. Display explicitly when there is no Provision, no Pricing, or Pricing is not applicable. Do not fetch every child list on initial page load. Preserve selected stable IDs when sorting/filtering; invalidate affected parent/child caches on save/activate/revise.

Separate *catalogue Definition* from *sales rule Provision* from *rate Pricing*. A Provision is a rule for eligibility and POS, not a physical seat. Each row may have multiple revisions. No inline editing of immutable Active rows.

## 2. Source-backed API calls (all carry `/Backoffice/v1` prefix)

### Existing Definition

| Action | Exact route |
|---|---|
| List | `GET /AncillaryServiceDefinitions/Paginated?Profile=Seat&PageNumber=...&PageSize=...` |
| Variants | `GET /AncillaryServiceDefinitions/Variants?Profile=Seat` |
| Create | `POST /AncillaryServiceDefinitions/Seat` |
| Edit Draft | `PUT /AncillaryServiceDefinitions/Seat/{serviceDefinitionId}` |
| Read typed edit form | `GET /AncillaryServiceDefinitions/Seat/{serviceDefinitionId}` |
| Activate/revise/status | Existing `POST /AncillaryServiceDefinitions/{id}/Activate`, `/Revise`, `/Suspend`, `/Reactivate`, `/Retire` |

Use `GetSeat(...).form` as authoritative PUT form; use server `variantCode` and typed `specification`. The existing create request additionally requires `ownerAirlineId`, `serviceDefinitionRef`, etc.; source is `DefineServiceDefinitionRequest<SeatSpecificationRequest>`. Do not include ownerAirlineId in PUT if existing `ChangeServiceDefinitionRequest` does not accept it.

### Provision - existing child list/lifecycle, new Seat editor contract

| Action | Route | Status |
|---|---|---|
| List exact Definition children | `GET /AncillaryProvisions/Paginated?ServiceDefinitionId={definitionId}&PageNumber=...&PageSize=...` | Existing |
| Metadata to create Seat Provision | `GET /AncillaryServiceDefinitions/Seat/{definitionId}/ProvisionForm` | **NEW; wait for Backend proof** |
| Create Seat Provision | `POST /AncillaryServiceDefinitions/Seat/{definitionId}/Provisions` | **NEW** |
| Read Seat-specific edit form | `GET /AncillaryServiceDefinitions/Seat/{definitionId}/Provisions/{provisionId}/Form` | **NEW** |
| Edit Draft Seat Provision | `PUT /AncillaryServiceDefinitions/Seat/{definitionId}/Provisions/{provisionId}` | **NEW** |
| Existing generic read (diagnostics if needed) | `GET /AncillaryProvisions/{provisionId}` / `{id}/Form` | Existing; NOT required as frontend mutation |
| Activate / publish / swap price | `POST /AncillaryProvisions/{id}/Activate`; `/Publish` body `{ pricingId }`; `/SwitchActivePricing` existing body | Existing |
| Suspend / reactivate / retire | `POST /AncillaryProvisions/{id}/Suspend`, `/Reactivate`, `/Retire` | Existing |

The new Seat POST/PUT body MUST have only Seat-specific fields documented in the companion contract. It does NOT include `serviceDefinitionId` or `applicationType` (supplied by trusted server route/mapping). Do not show or submit foreign `baggageApplication`, `petRule`, `assistedTravelRule`, `airportServiceRule` or physical stock fields. GET edit form is projected as `.form` and must round-trip to PUT after metadata/id/status are stripped. Preserve collapsed advanced rule values.

### Pricing - all EXISTING; no custom Seat Pricing endpoint

| Action | Route |
|---|---|
| Paginated by Provision | `GET /AncillaryPricings/Paginated?AncillaryProvisionId={provisionId}&PageNumber=...&PageSize=...` |
| Create | `POST /AncillaryPricings` with `{ ancillaryProvisionId, rates }` |
| Read details | `GET /AncillaryPricings/{pricingId}` |
| Edit Draft | `PUT /AncillaryPricings/{pricingId}` with `{ rates }` |
| Lifecycle | `POST /AncillaryPricings/{id}/Activate`, `/Suspend`, `/Reactivate`, `/Retire`, `/Revise` |

`GET PricingById` returns rich read DTO including readonly/computed `UnitTotal`, `IncludedTaxes`, `IsUnitTotalComplete`, `UnappliedFees` and rate/line IDs. These fields are NOT in `ChangePricingRequest`. On edit, map explicitly to `{ rates: PricingRateInput[] }`: `passengerTypeCode?`, `ageFromInclusive?`, `ageToExclusive?`, `basePrice:{ amount,currencyId }`, `components[]` with `category`, `code`, `name?`, `countryId?`, `stationAirportId?`, `amount:{ amount,currencyId }`, `feeApplicationUnit?`, `taxIncludedInSource?`, `taxTreatment?`. Read and preserve all editable price components; do not send computed readonly totals or row IDs. Rate precision follows verified currency-decimal reference; no JS floating-point re-computation of authoritative totals or invented FX.

### Inventory (separate navigation/status, NOT child of Provision/Pricing)

`GET /AncillaryInventoryPolicies/ByServiceIdentity?serviceDefinitionRef={stableServiceDefinitionRef}`

`GET /AncillaryInventoryConfigurationSnapshots?ServiceDefinitionRef={stableServiceDefinitionRef}[&FlightId=...]`

Show a compact Seat Inventory/Readiness section **associated with the Definition stable ref**, outside nested Pricing. It indicates `FlightFlow` delegated authority or `NotConfigured/DelegatedCheckRequired/Unknown` based on actual API response, never real available seat numbers or guaranteed availability. No local seat stock authoring; do not create a policy automatically when a Seat Definition is created. Existing InventoryPolicy management can be linked to if available in real frontend; do not build a second inventory subsystem.

## 3. Level 1 Seat Definition form (A07/A08/A09)

Use backend typed `POST/PUT /AncillaryServiceDefinitions/Seat` contract and actual `GET Seat/{id}.form` as the source for read/edit round-trip. Common fields already on `DefineServiceDefinitionRequest` / `ChangeServiceDefinitionRequest`:

- Airline context on create; `supplierId`, `serviceDefinitionRef` on create only, `serviceSubCode`, `subCodeSource`, `serviceTypeCode`, `groupCode`, `subGroupCode?`, `description1Code?`, `description2Code?`, `pricingUnit`, `serviceDateBasis`, `variantCode`, `documentRouting`, `commercialName`, `description?`, `document`, `booking`, `salesEffectiveFrom?`, `salesDiscontinueOn?`, and **typed** `specification`.
- `Seat` profile is server-owned by chosen endpoint. Only variants `A07`, `A08`, `A09`. Do not use `profile` as mutable input. Existing seat definitions use `PricingUnit=PerSeat`, FlightDeparture service date basis per source variant; validate against real returned `Variants`, not by invented numeric UI values.
- A07 (`StandardSeatSpecificationDto`): `seatCharacteristicCodes[]` (may be empty), `applicableCabinIds[]`, `requiresExitRowEligibility`.
- A08 (`PreferredSeatSpecificationDto`): same fields; `seatCharacteristicCodes[]` **must be nonempty**.
- A09 (`ExtraSeatSpecificationDto`): `seatCharacteristicCodes[]`, `applicableCabinIds[]`, `requiresExitRowEligibility`, `requiresAdjacentSeat`, `extraSeatPurpose` (`PassengerComfort` or `CabinBaggage`, verify actual serialized enum), `extraOccupiedSeatCount` positive integer, `requiresExternalTicketAction` according to recorded airline booking/document policy. These are **Definition** fields, NOT Provision edits.
- Typed edit GET form includes its variant's own specification only; never show the full legacy all-null SeatSpecification request in UX. Legacy v1 shape may be accepted by backend but should not govern the frontend experience.
- EXST/CBBG can require TicketOrExchange and specific Booking/Document consistency. Do NOT assert every Seat product is EMD, do not issue tickets, and do not fabricate RFIC/RFISC or supplier code. Make `Document`, `Booking`, `DocumentRouting` explicit and use actual allowed input/validation contract.

Draft can be edited; active version is read-only. `Revise` creates a new Draft version; always use returned new id/version, reload its form and child list. The Provision list attaches to **exact Definition version id**, not merely stable ref. InventoryPolicy attaches to stable ref across versions.

## 4. Level 2 - Seat-specific Provision form

On [+ Add Provision], first GET the new `/Seat/{definitionId}/ProvisionForm`. Do not display a fake editable form until the server verifies `Profile=Seat` and variant. Render only these fields, grouped for operator clarity:

| Group | Fields | Behavior |
|---|---|---|
| Core / commercial | `sequence`, `coverageScope`, `purchaseStage`, `priceOrigin`, `quoteProviderKey?`, `quantity`, `outcome` | required source fields; consistent Paid/Free/NotAvailable |
| Sale channel / POS | `salesRestrictions` (one real allowed POS before publication, effective sale dates, customer constraints) | no global/default fake POS; server validates |
| Passenger / Fare | `passengerEligibility?`, `fareApplication?` | optional PTC/age, FareFamily/Cabin/RBD/FareBasis eligibility |
| Flight / route | `flightApplication?`, `geography?` | optional carrier/flight/aircraft, airport/route filters |
| Seat-specific eligibility | `seatApplication` with `seatNumbers[]`, `seatCharacteristicCodes[]` | require at least one nonempty, explicit seat numbers require aircraft filter; no seat-map/occupancy values |
| Booking/settlement | `settlement`, `availability`, `fulfillment` | existing source typed inputs; source-verified provider |
| Advanced selling restrictions | `travelDate?` with **permittedPeriods + blackoutPeriods**, `dayTimeApplication?`, `advancePurchase?` | collapsed initially; visible indicator when populated; preserve on every edit |

Backend sets `ApplicationType=Seat` and trusted `ServiceDefinitionId` from URL. A07/A08 enforce source `MaxQuantity=1` at publication. A09 has its own Definition-level `ExtraOccupiedSeatCount`, but do not invent a Quantity bound. `Quantity.Unit=Each` per `PerSeat` mapping, not `Seat` unit. `ServiceCoverageScope.Portion` is the actual enum, not a fake `Bound`. The form's `SeatApplication` is a commercial condition; selecting/holding a physical specific seat is another system.

All optional rule groups are **semantically real**. A blackout means do not sell in specified travel-date intervals, NOT 'seat occupied in seat map'. Do not delete it from server payload or silently strip it when UI advanced accordion is closed. If PUT is full replacement, GET first and merge correctly before submitting; avoid sending placeholder zeros/false for missing optional fields.

Variant matrix in Provision UI:

- A07 Standard: standard seat condition, eligibility/cabin; normal maximum Qty 1.
- A08 Preferred: characteristic-based preferred seat. Keep Definition `RequiresExitRowEligibility` and PTC/age restrictions visible to operator; never imply physical eligibility without FlightFlow verification; Qty 1.
- A09 Extra Seat: show badge/text for purpose and external ticket action from **read-only Definition spec**; do not put these Definition fields into Provision body. Reflect no guarantee of adjacent seats; no EMD-only assumption.

## 5. Level 3 - Pricing editor & lifecycle

- Clicking a Provision row (separate expander) GETs Pricing filtered by `AncillaryProvisionId`, paginated, ordered by source. Include revision/version/status; show `Active` price distinctly without losing draft revisions.
- `Paid + Filed`: show `[+ Add Pricing]`, create Draft with positive/valid amount and reference-backed currency, PTC/age optional rate filters, tax/fee source codes and TaxTreatment. It must match Definition `PricingUnit=PerSeat`; do not re-price from local speculative tax/FX computations.
- `Free + Free`, `NotAvailable + NotAvailable`: explicitly show 'No authored Pricing required' and disable arbitrary pricing create. Never create 0.00 paid price to simulate free.
- `Paid + ExternalQuote`: show 'Provider quote required; no fixed rate' and actual QuoteProviderKey state; no fake pricing/revenue amount.
- `GET PricingById` is a view object. For edit only transform authorized source writable fields into `ChangePricingRequest { rates }`. Respect Draft-only PUT and existing Revise lifecycle. Verify Component.CurrencyId matches rate according to Domain validator.
- `POST /AncillaryProvisions/{id}/Publish` needs the chosen PricingId for Paid+Filed. It may activate the Draft Pricing and Provision in one pre-existing domain flow; UI must not force separate `Activate Pricing` first when Publish handles it, and should reload both lists after response. For Free/NotAvailable/ExternalQuote, use the verified `Activate` flow only when the source actually permits it. Distinguish provider-reference block from UI errors.
- If multiple Pricing revisions exist, never assume newest is active; rely on server Status. `SwitchActivePricing` accepts `newPricingId` and `expectedOldPricingId` per real request; no silent current value assumptions.

## 6. Operator journey (end-to-end for each variant)

1. Open Seat Management. GET paginated Seat definitions filtered Profile Seat. Show supplier/name/version/variant/status.
2. Create A07/A08/A09 Seat: get `Variants?Profile=Seat`, choose variant and complete typed Definition form; POST Seat; preserve returned DefinitionId; GET typed Seat form for verification; edit Draft through PUT Seat/{id}.
3. Activate Definition with existing lifecycle (real references, supplier and documented booking/document policy must be valid). If backend rejects, render the real error, no fake success.
4. Expand Definition row: GET paginated child Provisions by exact `ServiceDefinitionId`.
5. [+ Add Provision]: GET Seat/{definitionId}/ProvisionForm; show only applicable Seat groups; user configures POS, eligibility, SeatApplication, price mode, settlement/fulfillment; POST Seat/{definitionId}/Provisions; read back the new Seat-specific Form; PUT same roundtrip; reload child list.
6. Expand Provision row: GET paginated Pricing by exact `AncillaryProvisionId`. If Paid+Filed, POST Pricing `{ ancillaryProvisionId, rates }`, GET Pricing/{id}, edit Draft via PUT; after validation, Publish Provision with `{ pricingId }`. If Free/NotAvailable or true ExternalQuote, use source-legal Activate flow without fake price.
7. Navigate to inventory/readiness by `serviceDefinitionRef` for the product's Seat/FlightFlow delegation state. Show source status and why no physical seats are available in authoring screen. This is not seat-map shopping.
8. Verify post-operation status via fresh GETs, preserve version history, show errors from API without suppressing details, and do not automatically move/expand to a misleading level on failure.

## 7. UX and error correctness

- Adopt the **existing frontend design system**: RTL/Persian labels if that is the existing convention, current locale/i18n, accessible keyboard expander icons, consistent modals/drawers and loading skeletons. Do not bring in a new UI framework.
- Strict types from the **actual** published OpenAPI/DTO; large `long` IDs are often emitted as strings in paginated `GridData` while item GET may expose numeric `long`. Preserve IDs as lossless strings in frontend UI/state; do not route them through JS IEEE754 arithmetic, then convert only as required by actual API client serialization.
- Enum shapes can differ by route: list/read `EnumValueDto { value, name }` vs command enum literals. Build explicit mapping derived from actual Swagger/live responses; never blindly spread read DTO into write request or assume numeric enum values.
- DateOnly, TimeOnly and DateTimeOffset must follow real wire formats and timezone semantics; no local-time reinterpretation of `salesEffectiveFrom`/`salesDiscontinueAt` without user choice.
- Monetary arithmetic/display: preserve decimal precision using project's decimal utilities or input strings; don't round backend `UnitTotal` or compute taxes/included fees locally.
- Distinguish unauthorized (401/403), no longer present (404), domain validation (actual backend status/error codes), conflicting version/immutable Active (409 or actual error), unavailable provider/reference, empty list and network errors. Never show silent success.
- Cancellation and stale-request guard: rapid parent expansion switches must not show Provisions for a previous Definition or Prices for a different Provision.
- Preserve keyboard navigation, row actions independent from click-to-expand, all editable fields from edit form, pagination at each level, and selected parent context displayed in drawer header.

## 8. Acceptance tests and deliverables

Implement/integrate into actual existing frontend test framework (don't install an unapproved framework). Minimum:

- All three levels render, filter and paginate independently; expansion fetches correct parent children; empty and loading states; clicking edit does not expand.
- A07, A08, A09 typed Definition create/read/PUT behavior, correct per-variant required/forbidden fields (A08 characteristics, A09 extra seat fields and document routing).
- New Seat Provision create metadata, typed POST, GET and PUT; no foreign-family fields or editable applicationType/serviceDefinitionId, correct parent URL; real domain errors rendered.
- Advanced `travelDate.blackoutPeriods` and permitted periods round-trip unchanged when collapsed; re-opening shows them; no destructive PUT.
- `SeatNumbers` requires aircraft; at least one seat filter; A07/A08 Quantity max 1 at publish, A09 separate semantics; no seat inventory editor.
- Pricing existing nested paginated GET / POST / GET by id / PUT only Draft, version revise and active switching; price modes Free vs Filed vs ExternalQuote; no fake 0 amounts.
- Auth errors, cross-parent id mismatch, stale responses, Refresh after save, no duplicate child rows, no direct editing Active, error-specific feedback.
- Actual integrated browser + authenticated Backend HTTP smoke **after** backend agent publishes confirmed schema; if backend not yet ready, report `BLOCKED_BACKEND_CONTRACT` and deliver UI component tests separately.

Deliver: a minimal readable Seat-only frontend patch, exact route map, screenshots or component-test artifact from actual app if available, test logs, a real captured HTTP contract/serialization list, known blockers and a no-regression checklist. Do not falsely claim full end-to-end success without running against deployed/started current backend.

## 9. Explicit non-goals and stop rules

Do not build nine profile editors, no ancillary shopping/search, no SeatMap availability purchase, no reservation, no ticket or EMD issuing, no payment, no physical capacity editor, no generic schema-driven CMS, no new frontend architecture or auth provider, no changes to another service. If the actual frontend workspace or the current Backend contract differs materially, report exact evidence and adapt only with Owner-approved constraints; do not silently guess request bodies, data dictionary, API base URLs or enumerations.
