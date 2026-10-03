# Ancillary — documentation

Start here every time. This page says how the documents are organised; `phases/README.md` says where the project is and what comes next.

## Layout

```text
docs/
  README.md                         this page
  reference/                        THE AUTHORITY — always current, updated in place, never copied
    Ancillary-Domain-Master.md        domain, boundaries, every field, rule, operation, error
    Ancillary-Edge-Contract.md        what a seller sees; mapping to NDC / ONE Order / Amadeus / Sabre
    CHANGELOG.md                      every change to the two documents above, newest first
  phases/                           ONE FOLDER PER PHASE
    README.md                         the phase plan and the state of every phase  ← where we are
    P1-Extra-Baggage/                 phase.md · proof.http · prompt.md
    P2-Lounge-Access/                 phase.md · proof.http · prompt.md
    P2B-Reservations/                 phase.md · proof.http · prompt.md
    P4-Stock/                         phase.md · prompt.md
    P5-Paid-Seat/                     phase.md · prompt.md
    P6-Pricing-Depth/                 phase.md · prompt.md
  handover/                         WORK OF OTHER SERVICES — copied into their repositories
    README.md                         what to copy where, and the state of each service's work
    AirOffer/                         spec.md · prompt.md
    Ordering/                         spec.md · prompt.md
    End-to-End.md                     the manual run across the three services
```

Outside `docs/`: `CLAUDE.md` (how an agent works in this repository) and `Contracts/ancillary-quotes-v1.openapi.yaml` (the wire contract of the quote; tests read it there).

## What each kind of file is

| File | Role | Changes when |
|---|---|---|
| `reference/*` | The single authority for what Ancillary is and how it meets other services. | A decision changes. The change is made in place and recorded in `reference/CHANGELOG.md`. |
| `phases/<phase>/phase.md` | Which part of the reference is built in that phase, with examples, build order and the expected-behaviour rows that become tests. | Before the phase starts. After the phase is closed it is not changed, except for a planned adjustment named by a later phase. |
| `phases/<phase>/proof.http` | Requests that prove the running slice. | With its `phase.md`. |
| `phases/<phase>/prompt.md` | The exact prompt for the agent. For a closed phase, the record of what was used. | Written when the phase opens. |
| `phases/README.md` | The plan and the state. | Every time a phase starts, is reviewed or is closed. |
| `handover/*` | What AirOffer and Ordering build, with their prompts. | When their part changes. |

## Rules

1. If a phase document and the reference disagree, the reference wins and the disagreement is a question for the Owner.
2. Paths inside documents are relative to `docs/`. In another repository the same files sit under `docs/ancillary/` with the same relative paths.
3. No second copy of a document, no zip, no versioned file name. History is in git and in `reference/CHANGELOG.md`.
