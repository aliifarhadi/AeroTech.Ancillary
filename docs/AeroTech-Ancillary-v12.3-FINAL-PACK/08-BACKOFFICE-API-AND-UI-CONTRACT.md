# 08 — Backoffice P1 authoring and P2 inventory screens

## Seat editor and existing REST contracts
Keep `GET /Backoffice/v1/AncillaryServiceDefinitions/Paginated?Profile=Seat`; `GET /Backoffice/v1/AncillaryServiceDefinitions/Variants?Profile=Seat`; POST/GET/PUT `/Backoffice/v1/AncillaryServiceDefinitions/Seat` and `Seat/{id}`; nested `Seat/{id}/ProvisionForm`, `Seat/{id}/Provisions`, `Seat/{id}/Provisions/{provisionId}/Form`, PUT same path; generic pricing and lifecycle under current `AncillaryProvisions`/`AncillaryPricings`. See `04` for all actual fields, validator rules and form round-trips.

**Seat page layout:** Definition > Provisions > Pricings, with list/revision/activation/publish only as actually provided. **NO Seat InventoryPolicy tab, wizard, Create/Activate/ProviderKey control or count stock.** Remove previous seat-inventory frontend instructions from authority. SeatMap rendering and actual seat choice are NOT part of P1 Seat definition UI.

## NonSeat editor
Other eight profiles keep their typed authoring forms and appropriate inventory configuration controls when the airline actually owns or delegates a finite resource. Policy routes remain for nonSeat at `/Backoffice/v1/AncillaryInventoryPolicies` with Define, Change, Activate, Suspend, Retire, ByServiceIdentity, and `/AncillaryInventoryConfigurationSnapshots`. FlightCount/FlightWeight/AirportSlot editors retained. For explicit `Unlimited`, UI must not claim guaranteed capacity. Failed reference proof must not invent ProviderKey or fake facility.

## Definition / pricing details
- Each Seat variant exposes only its allowed `Specification` properties; A08 must require at least one characteristic; A09 extra-seat fields do not leak into A07/A08.
- `GET Seat/{id}` `.form` is exact PUT body. `ownerAirlineId`, stable ref, status, version, creation timestamps are read-only/header; not sent to PUT. Similarly nested Seat Provision GET Form is round-trippable; create-form lacks persisted ID/editable flags.
- Show `PriceOrigin` and Outcome; Paid Filed needs active Pricing Rate in actual currency; Free has no synthetic money; supplier identity separate from inventory.
- Display all shared rules including `TravelDate.BlackoutPeriods`, FlightApplication aircraft IDs, `FareApplication`, SalesRestrictions zero-or-many POS filter.
- Existing surface Auth/JWT and owner scope guard remain; do not treat caller-submitted OwnerAirlineId as authorization proof.

## Reusable authoring acceptance
One create/edit/get/publish smoke per A01–A24, one negative wrong-family spec, one negative malformed enum/member; Seat no InventoryPolicy requests. NonSeat inventory policy forms still function. No new backend field invented to satisfy UI aesthetics.
