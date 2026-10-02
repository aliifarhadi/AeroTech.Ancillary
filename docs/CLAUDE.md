# CLAUDE.md — AeroTech.Ancillary

AeroTech.Ancillary is the airline's ancillary product catalogue, the authority for ancillary prices and, later, the keeper of limited ancillary stock. It creates no offers, no orders and no documents.

## 1. What to follow

1. `docs/Ancillary-Domain-Master.md` (R5.3) — the domain and the boundaries: every field, rule, operation, error and what crosses to other services. It covers every phase.
2. `docs/phases/Phase-N-*.md` — what is built in the phase the Owner names, with payload examples and expected behaviour.
3. `contracts/ancillary-quotes-v1.openapi.yaml` — the wire shape of the quote.
4. This file — how the code is laid out in this repository.

`docs/Phase-1-End-to-End.md` and `docs/Ordering-P1-Ancillary-Implementation-Spec.md` describe what AirOffer and Ordering do. Nothing in them is built in this repository.

Nothing else is an authority: not earlier commits or documents of this repository, not other services' behaviour, not industry systems. If a needed fact is missing or two sources disagree, stop and ask the Owner. Never fill a gap with a plausible value.

## 2. Where the repository stands

The starting point is commit `843cf7c` ("Bootstrap AeroTech.Ancillary skeleton on the Ordering framework"), the head of `k8s-stg`. It contains:

| Present | Absent |
|---|---|
| `Framework/` (Core, Infrastructure, Presentation) — shared, do not modify | any aggregate, value object, enum of this service |
| `Contracts/AeroTech.Messages` — shared contracts; this service owns only the folder `Ancillary/` (to be created) | any controller except `PingController` |
| `src/` projects: Domain, Application, Persistence, Providers, Query, Synchronizer, Consumers, ReferenceData, RestApi, ServiceHost, already wired in `Program.cs` | any migration of the command or query context (the ReferenceData module has its own) |
| `AncillaryDbContext` (schema `Ancillary`, outbox and inbox tables), `AncillaryQueryDbContext` (schema `ReadModel`), `AncillaryUnitOfWork` (saves both contexts) | any test |
| `Domain/_Shared/Resources/ExceptionFactory.cs` and `ExceptionMessages.cs` with codes 16001–16004 | |
| `docs/AeroTech-Ancillary-Master-Domain-ADR-PRD-v1.1-FINAL.md` — superseded; **delete it first** | |

Local configuration (connection strings `CommandDbContext` / `QueryDbContext`, RabbitMq, Redis, Jwt, AirInfo, AeroCore) is not in the repository. If the service cannot start because a value is missing, report it; do not invent values. `.gitlab-ci.yml` is the Owner's concern; do not edit it.

The same framework and conventions are used by `AeroTech.Ordering.Final`. If the Owner gives its local path, its `DocumentStockAggregate` slice is the reference example of everything in §3. If not, §3 is sufficient.

## 3. How code is laid out

Stack: .NET 10, EF Core 10 (SQL Server), MediatR, FluentValidation, MassTransit/RabbitMQ, snowflake `long` ids. Do not add, upgrade or remove a package, and do not add or change a project reference.

**Folders.** One folder per aggregate in every layer: `ServiceSubCodeAggregate`, `AncillaryProductAggregate`, `AncillaryPriceRuleAggregate` (later `StockPoolAggregate`, `StockHoldAggregate`). The quote is not an aggregate; its folder is `AncillaryQuote`. Folder = namespace.

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

**Enums.** `Contracts/AeroTech.Messages/Ancillary/Enums/`, namespace `AeroTech.Messages.Ancillary.Enums`, one file per enum, explicit numeric values exactly as Master §9, and only the members of the phases built so far. The framework already writes and reads enums as names.

**Industry reference** (Master §4.5). An immutable in-code list in the Domain (`ServiceSubCodeAggregate`), containing exactly the reference rows of the phases built so far. No table, no endpoint, no configuration.

