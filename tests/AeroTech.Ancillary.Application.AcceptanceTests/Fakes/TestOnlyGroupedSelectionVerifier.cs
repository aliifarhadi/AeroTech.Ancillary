using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fakes;

public sealed class TestOnlyGroupedSelectionVerifier : IGroupedSelectionVerifier
{
    public Dictionary<string, (long DefinitionId, long ProvisionId, long TravellerId, ServiceCoverageScope CoverageScope, long FlightId, int Quantity)> Accepted { get; } = [];

    public List<GroupedSelectionVerification> Verified { get; } = [];

    public Task<IReadOnlyList<VerifiedGroupedSelection>> VerifyAsync(GroupedSelectionVerification verification, CancellationToken cancellationToken = default)
    {
        Verified.Add(verification);

        return Task.FromResult<IReadOnlyList<VerifiedGroupedSelection>>(verification.Selections
            .Select(entry => Accepted.TryGetValue(entry.SelectionId, out var service)
                ? new VerifiedGroupedSelection(entry.OrderServiceId, entry.SelectionId, entry.Profile, service.DefinitionId, service.ProvisionId, service.TravellerId, service.CoverageScope, [service.FlightId], service.Quantity)
                : throw ExceptionFactory.AncillaryHoldRequestIsInvalid($"{entry.Profile}[{entry.OrderServiceId}].SelectionId:AcceptedSelectionId"))
            .ToList());
    }
}
