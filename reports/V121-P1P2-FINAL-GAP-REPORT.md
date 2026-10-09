# AeroTech Ancillary v12.1 — Phase 1 + Phase 2 final completion — baseline and gap report

Date: 2026-10-09
Written before any code change of this iteration.
Authority: `docs/AeroTech-Ancillary-v12.1/` files `00-START-HERE` … `10-CODING-AGENT-PROMPT` (13 manifest entries of `SHA256SUMS.txt` verified).

## 1. Baseline

| Item | Value |
|---|---|
| Branch / HEAD | `feat/ancillary-v12.1-phase2-stock` @ `1abf7a53e0efb9eb892097977327718c61486158` = `origin` branch |
| Target | `k8s-stg` @ `933b7b7a793b426dbcb6362bbf8519886edf9080` |
| Working tree | no tracked source change. Untracked: the pack files and zip under `docs/` (Owner), `reports/V12.1-PRICING-PREMERGE-AUDIT.md` (read-only audit of the same day). `docs/AeroTech-Ancillary-v12.1/SHA256SUMS.txt` modified by the Owner's pack. |
| `dotnet build --no-incremental` at this SHA (re-run 2026-10-09) | 13 warnings, 0 errors |
| Domain conformance tests (re-run) | 200 / 200 |
| Application acceptance tests on SQL Server (re-run) | 153 / 153 |
| HTTP smoke of Phase 2 | 76 / 76 on 2026-10-09, before the commit; not repeated here |
| Frozen reservation files | 36 files, combined SHA-256 `6974ABA2C62BAFE7CDE159BDCE8A4969F1CAECCF25368012F30D1BE7DDA4E486` (asserted by `P2_X07`) |
| Command migrations | `InitialAncillary`, `V11Phase1CommercialAuthoring`, `V12Phase1NormalizedProvisionAndPricing`, `V121Phase1RuleGroups`, `V121Phase2StockCapacity`, `V121LegacySchemaCleanup` (+ the `…Query` counterparts) |

Dev database `DotAirAncillary` (all migrations applied):

| Measure | Value |
|---|---|
| Definitions / provisions / pricings | 31 / 99 / 69 |
| Pricings by status | 69 Active, all version 1 |
| Pricings by currency | IRR 28, USD 41 |
| Price lines | 97 = 69 base + 28 tax + 0 fee; none with passenger type or age band |
| Root `FeeApplicationUnit` | Item 50, SectorOrPortion 12, Ticket 7 |
| Inventory policies / sources / adjustments | 0 / 0 / 0 |
| `ReferenceData.Currencies` | 170 rows; `DecimalPlaces` = 2 for EUR and USD only, 0 for the other 168 (KWD, BHD, OMR, GBP, CHF, AED included); `RoundingFactor` 1000 for IRR, 0 elsewhere |

## 2. Current names that the corrections touch

| Area | Current |
|---|---|
| Pricing | `AncillaryPricing` (root: `CurrencyId`, `FeeApplicationUnit`, `PriceLines`), `AncillaryPricingLine` (`Category` Ancillary / Tax / Fee, `Amount` with exactly two decimals), `AncillaryPricingLineArgs`, `PricingLineInput`, `BackofficePricingDto` + `BackofficePricingLineDto` + `BackofficePricingRateDto`, tables `AncillaryPricings`, `AncillaryPricingLines` in both schemas |
| Currency scale | `CurrencyReadModel.DecimalPlaces` exists in `ReferenceData`; no code reads it |
| Booking | `BookingDefinition(Method, SsrCode, SsimCode)` on `AncillaryServiceDefinition` |
| Provision | no purchase stage; `ProvisionAdvancePurchaseRule(MinimumPeriod, Unit, SameTimeAsTicketed)`; `ProvisionBaggageApplicationRule` without charge kind or allowance concept |
| Inventory policy | `AncillaryInventoryPolicy.Activate` refuses `Unlimited` when an Active provision has `MustCheckAvailability` (16606); `ServiceDefinitionId` is stored at Define and never reconciled |
| Reference ports | `NotConnectedFlightOccurrenceReference`, `…InventoryResourceReference`, `…AirportFacilityReference`, `…FlightFlowDelegationReference`, `…CountingFamilyReference` |

## 3. Required deltas (docs 01–09)

