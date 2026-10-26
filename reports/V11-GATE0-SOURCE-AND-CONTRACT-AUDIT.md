# Reservation v11 — Gate 0: Source and contract audit (report only)

**Date:** 2026-10-26 · **Pack:** `docs/Pack-v11` (`VERIFY-PACK.py`: PASS, 15 Markdown files, 98 unique tests)
**HEAD:** `7a74055` "V10 Code", exactly the pinned SHA. The working tree is clean apart from the pack itself (untracked).
**Code, migration or commit:** none. Open questions are in `reports/V11-GATE0-QUESTIONS-AND-CONFLICTS.md`.

## 1. Summary
- **The v11 diagnosis is correct.**
  - The Hold request is a flat `services[]` that takes internal ids from the caller: `serviceDefinitionId`, `coverageScope`, `coveredFlightIds`, `inventoryFacts`.
  - The write result and the GET echo the same internals, plus `pricingId` and `seatMapSelectionId`.
  - A unit stores no typed selection (SSR, dimensions, pet, appointment).
- **The target contract can be built on the existing Offers, Price and Resolve types.**
  - `SelectionOptionsInput` and the six `*SelectionOptions` records can be taken as they are: same names, already strict.
  - In Offers a "product" is exactly one provision, so `products[].provisionId` means the same thing in the Hold.
- **Five findings change the pack's tables:**
  1. The fixed choices of A02, A06, A10 (fixed), A11, A12 and A24 come uniquely from the Definition in the source, so they are not GATED (D-5).
  2. A Pet definition is always `SubjectToConfirmation` under the current domain rule, so A13/A14 are GATED (D-6).
  3. A10 with `FixedAncillary` is sold by Price today (D-7).
  4. `CoveredFlightIds` is stored as JSON in `nvarchar(max)`, which breaks rule 3 of `CLAUDE.md` (D-9).
  5. The server writes every `long` as a JSON string (D-16).

## 2. Sibling: how Offers, Price and Resolve are built

| Step | Route (one folder per surface) | Input | Output |
|---|---|---|---|
| Offers | `POST {Backoffice\|OtaPanel\|Api}/v1/AncillaryOffers` | query with the offerId | `AncillaryOffersDto`: ten family arrays of `AncillaryVariantGroupDto<TDetails>{variantCode, title, products[]}` |
| Price | `POST {Surface}/v1/AncillaryOffers/Price` | `PriceAncillaryOffersRequest{offerId, selections[AncillarySelectionLine{offerItemId, quantity?, options?: SelectionOptionsInput}]}` | **small result** `SelectionSnapshotResult{selectionId, expiresAtUtc}` |
| Read the priced selection | `GET {Surface}/v1/AncillaryOffers/Price/{selectionId}` | — | priced offers |
| Resolve | `POST Service/v1/AncillaryOffers/Resolve` | `{selectionId}` (strict) | `ServiceResolvedSelectionsDto`: ten family arrays, **flat**, each item a `ServiceResolvedSelectionDto<TDetails>` |

**Layering:**
- Request record in RestApi with `JsonUnmappedMemberHandling.Disallow`.
- A thin command or query plus handler per surface, each in its own folder (`Api/`, `Backoffice/`, `OtaPanel/`, `Service/`).
- One shared service.
- Static mapper and DTOs in Query.

The write is small and the details come from a separate GET. That is the same split the pack asks for in the Hold: a small Receipt and a typed GET.

**Shapes with evidence:**

