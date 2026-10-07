# 06 — FINAL Scenario & Conformance Catalog v9.1

**Status:** implementation acceptance authority.  
**Rule:** every scenario marked `CORE` must have an automated test or an explicit cross-service contract/conformance test. A `FUTURE` item must not appear as inactive nullable domain fields.

---

# A. Supplier marketplace

| ID | Scenario | Class | Expected |
|---|---|---|---|
| A01 | Register active Local supplier | CORE | unique ID, airline owner, name, `FulfillmentKind=Local` |
| A02 | Register active External supplier | CORE | `FulfillmentProviderKey` required |
| A03 | External supplier with empty provider key | CORE | reject |
| A04 | Unknown external provider key at production activation/use | CORE | deterministic configuration failure |
| A05 | Two suppliers publish equivalent lounge definitions | CORE | both may be shoppable if provisions match |
| A06 | Supplier identity included in AirAvail internal catalog | CORE | `SupplierId` available to accepted detail |
| A07 | Supplier name exposed only where contract requires display | CORE | runtime routing does not depend on name |
| A08 | Retire supplier | CORE | no new shopping for its definitions |
| A09 | Existing sold service from retired supplier | CORE | can still validate/cancel/complete historical fulfillment |
| A10 | Definition references nonexistent supplier | CORE | reject |
| A11 | Definition references retired supplier at new publication | CORE | reject new publish |
| A12 | Local supplier reserve/confirm/read/cancel | CORE | local Ancillary capability selected |
| A13 | External supplier reserve/confirm/read/cancel | CORE | registered adapter selected by provider key |
| A14 | Behavior selected by `switch (SupplierId)` | NEGATIVE | forbidden |
| A15 | Supplier is airline itself | CORE | supported as ordinary supplier case |
| A16 | External supplier credentials/URL/token in Supplier entity | NEGATIVE | forbidden |

---

# B. Industry ServiceSubCode reference

| ID | Scenario | Class | Expected |
|---|---|---|---|
| B01 | Industry subcode exists in approved reference | CORE | accepted |
| B02 | Industry subcode does not exist | CORE | reject |
| B03 | Caller supplies conflicting RFIC/group semantics for industry code | CORE | reject |
| B04 | Industry semantics copied/validated from reference | CORE | deterministic |
| B05 | Carrier-defined allowed code with explicit semantics | CORE | accepted |
| B06 | Carrier-defined definition published | CORE | semantics frozen with definition version |
| B07 | Donor two-row reference used as whole production dataset | NEGATIVE | forbidden |
| B08 | Mutable `ServiceSubCode` Aggregate restored | NEGATIVE | forbidden |
| B09 | Backoffice reference lookup displays canonical description/group/RFIC | CORE | supported |

---

# C. ServiceDefinition lifecycle

| ID | Scenario | Class | Expected |
|---|---|---|---|
| C01 | Define draft service | CORE | Draft |
| C02 | Definition has mandatory SupplierId | CORE | required |
| C03 | Define standard, baggage, seat-capable service | CORE | typed application semantics |
| C04 | Activate/publish valid definition | CORE | immutable published version |
| C05 | Revise active definition | CORE | new version/row, old sold IDs remain resolvable |
| C06 | Suspend | CORE | excluded from new shopping |
| C07 | Retire | CORE | excluded from new shopping; historical IDs valid |
| C08 | Resolve sold service by Ref+Version | NEGATIVE | forbidden |
| C09 | Use Ref+Version for display/audit | CORE | allowed |
| C10 | One aggregate per Meal/Lounge/Baggage/Seat | NEGATIVE | forbidden |

---

# D. Provision authoring and precedence

| ID | Scenario | Class | Expected |
|---|---|---|---|
| D01 | Create provision for ServiceDefinitionId | CORE | valid |
| D02 | Sequence unique/valid within intended service scope | CORE | deterministic ordering |
| D03 | Lowest matching sequence wins | CORE | deterministic |
| D04 | Matching NotAvailable provision wins at its sequence | CORE | terminates evaluation |
| D05 | No match | CORE | no offer |
| D06 | Free outcome | CORE | zero monetary total, no fabricated paid line |
| D07 | Sales effective date | CORE | honored |
| D08 | Travel effective date | CORE | honored |
| D09 | Suspended/retired provision in shopping | CORE | excluded |
| D10 | Static overlap/shadow preview | CORE | Backoffice can diagnose authoring conflicts |
| D11 | Static Preview chooses live passenger winner | NEGATIVE | forbidden; only AirAvail evaluates live context |

