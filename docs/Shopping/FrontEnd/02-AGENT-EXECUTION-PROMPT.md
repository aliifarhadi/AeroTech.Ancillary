# پرامپت اجرایی ایجنت — AeroTech Ancillary UI Grouping & Typed Options Batch v9

## هویت مأموریت

You are the **coding agent**. Implement only the approved *v9 UI Grouping + Batch Typed Options* delta. Do **not** redesign Ordering, Shopping Engine, SeatMap or previous v8 flows. Produce correct, maintainable, low-complexity .NET code and report evidence after each gate. **Source of truth**: `01-IMPLEMENTATION-MAP-AND-API-CONTRACT.md` in this package for *changed behavior*; current branch code for unchanged behavior. `04-ACCEPTANCE-MATRIX.md` supplies acceptance. Do not consult earlier Packs to invent behavior.

**Repository**: `https://github.com/aliifarhadi/AeroTech.Ancillary`  
**Approved branch baseline**: `feat/ancillary-offers-v8@324e32999bac6a3a9dfd69604dbe9e31272c43f0`  
**Execution branch**: work on a **new feature branch based on that SHA**. Do not modify `k8s-stg`; do not copy old deleted grouped branch. Report actual HEAD before proceeding.  
**Language**: report to Owner in Persian; source code style follow existing repository.

## Non-negotiable guardrails

- The goal is working UI flow: **one card per Variant → products → selection units**, and **one Batch Forms + one Batch Submit per Family** for many people/flights.
- Do not change Domain aggregate/invariant, provisions, catalog, pricing engine, stock, new HMAC/codec, Resolver, snapshot entity/table/migration, sales-scope policies, Retry/TTL rules, SeatMap, Hold/Reserve/Cancel/EMD, Ordering, legacy APIs outside target.
- No `Dictionary<string, object>` or dynamic `object` request inputs. Use existing `TFamilyOptions` record DTOs. No generic Form Builder, polymorphic JSON handler, orchestration layer, distributed transaction, queue, cache, second snapshot table, or new server config.
- Keep **three auth surfaces**, same authorization attributes. Keep the existing six **single-item GET/POST Options routes** and their HTTP semantics functional.
- Preserve Backoffice `EnumValueDto` shape rather than normalizing all three surfaces.
- **Do not implement** gated A09, A17, A19, TravelInsurance, new Variant or new `PerFlight` scope. Do not offer supplier/approval-dependent services accidentally.
- No reprice, auth, validation, Price/Tax/Quantity change to make tests pass. Use engine and proof already implemented.
- Do not merge or push to mainline; report commits/changed files/build/test results. No automatic progression through gates when prior gate has unresolved FAILs. Prefer fixed existing naming; add only minimal classes required.

## GATE 0 — baseline read-only audit (NO code changes)

1. Confirm repository and SHA, branch tree, clean worktree. If HEAD differs, **do not assume** file paths; diff and report. Owner approval required if a materially different code baseline.
2. Read actual files:
   - `src/AeroTech.Ancillary.Shopping/Offers/AncillaryOfferRow.cs`, `AncillaryOfferListing.cs`
   - `src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Snapshots/AncillaryOffersSnapshot.cs`
   - `src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Queries/GetAncillaryOffers/AncillaryOffersMapper.cs`, `Backoffice/BackofficeAncillaryOffersMapper.cs`
   - `src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Dto/{AncillaryOffersDto,AncillaryOfferDto,OfferCoverageDto,OfferUnitDto,SelectionOptionsDto,SelectionFieldDto}.cs`
   - `src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Queries/GetSelectionOptions/GetSelectionOptionsService.cs`
   - `src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Queries/VerifyTypedSelection/TypedSelectionVerifier.cs`
   - `src/AeroTech.Ancillary.Shopping/Options/{TypedSelectionService,TypedSelectionForm}.cs`
   - `src/AeroTech.Ancillary.Application/SelectionSnapshotAggregate/Commands/SubmitSelectionOptions/{SubmitSelectionOptionsService,ISubmitSelectionOptionsCommand,SelectionSnapshotResult}.cs`
   - `src/AeroTech.Ancillary.RestApi/V1/AncillaryOffers/Controllers/{BackofficeController,OtaPanelController,OtaController}.cs`
   - `src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Queries/ResolveGroupedSelections/ResolveGroupedSelectionsService.cs` (read only).
3. List the six supported typed families and their existing options request records, check casing and serializer configuration. Check the existing v8 tests and test fixtures.
4. Write a **one-page change map** mapping each v9 operation to the exact file(s). Show that no extra Domain/DB work is needed; list any true blocker rather than inventing it. Stop at Gate 0 until review if blocker.

