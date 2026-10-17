using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto
{
    public sealed record BackofficeProfileProvisionFormDto<TForm>(
        long Id,
        long ServiceDefinitionId,
        EnumValueDto Status,
        EnumValueDto Profile,
        string VariantCode,
        bool Editable,
        IReadOnlyList<string> AllowedRuleSections,
        ProvisionConstraintsDto Constraints,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ActivatedAt,
        DateTimeOffset? SuspendedAt,
        DateTimeOffset? RetiredAt,
        TForm Form);
}
