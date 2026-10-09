# Pricing, publication and commercial invariant contract

## Why AR3 stays separate
AncillaryPricing is independently versioned and reusable only WITHIN its one parent Provision. It has one product PricingUnit inherited from ServiceDefinition. One Provision can have N versions but at most one Active at any moment. Changing an active price never changes identity or conditions of the existing Provision.

## PricingLine (existing v12 schema preserved)
`AncillaryPricingLine(Id, AncillaryPricingId, PassengerTypeCode?, AgeFromInclusive?, AgeToExclusive?, Category, Code?, Name?, CountryId?, StationAirportId?, Amount)`.
- `Category`: `Ancillary=1` base, `Tax=2`, `Fee=3` (canonical v12 values). This is a monetary COMPONENT category and is not an ATPCO Fare Rules category.
- Pricing root has `CurrencyId:int` and `decimal(18,2)` amounts (current platform convention; reject invalid precision). No implicit currency conversions.
- Selector key `K=(PTC?,AgeFromInclusive?,AgeToExclusive?)`. Exactly 1 positive base line per K; 0..N nonnegative tax/fee lines with distinct `(K,Category,Code,CountryId,StationAirportId)`; no orphan tax/fee.
- For PerPassenger: either all PTC selectors null OR all are explicit PTC values; do not silently mix generic and specific rates. Within each PTC, optional age bands must be non-overlapping; no generic unbounded band alongside specific finite band for that PTC. Null To is open-ended. Age-selector alternatives never sum with each other.
- For any other PricingUnit: all PTC/Age are null and there must be exactly 1 base selector.
- `UnitTotal(K)=sum(Base,Tax,Fee)` for one selected K ONLY. Quantity multiplier and fee application coverage require an authorized consumer calculation and must never mix different alternatives or quantities.
- Free or NotAvailable Provision does not get an active Pricing; no Paid base with Amount=0; a free infant may be represented by a distinct matching Free Provision or an approved future zero-rate policy (not invented here).
- AdvancePurchase and date/flight/sales rules live on Provision, not Pricing. Different seasonal price while preserving one active price per Provision -> two independently conditioned Provisions with distinct Sequence. Do not introduce parallel effective-date prices beneath one Provision without business authorization.

## Lifecycle
ServiceDefinition: Draft -> Active <-> Suspended -> Retired (Retired terminal). Published changed definition -> Revise(new id/version) with fixed PricingUnit/DateBasis.
Provision: Draft -> Active <-> Suspended -> Retired; replace ACTIVE semantics by a new Draft row at same Sequence and atomic supersession.
Pricing: Draft -> Active <-> Suspended -> Retired; Revise(new id/version), SwitchActivePricing(expectedOldId,newId). No direct active line edit.
All newly authored Pricing must have known product unit; nullable PricingUnit remains only as documented transitional legacy and blocks publication.

## Atomic guards
- Activation of Paid Provision without EXACTLY one active compatible Pricing fails.
- Activation of Free/NotAvailable Provision with active Pricing fails.
- Standalone pricing Activate permitted for Draft/Paid Provision but does not yet make service offerable until Provision Active and parent ServiceDefinition Active.
- Suspend/Retire of active Pricing when parent is Active/Paid must fail unless the same transaction atomically switches a replacement or retires/disables the parent Provision.
- A successful Switch deactivates old then activates new in one database transaction and synchronizes the read model before response; `expectedOldId` stale -> conflict. Filtered unique index and concurrency control protect races.
- Role restriction: Backoffice only; user identity and audit existing framework. Existing Hold/Get/Confirm and provider APIs frozen.

## Unknown vs forbidden semantics
- NotAvailable = matching rule explicitly forbids sale.
- NoMatch = no active matching rule; not saleable but NOT the same as an authored exclusion.
- UnsupportedContext = a restricted field needs missing or unverified context; refuse sale without fallback.
- OutOfStock = later Stock service says no units (Phase 2+3), not a Provision eligibility failure.
These outcomes must never collapse to one ambiguous bool in the eventual shopper/consumer contract.
