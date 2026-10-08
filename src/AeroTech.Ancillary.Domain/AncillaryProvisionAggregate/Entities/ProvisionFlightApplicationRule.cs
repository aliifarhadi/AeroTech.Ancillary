using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionFlightApplicationRule : Entity<long>
    {
        private readonly List<ProvisionMarketingAirline> _marketingAirlines = new();
        private readonly List<ProvisionOperatingAirline> _operatingAirlines = new();
        private readonly List<ProvisionFlightNumber> _flightNumbers = new();
        private readonly List<ProvisionFlight> _flights = new();
        private readonly List<ProvisionAircraft> _aircraft = new();

        private ProvisionFlightApplicationRule()
        {
        }

        private ProvisionFlightApplicationRule(long id, long ancillaryProvisionId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
        }

        public long AncillaryProvisionId { get; private set; }

        public IReadOnlyCollection<ProvisionMarketingAirline> MarketingAirlines => _marketingAirlines.AsReadOnly();

        public IReadOnlyCollection<ProvisionOperatingAirline> OperatingAirlines => _operatingAirlines.AsReadOnly();

        public IReadOnlyCollection<ProvisionFlightNumber> FlightNumbers => _flightNumbers.AsReadOnly();

        public IReadOnlyCollection<ProvisionFlight> Flights => _flights.AsReadOnly();

        public IReadOnlyCollection<ProvisionAircraft> Aircraft => _aircraft.AsReadOnly();

        internal static Func<ProvisionFlightApplicationRule?> Plan(
            ProvisionFlightApplicationRule? stored,
            long ancillaryProvisionId,
            ProvisionFlightApplicationArgs? args,
            IIdGenerator idGenerator)
        {
            if (args is null || args.IsEmpty)
                return () => null;

            var rule = stored ?? new ProvisionFlightApplicationRule(idGenerator.NewId(), ancillaryProvisionId);
            var marketingAirlines = AncillaryProvision.MergeRows(
                rule._marketingAirlines,
                args.MarketingAirlineIds.Select(value => new ProvisionMarketingAirline(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(MarketingAirlines));
            var operatingAirlines = AncillaryProvision.MergeRows(
                rule._operatingAirlines,
                args.OperatingAirlineIds.Select(value => new ProvisionOperatingAirline(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(OperatingAirlines));
            var flightNumbers = AncillaryProvision.MergeRows(
                rule._flightNumbers,
                args.FlightNumbers.Select(value => new ProvisionFlightNumber(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(FlightNumbers));
            var flights = AncillaryProvision.MergeRows(
                rule._flights,
                args.FlightIds.Select(value => new ProvisionFlight(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(Flights));
            var aircraft = AncillaryProvision.MergeRows(
                rule._aircraft,
                args.AircraftIds.Select(value => new ProvisionAircraft(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(Aircraft));

            return () =>
            {
                AncillaryProvision.ReplaceRows(rule._marketingAirlines, marketingAirlines);
                AncillaryProvision.ReplaceRows(rule._operatingAirlines, operatingAirlines);
                AncillaryProvision.ReplaceRows(rule._flightNumbers, flightNumbers);
                AncillaryProvision.ReplaceRows(rule._flights, flights);
                AncillaryProvision.ReplaceRows(rule._aircraft, aircraft);

                return rule;
            };
        }
    }
}