## GATE 1 — Grouped Shopping Mapper (NO Engine change)

1. Add minimal DTOs in `Query/Shopping/AncillaryOffers/Dto/`:
   - `AncillaryVariantGroupDto<TDetails>(VariantCode,Title,Products)`
   - `AncillaryProductGroupDto<TDetails>(ProductKey,ServiceDefinitionRef,Title,Description?,Details,Units)`
   - `AncillaryUnitOfferDto(SelectionScope,Unit,Coverage,UnitPrice,QuantityPolicy,SelectionId?,QuantityOptions?,Reference?)`
   - Backoffice equivalents **only where required** to keep enum wrappers (`EnumValueDto` for `SelectionScope` and `QuantityPolicy.Unit`); avoid duplicating Details logic.
   - Use `JsonIgnore(WhenWritingNull)` for optional values as in current DTO.
2. Change the 10 family collection types of `AncillaryOffersDto` and `BackofficeAncillaryOffersDto` from flat `AncillaryOfferDto<T>` lists to variant group lists. Keep envelope names and `offerId/currencyId` unchanged.
3. Change `AncillaryOffersMapper` (Api+OtaPanel) and `BackofficeAncillaryOffersMapper`: Group by **Profile/family → `variantCode` → `(DefinitionVersionId, ProvisionId)`**. Use `AncillaryVariant.Find(variantCode).Name` for card heading; use Candidate.CommercialName at Product level. Make `ProductKey` the string `${DefinitionVersionId}:${ProvisionId}` (UI-only). Each original row maps to **exactly one** Unit; preserve ordering, money, `unit`, `coverage`, `selectionId`, `quantityOptions`, `reference`; never merge two units sharing keys, even if POS varies.
4. Move specialist `details` to Product. Build details via current mapping from row's Definition+Provision; no dynamic JSON. Remove redundant repeated `optionsEndpoint` from grouped Units; the 6 batch URL templates are a **fixed documented contract**. Keep single-item Options endpoints for compatibility.
5. Do NOT call `_engine.ShopAsync` more often than before; no source reading in the mapper.
6. Tests: three surface JSON snapshots, 2 variants one family, **same variant with two product definitions**, same definition with two provisions, same unit but different signed refs, a Variant with 2 travellers × 2 flights, a Portion spanning 2 flights, Order-scoped, quantity/direct/typed, empty/gated family, Backoffice `EnumValueDto`. For a concrete baseline fixture assert no lost/duplicated rows by flattening v9 Units and comparing to baseline v8 rows (ids, price, coverage, unit, quantity). Test name/record DTO compiles.
7. Report stage results and changed-file paths. No other scope.

## GATE 2 — Batch Forms (read-only)

1. Add route `POST /{Surface}/v1/AncillaryOffers/{Family}/Options/Forms` for existing six families and all three surfaces. This route may use `{family}` dynamic segment, because **no typed options body** is received. Route has Auth attribute from same Controller.
2. Request `BatchFormsRequest(OfferId:string, References:IReadOnlyList<string>)`, strict JSON unknown members; 1..50 references, no empty, no duplicates, max length 1024. Return `BatchFormsResponse(Forms: IReadOnlyList<{Reference,VariantCode,Fields}>)` where `Fields` uses **unchanged** SelectionOptionsDto fields.
3. In Query orchestration resolve trusted SalesScope **once**, load `FlightOfferShopping` with OfferId **once**, apply `PricedOfferScopeGuard.EnsureSellable` once; reuse context list for each signed reference. For each reference execute `TypedSelectionService.ReadReferenceAsync` and `FormAsync` with the **same contexts**. Do NOT loop over `GetSelectionOptionsService.ExecuteAsync`.
4. Preserve MAC/TTL/binding/surface/family/rate checks as implemented, no bypass, no new claims. Map each form with `SelectionOptionsMapper` existing; add original reference. If any invalid, return existing error status + `references[index]` detail, **no partial response**. No DB write.
5. Tests for N=1, 2, 50, empty, duplicate, 51, wrong family, changed Offer fingerprint, different POS, retired rate, bad signed ref, expired, different per-traveller choices, no DB writes. A spy/fake `IFlightOfferShoppingSource` verifies **exactly one LoadAsync per Batch** even when N=50. Existing single GET unchanged.
6. Report complete stage evidence.

## GATE 3 — Batch Submit (typed write, one SaveChanges)

