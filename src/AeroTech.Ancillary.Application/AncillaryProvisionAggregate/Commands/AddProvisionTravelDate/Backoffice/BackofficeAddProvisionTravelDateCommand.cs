using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate.Backoffice
{
    public sealed record BackofficeAddProvisionTravelDateCommand(
        long ProvisionId,
        DateOnly TravelDate) : IRequest<ProvisionConditionRowResult>, IAddProvisionTravelDateCommand;
}
