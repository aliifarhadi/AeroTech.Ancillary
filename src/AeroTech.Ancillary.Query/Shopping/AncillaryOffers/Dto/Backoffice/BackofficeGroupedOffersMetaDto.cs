using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeGroupedOffersMetaDto(
        string? OfferId,
        long? OrderId,
        int CurrencyId,
        DateTimeOffset EvaluatedAtUtc,
        EnumValueDto ShoppingStage);
}
