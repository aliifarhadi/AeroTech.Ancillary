using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryQuote;
using AeroTech.Ancillary.Domain.ServiceReservationAggregate;
using AeroTech.Ancillary.Domain.ServiceReservationAggregate.Arguments;
using AeroTech.Ancillary.Domain.ServiceReservationAggregate.Contracts;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation
{
    public sealed class ReserveServiceReservationService : IReserveServiceReservationService
    {
        private readonly IServiceReservationRepository _reservations;
        private readonly IAncillaryProductRepository _products;
        private readonly IAncillaryPriceRuleRepository _priceRules;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdGenerator _idGenerator;
        private readonly IClock _clock;

        public ReserveServiceReservationService(
            IServiceReservationRepository reservations,
            IAncillaryProductRepository products,
            IAncillaryPriceRuleRepository priceRules,
            IUnitOfWork unitOfWork,
            IIdGenerator idGenerator,
            IClock clock)
        {
            _reservations = reservations;
            _products = products;
            _priceRules = priceRules;
            _unitOfWork = unitOfWork;
            _idGenerator = idGenerator;
            _clock = clock;
        }

        public async Task<ServiceReservationResult> ReserveAsync(IReserveServiceReservationCommand command, CancellationToken cancellationToken = default)
        {
            var now = _clock.GetDateTime();
            var stored = await _reservations.FindByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);

            if (stored is not null)
                return Replayed(stored, command, now);

            ServiceReservation.EnsureReservable(command.ExpiresAt, command.Units.Select(unit => unit.UnitReference).ToList(), now);

            var products = await _products.ListActiveAsync(cancellationToken);
            var priceRules = await _priceRules.ListActiveAsync(command.Context.CurrencyId, cancellationToken);
            var result = AncillaryQuoteEvaluator.Evaluate(ToRequest(command), products, priceRules);
            var flightIds = command.Context.Bounds
                .SelectMany(bound => bound.Flights)
                .ToDictionary(flight => flight.Ref, flight => flight.FlightId, StringComparer.Ordinal);

            var units = command.Units
                .Zip(result.Items, (unit, item) => new ServiceReservationUnitArgs(
                    unit.UnitReference,
                    item.Product.OwnerAirlineId,
                    item.Product.ProductRef,
                    item.Product.Version,
                    item.PriceRuleId,
                    item.TravellerRef,
                    item.BoundRef,
                    item.FlightRef,
                    item.CoveredFlightRefs.Select(flightRef => flightIds[flightRef]).ToList(),
                    item.Quantity,
                    result.CurrencyId,
                    item.Total,
                    item.Product.InventoryControl))
                .ToList();

            var reservation = ServiceReservation.Reserve(
                _idGenerator.NewId(),
                command.IdempotencyKey,
                command.Reference,
                command.ExpiresAt,
                units,
                _idGenerator,
                now);

            await _reservations.AddAsync(reservation, cancellationToken);

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                var winner = await _reservations.FindByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);

                if (winner is null)
                    throw;

                return Replayed(winner, command, now);
            }

            return reservation.ToResult(now);
        }

        private static ServiceReservationResult Replayed(ServiceReservation stored, IReserveServiceReservationCommand command, DateTimeOffset now)
        {
            stored.EnsureSameContent(
                command.Reference,
                command.ExpiresAt,
                command.Units
                    .Select(unit => new ServiceReservationUnitSelection(
                        unit.UnitReference,
                        unit.ProductRef,
                        unit.ProductVersion,
                        unit.PriceRuleId,
                        unit.TravellerRef,
                        unit.BoundRef,
                        unit.FlightRef,
                        unit.Quantity))
                    .ToList());

            return stored.ToResult(now);
        }

        private static AncillaryQuoteRequest ToRequest(IReserveServiceReservationCommand command)
            => new(
                command.Context.CurrencyId,
                command.Context.AsOf,
                command.Context.Travellers
                    .Select(traveller => new AncillaryQuoteTraveller(traveller.Ref, traveller.PassengerTypeCode, traveller.FlightRefs))
                    .ToList(),
                command.Context.Bounds
                    .Select(bound => new AncillaryQuoteBound(
                        bound.Ref,
                        bound.Flights
                            .Select(flight => new AncillaryQuoteFlight(
                                flight.Ref,
                                flight.OriginAirportId,
                                flight.DestinationAirportId,
                                flight.DepartureDateTime,
                                flight.MarketingAirlineId))
                            .ToList()))
                    .ToList(),
                (command.Context.Existing ?? [])
                    .Select(existing => new AncillaryQuoteExistingOccurrence(
                        existing.ProductRef,
                        existing.TravellerRef,
                        existing.BoundRef,
                        existing.FlightRef,
                        existing.Quantity))
                    .ToList(),
                command.Units
                    .Select(unit => new AncillaryQuoteSelection(
                        unit.ProductRef,
                        unit.ProductVersion,
                        unit.PriceRuleId,
                        unit.TravellerRef,
                        unit.BoundRef,
                        unit.FlightRef,
                        unit.Quantity))
                    .ToList());
    }
}