| Type | Fields | File:line |
|---|---|---|
| `AncillaryOffersDto` | `offerId, currencyId, baggages, meals, pets, assistedTravel, airportServices, priorityServices, connectivity, upgrades, extraSeats, travelInsurance` | `Query/Shopping/AncillaryOffers/Dto/AncillaryOffersDto.cs:3-15` |
| `AncillaryVariantGroupDto<T>` | `variantCode, title, products` | `…/AncillaryVariantGroupDto.cs:3-6` |
| `AncillaryProductGroupDto<T>` | `productKey, serviceDefinitionRef, title, description?, details, units` | `…/AncillaryProductGroupDto.cs:5-11` |
| `AncillaryUnitOfferDto` | `selectionScope, unit, coverage, unitPrice, quantityPolicy, offerItemId, options?, selected?` | `…/AncillaryUnitOfferDto.cs:6-14` |
| `OfferUnitDto` (polymorphic, no discriminator) | `OrderUnitDto{scope}`, `TravellerUnitDto{travellerRef}`, `TravellerPortionUnitDto{travellerRef, portionRef}`, `TravellerFlightUnitDto{travellerRef, flightRef}` | `…/OfferUnitDto.cs:5-9` and the files of the same names |
| `OfferCoverageDto` | `travellerRefs, portionRefs, flightRefs` (all `string[]`) | `…/OfferCoverageDto.cs:3-6` |
| `SelectedUnitDto` | `quantity:int, options?: SelectionOptionsDto<JsonObject>` | `…/SelectedUnitDto.cs:6-8` |
| `SelectionOptionsDto<T>` | `baggage, pet, assistedTravel, airportService, priority, connectivity` (no meal, no upgrade) | `…/SelectionOptionsDto.cs:5-12` |
| `ServiceResolvedSelectionDto<T>` | `offerItemId, variantCode, selectionScope, unit, coverage, sourceBinding, definition, provision, pricing, details, selected` | `…/Service/ServiceResolvedSelectionDto.cs:5-16` |
| `ServiceResolvedProvisionDto` | `provisionId:long, coverageScope, purchaseStage, bookingMethod, …` | `…/Service/ServiceResolvedProvisionDto.cs:6-17` |
| Product grouping in Offers | product = `(DefinitionVersionId, ProvisionId)`; `productKey = "{definitionVersionId}:{provisionId}"` | `…/GetAncillaryOffers/AncillaryOffersMapper.cs:34-41` |

**The reservation today** already follows the layering: a `Service/` folder per operation, one shared service, the GET in Query. The problem is only the **shape of the contract** (section 3).

## 3. Exact CLR tables — today versus the `02` target

### 3.1 Today
| Path | CLR | File:line |
|---|---|---|
| `POST Service-Holds` body | `HoldAncillaryServicesRequest(IdempotencyKey:string, OrderId:long, Reference:string, RequestedExpiresAt:DateTimeOffset, Services:IReadOnlyList<AncillaryHoldServiceInput>)`, strict | `RestApi/V1/AncillaryReservationAggregate/Requests/HoldAncillaryServicesRequest.cs:6-12` |
| `services[]` | `AncillaryHoldServiceInput(OrderServiceId:long, ServiceDefinitionId:long, ProvisionId:long, TravellerId:long?, CoverageScope:ServiceCoverageScope, CoveredFlightIds:IReadOnlyList<long>, Quantity:int, InventoryFacts:InventoryHoldFactsInput?)` | `Application/…/HoldAncillaryServices/AncillaryHoldInputs.cs:6-15` |
| `inventoryFacts` | `InventoryHoldFactsInput(TimeWithOffset:DateTimeOffset?, GuestCount:int?)` | `…/InventoryHoldFactsInput.cs:5-6` |
| Write result | `AncillaryHoldResult(HoldId, IdempotencyKey, OrderId, Reference, Status, ExpiresAt?, Units[AncillaryHoldUnitResult(UnitId, OrderServiceId, ServiceDefinitionId, ProvisionId, TravellerId?, CoverageScope, CoveredFlightIds, Quantity, SeatMapSelectionId?, SeatNumber?, PricingId?, Status, CancellationReasonCode?)])` | `…/AncillaryHoldResult.cs:5-27` |
| GET | `AncillaryHoldDto`, field for field the same shape (`AncillaryHoldUnitDto`) | `Query/AncillaryReservationAggregate/Dto/AncillaryHoldDto.cs:5-27` |
| Cancel | `CancelAncillaryHoldRequest(ReasonCode:string, UnitIds:IReadOnlyList<long>?)`, strict | `…/Requests/CancelAncillaryHoldRequest.cs:5-6` |
| Release | `[FromQuery] long[]? unitIds`; an empty array means "all" | `…/Controllers/ServiceController.cs:41-43` |

