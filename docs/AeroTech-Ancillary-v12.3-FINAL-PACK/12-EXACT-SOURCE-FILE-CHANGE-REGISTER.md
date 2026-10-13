# 12 — Source-verified change register (`k8s-stg@8fda5f1e`)

All paths below were verified in the reviewed repo. Line numbers are deliberately omitted because HEAD may advance. Agent: inspect and grep actual current tree before editing. Do not create duplicate entities/services if the named code is already present.

## A. Remove direct Seat/FlightFlow infrastructure: MUST change
| File / source | Observed defect / directive |
|---|---|
| `src/AeroTech.Ancillary.Domain/AncillaryInventoryPolicyAggregate/AncillaryInventoryPolicy.cs` | `Activate` switches on `InventoryAuthority.FlightFlow` and calls `EnsureFlightFlowIsDelegated`. Remove branch/method and disallow Seat policies at application boundary. Other authority logic untouched. |
| `src/AeroTech.Ancillary.Application/AncillaryInventoryPolicyAggregate/Services/InventoryPolicyEvidenceBuilder.cs` | Calls `IFlightFlowDelegationReference`; remove dependency, constructor parameter and evidence member for delegation. Retain Supplier/Local/CountingFamily evidence. |
| `src/AeroTech.Ancillary.Domain/AncillaryInventoryPolicyAggregate/Contracts/InventoryPolicyEvidence.cs` | Remove obsolete FlightFlow-specific property/positional field; fix all constructor callsites/fixtures. |
| `src/AeroTech.Ancillary.Domain/_Shared/Contracts/IFlightFlowDelegationReference.cs` | Delete as dead interface after verifying no remaining consumers. |
| `src/AeroTech.Ancillary.Providers/InventoryReferences/NotConnectedFlightFlowDelegationReference.cs` | Delete obsolete fake provider. |
| `src/AeroTech.Ancillary.Shopping/Engine/InventoryReferenceProbe.cs` | Remove FlightFlow-specific reference constructor field and `FlightFlowAsync`. Keep Resources/Facility/CountingFamily. |
| `src/AeroTech.Ancillary.Shopping/Engine/AvailabilityAssessor.cs` | Remove `case InventoryAuthority.FlightFlow`; for Seat never invoke policy assessment. NonSeat Supplier/Local/Unlimited behavior stays. |
| `src/AeroTech.Ancillary.Providers/DependencyInjection.cs` | Remove `IFlightFlowDelegationReference`, `IFlightSeatMapReference`, explicit FlightFlow typed HttpClient registration, `FlightFlow:BaseUrl`, obsolete using directives. Keep working FlightOffer provider. |
| `src/AeroTech.Ancillary.Providers/FlightFlow/Services/FlightFlowFlightOccurrenceReference.cs`, `Wire/*` | Remove HTTP adapter/wire types if only used by it. Keep generic interface `IFlightOccurrenceReference` for nonSeat source proof; register a truthful `SourceUnavailable` adapter if no available trusted replacement. |
| `src/AeroTech.Ancillary.Domain/_Shared/Contracts/IFlightSeatMapReference.cs` and `src/AeroTech.Ancillary.Providers/SeatMap/NotConnectedFlightSeatMapReference.cs` | Delete broken SeatMap provider abstraction; Ancillary will never fetch/display physical maps. |
| `src/AeroTech.Ancillary.Query/AncillaryOffers/Queries/GetSeatMap/**` | Delete all Query/Service/Handlers for broken SeatMap. Do not delete unrelated `GetAncillaryOffers`, `EvaluateSelection` or price readers. |
| `src/AeroTech.Ancillary.RestApi/V1/AncillaryOffers/Controllers/{Api/OtaPanel/Backoffice}Controller.cs` | Remove `SeatMaps` routes/usings only; add separate `SeatOffers` controllers/queries as chapter 09. |
| `Contracts/AeroTech.Messages/Ancillary/Enums/InventoryAuthority.cs` | Delete FlightFlow=4 from live source-owned enum; preserve other numeric IDs. |

## B. Policy Seat guard / truthfulness: MUST change
| File / source | Action |
|---|---|
| `...Application/AncillaryInventoryPolicyAggregate/Commands/DefineInventoryPolicy/DefineInventoryPolicyService.cs` | Resolve commercial facts `ServiceDefinitionId`; reject Profile Seat (ensure facts reader projects Profile). Maintain owner/ref validations and one-current-policy index. |
| `...Commands/ChangeInventoryPolicy/ChangeInventoryPolicyService.cs` | Reject target Seat on change; protect wrong stable product identity. |
| `...Commands/ActivateInventoryPolicy/ActivateInventoryPolicyService.cs` | Reject Seat definition, even if a policy was somehow inserted; preserve nonSeat external proofs. |
| `...Persistence/AncillaryInventoryPolicyAggregate/InventoryCommercialFactsReader.cs` | Add `AncillaryProfile` to returned commercial facts if using this read model for guards; avoid adding independent EF joins in each caller. |
| `...Domain/AncillaryInventoryPolicyAggregate/Contracts/IInventoryCommercialFactsReader.cs` | Typed profile field in facts DTO, update consuming code/tests. |
| `...Query/AncillaryInventoryPolicyAggregate/Queries/GetInventoryPolicyByServiceIdentity/*` | Do not encourage Seat Policy creation. Return clear NotApplicable response/error after owner lookup if queried for Seat. |
| `...Query/AncillaryInventoryPolicyAggregate/Queries/GetInventoryConfigurationSnapshot/GetInventoryConfigurationSnapshotService.cs` | Detect Seat from current Definition, return Inventory state NotApplicable, `IsGuaranteed=false`, no Seat policy lookup/requires-internal-stock conclusion; do not confuse `MustCheckAvailability` metadata with existence of a policy. |
| `Contracts/AeroTech.Messages/Ancillary/Enums/InventoryCapacityReadState.cs` | Append `NotApplicable=8`, preserving existing numeric values 1-7, if this explicit indicator is required by current generic DTO; update all mappers/tests. |

