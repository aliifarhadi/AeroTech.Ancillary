using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionBlackoutPeriod.Backoffice
{
    public sealed record BackofficeAddProvisionBlackoutPeriodCommand(
        long ProvisionId,
        DateOnly StartDate,
        DateOnly EndDate) : IRequest<ProvisionConditionRowResult>, IAddProvisionBlackoutPeriodCommand;
}
