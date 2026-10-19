using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots
{
    public sealed record OfferOptionGroup(string FieldName, OptionSourceState SourceState, IReadOnlyList<OfferOptionChoice> Choices);
}
