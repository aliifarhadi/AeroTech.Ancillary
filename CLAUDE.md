# CLAUDE.md — AeroTech.Ancillary

AeroTech.Ancillary is the airline's ancillary product catalogue, the authority for ancillary prices, the place where every ancillary sale is reserved and confirmed, and (later) the keeper of limited ancillary stock. It creates no offers, no orders and no documents.

## 1. Where to look

| Need | File |
|---|---|
| How the documents are organised | `docs/README.md` |
| Where the project is and what is next | `docs/phases/README.md` |
| What Ancillary is — the authority | `docs/reference/Ancillary-Domain-Master.md` |
| What sellers see; how other services meet Ancillary | `docs/reference/Ancillary-Edge-Contract.md` |
| What one phase builds, its rows, its proof, its prompt | `docs/phases/<phase>/phase.md`, `proof.http`, `prompt.md` |
| Wire contract of the quote | `Contracts/ancillary-quotes-v1.openapi.yaml` |
| How code is laid out and how to work | this file |

`docs/handover/` describes what AirOffer and Ordering build. Nothing in it is built in this repository.

Nothing else is an authority: not earlier commits or documents, not other services' behaviour, not industry systems. If a needed fact is missing or two sources disagree, stop and ask the Owner. Never fill a gap with a plausible value.

## 2. Repository facts

- Built on the skeleton `843cf7c`. The code already in the repository is the reference example of everything in §3.
- Tests: `tests/AeroTech.Ancillary.Domain.ConformanceTests` (no database) and `tests/AeroTech.Ancillary.Application.AcceptanceTests` (real SQL Server; it also references `src/AeroTech.Ancillary.Query` and `Framework/AeroTech.Framework.Presentation`). Some tests read documents: the golden examples in `docs/phases/<phase>/phase.md`, requests in `docs/phases/<phase>/proof.http`, and the OpenAPI file. Their fixtures, fakes and naming are the pattern for every later phase.
- Local configuration (connection strings `CommandDbContext` / `QueryDbContext`, RabbitMq, Redis, Jwt, AirInfo, AeroCore) is not in the repository. If the service cannot start because a value is missing, report it; do not invent values. `.gitlab-ci.yml` is the Owner's concern; do not edit it.

## 3. How code is laid out

Stack: .NET 10, EF Core 10 (SQL Server), MediatR, FluentValidation, MassTransit/RabbitMQ, snowflake `long` ids. Do not add, upgrade or remove a package, and do not add or change a project reference.

**Folders.** One folder per aggregate in every layer: `ServiceSubCodeAggregate`, `AncillaryProductAggregate`, `AncillaryPriceRuleAggregate`, `ServiceReservationAggregate` (later `StockPoolAggregate`). The quote is not an aggregate; its folder is `AncillaryQuote`. Folder = namespace.

**One write use case** (surface = `Backoffice` or `Service`):

```
Domain/<Agg>Aggregate/<Agg>.cs                                  aggregate root: AggregateRoot<long>, private setters, static factory
Domain/<Agg>Aggregate/ValueObjects/*.cs                         derive from ValueObject
Domain/<Agg>Aggregate/Contracts/I<Agg>Repository.cs
Domain/<Agg>Aggregate/Contracts/I<Agg>QueryDbSynchronizer.cs, <Agg>ReadModelSnapshot.cs
Application/<Agg>Aggregate/Commands/<UseCase>/I<UseCase>Command.cs, I<UseCase>Service.cs, <UseCase>Service.cs,
                                              <UseCase>Validator.cs (abstract FluentValidation base), <UseCase>Result.cs
Application/<Agg>Aggregate/Commands/<UseCase>/<Surface>/<Surface><UseCase>Command.cs (IRequest<Result>),
                                              <Surface><UseCase>CommandHandler.cs (delegates to the service), <Surface><UseCase>CommandValidator.cs
Application/<Agg>Aggregate/Projection/<Agg>ReadModelSnapshotFactory.cs
Persistence/<Agg>Aggregate/<Agg>Configuration.cs, <Agg>Repository.cs
Query/<Agg>Aggregate/Models/<Agg>ReadModel.cs, Configurations/, Dto/, Queries/<Query>/<Surface>/...
Synchronizer/<Agg>Aggregate/<Agg>QueryDbSynchronizer.cs
RestApi/V1/<Agg>Aggregate/Controllers/<Surface>Controller.cs, Requests/*.cs
```

A use-case service: load → call the aggregate → project the read-model snapshot → `IUnitOfWork.SaveChangesAsync()` once → return a result record. Handlers contain no logic. A rule that spans aggregates (uniqueness, "retire the previous version", "sub code must be Active") lives in the use-case service inside that one save.

**Domain.** Ids from `IIdGenerator`, time from `IClock`. State changes only through aggregate methods. Aggregates reference each other by business key only. `RowVersion` on the root is the concurrency token; every "at most one" rule of the Master is also backed by a unique (filtered where needed) index. Nothing is hard-deleted. No domain event and no outbox message unless the Master names one.

