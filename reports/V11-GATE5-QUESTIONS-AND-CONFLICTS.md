# V11 Gate 5 — questions and conflicts

Date: 2026-10-27. Branch `feat/ancillary-offers-v9`, base `296197a`. Gates 1–4 and Hold contract R1 are uncommitted.
Authority: `docs/Pack-v11` 15, with 07 R01–R10 and §H, and the Owner's Q6.
Status: answered 2026-10-27. Q1 (a), Q2 (a) with the offer below, Q3 (a). The answers are merged into the pack (01 §L, 15); this file is history.

## 1. How it was done before (sibling runs in this repository)

| Need | Earlier proven path |
|---|---|
| Live host | my own side-built host on a separate port against the dev DB (Gate 4: :5291), never the Owner's :4046 |
| Catalog writes | Backoffice routes of that host with the Owner's Backoffice token (`authz_surface backoffice`, airline context); the Domain validates every product |
| A real seat-map selection id | `POST OtaPanel/v1/SeatMaps {offerId}` with the Owner's OtaPanel token and an AirAvail priced `offerId`; flight facts from the AirAvail offer detail; then `POST Service/v1/Ancillaries/SeatMaps/Holds` (scratch `live/seat_hold_live.py`, `flight_facts.py`) |
| Hold, Confirm, Release, Cancel, GET | the anonymous Service surface (S2S needs no auth) |

## 2. Dev catalog today

Read with `sqlcmd` on `DotAirAncillary`, 2026-10-27 00:50:

| Variant | Definitions | Holdable today |
|---|---|---|
| A01 | `BAG_EXTRA_32`, `BAG_EXTRA_23`: Active, Immediate, local supplier, Unlimited policy Active, 3 and 8 active provisions | yes |
| A15 | `ASSIST_WCHR`, `ASSIST_WCHS`, `ASSIST_WCHC`: all `SubjectToConfirmation` | no (Q6 asks for one with `Immediate`) |
| A20 | `LOUNGE_THR`, `LOUNGE_IKA`, `LOUNGE_IST`, `LOUNGE_DXB`: external suppliers, Supplier policy, `RequiresSpecificAppointment = false` | no (Q6 asks for one with a local supplier and Unlimited; pack 15 asks for an appointment, so it needs `RequiresSpecificAppointment = true`) |
| A07/A08 | 6 active seat definitions (no policy, as Seat has none) | through SeatMaps only |
| Reservations | 0 rows | — |

The local supplier is `1560040657475600384`. Staging AirInfo, FlightFlow, AirAvail, AirPrice and Identity answer from this machine.

## 3. Planned without question

1. Full proof (pack 15 item 1): clean build, the three suites with discovered = executed, `has-pending-model-changes`, and the idempotent migration SQL script checked.
2. Live run on my own host (:5291), using pack 15 item 2 order:
   1. create the Q6 catalog through Backoffice:
      - A15 `Immediate`, the WCHR rule of the existing `ASSIST_WCHR`, new reference;
      - A20 on the local supplier with `RequiresSpecificAppointment = true`, Unlimited policy, new reference;
   2. one mixed hold A01 (quantity 2) + A15 (`WCHR`) + A20 (appointment);
   3. SeatMap append;
   4. GET;
   5. release one unit;
   6. confirm;
   7. cancel one unit;
   8. replay;
   9. a second short hold left to expire through the real poller.
3. Wire-shape checks after an SQL reload and after expiry; prohibited members rejected (pack 15 item 3).
4. Audit (items 4–6): routes, providers, Ordering/FlightFlow paths and over-engineering checklist; Offers/Resolve versus Hold field names with the three documented id changes; supported/GATED matrix A01–A24 with their tests.
5. **R06:** Local stock stays GATED (no dev reference source). **R07:** Ordering end-to-end stays NOT VERIFIED.
6. Report `reports/V11-FINAL-CLOSURE.md`.

## 4. Questions

### Q1 — creating the Q6 catalog

- **(a) Recommended.** Send a Backoffice token; I create the two products through my host's Backoffice routes, so the Domain validates them.
- **(b)** You create them in the panel and send me their references.
(a) — the Owner sent a Backoffice token; it is kept outside the repository (scratch `live/`).
### Q2 — a priced offer for the seat append

The seat-map selection id must come from a real SeatMap answer.

- **(a) Recommended.** Send an AirAvail priced `offerId` for an economy flight inside the dev seat catalog's window (OtaPanel, like the earlier runs). Your OtaPanel token expires at 03:31 today (local time).
- **(b)** Skip the live seat append. It is then marked NOT VERIFIED live; the SQL acceptance test covers it.
(a) — the Owner's priced offer: `1|1|1;1558420526873968841~1469435200688619520_1550116065952924283~1469436768104218624;3|1551571415767973888||70|1792999957`
### Q3 — "GET after the Definition is retired" (pack 15 item 3, 07 §H)

- **(a) Recommended.** I create one more throwaway A15 (`Immediate`), hold it, retire its definition, and check that the GET is unchanged. The Q6 products stay active for later use.
- **(b)** Retire the Q6 A15 itself after the run.

(a) — the Owner's answer.
