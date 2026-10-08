using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision
{
    public interface IChangeAncillaryProvisionCommand
    {
        long ProvisionId { get; }

        int Sequence { get; }

        DateTimeOffset? SalesEffectiveFrom { get; }

        DateTimeOffset? SalesDiscontinueAt { get; }

        ServiceCoverageScope CoverageScope { get; }

        ProvisionPassengerCriteriaInput? Passenger { get; }

        ProvisionSalesCriteriaInput? Sales { get; }

        ProvisionTravelCriteriaInput? Travel { get; }

        ProvisionFareCriteriaInput? Fare { get; }

        ProvisionAdvancePurchaseInput? AdvancePurchase { get; }

        ProvisionQuantityInput Quantity { get; }

        ProvisionApplicationInput Application { get; }

        ProvisionOutcomeInput Outcome { get; }

        ProvisionSettlementInput Settlement { get; }

        ProvisionAvailabilityInput Availability { get; }

        ProvisionFulfillmentInput Fulfillment { get; }
    }
}
