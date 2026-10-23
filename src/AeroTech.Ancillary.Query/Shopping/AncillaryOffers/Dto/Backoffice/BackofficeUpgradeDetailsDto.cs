using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeUpgradeDetailsDto(
        int FromCabinId,
        int ToCabinId,
        EnumValueDto AllowedUpgradeKind,
        bool RequiresTicketExchange,
        IReadOnlyList<long> EligibleFareFamilyIds);
}
