# CODING AGENT — AeroTech Ancillary Phase 2 Inventory/Capacity — OWNER APPROVED

> **Project authority:** AeroTech Ancillary **v12.1 — Phase 2: Stock & Capacity**. Continuation of the approved Phase 1 v12.1; not a separately versioned specification. Phase 3 remains closed.


**This is an implementable instruction.** Authority: the approved **AeroTech Ancillary v12.1 master pack**, Phase 2 directory `PHASE2-STOCK-CAPACITY/`; approved owner decisions D1-D12, merged Phase1 source, documents 09-12. Do NOT reinterpret this pack into a generic stock engine. Phase3 is NOT authorized.

## 0. SOURCE AND BRANCH — DO FIRST

- Repo `https://github.com/aliifarhadi/AeroTech.Ancillary`, start from `k8s-stg` merge SHA **`933b7b7a793b426dbcb6362bbf8519886edf9080`**. Use a new branch named `feat/ancillary-v12.1-phase2-stock`. Verify `origin/k8s-stg` contains PR #1 merge and current working tree clean; if already ahead, compare diff before proceeding, preserve unrelated edits.
- Create isolated branch **`feat/ancillary-phase2-stock`** from merged target. If name exists, do not overwrite; inspect/reconcile and use appropriate new branch.
- Read checked-in `docs/AeroTech-Ancillary-v12.1/` and `reports/V12.1-Phase1-Implementation-Report.md`. Read `PHASE2-STOCK-CAPACITY/` documents 00-14, starting 08, 09, 10, 11 and 12. Compare actual names/types before editing. Do not copy v12 or v11 donor behavior in conflict with merged v12.1. This instruction is **Phase 2 of v12.1**, not a new version named 1.x, 12.2, or 13.
- Preserve framework (.NET DDD/MediatR/EF), domain exception conventions, existing REST style, query synchronizer, and separate SQL Server command and read models.
- Do not change any other repository (FlightFlow, Ordering, AirPrice, AirAvail, JetPay). Do not touch or reimplement `AncillaryReservationAggregate`, its persisted tables, existing Hold/Confirm/Get APIs or their tests. Do not change `AncillaryProvision` and `AncillaryPricing` semantics to force inventory support.

## 1. TARGET AND ARCHITECTURE — DO NOT OVER-ENGINEER

Build inventory **AUTHORING** and explicit policy-based ownership, distinct by physical behavior:
1. `AncillaryInventoryPolicy` AR — ProductIdentity `(OwnerAirlineId,ServiceDefinitionRef)`, commercial `ServiceDefinitionId` reference validated, authority Unlimited/Local/Supplier/FlightFlow. Typed `PassengerUsageLimit` children and a CLOSED set of consumption bindings. NO capacity counter on the policy.
2. `FlightCountInventory` AR — source per real `(OwnerAirlineId,FlightId,ResourceId)` and count unit; immutable `FlightCountAdjustment` child.
3. `FlightWeightInventory` AR — commercial kilograms per `(OwnerAirlineId,FlightId,WeightResourceId)`; immutable `FlightWeightAdjustment` child; decimal(18,3), NOT aircraft safety/load control.
4. `AirportSlotInventory` AR — occupancy per `(OwnerAirlineId, FacilityId, [StartUtc,EndUtc))` and typed `AirportId`; immutable `AirportSlotAdjustment` child; concurrency-safe **overlap** protection, not just exact duplicates.
5. Typed `CountPlusWeight` combination in Policy is **exactly one of each** and tied to same future flight occurrence; not arbitrary N-resource rules.
6. `DailyServiceInventory`, `RoomNightInventory` field schema and contract MUST be documented and scenario coverage classified. Implement actual Domain/SQL/API only if the local owned/guaranteed allotment source, ID provenance and tenancy are concretely verifiable in repo or accompanying owner evidence. In absence of evidence, mark `DEFERRED_PROVIDER_EVIDENCE`, reject Active local policy for them with explicit unsupported state, do not invent schemas for nonexistent reference data.
7. `AssignedAssetInventory` remains DEFERRED until proved physical assigned vehicle/equipment operations, even though its schema is documented.
8. SupplierManaged and FlightFlowManaged are delegated source-of-truth, no local stock counter. Unlimited has no counter. Missing/unknown policy is NOT Unlimited.

