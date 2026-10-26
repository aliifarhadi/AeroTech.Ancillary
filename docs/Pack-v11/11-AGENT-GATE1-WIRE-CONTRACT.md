# Coding Agent — Gate 1 — exact Service DTO, validators and API projection

**Precondition:** Gate 0 approved and exact source DTO discrepancies resolved. Use `02` as single API authority; `03` for variant constraints; `01` for boundaries. No business-code outside contract/adapter except unavoidable compile changes.

## Tasks
1. Replace public `HoldAncillaryServicesRequest.Services[]` with **eight Family arrays**: `baggages, meals, pets, assistedTravel, airportServices, priorityServices, connectivity, upgrades`; `variantCode→products[]→units[]` nesting. Use strongly typed C# records, no `object`, `JsonElement`/`JsonObject`, dictionary of arbitrary fields or giant all-variant record. Family-specific options types reuse existing `*SelectionOptions` field names.
2. Every Unit wire has `orderServiceId`, `unit` with `travellerId` if pax scoped, `coverage.flightIds` only if actual coverage/stock requires, and `selected` with `quantity?`/`options?` exactly as Resolve. Product has only `provisionId`; variant has only `variantCode`. **Do not add productKey, serviceDefinitionId, coverageScope, inventoryFacts, price, offerId, selectionId, currencyId** to request.
3. Create a single *small* typed mapping path from family/Product/Unit to internal domain input; use Provision-derived identity/Scope and Rules. A variant mismatched with Provision or Family rejects all. `orderServiceId` uniqueness entire request; bound count by existing project convention if available, not invented.
4. Replace `AncillaryHoldResult` for Create/Confirm/Release/Cancel and `SeatMaps/Holds` with the **Receipt only** of 02 §2: HoldId, Reference, Status, ExpiresAt, Units(UnitId, OrderServiceId, Status). No echo of the internal Provision, scope, flights or SeatMap token. The GET DTO is *different*: the typed reservation detail of 02 §3 with the family arrays (including `seats`), product `serviceDefinitionRef`/`details` and the selected history; define the projections now and implement persistence in Gate 2.
5. Keep REST routes unchanged; SeatMap Hold contract stays untouched except shared receipt and Unit projection. `DELETE ?unitIds=..` and Cancel `{reasonCode,unitIds?}` semantics unchanged.
6. Strict JSON unknown disallow for all new public records. Reuse project existing status/error serializer. No new auth semantics or outgoing Order client.
7. The `pet` options key and the GATED variants (Owner Q1) are refused explicitly. Option validation against the Definition/Provision and fixed-choice capture (Owner Q2) belong to Gate 2; Gate 1 maps the shape only.

## Tests / report
- W01–W24 and W27 named tests; W25–W26 as GET DTO serialization shape (SQL round trip in Gate 2); serialization roundtrip with variant/family, unknown `services`/`coverageScope` rejected, minimal expected fields and absence of forbidden fields. SeatMap existing contract serialization unchanged.
- `reports/V11-GATE1-WIRE.md`: exact DTO table, sample HTTP JSON and Response, changed file list, discovered/executed tests, what remains GATED. **Do not claim Hold is production-ready until typed persistence and SQL verified.**
