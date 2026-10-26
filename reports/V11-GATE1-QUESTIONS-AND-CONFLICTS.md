# Reservation v11 — Gate 1: Sibling evidence and open points

**Base:** `296197a` · **Authority:** `docs/Pack-v11` (Gate 0 answers merged, `VERIFY-PACK.py` PASS, 104 tests).
**Done before this file:** pack merge (01–08, 10–15, SHA256SUMS); dev reservation rows deleted (9 reservations, 18 units, 0 allocations → 0/0/0).
**Code:** none yet. Gate 1 starts when Q1–Q3 are answered.

## 1. How the siblings do it

| Concern | Sibling | Evidence |
|---|---|---|
| Write request → command | RestApi request record copied field by field into one command per surface; shared `I…Command` interface; abstract validator + one validator per nested record | `PriceAncillaryOffersValidator.cs`, `AncillarySelectionLineValidator.cs`, `ServiceController.cs:24-31` |
| Typed options | `SelectionOptionsInput` with six keys (`baggage`, `pet`, `assistedTravel`, `airportService`, `priority`, `connectivity`), strict JSON, exactly one key per line | `SelectionOptionsInput.cs`, `AncillarySelectionLineValidator.cs:16-19` |
| Typed request → unit args | Seat facade: `HoldSeatSelectionsService` → port `ISeatSelectionVerifier` (Application `Services/`) implemented in Query (`SeatSelectionVerifier`) → `AncillaryReservationUnitArgs` → `IHoldAncillaryServicesService.HoldAsync(header, units)` | `HoldSeatSelectionsService.cs:21-58`, `Query/DependencyInjection.cs:80` |
| Catalog reads in Query | Price loads provisions and definitions in one batch each | `IActiveAncillaryProvisionReader.FindActiveProvisionsAsync(ids)`, `IActiveAncillaryDefinitionReader.FindActiveDefinitionsAsync(ids)` |
| Hold refusals | Domain `AncillaryHoldChecks.Accept`: not Active, `SubjectToConfirmation` (so pets A13/A14), quantity out of range, scope mismatch → 16419 / 16402 | `AncillaryHoldChecks.cs:16-46` |
| Price gates | A17, A19 by variant; quotes by `Price.Origin == Filed` and `QuoteProviderKey == null` | `SellableCandidate.cs:9-30` |
| Omitted quantity | Price uses the provision minimum | `QuantityAssessor.cs:26` |
| Write answers | every write returns `AncillaryHoldResult` (13 fields per unit) | `AncillaryHoldResultFactory.cs` |

## 2. What Gate 1 builds (pack 11, no open point)
1. `POST Service-Holds` body: header + eight optional family arrays → `variantCode` → `products[{provisionId}]` → `units[{orderServiceId, unit{travellerId?}, coverage{flightIds}, selected{quantity?, options?}}]`; `options` = the existing `SelectionOptionsInput`. Strict JSON everywhere.
2. Validators (shape only): at least one unit in total; `orderServiceId` unique in the request; in each family array `options` may hold only that family's key (`meals` and `upgrades` take none).
3. Derive `ServiceDefinitionId`, `CoverageScope` and the quantity from the provision; refuse a `variantCode` or family that does not match the definition (16402, 422).
4. Explicit refusals (Owner Q1) in `AncillaryHoldChecks.Accept`, next to the existing `SubjectToConfirmation` refusal: A17, A18, A19 and a provision priced by `ExternalQuote` → 16419. Pets are already refused there.
5. Receipt `{holdId, reference, status, expiresAt, units[{unitId, orderServiceId, status}]}` for Create, Confirm, Release, Cancel and `SeatMaps/Holds`; routes and status codes unchanged.
6. GET projection records of 02 §3 defined; the GET switches to them in Gate 2 with the persisted data.
7. Tests W01–W24 and W27, W25–W26 as serialization shape; report `reports/V11-GATE1-WIRE.md`. Reservation hash pins updated and reported.

## 3. Open points

### Q1 — Where `provisionId` is turned into definition, scope, variant and family
- (a) One Application port now, following `ISeatSelectionVerifier`, implemented in Query with the two batch readers above. Gate 2 adds option validation and fixed-choice capture to the same port (the port of Owner Q2). One catalog read in the port + the existing read in `AncillaryHoldAllocator`.
- (b) Pack 05 §1 step 3 literally: an Application mapper reading `IAncillaryProvisionRepository` and `IAncillaryServiceDefinitionRepository` per unit; Gate 2 adds a second port. Three reads per unit (mapper, port, allocator).

**Recommendation:** (a).

**Answer:**(a).

### Q2 — `selected.quantity` omitted when the provision allows a range
- Pack W12/W13: fixed quantity (min = max) → that value; a range → 422.
- Price: the provision minimum (`QuantityAssessor.cs:26`).
- Resolve always returns `selected.quantity`, so a caller that copies Resolve always sends it.

**Recommendation:** the pack (a hold never invents a quantity).

**Answer:**the pack (a hold never invents a quantity).

### Q3 — Names of the new records
Offers names with `Offer` replaced by `Hold`:

| Wire | Offers record | Proposed record |
|---|---|---|
| family array item | `AncillaryVariantGroupDto<T>` | `AncillaryVariantHoldInput` |
| `products[]` | `AncillaryProductGroupDto<T>` | `AncillaryProductHoldInput` |
| `units[]` | `AncillaryUnitOfferDto` | `AncillaryUnitHoldInput` |
| `unit` | `OfferUnitDto` | `HoldUnitInput` |
| `coverage` | `OfferCoverageDto` | `HoldCoverageInput` |
| `selected` | `SelectedUnitDto` | `SelectedUnitInput` |
| Receipt | — | `AncillaryHoldReceipt`, `AncillaryHoldUnitReceipt` (replace `AncillaryHoldResult`, `AncillaryHoldUnitResult`) |
| GET | `AncillaryHoldDto` (existing) | `AncillaryHoldDto` with `AncillaryVariantHoldDto<T>`, `AncillaryProductHoldDto<T>`, `AncillaryUnitHoldDto`; details `HeldBaggageDetailsDto`, `HeldMealDetailsDto`, `HeldUpgradeDetailsDto`, `HeldAirportServiceDetailsDto` |

**Recommendation:** the proposed names.

**Answer:**the proposed names.
