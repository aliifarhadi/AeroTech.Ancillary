# Phase 5 — Paid seat (pricing only)

**Authority:** `Ancillary-Domain-Master.md` (R5.3) §4.2 (`SeatDetail`), §4.5 (`0B5`), §7.8.
**Precondition:** Phase 4 closed.
**Business outcome:** the airline charges for chosen seats. FlightFlow keeps the seat map and the seat hold. Ancillary only answers what a given seat costs for a given traveller and which document it needs.

## 0. Before starting

| Needed | Who |
|---|---|
| AirOffer sends each candidate seat with its characteristic codes, taken from the seat map FlightFlow serves (`SeatMapItemDto.CharacteristicsCodes`). | AirOffer |
| Ordering allocates the seat price to its existing seat service and issues the EMD-A after FlightFlow confirms the seat. | Ordering |

The matching rule of Master §7.8 applies unless the Owner replaces it before this phase starts.

## 1. What is built

| Master section | Added in Phase 5 |
|---|---|
| §3 | `SeatMap`, and the `Seat` row of §3.4 (quantity 1–1) |
| §4 | field `Seat`, value object `SeatDetail`; the `Seat` classification rule (`Rfic A`, `GroupCode SA`); the industry reference entry `0B5` |
| §7 | request `seats[]`; `seatNumber` on a selection; item block `seat`; §7.8 |
| §9 | `Seat`, `SeatMap` |

No new operation, no message, no stock.

## 2. Expected behaviour

Product W: `Seat`, `SeatCharacteristicCodes [W]`. Product WE: `SeatCharacteristicCodes [W, E]`. Both on `0B5`, Active, each with an Active rule (W 10.00, WE 25.00). Traveller `T1` on flight `F1`.

| # | Behaviour |
|---|---|
| S01 | `seats` = seat `12A` with codes `[W]` → one item for `T1/F1`, product W, 10.00, `seat { number 12A }`, `inventory.control SeatMap`. |
| S02 | Seat `14A` with codes `[W, E]` → product WE (it has more codes), 25.00. |
| S03 | Seat `20C` with codes `[A]` → no item. |
| S04 | Catalogue mode without `seats` → no seat item; other products unaffected. |
| S05 | Selection of product W for `12A` → priced. For `14A` → `16305` (WE matches it). For a seat not in `seats` → `16304`. |
| S06 | Two seat selections for `T1` on `F1` → `16307`. |
| S07 | A `Seat` product without `seat`, with an empty code list, or on a sub code whose classification is not `Rfic A`, `GroupCode SA` → `16106`. With `inventoryControl` other than `SeatMap`, or a scope other than `TravellerSegment` → `16105`. |
| S08 | Everything of Phases 1, 2 and 4 behaves exactly as before. |

## 3. Not in this phase

Showing or storing a seat map, assigning seats, changing a seat after sale, free seats granted by a fare.
