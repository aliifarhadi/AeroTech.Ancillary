# S2 targeted correction on `k8s-stg@520e493` — questions and conflicts

Date: 2026-10-20. HEAD `520e493` equals `origin/k8s-stg`. The tree is clean apart from the three new untracked Owner docs. Nothing has been coded in this round; this file comes first, per CLAUDE.md rules 6 and 7.

Inputs:
- `docs/S2-520e493-OWNER-CODE-AUDIT.md` (B1–B8, C, D);
- `docs/AeroTech-Ancillary-S2-520e493-TARGETED-CORRECTION-AGENT-PROMPT.md`;
- Owner v1.1;
- `docs/ancillary_s2_execution_pack/01` §07.3, §11.3 and line 458, and `03` §2, §5 and §7.

## 1. How the current code does it, and what needs no decision

| Item | Current source (520e493) | Planned change (no Owner decision needed) |
|---|---|---|
| B7 seat/source revalidation | `ResolveSelectionsService.cs:90` checks the Order version. `:114` compares only currency and total. `OfferItemRef.OfSeat` (`OfferItemRef.cs:36–56`) hashes neither the source version nor the flight capacity. | Compare `token.SourceVersion` with the live Order source version, and the signed seat facts (`FlightRef`, `FlightId`, `FlightCapacityId`, `TravellerRef`, `SeatNumber`, coverage) with the regenerated seat. Any mismatch → `Stale`. The 43-character id is unchanged. Add a test where the flight version or capacity changes but seat number and price stay the same. |
| B5 detection | `OrderShoppingContextMapper.cs:28` leaves `OrderTravellerId` null when the Order has no matching traveller. `ServiceSelectionResolutionMapper.cs:33` drops it silently with `OfType<long>()`. | Detect a passenger-scoped line whose travellers lack an `OrderTravellerId`. Genuine Order-scope lines (A22/A24) keep an empty list. The status is Q3. |
| B1 source facts | The candidate already holds `VariantCode`, `Profile`, `ServiceTypeCode`, `ServiceSubCode`, `Booking.Method`, `Booking.SsrCode`, `Document.Routing/Type/Rfic/Rfisc`, `Confirmation.ConfirmationRequirement`, `SupplierId` and `Price.PricingRateId`. The provision holds `Fulfillment.FulfillmentProviderKey` (`FulfillmentDefinition.cs`), and the signed token holds the full `AncillarySelection`. | Expose only these sourced facts; no invented provider contract. The shape is Q4. |
| B2 mechanics | A seat line carries only `seatMapSelectionId` (`BackofficeSelectionLineInput.cs:16`). The seat candidate is the precomputed seat-map candidate (`EvaluateBatchService.cs:58`). | Re-evaluate the found seat's candidate through the existing engine, with `SeatNumber` from the seat map plus the client's acknowledgement. Sign the acknowledgement and re-evaluate it at Resolve. Acceptance never proves eligibility, which stays with the engine's traveller facts. The field name is Q5. |
| B3 loading | Option groups are built only in Details (`OfferOptionCatalog.Build(definition, provision, candidate, context)`; `OfferSelectionDetailsMapper.OptionGroups`). List items carry selection fields and quantity on all three surfaces, but no option groups or interaction. | List loads active definitions and provisions once per point of sale (no N+1) and reuses the same catalog builder and the shared `OfferOptionGroupDto`. Additions are additive on the three item DTOs; goldens gain only the new keys. Details stays on demand. The shape is Q7, the budget Q8. |
| Consumers | Neither `AeroTech.Backoffice` (`7a69349`) nor `AeroTech.OtaPanel.V1` (`ae583b6`) calls any `AncillaryOffers`, `SeatMaps` or `EvaluateBatch` route yet. | No frontend shape constrains B3; `03` §2 is the only target. |
| C shared fees | `PriceAssessor` leaves a line with unapplied non-Item fees non-purchasable, and the basket is not ready. | Add the requested Resolve test: a non-fully-priced fee scenario never resolves `Valid`. |
| C variant truth | The matrix proves engine behaviour on a synthetic catalogue only. | The new report labels each of A01–A24 `Supported`, `NeedsQuote`, `NeedsVerification` or `UnsupportedSource`, with evidence. |
| Report | `reports/S2-IMPLEMENTATION-CONFORMANCE-v1.1.md` still says "uncommitted", but the work is commit `520e493`. | A new report for this round; the old one stays as history. |