---

# E. Passenger criteria

| ID | Scenario | Class | Expected |
|---|---|---|---|
| E01 | No PTC restriction | CORE | all supported PTCs eligible |
| E02 | ADT-only provision | CORE | ADT eligible, others not |
| E03 | CHD-specific price | CORE | separate provision can win |
| E04 | INF-specific applicability | CORE | separate provision can win |
| E05 | Age fields exist in v9.1 domain | NEGATIVE | forbidden |
| E06 | Occurrence fields exist in v9.1 domain | NEGATIVE | forbidden |
| E07 | FF status/keyword/customer score fields exist | NEGATIVE | forbidden |

---

# F. Sales / POS criteria

| ID | Scenario | Class | Expected |
|---|---|---|---|
| F01 | No POS restriction | CORE | available across authorized POS contexts |
| F02 | PointOfSaleIds restriction | CORE | only matching canonical POS IDs |
| F03 | CustomerIds restriction | CORE | exact match |
| F04 | CustomerTypes restriction | CORE | exact canonical type match |
| F05 | Duplicate raw country/channel/office rule arrays instead of POS | NEGATIVE | forbidden when POS already represents them |
| F06 | PCC field without canonical source | NEGATIVE | absent |

---

# G. Travel criteria

| ID | Scenario | Class | Expected |
|---|---|---|---|
| G01 | Origin airport | CORE | match |
| G02 | Destination airport | CORE | match |
| G03 | Exact route pair | CORE | no accidental cartesian product |
| G04 | Via airport | CORE | match when context supports it |
| G05 | Travel date interval | CORE | inclusive semantics frozen in code/tests |
| G06 | Day of week | CORE | match |
| G07 | Departure time window | CORE | match |
| G08 | Marketing airline | CORE | match |
| G09 | Operating airline | CORE | match |
| G10 | Flight number | CORE | match |
| G11 | Concrete FlightId exception | CORE | exact flight instance match |
| G12 | AircraftId | CORE | match canonical AirInfo/flight aircraft ID |

---

# H. Fare criteria

| ID | Scenario | Class | Expected |
|---|---|---|---|
| H01 | AirFareId | CORE | exact match |
| H02 | AirFareType | CORE | exact match |
| H03 | FareFamilyId | CORE | exact canonical ID match |
| H04 | FareBasis | CORE | normalized exact string match |
| H05 | CabinClassId | CORE | exact ID match |
| H06 | RbdId | CORE | exact ID match |
| H07 | AirAvail pre-order context missing FareFamilyId for FareFamily rule | CORE | contract/conformance failure, not silent fallback to name |
| H08 | Ordering post-order context missing FareFamilyId | CORE | contract/conformance failure, not name-based guess |
| H09 | Ticket designator/account/tour/tariff/rule-number fields | FUTURE | absent from v9.1 code |

---

# I. Money and FX

| ID | Scenario | Class | Expected |
|---|---|---|---|
| I01 | Filed ancillary monetary line | CORE | `decimal`, persisted per platform money precision (`decimal(18,2)`) |
| I02 | Negative filed monetary line | CORE | reject unless explicit credit concept exists in future |
| I03 | AirAvail target currency differs from filed currency | CORE | AirAvail converts using platform ROE source |
| I04 | AirAvail detail includes filed and selling evidence needed by Ordering | CORE | yes |
| I05 | Ordering snapshots accepted selling totals/ROE | CORE | yes |
| I06 | Ancillary Hold contains CurrencyId | NEGATIVE | forbidden |
| I07 | Ancillary Hold contains AcceptedRevenue / selling amount | NEGATIVE | forbidden |
| I08 | Ancillary reprices during Hold | NEGATIVE | forbidden |
| I09 | Ancillary performs FX conversion | NEGATIVE | forbidden |
| I10 | ROE precision reduced to money scale | NEGATIVE | forbidden; high-precision ROE retained |

---

# J. Pre-order ancillary shopping

