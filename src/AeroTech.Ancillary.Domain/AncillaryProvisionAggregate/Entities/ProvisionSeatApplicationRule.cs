using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionSeatApplicationRule : Entity<long>
    {
        private readonly List<ProvisionSeatNumber> _seatNumbers = new();
        private readonly List<ProvisionSeatCharacteristic> _seatCharacteristics = new();

        private ProvisionSeatApplicationRule()
        {
        }

        private ProvisionSeatApplicationRule(long id, long ancillaryProvisionId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
        }

        public long AncillaryProvisionId { get; private set; }

        public IReadOnlyCollection<ProvisionSeatNumber> SeatNumbers => _seatNumbers.AsReadOnly();

        public IReadOnlyCollection<ProvisionSeatCharacteristic> SeatCharacteristics => _seatCharacteristics.AsReadOnly();

        internal static Func<ProvisionSeatApplicationRule?> Plan(
            ProvisionSeatApplicationRule? stored,
            long ancillaryProvisionId,
            ProvisionSeatApplicationArgs? args,
            IIdGenerator idGenerator)
        {
            if (args is null || args.IsEmpty)
                return () => null;

            var rule = stored ?? new ProvisionSeatApplicationRule(idGenerator.NewId(), ancillaryProvisionId);
            var seatNumbers = AncillaryProvision.MergeRows(
                rule._seatNumbers,
                args.SeatNumbers.Select(value => new ProvisionSeatNumber(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(SeatNumbers));
            var seatCharacteristics = AncillaryProvision.MergeRows(
                rule._seatCharacteristics,
                args.SeatCharacteristicCodes.Select(value => new ProvisionSeatCharacteristic(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(SeatCharacteristics));

            return () =>
            {
                AncillaryProvision.ReplaceRows(rule._seatNumbers, seatNumbers);
                AncillaryProvision.ReplaceRows(rule._seatCharacteristics, seatCharacteristics);

                return rule;
            };
        }
    }
}
