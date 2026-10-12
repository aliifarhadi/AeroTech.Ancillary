using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto
{
    public sealed record CabinUpgradeSpecificationDto(
        int FromCabinId,
        int ToCabinId,
        UpgradeKind AllowedUpgradeKind,
        bool RequiresTicketExchange,
        IReadOnlyList<long> EligibleFareFamilyIds);

    public sealed record PriorityBoardingCheckinSpecificationDto(
        PriorityKind Kind,
        string? PriorityZoneCode,
        string? PriorityGroupCode,
        string? FareBenefitRef,
        IReadOnlyList<int> AirportIds);

    public sealed record OnboardWifiSpecificationDto(
        ConnectivityPlanKind PlanKind,
        int? DurationMinutes,
        int? IncludedDataMb,
        int? MaxDevices,
        IReadOnlyList<int> EligibleAircraftIds,
        PurchaseStage DeliveryStage,
        string? FulfillmentProviderRef);
}