**Quote.** Evaluation is a pure domain service in `Domain/AncillaryQuote` (plain records and classes; not entities, not value objects). The application service loads the Active products, sub codes and rules through the repositories, without tracking, and calls it. It writes nothing.

**API.** Route `{Surface}/v{version:apiVersion}/{Resource}`, `[ApiVersion("1.0")]`, `[Tags("{Surface}")]`, one controller per aggregate per surface. `Backoffice` controllers carry `[Authorize(SurfaceAuthorization.Backoffice)]`. The `Service` controller carries no authorization attribute, as in the sibling services. Responses use `ApiResult<T>`; `long` values are written as strings (framework converter). Paginated queries use the framework's `PaginationQuery` / `PaginatedList<T>`.

**Errors.** A malformed request is a FluentValidation failure (HTTP 400). Every business rule throws through `ExceptionFactory`, with the code and HTTP status of Master §11 and its message in `ExceptionMessages`. Never `new BusinessException(...)` inline.

**Persistence.** Command schema `Ancillary`, read-model schema `ReadModel`. Money amounts use the context's existing decimal convention; do not change it. Phase 1 adds the first migration of the command context (outbox, inbox and the new tables) and of the query context, and applies both to the development database.

**Style.** No code comments unless asked. No hard-coded settings and no silent fallbacks for missing configuration. No speculative abstractions and nothing "for later".

## 4. How to work

Every phase is done in two steps. Both are required; a phase is closed only after step B.

**Step A — build and prove.**
- Build exactly the phase the Owner names. Nothing from a later phase may exist in the code: no enum member, no field, no reference row, no table.
- Write production code only: domain, application, persistence, query, API, migrations. No tests or test tooling in this step.
- Prove the slice by starting the service and running the phase's `docs/proof/*.http` file from top to bottom against the development database.
- Report (see below) and stop. The Owner reviews the domain, the operations and the cross-service fit before step B.

**Step B — conformance tests.** Started by the Owner after the review of step A.
- Every row of the phase document's "Expected behaviour" is covered by at least one automated test whose name starts with the phase and row id (for example `P1_C11_…`).
- Domain tests (`tests/AeroTech.Ancillary.Domain.ConformanceTests`, no database): aggregate invariants, lifecycle transitions, the industry reference, the quote evaluator.
- Application tests (`tests/AeroTech.Ancillary.Application.AcceptanceTests`): the real use-case services against a real SQL Server database created for the test run and dropped afterwards, with a fixed clock and a deterministic id generator.
- Persistence tests in the same project, wherever a behaviour depends on a unique index or on concurrency (for example two simultaneous activations, a duplicate sub code, a priority conflict).
- Contract tests for the quote: the golden example request produces exactly the documented response; field names and value names match `contracts/ancillary-quotes-v1.openapi.yaml`.
- The `.http` proof still passes unchanged.
- A test is never weakened, skipped or deleted to make it pass. A row that seems wrong is a question for the Owner. A defect found by a test is fixed in the code and listed in the report.

**Always.**
- Stay inside this service. Do not change `Framework/`, the ReferenceData module, or any contract folder other than `Ancillary/`.
- Do not commit or push; leave the work for the Owner's review.
- Do not create documents.
- Report in your final message. Step A: what was built (by layer), the routes, the migrations, the actual status and body of every proof request, anything you could not decide, deviations (there should be none). Step B: the table "row id → test name", the test run result, production code changed (defect fixes only), open questions.

## 5. Phase 1 build order

1. Delete the superseded document.
2. Enums (Master §9, members marked P1).
3. `ServiceSubCode` with the industry reference (`0CC` only) — Master §4.5.
4. `AncillaryProduct` — Master §4.
5. `AncillaryPriceRule` — Master §5.
6. Quote — Master §7 (P1 parts) and the OpenAPI file.
7. Migrations; start the service; run `docs/proof/phase-1.http`; report.

That is step A. Stop for the Owner's review (`docs/phases/Phase-1-Extra-Baggage.md` §5). Step B follows when the Owner starts it.
