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
        ProvisionPassengerCriteriaInput? Passenger,
        ProvisionSalesCriteriaInput? Sales,
        ProvisionTravelCriteriaInput? Travel,
        ProvisionFareCriteriaInput? Fare,
        ProvisionAdvancePurchaseInput? AdvancePurchase,
        ProvisionQuantityInput Quantity,
        ProvisionApplicationInput Application,
        ProvisionOutcomeInput Outcome,
        ProvisionSettlementInput Settlement,
        ProvisionAvailabilityInput Availability,
        ProvisionFulfillmentInput Fulfillment);
}
