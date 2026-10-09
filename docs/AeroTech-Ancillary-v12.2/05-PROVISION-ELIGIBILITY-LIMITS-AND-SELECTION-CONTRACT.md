# 05 — Precise Provision contract and buyer selection metadata (P1+2)

## 1. Provision responsibilities
Keep existing ten typed Rule Entity groups as defined in 02. `AncillaryProvision` is a **commercial rule instance**, not an independent rule-engine type per profile. Exactly one matching `ServiceDefinitionId` version. A published Provision can match exactly one effective POS and a set of carrier/Fare/route/time/passenger predicates, then supply one `Outcome` and `PriceOrigin`, one coverage scope, accepted `QuantityRule` and an optional `ProfileRule`.

**POS decision:** One separately addressable published Provision per POS; `SalesRestrictions.PointsOfSale` must contain **exactly one POS** for new v12.2 published rows. (Any shared groups of shops must be pre-defined POS identities, not a magically unconstrained empty set.) A full copy per POS is expected business authoring; reuse UI templates and common eligibility builders rather than clone Domain aggregate classes.

## 2. Shared field-level contract
| Name | Type | Domain check |
|---|---|---|
| `ServiceDefinitionId` | long required | existing and matching version |
| `Sequence` | int > 0 | deterministic winner among overlapping Active rules; tie in same exact evaluation context must not publish |
| `PointOfSaleId` | long required logically, stored in existing SalesRestrictions rule row | one for v12.2; no silent global POS |
| `PurchaseStage` | PreOrder/PostTicketed/Both; additionally OnBoard **only for A24** if needed | legacy value never newly published; unsupported OnBoard vs API stage explicitly rejected |
| `CoverageScope` | existing enum, semantic Sector, Bound, Journey, Order, ServiceOccurrence | consistent PriceUnit + ServiceDateBasis and selected flight list |
| `Outcome` | existing Paid, Free, NotAvailable | paid pricing origin matches valid price; Free no paid price |
| `PriceOrigin` | Filed, ExternalQuote, Free, NotAvailable | constrained by Outcome; NOT two independent inconsistent Booleans |
| `QuantityRule` | Existing `(Unit,MinQuantity>=1,MaxQuantity>=Min)` | accepted units >=1, UI may show 0 no selection; kg vs packaged kg distinguished |
| `PassengerEligibility` | rule children PTC + age bands | no overlapping age bands with contradictory acts; child ages derived from verified date-of-birth later |
| `SalesRestrictions` | EffectiveDateTime?, discontinued?, single POS, customer type/customer filters | from<to; stable tenant owner; per POS separate |
| `Geography` | existing route/airport/via, country location rows | reject unsupported branch scope |
| `FlightApplication` | existing marketing/operating carrier, flight/aircraft | no spoofed flight occurrence |
| `FareApplication` | existing Fare/FareFamily/Cabin/RBD/FareBasis | don't infer inclusion from raw text |
| `TravelDate` | permitted+blackout DateOnly intervals | blackout first, then allow, inclusive dates |
| `DayTimeApplication` | explicit weekday masks, time windows, allow/deny | deny over allow; timezone source mandatory at airport-bound evaluation |
| `AdvancePurchase` | min/max + AirPrice TimeUnit + same-time-as-ticketed | min<=max; cutoff semantics tested at exact boundary |
| `BaggageApplication` | existing group fields only if Baggage profile | must agree with typed spec or explicit value-restricted override; never an arbitrary field on Pet/Meal |
| `SeatApplication` | existing group fields only if Seat profile | forbid Pet/Meal; no physical occupancy stored |
| `Availability` | existing MustCheckAvailability flag | flag not equivalent to Local quota/guaranteed; true permitted even for Unlimited |
| `Fulfillment` | existing ProviderKey | can name a supplier that requires confirmation but not fake completed provider call |

## 3. Minimum additional closed `ProfileRule` (as needed for published P1/2)
Do not duplicate common Passenger/Flight/Geography Rule groups. Provide only narrow typed **per-profile predicate** held by Provision, with correct mapping/ETag/EF:
- `PetRule` optional: `CountryExceptionCode?`, `MinAnimalAgeWeeksOverride?`, `MaxCombinedKgOverride?`, `AcceptanceMode`. Overrides cannot **widen** Definition safety maxima without new Definition version.
- `AssistedTravelRule`: minimum lead time, allowed connection classes, `MedicalApprovalRequired` where genuinely narrower than Definition. Uses variant tags to avoid medical fields on wheelchair.
- `AirportServiceRule`: terminal/time window/direction overrides, allowed facility, guest maximum not exceeding Spec.
- `UpgradeRule`: eligible from/to Cabin filtering beyond shared FareApplication, no fabricated class availability.
- `BaggageRule` and `SeatRule` existing two Rule groups remain, adjusted to enforce variant compatibility.
- `Meal/Priority/Connectivity` rely on existing Shared Rule groups + Definition constraints unless a test demonstrates otherwise; do **not** create placeholder empty tables.

