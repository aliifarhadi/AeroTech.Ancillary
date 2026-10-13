# 16 — External evidence and verified internal source ledger

## PUBLIC evidence; what it proves and does NOT prove
1. **Amadeus Self-Service, Seatmap Display**: API guidance and FAQ: `https://admin.developers.amadeus.com/self-service/apis-docs/guides/developer-guides/faq/` confirms physical seat states AVAILABLE/BLOCKED/OCCUPIED and that SeatMap is distinct from other flight shopping endpoints. It does NOT reveal Amadeus internal database/service boundaries. Do not attribute AeroTech's independent commercial engine architecture to Amadeus as fact.
2. **NDC 19.2 message catalog**: `https://docs.ndc.one/Doku-19_2to/ndc-messages.html` enumerates `IATA_SeatAvailabilityRQ/RS` separately from `IATA_ServiceListRQ/RS`. This supports a separate consumer Seat Offers surface; not a requirement for two internal pricing engines.
3. v12.2 pack's source traceability includes Condor/flydubai references; if those sources are unavailable, do not claim exact wire behavior. No P3 reference used to authorize new reservation code.

## CODE evidence (GitHub direct read; 2026-10-13, HEAD `8fda5f1e`)
- `src/AeroTech.Ancillary.RestApi/V1/AncillaryOffers/Controllers/{Api,OtaPanel,Backoffice}Controller.cs`: shared Offers, Details, Evaluate, broken SeatMaps under all three surfaces.
- `src/AeroTech.Ancillary.Query/AncillaryOffers/Queries/GetSeatMap/GetSeatMapService.cs`: checks only `IFlightSeatMapReference` then ALWAYS throws `SeatMapSourceUnavailable`.
- `src/AeroTech.Ancillary.Providers/DependencyInjection.cs`: registers disconnected seatMap/delegation providers and a concrete FlightFlow HTTP flight-occurrence client.
- `src/AeroTech.Ancillary.Application/AncillaryInventoryPolicyAggregate/Services/InventoryPolicyEvidenceBuilder.cs`: hard dependency on `IFlightFlowDelegationReference`.
- `src/AeroTech.Ancillary.Domain/AncillaryInventoryPolicyAggregate/AncillaryInventoryPolicy.cs`: `Activate` switches FlightFlow delegation; local stock and supplier independently valid.
- `src/AeroTech.Ancillary.Shopping/Engine/AncillaryShoppingEngine.cs`: already present `ShopAsync(context, ShoppingFilter?)` and `EvaluateSelectionAsync`, loads InventoryPolicy and determines readiness based on it, affecting Seat incorrectly.
- `src/AeroTech.Ancillary.Shopping/Engine/AvailabilityAssessor.cs`: direct `InventoryAuthority.FlightFlow` branch with source probing.
- `src/AeroTech.Ancillary.Shopping/Context/SeatCharacteristicFacts.cs`: declared trust metadata source for seat number/characteristics; `FlightOfferShoppingContextMapper` does not populate it from ordinary AirOffer.
- `src/AeroTech.Ancillary.Query/AncillaryOffers/Queries/GetAncillaryOffers/Api/ApiAncillaryOffersMapper.cs`: Seat currently packaged among OtherServices, rather than a dedicated Seat surface.
- `src/AeroTech.Ancillary.Shopping/Reading/Sql/SqlActiveAncillaryProvisionReader.cs`: null/empty POS allows current eligible provision.
- `src/AeroTech.Ancillary.Domain/AncillaryProvisionAggregate/AncillaryProvision.Publication.cs`: mandatory POS guard intentionally removed at HEAD.
- `src/AeroTech.Ancillary.RestApi/V1/AncillaryServiceDefinitionAggregate/Requests/SeatSpecificationRequest.cs` and `...Query/AncillaryServiceDefinitionAggregate/Dto/SeatVariantSpecificationDtos.cs`: actual A07/A08/A09 typed API.

## Boundaries of this review
Source control facts verified through GitHub connector at the named commit. Public benchmark facts above externally sourced. This document is an implementation SPECIFICATION, not a record of successful code edits or executed tests. There has been NO independent dotnet solution build, SQL Server migration run, or live authenticated HTTP request in this authoring step. Exact new SeatOffers route and NotApplicable=8 are v12.3 design decisions, not claims that they currently exist.
