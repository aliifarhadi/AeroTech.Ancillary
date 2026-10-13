using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Api;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.Api
{
    public static class ApiSeatOffersMapper
    {
        public const string SeatOffers = "seat-offers";
        public const string SeatOffer = "seat-offer";
        public const string SeatSelection = "seat-selection";
        public const string Seats = "SEATS";

        public static ApiSeatOffersDto Map(AncillaryShoppingSnapshot snapshot, ApiOfferReferences references)
        {
            var items = snapshot.Items.Where(item => ApiAncillaryOffersMapper.IsOffered(item.Candidate.OfferReadiness)).ToList();

            return new ApiSeatOffersDto(
                new ApiMetaDto(
                    items.Count,
                    snapshot.Items.Count() - items.Count,
                    items.Count > 0 ? ApiAncillaryOffersMapper.NeedsSelectionOrRecheck : ApiAncillaryOffersMapper.NoApplicableOffer),
                new ApiSeatOffersDataDto(SeatOffers, snapshot.Offer.OfferId, references.Travelers, references.Segments),
                new ApiSeatIncludedDto(items.ToDictionary(item => item.OfferItemRef, item => Offer(item.Candidate, references), StringComparer.Ordinal)));
        }

        public static ApiSeatOfferDetailDto Detail(AncillaryOfferDetailSnapshot detail, ApiOfferReferences references)
            => new(
                new ApiSeatOfferDetailDataDto(
                    SeatOffer,
                    detail.Item.OfferItemRef,
                    detail.Shopping.Offer.OfferId,
                    Offer(detail.Item.Candidate, references),
                    ApiServiceSpecificationMapper.ToDto(detail.Specification.Seat!),
                    ApiOfferDetailSectionsMapper.SeatRule(detail.SeatRule),
                    ApiOfferDetailSectionsMapper.Restrictions(detail.Restrictions),
                    ApiOfferDetailSectionsMapper.Terms(detail.Terms),
                    detail.PublishedRates.Select(rate => ApiOfferDetailSectionsMapper.Rate(rate, references)).ToList()));

        public static ApiSeatSelectionResultDto Selection(AncillarySelectionSnapshot selection, ApiOfferReferences references)
        {
            var candidate = selection.Evaluation.Candidate;

            return new ApiSeatSelectionResultDto(
                new ApiSeatSelectionResultDataDto(
                    SeatSelection,
                    selection.Shopping.Offer.OfferId,
                    selection.Evaluation.Status.ToString(),
                    selection.Item.OfferItemRef,
                    selection.CurrentOfferItemRef,
                    selection.Evaluation.ProvisionChangedBySelection,
                    Offer(candidate, references),
                    ApiAncillaryOffersMapper.Price(candidate.Price.RequestedQuantityTotal, candidate.Price.CurrencyId, references),
                    selection.Evaluation.FieldIssues.Select(issue => new ApiSelectionIssueDto(issue.Field, issue.ReasonCode)).ToList(),
                    false));
        }

        public static ApiSeatOfferDto Offer(CanonicalAncillaryOfferCandidate candidate, ApiOfferReferences references)
            => new(
                ApiAncillaryOffersMapper.Name(candidate),
                ApiAncillaryOffersMapper.Price(candidate.Price.CompleteUnitTotal, candidate.Price.CurrencyId, references),
                candidate.Travellers.Count == 1,
                candidate.Scope.CoverageScope == ServiceCoverageScope.Journey,
                references.SegmentIds(candidate.FlightRefs),
                references.TravelerIds(candidate.Travellers),
                Terms(candidate.Seat!),
                new ApiSeatAncillaryDto(
                    Seats,
                    ApiAncillaryOffersMapper.Service(candidate),
                    ApiAncillaryOffersMapper.Pricing(candidate.Price, references),
                    ApiAncillaryOffersMapper.Quantity(candidate),
                    ApiAncillaryOffersMapper.SelectionRequirements(candidate),
                    candidate.Confirmation.ConfirmationRequirement.ToString(),
                    candidate.OfferReadiness.ToString(),
                    false,
                    candidate.ReasonCodes));

        private static ApiSeatTermsDto Terms(SeatOfferTerms seat)
            => new(
                seat.Purpose.ToString(),
                seat.CharacteristicCodes,
                seat.ApplicableCabinIds,
                seat.RequiresExitRowEligibility,
                seat.RequiresAdjacentSeat,
                seat.ExtraSeatPurpose?.ToString(),
                seat.ExtraOccupiedSeatCount,
                seat.SeatNumbers);
    }
}
