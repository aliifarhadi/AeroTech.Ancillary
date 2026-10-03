# Phase 2B — Reservations and the published read model

**Authority:** `reference/Ancillary-Domain-Master.md` §2, §8.1–§8.3, §15. This document only says which part of it is built now and gives concrete examples.
**Precondition:** Phases 1 and 2 closed (`9bbd385`).
**Business outcome:** every ancillary sale is reserved and confirmed at Ancillary through one interface, whatever the product; and AirOffer can read the catalogue and prices directly from Ancillary's read model and know when it changed.

Everything Phases 1 and 2 built stays as it is. This phase only adds.

## 1. What is built

| Master section | Added in Phase 2B |
|---|---|
| §8.1 | aggregate `ServiceReservation` with its `ServiceReservationUnit`s |
| §8.2 | the behaviour rules, including the as-of-now expiry rule |
| §8.3 | Reserve, Read, Confirm, Release, Cancel |
| §9 | `ServiceReservationUnitStatus` (all five members) |
| §10, §11 | the rows marked P2B |
| §15 | column `LastUpdateTime` on the read models `AncillaryProducts` and `AncillaryPriceRules`, written on every projection; the tables and columns of §15 as a fixed contract |

Reserve evaluates its units with the **existing** quote evaluator in selection mode (Master §7.6). No second evaluator is written, and the quote operation does not change.

Not built: `StockPool`, any counter, the lapse-recording job, any message or outbox entry, any backoffice screen for reservations, any call to another service. A lapsed reservation is only *seen* as expired (rule 5 of §8.2); nothing is written when it lapses.

## 2. Operations

All under `Service/v1`, no authorization attribute (as `AncillaryQuotes`). Every successful operation answers HTTP 200 with the reservation body of Master §8.3 inside `data`.

| Operation | Route |
|---|---|
| Reserve | `POST ServiceReservations` |
| Read | `GET ServiceReservations/{reservationId}` |
| Confirm | `POST ServiceReservations/{reservationId}/Confirmations` |
| Release | `POST ServiceReservations/{reservationId}/Releases` |
| Cancel confirmed units | `POST ServiceReservations/{reservationId}/Cancellations` — body `{ "unitRefs": ["…"] }` |

Request and response examples: Master §8.3.

Shape validation (HTTP 400): `idempotencyKey`, `reference`, each `unitReference` present and 1–128 characters; `expiresAt` present; `context` valid as a quote request without `selections` (the Phase-1 validator rules); each unit valid as a selection; `unitRefs` a list of ids. Everything else is a business rule with the code of Master §8.3.

A Reserve with a new `idempotencyKey` and one with a known key both answer 200; the caller tells them apart only by content.

## 3. Published read model

- Add `LastUpdateTime` (`datetimeoffset`, not null) to the read models `AncillaryProducts` and `AncillaryPriceRules`. The migration fills existing rows with their `CreatedAt`.
- Every projection of a product or a price rule — define, change, activate, suspend, retire, revise, and the retirement of a previous version during activation — writes the clock's current time into `LastUpdateTime` of each row it writes.
- No table or column of Master §15 is renamed or removed.

## 4. Build order

1. Enum `ServiceReservationUnitStatus` (Master §9).
2. Aggregate `ServiceReservation` with `ServiceReservationUnit` (Master §8.1), folder `ServiceReservationAggregate` in every layer; unique index on `IdempotencyKey`.
3. Reserve: evaluate the units with the existing `AncillaryQuoteEvaluator` in selection mode, loading Active products, sub codes and rules as the quote service does. No second evaluator; the quote operation does not change. Then Read, Confirm, Release, Cancel (Master §8.2, §8.3).
4. `Service` controller `ServiceReservations`, no authorization attribute (as `AncillaryQuotes`). A reservation needs no read model unless a row below requires one; Read may load the aggregate.
5. `LastUpdateTime` on the read models `AncillaryProducts` and `AncillaryPriceRules`, written by their synchronizers on every projection (§3); migrations for the command and query contexts.
6. Run `phases/P2B-Reservations/proof.http`, then the proofs of P1 and P2.
7. Conformance tests: every row of §5, named `P2B_<rowId>_…`.

## 5. Expected behaviour

Data: Sub codes, Products X, G, L and Rules R, RG, RL of Phases 1 and 2 (airline 10), all Active. Context: one-flight bound `B1` with flight `F1` (`flightId 100`, airport 1 → 2), traveller `T1` (`ADT`, flies `F1`), currency 978, `asOf` = the clock. "Reserve X" = one unit: `XBAG1`, version 1, Rule R, `T1`, `B1`, quantity 1.

### Reserve