### 3.2 The `02` target against the source
| Target path | Source type that can be used | Difference |
|---|---|---|
| `idempotencyKey`, `orderId`, `reference`, `requestedExpiresAt` | the same four fields as today | none |
| `baggages … upgrades` (8 arrays) | names exactly the camelCase properties of `AncillaryOffersDto` | `extraSeats` and `travelInsurance` are left out on purpose. Offers always returns them empty (`AncillaryOffersMapper.cs:28-29`), and with strict JSON sending them gives 400 |
| `{family}[].variantCode` | `string` (`AncillaryVariant.cs:7-30`) | checked against `Definition.VariantCode` |
| `products[].provisionId` | `long`; in Resolve the same value sits at `provision.provisionId` | same value at another depth; product = provision as in Offers |
| `units[].orderServiceId` | `long` | none |
| `units[].unit` | Offers: polymorphic type with string refs | Hold: only `{travellerId:long}`; translation documented by the pack (02 §1.2) |
| `units[].coverage` | Offers: `{travellerRefs, portionRefs, flightRefs}` as strings | Hold: only `{flightIds:long[]}`. The unit's Offers `flightRef` or `portionRef` becomes `flightIds` in coverage |
| `units[].selected.quantity` | `int` (`SelectedUnitDto.cs:7`) | optional in the Hold; the default comes from the provision |
| `units[].selected.options` | `SelectionOptionsInput` (`Application/SelectionSnapshotAggregate/Commands/PriceAncillaryOffers/SelectionOptionsInput.cs:5-12`) | the same Price input type. Resolve's output is these same records serialized to JSON (`PriceAncillaryOffersService.cs:21-25`; `SelectionOptionsMapper.cs:17-18`), so the names match |

### 3.3 Options per family (the Price input, all strict)
| Key | Field and CLR | File:line |
|---|---|---|
| `baggage` | `bagRef:string?`, `measuredWeightKg:decimal?`, `dimensions:SelectedDimensionsInput?`, `weightKg:decimal?` | `BaggageSelectionOptions.cs:5-10` |
| `pet` | `animalType:PetAnimalType?`, `otherAnimalCode:string?`, `animalAgeWeeks:int?`, `combinedWeightKg:decimal?`, `carrierDimensions:SelectedDimensionsInput?`, `documentAcknowledgements:IReadOnlyList<string>?`, `sizeBracket:string?` | `PetSelectionOptions.cs:6-14` |
| `assistedTravel` | `assistanceSsrCode:string?`, `infantRef:string?`, `guardianRef:string?` | `AssistedTravelSelectionOptions.cs:5-9` |
| `airportService` | `timeWithOffset:DateTimeOffset?`, `guestCount:int?` | `AirportServiceSelectionOptions.cs:5-8` |
| `priority` | `optIn:bool?` | `PrioritySelectionOptions.cs:5-6` |
| `connectivity` | `deviceCount:int?` | `ConnectivitySelectionOptions.cs:5-6` |
| `dimensions` | `lengthCm, widthCm, heightCm` (all `decimal`) | `SelectedDimensionsInput.cs:5-6` |

### 3.4 SeatMaps/Holds (no redesign)
- `SeatHoldSelectionInput(OrderServiceId:long, TravellerId:long, SeatMapSelectionId:string, PassengerTypeCode, DateOfBirth:DateOnly?, Flight:SeatHoldFlightInput, Fare:SeatHoldFareInput?, CouponOpenForUse:bool?)` — `HoldSeatSelections/SeatHoldSelectionInput.cs:5-13`.
- `SeatHoldFlightInput(FlightNumber:string, MarketingAirlineId:int, OperatingAirlineId:int, OriginAirportId:int, DestinationAirportId:int, DepartureAt:DateTimeOffset, ArrivalAt:DateTimeOffset, AircraftId:int, CabinClassId:int, RbdId:long?)` — `SeatHoldFlightInput.cs:3-13`.
- `SeatHoldFareInput(AirFareId:long, AirFareType, FareBasis:string?, BookingClass:string?)` — `SeatHoldFareInput.cs:5`.
- The validators apply the same rules as the pack's table (`SeatHoldFlightInputValidator.cs:9-16`, `SeatHoldFareInputValidator.cs:9-12`, `SeatHoldSelectionInputValidator.cs:9-14`).
- The pack's table matches the source. **The one difference:** `HoldSeatSelectionsRequest` and the seat inputs have no `Disallow`, so they are not strict.

