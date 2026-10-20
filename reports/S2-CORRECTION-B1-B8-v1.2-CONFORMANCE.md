# S2 correction B1–B8 (Owner decisions v1.2): execution and conformance report

**Verdict: `INTERNAL_READY`.** The verdict is not `S2_CLOSED`, because external limits remain open (section 13).

| Item | Observed value |
|---|---|
| Repository / branch | AeroTech.Ancillary / `k8s-stg` |
| `HEAD` | `c865cccac86b89f597fcc228f3511f6ec67b2291`, "S2 Correction Questions" |
| Parent | `520e493a53faee724bb60592b691680a4c8d705f`, "S2-01" (the audited code) |
| v1.2 corrections | In the working tree on `c865ccc`. Not committed, because the Owner gave no commit order. |
| Migrations, Domain, Contracts, RestApi, Persistence, Providers, `docs/` | Unchanged (`git status --short` shows 0 entries under each) |
| SeatMapsDto and the three SeatMaps routes | Unchanged |

The decisions source is `AeroTech-Ancillary-S2-B1-B8-OWNER-DECISIONS-AND-AGENT-EXECUTION-v1.2.md`, Q1–Q8.

No entirely new business decision came up. Two findings are left for the Owner (section 14).

---

## 1. Q1 / B8: the basket is committable or no line carries a reference

**Rule implemented**
- `allReady` is computed before any token is issued.
- A `pricedSelectionRef` is issued only when `allReady` is true. Then every line gets one.
- `allReady` is `priced && orderBound`. `priced` means every line can be purchased, every line is in the basket currency, and no shared fee is unapplied.
- An offer-only preview is never ready. It carries `OrderBindingRequired` and never touches the signing key.

**Code**
- `Queries/EvaluateBatch/SelectionBasketRules.cs`
  - `Basket(lines, currencyId, orderBound)` computes `AllReady = priced && orderBound`.
  - Its reason codes are `SharedFeeNotApplied` and `OrderBindingRequired`.
- `Queries/EvaluateBatch/EvaluateBatchService.cs` runs Settle, then Link (Order source only), then Basket. It issues tokens only when `basket.AllReady && binding is not null`.

**Tests (`S2BasketCommitTests`)**
- `B8_one_unusable_line_withholds_the_reference_of_every_line_of_the_basket`
- `B8_an_unapplied_shared_fee_withholds_every_reference_although_each_line_is_accepted`
- `B8_an_order_bound_basket_whose_every_line_is_purchasable_gets_a_reference_on_every_line`
- `B8_an_offer_preview_is_priced_but_never_ready_and_never_needs_the_signing_key`
- `B8_a_ready_order_basket_fails_closed_without_a_signing_key_and_a_basket_that_is_not_ready_never_asks_for_it`

In `S2SelectionBatchTests`, four offer-source assertions changed from `AllReady = true` to `false`, as Q1 requires.

## 2. Q3 / B5: a passenger or seat line needs a unique positive OrderTravellerId

**Rule implemented**
- `Link` checks every beneficiary of a line: the seat traveller, or `Candidate.Travellers`.
- If a beneficiary is not linked to a unique positive `OrderTravellerId`, the line becomes Incomplete with `OrderTravellerNotLinked` and `canPurchase=false`. B8 then withholds every reference.
- Resolve maps the code to `NeedsVerification`.
- A true Order-scoped line (A22/A24 with no beneficiary) keeps its token.

**Code**
- `SelectionBasketRules.Link`.
- `Common/Orders/OrderShoppingContextMapper.cs` links a traveller only when the link is unambiguous: id > 0, one id per TravellerRef, and the id is not reused.
- `ResolveSelections/ResolveSelectionsService.cs`.

**Tests**
- `B5_a_traveller_the_order_does_not_link_makes_its_line_unverified_and_the_basket_issues_no_reference`
- `B5_an_ambiguous_order_traveller_link_is_treated_as_no_link`
- `B5_a_true_order_scoped_service_has_no_beneficiary_and_still_gets_its_reference`

## 3. Q2 / B4: repetition of one candidate

**Rule implemented**

Ancillary lines are grouped by `offerItemRef`. Every line of a repeated group is rejected, so there is no winner. Each line gets one of these reasons:

| Case | Reason code |
|---|---|
| The two selections are equal in canonical form | `DuplicateSelection` |
| The selections are equal except for quantity | `QuantitySplitAcrossLines` (send one line) |
| Otherwise | `RepeatedInstanceNotAllowed` |

