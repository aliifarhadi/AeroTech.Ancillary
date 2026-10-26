using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Selections
{
    public sealed class ConnectivitySelection
    {
        private const string Name = nameof(ConnectivitySelection);

        private ConnectivitySelection()
        {
        }

        public int? DeviceCount { get; private set; }

        internal static ConnectivitySelection Create(ConnectivitySelectionArgs args)
        {
            if (args.DeviceCount is not > 0)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid($"{Name}.{nameof(DeviceCount)}");

            return new ConnectivitySelection { DeviceCount = args.DeviceCount };
        }

        public ConnectivitySelectionArgs ToArgs() => new(DeviceCount);

        internal bool IsChosenBy(ConnectivitySelectionArgs? chosen) => DeviceCount == chosen?.DeviceCount;
    }
}
