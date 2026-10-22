using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections.Service
{
    public sealed class ServiceResolveGroupedSelectionsQuery : IRequest<ServiceGroupedSelectionResolutionDto>
    {
        public string? OfferId { get; set; }

        public long? OrderId { get; set; }

        public int? ExpectedOrderCommercialVersion { get; set; }

        public IReadOnlyList<string>? Seats { get; set; }

        public IReadOnlyList<string>? Baggages { get; set; }

        public IReadOnlyList<string>? Meals { get; set; }

        public IReadOnlyList<string>? Pets { get; set; }

        public IReadOnlyList<string>? AssistedTravel { get; set; }

        public IReadOnlyList<string>? AirportServices { get; set; }

        public IReadOnlyList<string>? PriorityServices { get; set; }

        public IReadOnlyList<string>? Connectivity { get; set; }

        public IReadOnlyList<string>? Upgrades { get; set; }

        public IReadOnlyList<string>? ExtraSeats { get; set; }
    }
}
