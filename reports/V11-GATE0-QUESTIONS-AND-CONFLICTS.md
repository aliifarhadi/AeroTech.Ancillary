# Reservation v11 — Gate 0: Questions and conflicts

> **Status: history.** All six answers are merged into `docs/Pack-v11` (summary in 01 §I; file-by-file in 01 §I "Merged into"). The pack is the only executable authority; this file is not.

**Base:** `7a74055` · **Full evidence:** `reports/V11-GATE0-SOURCE-AND-CONTRACT-AUDIT.md`.
Only points that change the contract shape or the domain are listed. No code is written until they are answered.

## Q1 — Support table for A01 to A24
**Evidence:**
- The fixed choices of A02, A06, A10 (fixed), A11, A12 and A24 come uniquely from the Definition in Shopping (`TypedSelectionForm.cs:55-70`). When the value is missing, the product is dropped from Offers (`CustomerInputs.cs:10-18`).
- A Pet definition must be `SubjectToConfirmation` (`AncillaryServiceDefinition.Profile.cs:130`), and both Price and the Hold refuse that.
- `infantRef` and `guardianRef` in A18 are traveller refs inside the offer (`TypedSelectionForm.cs:39-45`).

**Recommendation:**
- **Supported:** A01, A02, A03 (PerPiece), A04, A05, A06, A10 (`FixedAncillary` only), A11, A12, A15, A16, A20, A21, A22, A23, A24. Seats (A07 and A08) through SeatMaps.
- **GATED:**
  - A09: no sale path.
  - A10 with a quote.
  - A13 and A14: confirmation is mandatory.
  - A17 and A19: gated in Price.
  - A18: refs tied to the offer.

**Difference from the pack:**
- A02, A06, A12, A24 and A10 fixed are **supported**.
- A13 and A14 are **GATED**. Tests D03, D08, D09 and D10 are rewritten.

**Answer:** **Supported:** A01, A02, A03 (PerPiece), A04, A05, A06, A10 (`FixedAncillary` only), A11, A12, A15, A16, A20, A21, A22, A23, A24. Seats (A07 and A08) through SeatMaps.
- **GATED:**
  - A09: no sale path.
  - A10 with a quote.
  - A13 and A14: confirmation is mandatory.
  - A17 and A19: gated in Price.
  - A18: refs tied to the offer.

## Q2 — Where the fixed-choice rule and option validation live for the Hold
**Evidence:** Both rules are in the Shopping project (`TypedSelectionForm.Fields` and `WithFixedChoices`), but Application references only Domain. The existing sibling for the same problem: the seat facade uses the `ISeatSelectionVerifier` port in Application, implemented in Query (`SeatSelectionVerifier`).

**Options:**
- (a) A port in Application following the `ISeatSelectionVerifier` pattern, implemented in Query by calling the same two Shopping functions. Shopping does not change and there is one truth.
- (b) Move both rules into Domain. This changes the frozen Shopping project.
- (c) Copy the rule into Application. This creates two truths and is rejected.

**Validation depth:** only the static rules of the Definition and Provision:
- required fields and the right family;
- allowed values (SSR code, animal type, size bracket, document code);
- numeric ranges (weight, dimensions, maximum guests, device count).

Commercial eligibility (traveller, route, point of sale) is not checked, because the Hold has no offer context.

**Recommendation:** (a), with this depth.

**Answer:** (a), with this depth.

## Q3 — Where seat units go in the GET
**Evidence:** Offers and Resolve have no family for seats. SeatMaps Resolve uses `seats[]{id, travellerRef, flightId, seatNumber, …}` (`ServiceResolvedSeatDto.cs:3-10`).

**Recommendation:** a `seats` array in the GET with the same layering as the other families:
```json
"seats":[{"variantCode":"A07","products":[{"provisionId":"…","units":[
  {"unitId":"…","orderServiceId":"…","status":"Held","unit":{"travellerId":"…"},"coverage":{"flightIds":["…"]},
   "selected":{"quantity":1},"seatNumber":"12A","seatMapSelectionId":"…"}]}]}]
```
- `pricingId` is not exposed.
- The names come from `SeatMaps/Holds` and SeatMaps Resolve.

**Answer:** a `seats` array in the GET with the same layering as the other families

## Q4 — Fixed data snapshotted at Hold time, shown in the GET
**Evidence:** The current `*DetailsDto` records are complete records with required fields, so a subset of them cannot be returned. A02 and A24 equal `serviceDefinitionRef`, which is already a field of the Offers product.

**Options:**
- (a) `products[]` = `{provisionId, serviceDefinitionRef, details?, units}`. `details` is a small record per family that uses only existing Offers names: `equipmentKind`; `mealCode` and `menuItemRef`; `toCabinId`; `airportId` and `facilityId`.
- (b) Only `serviceDefinitionRef`, no `details`.

**Recommendation:** (a). This is not a full copy of the Definition.

**Answer:** (a). This is not a full copy of the Definition.

## Q5 — Old rows during the migration
**Conflict:** the Owner's standing rule (2026-10-09) says DB data is test data and migrations stay plain. Pack v11 (06-B) says old rows are kept and backfilled with no data loss.

**Data on dev today:** 9 reservations and 18 units.

**Recommendation:**
- In the same migration, move `CoveredFlightIds` into `AncillaryReservationUnitFlights` with `OPENJSON`, then drop the JSON column.
- Fill `VariantCode` with a join to `AncillaryServiceDefinitions`. A definition id never changes variant; this is re-checked in Gate 2.
- An old unit has no typed selection. The GET omits `options` for it, without any new field.

**Answer:** DB data is test data and migrations stay plain. delete reservations in database.

## Q6 — Dev data for the Gate 5 live run (A01 + A15 + A20)
**Evidence:** On dev, A15 is `SubjectToConfirmation` and every A20 belongs to an external Supplier, so none of them can be held. Variants that can be held today: A01, A02, A03, A05, A11, A16 and A23, plus seats.

**Options:**
- (a) Run live with A01, A16, A23 and seats; A20 live stays GATED.
- (b) With your permission, create on dev one A15 with `Immediate` confirmation and one A20 with a local Supplier and an Unlimited policy.

**Recommendation:** (b), only with your approval. Dev data is test data.

**Answer:** (b), only with your approval. Dev data is test data.
