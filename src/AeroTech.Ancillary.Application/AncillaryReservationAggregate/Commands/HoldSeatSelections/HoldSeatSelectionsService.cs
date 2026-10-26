using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
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

        public async Task<AncillaryHoldReceipt> HoldAsync(IHoldSeatSelectionsCommand command, CancellationToken cancellationToken = default)
        {
            if (command.Seats.Select(seat => seat.SeatMapSelectionId).Distinct(StringComparer.Ordinal).Count() != command.Seats.Count)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid(nameof(SeatHoldSelectionInput.SeatMapSelectionId));

            var header = new AncillaryHoldHeader(command.IdempotencyKey, command.OrderId, command.Reference, command.RequestedExpiresAt);
            var held = await _holds.FindHeldSeatsAsync(
                command.HoldId,
                header,
                command.Seats.Select(seat => new AncillaryReservationSeatSelectionArgs(seat.OrderServiceId, seat.TravellerId, seat.SeatMapSelectionId)).ToList(),
                cancellationToken);

            if (held is not null)
                return held;

            var verified = await _verifier.VerifyAsync(
                new SeatSelectionVerification(command.OrderId, command.OrderCommercialVersion, command.PointOfSaleId, command.CurrencyId, command.TicketedAtUtc, command.Seats),
                cancellationToken);
            var seats = verified
                .Select(seat => new AncillaryReservationUnitArgs(
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

            return command.HoldId is { } holdId
                ? await _holds.AppendSeatsAsync(holdId, header, seats, cancellationToken)
                : await _holds.HoldAsync(header, seats, cancellationToken);
        }
    }
}