| ID | Scenario | Class | Expected |
|---|---|---|---|
| J01 | Surface owns valid priced flight Offer | CORE | request reaches AirAvail with offer context |
| J02 | AirAvail builds/recovers full evaluation context | CORE | traveller, flight, fare, POS, currency available |
| J03 | Generic ancillary offers | CORE | `AncillaryOffers` returns eligible priced items |
| J04 | Offer list exposes internal ProvisionId | NEGATIVE | hidden from normal customer surface |
| J05 | Details for selected OfferItem | CORE | resolves exact ServiceDefinitionId/ProvisionId internally |
| J06 | Details expiration | CORE | stale offer rejected/repriced according to contract |
| J07 | No eligible provision | CORE | item absent, not fake NotAvailable product unless surface contract requires it |

---

# K. Post-order ancillary shopping — one-way dependency

| ID | Scenario | Class | Expected |
|---|---|---|---|
| K01 | Customer asks ancillaries for existing Order | CORE | surface request starts at Ordering |
| K02 | Ordering authorizes Order access | CORE | before shopping |
| K03 | Ordering builds authoritative shopping context | CORE | from current Order facts |
| K04 | Ordering calls AirAvail AncillaryOffers | CORE | one-way dependency |
| K05 | AirAvail calls Ordering to fetch Order context | NEGATIVE | forbidden |
| K06 | Ordering returns AirAvail surface-mapped result | CORE | yes |
| K07 | Post-order Details | CORE | Ordering -> AirAvail Details |
| K08 | Offer bound to OrderId + CommercialVersion | CORE | stale commercial version rejected |
| K09 | Existing services/quantities passed in context | CORE | evaluator can enforce quantity/max logic |
| K10 | Cancelled/ended service counted as active existing occurrence | CORE | no, only applicable active/current state |

---

# L. Single evaluator

| ID | Scenario | Class | Expected |
|---|---|---|---|
| L01 | Production generic ancillary evaluation | CORE | AirAvail `AncillaryOfferEvaluator` |
| L02 | Production post-order evaluation | CORE | same evaluator |
| L03 | Backoffice simulation | CORE | `POST Backoffice/v1/AncillaryOffers` in AirAvail uses same evaluator |
| L04 | Ancillary static Preview | CORE | validates authoring/reference/overlap only |
| L05 | Ancillary runtime `Simulate` endpoint | NEGATIVE | absent |
| L06 | Separate Ancillary live eligibility evaluator | NEGATIVE | absent |
| L07 | Two code paths disagree on winning Provision | NEGATIVE | impossible by design; one evaluator |

---

# M. ID-based accepted snapshot

| ID | Scenario | Class | Expected |
|---|---|---|---|
| M01 | AirAvail Details returns internal accepted identity | CORE | ServiceDefinitionId + ProvisionId + SupplierId |
| M02 | Ordering stores those IDs in accepted ancillary snapshot/service detail | CORE | yes |
| M03 | Ordering stores display metadata | CORE | Ref/version/name may be copied for history |
| M04 | Historical operation resolves definition by ID | CORE | yes |
| M05 | Historical operation resolves by ServiceDefinitionRef+Version | NEGATIVE | forbidden |
| M06 | Provision row mutable after publish so ID meaning changes | NEGATIVE | forbidden |

---

# N. Hold / reservation

| ID | Scenario | Class | Expected |
|---|---|---|---|
| N01 | Hold request includes OrderId and idempotency key | CORE | yes |
| N02 | Hold unit includes OrderServiceId | CORE | yes |
| N03 | Hold unit includes ServiceDefinitionId/ProvisionId | CORE | yes |
| N04 | Hold unit includes SupplierId from caller and trusts it blindly | NEGATIVE | Ancillary resolves/validates SupplierId from definition |
| N05 | Hold validates definition/provision relationship | CORE | yes |
| N06 | Hold validates quantity/coverage identity | CORE | yes |
| N07 | Hold validates Ancillary-owned stock quota | CORE | when applicable |
| N08 | Hold validates live physical seat occupancy | NEGATIVE | FlightFlow owns seat state |
| N09 | Idempotent replay same request | CORE | same effect/result |
| N10 | Concurrency on last quota unit | CORE | no oversell |
| N11 | Confirm | CORE | supplier-specific behavior allowed |
| N12 | Release pre-confirmation | CORE | supplier-specific behavior allowed |
| N13 | Read-back/validation | CORE | supplier-specific capability |
| N14 | Cancel confirmed | CORE | supplier-specific behavior allowed |

---

# O. Supplier fulfillment boundary and accountable EMD