## 2. Questions and conflicts

### Q1 — B8: tokens when the basket is not ready

Evidence: `EvaluateBatchService.cs:65` signs every purchasable line before `SelectionBasketRules.Basket` (`:68`) computes `allReady`. `03` §5 says "`allReady=false` means no basket Add, irrespective of individual status". The correction prompt says "no committable token for an incomplete Basket".

Options:
- **A.** Issue no token for any line while `allReady=false`. Lines keep their own status and price. One valid line plus one invalid line → no tokens.
- **B.** Keep per-line tokens, but sign `basketReady=false` into them; Resolve rejects such tokens.

Recommendation: **A**. It is the simplest reading of v1.1 and `03` §5, and needs no extra token state.

Answer:

### Q2 — B4: the same candidate twice in one basket (conflict with the Master)

Evidence:
- Master `01` line 458 chooses: "duplicate exact candidate within one basket → 422; the UI must fix Quantity". Two seats for one traveller and flight, or one seat for two travellers, are a conflict. The current code follows this: `SelectionBasketRules.cs:14` rejects every line sharing an `OfferItemRef`.
- The audit (B4) asks to allow distinct typed selections that share a candidate when the provision permits it.
- The only domain caps are `Provision.Quantity` (`QuantityRule.MinQuantity/MaxQuantity`, `QuantityRule.cs:20–24`) and inventory-policy passenger usage limits. No domain type states that "distinct instances" are allowed.

Options:
- **A.** Keep the Master rule. The same `OfferItemRef` twice → every copy rejected (`DuplicateSelection`).
- **B.** Same `OfferItemRef` with an **equal normalized selection** → every copy rejected (`DuplicateSelection`). Same ref with **different** selections → allowed while the summed charged units stay within `Provision.Quantity.MaxQuantity`; above it → every copy rejected with the existing reason `QuantityAboveMaximum`. Seat collisions are unchanged.

Recommendation: **B**. It satisfies the audit and keeps the Master's rejection of true duplicates, and its only cap is an existing domain rule. With `MaxQuantity = 1` it behaves exactly like A.

Answer:

### Q3 — B1/B5: status when a passenger-scoped line has no `OrderTravellerId`

Evidence: Master §11.3 says "`beneficiaries[].orderTravellerId` — Pax-scoped required … 0/null never fake", but names no status. v1.1 has seven statuses.

Options:
- **A.** Resolve `NeedsVerification` with a new reason code string `OrderTravellerNotLinked`. In an Order-bound Batch the line is not purchasable, so no token is issued.
- **B.** `Rejected` with the same reason.
- **C.** `Unavailable` with the same reason.

Recommendation: **A**. The Order itself is inconsistent, and Ordering can fix it without the customer re-choosing. Only a reason string is added, not an error code.

Answer:

### Q4 — B1: the extended Resolve `service`

Evidence: the current `ServiceResolvedServiceDto` fields stay unchanged. B1 asks for the full selection and the commercial and document facts. Master §11.3 lists the target fields.

Proposed additive fields, all sourced and none invented:
- `variantCode`, `profile`, `serviceTypeCode`, `serviceSubCode`;
- `bookingMethod`, `documentRouting`, `documentType`, `confirmationRequirement`;
- `supplierId`, `fulfillmentProviderKey`, `pricingRateId`;
- `flightCapacityIds` (parallel to `flightIds`);
- `validUntilUtc` (the token expiry);
- `selection`.

