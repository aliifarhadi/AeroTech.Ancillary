using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections
{
    public sealed class HoldGroupedSelectionsService : IHoldGroupedSelectionsService
    {
        public const int MaximumUnits = 50;

        private readonly ISeatSelectionVerifier _seats;
        private readonly IGroupedSelectionVerifier _selections;
        private readonly IHoldAncillaryServicesService _holds;
        private readonly IAncillaryReservationRepository _reservations;
        private readonly IClock _clock;

        public HoldGroupedSelectionsService(
            ISeatSelectionVerifier seats,
            IGroupedSelectionVerifier selections,
            IHoldAncillaryServicesService holds,
            IAncillaryReservationRepository reservations,
            IClock clock)
        {
            _seats = seats;
            _selections = selections;
            _holds = holds;
            _reservations = reservations;
            _clock = clock;
        }

        public async Task<GroupedHoldResult> HoldAsync(IHoldGroupedSelectionsCommand command, CancellationToken cancellationToken = default)
        {
            var entries = Entries(command);

            if (command.ExtraSeats.Count > 0)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid(nameof(IHoldGroupedSelectionsCommand.ExtraSeats));

            if (command.Seats.Count + entries.Count is < 1 or > MaximumUnits)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid(nameof(IHoldGroupedSelectionsCommand.Seats));

            if (command.Seats.Select(seat => seat.OrderServiceId).Concat(entries.Select(entry => entry.OrderServiceId)).Distinct().Count() != command.Seats.Count + entries.Count)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid(nameof(GroupedHoldSelectionInput.OrderServiceId));

            if (command.Seats.Select(seat => seat.SeatMapSelectionId).Distinct(StringComparer.Ordinal).Count() != command.Seats.Count)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid(nameof(SeatHoldSelectionInput.SeatMapSelectionId));

            if (entries.Select(entry => entry.SelectionId).Distinct(StringComparer.Ordinal).Count() != entries.Count)
                throw ExceptionFactory.AncillaryHoldRequestIsInvalid(nameof(GroupedHoldSelectionInput.SelectionId));

            if (await _reservations.FindByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken) is { } stored)
                return Project(Replayed(stored, command, entries), entries);

            var seats = command.Seats.Count == 0
                ? []
                : await _seats.VerifyAsync(
                    new SeatSelectionVerification(command.OrderId, command.OrderCommercialVersion, command.PointOfSaleId, command.CurrencyId, command.TicketedAtUtc, command.Seats),
                    cancellationToken);
            var selections = entries.Count == 0
                ? []
                : await _selections.VerifyAsync(
                    new GroupedSelectionVerification(command.OrderId, command.OrderCommercialVersion, command.PointOfSaleId, command.CurrencyId, command.TicketedAtUtc, entries),
                    cancellationToken);
            var units = seats
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
                .Concat(selections.Select(selection => new AncillaryReservationUnitArgs(
                    selection.OrderServiceId,
                    selection.ServiceDefinitionId,
                    selection.ProvisionId,
                    selection.TravellerId,
                    selection.CoverageScope,
                    selection.CoveredFlightIds,
                    selection.Quantity,
                    SelectionId: selection.SelectionId)))
                .ToList();
            var hold = await _holds.HoldAsync(
                new AncillaryHoldHeader(command.IdempotencyKey, command.OrderId, command.Reference, command.RequestedExpiresAt),
                units,
                cancellationToken);

            return Project(hold, entries);
        }

        private AncillaryHoldResult Replayed(AncillaryReservation stored, IHoldGroupedSelectionsCommand command, IReadOnlyList<GroupedHoldSelectionEntry> entries)
        {
            var same = stored.OrderId == command.OrderId
                       && stored.Reference == command.Reference
                       && stored.RequestedExpiresAt == command.RequestedExpiresAt
                       && stored.Units.Count == command.Seats.Count + entries.Count
                       && command.Seats.All(seat => stored.Units.Any(unit => unit.OrderServiceId == seat.OrderServiceId && string.Equals(unit.SeatMapSelectionId, seat.SeatMapSelectionId, StringComparison.Ordinal)))
                       && entries.All(entry => stored.Units.Any(unit => unit.OrderServiceId == entry.OrderServiceId && string.Equals(unit.SelectionId, entry.SelectionId, StringComparison.Ordinal)));

            return same ? stored.ToResult(_clock.GetDateTime()) : throw ExceptionFactory.AncillaryHoldIdempotencyKeyReused();
        }

        private static IReadOnlyList<GroupedHoldSelectionEntry> Entries(IHoldGroupedSelectionsCommand command)
            => Of(AncillaryProfile.Baggage, command.Baggages)
                .Concat(Of(AncillaryProfile.Meal, command.Meals))
                .Concat(Of(AncillaryProfile.Pet, command.Pets))
                .Concat(Of(AncillaryProfile.AssistedTravel, command.AssistedTravel))
                .Concat(Of(AncillaryProfile.AirportService, command.AirportServices))
                .Concat(Of(AncillaryProfile.Priority, command.PriorityServices))
                .Concat(Of(AncillaryProfile.Connectivity, command.Connectivity))
                .Concat(Of(AncillaryProfile.Upgrade, command.Upgrades))
                .Concat(Of(AncillaryProfile.Seat, command.ExtraSeats))
                .ToList();

        private static IEnumerable<GroupedHoldSelectionEntry> Of(AncillaryProfile profile, IReadOnlyList<GroupedHoldSelectionInput> inputs)
            => inputs.Select(input => new GroupedHoldSelectionEntry(profile, input.OrderServiceId, input.SelectionId));

        private static GroupedHoldResult Project(AncillaryHoldResult hold, IReadOnlyList<GroupedHoldSelectionEntry> entries)
        {
            var byOrderService = entries.ToDictionary(entry => entry.OrderServiceId);

            IReadOnlyList<GroupedHoldUnitResult> Family(AncillaryProfile profile)
                => hold.Units
                    .Where(unit => unit.SeatMapSelectionId is null && byOrderService.TryGetValue(unit.OrderServiceId, out var entry) && entry.Profile == profile)
                    .Select(unit => new GroupedHoldUnitResult(unit.UnitId, unit.OrderServiceId, unit.Status, byOrderService[unit.OrderServiceId].SelectionId))
                    .ToList();

            return new GroupedHoldResult(
                hold.HoldId,
                hold.IdempotencyKey,
                hold.OrderId,
                hold.Reference,
                hold.Status,
                hold.ExpiresAt,
                hold.Units
                    .Where(unit => unit.SeatMapSelectionId is not null)
                    .Select(unit => new GroupedHoldSeatResult(unit.UnitId, unit.OrderServiceId, unit.Status, unit.SeatMapSelectionId!, unit.SeatNumber!))
                    .ToList(),
                Family(AncillaryProfile.Baggage),
                Family(AncillaryProfile.Meal),
                Family(AncillaryProfile.Pet),
                Family(AncillaryProfile.AssistedTravel),
                Family(AncillaryProfile.AirportService),
                Family(AncillaryProfile.Priority),
                Family(AncillaryProfile.Connectivity),
                Family(AncillaryProfile.Upgrade),
                Family(AncillaryProfile.Seat));
        }
    }
}
