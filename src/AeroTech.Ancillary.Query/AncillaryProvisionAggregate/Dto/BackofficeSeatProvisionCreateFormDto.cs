using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto
{
    public sealed record BackofficeSeatProvisionCreateFormDto(
        long ServiceDefinitionId,
        string VariantCode,
        EnumValueDto Profile,
        EnumValueDto DefinitionStatus,
        EnumValueDto PricingUnit,
        EnumValueDto ServiceDateBasis,
        EnumValueDto ApplicationType,
        IReadOnlyList<string> AllowedRuleSections,
        SeatProvisionConstraintsDto Constraints);

    public sealed record SeatProvisionConstraintsDto(
        EnumValueDto QuantityUnit,
        int? MaxQuantity,
        bool RequiresSeatNumberOrCharacteristic,
        bool SeatNumberFilterRequiresAircraft,
        int PublishedPointOfSaleCount,
        bool RequiresPreferredCharacteristic);
}
