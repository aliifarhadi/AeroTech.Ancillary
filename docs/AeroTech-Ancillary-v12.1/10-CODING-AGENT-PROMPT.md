# CODING AGENT — FINAL IMPLEMENTATION AUTHORITY
## AeroTech.Ancillary v12.1 — COMPLETE PHASE 1 + PHASE 2 (CORRECTION ONLY)

You are a **coding-only agent**. The Owner has confirmed D01-D07, D09, D12-D14, D04=Different Provision per POS, D11=Keep Current. The domain architect resolved D08 and D10 in this final approved completion spec without introducing generalized abstractions. **Implement exactly docs 00-09 and stop for Owner code audit.** No unauthorized redesign, renamed version, optional invented concepts, extra aggregate, work on external services, autonomous merge or Phase3 changes.

### 0. Verify repository baseline BEFORE changing a line
- `aliifarhadi/AeroTech.Ancillary` branch `feat/ancillary-v12.1-phase2-stock` at PUSHED SHA `1abf7a53e0efb9eb892097977327718c61486158`. `k8s-stg` baseline `933b7b7a793b426dbcb6362bbf8519886edf9080`.
- Check clean working tree, real SHA, current Entity/DTO names, migrations, source of Currency.DecimalPlaces, `BookingDefinition`, `AncillaryProvision`, `AncillaryPricing`, `AncillaryPricingLine`, `AncillaryInventoryPolicy`, typed Inventory roots, real provider reference ports, `reports/PHASE2-STOCK-CAPACITY-IMPLEMENTATION-REPORT.md` and actual dev schema. Preserve pushed Phase2 investment; do not reset or rebootstrap.
- Use one branch continuing Phase2; if workspace HEAD differs, assess drift and note exact differences before edits, never rewrite unrelated history. Do not commit/merge without Owner direction; all work must be visible as inspectable diff.
- **FROZEN**: `AncillaryReservationAggregate` source/DTO/routes/tests; all `AeroTech.FlightFlow`, `AeroTech.Ordering.Final`, `Aerotech.AirPrice`, `AirAvail`, `JetPay`, other repositories and their contracts. No cross-service calls. No Phase3 Hold, Confirm, Release, expiry, EMD, shopping engine, OrderChange or stock allocation.

### 1. Acceptance-first and baseline report
- Create `reports/V121-P1P2-FINAL-GAP-REPORT.md` with actual baseline and each required delta from docs 01-09; include current Phase2 smoke/test report but DON'T mistake Agent's old claim for rerun.
- Write failing Domain and Acceptance tests PR01..PR25, family tests `F01..F15_DEF/RULE/PRICE/POLICY`, core X01..X20. Fail first, record exact evidence; no edits to existing tests solely to hide failures.

### 2. Implement Pricing precisely
- KEEP `AncillaryPricing` root, Status/Version/ProvisionId/PricingUnit and one Active revision per Provision.
- Replace mixed `AncillaryPricingLine` with `AncillaryPricingRate` and 0..N `AncillaryPriceComponent`. Rate selector = CurrencyId + optional PTC / age band; each Rate has **BasePrice Money(Amount,CurrencyId)**; each component has Money and Category Tax/Fee, coded metadata and FeeApplicationUnit only on Fee.
- Decimal storage(19,6), enforce actual CurrencyReadModel.DecimalPlaces at publication/input; zero and three decimal tests. Money currency of components must match own Rate; no FX. Multiple independent currencies in same Active Pricing; no POS on Rate. For non-PerPassenger one selector **per currency**.
- Net Base + explicit additive tax and fee components; unit-applicable component amounts may contribute to UnitTotal. Fees per Ticket/OneWay/Sector/RoundTrip remain separate until scoped future quote; never blindly sum incompatible fee bases. Do not double-count gross-inclusive taxes. No percentage tax, agency commission, discount or fabricated conversion engine.
- Update Ancillary-owned Backoffice Commands, validators, EF, ReadModel/DTO/query, snapshots and tests, without inventing endpoints of other services. Active history immutable; Draft revision and atomic switch. Legacy flat price authoring removed only after safe mapping and one-to-one verified migration.

### 3. Fix Provision rules and minimum airline-specific descriptors
- PRESERVE ten typed Rule group entities + 27 typed rows and all existing authoring/publishing invariants; NO new generic DSL or AR.
- D04: POS-specific prices live in distinct Provisions, each with `ProvisionSalesRestrictionsRule.PointOfSale`; NO POS selector or market price layer under Rate.
- BookingDefinition adds `ConfirmationRequirement Immediate|SubjectToConfirmation`, while keeping BookingMethod, SSR/SSIM.
- Provision adds `PurchaseStage PreOrder|PostTicketed|Both` (plus `LegacyUnspecified` safe migration state only, not newly activatable). Validate stage and date context in publish authoring; do not touch Orders/Issue.
- Existing AdvancePurchase adds optional MaximumPeriod same TimeUnit with finite range guard; no unsupported calendar math.
- BaggageApplication adds `ChargeKind ExtraPiece|WeightPackage|Overweight|Oversize|SpecialEquipment` and optional `AllowanceConcept Piece|Weight` matching actual fare/route definition; preserve existing approved baggage fields. Do not infer baggage concept or quota from service subtype or SKU. For fixed weight 5/10/20 use **separate service definitions**, PerItem+Each + existing baggage `Weight` per package, not QuantityRule.StepQuantity. For genuine 1kg selectable weight sold by an airline, a separate `PerKilogram/Kilogram` definition with valid time-and-route Provision expresses it without silently permitting sub-kilo quantities.