## 4. Discrepancies and the smallest change

| # | Pack | Source (evidence) | Smallest change |
|---|---|---|---|
| D-1 | The Hold is "like Resolve" | Resolve is a **flat** list per family (`ServiceResolvedSelectionsDto.cs:3-13`); Offers is nested | Per the pack the Hold takes the Offers nesting. The caller groups the Resolve items by `variantCode` and `provision.provisionId`. Shopping does not change |
| D-2 | `products[].provisionId` | In Resolve the same value is `provision.provisionId`; in Offers product = provision too | Documentation only |
| D-3 | `unit.travellerId` and `coverage.flightIds` | Offers has both as strings scoped to the offer | The translation documented by the pack. The caller maps; Ancillary does not match against Ordering |
| D-4 | `selected.options` as in Resolve | Resolve's output is a `JsonObject`, but it is built from the same typed records | The Hold takes the typed `SelectionOptionsInput`. No `JsonObject` in input or storage |
| D-5 | A02, A06, A12 and A24 may have no source and be GATED | `TypedSelectionForm.WithFixedChoices` builds every fixed choice uniquely from the Definition: A02 and A24 = `ServiceDefinitionRef`, A06 = `Baggage.EquipmentKind`, A10 = `Upgrade.ToCabinId`, A11 = `Meal.MealCode`, A12 = `Meal.MenuItemRef`, A20–A22 = `AirportId` and `FacilityRef` (`Shopping/Options/TypedSelectionForm.cs:55-70`). When the value is missing, `CustomerInputs.FixedChoicesAreDefined` drops the product from Offers (`Shopping/Offers/CustomerInputs.cs:10-18`) | **Not GATED.** Snapshot them at Hold time with **the same rule**. Placement of the shared rule: Q2 |
| D-6 | A13/A14 supported (D03) | A Pet definition must be `SubjectToConfirmation` (`AncillaryServiceDefinition.Profile.cs:130`), and so must the PetRule (`AncillaryProvision.Publication.cs:249`). Price sells only `Immediate` (`SellableCandidate.cs:18`), and the Hold refuses `SubjectToConfirmation` with 16419 (`AncillaryHoldChecks.cs:37`) | **GATED** under the current rules. Test D03 cannot run unless the Owner changes the rule |
| D-7 | A10 fully GATED | A10 with `FixedAncillary` and a filed price passes `SellableCandidate` (`SellableCandidate.cs:16-33`; `CustomerInputs.cs:14`) | **Support the fixed case** (snapshot `toCabinId`). Quotes stay GATED |
| D-8 | A18 with `infantRef` and `guardianRef` | Both are traveller refs **inside the offer** (`TypedSelectionForm.cs:39-45`). The Hold only has the numeric Order traveller id | **GATED**; no new field |
| D-9 | Lists in child tables | `CoveredFlightIds` is a `PrimitiveCollection` stored as JSON in `nvarchar(max)` (`AncillaryReservationUnitConfiguration.cs:21-24`; `20261007141213_InitialAncillary.cs:216`). Value on dev: `[1558420526873968642]` | Table `AncillaryReservationUnitFlights` plus backfill. Method: Q5 |
| D-10 | The unit has a `VariantCode` | No such column (dev column list, section 7) | Additive column. Backfill of old rows: Q5 |
| D-11 | GET grouped by family | Offers and Resolve have no family for seats. SeatMaps Resolve: `seats[]{id, travellerRef, flightId:long, seatNumber, …}` (`SeatMaps/Dto/Service/ServiceResolvedSeatDto.cs:3-10`) | Where seat units go in the GET: Q3 |
| D-12 | GET with historical `products[].details` | The current `*DetailsDto` records are complete records with required fields (for example `MealDetailsDto.cs:6-12`) | Shape of the fixed snapshot in the GET: Q4 |
| D-13 | Strict JSON in every v11 DTO | SeatMaps/Holds is not strict | Left as is, per "do not redesign SeatMap" |
| D-14 | No new error code | 16402, 16403, 16407, 16417, 16418, 16419 and 16621 exist; a family or variant mismatch is covered by 16402 (422) | No new code needed |
| D-15 | Gate 5 live with A01 + A15 + A20 | On dev, A15 is `SubjectToConfirmation` and A20 is an external Supplier. The only variants that can be held today: A01, A02, A03, A05, A11, A16 and A23, plus seats (A07 and A08) | Q6 |
| D-16 | The pack's samples show ids as numbers | The server writes every `long`, including inside arrays, as a **string**, and accepts a number or a string (`PresentationExtensions.cs:68-70`; `LongToStringJsonConverter.cs`). Live sample: `"unitId":"1564289997664681985"`, `"coveredFlightIds":["1558420526873968642"]` | Correct the v11 contract samples to strings. Server behaviour does not change |
| D-17 | Enums shown as numbers | The server writes enums **by name** (`"status":"Held"`) and also accepts numbers (`PresentationExtensions.cs:68`) | Correct the samples to names |

