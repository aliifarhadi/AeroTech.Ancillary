using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionFareApplicationArgs(
        IReadOnlyList<long> AirFareIds,
        IReadOnlyList<AirFareType> AirFareTypes,
        IReadOnlyList<long> FareFamilyIds,
        IReadOnlyList<string> FareBasisCodes,
        IReadOnlyList<int> CabinClassIds,
        IReadOnlyList<long> RbdIds)
    {
        public bool IsEmpty
            => AirFareIds.Count == 0
               && AirFareTypes.Count == 0
               && FareFamilyIds.Count == 0
               && FareBasisCodes.Count == 0
               && CabinClassIds.Count == 0
               && RbdIds.Count == 0;
    }
}