**Enums.** `Contracts/AeroTech.Messages/Ancillary/Enums/`, namespace `AeroTech.Messages.Ancillary.Enums`, one file per enum, explicit numeric values exactly as Master §9, and only the members of the phases built so far. Every member carries `[Display(Name = "Title Case")]` in Ordering's one-line style (`[Display(Name = "Extra Baggage")] ExtraBaggage = 1`). The framework already writes and reads enums as names.

**Industry reference** (Master §4.5). An immutable in-code list in the Domain (`ServiceSubCodeAggregate`), containing exactly the reference rows of the phases built so far. No table, no endpoint, no configuration.

**Quote.** Evaluation is a pure domain service in `Domain/AncillaryQuote` (plain records and classes; not entities, not value objects). The quote query lives in the Query project (`Query/AncillaryQuote/Queries/GetAncillaryQuote/Service/`): it loads the Active products and the Active rules through the domain repositories, without tracking, and calls the evaluator. It does not load sub codes — a product version carries its copied codes. It writes nothing.

**API.** Route `{Surface}/v{version:apiVersion}/{Resource}`, `[ApiVersion("1.0")]`, `[Tags("{Surface}")]`, one controller per aggregate per surface. `Backoffice` controllers carry `[Authorize(SurfaceAuthorization.Backoffice)]`. The `Service` controller carries no authorization attribute, as in the sibling services (service-to-service calls need no token). Responses use `ApiResult<T>`; `long` values are written as strings (framework converter). Backoffice reads follow Ordering: every enum in a Backoffice `GET` or list response is `EnumValueDto { value, name, title }`; paginated lists are `PaginationQuery` → `GridData<TRow>` with `[Grid("Title")]` on the row DTO columns; `GET` of an unknown id is HTTP 404 with the business code. Command results and the `Service` quote keep plain enum names.

**Errors.** A malformed request is a FluentValidation failure (HTTP 400): validators check only presence, length, format and enum names. Every business rule — including numeric ranges such as min/max, amounts, priority and ids — throws through `ExceptionFactory`, with the code and HTTP status of Master §11 and its message in `ExceptionMessages`. Never `new BusinessException(...)` inline. Ancillary's codes live in 16000–16999. The loser of a race on a unique index or a `RowVersion` gets the framework default (HTTP 500); no extra mapping.

**Persistence.** Command schema `Ancillary`, read-model schema `ReadModel`. Money amounts use the context's existing decimal convention; do not change it. Phase 1 added the first migration of the command context (outbox, inbox and the new tables) and of the query context; every later migration is added on top and applied to the development database.

**Style.** No code comments unless asked. No hard-coded settings and no silent fallbacks for missing configuration. No speculative abstractions and nothing "for later".

## 4. How to work

- Build exactly the phase the Owner names, as its `prompt.md` says. Nothing of a later phase may exist in the code: no enum member, no field, no reference row, no table.
- **Two steps per phase**, both required; a phase is closed only after the second:
  1. *Build and prove* — production code (domain, application, persistence, query, API, migrations); prove it by starting the service and running the phase's `proof.http` from top to bottom against the development database.
  2. *Conformance tests* — every row of the phase's "Expected behaviour" is covered by at least one test named `<Phase>_<rowId>_…` (for example `P1_C11_…`):
     - domain tests (no database): aggregate invariants, lifecycle transitions, the industry reference, the evaluator;
     - application tests: the real use-case services against a real SQL Server database created for the test run and dropped afterwards, with a fixed clock and a deterministic id generator;
     - persistence tests wherever a behaviour depends on a unique index or on concurrency;
     - contract tests: golden examples and the OpenAPI file (the golden request produces exactly the documented response; field names and value names match `Contracts/ancillary-quotes-v1.openapi.yaml`).
  A phase's `prompt.md` says whether both steps run in one session.
- The `proof.http` files of earlier phases still pass unchanged, except for a planned adjustment named by a later phase.
- A test is never weakened, skipped or deleted to make it pass. A row that seems wrong is a question for the Owner. A defect found by a test is fixed in the code and listed in the report.
- Stay inside this service. Do not change `Framework/`, the ReferenceData module, or any contract folder other than `Ancillary/`.
- Do not create, copy or rename documents. The only change an agent makes in `docs/` is the state line its prompt names in `docs/phases/README.md`.
- Do not commit or push; leave the work for the Owner's review.
- Report in your final message, in the order the phase's prompt gives. Where a prompt is silent: what was built (by layer), the routes, the migrations, the actual status and body of every proof request, anything that could not be decided, deviations (there should be none); for tests, the table "row id → test name", the run result, production code changed (defect fixes only) and open questions.

## 5. Standing Owner decisions and working agreements

These were decided by the Owner during Phases 1 and 2. They hold until the Owner changes them.

**Follow Ordering exactly.** `AeroTech.Ordering.Final` is the pattern source for the framework and the implementation style. Never change the framework or the implementation pattern without the Owner's approval, even when a document is silent or says otherwise; when a document conflicts with the Ordering pattern, ask which wins. Before writing a class, find the class that already does the same job (here, else in Ordering) and copy its placement, layering, naming and structure. Never invent a helper layer, shared folder or code style Ordering does not have.

