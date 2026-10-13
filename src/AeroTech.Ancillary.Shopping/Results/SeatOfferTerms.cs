using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Results
{
    public sealed record SeatOfferTerms(
        SeatPurpose Purpose,
        IReadOnlyList<string> CharacteristicCodes,
        IReadOnlyList<int> ApplicableCabinIds,
        bool RequiresExitRowEligibility,
        bool RequiresAdjacentSeat,
        ExtraSeatPurpose? ExtraSeatPurpose,
        int? ExtraOccupiedSeatCount,
        IReadOnlyList<string> SeatNumbers)
    {
        public static SeatOfferTerms Of(SeatSpecification specification, ProvisionSeatApplicationRule? application)
        {
            var ruled = application?.SeatCharacteristics.Select(row => row.CharacteristicCode).Order(StringComparer.Ordinal).ToList() ?? [];

            return new SeatOfferTerms(
                specification.SeatPurpose,
                ruled.Count > 0 ? ruled : specification.SeatCharacteristicCodes.Select(row => row.Code).Order(StringComparer.Ordinal).ToList(),
                specification.ApplicableCabins.Select(row => (int)row.ReferenceId).Order().ToList(),
                specification.RequiresExitRowEligibility,
                specification.RequiresAdjacentSeat,
                specification.ExtraSeatPurpose,
                specification.ExtraOccupiedSeatCount,
                application?.SeatNumbers.Select(row => row.SeatNumber).Order(StringComparer.Ordinal).ToList() ?? []);
        }
    }
}
