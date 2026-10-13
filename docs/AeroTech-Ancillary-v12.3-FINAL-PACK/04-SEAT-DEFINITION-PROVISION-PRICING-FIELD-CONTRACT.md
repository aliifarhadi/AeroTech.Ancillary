# 04 — Seat A07/A08/A09 exact field-level authoring and P2 pricing contract

## Current, already implemented Backoffice endpoints (retain, do not duplicate)
- `POST /Backoffice/v1/AncillaryServiceDefinitions/Seat`
- `GET /Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId}`
- `PUT /Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId}`
- `GET /Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId}/ProvisionForm`
- `POST /Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId}/Provisions`
- `GET /Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId}/Provisions/{provisionId}/Form`
- `PUT /Backoffice/v1/AncillaryServiceDefinitions/Seat/{serviceDefinitionId}/Provisions/{provisionId}`
- Existing `AncillaryPricings` paginated/CRUD/Activate/Revise and `/AncillaryProvisions/{provisionId}/Pricings` remain.

## `DefineServiceDefinitionRequest<SeatSpecificationRequest>` exact POST fields
| Field | Type | Rule |
|---|---|---|
| `ownerAirlineId` | int | Required >0, authenticated airline scope; CREATE only |
| `serviceDefinitionRef` | string | CREATE only, uppercase/digits/underscore, <=30, stable product identity |
| `supplierId` | long | >0, must be valid owner supplier |
| `serviceSubCode` | string(3) | required alphanumeric industrial or carrier code |
| `subCodeSource` | enum | Industry or CarrierDefined, use real registered meaning |
| `serviceTypeCode` | string(1)? | code validated or looked up when Industry |
| `groupCode` / `subGroupCode` | string(2)? | industrial scheme compatibility |
| `description1Code` / `description2Code` | string(2)? | classification |
| `pricingUnit` | enum | EXACT `PerSeat=5` for all A07/A08/A09 |
| `serviceDateBasis` | enum | EXACT `FlightDeparture` for A07/A08/A09 |
| `variantCode` | string | `A07`, `A08`, `A09` only; determines specification union |
| `documentRouting` | enum | Emd / TicketOrExchange / NoAncillaryDocument; consistent with nested document and spec |
| `specification` | typed SeatSpecificationRequest | variant-specific, see below |
| `commercialName` | string | required <=100 |
| `description` | string? | <=500 |
| `document` | `{type,rfic?,rfisc?}` | None=>both null; EMD=>approved 1-char RFIC and 3-char RFISC, no fake ticket doc |
| `booking` | `{method,ssrCode?,ssimCode?,confirmationRequirement?}` | SSR method needs valid SSR; codes <=4 uppercase alnum; default confirmation Immediate |
| `salesEffectiveFrom`, `salesDiscontinueOn` | ISO DateOnly? | Optional, effective <= discontinue |

PUT body is exactly POST minus `ownerAirlineId`, `serviceDefinitionRef`; use `GET Seat/{id}` response `.form` as the PUT payload, without UI-only metadata.

## Typed `SeatSpecificationRequest` and read-view differences
| Field | A07 | A08 | A09 |
|---|---|---|---|
| `seatCharacteristicCodes:string[]?` | optional | **nonempty required** | optional |
| `applicableCabinIds:int[]?` | optional | optional | optional |
| `requiresExitRowEligibility:bool` | required | required | required |
| `requiresAdjacentSeat:bool` | false/absent | false/absent | extra-seat field |
| `extraSeatPurpose:ExtraSeatPurpose?` | null/absent | null/absent | **required** PassengerComfort or CabinBaggage |
| `extraOccupiedSeatCount:int?` | null/absent | null/absent | **required >=1** |
| `requiresExternalTicketAction:bool` | false/absent | false/absent | actual airline commercial ticket declaration |
| `seatPurpose:SeatPurpose?` | derived Standard | derived Preferred | derived ExtraSeat |

POST currently tolerates `seatPurpose`, computed from variant when absent; if explicitly supplied must agree. For new FE do NOT render a seatPurpose choice and do NOT put extra-seat-only fields in A07/A08 DTO. `GET` variant DTO intentionally has only variant-relevant members. Codes normalize uppercase and require uniqueness, <=25; cabins must be unique positive IDs. Exit-row eligibility is a passenger safety predicate, not an assertion that an exit-row seat exists or is available.

## Seat Provision rules (retained)
- `ApplicationType=Seat`, `SeatApplication.SeatNumbers[]` OR `SeatApplication.SeatCharacteristicCodes[]` nonempty; SeatNumbers requires appropriate `FlightApplication.AircraftIds[]` scope. Neither array is physical stock.
- `Quantity.Unit=Each`, A07/A08 `MaxQuantity=1`; A09 extra occupied seat count remains in Definition, no fabricated single-seat cap.
- Shared ten rules include PassengerEligibility, SalesRestrictions (POS optional), Geography, FlightApplication, FareApplication, TravelDate with **BlackoutPeriods**, DayTimeApplication, AdvancePurchase, BaggageApplication (only Baggage), SeatApplication (only Seat).
- Seat Pricing `Paid+Filed`: properly active pricing rates, exact currency and tax/fee math; `Free`: no fake zero-value money; `NotAvailable`: not offered; Quote: only with real declared authority, no guessed rates.
- Definition/Provision/Pricing CRUD/activation can be executed with **zero InventoryPolicy** and **zero network access to FlightFlow**.

## Tests
SEAT-D01/02/03: each variant CREATE + GET + PUT full form round-trip; invalid A08 empty characteristics and invalid A09 purpose/count rejected. SEAT-P01: A07/A08 quantity >1 rejected, A09 legitimate extra-seat definition allowed. SEAT-I01: zero policy creation required; generic policy CREATE/CHANGE/ACTIVATE against Seat rejected with consistent typed 422; no Stock sources for Seat. SEAT-S01: price from published Seat provision via existing shopping engine, with no policy/no FlightFlow. SEAT-S02: fare/cabin/currency/exit-row predicates preserved; seat availability never invented. SEAT-S03: nonSeat policy does not apply to Seat.
