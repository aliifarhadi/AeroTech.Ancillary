using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Selections
{
    public sealed class AirportServiceSelection
    {
        private const string Name = nameof(AirportServiceSelection);

        private AirportServiceSelection()
        {
        }

        public DateTimeOffset? TimeWithOffset { get; private set; }

        public int? GuestCount { get; private set; }

        public int AirportId { get; private set; }

        public long? FacilityId { get; private set; }

        internal static AirportServiceSelection Create(AirportServiceSelectionArgs args)
        {
            Require(args.GuestCount is null or >= 0, nameof(GuestCount));
            Require(args.AirportId is > 0, nameof(AirportId));
            Require(args.FacilityId is null or > 0, nameof(FacilityId));

            return new AirportServiceSelection
            {
                TimeWithOffset = args.TimeWithOffset,
                GuestCount = args.GuestCount,
                AirportId = args.AirportId!.Value,
                FacilityId = args.FacilityId
            };
        }

        public AirportServiceSelectionArgs ToArgs() => new(TimeWithOffset, GuestCount, AirportId, FacilityId);

        internal bool IsChosenBy(AirportServiceSelectionArgs? chosen)
            => TimeWithOffset == chosen?.TimeWithOffset && GuestCount == chosen?.GuestCount;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid($"{Name}.{field}");
        }
    }
}
