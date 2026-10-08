using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionPassengerEligibilityArgs(
        IReadOnlyList<PassengerTypeCode> PassengerTypes,
        IReadOnlyList<ProvisionAgeBandArgs> AgeBands)
    {
        public bool IsEmpty
            => PassengerTypes.Count == 0 && AgeBands.Count == 0;
    }
}
