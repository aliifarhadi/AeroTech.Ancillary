using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services
{
    public sealed record VerifiedHoldSelection(
        long ServiceDefinitionId,
        ServiceCoverageScope CoverageScope,
        int Quantity,
        AncillaryReservationSelectionArgs? Selection);
}
