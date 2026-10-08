using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionPassengerEligibilityRule : Entity<long>
    {
        private readonly List<ProvisionPassengerType> _passengerTypes = new();
        private readonly List<ProvisionEligibleAgeBand> _ageBands = new();

        private ProvisionPassengerEligibilityRule()
        {
        }

        private ProvisionPassengerEligibilityRule(long id, long ancillaryProvisionId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
        }

        public long AncillaryProvisionId { get; private set; }

        public IReadOnlyCollection<ProvisionPassengerType> PassengerTypes => _passengerTypes.AsReadOnly();

        public IReadOnlyCollection<ProvisionEligibleAgeBand> AgeBands => _ageBands.AsReadOnly();

        internal static Func<ProvisionPassengerEligibilityRule?> Plan(
            ProvisionPassengerEligibilityRule? stored,
            long ancillaryProvisionId,
            ProvisionPassengerEligibilityArgs? args,
            IIdGenerator idGenerator)
        {
            if (args is null || args.IsEmpty)
                return () => null;

            var rule = stored ?? new ProvisionPassengerEligibilityRule(idGenerator.NewId(), ancillaryProvisionId);
            var passengerTypes = AncillaryProvision.MergeRows(
                rule._passengerTypes,
                args.PassengerTypes.Select(value => new ProvisionPassengerType(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(PassengerTypes));
            var ageBands = AncillaryProvision.MergeRows(
                rule._ageBands,
                args.AgeBands.Select(band => new ProvisionEligibleAgeBand(idGenerator.NewId(), ancillaryProvisionId, rule.Id, band)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(AgeBands));

            var ordered = ageBands.OrderBy(band => band.AgeFromInclusive).ToList();

            for (var index = 1; index < ordered.Count; index++)
                if (ordered[index - 1].AgeToExclusive is not { } upper || upper > ordered[index].AgeFromInclusive)
                    throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionPassengerEligibilityRule)}.{nameof(AgeBands)}");

            return () =>
            {
                AncillaryProvision.ReplaceRows(rule._passengerTypes, passengerTypes);
                AncillaryProvision.ReplaceRows(rule._ageBands, ageBands);

                return rule;
            };
        }
    }
}
