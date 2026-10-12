using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm
{
    public static class SingleVariantSpecificationMapper
    {
        public static CabinUpgradeSpecificationDto ToCabinUpgrade(AncillaryServiceDefinition definition)
        {
            var upgrade = definition.Upgrade!.ToArgs();

            return new CabinUpgradeSpecificationDto(
                upgrade.FromCabinId,
                upgrade.ToCabinId,
                upgrade.AllowedUpgradeKind,
                upgrade.RequiresTicketExchange,
                upgrade.EligibleFareFamilyIds);
        }

        public static PriorityBoardingCheckinSpecificationDto ToPriorityBoardingCheckin(AncillaryServiceDefinition definition)
        {
            var priority = definition.Priority!.ToArgs();

            return new PriorityBoardingCheckinSpecificationDto(
                priority.Kind,
                priority.PriorityZoneCode,
                priority.PriorityGroupCode,
                priority.FareBenefitRef,
                priority.AirportIds);
        }

        public static OnboardWifiSpecificationDto ToOnboardWifi(AncillaryServiceDefinition definition)
        {
            var connectivity = definition.Connectivity!.ToArgs();

            return new OnboardWifiSpecificationDto(
                connectivity.PlanKind,
                connectivity.DurationMinutes,
                connectivity.IncludedDataMb,
                connectivity.MaxDevices,
                connectivity.EligibleAircraftIds,
                connectivity.DeliveryStage,
                connectivity.FulfillmentProviderRef);
        }
    }
}