## 4. Limits are *three distinct objects*
1. `QuantityRule` accepted amount **per Add selection**. Existing max and min; UI stepper may present 0 to skip.
2. `PassengerUsageLimit` cumulative allowed quantity per `CountingFamilyCode` and a typed scope: existing `PerOrder`, `PerFlightOccurrence`, `PerServiceDate`; **add `PerPortion=4`** (preserve values 1–3). v12.2 Config stores max but no cross-order ledger. If per-selection cap differs, both apply: AcceptedQty <= Min(current-step remaining, cumulative eligibility remaining). Do not count purchased `10kg package` as one *kilogram*: support explicit `ConsumptionUnit=PurchasedUnit|Kilogram` and `UnitsPerPurchase` in the commercial limit if the same family combines packages; otherwise forbid mixed-unit family rather than miscount.
3. `PhysicalCapacity`: verified resource slot/Count/Weight owned and provisioned in P2, with a separate resource ref; never infer from a passenger limit. Actual Remaining requires P3 Hold/Allocation.

## 5. Define `CustomerSelectionContract` (authored P1, instantiated P3)
Immutable typed metadata attached to Definition/variant: a closed list of *known fields required for this variant*, **no arbitrary input names or regex execution**. Examples:
| Variant | Allowed selector fields (type) | Required when |
|---|---|---|
| Baggage piece | `Quantity:int`, `BoundRef:string`, `TravellerRef:string` | A01 |
| Baggage weight package | `PackageProductRef:string`, `Quantity:int`, Bound/Traveller | A02; `PackageWeightKg` fixed in spec |
| Pet cabin/hold | `AnimalType:enum`, `CombinedWeightKg:decimal`, `DimensionsCm`, `DocumentAcknowledgements:set<code>` | A13/A14 |
| Seat | `FlightRef`, `SeatNumber:string`, `Purpose?:enum` | A07/A08/A09 |
| Meal | `MealCode` or `MenuItemRef`, `FlightRef`, `Quantity:int` | A11/A12 |
| Wheelchair/Assistance | `AssistanceSsrCode`, `FlightRef` | A15/A16 |
| Medical | `EquipmentCode`, required validation documents/quantity | A17 |
| Bassinet | `InfantRef`, `GuardianRef`, `FlightRef` | A18 |
| UMNR | `ChildRef`, `GuardianHandoffContact`, `GuardianPickupContact` (types only, sensitive values future) | A19 |
| Airport service | `AirportId`, `FacilityRef?`, `TimeWithOffset`, `GuestCount:int` | A20–A22 based on appointment requirement |
| Priority | `OptIn:bool`, `FlightRef` | A23 |
| WiFi | `PlanCode`, `DeviceCount:int?` | A24 |

`Required/Optional` is determined by closed variant schema and airline spec, not stored as unvalidated arbitrary user-provided fields. Code can return a schema DTO for Backoffice UI; **P1/P2 must round-trip the schema**, but must NOT persist actual traveller's declared medical/pet/guardian details. Avoid sensitive fields in logs and reporting. A variant cannot be published when mandatory input definition is absent/inconsistent.

## 6. Outcome/booking semantics matrix
| Outcome + Pricing Origin | Publish pricing rule | Future selection behavior |
|---|---|---|
| Paid + Filed | Active PricingRate required with valid Money; exact currency/PTC selection | price accepted snapshot P3 |
| Paid + ExternalQuote | no fake PricingRate; trusted quote provider/mode and Booking policy required | no sale without quote; expired quote -> reprice |
| Free + Free | no Active Pricing, no amount zero-valued fake line | Service Request, possible Pending confirmation |
| NotAvailable + NotAvailable | no Active Pricing | no purchasable service offer |
| any other pair | reject | N/A |

P1/P2 authoring must make all four pathways selectable as *contracts* even if real supplier quote execution is P3. Preserve `CommercialOutcome.DocumentRequired` and `BookingRequired` validity; a booking without payment can be valid where airline defines Free, but without appropriate confirmation is not automatically fulfilment.