**Hard ban** on `AncillaryStockPool` (or renamed generic equivalent), `ScopeKind + ScopeReference + Value` EAV, JSON/CSV stock allocation lists, catch-all Repository, inheritance stock-base rules engine, 1 aggregate per SKU, fake supplier API, local airplane seat counter, and user-defined dynamic capacity expressions.

## 2. START WITH RED TESTS AND FIELD-BY-FIELD DESIGN

Before writing implementation, produce short `reports/PHASE2-INITIAL-CONFORMANCE-GAPS.md`: true current Phase1 public surface, V12.1 open issues, actual ID types, existing references, codes, migrations, authorization, and real provider/resource evidence availability. Compare to documents 03, 09-11. Write failing tests for: absent policy NotConfigured; two products sharing same physical count; 5kg package consumption binding; rejected duplicate active physical key; overlapping UTC facility slots even when intervals not identical; stale capacity expectedVersion; no capacity guarantee; frozen reservation files unchanged.

Agent may choose idiomatic class layout conforming to project framework, but must preserve the **semantic and field inventory** in 03+10; any deviation must be listed as `OwnerDecisionRequired` with reason and no silent fallback.

## 3. DOMAIN AND PERSISTENCE REQUIRED

Use explicit typed AR roots/entities/VOs specified in documents 03 and 10. Keep all owned child collection changes through parent methods. `Id long` generated with existing generator; numeric reference IDs checked for >0 and actual tenant/resource/provenance where authoritative source exists. Avoid assuming IDs are all `long`: AirportId is `int` in referenced code.

Policy:
- Publish constraints for all 4 authority modes and local patterns 1-4 exactly per document 11 activation matrix. Unlimited forbids finite bindings and provider counters. Local requires typed binding(s); Supplier/FlightFlow require validated delegated provider keys, no locally owned capacity.
- One current non-Retired Policy per owner+ServiceDefinitionRef, including Draft/Suspended (filtered unique index); Active immutable except lifecycle operations. Retire then create a successor row if policy authority truly changes. Reuse the same current policy for a new commercial version; never clone physical stock. Enforce allowed statuses/version and uniqueness in domain and SQL.
- PassengerUsageLimit is policy child only, not separate AR, no cross-order counter now. A traveller-specific entitlement must come from a separately verified issuer/source; don't fake internal named-person stock.
- No automatic assignment of a policy to the 31 legacy ServiceDefinitions. `AvailabilityDefinition.MustCheckAvailability` is NOT an inventory mode. Report conflicts; leave unassigned state NotConfigured.

Typed Count/Weight/Slot roots:
- `Status Draft|Active|Suspended|Retired`, `ClosedForSale`, stable physical key, configured total, Version/rowversion concurrency, actor/time/correlation audit, immutable adjustment per actual total change.
- No edit of active total with ordinary entity `Change`/`PUT`; only `AdjustToAbsolute(newTotal,expectedVersion,reason,actor,correlation)`. Validate nonnegative, precision, and overflow; keep previous/new values.
- Same expectedVersion concurrent commands: one winner (200/201) and loser domain 409. Correlation retries with same exact payload must not append a second adjustment; same key with different payload -> 409.
- Count and Weight physical sources may be shared across distinct products. Physical unique key excludes Product and Provision IDs; additional second active source on identical key is disallowed. No inconsistent CountUnit across one physical source.
- Slot intervals use half-open `[start,end)` in UTC, require Start < End. Two overlapping windows for same owner/facility must conflict **under SQL concurrency**, not only sequentially; adjacent windows allowed. Lock source row/facility key or SERIALIZABLE range predicate. Facility location+timezone must be validated using real source; avoid implicit server time zone and DST conversion guesses.
- Status, `ClosedForSale`, and capacity=0 are separate states. Changes do not retroactively mutate commercial pricing, customer orders or historical audit.
- No Held/Confirmed/Sold, no `Available=Total` trick, and no claims of oversell guarantee before Phase3. Snapshot is only configured/admin state `IsGuaranteed=false`.

EF migrations:
- Additive command + query contexts, typed tables, composite unique/filter indexes, typed FK, correct `decimal(18,3)`, concurrency token, timestamps, projections and migration history. No destructive v12.1 backfill/column drops. Validate migration on empty database and database cloned from v12.1 with active old catalog; generate clear delta evidence and Down/Up schema safety.
- Actual SQL Server integration required for concurrency unique-index, filtered key and interval overlap tests; unit tests or SQLite cannot stand in for SQL lock evidence.

