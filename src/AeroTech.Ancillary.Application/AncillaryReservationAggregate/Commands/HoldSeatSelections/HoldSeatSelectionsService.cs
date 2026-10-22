using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections
{
    public sealed class HoldSeatSelectionsService : IHoldSeatSelectionsService
    {
        private readonly ISeatSelectionVerifier _verifier;
        private readonly IHoldAncillaryServicesService _holds;

        public HoldSeatSelectionsService(ISeatSelectionVerifier verifier, IHoldAncillaryServicesService holds)
        {
            _verifier = verifier;
            _holds = holds;
        }

        public async Task<AncillaryHoldResult> HoldAsync(IHoldSeatSelectionsCommand command, CancellationToken cancellationToken = default)
        {
            if (command.Seats.Select(seat => seat.SeatMapSelectionId).Distinct(StringComparer.Ordinal).Count() != command.Seats.Count)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid(nameof(SeatHoldSelectionInput.SeatMapSelectionId));

            var verified = await _verifier.VerifyAsync(
                new SeatSelectionVerification(command.OrderId, command.OrderCommercialVersion, command.PointOfSaleId, command.CurrencyId, command.TicketedAtUtc, command.Seats),
                cancellationToken);

            return await _holds.HoldAsync(new SeatHold(command, verified), cancellationToken);
        }

        private sealed class SeatHold : IHoldAncillaryServicesCommand
        {
            public SeatHold(IHoldSeatSelectionsCommand command, IReadOnlyList<VerifiedSeatSelection> seats)
            {
                IdempotencyKey = command.IdempotencyKey;
                OrderId = command.OrderId;
                Reference = command.Reference;
                RequestedExpiresAt = command.RequestedExpiresAt;
                Services = seats
                    .Select(seat => new AncillaryHoldServiceInput(
                        seat.OrderServiceId,
                        seat.ServiceDefinitionId,
                        seat.ProvisionId,
                        seat.TravellerId,
                        ServiceCoverageScope.Sector,
                        [seat.FlightId],
                        1,
                        seat.SeatMapSelectionId,
                        seat.SeatNumber,
                        seat.PricingId))
                    .ToList();
            }

            public string IdempotencyKey { get; }

            public long OrderId { get; }

            public string Reference { get; }

            public DateTimeOffset? RequestedExpiresAt { get; }

            public IReadOnlyList<AncillaryHoldServiceInput> Services { get; }
        }
    }
}
