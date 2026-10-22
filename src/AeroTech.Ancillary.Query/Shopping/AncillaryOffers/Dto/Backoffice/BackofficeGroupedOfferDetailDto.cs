using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeGroupedOfferDetailDto(
        string OfferItemRef,
        string VariantCode,
        EnumValueDto Readiness,
        BackofficeGroupedQuantityDto? Quantity,
        IReadOnlyList<BackofficeGroupedRequiredFieldDto> RequiredFields,
        IReadOnlyList<BackofficeGroupedOptionGroupDto> Options,
        BackofficeGroupedSelectedDto? Selected,
        BackofficeGroupedPriceDto Price,
        string? SelectionId,
        IReadOnlyList<string> ReasonCodes);
}
