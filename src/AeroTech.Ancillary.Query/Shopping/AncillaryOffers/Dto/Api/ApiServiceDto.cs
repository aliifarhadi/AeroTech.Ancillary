namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api
{
    public sealed record ApiServiceDto(
        string ServiceDefinitionRef,
        string CommercialName,
        string? Description,
        string Category,
        string VariantCode,
        string ServiceTypeCode,
        string ServiceSubCode,
        string? Rfic,
        string? Rfisc,
        string DocumentType,
        string BookingMethod,
        string? SsrCode);
}
