using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service
{
    public sealed record ServiceGroupedSelectionResolutionDto(
        ServiceGroupedResolutionMetaDto Meta,
        IReadOnlyList<ServiceResolvedSeatDto> Seats,
        IReadOnlyList<ServiceGroupedResolvedSelectionDto> Baggages,
        IReadOnlyList<ServiceGroupedResolvedSelectionDto> Meals,
        IReadOnlyList<ServiceGroupedResolvedSelectionDto> Pets,
        IReadOnlyList<ServiceGroupedResolvedSelectionDto> AssistedTravel,
        IReadOnlyList<ServiceGroupedResolvedSelectionDto> AirportServices,
        IReadOnlyList<ServiceGroupedResolvedSelectionDto> PriorityServices,
        IReadOnlyList<ServiceGroupedResolvedSelectionDto> Connectivity,
        IReadOnlyList<ServiceGroupedResolvedSelectionDto> Upgrades,
        IReadOnlyList<ServiceGroupedResolvedSelectionDto> ExtraSeats);
}