| ID | Scenario | Class | Expected |
|---|---|---|---|
| O01 | Generic supplier `Issuance` endpoint exists without concrete supplier contract | NEGATIVE | forbidden in v9.1 CORE |
| O02 | No-op supplier issuance capability added only to satisfy abstraction | NEGATIVE | forbidden |
| O03 | Supplier fulfillment allocates airline EMD number | NEGATIVE | forbidden |
| O04 | Ordering issues EMD-A | CORE | Ordering owns document |
| O05 | Ordering issues EMD-S | CORE | Ordering owns document |
| O06 | EMD coupon linked to OrderService | CORE | yes where applicable |
| O07 | Supplier/provider references from reserve/confirm preserved as provider evidence | CORE | yes |
| O08 | Ordering stale validation before accountable issue | CORE | provider/Ancillary validation used as required |

---

# P. AncillaryStockPool

| ID | Scenario | Class | Expected |
|---|---|---|---|
| P01 | Ancillary-controlled quota configured | CORE | ServiceDefinitionId + FlightId identity |
| P02 | Same service definition on two flights | CORE | separate pools |
| P03 | Same type from two suppliers | CORE | separate definitions/pools |
| P04 | Pool keyed by ServiceDefinitionRef+Version | NEGATIVE | forbidden |
| P05 | Pool used for physical seat map | NEGATIVE | forbidden |
| P06 | Remaining quantity decremented/held safely | CORE | concurrency-safe |

---

# Q. Paid seat authoring and shopping

| ID | Scenario | Class | Expected |
|---|---|---|---|
| Q01 | Analyst selects Aircraft from AirInfo reference | CORE | canonical ID |
| Q02 | Analyst loads static seat map | CORE | AirInfo-backed layout/reference |
| Q03 | Analyst selects exact seat numbers | CORE | saved in SeatApplication with applicable aircraft scope |
| Q04 | Analyst selects PADIS characteristics | CORE | saved as characteristic criteria |
| Q05 | Seat rule by FareFamily/PTC/route/date | CORE | supported |
| Q06 | Shopping concrete flight seat map | CORE | live state from FlightFlow |
| Q07 | AirAvail prices eligible available seat | CORE | commercial + live state composition |
| Q08 | Blocked/occupied seat offered | NEGATIVE | forbidden |
| Q09 | SeatOffers same endpoint as generic list with no seat state | NEGATIVE | separate seat retailing operation |
| Q10 | Ancillary owns seat assignment/hold | NEGATIVE | FlightFlow owns physical seat state through Ordering provider flow |

---

# R. Backoffice authoring

| ID | Scenario | Class | Expected |
|---|---|---|---|
| R01 | Supplier option lookup | CORE | available |
| R02 | Industry subcode lookup | CORE | read-only |
| R03 | Aircraft/cabin/airport/currency lookup | CORE | from ReferenceData/AirInfo authority |
| R04 | FareFamily/RBD/AirFare lookup | CORE | canonical AirPrice source |
| R05 | Bulk matrix expands to Provision drafts | CORE | no second persistence model |
| R06 | Preview invalid reference | CORE | validation error |
| R07 | Preview overlapping/shadowed rows | CORE | diagnostic |
| R08 | Publish valid rows | CORE | immutable active provisions |
| R09 | Simulate concrete passenger/flight/fare | CORE | call AirAvail Backoffice evaluator |
| R10 | Backoffice introduces generic JSON rule language | NEGATIVE | forbidden |

---

# S. Reference-data ownership

| ID | Scenario | Class | Expected |
|---|---|---|---|
| S01 | Aircraft | CORE | AirInfo authority |
| S02 | Cabin class | CORE | AirInfo authority |
| S03 | Static aircraft seat map | CORE | AirInfo authority |
| S04 | Airport/city/country/currency/airline | CORE | AirInfo authority |
| S05 | FareFamily/RBD/AirFare/POS | CORE | AirPrice authority |
| S06 | Flight instance/live seat state | CORE | FlightFlow authority |
| S07 | Customer/agency/office identity | CORE | AeroCore/Core authority |
| S08 | Ancillary creates duplicate aircraft master | NEGATIVE | forbidden |

---

# T. Deletion / anti-regression audit

Before Milestone 6 final closure the codebase must prove all of these:

```text
no target AncillaryProduct aggregate
no target AncillaryPriceRule aggregate
no mutable ServiceSubCode aggregate
no rule-evaluator implementation in Ancillary
no Ancillary /Simulate live endpoint
no AirAvail -> Ordering order-context fetch
no ServiceDefinitionRef+Version runtime FK
no CurrencyId/AcceptedRevenue in Ancillary Hold
no v9.1 age/occurrence/FF/keyword/customer-score inactive fields
no mileage/external-quote/upgrade inactive methods
no Supplier prohibition
no SupplierId behavior switch
no leaked Supplier.FulfillmentProviderKey through AirAvail/public contracts
no generic Supplier Issuance/Activation endpoint
no duplicate aircraft/fare/customer reference masters
no EMD number allocation in Ancillary
```

Legacy donor code may still exist only if the implementation plan explicitly migrates/deletes it before closure. Closure means runtime and target model contain no ambiguity.

---

# U. Cross-service mandatory conformance tests

| ID | Cross-service proof | Class | Expected |
|---|---|---|---|
| U01 | Published definition/provision appears in AirAvail catalog projection | CORE | source-backed projection/cache works |
| U02 | Two suppliers of same broad service produce independently routable offers | CORE | marketplace identity preserved |
| U03 | FareFamilyId flows AirPrice -> AirAvail offer context -> Order snapshot -> post-order context | CORE | canonical ID end-to-end |
| U04 | Equivalent pre-order/post-order facts select same Provision | CORE | same evaluator result |
| U05 | Backoffice AirAvail simulation and production evaluation select same Provision | CORE | one evaluator |
| U06 | Post-order shopping executes Ordering -> AirAvail with no reverse dependency | CORE | one-way |
| U07 | Details returns the exact IDs Ordering snapshots | CORE | no business-key reconstruction |
| U08 | Hold accepts IDs/fulfillment facts only and performs no FX/repricing | CORE | trust boundary preserved |
| U09 | Retired supplier/definition excluded from new shopping while historical sold IDs remain serviceable | CORE | lifecycle/history |
| U10 | Paid-seat result contains only FlightFlow-available seats and Ancillary commercial pricing | CORE | ownership split |
| U11 | No generic supplier-side Issuance/Activation endpoint exists without a concrete supplier contract | NEGATIVE | absent |
| U12 | Ordering EMD issue links document/coupon to accepted OrderService | CORE | Ordering document authority |

---

# V. Milestone minimum acceptance map

This map prevents the coding agent from guessing which CORE scenarios must close at each checkpoint. It is a **minimum** map; a milestone may also satisfy later scenarios early, but that does not authorize starting the next milestone without audit.

| Milestone | Minimum scenario coverage before marker |
|---|---|
| M1 Walking Ancillary | `A01,A06,A10,A12,A14,A15,C01,C02,C04,C08,C09,C10,D01,D02,D05,I01,I02,I06,I07,I08,I09,J01,J03,J05,J06,J07,L01,L05,L06,M01-M06,N01-N06,N09,N11,O01-O03` plus cross-service tests U01/U07/U08 where applicable |
| M2 Marketplace + post-order | `A02-A05,A07-A09,A11,A13,A16,K01-K10,L02,N12-N14,O07` plus U02/U06/U09 |
| M3 Commercial + Backoffice | `B01-B09,C03,C05-C07,D03-D11,E01-E07,F01-F06,G01-G12,H01-H09,I03-I05,I10,J02,L03,L04,L07,R01-R10,S04,S05,S07` plus U03-U05 |
| M4 Baggage + quota + EMD | `N07,N10,O04-O08,P01-P06` plus baggage-specific CORE behaviors in Domain Master and U12 |
| M5 Paid seat | `Q01-Q10,S01-S03,S06,S08` plus U10 |
| M6 Closure | every remaining CORE scenario, every NEGATIVE assertion in scope, full Section T deletion audit, and all Section U cross-service tests |

Ranges such as `M01-M06` mean every listed ID in that inclusive group. FUTURE rows never become hidden implementation obligations.

---

# W. Final conformance verdict

The implementation is **not** complete merely when individual projects compile.

Closure marker is allowed only when:

```text
all CORE scenarios required by the implemented milestone pass
all negative/deletion assertions pass
cross-service contracts pass
no documented v9.1 invariant is implemented by a second competing mechanism
```

Final expected marker after Milestone 6:

`ANCILLARY_V91_IMPLEMENTED_READY_FOR_ARCHITECT_AUDIT`