| # | Delta | Source |
|---|---|---|
| G1 | Replace the flat line by `AncillaryPricingRate` (selector + base `Money`) with `AncillaryPriceComponent` children (Tax / Fee `Money`, code, country, station, fee unit on Fee only, `TaxIncludedInSource` on Tax only) | D01, D02, D05, D06; doc 02, 03 |
| G2 | `Money(Amount decimal(19,6), CurrencyId)`; currency must exist; scale ≤ `DecimalPlaces`; component currency = rate currency; no conversion | D02, D07; doc 03 |
| G3 | Several currencies in one Active pricing; rate key `(CurrencyId, PTC?, AgeFrom?, AgeTo?)`; non-passenger unit: exactly one rate per currency | D03; doc 03 |
| G4 | Unit total = base + taxes + fees applied per item; fees per ticket / one-way / round-trip / sector are reported separately, never summed into the unit total | doc 03 |
| G5 | Root `CurrencyId` and `FeeApplicationUnit` stop being authoritative; legacy rows migrate one-to-one with lineage; no rounding | doc 03, 08 |
| G6 | Backoffice commands, validators, read model, DTOs and paginated list move to rates; the flat payload is refused explicitly | doc 03 (PR22, PR24), doc 08 |
| G7 | `BookingDefinition.ConfirmationRequirement` (Immediate, SubjectToConfirmation) | doc 01, 04 |
| G8 | `AncillaryProvision.PurchaseStage` (PreOrder, PostTicketed, Both, LegacyUnspecified — not newly activatable) | doc 01, 04 |
| G9 | `ProvisionAdvancePurchaseRule.MaximumPeriod?` ≥ minimum, same unit | D08; doc 04 |
| G10 | `ProvisionBaggageApplicationRule.ChargeKind` + optional `AllowanceConcept`; WeightPackage ↔ Weight, ExtraPiece ↔ Piece | doc 01, 04 |
| G11 | Remove the Unlimited + `MustCheckAvailability` refusal; expose "requires check" in the admin snapshot; never a guarantee | D10; doc 05 |
| G12 | Policy identity stays `(OwnerAirlineId, ServiceDefinitionRef)` across definition revisions; `ServiceDefinitionId` is reconciled to the current version | doc 05 |
| G13 | Strict reference gating stays as is | D11 |
| G14 | Daily / RoomNight / AssignedAsset stay unsupported | D12 |
| G15 | Tests PR01–PR25, F01–F15 × DEF / RULE / PRICE / POLICY, X01–X20, migration and reconciliation proofs | doc 09 |

## 4. Facts that limit what can be proven

| Fact | Consequence |
|---|---|
| Dev currency reference has `DecimalPlaces = 0` for every currency except EUR and USD | The scale rule is enforced against the reference as it is. Zero / two / three decimal behaviour is proven with currencies of the test database; on dev a KWD or GBP amount with decimals would be refused until the reference data is corrected by its owner. |
| All 69 dev pricings carry a root fee unit and none has a Fee line | D06 moves the unit to Fee components only, so these 69 values have no component to move to. They stay in the legacy root column as lineage and are not part of the new model. |
| Five reference ports are not connected | Local, FlightFlow and usage-limited policies cannot be activated in a deployed host (D11). Reported as blocked, not as passed. |
| No consumer of Pricing exists | Rate selection for a traveller and currency (PR03) is proven with the test-only oracle, as in Phase 1; no shopping evaluator is added. |

## 5. Red-test evidence

Recorded on 2026-10-09, before the pricing implementation: the first build of `V121FinalPricingConformanceTests` (PR01–PR25 at domain level) failed with 8 compile errors (CS0234 ×2, CS0246 ×14 occurrences) because `Money`, `AncillaryPricingRateArgs`, `AncillaryPriceComponentArgs` and the rate oracle did not exist. The raw compiler output is kept outside the repository (scratch file `red-domain-build.txt`).

What was **not** written red-first, stated plainly:

- The descriptor tests (`V121FinalDescriptorConformanceTests`), the family matrix (`V121FinalFamilyAcceptanceTests`, F01–F15 × DEF / RULE / PRICE / POLICY), the cross-phase cases (`V121FinalCrossPhaseAcceptanceTests`, X01–X20) and the SQL pricing cases (`V121FinalPricingAcceptanceTests`) were written after their production code, in the same iteration. No failing run of those tests against the old code was recorded.
- For D10 and for the stable policy identity the old behaviour was pinned by tests that existed at `1abf7a5` and asserted the opposite (`P2_C01_X09…is_not_declared_unlimited` and `P2_X09_legacy_definition_authority_is_not_inferred` expected 16606). Those two tests were changed to the new rule; that change is the only evidence that the behaviour moved.