No current domain rule authorizes repeated instances of one candidate, so distinct instances fail closed. The canonical form normalizes:
- decimals;
- `DateTimeOffset` values to UTC;
- string lists, sorted ordinally;
- enums as Int64.

Seat collisions (`SeatSelectedTwice`, `TravellerSeatSelectedTwice`) are unchanged.

**Code**
- New `Queries/EvaluateBatch/SelectionCanonicalForm.cs`.
- `SelectionBasketRules.Settle`.

**Tests (`S2SelectionBatchTests`)**
- `PRICE_20_B4_the_same_offer_item_twice_with_the_same_or_a_split_quantity_rejects_both_lines_without_choosing_a_winner` (1+1 and 1+2)
- `B4_two_different_selections_of_one_candidate_are_refused_because_no_rule_authorizes_a_repeated_instance`
- `B4_the_same_product_for_another_traveller_or_another_bound_is_never_a_duplicate_and_one_line_keeps_its_quantity_cap`

## 4. Q4 / B1: the Resolve service hand-off

**Rule implemented**
- Additive fields on the Service Resolve `service`:
  - `variantCode`, `serviceTypeCode`, `serviceSubCode`;
  - `bookingMethod`, `bookingRequired`;
  - `documentRouting`, `documentType`, `documentRequired`;
  - `confirmationRequirement`, `pricingRateId`;
  - the full typed `selection`.
- `selection` mirrors every `AncillarySelection` property, in the same order and with the same types.
- A seat's selection is the signed seat selection: `{seatNumber, exitRowTermsAccepted}`.
- `supplierId` is not added. `fulfillmentProviderKey` is not exposed, although the candidate carries it (`Confirmation.FulfillmentProviderKey`).

**Code**
- New `Dto/Service/ServiceAncillarySelectionDto.cs` and `Dto/Service/ServiceSelectedDimensionsDto.cs`.
- `Dto/Service/ServiceResolvedServiceDto.cs`.
- `ResolveSelections/Service/ServiceSelectionResolutionMapper.cs`.

**Tests (`S2ResolveHandoffTests`)**
- `B1_the_selection_the_customer_sent_reaches_ordering_unchanged_with_the_commercial_and_document_facts` (A01, A06, A22, A24): JSON of the selection sent equals JSON of `service.selection`.
- `B1_a_paid_seat_reaches_ordering_with_the_seat_number_taken_from_the_seat_map`
- `B1_a_weight_package_is_not_offered_because_a_flight_offer_or_order_allowance_is_always_counted_in_pieces` (A02; see sections 10 and 13)
- `B1_an_unverified_pet_is_never_handed_to_ordering`

## 5. Q5 / B2: exit-row terms on a SeatMap line

**Rule implemented**
- Seat lines take an optional `exitRowTermsAccepted`.
- A paid seat is re-evaluated by the engine in the POS context of its seat-map offer, with `{seatNumber, exitRowTermsAccepted}`. The line status, `canPurchase` and the candidate come from that evaluation.
- Accepting the terms is not eligibility. Eligibility comes only from `ShoppingTraveller.VerifiedExitRowEligible`.
- The accepted value is signed inside the token's `selection`. Resolve re-runs the evaluation with the signed value.
- An ancillary line that carries the acknowledgement is refused with 422.
- The SeatMap id format is unchanged.

**Code**
- `SeatMaps/Snapshots/SeatMapSnapshot.cs`: the snapshot keeps the priced POS contexts and the offer's `PointOfSaleId`. This is internal only; SeatMapsDto is unchanged.
- `SeatMaps/Queries/GetSeatMap/GetSeatMapService.cs`.
- `Snapshots/SeatSelectionSnapshot.cs`.
- `EvaluateBatchService.SeatAsync` and `WellFormed`.
- `SelectionLineRequest.cs`.
- The three `*SelectionLineInput` and `*SelectionLineInputMapper` files.
- `PricedSelectionMapper.cs`.
- `ResolveSelectionsService.Request`.
- `Common/OfferPurchase.cs` (the unused seat overload was removed).

**Tests (`S2ExitRowSeatTests`, A08)**
- `B2_a_verified_adult_who_accepts_the_terms_gets_a_signed_exit_row_seat_that_resolves_valid`
- `B2_missing_or_refused_exit_row_terms_never_produce_a_reference`
- `B2_accepting_the_terms_is_not_eligibility_an_unverified_adult_and_an_ineligible_child_never_see_an_exit_row_seat_as_free`
- `B2_an_exit_row_seat_occupied_after_its_reference_was_signed_resolves_stale`
- `B2_an_ancillary_line_cannot_carry_the_seat_acknowledgement`