`sourceSelectionRefDigest` and `changedFields` are not proposed; the current `Stale` with `PriceChanged` already carries the current price.

Shape of `selection` — options:
- **A.** A flat object with exactly the Evaluate input names of `03` §5 (`quantity`, `packageProductRef`, `bagRef`, `measuredWeightKg`, `dimensions`, …, `planCode`, `deviceCount`), filled from the signed canonical selection. A round-trip test asserts it equals what the client sent.
- **B.** Per-variant typed blocks (`baggage`, `pet`, `airportService`, …).

Recommendation: **A** for `selection`, plus the field list above. Option A is exactly the accepted input; the engine already enforces the per-variant subsets.

Answer:

### Q5 — B2: the exit-row acknowledgement on a batch seat line (conflict with `03` §5)

Evidence: `03` §5 says "For Seat, pass only `seatMapSelectionId`". The audit and the correction prompt ask for a minimal optional acknowledgement on the new batch seat input.

Options:
- **A.** An optional flat `exitRowTermsAccepted: bool?` on the seat line, next to `seatMapSelectionId`.
- **B.** `selection: {exitRowTermsAccepted}` on the seat line, with every other selection field rejected for seats.

Recommendation: **A**. It is the smallest change, and a seat line keeps no generic selection object that could carry seat number, traveller or price.

Answer:

### Q6 — B6: `selectedOptionRefs`

Evidence: the `03` §5 envelope shows `"selectedOptionRefs":{}` on ancillary lines; the current inputs lack it. The engine already validates the authored values.

Options:
- **A.** Add an optional `selectedOptionRefs: {fieldName: optionRef}` on ancillary lines. When supplied, each ref must equal the ref issued for that field and value on the current candidate (`OfferItemRef.OptionOf`). On mismatch the line is `Rejected` with the existing `SelectedOptionNotOffered` field issue. It is never required, and never applies to typed or free values.
- **B.** Do not add it in S2.

Recommendation: **A**. It aligns with the frozen envelope at no extra cost.

Answer:

### Q7 — B3: List additions per surface

Evidence: `03` §2 names `selection`, `optionGroups[]` (`fieldName, sourceState, choices[], isComplete, requiresDetails`) and `interaction` (`Inline | DetailsOnDemand | DedicatedSeatMap`, "derived from the actual candidate, not by Variant alone"). Seats are not in the List (`OfferFamilyFilters.Services`), so `DedicatedSeatMap` cannot appear there.

Proposal:
- **Backoffice item and Api item:** add `optionGroups` (the shared group shape plus `isComplete` and `requiresDetails` on each group) and `interaction {mode, requiresDetails, isComplete}`.
- **OtaPanel item:** the same two, plus `selectionFields [{name, type, required}]`, because it has only `requiredInputs` names today.
- **New Contracts enum:** `OfferInteractionMode { Inline = 1, DetailsOnDemand = 2, DedicatedSeatMap = 3 }`.
- **Derivation rule:** `Inline` only when every required non-coverage field can be completed inline:
  - `Integer` and `Boolean` fields;
  - `Code`, `CodeSet` and `Reference` fields with a `Known` option group that fits the budget.

  Otherwise `DetailsOnDemand`: any `Decimal`, `Text`, `DimensionsCm` or `DateTimeWithOffset` field, an `Unknown` source, or truncated choices. `isComplete` = every group is complete. An unknown source never shows an empty catalogue as complete.

Recommendation: accept as proposed.

Answer:

### Q8 — B3: the inline choice budget

Evidence: `03` §2 suggests "12 choices / 4096 bytes, configurable".

Options:
- **A.** A constant of 12 choices per group, no byte budget.
- **B.** A required configuration key `Shopping:InlineOptionChoiceLimit`, fail-fast like `Shopping:SourceContextTtl`.
- **C.** Both a choice limit and a 4096-byte limit.

Recommendation: **A**. No consumer exists yet, and a byte measure of the DTO is brittle. B is a one-line change later if needed.

Answer:
