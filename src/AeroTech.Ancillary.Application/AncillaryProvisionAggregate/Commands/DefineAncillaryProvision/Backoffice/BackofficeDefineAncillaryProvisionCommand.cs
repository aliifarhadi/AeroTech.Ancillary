using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice
{
    public sealed record BackofficeDefineAncillaryProvisionCommand(
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
        ProvisionFulfillmentInput Fulfillment) : IRequest<ProvisionResult>, IDefineAncillaryProvisionCommand;
}
