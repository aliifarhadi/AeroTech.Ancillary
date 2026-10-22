using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections.Service
{
    public sealed class ServiceResolveGroupedSelectionsQueryHandler : IRequestHandler<ServiceResolveGroupedSelectionsQuery, ServiceGroupedSelectionResolutionDto>
    {
        private readonly IResolveGroupedSelectionsService _service;

        public ServiceResolveGroupedSelectionsQueryHandler(IResolveGroupedSelectionsService service) => _service = service;

        public async Task<ServiceGroupedSelectionResolutionDto> Handle(ServiceResolveGroupedSelectionsQuery query, CancellationToken cancellationToken)
            => ServiceGroupedSelectionResolutionMapper.Map(
                await _service.ExecuteAsync(
                    new GroupedSelectionRequest(
                        query.OfferId,
                        query.OrderId,
                        query.ExpectedOrderCommercialVersion,
                        query.Seats ?? [],
                        Entries(AncillaryProfile.Baggage, query.Baggages)
                            .Concat(Entries(AncillaryProfile.Meal, query.Meals))
                            .Concat(Entries(AncillaryProfile.Pet, query.Pets))
                            .Concat(Entries(AncillaryProfile.AssistedTravel, query.AssistedTravel))
                            .Concat(Entries(AncillaryProfile.AirportService, query.AirportServices))
                            .Concat(Entries(AncillaryProfile.Priority, query.PriorityServices))
                            .Concat(Entries(AncillaryProfile.Connectivity, query.Connectivity))
                            .Concat(Entries(AncillaryProfile.Upgrade, query.Upgrades))
                            .Concat(Entries(AncillaryProfile.Seat, query.ExtraSeats))
                            .ToList()),
                    cancellationToken));

        private static IEnumerable<GroupedSelectionEntry> Entries(AncillaryProfile profile, IReadOnlyList<string>? ids)
            => (ids ?? []).Select(id => new GroupedSelectionEntry(profile, id));
    }
}
