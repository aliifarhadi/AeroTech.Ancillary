using System.Globalization;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Providers._Shared;
using AeroTech.Ancillary.Providers.Ordering.Wire;
using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Providers.Ordering.Services
{
    public static class OrderingAncillaryContextMapper
    {
        public static OrderCommercialContext ToDomain(OrderingAncillaryContext source, long requestedOrderId, DateTimeOffset readAt)
        {
            Require(source.OrderId == requestedOrderId, requestedOrderId, nameof(source.OrderId));
            Require(source.CurrencyId > 0, requestedOrderId, nameof(source.CurrencyId));
            Require(source.AncillaryServices.Count == 0, requestedOrderId, "ancillary services are not supported yet");

            var segments = source.Journeys
                .SelectMany(journey => journey.Segments.Select(segment => new SoldSegment(journey, segment)))
                .ToList();

            Require(source.Journeys.Count > 0 && source.Journeys.All(journey => journey.Segments.Count > 0), requestedOrderId, nameof(source.Journeys));
            Unique(source.Journeys.Select(journey => journey.BoundId), requestedOrderId, nameof(OrderingJourney.BoundId));
            Unique(segments.Select(row => Ref(row.Segment.SegmentId)), requestedOrderId, nameof(OrderingSegment.SegmentId));
            Unique(source.Travellers.Select(traveller => Ref(traveller.TravellerId)), requestedOrderId, nameof(OrderingTraveller.TravellerId));
            Unique(source.AirServices.Select(service => Ref(service.OrderServiceId)), requestedOrderId, nameof(OrderingAirService.OrderServiceId));
            Unique(source.AirServices.Select(service => $"{service.TravellerId}:{service.SegmentId}"), requestedOrderId, nameof(source.AirServices));

            var travellerIds = source.Travellers.Select(traveller => traveller.TravellerId).ToHashSet();
            var segmentById = segments.ToDictionary(row => row.Segment.SegmentId);
            var servicesBySegment = source.AirServices.ToLookup(service => service.SegmentId);

            Require(source.Travellers.All(traveller => traveller.InfantParentTravellerId is not { } parent || travellerIds.Contains(parent)), requestedOrderId, nameof(OrderingTraveller.InfantParentTravellerId));
            Require(source.AirServices.All(service => travellerIds.Contains(service.TravellerId)), requestedOrderId, nameof(OrderingAirService.TravellerId));
            Require(source.AirServices.All(service => segmentById.ContainsKey(service.SegmentId)), requestedOrderId, nameof(OrderingAirService.SegmentId));

            var air = new FlightOffer(
                Ref(source.OrderId),
                readAt,
                null,
                source.CustomerId,
                source.CurrencyId,
                null,
                0,
                0,
                0,
                source.Journeys.OrderBy(journey => journey.Sequence).Select(journey => MapBound(journey, servicesBySegment, requestedOrderId)).ToList(),
                source.AirServices
                    .Where(service => service.AirFareId is not null)
                    .Select(service => MapFareComponent(service, segmentById[service.SegmentId].Journey, requestedOrderId))
                    .Distinct()
                    .ToList(),
                source.Travellers
                    .Select((traveller, index) => MapTicket(traveller, index + 1, source.AirServices, segmentById, requestedOrderId))
                    .ToList(),
                [],
                [],
                null,
                null);

            return new OrderCommercialContext(
                source.OrderId,
                source.CommercialVersion,
                source.TicketedAt?.ToUniversalTime(),
                source.Sales?.TravelAgencyId,
                air,
                source.Travellers
                    .Select(traveller => new OrderTravellerFacts(
                        Ref(traveller.TravellerId),
                        traveller.TravellerId,
                        traveller.DateOfBirth,
                        traveller.InfantParentTravellerId is { } parent ? Ref(parent) : null))
                    .ToList(),
                []);
        }

        private static FlightOfferBound MapBound(OrderingJourney journey, ILookup<long, OrderingAirService> servicesBySegment, long orderId)
            => new(
                journey.BoundId,
                journey.Sequence,
                null,
                journey.OriginAirportId,
                journey.DestinationAirportId,
                journey.Segments.OrderBy(segment => segment.Sequence).Select(segment => MapFlight(segment, servicesBySegment[segment.SegmentId], orderId)).ToList());

        private static FlightOfferFlight MapFlight(OrderingSegment segment, IEnumerable<OrderingAirService> services, long orderId)
        {
            var sold = services
                .Select(service => (service.FlightCapacityId, CabinClassId: CabinClass(service.CabinClassId, orderId), service.RbdId, BookingClass: Text(service.BookingClass)))
                .Distinct()
                .ToList();

            Require(sold.Count == 1 && sold[0].FlightCapacityId > 0, orderId, nameof(OrderingAirService.FlightCapacityId));

            return new FlightOfferFlight(
                segment.Sequence,
                sold[0].FlightCapacityId,
                segment.FlightId,
                segment.FlightVersion,
                Text(segment.FlightNumber),
                segment.OriginAirportId,
                segment.OriginAirportTerminalId,
                segment.DestinationAirportId,
                segment.DestinationAirportTerminalId,
                segment.OperatingAirlineId,
                segment.MarketingAirlineId,
                segment.DepartureAt,
                segment.ArrivalAt,
                segment.AircraftId,
                sold[0].CabinClassId,
                sold[0].RbdId,
                sold[0].BookingClass,
                segment.Legs
                    .OrderBy(leg => leg.Sequence)
                    .Select(leg => new FlightOfferLeg(leg.Sequence, leg.LegId, leg.OriginAirportId, leg.DestinationAirportId, leg.DepartureAt, leg.ArrivalAt))
                    .ToList());
        }

        private static FlightOfferFareComponent MapFareComponent(OrderingAirService service, OrderingJourney journey, long orderId)
            => new(
                service.AirFareId!.Value,
                journey.BoundId,
                CabinClass(service.CabinClassId, orderId),
                service.RbdId,
                Text(service.BookingClass),
                Text(service.FareBasis),
                Text(service.FareFamily),
                WireEnum.Named<AirFareType>(service.FareType));

        private static FlightOfferTicket MapTicket(
            OrderingTraveller traveller,
            int travellerIndex,
            IReadOnlyList<OrderingAirService> services,
            IReadOnlyDictionary<long, SoldSegment> segmentById,
            long orderId)
        {
            var passengerType = WireEnum.Named<PassengerTypeCode>(traveller.PassengerTypeCode);

            Require(passengerType is not null, orderId, nameof(OrderingTraveller.PassengerTypeCode));

            return new FlightOfferTicket(
                Ref(traveller.TravellerId),
                travellerIndex,
                passengerType!.Value,
                services
                    .Where(service => service.TravellerId == traveller.TravellerId)
                    .Select(service => (Service: service, Sold: segmentById[service.SegmentId]))
                    .OrderBy(row => row.Sold.Journey.Sequence)
                    .ThenBy(row => row.Sold.Segment.Sequence)
                    .Select(row => MapCoupon(row.Service, row.Sold))
                    .ToList());
        }

        private static FlightOfferCoupon MapCoupon(OrderingAirService service, SoldSegment sold)
            => new(
                sold.Journey.BoundId,
                sold.Segment.FlightId,
                Baggage(service.CheckedBaggageAllowance),
                Baggage(service.CabinBaggageAllowance),
                service.IsRefundable,
                service.IsChangeable,
                service.IsUpgradable,
                service.AirFareId is { } airFareId ? [airFareId] : [],
                [])
            {
                IsOpenForUse = service.CouponOpenForUse
            };

        private static FlightOfferBaggage? Baggage(OrderingBaggage? baggage)
            => baggage is not null && WireEnum.Named<WeightUnit>(baggage.Unit) is { } unit && baggage.Pieces >= 0 && baggage.Weight >= 0
                ? new FlightOfferBaggage(baggage.Pieces, baggage.Weight, unit)
                : null;

        private static int? CabinClass(long? cabinClassId, long orderId)
        {
            Require(cabinClassId is null or (> 0 and <= int.MaxValue), orderId, nameof(OrderingAirService.CabinClassId));

            return (int?)cabinClassId;
        }

        private static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

        private static string Ref(long id) => id.ToString(CultureInfo.InvariantCulture);

        private static void Unique(IEnumerable<string> values, long orderId, string field)
        {
            var list = values.ToList();

            Require(list.All(value => !string.IsNullOrWhiteSpace(value)) && list.Distinct(StringComparer.Ordinal).Count() == list.Count, orderId, field);
        }

        private static void Require(bool condition, long orderId, string field)
        {
            if (!condition)
                throw ExceptionFactory.OrderSourceUnavailable(orderId, field);
        }

        private sealed record SoldSegment(OrderingJourney Journey, OrderingSegment Segment);
    }
}