## 6. Q6 / B6: selected option references

**Rule implemented**
- Ancillary lines take an optional `selectedOptionRefs:{fieldName: optionRef}`. Each pair must meet all of these conditions:
  - it names a `Known` option group with that exact field name (ordinal comparison);
  - the selected value is not null and is one of that group's choices;
  - the ref equals `OfferItemRef.OptionOf(offerItemRef, fieldName, value)`.
- Any mismatch rejects the line with `SelectedOptionNotOffered`.
- References are never required for typed or free values.
- A seat line that carries references is refused with 422.

**Code**
- `EvaluateBatchService.AncillaryAsync` and `OptionRefsMatch`.

**Tests (`S2OptionRefTests`)**
- `B6_an_issued_option_reference_for_an_authored_plan_or_equipment_kind_is_accepted` (A24, A06)
- `B6_an_option_reference_that_was_not_issued_for_this_candidate_field_and_value_rejects_the_line`: wrong ref, another traveller's ref (cross-candidate), key-case mismatch, value not selected
- `B6_typed_measurements_need_no_option_reference`
- `B6_a_dynamic_quote_without_a_trusted_option_source_refuses_a_guessed_reference_and_never_sells_a_guessed_value` (A10 `quoteRef`, source Unknown)
- `B6_a_seat_line_cannot_carry_option_references`

## 7. B7: Resolve re-validates source and signed facts

**Order of checks** in `ResolveSelectionsService.Resolve`:
1. Order version, else `OrderVersionChanged`.
2. Current trusted source version (`OrderBinding.Of(context.SourceIdentity)`) against the signed `SourceVersion`, else Stale `SourceVersionChanged`.
3. Rejected line.
4. `!CanPurchase` mapping.
5. Fresh signed facts, recomputed with `PricedSelectionMapper.Of`, compared with the token, else Stale `SignedFactsChanged`. The comparison covers:
   - kind, offer item, SeatMap id and seat;
   - coverage scope, travellers, flights and portions;
   - definition, version, provision and pricing revision;
   - units;
   - the canonical selection.
6. Price, else Stale `PriceChanged`.

**Tests**
- `B7_a_flight_change_that_keeps_the_order_version_the_seat_numbers_and_the_prices_still_makes_every_reference_stale` (`S2SelectionResolveTests`, flight-version and capacity rows)
- B2's occupied-after-signing test

## 8. Q7 + Q8 / B3: inline options and interaction on the three List surfaces

**Rule implemented**
- Definitions are read once per List query. Provisions are read once per point of sale, and only when that POS has candidates.
- The engine is not run again, and the List never calls Details.
- `OfferOptionCatalog.Build` is the single option source.
- Every visible candidate gets these fields, inside its existing payload:

| Surface | Added fields |
|---|---|
| Backoffice | `optionGroups`, `interaction` (`selection` already existed) |
| OtaPanel | `selectionContract`, `optionGroups`, `interaction` |
| Api | `optionGroups`, `interaction` (the existing `selectionRequirements` is the selection contract and is kept) |

- `MaxInlineOptionsPerGroup = 12` is a constant in `OfferInlineOptions`. There is no setting and no database.
- A `Known` group with 12 or fewer choices is inline. With more than 12: `choices=[]`, `totalChoices=N`, `inlineChoicesIncluded=false` and `sourceState` stays `Known`.
- An `Unknown` group is never shown as known-empty. It has `sourceState=Unknown`, `choices=[]`, `inlineChoicesIncluded=false`, and the reason `OptionSourceUnknown`.
- `interaction.mode` is `Inline` or `DetailsOnDemand`. These are presentation strings, not a Domain enum. The reason codes are:
  - `QuoteRequired`, `DocumentsRequired`, `AppointmentRequired`, `TypedInputRequired`;
  - `OptionSourceUnknown`, `NoOptionAvailable`, `OptionsDeferred`.
- The mode follows the required non-coverage fields and their options. It is not a variant whitelist.
- No `SeatMap` mode was added. A07/A08 in the generic list show `DetailsOnDemand` with `TypedInputRequired`, and the dedicated SeatMaps endpoint stays.

