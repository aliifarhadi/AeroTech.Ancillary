using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Shopping.Context
{
    public sealed record ShoppingTraveller
    {
        public required string TravellerRef { get; init; }

        public long? OrderTravellerId { get; init; }

        public required PassengerTypeCode PassengerTypeCode { get; init; }

        public DateOnly? DateOfBirth { get; init; }

        public int? VerifiedAgeAtTravel { get; init; }

        public DateOnly? AgeEvidenceAsOfDate { get; init; }

        public string? AssociatedAdultRef { get; init; }

        public string? StableTravellerIdentity { get; init; }

        public bool? VerifiedExitRowEligible { get; init; }
    }
}
