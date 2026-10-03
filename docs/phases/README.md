# Phase plan and state

**Last updated:** 2026-10-03

## Where we are

**Next action:** Ancillary Phase 2B — `P2B-Reservations/prompt.md`.
Then, in this order: AirOffer (`../handover/AirOffer/prompt.md`) → Ordering (`../handover/Ordering/prompt.md`) → the manual run of `../handover/End-to-End.md`.

## Ancillary phases

| Phase | Folder | What it delivers | Depends on | State | Commit |
|---|---|---|---|---|---|
| P1 | `P1-Extra-Baggage/` | Sub codes, products, price rules, the evaluator and quote; Extra Baggage (EMD-A) | — | **done** | `11e5941`, `467aa55` |
| P2 | `P2-Lounge-Access/` | Flight-scoped products; Lounge Access (EMD-S); combination rule | P1 | **done** | `9bbd385` |
| P2B | `P2B-Reservations/` | Reservations for every product (reserve, read, confirm, release, cancel); `LastUpdateTime` and the published read model | P2 | **built — awaiting Owner review** | — |
| P3 | — | Selling while shopping for flights. No Ancillary work: AirOffer and Ordering, each with its own document when it starts. | P2B, AirOffer, Ordering | not started | — |
| P4 | `P4-Stock/` | Counted stock (pet in cabin) on top of the reservations | P2B; FlightFlow's flight-cancelled message | blocked (`phase.md` §0) | — |
| P5 | `P5-Paid-Seat/` | Seat pricing | P4; AirOffer's seat input | blocked (`phase.md` §0) | — |
| P6 | `P6-Pricing-Depth/` | Fee lines, more conditions, sales context, document `None` | P2B | not started | — |

State values: `not started` · `next` · `in progress` · `built — awaiting Owner review` · `done` · `blocked`.

## Work in other services

| Work | Document | Depends on | State |
|---|---|---|---|
| AirOffer: read the catalogue, evaluate, service list and details | `../handover/AirOffer/spec.md` | P2B | not started |
| Ordering: list, add, reserve and confirm at Ancillary, EMD-A | `../handover/Ordering/spec.md` | P2B, AirOffer | not started |
| Manual end-to-end run | `../handover/End-to-End.md` | all of the above | not started |

## How a phase moves

1. **Open:** its `phase.md` and `proof.http` are complete, its `prompt.md` is written, the facts it relies on in other repositories are checked again. State → `next`.
2. **Build:** the agent builds, proves with `proof.http`, writes one test per expected-behaviour row, and sets the state to `built — awaiting Owner review`.
3. **Review:** the Owner reviews the report and the code. State → `done`, with the commit.
