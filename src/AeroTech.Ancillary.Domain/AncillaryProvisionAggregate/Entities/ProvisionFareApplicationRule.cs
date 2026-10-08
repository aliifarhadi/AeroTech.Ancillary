using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionFareApplicationRule : Entity<long>
    {
        private readonly List<ProvisionAirFare> _airFares = new();
        private readonly List<ProvisionAirFareType> _airFareTypes = new();
        private readonly List<ProvisionFareFamily> _fareFamilies = new();
        private readonly List<ProvisionFareBasis> _fareBases = new();
        private readonly List<ProvisionCabinClass> _cabinClasses = new();
        private readonly List<ProvisionRbd> _rbds = new();

        private ProvisionFareApplicationRule()
        {
        }

        private ProvisionFareApplicationRule(long id, long ancillaryProvisionId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
        }

        public long AncillaryProvisionId { get; private set; }

        public IReadOnlyCollection<ProvisionAirFare> AirFares => _airFares.AsReadOnly();

        public IReadOnlyCollection<ProvisionAirFareType> AirFareTypes => _airFareTypes.AsReadOnly();

        public IReadOnlyCollection<ProvisionFareFamily> FareFamilies => _fareFamilies.AsReadOnly();

        public IReadOnlyCollection<ProvisionFareBasis> FareBases => _fareBases.AsReadOnly();

        public IReadOnlyCollection<ProvisionCabinClass> CabinClasses => _cabinClasses.AsReadOnly();

        public IReadOnlyCollection<ProvisionRbd> Rbds => _rbds.AsReadOnly();

        internal static Func<ProvisionFareApplicationRule?> Plan(
            ProvisionFareApplicationRule? stored,
            long ancillaryProvisionId,
            ProvisionFareApplicationArgs? args,
            IIdGenerator idGenerator)
        {
            if (args is null || args.IsEmpty)
                return () => null;

            var rule = stored ?? new ProvisionFareApplicationRule(idGenerator.NewId(), ancillaryProvisionId);
            var airFares = AncillaryProvision.MergeRows(
                rule._airFares,
                args.AirFareIds.Select(value => new ProvisionAirFare(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(AirFares));
            var airFareTypes = AncillaryProvision.MergeRows(
                rule._airFareTypes,
                args.AirFareTypes.Select(value => new ProvisionAirFareType(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(AirFareTypes));
            var fareFamilies = AncillaryProvision.MergeRows(
                rule._fareFamilies,
                args.FareFamilyIds.Select(value => new ProvisionFareFamily(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(FareFamilies));
            var fareBases = AncillaryProvision.MergeRows(
                rule._fareBases,
                args.FareBasisCodes.Select(value => new ProvisionFareBasis(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(FareBases));
            var cabinClasses = AncillaryProvision.MergeRows(
                rule._cabinClasses,
                args.CabinClassIds.Select(value => new ProvisionCabinClass(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(CabinClasses));
            var rbds = AncillaryProvision.MergeRows(
                rule._rbds,
                args.RbdIds.Select(value => new ProvisionRbd(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(Rbds));

            return () =>
            {
                AncillaryProvision.ReplaceRows(rule._airFares, airFares);
                AncillaryProvision.ReplaceRows(rule._airFareTypes, airFareTypes);
                AncillaryProvision.ReplaceRows(rule._fareFamilies, fareFamilies);
                AncillaryProvision.ReplaceRows(rule._fareBases, fareBases);
                AncillaryProvision.ReplaceRows(rule._cabinClasses, cabinClasses);
                AncillaryProvision.ReplaceRows(rule._rbds, rbds);

                return rule;
            };
        }
    }
}
