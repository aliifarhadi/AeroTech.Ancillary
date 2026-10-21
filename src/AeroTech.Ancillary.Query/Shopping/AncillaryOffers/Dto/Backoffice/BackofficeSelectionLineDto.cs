using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeSelectionLineDto(
        string ClientSelectionId,
        EnumValueDto Status,
        IReadOnlyList<string> ReasonCodes,
        bool CanPurchase,
        string? PricedSelectionRef,
        DateTimeOffset? ValidUntilUtc,
        BackofficeSelectionResultDto? Ancillary);
}