| # | Behaviour |
|---|---|
| V01 | Reserve X → one reservation, one unit `Held`: `ownerAirlineId 10`, `productRef XBAG1`, `productVersion 1`, `priceRuleId` = Rule R, `travellerRef T1`, `boundRef B1`, `flightRef null`, `coveredFlightIds ["100"]`, `quantity 1`, `currencyId 978`, `total 38.50`, `inventoryControl Unlimited`; `unitRef` is the unit's id. |
| V02 | The same request again → the same `reservationId` and `unitRef`; one reservation exists. Still the same answer after Rule R has been retired and replaced: a replay evaluates nothing. |
| V03 | The same `idempotencyKey` with another `quantity`, another `expiresAt`, another `reference`, or another set of units → `16405`. With only the `context` changed → the existing reservation. |
| V04 | A unit that fails Master §7.6 is refused with that check's code and nothing is stored: unknown `productRef` → `16302`; `productVersion 2` or another rule id → `16309`; a bound on which `T1` flies two flights → `16305`; Product X with quantity 2 → `16306`; `flightRef` given for a bag → `16303`; an unknown `travellerRef` → `16304`; a repeated traveller `ref` in the context → `16301`. |
| V05 | Two units, the second fails → the whole request is refused; no reservation and no unit is stored. |
| V06 | Two units naming the same occurrence → `16307`. |
| V07 | `context.existing` = X / `T1` / `B1` / 1, then Reserve X → `16305`. Product G: `existing` 1 and a unit of quantity 1 → reserved, `total 30.00`; a unit of quantity 2 → `16306`. |
| V08 | A lounge unit: `LNGTHR`, version 1, Rule RL, `T1`, `flightRef F1`, `boundRef null`, quantity 1 → `Held`, `flightRef F1`, `boundRef B1`, `coveredFlightIds ["100"]`, `total 20.00`. |
| V09 | One request with a bag unit and a lounge unit → one reservation with two units, in request order. |
| V10 | `expiresAt` equal to or before the clock; two units with the same `unitReference`; an empty `units` list → `16412`. A missing `idempotencyKey` or `reference` → HTTP 400. |
| V11 | Two simultaneous Reserve requests with the same `idempotencyKey` and content → one reservation exists, and both callers receive it. |

### Read, confirm, release, cancel

| # | Behaviour |
|---|---|
| V12 | Read → the reservation with current statuses. Unknown id → `16401` (HTTP 404), in every operation. |
| V13 | Confirm a held reservation → every unit `Confirmed`. Confirm again → success, no change. |
| V14 | Release a held reservation → every unit `Released`. Release again → success. Release a confirmed one → `16410`. Confirm a released one → `16407`. |
| V15 | After `ExpiresAt` has passed: Read shows `Expired`; Confirm → `16406`; Release → `16406`. Reading changes no stored data. A confirmed reservation does not expire. |
| V16 | Cancel a confirmed unit by its `unitRef` → `Cancelled`. Cancel again → success. Cancel a held unit → `16411`. A `unitRef` of another reservation, or an empty `unitRefs` → `16412`. |
| V17 | A reservation with two confirmed units: cancel one → it is `Cancelled`, the other stays `Confirmed`; a Confirm call afterwards → `16408`; cancelling the other → both `Cancelled`. |
| V18 | Confirm succeeds after the product has been revised or retired and after the rule has been retired; the unit keeps its `productVersion`, `priceRuleId` and `total`. |
| V19 | A Confirm and a Release sent at the same time → exactly one takes effect; all units of the reservation end in the same status. |
| V20 | No reservation operation writes an outbox message or calls another service. |

### Published read model

| # | Behaviour |
|---|---|
| M01 | The tables `ReadModel.AncillaryProducts`, `ReadModel.AncillaryPriceRules` and `ReadModel.PriceLines` have exactly the columns Master §15 lists (more are allowed, none missing). |
| M02 | Each product operation — define, change, activate, suspend, retire, revise — sets `LastUpdateTime` of the product row it writes to the clock. Activating version 2 sets it on both rows (version 2 and the retired version 1). |
| M03 | Each price-rule operation — define, change, activate, suspend, retire — sets `LastUpdateTime` of the rule row to the clock. |
| M04 | With the clock advanced before each step, the stamp (`MAX(LastUpdateTime)` and `COUNT(*)` over both tables) is different after every operation of M02 and M03. |
| M05 | List columns hold JSON arrays of numbers (`[1,5]`); a condition without restriction and the lounge column of a bag are SQL `NULL`. Enumerated columns hold the numbers of Master §9. |
| M06 | After the migration, rows that existed before have `LastUpdateTime = CreatedAt`. |

### Regression

| # | Behaviour |
|---|---|
| G01 | The whole Phase-1 and Phase-2 test suites pass without any change to their tests. |
| G02 | `phases/P1-Extra-Baggage/proof.http` and `phases/P2-Lounge-Access/proof.http` still pass. |

## 6. Proof

Run `phases/P2B-Reservations/proof.http` from top to bottom against the development database, with an airline that has no Ancillary data yet.
