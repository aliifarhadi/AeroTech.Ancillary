using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeGroupedConnectivitySelectedDto(
        string? PlanCode,
        int? DeviceCount) : BackofficeGroupedSelectedDto;
}
