using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto
{
    public sealed record AncillaryVariantDto(
        string Code,
        string Name,
        EnumValueDto Profile,
        EnumValueDto ApplicationType,
        IReadOnlyList<EnumValueDto> PricingUnits,
        IReadOnlyList<EnumValueDto> ServiceDateBases);
}
