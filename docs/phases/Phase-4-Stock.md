# Phase 4 — Stock for a limited product

**Authority:** `Ancillary-Domain-Master.md` (R5.4) §8.
**Precondition:** Phase 2 closed. (Phase 3 has no Ancillary work.)
**Business outcome:** a product can have a limited number of units per flight. The quote shows what is left; Ordering holds, confirms, releases and cancels units.

## 0. Before starting

| Needed | Who |
|---|---|
| The first limited product is `PetInCabin` on the industry sub code `0BT` (Master §3.4, §4.5). The Owner may name another product instead; then only the row of §3.4, the classification row and the reference entry change. | Owner |
| FlightFlow states a flight's cancellation in a message. Checked at `d2180b2`: on cancellation it publishes `FlightUpdated` carrying only the flight id and version; `Status` is not filled. FlightFlow fills it, or publishes a dedicated message, before this phase starts. Cancellation is never inferred from anything else. | FlightFlow |
| Ordering's hold calls match Master §8.4 exactly. | Ordering |

## 1. What is built

| Master section | Added in Phase 4 |
|---|---|
| §3 | `Quota`, and the `PetInCabin` row of §3.4 |
| §4 | field `DefaultQuotaPerFlight` |
| §7 | applicability rule 5, selection check 8 (`16308`), `inventory` for `Quota` |
| §8 | all of it |
| §9 | `Quota`, `StockPoolStatus`, `StockHoldUnitStatus`, `PetInCabin` |
| §10, §11 | the rows marked P4 |

Product rule added: `InventoryControl = Quota` requires `DefaultQuotaPerFlight ≥ 0`; any other inventory control requires it empty (`16106`).

## 2. Expected behaviour

Product Q: `PetInCabin` on `0BT`, airline 10, `TravellerSegment`, `EmdAssociated`, `Quota`, `DefaultQuotaPerFlight = 2`, quantity `Each` 1–1, Active, with an Active rule. Flight 100.

| # | Behaviour |
|---|---|
| S01 | Hold 1 unit → created, unit `Held`; the pool now exists with capacity 2, held 1, available 1. |
| S02 | The same request again → the same hold; the pool is unchanged. |
| S03 | The same idempotency key with another quantity → `16405`; the pool is unchanged. |
| S04 | Two holds of 1, then a third → `16403`; held stays 2. |
| S05 | One request with units on flight 100 (stock left) and flight 101 (none left) → `16403`; nothing is held on flight 100. |
| S06 | Confirm a held hold → held 0, confirmed 1. Confirm again → success, no change. |
| S07 | Release a held hold → held 0. Release again → success. Release a confirmed hold → `16410`. |
| S08 | After `ExpiresAt` has passed, before anything recorded it: reading shows `Expired`; confirm → `16406`; release → `16406`. |
| S09 | The periodic job after expiry: unit recorded `Expired`, held reduced, exactly one `AncillaryStockHoldExpiredV1`; a second run adds nothing. |
| S10 | Pool of capacity 1 with one lapsed, not yet recorded hold: a new hold succeeds; the old hold is recorded `Expired` with its message at the same moment; a later job run adds nothing. |
| S11 | Cancel a confirmed unit → confirmed reduced. Cancel again → success. Cancel a held unit → `16411`. |
| S12 | A hold of two units, both confirmed, one cancelled → that unit `Cancelled`, the other `Confirmed`; a confirm call afterwards → `16408`. |
| S13 | Twenty simultaneous holds of 1 on a pool of capacity 2 → exactly two succeed. |
| S14 | Capacity set to 5 → available grows. Capacity below held + confirmed → `16403`. |
| S15 | Flight 100 is cancelled → its pool is `Closed`; a new hold → `16404`; the existing hold can still be confirmed, released and cancelled. |
| S16 | A hold for a product that is `Unlimited`, suspended or unknown → `16402`. |
| Q01 | Quote before any hold → `inventory` = `control Quota`, `remaining 2`. |
| Q02 | After two confirmed units → no item in catalogue mode; a selection → `16308`. |
| Q03 | Closed pool → no item. |
| Q04 | A quote still changes nothing. |
| Q05 | Everything of Phases 1 and 2 behaves exactly as before. |

## 3. Not in this phase

Splitting or extending a hold, waiting lists, overbooking, pools keyed by anything other than a flight, automatic cancellation of holds, any call to Ordering or FlightFlow.

## 4. Proof and review

A `docs/proof/phase-4.http` run, then a manual run with Ordering: sell the limited product, hold, confirm, issue; let another hold lapse and see Ordering receive the message.
