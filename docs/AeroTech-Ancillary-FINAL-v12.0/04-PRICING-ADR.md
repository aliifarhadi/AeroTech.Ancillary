# 04 - AncillaryPricing: standalone filed price aggregate and invariants

## 1. Root (AR #3)
`AncillaryPricing { Id:long, AncillaryProvisionId:long, PricingUnit:PricingUnit, CurrencyId:int, FeeApplicationUnit:FeeApplicationUnit?, Version:int, Status:PricingStatus, CreatedAt:DateTimeOffset, ActivatedAt:DateTimeOffset?, SuspendedAt:DateTimeOffset?, RetiredAt:DateTimeOffset?, PriceLines:IReadOnlyCollection<AncillaryPricingLine> }`.
`PricingStatus = Draft(1), Active(2), Suspended(3), Retired(4)`; explicit values, no guessed renumbering of existing enums. `Version` monotonically increases per Provision; unique (ProvisionId,Version). `PricingUnit` is copied from same owning ServiceDefinition to immutable snapshot and validated through repository; NOT separately chosen by Backoffice. No circular domain navigation to Provision.

## 2. Distinct fee and charging dimensions
`PricingUnit` is the number of **commercial product units billed**, not the passenger selection. Closed Phase 1 enum (explicit values): `PerPassenger=1`, `PerRoom=2`, `PerItem=3`, `PerVehicle=4`, `PerSeat=5`, `PerPiece=6`, `PerKilogram=7`. Use only meaningfully defined values; existing platform naming if available overrides these code names after a verified identical semantic check. A product cannot change PricingUnit across ServiceDefinition versions of the SAME ServiceDefinitionRef; create a new commercial identity if charging semantics change.
`FeeApplicationUnit` from current Ancillary contract (`OneWay`, `RoundTrip`, `Item`, `SectorOrPortion`, `Ticket` and baggage-specific deferred units) answers **what travel/ticket/quantity coverage the fee applies to**, separate from product unit. Do not convert a room into ticket or a vehicle into a passenger. The ratio quantity/routing calculation belongs to future shopping, not Phase 1. Reject activating unsupported application units with unfrozen formula as v11 does; retain historical units/values without implementer-invented computation.
`RateSelector` on each PriceLine is `PassengerTypeCode? + AgeFromInclusive:int? + AgeToExclusive:int?`; it picks a rate within a single PricingUnit. `AgeRange` is not a PricingUnit. User can file 0-64 years and 65+ for PerPassenger insurance using two selector keys.

## 3. Child Entity `AncillaryPricingLine`
| Field | Type | Meaning |
|---|---|---|
| `Id` | long | immutable child identity |
| `AncillaryPricingId` | long | FK to root |
| `PassengerTypeCode` | `AirPrice.PassengerTypeCode?` | optional selector |
| `AgeFromInclusive` | `int?` | >=0 if supplied, >=0 years |
| `AgeToExclusive` | `int?` | > min when supplied; null means no upper limit |
| `Category` | `AncillaryPriceLineCategory` | retain `Ancillary=1`, `Tax=2`, `Fee=3` |
| `Code` | `string(10)?` | tax/fee official or carrier code, no inventing |
| `Name` | `string(100)?` | analyst-visible label |
| `CountryId` | `int?` | existing tax/fee evidence |
| `StationAirportId` | `int?` | existing tax/fee evidence |
| `Amount` | `decimal(18,2)` | fixed nonnegative filed monetary amount; positive for base of Paid |

**No child Line.Status** in Phase 1: publication happens at Pricing level, line edits Draft only. CurrencyId is on Pricing root and uniform across all lines; do not silently discard old per-Provision currency. Existing `ProvisionPriceLine.UnitAmount` becomes `AncillaryPricingLine.Amount` with preserved category/codes/country/station and real old/new ID migration mapping. PTC and age fields are intentionally repeated across monetary component lines; additional `AncillaryPricingRate`/fourth level is NOT part of v12 to avoid unapproved over-engineering.

