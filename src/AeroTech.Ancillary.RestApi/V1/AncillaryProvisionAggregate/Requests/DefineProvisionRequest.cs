using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Requests
{
    public sealed record DefineProvisionRequest(
        long ServiceDefinitionId,
        int Sequence,
        DateTimeOffset? SalesEffectiveFrom,
        DateTimeOffset? SalesDiscontinueAt,
        ServiceCoverageScope CoverageScope,
        ProvisionQuantityInput Quantity,
        ProvisionApplicationInput Application,
        ProvisionOutcomeInput Outcome,
        ProvisionFeeInput? Fee,
        ProvisionSettlementInput Settlement,
        ProvisionAvailabilityInput Availability,
        ProvisionFulfillmentInput Fulfillment);
}
