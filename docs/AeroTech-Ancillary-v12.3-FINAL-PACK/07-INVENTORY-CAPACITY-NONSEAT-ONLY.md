# 07 — InventoryPolicy: only for genuine non-Seat inventory

## Decision
**A07/A08/A09 Seat:** NO `AncillaryInventoryPolicy`, no `InventoryAuthority.FlightFlow`, no `ProviderKey` for physical seat ownership, no `FlightSeatMapReference`, no local stock. Missing policy is `NotApplicable` to Ancillary's inventory check and MUST NOT be reported `NotConfigured`, `ClosedForSale`, `Unknown` or `Unlimited`. Zero callbacks to physical seat inventory in Ancillary.

**NonSeat:** retain existing `AncillaryInventoryPolicy` AR with stable identity `(OwnerAirlineId, ServiceDefinitionRef)`, latest/current `ServiceDefinitionId` pointer, Authority `Unlimited|Local|Supplier`, optional `LocalPattern`, `ProviderKey`, Count/Weight/Slot typed consumption, PassengerUsageLimits, state/version and audit. No implicit missing-policy=Unlimited; missing policy -> NotConfigured if a real internal policy is expected. `Unlimited` = no local finite pool, NOT confirmed/guaranteed. Supplier=>delegated external source, NOT guaranteed. `Local`=> real typed configured resource, not fabricated physical availability.

## Typed local stocks retained
- `FlightCountInventory`: `(OwnerAirlineId, FlightId, ResourceId)`, `TotalCapacity:int>=0`, `CountUnit` Person/Piece/Item/AnimalCarrier/Equipment, closed-for-sale, expectedVersion+append-only adjustments and idempotency correlation.
- `FlightWeightInventory`: `(OwnerAirlineId, FlightId, WeightResourceId)`, `CapacityKg:decimal(18,3)>=0`, version/adjustments, not aircraft payload safety.
- `AirportSlotInventory`: `(OwnerAirlineId, AirportId, FacilityId, [StartUtc,EndUtc))`, `CapacityPersons>=0`, DST/timezone correctness, lock against nonidentical overlapping intervals.
- `PassengerUsageLimit`: `(CountingFamilyCode, LimitScope, MaxUnits, ConsumptionUnit, UnitsPerPurchase?)`, commercial cumulative entitlement, NOT physical stock.

## Exact code-impact scope
- Remove `InventoryAuthority.FlightFlow` enum member and all switch branches, delegation evidence, provider stub and `InventoryReferenceProbe.FlightFlowAsync` for new v12.3. No need to preserve old number 4 because owner confirms no historical data; **do not renumber** Unlimited=1, Local=2, Supplier=3.
- `DefineInventoryPolicyService`, `ChangeInventoryPolicyService`, `ActivateInventoryPolicyService`: resolve ServiceDefinition by ID/ref, forbid Profile.Seat from policy actions and mismatched `OwnerAirlineId`. Do not insert seat draft policies. Query `ByServiceIdentity` for Seat must unambiguously reject/not-applicable (rather than encouraging creation).
- `GetInventoryConfigurationSnapshotService`: return a clear nonSale inventory **not-applicable** state for Seat, with `RequiresAvailabilityCheck` defined separately from a policy (no seat stock). No guessed guarantee. A new appended enum `InventoryCapacityReadState.NotApplicable=8` is permissible; validate all DTO/switch/mapper consumers and test actual wire serialization; no number reuse.
- `AncillaryShoppingEngine`: Seat branch must skip `_inventory.ListAsync` for Seat stable refs, skip `AvailabilityAssessor` physical policy and `QuantityAssessor` policy-driven cumulative limits **for Seat only**, but keep per-selection `QuantityRule` and price/eligibility. Use explicit `NotApplicable` availability assessment with `Authority=null`, `CheckedState=NotChecked`, `IsGuaranteed=false` and no physical stock. Readiness must not block Seat commercial offer merely because no Ancillary policy. NonSeat calculations unchanged.
- Reject accidental fake `Unlimited` for Seat, including when no policy. Nothing else can silently map missing nonSeat policy to Unlimited.

## Generic flight ID validation for remaining nonSeat inventories
`IFlightOccurrenceReference` is used by Count/Weight inventory activation to check that a configured flight exists/belongs to airline. Keep a generic **trusted flight reference source interface**; remove the `FlightFlowFlightOccurrenceReference` HTTP adapter, `FlightFlow:BaseUrl` and its DI/client registration. Implement replacement only if a trusted, already available **Ancillary-owning** read/source exists (e.g. project-local read model validated against an authoritative upstream) and can actually verify flight IDs; otherwise register an explicit `SourceUnavailable` fallback, with activation truthfully blocked as a documented external-source gap. This does **not** block Seat Define/Provision/Pricing/Shopping. No implementation in another repository. Do not silently skip verification or assert success based on FlightId>0.

## SQL and data
No production/historical records to migrate: remove obsolete FlightFlow column/mappings/code and regenerate only necessary current-schema migrations/QueryDb snapshots if needed. Preserve/verify nonSeat local inventory unique indexes, version concurrency, adjustment correlation and row-level Owner. No deleting all inventories just to eliminate Seat dependency.