**The Api has its own projection** (`ApiOptionGroupDto`, `ApiOptionChoiceDto`, `ApiOptionApplicabilityDto`, `ApiOptionPricePreviewDto`):
- `travelerIds` and `segmentIds`;
- a price with `amount` as a string and a `currencyCode`;
- no `sourceVersion` (it holds internal definition and provision ids) and no `currencyId`.

The agency leak pin `P32B_SRF_02` stays green.

**Code**
- New:
  - `Queries/GetAncillaryOffers/OfferInlineOptions.cs`;
  - `Dto/Selection/OfferInlineOptionGroupDto.cs`, `Dto/Selection/OfferInteractionDto.cs`;
  - `Dto/Api/ApiOption*.cs` (4 files).
- Changed:
  - `GetAncillaryOffersService.cs` (adds the definition and provision readers);
  - `Snapshots/AncillaryShoppingSnapshot.cs` (`AncillaryOfferItem.OptionGroups`);
  - the three List mappers, the three EvaluateSelection result mappers and the Api and OtaPanel Detail mappers;
  - `OfferSelectionDetailsMapper.cs` (`Choices` made public).

**Tests (`S2InlineOptionsTests`)**
- `B3_a_quantity_bag_a_wifi_plan_and_a_disability_assistance_ssr_are_inline_with_their_authored_choices_on_all_three_surfaces` (A01, A24, A16)
- `B3_Q8_twelve_choices_stay_inline_thirteen_are_deferred_and_an_unknown_source_is_never_known_empty` (0, 1, 12, 13, Unknown, each on all three surfaces)
- `B3_a_quote_upgrade_names_the_quote_and_its_unknown_source_and_a_seat_in_the_generic_list_is_never_inline`
- `B3_twenty_list_items_read_the_catalog_once_need_no_details_call_and_one_click_reads_details_once`: at least 20 list items (asserted), one definition read, one provision read, zero Details reads. One Details click gives one read of each.

**Goldens.** The 9 goldens (3 surfaces × offers/detail/selection) were regenerated. A JSON walk compared each with its `HEAD` version and found **0 changed or removed values**. Only these keys were added:
- Api: `ancillary.optionGroups`, `ancillary.interaction`
- Backoffice: `offers[]` and `blockedOffers[]` `.optionGroups` and `.interaction`; detail and selection `offer.optionGroups` and `offer.interaction`
- OtaPanel: `services[]` `.selectionContract`, `.optionGroups` and `.interaction`; detail and selection `service.*` with the same three keys

**List contract, before and after.** Before, the payloads had none of these keys. After, A24 shows these additions, taken from the regenerated goldens.

Api (`included.other-services.<ref>.ancillary`):
```json
"optionGroups": [{
  "fieldName": "planCode", "sourceState": "Known",
  "choices": [{
    "optionRef": "4cac6zboJFV2bYvuqODovtCvfspKwOUaxUOCR5edLqI",
    "value": "WIFI_FULL_FLIGHT", "label": "Wi-Fi full flight (FullFlight)", "availability": "NotChecked",
    "applicability": { "travelerIds": ["1"], "segmentIds": ["1"], "portionRefs": ["B1"] },
    "pricePreview": { "amount": "981000.00", "currencyCode": "IRR", "isComplete": false },
    "validUntilUtc": "2026-10-01T10:30:00+00:00" }],
  "totalChoices": 1, "inlineChoicesIncluded": true }],
"interaction": { "mode": "Inline", "requiresDetails": false, "reasonCodes": [] }
```

Backoffice (`offers[]`) and OtaPanel (`services[]`) share the option DTO. Each choice carries:
- `applicability {travellerRefs, flightRefs, portionRefs}`;
- `pricePreview {currencyId, amount, isComplete}`;
- `sourceVersion "D{definitionVersionId}-P{provisionId}"`;
- `validUntilUtc`.

OtaPanel also gets the following, with A24 as the example:
```json
"selectionContract": { "kind": "TypedForm", "fields": [
  { "name": "planCode", "type": "Code", "required": true },
  { "name": "deviceCount", "type": "Integer", "required": false } ] }
```

A10 on Backoffice:
```json
"optionGroups": [
  { "fieldName": "targetCabinId", "sourceState": "Known", "choices": [ { "value": "2", "...": "..." } ], "totalChoices": 1, "inlineChoicesIncluded": true },
  { "fieldName": "quoteRef", "sourceState": "Unknown", "choices": [], "totalChoices": 0, "inlineChoicesIncluded": false } ],
"interaction": { "mode": "DetailsOnDemand", "requiresDetails": true, "reasonCodes": ["QuoteRequired", "OptionSourceUnknown"] }
```