## C. P2 Shopping split and decoupling: MUST change
| File / source | Action |
|---|---|
| `src/AeroTech.Ancillary.Shopping/Engine/AncillaryShoppingEngine.cs` | Constructor remove Flow delegation port. `ShopAsync` already accepts `ShoppingFilter`. Ensure load skips inventory for Seat. `BuildAsync`: Seat gets noninventory availability assessment; preserve PriceAssessor, per-selection Quantity, rules and IDs. Fix `Readiness()` Seat no-policy logic. |
| `src/AeroTech.Ancillary.Shopping/Engine/ShoppingCatalog.cs` | Continue one canonical catalog; no new Seat pricing repository. |
| `src/AeroTech.Ancillary.Shopping/Engine/QuantityAssessor.cs` | Existing rule uses Policy `PassengerUsageLimits`; for Seat no Policy means no cumulative limits. Keep accepted min/max and other nonSeat checks. |
| `src/AeroTech.Ancillary.Shopping/Engine/ProvisionEvaluation.ProfileRules.cs` | Retain actual Seat logic and `SeatCharacteristicFacts` trust; lack of characteristic data must NOT become a success for specific seat filtering. |
| `src/AeroTech.Ancillary.Query/AncillaryOffers/Queries/GetAncillaryOffers/GetAncillaryOffersService.cs` | Implement explicit Seat/nonSeat scoping before ShopAsync, not merely filter resulting JSON; preserve source AirOffer mapping and per POS resolution. |
| `src/AeroTech.Ancillary.Query/AncillaryOffers/Queries/GetAncillaryOffers/Api/ApiAncillaryOffersMapper.cs` | Do not put Seat into `OtherService` anymore; Seat-only mapper separate and variant-aware. |
| `...Query/AncillaryOffers/Queries/GetAncillaryOffers/{Api/OtaPanel/Backoffice}/...Handler.cs` | Call shared service with `nonSeat` filter, unchanged scope resolver and auth. |
| `...Query/AncillaryOffers/Queries/GetAncillaryOfferDetail/*` and `...Queries/EvaluateSelection/*` | Enforce same endpoint-family boundary: no seat candidate via generic `Details` or `Selections/Evaluate`; reverse Seat routes reject nonSeat refs. Preserve rules and id/ref guards. |
| NEW narrow `SeatOffers` Query/DTO/Controller for each surface | Reuse shared `IGetAncillaryOffersService`, existing `ShoppingFilter`, engine, per-surface mapper conventions, no duplicated pricing/rule code. Use `POST /{Surface}/v1/SeatOffers`, `/Details`, `/Selections/Evaluate`. |
| `src/AeroTech.Ancillary.Query/AncillaryOffers/Snapshots/FlightOfferShoppingContextMapper.cs` | Retain AirOffer-to-commercial context; verify correct SourceIdentity, Traveller/FlightRef. Do not synthesize seat characteristics or import FlightFlow client. |
| `src/AeroTech.Ancillary.Shopping/Context/SeatCharacteristicFacts.cs` | Existing `FlightRef/SeatNumber/CharacteristicCodes/SourceCompleteness/SourceAuthority`. Accept only actually trusted verified source for per-seat-number applicability; do not treat client-supplied string SourceAuthority as proof. |

## D. KEEP these unaffected baselines
- `src/AeroTech.Ancillary.Domain/AncillaryServiceDefinitionAggregate/Specifications/SeatSpecification.cs`: variant checks unchanged.
- `src/AeroTech.Ancillary.RestApi/V1/AncillaryServiceDefinitionAggregate/Controllers/BackofficeController.cs`: already has Seat POST/GET/PUT; keep.
- `src/AeroTech.Ancillary.RestApi/V1/AncillaryProvisionAggregate/Controllers/SeatBackofficeController.cs`: four nested routes already implemented; keep.
- `src/AeroTech.Ancillary.Domain/AncillaryProvisionAggregate/AncillaryProvision.Publication.cs`: do not revert removed mandatory POS.
- `src/AeroTech.Ancillary.Shopping/Reading/Sql/SqlActiveAncillaryProvisionReader.cs`: current null/empty POS semantics preserved.
- `src/AeroTech.Ancillary.Application/AncillaryReservationAggregate/**`, `src/AeroTech.Ancillary.Domain/AncillaryReservationAggregate/**`, `...RestApi/.../AncillaryReservationAggregate/**`: **P3 FROZEN**.
- `src/AeroTech.Ancillary.Domain/FlightCountInventoryAggregate/**`, `FlightWeightInventoryAggregate/**`, `AirportSlotInventoryAggregate/**`: do not remove their nonSeat resources or adjustment logic.

## Source grep / deletion audit required before done
`rg -n 'FlightFlow|IFlightFlowDelegationReference|IFlightSeatMapReference|InventoryAuthority\.FlightFlow|SeatMaps|GetSeatMapService|FlightFlow:BaseUrl' src Contracts tests docs/AeroTech-Ancillary-v12.3*` — classify expected documentation mention versus forbidden runtime imports; **no runtime direct FlightFlow dependencies**. Use `git diff --name-status`, scan dead DI registration, compile all projects, regenerate OpenAPI snapshots, run all Shopping tests. Do not rewrite unrelated solution/framework architecture.