## 5. Variants A01 to A24
**The "dev" column** shows the current dev database: authority, confirmation, number of definitions.

| Variant | Family | Variable selection (source) | Fixed (source) | Inventory fact | Status | Reason / dev | Test |
|---|---|---|---|---|---|---|---|
| A01 | baggages | `quantity` (`CustomerSelectionContract.cs:33-36`) | — | flights (count or weight) | supported | Unlimited, Immediate, 2 | D01, W12 |
| A02 | baggages | `quantity` | `serviceDefinitionRef` (`TypedSelectionForm.cs:58`) | flights | supported (unlike the pack) | Unlimited, Immediate, 3 | rewrite D09 |
| A03 | baggages | `baggage.bagRef`, `measuredWeightKg` | — | flights; an `AcceptedWeightKg` policy is refused | supported for PerPiece. PerKilogram is not sold by Price (`SellableCandidate.cs:33`) | Unlimited, Immediate, 1 | new |
| A04 | baggages | `baggage.dimensions` | — | flights | supported in code | dev: SubjectToConfirmation → 16419 | D02 |
| A05 | baggages | `quantity` | — | flights | supported | Unlimited, Immediate, 1 | W12 |
| A06 | baggages | `baggage.dimensions?`, `weightKg?` | `equipmentKind` (`TypedSelectionForm.cs:59`) | flights | supported (unlike the pack) | dev: SubjectToConfirmation | rewrite D10 |
| A07, A08 | `SeatMaps/Holds` | `seatMapSelectionId` → SeatNumber and PricingId from the verifier | — | no allocation | supported (existing) | 4 and 2 definitions | L12–L15 |
| A09 | (`extraSeats` always empty) | `purpose` | — | — | GATED | the Seat profile is excluded from `SellableCandidate` (`:16`); the SeatMap path covers only A07 and A08 | D13 |
| A10 | upgrades | — | `toCabinId` (`TypedSelectionForm.cs:60`) | flight | fixed supported; quote GATED | dev: external Supplier → 16104 | D14 |
| A11 | meals | — | `mealCode` (`:61`) | flight | supported | Unlimited, Immediate, 8 | D07 |
| A12 | meals | `quantity` | `menuItemRef` (`:62`) | flight | supported (unlike the pack) | dev: external Supplier | rewrite D08 |
| A13, A14 | pets | `pet.*` | — | flights | **GATED** | D-6 | D03 GATED |
| A15, A16 | assistedTravel | `assistedTravel.assistanceSsrCode` (from `AllowedSsrCodes`) | — | flight | supported | A15 dev: SubjectToConfirmation; A16 dev: Immediate, 3 | D04 |
| A17 | assistedTravel | medical fields that are not in Price | — | — | GATED | `SellableCandidate.cs:9-13` | D11 |
| A18 | assistedTravel | `infantRef`, `guardianRef` (refs inside the offer) | — | flight | GATED | D-8; dev: SubjectToConfirmation | — |
| A19 | assistedTravel | contacts that are not in Price | — | — | GATED | `SellableCandidate.cs:9-13` | D12 |
| A20, A21, A22 | airportServices | `airportService.timeWithOffset` (required when `RequiresSpecificAppointment`), `guestCount` | `airportId` and `facilityRef` (`TypedSelectionForm.cs:63-67`) | AirportSlot: appointment and guests; or Unlimited | supported in code | dev: all external Supplier; A22 is also SubjectToConfirmation | D05 |
| A23 | priorityServices | `priority.optIn=true` | — | flight | supported | Unlimited, Immediate, 2 | new |
| A24 | connectivity | `connectivity.deviceCount?` | `serviceDefinitionRef` (`:68`) | flight | supported (unlike the pack) | dev: external Supplier | D06 |

