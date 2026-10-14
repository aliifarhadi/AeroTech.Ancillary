using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail.Backoffice
{
    public static class BackofficeAncillaryOfferDetailMapper
    {
        public static BackofficeAncillaryOfferDetailDto Map(AncillaryOfferDetailSnapshot detail)
            => new(
                detail.Shopping.Offer.OfferId,
                BackofficeAncillaryOfferMapper.Map(detail.Item),
                ServiceSpecificationMapper.ToDto(detail.Specification),
                detail.IncludedAllowance.Select(BackofficeOfferDetailSectionsMapper.Allowance).ToList(),
                BackofficeOfferDetailSectionsMapper.BaggageRule(detail.BaggageRule),
                BackofficeOfferDetailSectionsMapper.SeatRule(detail.SeatRule),
                BackofficeOfferDetailSectionsMapper.Restrictions(detail.Restrictions),
                BackofficeOfferDetailSectionsMapper.Terms(detail.Terms),
                detail.PublishedRates.Select(BackofficeOfferDetailSectionsMapper.Rate).ToList());
    }
}