**Code shape.**
- Child rows of an aggregate are `Entity<long>` in `Entities/` (like Ordering's `PricingLine`), with ids from `IIdGenerator`; value objects derive from `ValueObject` with a private parameterless constructor; a value object is never shared by two owners (copy it — EF tracks owned types by reference).
- Every command has a validator; an id-only command validates `GreaterThan(0)`.
- Lengths are literal numbers in EF configurations and validators (`HasMaxLength(20)`, `MaximumLength(100)`); a named length in the Domain is a `private const` of the type that enforces it, never a public constant shared with other layers.
- Two use cases with the same body (Define/Change) each list their own members in their own command interface and have their own validator; nested input records get one validator class each in the first use case's folder, reused with `SetValidator(new XValidator())` (like Ordering's `OrderTravellerValidator`). No shared "content" interface and no abstract validator base across use cases.
- Every query lives in the Query project, never under `Application/.../Queries`. Mapping shared by `GetById` and `Paginated` is a `public static class <Agg>Mapper` in `Queries/Get<Agg>ById/` (like Ordering's `BackofficeOrderMapper`).
- Never compare two entities with `==` / `!=` in new code; use `ReferenceEquals` or compare ids.

**Domain decisions (Phase 1).** An industry-code attribute counts as present only when it is not null. `Baggage.Weight` with more than two fractional digits is `16106`, never rounded. A repeated entry in `traveller.flightRefs` is `16301`. Sub-group and description codes are two characters `A–Z0–9`; a commercial name is ASCII letters, digits and spaces, 1–30.

**Database and migrations.**
- The development database is `DotAirAncillary` on `localhost\SQLEXPRESS`. Live proofs run against it, through HTTP, with the airline and currency the proof's header names.
- Apply the migrations of all three contexts every time: `AncillaryDbContext` (Persistence), `AncillaryQueryDbContext` (Query) and `ReferenceDbContext` (ReferenceData module, history `dbo.__ReferenceDataMigrationHistory`). A rebuilt database must end with the ReferenceData tables too.
- The Owner often debugs `ServiceHost`, which locks its `bin/`. Never stop the Owner's session: build the host with `-o` into a side folder and run `ef.dll` (`migrations add` / `database update`) against that output. Stop only processes you started yourself, by their task id; never kill a process by name.

**Builds and tests.**
- Verify with a clean build: `dotnet build AeroTech.Ancillary.sln -v q --nologo --no-incremental` (or the side-folder host build while the Owner debugs); report errors and any new warning.
- A test run is green only when the executed total equals the discovered count (`--list-tests`) and the output has no "aborted" or "crashed" line. Never filter test output down to the summary lines.
- The acceptance tests read `ConnectionStrings:CommandDbContext` from `tests/AeroTech.Ancillary.Application.AcceptanceTests/appsettings.Test.json` or the environment variable `ConnectionStrings__CommandDbContext`, create `DotAirAncillary_Tests_<guid>` once per run and drop it at the end; each test class works with its own airline id.

**Safety and conduct.**
- Every claim in a report (build, test, database, file content, what a sibling does) is backed by a command run in the same session; otherwise say it was not verified.
- When a choice is genuinely open, ask the Owner before acting; do not prune, move, rename or skip anything that was not asked for.
- A Backoffice token is a secret: keep it out of every file in the repository and out of memory; pass it only at run time.
- Commit and push happen only when the Owner explicitly approves that specific commit or push.
- The Owner writes in Persian; answer in Persian, short: where the work stands and what needs the Owner.

## 6. Build orders of the closed phases (record)

**Phase 1 — Extra Baggage** (`docs/phases/P1-Extra-Baggage/`)

1. Delete the superseded document.
2. Enums (Master §9, members marked P1).
3. `ServiceSubCode` with the industry reference (`0CC` only) — Master §4.5.
4. `AncillaryProduct` — Master §4.
5. `AncillaryPriceRule` — Master §5.
6. Quote — Master §7 (P1 parts) and the OpenAPI file.
7. Migrations; start the service; run `docs/phases/P1-Extra-Baggage/proof.http`; report.

**Phase 2 — Lounge Access** (`docs/phases/P2-Lounge-Access/`)

1. Enums: the four Phase-2 members (Master §9).
2. Industry reference: the `0BX` entry (Master §4.5).
3. `AncillaryProduct`: the combination check of §3.4, run first; field `Lounge` and value object `LoungeDetail`; the `LoungeAccess` classification; request, read model, DTO and migration for `lounge`.
4. Quote: flight-scoped occurrences, lounge applicability, occurrence identity, item order, `lounge` on items — exactly as `Contracts/ancillary-quotes-v1.openapi.yaml` shows.
5. The Phase-1 test adjustment of `docs/phases/P2-Lounge-Access/phase.md` §5, and nothing else in Phase-1 tests.
6. Migrations; run `docs/phases/P2-Lounge-Access/proof.http` and `docs/phases/P1-Extra-Baggage/proof.http`.
7. Phase-2 conformance tests: every row of `docs/phases/P2-Lounge-Access/phase.md` §4, named `P2_<rowId>_…`.

Later phases carry their build order in their own `phase.md`.
