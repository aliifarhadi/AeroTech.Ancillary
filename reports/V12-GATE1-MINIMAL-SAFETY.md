# V12.1 Gate 1 - Minimal Hold API Safety (F03)

Date: 2026-10-27
Authority: `CLAUDE.md`, then `docs/Pack-v12.1/AeroTech-Ancillary-Reservation-Pack-v12.1-GATE1-MINIMAL/11-AGENT-GATE1-SAFETY-AND-CONTRACT.md`
Result: **one guard added in `DELETE Hold/{holdId:long}`; isolated regression tests green; uncommitted.** Gate 2, 3 and 4 not started.

## 1. Source state

| Item | Value |
|---|---|
| Branch | `feat/ancillary-offers-v9` |
| HEAD | `e6a2012ee101546f5728db6a68844adcb6051204` (equals the Gate 0 pin; no new commit) |
| Dirty | 5 modified files, 1 new test fixture, plus the Owner's untracked v12.1 pack |

`git status --short`:

```
 M src/AeroTech.Ancillary.RestApi/V1/AncillaryReservationAggregate/Controllers/ServiceController.cs
 M tests/AeroTech.Ancillary.Application.AcceptanceTests/Boundary/P2BoundaryAcceptanceTests.cs
 M tests/AeroTech.Ancillary.Application.AcceptanceTests/Boundary/V12BoundaryAcceptanceTests.cs
 M tests/AeroTech.Ancillary.Application.AcceptanceTests/Holds/V10InventoryAllocationAcceptanceTests.cs
 M tests/AeroTech.Ancillary.Application.AcceptanceTests/Inventory/V121FinalCrossPhaseAcceptanceTests.cs
?? docs/AeroTech-Ancillary-Reservation-Pack-v12.1-GATE1-MINIMAL.zip
?? docs/Pack-v12.1/
?? tests/AeroTech.Ancillary.Application.AcceptanceTests/Fixtures/ReservationHttpHost.cs
```

## 2. Changed files

| File | Lines (+/-) | Change |
|---|---|---|
| `src/.../Controllers/ServiceController.cs` | +10 / -1 | Query-key guard in `Release` only |
| `tests/.../Holds/V10InventoryAllocationAcceptanceTests.cs` | +80 / -0 | Two HTTP tests and two read helpers (unit states, allocation rows) |
| `tests/.../Fixtures/ReservationHttpHost.cs` (new) | 68 | Isolated in-process HTTP host for the tests |
| `tests/.../Boundary/P2BoundaryAcceptanceTests.cs` | +1 / -1 | Freeze hash pin |
| `tests/.../Boundary/V12BoundaryAcceptanceTests.cs` | +1 / -1 | Freeze hash pin |
| `tests/.../Inventory/V121FinalCrossPhaseAcceptanceTests.cs` | +1 / -1 | Freeze hash pin |

`ServiceController.cs` is the only changed `src` file. Domain, Application, Query, Persistence, Providers, ServiceHost, Consumers and Contracts are unchanged. No new route, alias, parameter, error code, global filter, middleware or binder.

## 3. Rationale

- **The problem.** `[ApiController]` binds `long[]? unitIds` from the `unitIds` key only and ignores any other key. A typo such as `?unitId=` therefore leaves `unitIds` null, and null means "release every unit" in `ServiceReleaseAncillaryHoldCommand`.
- **Red proof without the guard** (2026-10-26 23:04 UTC). The guard was removed temporarily and the new test was run:

  ```
  Expected: Tuple ("unitId=1009", BadRequest)
  Actual:   Tuple ("unitId=1009", OK)
  ```

  The typo released the whole hold. The guard was then restored.
- **The guard.** Any query key other than `unitIds` returns 400 before `_mediator.Send`. The comparison is `OrdinalIgnoreCase`, the same as the binder. The response is `ValidationProblem(ModelState)`, the same ASP.NET ValidationProblemDetails that the binder already returns for `?unitIds=abc`.
- **Behaviour kept:**
  - No query key: release-all.
  - `unitIds=<positive ids>`: only those units are released.
  - Empty, non-numeric, zero or duplicate ids: still refused by the existing binder or validator.

