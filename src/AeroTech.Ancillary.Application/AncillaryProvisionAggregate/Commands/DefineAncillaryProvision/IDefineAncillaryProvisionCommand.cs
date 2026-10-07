using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision
{
    public interface IDefineAncillaryProvisionCommand
    {
        long ServiceDefinitionId { get; }

        int Sequence { get; }

        DateTimeOffset? SalesEffectiveFrom { get; }

        DateTimeOffset? SalesDiscontinueAt { get; }

        ServiceCoverageScope CoverageScope { get; }

        ProvisionQuantityInput Quantity { get; }

        ProvisionApplicationInput Application { get; }

        ProvisionOutcomeInput Outcome { get; }

        ProvisionFeeInput? Fee { get; }

        ProvisionSettlementInput Settlement { get; }

        ProvisionAvailabilityInput Availability { get; }

        ProvisionFulfillmentInput Fulfillment { get; }
    }
}