## 4. APPLICATION, REST AND READ MODELS

Create Backoffice `v1` routes/commands/validators/results/list/detail/pagination/sync in document 11; align class naming with established conventions, do not invent generic admin endpoints. Reuse existing permission attributes and tenant checks. Actor and owner identity must come from **verified authorization/caller context**, not a spoofable request field without checking. Business 404 for missing, 409 for versions/duplicate/overlap, 422 for invalid domain inputs, 403 for cross-tenant where convention requires; never leak EF SQL raw exception.

Agent must show a complete HTTP sample for Unlimited, Local Count, Local Weight, Slot and Supplier/FlightFlow delegated policies. `GET InventoryConfigurationSnapshot` never expresses true availability or promise. External provider failure gives `Unknown`/`DelegatedCheckRequired`, not Unlimited, and no fallback stale number. Only introduce external-read ports if the source contract is real; a no-op fake client or placeholder success is forbidden.

## 5. NO STOCK RESERVATION IN THIS PHASE

Do not call/use/modify Hold/Confirm on `AncillaryReservation`, `FlightCapacity`, `Order`, or third-party suppliers. No payment, EMD, TTL expiration, decrement, operational allocation, user consumption ledger, or freeing stock on refund. Current M1 Hold remains as-is, so limited-stock product production Sell **must remain blocked** until Phase3. If a feature flag is already available and needed for non-inventory clients, wire only a safely closed configuration capability without editing frozen Reservation; otherwise document external rollout gate. Never tell owner overbooking is solved by Phase2.

## 6. ACCEPTANCE / STRESS / SOURCE PROOF

Implement scenario matrix 06 with 62 exact IDs. For each ID record `Executed PASS`, `Executed FAIL`, `DEFERRED_PHASE3`, `DEFERRED_PROVIDER_EVIDENCE`, or `DEFERRED_ASSET_EVIDENCE`; no fake success counts. Mandatory executable scenarios include all ownership, count, weight (except W06), slot authoring, concurrency, migration, authorization, no-local-counter, shared quotas, and snapshot truthfulness. Obtain tests required in 12 with real SQL Server for concurrent same-version adjustment and overlapping non-identical slots. If the reference-data subsystem cannot validate a facility / resource, STOP activation of the affected source and report exact missing port — but finish independent code and tests.

Run the FULL original v12.1 Domain and Application acceptance tests, plus all new Phase2 tests. Re-run EF `HasPendingModelChanges` for both command and query contexts. Test fresh empty schema, clone of historical v12.1 schema and real-data no-destructive migration. Compare command/read models. Negative tests: tenant mismatch, invalid weights, DST ambiguity, zero vs unlimited, duplication, race, failed projection, guessed legacy policy, supplier failure no invented availability.

## 7. STRICT DIFF GUARD / COMPLETION

No modifications to existing 36 frozen reservation files or their tests; `AncillaryReservationUnit.StockPoolId?` stays untouched. No new `Hold`, `Confirm`, `Release`, `Expire`, `ReservedQuantity` mutations. No edits to other repositories or framework. Any accidental modifications must be reverted unless owner explicitly approved, with diff evidence. Do not reopen Phase1 pricing/eligibility gaps here; isolate and disclose any blocker.

Save `reports/PHASE2-STOCK-CAPACITY-IMPLEMENTATION-REPORT.md` with the full 12 required sections in document 14. List every new AR/Entity/VO and fields/types/indexes; every migration; 62 scenario IDs; concrete test commands and counts; 31 legacy policy assignments or lack thereof; previous Phase1 unresolved cases; SQL test evidence; HTTP authorization proof; exact source/commit; boundary negative diff; decision register.

At completion: `PHASE2_READY_FOR_OWNER_AUDIT` only if all MANDATORY Phase2 gates passed and deferrals clearly documented; otherwise `PHASE2_BLOCKED` with actionable specific gap list. Never declare production limited-inventory sell ready. **STOP for Owner audit**. Do not implement Phase3, and do not commit/push/merge unless the Owner separately approved the action in this Coding Agent conversation.
