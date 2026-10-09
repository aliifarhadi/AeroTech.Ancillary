using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto
{
    public sealed record CustomerSelectionContractDto(
        EnumValueDto Kind,
        bool ZeroQuantityMeansNoSelection,
        IReadOnlyList<CustomerSelectionFieldDto> Fields);

    public sealed record CustomerSelectionFieldDto(
        string Name,
        EnumValueDto Type,
        bool Required);
}
