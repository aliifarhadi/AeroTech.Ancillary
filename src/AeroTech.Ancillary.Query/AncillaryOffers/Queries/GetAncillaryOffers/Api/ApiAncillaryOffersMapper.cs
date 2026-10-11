using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api
{
    public static class ApiAncillaryOffersMapper
    {
        public const string NeedsSelectionOrRecheck = "NeedsSelectionOrRecheck";
        public const string NoApplicableOffer = "NoApplicableOffer";

        public static bool IsOffered(OfferReadiness readiness)
            => readiness is OfferReadiness.Selectable or OfferReadiness.NeedsSelection or OfferReadiness.NeedsQuote;

        public static ApiAncillaryOffersDto Map(AncillaryShoppingSnapshot snapshot)
        {
            var offered = snapshot.PointsOfSale
                .Select(pointOfSale => pointOfSale.Items.Where(item => IsOffered(item.Candidate.OfferReadiness)).ToList())
                .Where(items => items.Count > 0)
                .ToList();
            var items = offered.SelectMany(group => group).ToList();

            return new ApiAncillaryOffersDto(
                snapshot.Offer.OfferId,
                Currency(snapshot),
                new ApiDataListsDto(
                    snapshot.Offer.Tickets.Select(ticket => new ApiPassengerDto(ticket.TravellerRef, ticket.PassengerTypeCode.ToString())).ToList(),
                    snapshot.Offer.Bounds
                        .SelectMany(bound => bound.Flights.Select(flight => new ApiPaxSegmentDto(
                            FlightOfferShoppingContextMapper.FlightRef(bound, flight),
                            bound.BoundId,
                            flight.FlightId,
                            flight.FlightNumber,
                            flight.MarketingAirlineId,
                            flight.OperatingAirlineId,
                            flight.OriginAirportId,
                            flight.DestinationAirportId,
                            flight.DepartureAt,
                            flight.ArrivalAt,
                            flight.CabinClassId)))
                        .ToList(),
                    items
                        .Select(item => item.Candidate)
                        .DistinctBy(candidate => candidate.ServiceDefinitionRef, StringComparer.Ordinal)
                        .OrderBy(candidate => candidate.ServiceDefinitionRef, StringComparer.Ordinal)
                        .Select(ServiceDefinition)
                        .ToList()),
                offered.Select(group => new ApiALaCarteOfferDto(group.Select(item => OfferItem(item.OfferItemRef, item.Candidate)).ToList())).ToList(),
                snapshot.Items.Count() - items.Count,
                items.Count > 0 ? NeedsSelectionOrRecheck : NoApplicableOffer);
        }

        public static ApiCurrencyDto Currency(AncillaryShoppingSnapshot snapshot)
            => new(snapshot.CurrencyId, snapshot.CurrencyId == snapshot.Offer.CurrencyId ? snapshot.Offer.CurrencyCode : null);

        public static ApiServiceDefinitionDto ServiceDefinition(CanonicalAncillaryOfferCandidate candidate)
            => new(
                candidate.ServiceDefinitionRef,
                candidate.CommercialName,
                candidate.Description,
                candidate.Profile.ToString(),
                candidate.VariantCode,
                candidate.ServiceTypeCode,
                candidate.ServiceSubCode,
                candidate.Document.Rfic,
                candidate.Document.Rfisc,
                candidate.Document.DocumentType.ToString(),
                candidate.Booking.Method.ToString(),
                candidate.Booking.SsrCode);

        public static ApiOfferItemDto OfferItem(string offerItemRef, CanonicalAncillaryOfferCandidate candidate)
            => new(
                offerItemRef,
                candidate.ServiceDefinitionRef,
                new ApiEligibilityDto(candidate.Scope.CoverageScope.ToString(), candidate.Travellers, candidate.FlightRefs, candidate.PortionRefs),
                Pricing(candidate.Price),
                new ApiQuantityDto(candidate.Quantity.Unit.ToString(), candidate.Quantity.MinPerSelection, candidate.Quantity.MaxPerSelection),
                candidate.Selection.Fields.Select(field => new ApiSelectionRequirementDto(field.Name, field.Type.ToString(), field.Required)).ToList(),
                new ApiAvailabilityDto(candidate.Availability.CheckedState.ToString(), candidate.Availability.IsGuaranteed, candidate.Availability.RequiresCheckAtReserve),
                candidate.Confirmation.ConfirmationRequirement.ToString(),
                candidate.OfferReadiness.ToString(),
                false,
                candidate.ReasonCodes);

        public static ApiMoneyDto? Money(decimal? amount, int? currencyId)
            => amount is { } value && currencyId is { } currency ? new ApiMoneyDto(value, currency) : null;

        private static ApiPricingDto Pricing(PriceAssessment price)
            => new(
                price.Origin.ToString(),
                price.Status.ToString(),
                price.PricingUnit?.ToString(),
                Money(price.CompleteUnitTotal, price.CurrencyId),
                Money(price.BaseAmount, price.CurrencyId),
                price.AddedTaxLines.Select(Line).ToList(),
                price.IncludedTaxLines.Select(Line).ToList(),
                price.AppliedUnitFeeLines.Select(Line).ToList(),
                price.UnappliedFeeLines.Select(Line).ToList(),
                Money(price.RequestedQuantityTotal, price.CurrencyId),
                price.IsOrderLevelTotalComplete);

        private static ApiPriceLineDto Line(PriceComponentLine line) => new(line.Code, line.Amount, line.CurrencyId, line.FeeApplicationUnit?.ToString());
    }
}
