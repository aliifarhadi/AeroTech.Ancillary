using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeGroupedAirportServiceSelectedDto(
        int? AirportId,
        long? FacilityRef,
        DateTimeOffset? TimeWithOffset,
        int? GuestCount) : BackofficeGroupedSelectedDto;
}