1. For each of the **six existing POST-capable families** and **three existing Controllers**, add concrete Batch POST route under `/{Family}/Options/Batch`. Avoid one generic POST with `object` or family discriminator in body. All required request field names and DTOs are in v9 `01` §5.3.
2. Reuse existing per-family options types; introduce minimal generic strongly-typed `SubmitSelectionOptionsBatchRequest<TFamilyOptions>` with `offerId` + `selections:[{reference,options:TFamilyOptions}]`. Preserve strict JSON unmapped-member rejection for envelope and options; validators 1..50 / no dup / family specific.
3. Add `VerifyBatchAsync` on **existing** `ITypedSelectionVerifier` (or equivalent existing port extended minimally) returning verified claims/proofs **in input order**. Query implementation resolves Surface Scope and Offer once, then loops over existing `ReadReferenceAsync` and `ProveAsync` against same contexts; no new validation rules and no `VerifyAsync` N times. Single-item verification should reuse a helper without changing its semantics.
4. Add one Application batch service (or a minimal method added to existing command service) to (i) validate all line structures, (ii) await complete verification of **all** inputs, (iii) create each `SelectionSnapshot` with the *same existing ID generator, JSON serializer, TTL, and repository*, (iv) `AddAsync` all and call **one** `IUnitOfWork.SaveChangesAsync` (one relational transaction). Do not create snapshots before all proofs pass.
5. Return `{selections:[{reference,selectionId,expiresAtUtc}]}` in request order. A repeated successful batch is allowed to return new s_ IDs (existing non-idempotent semantics); no token dedup table. Error: no partial write, error index in field-path, keep existing HTTP exception/error codes; never emit partial successes.
6. Tests include: 2 valid mixed allowed Variant in SAME family, 2 different traveller/flight references, 1 invalid in middle → **0 newly committed rows**, 51/duplicate/missing, same Offer but wrong sales context, wrong family, expired, non-active pricing, wrong choices, cross-user binding, two successful retry batches, final `ResolveGrouped {ids:[s1,s2]}` returns two selections, not one; `SaveChangesAsync` count=1 and Offer Load count=1 for N=2 and N=50. Single POST still works and is tested.
7. Make sure a context read or rate lookup may happen more than once within the same app if current code does; optimizing SQL N+1 is *not* a reason to invent a new cache or engine in this change. Optimize only if measured or required to pass bounded performance tests.

## GATE 4 — End-to-end and Closure

1. Run full build and existing test suites. At a minimum: `dotnet build AeroTech.Ancillary.sln --no-incremental`, `dotnet test tests/AeroTech.Ancillary.Shopping.Tests`, `dotnet test tests/AeroTech.Ancillary.Domain.ConformanceTests`, `dotnet test tests/AeroTech.Ancillary.Application.AcceptanceTests` (adapt paths only if solution changes; report exact commands).
2. Run *realistic* end-to-end: Shopping `A15/A16` if enabled: 2 traveller refs -> one Variant card -> one Product or more -> 2 Units with distinct `r_` -> **one Forms request** -> **one Batch Submit** -> two `s_` -> **unchanged ResolveGrouped** with those IDs. Separate Quantity A01: pick from `quantityOptions` and Resolve Direct `d_` **without Snapshot**. If dev data unavailable, fixture test with honest limitation.
3. Test 3 surfaces, same Auth restrictions, special Backoffice enum serialization. Report HTTP request/response minimal redacted, statuses and counts, DB rows and SQL transaction evidence.
4. Protect scope: `git diff --name-status`, ensure **zero changes** under SeatMap, Reservation, Domain, Pricing, Migrations, ResolveGrouped, Ordering, other providers. If an exception is truly unavoidable, STOP and report; do not self-authorize change.
5. Produce `reports/V9-UI-BATCH-CLOSURE.md` with: baseline/HEAD, exact routes and DTOs, tests pass/fail/gated, N+1 Offer Load proof, table insert counts, no partial commit proof, file-level diff, known blockers, compatibility of existing Single routes, Decision Evidence. Do not claim PASS without executing.
6. Before marking closed, execute `04-ACCEPTANCE-MATRIX.md` in this package. Do not treat earlier v8 acceptance matrix as evidence for the new grouping or Batch.

## Expected minimum deliverables

- One coherent Git branch with minimal changed files and tests.
- Three grouped Shopping Surfaces; 1 Batch Forms route per Surface (parameterized family); six typed Batch POST routes per Surface.
- Single existing Options still available. `ResolveGrouped`, SeatMap and Domain unchanged.
- A Closure Report with reproducible commands and representative JSON.

## Stop conditions

Stop and request owner decision only if: (a) HEAD materially diverged from recorded SHA, (b) scope would require changes to SeatMap, Domain, Pricing, Snapshot Schema or ResolveGrouped, (c) a valid scenario cannot be represented using existing six typed options DTOs, or (d) accepting this design would delete an existing business distinction between products. Do **not** stop for generic or self-evident questions about endpoint versioning, OfferId at Resolve, POS inside Resolve, form-builder libraries or other closed decisions.
