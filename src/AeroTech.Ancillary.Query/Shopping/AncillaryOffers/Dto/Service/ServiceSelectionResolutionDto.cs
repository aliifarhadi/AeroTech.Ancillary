namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service
{
    public sealed record ServiceSelectionResolutionDto(
        long OrderId,
        int? OrderCommercialVersion,
        string? CorrelationId,
        bool AllValid,
        IReadOnlyList<ServiceResolvedSelectionDto> Results);
}
