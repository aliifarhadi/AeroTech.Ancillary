namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetSelectionOptions
{
    public interface ISelectionOptionsQuery
    {
        string Family { get; }

        string Reference { get; }

        string? OfferId { get; }
    }
}
