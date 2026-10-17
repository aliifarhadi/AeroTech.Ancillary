using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto
{
    public sealed record BackofficeProfileProvisionCreateFormDto(
        long ServiceDefinitionId,
        string VariantCode,
        EnumValueDto Profile,
        EnumValueDto DefinitionStatus,
        EnumValueDto PricingUnit,
        EnumValueDto ServiceDateBasis,
        EnumValueDto ApplicationType,
        IReadOnlyList<string> AllowedRuleSections,
        ProvisionConstraintsDto Constraints);

    public sealed record ProvisionConstraintsDto(
        EnumValueDto QuantityUnit,
        int? MaxQuantity,
        IReadOnlyList<EnumValueDto> AllowedDispositions,
        IReadOnlyList<EnumValueDto> AllowedPriceOrigins,
        IReadOnlyList<EnumValueDto> AllowedPurchaseStages);
}
