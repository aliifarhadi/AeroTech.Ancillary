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
        ProvisionQuantityInput Quantity,
        ProvisionApplicationInput Application,
        ProvisionOutcomeInput Outcome,
        ProvisionFeeInput? Fee,
        ProvisionSettlementInput Settlement,
        ProvisionAvailabilityInput Availability,
        ProvisionFulfillmentInput Fulfillment) : IRequest<ProvisionResult>, IDefineAncillaryProvisionCommand;
}
