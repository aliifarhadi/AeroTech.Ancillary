using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Selections
{
    public sealed class UpgradeSelection
    {
        private const string Name = nameof(UpgradeSelection);

        private UpgradeSelection()
        {
        }

        public int ToCabinId { get; private set; }

        internal static UpgradeSelection Create(UpgradeSelectionArgs args)
        {
            if (args.ToCabinId <= 0)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid($"{Name}.{nameof(ToCabinId)}");

            return new UpgradeSelection { ToCabinId = args.ToCabinId };
        }

        public UpgradeSelectionArgs ToArgs() => new(ToCabinId);
    }
}
