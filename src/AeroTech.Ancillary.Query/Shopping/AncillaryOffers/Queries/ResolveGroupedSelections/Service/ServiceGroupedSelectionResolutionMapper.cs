using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections.Service;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections.Service;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections.Service
{
    public static class ServiceGroupedSelectionResolutionMapper
    {
        public static ServiceGroupedSelectionResolutionDto Map(GroupedSelectionResolutionSnapshot resolution)
            => new(
                new ServiceGroupedResolutionMetaDto(resolution.OfferId, resolution.OrderId, resolution.OrderCommercialVersion, resolution.AllValid),
                ServiceSeatMapResolutionMapper.Map(resolution.Seats).Seats,
                Of(resolution, AncillaryProfile.Baggage),
                Of(resolution, AncillaryProfile.Meal),
                Of(resolution, AncillaryProfile.Pet),
                Of(resolution, AncillaryProfile.AssistedTravel),
                Of(resolution, AncillaryProfile.AirportService),
                Of(resolution, AncillaryProfile.Priority),
                Of(resolution, AncillaryProfile.Connectivity),
                Of(resolution, AncillaryProfile.Upgrade),
                Of(resolution, AncillaryProfile.Seat));

        private static IReadOnlyList<ServiceGroupedResolvedSelectionDto> Of(GroupedSelectionResolutionSnapshot resolution, AncillaryProfile profile)
            => resolution.Selections
                .Where(selection => selection.Profile == profile)
                .Select(selection => new ServiceGroupedResolvedSelectionDto(
                    selection.SelectionId,
                    selection.Status.ToString(),
                    selection.ReasonCodes,
                    selection is { Line: { Candidate: { } candidate } line, Context: { } context } ? ServiceSelectionResolutionMapper.Service(line, candidate, context) : null))
                .ToList();
    }
}
