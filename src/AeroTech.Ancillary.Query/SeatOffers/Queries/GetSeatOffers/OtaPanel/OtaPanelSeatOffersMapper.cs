using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.SeatOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.OtaPanel
{
    public static class OtaPanelSeatOffersMapper
    {
        public static OtaPanelSeatOffersDto Map(AncillaryShoppingSnapshot snapshot)
        {
            var offer = snapshot.Offer;
            var flightRefs = offer.Bounds.SelectMany(bound => bound.Flights.Select(flight => FlightOfferShoppingContextMapper.FlightRef(bound, flight))).ToList();
            var shown = snapshot.Items.Where(item => OtaPanelAncillaryOffersMapper.DisplayStatusOf(item.Candidate.OfferReadiness) is not null).ToList();

            return new OtaPanelSeatOffersDto(
                offer.OfferId,
                snapshot.CurrencyId,
                snapshot.CurrencyId == offer.CurrencyId ? offer.CurrencyCode : null,
                offer.Tickets
                    .Select(ticket => new OtaPanelSeatTravellerGroupDto(
                        ticket.TravellerRef,
                        ticket.PassengerTypeCode.ToString(),
                        offer.Bounds.SelectMany(bound => bound.Flights.Select(flight => FlightGroup(ticket, bound, flight, flightRefs, shown))).ToList()))
                    .ToList(),
                snapshot.Items.Count() - shown.Count);
        }

        public static OtaPanelSeatOfferDetailDto Detail(AncillaryOfferDetailSnapshot detail)
        {
            var candidate = detail.Item.Candidate;
            var price = candidate.Price;

            return new OtaPanelSeatOfferDetailDto(
                detail.Shopping.Offer.OfferId,
                candidate.CandidateIdentity.TravellerRef,
                Offer(detail.Item.OfferItemRef, candidate),
                new OtaPanelPriceBreakdownDto(
                    OtaPanelAncillaryOffersMapper.Money(price.BaseAmount, price.CurrencyId),
                    price.AddedTaxLines.Select(OtaPanelAncillaryOffersMapper.Fee).ToList(),
                    price.IncludedTaxLines.Select(OtaPanelAncillaryOffersMapper.Fee).ToList(),
                    price.AppliedUnitFeeLines.Select(OtaPanelAncillaryOffersMapper.Fee).ToList(),
                    price.UnappliedFeeLines.Select(OtaPanelAncillaryOffersMapper.Fee).ToList(),
                    OtaPanelAncillaryOffersMapper.Money(price.CompleteUnitTotal, price.CurrencyId)),
                OtaPanelServiceSpecificationMapper.ToDto(detail.Specification.Seat!),
                OtaPanelOfferDetailSectionsMapper.SeatRule(detail.SeatRule),
                OtaPanelOfferDetailSectionsMapper.Restrictions(detail.Restrictions),
                OtaPanelOfferDetailSectionsMapper.Terms(detail.Terms),
                detail.PublishedRates.Select(OtaPanelOfferDetailSectionsMapper.Rate).ToList());
        }

        public static OtaPanelSeatSelectionResultDto Selection(AncillarySelectionSnapshot selection)
            => new(
                selection.Shopping.Offer.OfferId,
                selection.Evaluation.Status.ToString(),
                selection.Item.OfferItemRef,
                selection.CurrentOfferItemRef,
                selection.Evaluation.ProvisionChangedBySelection,
                Offer(selection.CurrentOfferItemRef, selection.Evaluation.Candidate),
                OtaPanelAncillaryOffersMapper.Money(selection.Evaluation.Candidate.Price.RequestedQuantityTotal, selection.Evaluation.Candidate.Price.CurrencyId),
                selection.Evaluation.FieldIssues.Select(issue => new OtaPanelInputIssueDto(issue.Field, issue.ReasonCode)).ToList(),
                false);

        public static OtaPanelSeatOfferDto Offer(string offerItemRef, CanonicalAncillaryOfferCandidate candidate)
            => new(
                offerItemRef,
                candidate.VariantCode,
                candidate.CommercialName,
                candidate.Description,
                candidate.Scope.CoverageScope.ToString(),
                candidate.FlightRefs,
                candidate.Price.Status.ToString(),
                candidate.Price.PricingUnit?.ToString(),
                OtaPanelAncillaryOffersMapper.Money(candidate.Price.CompleteUnitTotal, candidate.Price.CurrencyId),
                candidate.Price.UnappliedFeeLines.Select(OtaPanelAncillaryOffersMapper.Fee).ToList(),
                candidate.Quantity.MinPerSelection,
                candidate.Quantity.MaxPerSelection,
                candidate.Selection.Fields.Where(field => field.Required).Select(field => field.Name).ToList(),
                Terms(candidate.Seat!),
                OtaPanelAncillaryOffersMapper.DisplayStatusOf(candidate.OfferReadiness) ?? candidate.OfferReadiness.ToString(),
                false);

        private static OtaPanelSeatTermsDto Terms(SeatOfferTerms seat)
            => new(
                seat.Purpose.ToString(),
                seat.CharacteristicCodes,
                seat.ApplicableCabinIds,
                seat.RequiresExitRowEligibility,
                seat.RequiresAdjacentSeat,
                seat.ExtraSeatPurpose?.ToString(),
                seat.ExtraOccupiedSeatCount,
                seat.SeatNumbers);

        private static OtaPanelSeatFlightGroupDto FlightGroup(
            FlightOfferTicket ticket,
            FlightOfferBound bound,
            FlightOfferFlight flight,
            IReadOnlyList<string> flightRefs,
            IReadOnlyList<AncillaryOfferItem> shown)
        {
            var flightRef = FlightOfferShoppingContextMapper.FlightRef(bound, flight);

            return new OtaPanelSeatFlightGroupDto(
                flightRef,
                bound.BoundId,
                flight.FlightNumber,
                flight.OriginAirportId,
                flight.DestinationAirportId,
                flight.DepartureAt,
                shown
                    .Where(item => item.Candidate.CandidateIdentity.TravellerRef == ticket.TravellerRef
                                   && flightRefs.First(candidateFlight => item.Candidate.FlightRefs.Contains(candidateFlight)) == flightRef)
                    .OrderBy(item => item.Candidate.ServiceDefinitionRef, StringComparer.Ordinal)
                    .ThenBy(item => item.PointOfSaleId)
                    .Select(item => Offer(item.OfferItemRef, item.Candidate))
                    .ToList());
        }
    }
}
