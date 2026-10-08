using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionGeographyRule : Entity<long>
    {
        private readonly List<ProvisionOriginAirport> _originAirports = new();
        private readonly List<ProvisionDestinationAirport> _destinationAirports = new();
        private readonly List<ProvisionViaAirport> _viaAirports = new();
        private readonly List<ProvisionCoverageCountry> _coverageCountries = new();
        private readonly List<ProvisionRoutePair> _routePairs = new();
        private readonly List<ProvisionServiceLocation> _serviceLocations = new();

        private ProvisionGeographyRule()
        {
        }

        private ProvisionGeographyRule(long id, long ancillaryProvisionId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
        }

        public long AncillaryProvisionId { get; private set; }

        public IReadOnlyCollection<ProvisionOriginAirport> OriginAirports => _originAirports.AsReadOnly();

        public IReadOnlyCollection<ProvisionDestinationAirport> DestinationAirports => _destinationAirports.AsReadOnly();

        public IReadOnlyCollection<ProvisionViaAirport> ViaAirports => _viaAirports.AsReadOnly();

        public IReadOnlyCollection<ProvisionCoverageCountry> CoverageCountries => _coverageCountries.AsReadOnly();

        public IReadOnlyCollection<ProvisionRoutePair> RoutePairs => _routePairs.AsReadOnly();

        public IReadOnlyCollection<ProvisionServiceLocation> ServiceLocations => _serviceLocations.AsReadOnly();

        internal static Func<ProvisionGeographyRule?> Plan(
            ProvisionGeographyRule? stored,
            long ancillaryProvisionId,
            ProvisionGeographyArgs? args,
            IIdGenerator idGenerator)
        {
            if (args is null || args.IsEmpty)
                return () => null;

            var rule = stored ?? new ProvisionGeographyRule(idGenerator.NewId(), ancillaryProvisionId);
            var originAirports = AncillaryProvision.MergeRows(
                rule._originAirports,
                args.OriginAirportIds.Select(value => new ProvisionOriginAirport(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(OriginAirports));
            var destinationAirports = AncillaryProvision.MergeRows(
                rule._destinationAirports,
                args.DestinationAirportIds.Select(value => new ProvisionDestinationAirport(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(DestinationAirports));
            var viaAirports = AncillaryProvision.MergeRows(
                rule._viaAirports,
                args.ViaAirportIds.Select(value => new ProvisionViaAirport(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(ViaAirports));
            var coverageCountries = AncillaryProvision.MergeRows(
                rule._coverageCountries,
                args.CoverageCountryIds.Select(value => new ProvisionCoverageCountry(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(CoverageCountries));
            var routePairs = AncillaryProvision.MergeRows(
                rule._routePairs,
                args.RoutePairs.Select(pair => new ProvisionRoutePair(idGenerator.NewId(), ancillaryProvisionId, rule.Id, pair)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.Overlaps(right),
                nameof(RoutePairs));
            var serviceLocations = AncillaryProvision.MergeRows(
                rule._serviceLocations,
                args.ServiceLocations.Select(location => new ProvisionServiceLocation(idGenerator.NewId(), ancillaryProvisionId, rule.Id, location)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(ServiceLocations));

            return () =>
            {
                AncillaryProvision.ReplaceRows(rule._originAirports, originAirports);
                AncillaryProvision.ReplaceRows(rule._destinationAirports, destinationAirports);
                AncillaryProvision.ReplaceRows(rule._viaAirports, viaAirports);
                AncillaryProvision.ReplaceRows(rule._coverageCountries, coverageCountries);
                AncillaryProvision.ReplaceRows(rule._routePairs, routePairs);
                AncillaryProvision.ReplaceRows(rule._serviceLocations, serviceLocations);

                return rule;
            };
        }
    }
}
