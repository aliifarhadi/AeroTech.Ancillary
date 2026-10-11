using AeroTech.Ancillary.Query.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.FlightOffers;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers
{
    public static class AncillaryOffersMapper
    {
        public const string NoApplicableProduct = "NO_APPLICABLE_PRODUCT";

        public static AncillaryOffersDto Map(FlightOffer offer, AncillaryShoppingContext context, CanonicalAncillaryOfferResult result)
        {
            var offers = result.Candidates.Where(candidate => AncillaryOfferMapper.IsDiscoverable(candidate.OfferReadiness)).ToList();
            var blocked = result.Candidates.Where(candidate => !AncillaryOfferMapper.IsDiscoverable(candidate.OfferReadiness)).ToList();

            return new AncillaryOffersDto(
                offer.OfferId,
                result.EvaluatedAtUtc,
                new OfferCurrencyDto(context.CurrencyId, context.CurrencyId == offer.CurrencyId ? offer.CurrencyCode : null),
                new FlightOfferSummaryDto(offer.LastTicketingDate, offer.PricedAt, offer.CurrencyId, offer.BaseAmount, offer.ChargeAmount, offer.TotalAmount),
                new OfferScopeDto(result.OwnerAirlineId, result.PointOfSaleId, EnumValueDto.Of(context.ShoppingStage)),
                new OfferJourneyDto(offer.Bounds.Select(Portion).ToList()),
                offer.Tickets.Select(ticket => Traveller(ticket, context)).ToList(),
                Categories(offers),
                offers.Select(AncillaryOfferMapper.Map).ToList(),
                blocked.Select(AncillaryOfferMapper.Map).ToList(),
                Diagnostics(result),
                new OfferCompletenessDto(true, IsComplete(context), blocked.Count > 0, false));
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

        private static OfferTravellerDto Traveller(FlightOfferTicket ticket, AncillaryShoppingContext context)
            => new(
                ticket.TravellerRef,
                ticket.TravellerIndex,
                EnumValueDto.Of(ticket.PassengerTypeCode),
                context.TravellerBaggageFacts
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
                context.TravellerFareFacts
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

        private static IReadOnlyList<OfferDiagnosticDto> Diagnostics(CanonicalAncillaryOfferResult result)
        {
            var diagnostics = result.Diagnostics
                .Select(diagnostic => new OfferDiagnosticDto(
                    diagnostic.Code,
                    EnumValueDto.Of(diagnostic.Severity),
                    diagnostic.MissingContextFields,
                    diagnostic.SourceAuthority))
                .ToList();

            if (result.Candidates.Count == 0)
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