```csharp
[HttpDelete("Hold/{holdId:long}")]
public async Task<IActionResult> Release(long holdId, [FromQuery] long[]? unitIds, CancellationToken cancellationToken)
{
    if (Request.Query.Keys.FirstOrDefault(key => !string.Equals(key, nameof(unitIds), StringComparison.OrdinalIgnoreCase)) is { } unknown)
    {
        ModelState.AddModelError(unknown, $"The query parameter '{unknown}' is not supported; use '{nameof(unitIds)}'.");

        return ValidationProblem(ModelState);
    }

    return Ok(await _mediator.Send(new ServiceReleaseAncillaryHoldCommand(holdId, unitIds is { Length: > 0 } ? unitIds : null), cancellationToken));
}
```

## 4. Isolation

| Concern | Evidence |
|---|---|
| Host | `ReservationHttpHost` runs in-process Kestrel on `http://127.0.0.1:0`, built like the existing `PublishedOpenApi` fixture. It registers `AddPresentation` (RestApi), `AddSurfaceAuthorization`, MediatR over Application and Query, validators, the Logging and Validation behaviours, and the four reservation handlers from `AncillaryScope`. |
| Hosted services | None. The only `AddHostedService` calls are `OutboxPublisher`, `MessageRetentionPoller` and `AncillaryHoldExpiryPoller` in `src/AeroTech.Ancillary.Consumers/DependencyInjection.cs:40-42`, and the fixture does not call that code. `Framework/AeroTech.Framework.Presentation` registers no hosted service. |
| Database | `TestDatabase` creates `DotAirAncillary_Tests_{guid}` (`Fixtures/TestDatabase.cs:25`). The dev database `DotAirAncillary` was not touched. |
| Live systems | No token used, no dev or prod call, no live run. The Owner's host on :4046 was not touched. |

## 5. HTTP cases

The hold has three Local FlightCount units: u0, u1 and u2. Base path: `Service/v1/Ancillaries/Hold/{holdId}`.

| Prompt §3 | Request | Status | Effect |
|---|---|---|---|
| 1 | `DELETE …/Hold/{id}` (no query) | 200 | All remaining units Released |
| 2 | `DELETE …?unitIds=u0` | 200 | Only u0 Released |
| 2 | `DELETE …?UNITIDS=u1` | 200 | Only u1 Released (case-insensitive) |
| 3 | `DELETE …?unitIds=` | 400 | No change |
| 3 | `DELETE …?unitIds=abc` | 400 | No change |
| 3 | `DELETE …?unitIds=0` | 400 | No change |
| 3 | `DELETE …?unitIds=u0&unitIds=u0` | 400 | No change |
| 4 | `DELETE …?unitId=u0` | 400 | No change (was 200 and release-all before the guard) |
| 4 | `DELETE …?unitIds[]=u0` | 400 | No change |
| 4 | `DELETE …?unitIds[0]=u0` | 400 | No change |
| 4 | `DELETE …?foo=1` | 400 | No change |
| 4 | `DELETE …?unitIds=u0&unitId=u1` | 400 | No change |
| 5 | `POST …/Cancellations` `{"reasonCode":"CUSTOMER_REQUEST","unitIds":[]}` | 400 | Refused |
| 5 | `GET …/Hold/{id}` | 200 | Read |
| 5 | `POST …/Confirmations` | 200 | Unit Confirmed; body has `"status":"Confirmed"` |
| 6 | `GET …/Service-Holds/{id}` | 404 | Old route is not an alias |
| 6 | `DELETE …/Service-Holds/{id}` | 404 | Old route is not an alias |

The tests that cover these cases:

- **Cases 1-4:** `V12_L01_L02_L03_L04_a_release_over_http_reads_only_unitIds_and_refuses_any_other_query_key_before_any_change`
- **Cases 5-6:** `V12_L05_get_confirm_and_cancel_answer_on_the_hold_route_an_empty_cancel_list_is_refused_and_the_old_route_is_gone`
- **Route set** (including `SeatMaps/Holds` and `SeatMaps/Resolve`): `V12_C02_C04_the_rest_api_exposes_exactly_the_authoring_routes_and_the_frozen_service_routes`
- **Typed Hold request and response shapes:** the `V11WireAcceptanceTests` cases

