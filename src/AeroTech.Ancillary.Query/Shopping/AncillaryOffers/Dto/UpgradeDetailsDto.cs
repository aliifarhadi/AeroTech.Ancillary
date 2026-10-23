using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record UpgradeDetailsDto(
        int FromCabinId,
        int ToCabinId,
        UpgradeKind AllowedUpgradeKind,
        bool RequiresTicketExchange,
        IReadOnlyList<long> EligibleFareFamilyIds);
}