## 9. EvaluateBatch and Resolve contracts

**EvaluateBatch line input**
- The fields are the same on all three surfaces, and both additions are optional.
- Before: `clientSelectionId, kind, offerItemRef, selection, seatMapSelectionId`.
- After, two optional fields are added: `selectedOptionRefs` and `exitRowTermsAccepted`.
```json
{ "offerId": null, "orderId": "<orderId>", "selections": [
  { "clientSelectionId": "seat-1", "kind": "SeatMap", "seatMapSelectionId": "<id from SeatMaps>", "exitRowTermsAccepted": true },
  { "clientSelectionId": "wifi-1", "kind": "Ancillary", "offerItemRef": "<ref>",
    "selection": { "planCode": "WIFI_FULL_FLIGHT", "deviceCount": 1 },
    "selectedOptionRefs": { "planCode": "<optionRef from List or Details>" } } ] }
```
- A seat line with `offerItemRef`, `selection` or `selectedOptionRefs` is refused with 422. So is an ancillary line with `exitRowTermsAccepted`.

**EvaluateBatch response**
- The shape is unchanged.
- A line's `pricedSelectionRef` is null unless `basket.allReady` is true.
- `basket.reasonCodes` can now contain `OrderBindingRequired` and `SharedFeeNotApplied`.
- New line reason codes:
  - `OrderTravellerNotLinked`, `QuantitySplitAcrossLines`, `RepeatedInstanceNotAllowed`;
  - `SelectedOptionNotOffered` (already defined).

**Resolve (Service surface) `service`**
- The existing 18 fields are kept unchanged.
- Added fields, shown for A24 with placeholders where the value depends on the authored data:
```json
"variantCode": "A24", "serviceTypeCode": "F", "serviceSubCode": "WF1",
"bookingMethod": "<BookingMethod>", "bookingRequired": "<bool>",
"documentRouting": "<DocumentRouting>", "documentType": "<DocumentType>", "documentRequired": "<bool>",
"confirmationRequirement": "<ConfirmationRequirement>", "pricingRateId": "<id or null>",
"selection": { "quantity": null, "...": "all 37 AncillarySelection fields; a field not sent is null, except the two guardian-contact flags, which are false", "planCode": "WIFI_FULL_FLIGHT", "deviceCount": 1 }
```
- New Resolve result codes: Stale `SourceVersionChanged` and Stale `SignedFactsChanged`.
- `OrderTravellerNotLinked` maps to `NeedsVerification`.

## 10. A01–A24 classification

Engine rows come from `S2VariantMatrixTests` (one row per registered variant). Source rows come from the B1 and B2 tests.

| Code | Classification | Basis |
|---|---|---|
| A01 extra bag | Supported | Matrix Accepted; B1 round-trip; inline |
| A02 weight package | **UnsupportedSource** (FlightOffer/Order) | The engine accepts a weight-concept allowance. The FlightOffer and Order allowances are always counted in pieces (`SharedRules.cs:358`, `FlightOfferShoppingContextMapper.BaggageFacts`). B1 test. |
| A03 overweight | Supported per piece; per kg **NeedsVerification** | `P32B_ENG_07` and `PRICE_04`: no amount without an authoritative chargeable weight |
| A04 oversize | **NeedsVerification** | Matrix: charge combination not verified |
| A05 cabin bag | Supported | Matrix |
| A06 bicycle | Supported (check at reserve) | Matrix; B1; B6 |
| A07 standard seat | Supported through SeatMaps, the batch and Resolve | B1 paid seat; existing seat tests |
| A08 exit-row seat | Supported when eligibility is verified and the terms are accepted; otherwise **NeedsVerification** | B2. No production source sets `VerifiedExitRowEligible` yet (section 13). |
| A09 extra seat | **NeedsVerification** | Matrix: adjacency not verified |
| A10 upgrade | **NeedsQuote** | Matrix; B6 dynamic quote; B3 `QuoteRequired` |
| A11 special meal | Supported (free) | Matrix |
| A12 pre-order meal | Supported with an authored menu item | Matrix. A definition without `MenuItemRef` cannot be authored, because the Domain refuses it. |
| A13 / A14 pet | **NeedsVerification** | Matrix: animal age not verified; B1 unverified pet gives no token |
| A15 wheelchair | Supported (free, lead time, check at reserve) | Matrix |
| A16 disability assistance | Supported (free) | Matrix; B3 inline SSR |
| A17 medical oxygen | **NeedsVerification** | Matrix: medical approval not verified |
| A18 bassinet | Supported with a verified infant-guardian pair | Matrix |
| A19 unaccompanied minor | **NeedsVerification** | Matrix: guardian contacts not verified |
| A20 / A21 lounge, fast track | Supported (check at reserve) | Matrix |
| A22 CIP | Supported per passenger; Order scope needs a PerItem rate | Matrix; B1; B5 Order scope |
| A23 priority boarding | Supported (free) | Matrix |
| A24 Wi-Fi | Supported | Matrix; B1; B3; B6 |