All of these tests passed in the run in §7.

## 6. Unit and ledger state, before and after (F03)

The columns are:

- **Units:** unit status ordered by `OrderServiceId` (Held = 1, Released = 3).
- **Ledger:** the `AncillaryReservationUnitAllocations` rows of the hold, as count|sum.
- **Usage:** the stock source usage.

| Point | Units | Ledger | Usage |
|---|---|---|---|
| Before the refused calls | `1\|1\|1` | `3\|3.000` | `3.000` |
| After the nine 400 calls | `1\|1\|1` | `3\|3.000` | `3.000` |
| After `?unitIds=u0` | `3\|1\|1` | not asserted | `2.000` |
| After `?UNITIDS=u1` | `3\|3\|1` | not asserted | `1.000` |
| After the call with no query | `3\|3\|3` | `3\|3.000` | `0.000` |

The test asserts that the before and after rows are equal. That proves the nine refused calls changed no unit, no allocation and no usage.

## 7. Commands and counts

All commands target `tests/AeroTech.Ancillary.Application.AcceptanceTests/AeroTech.Ancillary.Application.AcceptanceTests.csproj`.

| Step | Command | Result |
|---|---|---|
| Build | `dotnet build <csproj> -v q --nologo -nodeReuse:false` | 0 Error(s), 38 Warning(s) |
| Discover | `dotnet test <csproj> --no-build --list-tests --filter "FullyQualifiedName~.Holds.\|FullyQualifiedName~.Boundary."` | 99 discovered |
| Run | `dotnet test <csproj> --no-build --filter "<same filter>" --logger "console;verbosity=normal"` | Total tests: 99, Passed: 99. No Failed or Skipped line, no aborted line. |

- The run covered Holds (87) and Boundary (12), which is 99 executed out of 99 discovered.
- The two new tests also passed alone before that run (2/2).
- The full suite was not rebuilt: the prompt allows that only after a related failure or on the Owner's order. No full-green claim is made.

## 8. Confirmations

| Item | Evidence |
|---|---|
| No migration | `git diff HEAD --name-only -- '*Migrations*'` gives 0. No untracked migration: the only untracked path containing "migration" is the Owner's pack file `12-AGENT-GATE2-DOMAIN-AND-MIGRATION.md`. |
| No schema change | No Persistence file changed. The `src` diff is `ServiceController.cs` only. |
| No Ordering callback | The diff adds no Ordering, FlightFlow, hosted-service or connection-string reference (grep of the added lines gives 0). |
| No FlightFlow call | `V10_S10_S11_V11_L15_L17_L18_no_type_reachable_from_a_reservation_operation_can_reach_flightflow_or_…` passed in the run. |
| No new route or alias | `V12_C02_C04` passed; the old `Service-Holds` routes return 404. |

## 9. Freeze pins

The reservation source hash covers `ServiceController.cs`, so the guard changed it.

| | Files | Hash |
|---|---|---|
| Old pin | | `26AD20C7973936ACDA1CDF8FAEF9CE40D7AF7E5117E55569DE410CF144B931DC` |
| New pin | 133 | `BB7E5038C5CDE87014A85FEFC94801D779241782DD242F31875E4AD4EA9890B7` |

The new pin was written into the three pin test files. Recomputed on 2026-10-27, the hash gives the same value, and the pin tests passed in the run.

## 10. Remaining items (outside Gate 1)

| Item | State |
|---|---|
| I05 generic Hold race test | Gate 3 |
| Ordering end-to-end | NOT VERIFIED |
| Local stock on the dev database | GATED |
| F07 (Offers/Resolve options union), F09 (0 IRR test catalog) | Out of scope per the prompt |

## 11. Stop

Gate 1 ends here. Gates 2, 3 and 4 were not started. Nothing was committed, pushed or merged.
