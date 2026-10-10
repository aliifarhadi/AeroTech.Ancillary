using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Shopping.Context
{
    public sealed record TravellerFareFacts
    {
        public required string TravellerRef { get; init; }

        public required string FlightRef { get; init; }

        public required string PortionRef { get; init; }

        public long? AirFareId { get; init; }

        public AirFareType? AirFareType { get; init; }

        public long? FareFamilyId { get; init; }

        public string? FareFamilyCodeOrName { get; init; }

        public string? FareBasisCode { get; init; }

        public int? CabinClassId { get; init; }

        public long? RbdId { get; init; }

        public string? BookingClass { get; init; }

        public string? FareSnapshotVersion { get; init; }
    }
}
