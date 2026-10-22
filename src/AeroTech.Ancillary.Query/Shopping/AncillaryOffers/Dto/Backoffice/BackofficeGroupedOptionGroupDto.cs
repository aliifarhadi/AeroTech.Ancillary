using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeGroupedOptionGroupDto(string FieldName, EnumValueDto SourceState, IReadOnlyList<BackofficeGroupedOptionChoiceDto> Choices);
}
