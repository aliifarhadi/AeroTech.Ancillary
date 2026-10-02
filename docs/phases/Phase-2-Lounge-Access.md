# Phase 2 — Lounge Access

**Authority:** `Ancillary-Domain-Master.md` (R5.3).
**Precondition:** the Owner has closed Phase 1.
**Business outcome:** on an existing order, the airline sells lounge access per traveller per flight, at the airport the flight departs from, documented by an EMD-S.

Everything Phase 1 built stays as it is. This phase only adds.

## 1. What is built

| Master section | Added in Phase 2 |
|---|---|
| §3 | `TravellerSegment`, `EmdStandalone`, the `LoungeAccess` row of §3.4, and the combination rule (`16105`) |
| §4 | field `Lounge` and value object `LoungeDetail`; the `LoungeAccess` classification rule (`Rfic E`, `GroupCode LG`); the industry reference entry `0BX` |
| §7 | flight-scoped occurrences; lounge applicability; `lounge` on items |
| §9 | `LoungeAccess`, `TravellerSegment`, `EmdStandalone`, `Each` |

No new aggregate, operation or condition.

Product bodies gain `"lounge": { "airportIds": [1, 5] }` (null for a bag).

## 2. Expected behaviour

Data: Product X and Rule R of Phase 1 (airline 10). The industry sub code `0BX` (`LOUNGE ACCESS`, group `LG`, RFIC `E`, document `EmdStandalone`), whose reference entry becomes available in this phase (Master §4.5), enabled for the airline. Product L, naming that sub code: airline 10, `LNGTHR`, `LoungeAccess`, `TravellerSegment`, quantity `Each` 1–1, `EmdStandalone`, `Unlimited`, `lounge.airportIds = [1]`, Active. Rule RL: product L, priority 1, currency 978, one `Ancillary` line 20.00, Active.

| # | Behaviour |
|---|---|
| C01 | Define, activate and read Product L → `lounge.airportIds = [1]`, `baggage = null`. |
| C02 | `LoungeAccess` without `lounge`, with an empty or repeated airport list, or with `baggage` → `16106`. `ExtraBaggage` with `lounge` → `16106`. `LoungeAccess` naming a sub code registered with `rfic C` or `groupCode BG` → `16106`. |
| C03 | `LoungeAccess` with `TravellerBound`, `EmdAssociated` or `Piece` → `16105`. `ExtraBaggage` with `TravellerSegment`, `EmdStandalone` or `Each` → `16105`. |
| Q01 | One traveller; bound `B1` with the single flight `F1` (departs airport 1), bound `B2` with the single flight `F2` (departs airport 2); catalogue mode → for `B1`: the bag, then the lounge for `F1`; for `B2`: the bag only. In a two-flight bound a traveller on both flights gets the lounge item for the flight departing airport 1 and no bag. |
| Q02 | The lounge item: `salesScope TravellerSegment`, `flightRef F1`, `boundRef B1`, `coveredFlightRefs [F1]`, `unit Each`, `document.type EmdStandalone`, `lounge.airportIds [1]`, `baggage null`, `total 20.00`. |
| Q03 | Lounge selection with `flightRef F1` and no `boundRef` → priced. With `boundRef B1` → priced. With `boundRef` of another bound → `16303`. Without `flightRef` → `16303`. |
| Q04 | Lounge selection for `F2` → `16305`. |
| Q05 | On a flight-scoped occurrence, `OriginAirportIds` and `DestinationAirportIds` are compared with that flight's own airports. |
| Q06 | `existing` = lounge, `T1`, `F1`, quantity 1 → no lounge item for `T1/F1`; other flights and travellers unaffected. |
| Q07 | Everything of Phase 1 behaves exactly as before. |

## 3. Proof and review

As in Phase 1: a `docs/proof/phase-2.http` run, then the Owner's review, including a manual run in which Ordering sells a lounge access and issues an EMD-S that is not associated with a ticket coupon.
