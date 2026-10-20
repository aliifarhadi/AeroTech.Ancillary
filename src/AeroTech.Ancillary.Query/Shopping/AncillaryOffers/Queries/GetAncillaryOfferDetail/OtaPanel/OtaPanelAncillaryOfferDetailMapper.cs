using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail.OtaPanel
{
    public static class OtaPanelAncillaryOfferDetailMapper
    {
        public static OtaPanelAncillaryOfferDetailDto Map(AncillaryOfferDetailSnapshot detail)
        {
            var candidate = detail.Item.Candidate;
            var price = candidate.Price;

            return new OtaPanelAncillaryOfferDetailDto(
                detail.Shopping.Offer.OfferId,
                candidate.CandidateIdentity.TravellerRef,
                OtaPanelAncillaryOffersMapper.Service(detail.Item.OfferItemRef, candidate, detail.OptionGroups),
                new OtaPanelPriceBreakdownDto(
                    OtaPanelAncillaryOffersMapper.Money(price.BaseAmount, price.CurrencyId),
                    price.AddedTaxLines.Select(OtaPanelAncillaryOffersMapper.Fee).ToList(),
                    price.IncludedTaxLines.Select(OtaPanelAncillaryOffersMapper.Fee).ToList(),
                    price.AppliedUnitFeeLines.Select(OtaPanelAncillaryOffersMapper.Fee).ToList(),
                    price.UnappliedFeeLines.Select(OtaPanelAncillaryOffersMapper.Fee).ToList(),
                    OtaPanelAncillaryOffersMapper.Money(price.CompleteUnitTotal, price.CurrencyId)),
                OtaPanelServiceSpecificationMapper.ToDto(detail.Specification),
                detail.IncludedAllowance.Select(OtaPanelOfferDetailSectionsMapper.Allowance).ToList(),
                OtaPanelOfferDetailSectionsMapper.BaggageRule(detail.BaggageRule),
                OtaPanelOfferDetailSectionsMapper.SeatRule(detail.SeatRule),
                OtaPanelOfferDetailSectionsMapper.Restrictions(detail.Restrictions),
                OtaPanelOfferDetailSectionsMapper.Terms(detail.Terms),
                detail.PublishedRates.Select(OtaPanelOfferDetailSectionsMapper.Rate).ToList(),
                OfferSelectionDetailsMapper.SelectionContract(candidate.Selection),
                OfferSelectionDetailsMapper.Quantity(candidate),
                OfferSelectionDetailsMapper.OptionGroups(detail, refs => refs, refs => refs),
                OfferSelectionDetailsMapper.OptionSourceState(detail),
                OfferSelectionDetailsMapper.PriceCompleteness(candidate),
                OfferSelectionDetailsMapper.CommercialTerms(detail.CommercialTerms));
        }
    }
}
