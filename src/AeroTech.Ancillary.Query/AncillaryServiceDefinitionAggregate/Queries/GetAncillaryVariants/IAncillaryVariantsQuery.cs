using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryVariants
{
    public interface IAncillaryVariantsQuery
    {
        AncillaryProfile? Profile { get; }
    }
}