### 4. Fix Phase 2 semantic defects WITHOUT adding capacity engines
- Keep Phase2 `AncillaryInventoryPolicy`, FlightCount/Weight/AirportSlot roots, their immutable adjustments, physical keys, SQL source concurrency, idempotency and read-model. No generic StockPool.
- **D10:** `MustCheckAvailability` on active Provision is not proof of finite local capacity; remove `EnsureUnlimitedIsCredible` ban for Unlimited+MustCheck. Policy configuration must make no booking/fulfillment guarantee; trace `requiresCheck` separately in admin view. Known supplier/seat external authority still Supplier/FlightFlow rather than Unlimited.
- D11: **KEEP CURRENT strict reference-evidence gating**: unresolved FlightOccurrence, Resource, Facility with timezone, FlightFlow delegation, CountingFamily **MUST NOT ACTIVATE** Local/FlightFlow/usage-specific policies. No invented local registry or stub returning verified; Draft admin and evidence-linked tests should work; `SourceUnavailable` remains explicit. Do not change another repo.
- Stable Policy identity `(OwnerAirlineId,ServiceDefinitionRef)` across ServiceDefinition Version revisions. `ServiceDefinitionId` cannot be authoritative if stale; resolve current identity and reconcile audited source pointer without cloning policy or capacity. Use existing repo patterns.
- Baggage/pets/meals/priority/SSR do NOT auto-create physical Count/Weight/Slot. Local only when real quota and its evidence exist. Shared keys are by physical resource, not by SKU/Provision. Room Night, DailyCount, AssignedAsset deferred per approved D12.
- SQL Server concurrency and half-open slot overlap must remain correct; no Phase3 reserved/available counters.

### 5. Database and Migration, only this service
- Expand/add typed price Rates, Components and descriptor columns to BOTH Command and Query DbContexts; create reversible schema and provenance mapping where meaningful. Use Money Amount DECIMAL(19,6), CurrencyId int per VO.
- Migrate old `AncillaryPricingLine` into Rates grouped by PTC+age and legacy parent CurrencyId; Base/Tax/Fee and `FeeApplicationUnit` preserved as detailed doc 03. Keep historical Pricing/Provision Status and Version; preserve legacy line ID provenance. Migrate currency scale without changing stored Amount or currency. If bad historical row, quarantine/report; don't silently round/normalize to publishable.
- D14: review existing `V121LegacySchemaCleanup` within **Ancillary only**, backup and verify historical rows. Do not invent cleanup tasks in AirAvail/FlightFlow or any other service. Do not delete data before reconciliation. Accurate Down/Up limitations documented.
- Real SQL Server integration: unique indexes, EF model parity, migrations on empty and realistic seeded/backup clone, command-read roundtrip and currency precision.

### 6. Required 15-family authoring scenarios
Execute every row 01..15 of doc 06 and their four field-level Fxx tests (`DEF/RULE/PRICE/POLICY`), plus worked examples A-E; test Paid/Free/NotAvailable, multi-currency, POS-specific Provisions, SSR request-only, seat delegated source, baggage by piece/weight package, valid supplier managed, real Local proof unavailable and no fake availability. Do not advertise a future shopping endpoint as created.

### 7. Closure, report and STOP
Run full clean solution build and ALL existing/new tests. Report exact commands, PASS/FAIL/skipped counts, new test cases by ID, schema source+target pre/post row reconciliation, live SQL concurrency evidence, read-model parity, updated API sample requests/responses, frozen Reservations SHA and git diff, no external repo changes, what remains deliberately unavailable under D11/D12/Phase3.

Produce `reports/V121-PHASE1-PHASE2-FINAL-COMPLETION-REPORT.md` with a **plain factual verdict**: Domain PASS/FAIL, local operational evidence BLOCKED/READY separately, Production limited-stock sell always NOT_A_PHASE2_GATE. A legitimate design success may coexist with `LocalActivation=BLOCKED_SOURCE_UNAVAILABLE`; DO NOT mark missing provider integration PASS.

If test/conformance fully satisfied, output `ANCILLARY_V12_1_PHASE1_PHASE2_DOMAIN_READY_FOR_OWNER_AUDIT` and STOP. Do NOT push, merge, change version, modify another service or start Phase3 without Owner authorization.
