namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferPriceCompletenessDto(
        bool IsComplete,
        IReadOnlyList<string> ReasonCodes,
        IReadOnlyList<string> RequiredInputs);
}
