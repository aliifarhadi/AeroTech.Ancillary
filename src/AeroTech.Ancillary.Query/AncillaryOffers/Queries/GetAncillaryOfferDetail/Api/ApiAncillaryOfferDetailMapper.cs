using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.Api
{
    public static class ApiAncillaryOfferDetailMapper
    {
        public static ApiAncillaryOfferDetailDto Map(AncillaryOfferDetailSnapshot detail)
            => new(
                detail.Shopping.Offer.OfferId,
                ApiAncillaryOffersMapper.Currency(detail.Shopping),
                ApiAncillaryOffersMapper.OfferItem(detail.Item.OfferItemRef, detail.Item.Candidate),
                ApiAncillaryOffersMapper.ServiceDefinition(detail.Item.Candidate),
                ApiServiceSpecificationMapper.ToDto(detail.Specification),
                detail.IncludedAllowance.Select(ApiOfferDetailSectionsMapper.Allowance).ToList(),
                ApiOfferDetailSectionsMapper.BaggageRule(detail.BaggageRule),
                ApiOfferDetailSectionsMapper.SeatRule(detail.SeatRule),
                ApiOfferDetailSectionsMapper.Restrictions(detail.Restrictions),
                ApiOfferDetailSectionsMapper.Terms(detail.Terms),
                detail.PublishedRates.Select(ApiOfferDetailSectionsMapper.Rate).ToList());
    }
}
