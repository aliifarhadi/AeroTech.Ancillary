# V11 — one live hold of every ancillary variant

Date: 2026-10-27. Branch `feat/ancillary-offers-v9`, base `296197a` plus the uncommitted v11 work. Nothing is committed or pushed.
Owner's order: define every missing product, hold one of each variant over HTTP, then cancel some, confirm some and let some expire.

## 1. How it was run

- My own side-built host (scratch `v11/g4host`, Development) ran on `http://localhost:5291` against the dev DB `DotAirAncillary`.
  - The Owner's host on :4046 (PID 32896) was running and was not touched.
  - My host was stopped afterwards, port 5291 was confirmed free, and its log has 0 error lines.
- Products were created through the Backoffice routes with the Owner's token, so the Domain validated them.
- Holds and the lifecycle went through the anonymous Service surface.
- Seat ids come from `POST OtaPanel/v1/SeatMaps` on the Owner's priced offer.
- Scripts and raw evidence (scratch `v11/`):

| Purpose | Script | Output |
|---|---|---|
| Inspect the source products | `g6_inspect.py` | `g6_sources.json` |
| Define the missing products | `g6_define.py` | `g6_created.json` |
| Read the seat map | — | `g6_seat_source.json` |
| Run the holds | `g6_live.py` | `g6_live.json` — 23/24 checks pass; the one miss is in §4 |

## 2. Products defined (missing as holdable on dev)

Each is a copy of an existing dev product with only the fields needed to make it holdable: a local supplier (`1560040657475600384`), Immediate confirmation and an Unlimited policy (Active).

| Variant | New reference | Copied from | Changed |
|---|---|---|---|
| A04 | `BAG_OVERSIZE_IMM` | `BAG_OVERSIZE` | Immediate (source needs confirmation); price 4,000,000 IRR as the source |
| A06 | `BAG_BIKE_IMM` | `BAG_BIKE` | Immediate; price 3,000,000 IRR as the source |
| A10 | `UPG_ECO_TO_BUS_FIXED` | `UPG_ECONOMY_TO_BUSINESS` | local supplier, `FixedAncillary` (source is `DynamicQuote`), provision Filed/Paid; **price 0 IRR** (the source has no price) |
| A12 | `MEAL_SAFFRON_CHICKEN_LOCAL` | `MEAL_SAFFRON_CHICKEN` | local supplier, provision Filed/Paid; **price 0 IRR** (the source has no price) |
| A21 | `FAST_TRACK_IKA_LOCAL` | `FAST_TRACK_IKA` | local supplier (source external); price as the source (0 IRR) |
| A22 | `CIP_IKA_LOCAL` | `CIP_IKA` | local supplier, Immediate; price as the source (0 IRR) |
| A24 | `WIFI_FULL_FLIGHT_LOCAL` | `WIFI_FULL_FLIGHT` | local supplier; price as the source (0 IRR) |

- Already holdable on dev and used as they are: A01, A02, A03, A05, A11, A16, A23.
- From Gate 5: A15 `ASSIST_WCHR_IMM` and A20 `LOUNGE_THR_LOCAL`.
- Seats: A07 and A08.
- The two 0 IRR prices (A10, A12) are placeholders, because their sources have no price. They need the Owner's real price if these products are kept.

## 3. Holds and their end state

### Hold `1564400860698509312` — confirmed (order `17930526171`)

| Variant | Product | Choice sent | End |
|---|---|---|---|
| A01 | `BAG_EXTRA_23` | quantity 2 | Confirmed |
| A03 | `BAG_OVERWEIGHT` | `bagRef B-32`, 27.5 kg | Confirmed |
| A05 | `BAG_CABIN_EXTRA` | — | Confirmed |
| A11 | `MEAL_VGML` | — | Confirmed |
| A15 | `ASSIST_WCHR_IMM` | SSR `WCHR` | Confirmed |
| A20 | `LOUNGE_THR_LOCAL` | appointment 2026-10-28 06:00 +03:30, 1 guest | Confirmed |
| A23 | `PRIORITY_BOARDING` | `optIn true` | Confirmed |
| A07 | `ECO_SEAT_STANDARD` | seat 20C on IKA→IST 1124 | Confirmed |

### Hold `1564400889660178432` — confirmed, then cancelled (order `17930526172`)

| Variant | Product | Choice sent | End |
|---|---|---|---|
| A02 | `BAG_WEIGHT_10` | quantity 1 | Cancelled `CUSTOMER_REQUEST` |
| A04 | `BAG_OVERSIZE_IMM` | 150×60×40 cm | Cancelled `CUSTOMER_REQUEST` |
| A06 | `BAG_BIKE_IMM` | 180×70×35 cm, 18 kg | Cancelled `SCHEDULE_CHANGE` |
| A10 | `UPG_ECO_TO_BUS_FIXED` | — | Cancelled `SCHEDULE_CHANGE` |
| A12 | `MEAL_SAFFRON_CHICKEN_LOCAL` | quantity 1 | Cancelled `SCHEDULE_CHANGE` |
| A16 | `ASSIST_BLND` | SSR `BLND` | Cancelled `SCHEDULE_CHANGE` |

The first two units were cancelled together by `unitIds`; the rest were then cancelled by a whole-hold cancel.

### Hold `1564400911759966208` — expired (order `17930526173`)

| Variant | Product | Choice sent | End |
|---|---|---|---|
| A21 | `FAST_TRACK_IKA_LOCAL` | — | Expired |
| A22 | `CIP_IKA_LOCAL` | appointment 2026-10-28 06:30 +03:30, 2 guests | Expired |
| A24 | `WIFI_FULL_FLIGHT_LOCAL` | 1 device | Expired |
| A08 | `ECO_SEAT_PREFERRED` | seat 8C on IST→IKA 1598 | Expired |

- The hold deadline was 90 seconds.
- The real expiry poller wrote `Status = 4` on the hold and all 4 units (SQL).
- Confirm afterwards → 409 16407.

## 4. Variants that cannot be held (refused before any write)

| Variant | Product | Answer |
|---|---|---|
| A09 extra seat | — | `extraSeats` is not a Hold member → 400 |
| A13/A14 pets | — | `pets` is not a Hold member → 400 |
| A17 | `MEDICAL_OXYGEN` | 409 16419, needs supplier confirmation |
| A18 | `BASSINET` | 409 16419, needs supplier confirmation |
| A19 | `UNACCOMPANIED_MINOR` | 409 16419, needs supplier confirmation |
| A10 with a quote | `UPG_ECONOMY_TO_BUSINESS` (external supplier) | **422 16104**, "Supplier fulfillment provider RevenueDesk is not registered" |

The last row is the one check of 24 that did not match: my script expected 16419. The product is still refused before any write; the existing external-supplier guard simply answers first.