## 6. Enums and serializer
| Enum | Values | File |
|---|---|---|
| `InventoryResourceKind` | FlightCount=1, FlightWeight=2, AirportSlot=3 | `Contracts/AeroTech.Messages/Ancillary/Enums/InventoryResourceKind.cs` |
| `ServiceCoverageScope` | Sector=1, Portion=2, Journey=3, Order=4 | `…/ServiceCoverageScope.cs` |
| `AncillaryReservationStatus` and `…UnitStatus` | Held=1, Confirmed=2, Released=3, Expired=4, Cancelled=5 | `…/AncillaryReservationStatus.cs`, `…/AncillaryReservationUnitStatus.cs` |
| `PetAnimalType` | Cat=1, Dog=2, RegisteredOther=3 | `…/PetAnimalType.cs` |
| `SelectionScope` | PerOrder=1, PerTraveller=2, PerTravellerPerPortion=3, PerTravellerPerFlight=4 | `…/SelectionScope.cs` |
| `ConfirmationRequirement` | Immediate=1, SubjectToConfirmation=2 | — |
| `PassengerTypeCode` | ADT=1, CHD=27 (by position) | `Contracts/AeroTech.Messages/AirPrice/Enums/PassengerTypeCode.cs:7,33` |
| `AirFareType` | Public=1, Private=2, Negotiated=3, Charter=4 | `…/AirPrice/Enums/FareType.cs` |

On the wire: enums are written **by name** and numbers are accepted; `long` is a string; names are camelCase.

## 7. Storage, old data and migration impact
- **Columns on dev:**
  - `AncillaryReservations`: Id, IdempotencyKey(128), OrderId, Reference(128), RequestedExpiresAt?, ExpiresAt?, Status, CreatedAt, UpdatedAt, RowVersion and the audit columns.
  - `AncillaryReservationUnits`: Id, AncillaryReservationId, OrderServiceId, ServiceDefinitionId, ProvisionId, TravellerId?, CoverageScope, **CoveredFlightIds nvarchar(max)**, Quantity, StockPoolId?, ProviderUnitRef?(100), Status, CancellationReasonCode?(50), PricingId?, SeatMapSelectionId?(256), SeatNumber?(16), AppendIdempotencyKey?(128), AppointmentTimeWithOffset?, GuestCount?.
  - `AncillaryReservationUnitAllocations`: Amount `decimal(19,3)`.
- **Indexes:** unique IdempotencyKey, OrderId, AncillaryReservationId, OrderServiceId, unique allocation (unit, kind, source) and (kind, source).
- **Old data on dev:** 9 reservations, 18 units, 0 allocations, all from the v10 live runs.
- **Migration impact (all additive):**
  - a `VariantCode` column;
  - child tables for typed selections, only for supported families that have data;
  - an `AncillaryReservationUnitFlights` table in place of the JSON;
  - `AppointmentTimeWithOffset` and `GuestCount` move into the AirportService selection, without a duplicate truth;
  - no table or row is deleted.