## 4. Rate and component uniqueness
Define selector key `K=(PassengerTypeCode?,AgeFromInclusive?,AgeToExclusive?)`. For each K:
- EXACTLY ONE `Category=Ancillary` **base line** with `Amount>0` (Paid only), `Code` optional but base distinguished by category (do not allow multiple base lines that ambiguously sum).
- ZERO or MORE `Tax`/`Fee` line items attached to K; require nonempty code for tax/fee; no duplicate component under `(K,Category,Code,CountryId,StationAirportId)`.
- `UnitTotal(K) = sum(Amount for all lines with selector K)`, all in Pricing.CurrencyId. Do NOT sum different K selector alternatives to get one purchase's price. Do NOT automatically calculate a tax percentage or jurisdiction.
- If both generic `PTC=null` and specific PTC are present in same Pricing, reject activation (no implicit fallback precedence). If specific age bands and unbounded age band coexist for a given PTC, reject activation. Reject overlapping ages for same PTC; half-open `[from,to)` intervals, age in completed years. If one age bound is provided, `AgeFromInclusive` must exist; upper null means open-ended. No gap-filling or default-band inference. Different PTC keys may legally share age bands.
- For nonpassenger PricingUnits (PerRoom,PerVehicle,PerItem,PerPiece,PerKilogram,PerSeat), selectors must have null PTC/age in this Phase 1 model, unless owner explicitly approves a different business rule. For `PerPassenger`, permit PTC and/or age selectors. Product age calculation basis (travel date vs service date vs policy effective date) is not set by this price table; matching is deferred to future consumer requirements and never guessed now.
- A Pricing with multiple selector keys represents alternatives, never additive charges. Tax/fee component lines cannot exist without their base K. At least one base K needed for Active Pricing.

## 5. Lifecycle & cross-aggregate guards
`DefineDraft` => Draft; `ChangeDraftLines`; `Activate`; `Suspend`; `Reactivate`; `Retire`; `Revise` creates a new Draft with new ID and next version. Active and Suspended published lines are immutable; Retired terminal. `SwitchActivePricing(provisionId,newId,expectedOldId?)` lives in Application and is ONE transaction: lock/check parent, deactivate old, activate new, update both and projection atomically; caller receives conflict if race; database filtered UNIQUE `(AncillaryProvisionId) WHERE Status=Active` required. No window with two Active; when Provision is Active+Paid no state with zero active Pricings may commit. Per-row concurrency must prevent lost updates.
An Active Paid Provision MUST have exactly one Active Pricing; Free/NotAvailable MUST have no Active Pricing (can be authored without any Pricing). Draft Paid Provision may have Draft Pricing; activation order recommended: activate Pricing with Draft parent permitted, then activate Provision in same command/transaction or with temporary unpublished state. Explicit `PublishPaidProvision` application orchestration is allowed purely to coordinate two catalog aggregates, **not** an end-user runtime shopping engine.
Publisher checks ServiceDefinition.PricingUnit matches PricingUnit for all versions, ServiceDefinition Active and supplier active when relevant. If existing v11 data is ambiguous, publication blocked until migration mapping approved.

## 6. Examples (authoring only)
| Ancillary identity | fixed PricingUnit | Provision rules | Pricing selector / base |
|---|---|---|---|
| Insurance plan | PerPassenger | travel season/window | ADT age `[0,65)` = EUR 20; ADT `[65,null)`=EUR 40; CHD=EUR 10 |
| Shared transfer | PerPassenger | airport/route and date | ADT EUR 18, CHD EUR 9, INF EUR 0 **cannot be a Paid base=0**; use separate Free/NotAvailable Provision for INF |
| Private transfer | PerVehicle | route | generic EUR 80 |
| Hotel service | PerRoom | city/date | generic EUR 45 |
| SIM | PerItem | market | generic EUR 12 |
| Seat selection | PerSeat | cabin/aircraft characteristic | generic EUR 15 |
| Baggage | PerPiece or PerKilogram (distinct service identities) | flight/date/route | generic EUR 25 |

A v11 adult/child provision split may remain valid if eligibility differs; when conditions identical except PTC/amount, agent may PROPOSE consolidation into one Provision with multiple pricing selectors **only with clear identity/priority equivalence and explicit approved mapping**. Do not silently merge historical active provisions.

## 7. Validation negatives
Reject wrong PricingUnit; two active Pricing IDs for one Provision; multiple `Ancillary` base lines for selector; missing base; overlapping ages; a rate with generic and specific PTC simultaneously; tax currency inconsistency; duplicate tax code within selector; negative amount; missing currency; unsupported FeeApplicationUnit; activating a Paid Provision without active Pricing; Free/NotAvailable active priced row; active Pricing mutation; active Pricing deactivation leaving paid active Provision without replacement; `Switch` race.
