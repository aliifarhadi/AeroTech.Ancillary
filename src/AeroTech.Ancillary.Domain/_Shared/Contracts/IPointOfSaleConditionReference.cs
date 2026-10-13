namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public sealed record PointOfSaleCondition
    {
        public required long PointOfSaleId { get; init; }

        public bool? CountryIsInclude { get; init; }

        public IReadOnlyCollection<long> CountryIds { get; init; } = [];

        public bool? AirlineOfficeIsInclude { get; init; }

        public IReadOnlyCollection<long> AirlineOfficeIds { get; init; } = [];

        public bool? TravelAgencyOfficeIsInclude { get; init; }

        public IReadOnlyCollection<long> TravelAgencyOfficeIds { get; init; } = [];

        public bool? ChannelIsInclude { get; init; }

        public IReadOnlyCollection<string> Channels { get; init; } = [];
    }

    public sealed record PointOfSaleConditionSet(IReadOnlyCollection<long> PointOfSaleIds, IReadOnlyCollection<PointOfSaleCondition> Conditions);

    public interface IPointOfSaleConditionReference
    {
        Task<PointOfSaleConditionSet> GetConditionSetAsync(CancellationToken cancellationToken = default);
    }
}