The fail-closed set required by v1.2 is retained and green: A03 per kg, A10, A19, A17, A04 and A13/14.

## 11. Commands and counts

All commands ran on 2026-10-20 in the working tree on `c865ccc`.

| Command | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | Build succeeded, 0 errors, 62 warnings. All warnings already existed: NU190x on `System.Security.Cryptography.Pkcs` 9.0.4; CS0618/CS8907/CS0108 in `Contracts/AeroTech.Messages` (Aegis, JetPay, LedgerFlow); CS8604 in `Framework/.../EnumTools.cs`; CS8620 in `SeatMapTests.cs:193`, a file this round did not change. |
| Shopping (`--list-tests` / run) | discovered 360, executed 360, passed 360, failed 0, skipped 0, not aborted |
| Domain conformance | discovered 259, executed 259, passed 259, failed 0, skipped 0, not aborted |
| Application acceptance | discovered 224, executed 224, passed 224, failed 0, skipped 0, not aborted (4 min 37 s). This includes the 9 `V12BoundaryAcceptanceTests`: route count, Providers file list and forbidden words. |
| Golden additive check (scratch script against `git show HEAD:` goldens) | 0 changed or removed values in 9 files |
| Line endings (byte check of the `.cs`, `.json` and `.md` files among the 68 changed or new paths, this report included) | 0 files with bare LF |
| Added comment lines (`git diff` plus new files) | 0 |

There is no CI run, and none is claimed.

## 12. Compatibility

- **SeatMap:** SeatMapsDto, its three routes and the SeatMap id are unchanged. `SeatMapSnapshot` is internal; it now keeps the priced POS contexts so a seat can be re-evaluated.
- **Legacy single Evaluate request:** unchanged.
- **List, Detail and Selection responses on the three surfaces:** additive only (goldens, section 8).
- **Resolve:** additive only.
- **Not added:** an aggregate, a PricedSelections table, a migration, IdentityServer, a DataProtection key table, a caller or auth framework, a route (the boundary route-count pin is green), a sibling-repo edit, or a guessed Ordering contract.

## 13. External limits (why the verdict is not `S2_CLOSED`)

1. **Ordering.** No Ordering code consumes the Resolve hand-off, or the B8/B5 rules, yet. Ordering is on hold by Owner order. No Ordering contract was guessed.
2. **Verified exit-row eligibility (A08).** No production provider sets `ShoppingTraveller.VerifiedExitRowEligible` (`ProvisionEvaluation.ProfileRules.cs:190`). Exit-row seats therefore stay unverified and not purchasable until DCS, Ordering or FlightFlow supplies an authoritative fact.
3. **A02 from FlightOffer/Order:** UnsupportedSource, because the allowance is counted only in pieces.
4. **FlightFlow occupancy and the AirInfo seat map:** these are used as they are today. Live end-to-end runs against those services were not part of this round.

## 14. Findings for the Owner (not changed in this round)

**F-1. Api Detail exposes internal option metadata.** This has been so since `7efee95`.
- `ApiAncillaryOfferDetailMapper` uses the shared `OfferSelectionDetailsMapper.OptionGroups`.
- As a result, each Api Detail choice carries:
  - `sourceVersion "D{definitionVersionId}-P{provisionId}"`;
  - `pricePreview.currencyId`.
- The new Api List projection omits both, as v1.2 Q7.4 asks.
- Changing the existing Detail field would break the contract, so it was left unchanged.

Recommendation: use the same Api projection on Detail, under a separate Owner order.

**F-2. A07/A08 in the generic list.**
- They show `DetailsOnDemand` with `TypedInputRequired`, because `seatNumber` is a typed field.
- No `SeatMap` mode was added, as Q7.3 requires.
- If the Owner wants a more specific presentation reason, for example `SeatMapRequired`, that is a naming decision for the Owner.