## 8. The six endpoints and outgoing calls
1. `POST Service/v1/Ancillaries/Service-Holds`
2. `GET Service-Holds/{holdId}`
3. `POST Service-Holds/{holdId}/Confirmations`
4. `DELETE Service-Holds/{holdId}?unitIds=`
5. `POST Service-Holds/{holdId}/Cancellations`

These five are in `RestApi/V1/AncillaryReservationAggregate/Controllers/ServiceController.cs:17-47`.

6. `POST Service/v1/Ancillaries/SeatMaps/Holds` (`RestApi/V1/SeatMaps/Controllers/ServiceController.cs:14,25`).

**Outgoing calls:**
- **Ordering: zero.** `IOrderCommercialContextReader` is used only in `Query/Shopping/Common/Orders/OrderShoppingSource.cs:10` and is bound to `NotConnectedOrderCommercialContextReader` (`Providers/DependencyInjection.cs:55`). The reservation folders in Domain, Application, Persistence, Query and RestApi contain no reference to `IOrderCommercialContextReader`, `IOccupiedSeatProvider`, `HttpClient` or FlightFlow (grep in this round).
- **FlightFlow: zero.** The dependencies of `SeatSelectionVerifier` include no FlightFlow type. The Gate 3 graph test and the Gate 4 live log showed the same; those are "as reported".

**Route names:** Shopping names its resource `AncillaryOffers` (`Service/v1/AncillaryOffers/Resolve`), while reservations live under `Ancillaries/…`. The pack keeps the six routes fixed. Recorded only; not asked.

## 9. flightRefs versus flightIds
- Offers: `coverage.flightRefs` and `unit.flightRef` are strings scoped to the offer.
- SeatMaps Resolve: `flightId` is numeric.
- Inventory sources are keyed by `FlightId:long` (`FlightCountInventory.cs:42`, `FlightWeightInventory.cs:40`).
- Mapping is the caller's job; Ancillary does not match anything against Ordering. `productKey` is not an API identity; `provisionId` comes from Resolve.

## 10. Types that change (smallest change)
- **Wire (Gate 1):**
  - `HoldAncillaryServicesRequest` is replaced by eight family arrays: one generic variant, product and unit record with a type parameter; `unit{travellerId}`; `coverage{flightIds}`; `selected{quantity?, options?: SelectionOptionsInput}`.
  - A small mapper flattens the nesting into `AncillaryReservationUnitArgs`.
  - A Receipt replaces `AncillaryHoldResult` for Create, Confirm, Release, Cancel and SeatMaps/Holds.
  - The GET DTO is grouped.
  - Removed: `AncillaryHoldServiceInput` and `InventoryHoldFactsInput`.
- **Domain (Gate 2):**
  - `VariantCode` and a typed selection on the unit (owned, relational).
  - The fixed-choice rule in one shared place (Q2).
  - Option validation against the Definition and Provision (Q2).
- **Persistence:** configurations and one additive migration.
- **Inventory (Gate 3):** a Retire guard in the three existing Retire services.
- **Tests:** four acceptance classes that build holds through the old command (34 calls) and the JSON contract test are rewritten.

## 11. Decision table

**Decided by the pack (not asked):**
- the six routes stay fixed;
- eight arrays with the Offers names;
- `provisionId` on the product;
- `unit.travellerId` and `coverage.flightIds`;
- `selected` equal to Resolve;
- a small Receipt;
- the GET only from stored data;
- strict JSON;
- no Ordering or FlightFlow call;
- Retire with active usage → 16621;
- the v10 ledger and root are kept.

**Open, in the questions file:**

| # | Topic |
|---|---|
| Q1 | Approval of the A01–A24 support table (D-5 to D-8) |
| Q2 | Placement of the fixed-choice rule and of option validation |
| Q3 | Where seats go in the GET |
| Q4 | Shape of the fixed snapshot in the GET |
| Q5 | Migration of old rows (`CoveredFlightIds` and `VariantCode`) |
| Q6 | Dev data for the Gate 5 live run |
