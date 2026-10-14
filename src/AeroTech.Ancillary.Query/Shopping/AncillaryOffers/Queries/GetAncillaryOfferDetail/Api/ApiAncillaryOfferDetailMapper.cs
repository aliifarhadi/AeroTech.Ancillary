using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail.Api
{
    public static class ApiAncillaryOfferDetailMapper
    {
        public const string AncillaryOffer = "ancillary-offer";

        public static ApiAncillaryOfferDetailDto Map(AncillaryOfferDetailSnapshot detail, ApiOfferReferences references)
        {
            var candidate = detail.Item.Candidate;
            var bag = ApiAncillaryOffersMapper.IsBag(candidate);

            return new ApiAncillaryOfferDetailDto(
                new ApiAncillaryOfferDetailDataDto(
                    AncillaryOffer,
                    detail.Item.OfferItemRef,
                    detail.Shopping.Offer.OfferId,
                    bag ? ApiAncillaryOffersMapper.Bag(candidate, references) : null,
                    bag ? null : ApiAncillaryOffersMapper.OtherService(candidate, references),
                    ApiServiceSpecificationMapper.ToDto(detail.Specification),
                    detail.IncludedAllowance.Select(fact => ApiOfferDetailSectionsMapper.Allowance(fact, references)).ToList(),
                    ApiOfferDetailSectionsMapper.BaggageRule(detail.BaggageRule),
                    ApiOfferDetailSectionsMapper.SeatRule(detail.SeatRule),
                    ApiOfferDetailSectionsMapper.Restrictions(detail.Restrictions),
                    ApiOfferDetailSectionsMapper.Terms(detail.Terms),
                    detail.PublishedRates.Select(rate => ApiOfferDetailSectionsMapper.Rate(rate, references)).ToList()));
        }
    }
}
