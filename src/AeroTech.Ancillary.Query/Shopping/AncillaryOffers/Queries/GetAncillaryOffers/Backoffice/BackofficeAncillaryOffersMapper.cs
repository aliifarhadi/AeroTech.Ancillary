using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice
{
    public static class BackofficeAncillaryOffersMapper
    {
        public const string NoApplicableProduct = "NO_APPLICABLE_PRODUCT";
        public const string NoAuthorizedPointOfSale = "NO_AUTHORIZED_POINT_OF_SALE";

        public static BackofficeAncillaryOffersDto Map(AncillaryShoppingSnapshot snapshot)
        {
            var offer = snapshot.Offer;
            var scope = snapshot.Scope;
            var offers = snapshot.Items.Where(item => BackofficeAncillaryOfferMapper.IsDiscoverable(item.Candidate.OfferReadiness)).ToList();
            var blocked = snapshot.Items.Where(item => !BackofficeAncillaryOfferMapper.IsDiscoverable(item.Candidate.OfferReadiness)).ToList();

            return new BackofficeAncillaryOffersDto(
                offer.OfferId,
                snapshot.EvaluatedAtUtc,
                new OfferCurrencyDto(snapshot.CurrencyId, snapshot.CurrencyId == offer.CurrencyId ? offer.CurrencyCode : null),
                new FlightOfferSummaryDto(offer.LastTicketingDate, offer.PricedAt, offer.CurrencyId, offer.BaseAmount, offer.ChargeAmount, offer.TotalAmount),
                new OfferScopeDto(
                    scope.OwnerAirlineId,
                    snapshot.PointsOfSale.Count == 1 ? snapshot.PointsOfSale[0].PointOfSaleId : null,
                    snapshot.PointsOfSale.Select(pointOfSale => pointOfSale.PointOfSaleId).ToList(),
                    EnumValueDto.Of(FlightOfferShoppingContextMapper.Stage)),
                new OfferJourneyDto(offer.Bounds.Select(Portion).ToList()),
                offer.Tickets.Select(ticket => Traveller(ticket, snapshot.Facts)).ToList(),
                Categories(offers.Select(item => item.Candidate).ToList()),
                offers.Select(BackofficeAncillaryOfferMapper.Map).ToList(),
                blocked.Select(BackofficeAncillaryOfferMapper.Map).ToList(),
                Diagnostics(snapshot),
                RatesOfExchange(snapshot),
                new OfferCompletenessDto(
                    true,
                    snapshot.PointsOfSale.Count > 0 && snapshot.PointsOfSale.All(pointOfSale => IsComplete(pointOfSale.Context)),
                    blocked.Count > 0,
                    false));
        }

        private static OfferPortionDto Portion(FlightOfferBound bound)
            => new(
                bound.BoundId,
                bound.Sequence,
                EnumValueDto.OfNullable(bound.Direction),
                bound.OriginAirportId,
                bound.DestinationAirportId,
                bound.Flights
                    .Select(flight => new OfferFlightDto(
                        FlightOfferShoppingContextMapper.FlightRef(bound, flight),
                        flight.FlightId,
                        flight.FlightVersion,
                        flight.FlightNumber,
                        flight.MarketingAirlineId,
                        flight.OperatingAirlineId,
                        flight.OriginAirportId,
                        flight.DestinationAirportId,
                        flight.DepartureAt,
                        flight.ArrivalAt,
                        flight.CabinClassId,
                        flight.RbdId,
                        flight.AircraftId,
                        flight.Legs
                            .Select(leg => new OfferFlightLegDto(leg.Sequence, leg.OriginAirportId, leg.DestinationAirportId, leg.DepartureAt, leg.ArrivalAt))
                            .ToList()))
                    .ToList());

        private static OfferTravellerDto Traveller(FlightOfferTicket ticket, FlightOfferFacts facts)
            => new(
                ticket.TravellerRef,
                ticket.TravellerIndex,
                EnumValueDto.Of(ticket.PassengerTypeCode),
                facts.Baggage
                    .Where(fact => fact.TravellerRef == ticket.TravellerRef)
                    .Select(fact => new OfferIncludedBaggageDto(
                        fact.FlightRef,
                        fact.CheckedPieces,
                        fact.CheckedWeight,
                        EnumValueDto.OfNullable(fact.CheckedWeightUnit),
                        fact.CabinPieces,
                        fact.CabinWeight,
                        EnumValueDto.OfNullable(fact.CabinWeightUnit)))
                    .ToList(),
                facts.Fares
                    .Where(fact => fact.TravellerRef == ticket.TravellerRef)
                    .Select(fact => new OfferFareDto(
                        fact.FlightRef,
                        fact.AirFareId,
                        fact.FareBasisCode,
                        fact.FareFamilyCodeOrName,
                        fact.FareFamilyId,
                        fact.CabinClassId,
                        fact.RbdId))
                    .ToList());

        private static IReadOnlyList<OfferCategoryDto> Categories(IReadOnlyList<CanonicalAncillaryOfferCandidate> offers)
            => offers
                .GroupBy(candidate => candidate.Profile)
                .OrderBy(group => group.Key)
                .Select(group => new OfferCategoryDto(EnumValueDto.Of(group.Key), EnumValueDto.Of(group.Key).Title, group.Count()))
                .ToList();

        private static IReadOnlyList<OfferRateOfExchangeDto> RatesOfExchange(AncillaryShoppingSnapshot snapshot)
            => snapshot.PointsOfSale
                .SelectMany(pointOfSale => pointOfSale.Result.RatesOfExchange)
                .DistinctBy(rate => rate.RateOfExchangeTableId, StringComparer.Ordinal)
                .OrderBy(rate => rate.FromCurrencyId)
                .Select(rate => BackofficeAncillaryOfferMapper.RateOfExchange(rate)!)
                .ToList();

        private static IReadOnlyList<OfferDiagnosticDto> Diagnostics(AncillaryShoppingSnapshot snapshot)
        {
            var diagnostics = snapshot.PointsOfSale
                .SelectMany(pointOfSale => pointOfSale.Result.Diagnostics)
                .DistinctBy(diagnostic => (diagnostic.Code, diagnostic.Severity, string.Join('|', diagnostic.MissingContextFields), diagnostic.SourceAuthority))
                .Select(diagnostic => new OfferDiagnosticDto(
                    diagnostic.Code,
                    EnumValueDto.Of(diagnostic.Severity),
                    diagnostic.MissingContextFields,
                    diagnostic.SourceAuthority))
                .ToList();

            if (snapshot.PointsOfSale.Count == 0)
                diagnostics.Add(new OfferDiagnosticDto(NoAuthorizedPointOfSale, EnumValueDto.Of(ShoppingDiagnosticSeverity.Info), [], null));
            else if (!snapshot.Items.Any())
                diagnostics.Add(new OfferDiagnosticDto(NoApplicableProduct, EnumValueDto.Of(ShoppingDiagnosticSeverity.Info), [], null));

            return diagnostics;
        }

        private static bool IsComplete(AncillaryShoppingContext context)
            => context.CoverageCompleteness is
               {
                   TravellersComplete: true,
                   ItineraryComplete: true,
                   FareFactsComplete: true,
                   BaggageFactsComplete: true,
                   FareBenefitsComplete: true,
                   ExistingServicesComplete: true
               }
               && context.Flights.All(flight => flight is { OriginIanaTimeZoneId: not null, OriginCountryId: not null, DestinationCountryId: not null })
               && context.Travellers.All(traveller => traveller.DateOfBirth is not null || traveller.VerifiedAgeAtTravel is not null);
    }
}
