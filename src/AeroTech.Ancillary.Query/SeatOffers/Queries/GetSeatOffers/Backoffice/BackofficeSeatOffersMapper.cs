using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.Backoffice
{
    public static class BackofficeSeatOffersMapper
    {
        public static BackofficeSeatOffersDto Map(AncillaryShoppingSnapshot snapshot)
        {
            var offer = snapshot.Offer;
            var scope = snapshot.Scope;
            var offers = snapshot.Items.Where(item => BackofficeAncillaryOfferMapper.IsDiscoverable(item.Candidate.OfferReadiness)).ToList();
            var blocked = snapshot.Items.Where(item => !BackofficeAncillaryOfferMapper.IsDiscoverable(item.Candidate.OfferReadiness)).ToList();

            return new BackofficeSeatOffersDto(
                offer.OfferId,
                scope.SalesInstantUtc,
                new OfferCurrencyDto(snapshot.CurrencyId, snapshot.CurrencyId == offer.CurrencyId ? offer.CurrencyCode : null),
                new OfferScopeDto(
                    scope.OwnerAirlineId,
                    snapshot.PointsOfSale.Count == 1 ? snapshot.PointsOfSale[0].PointOfSaleId : null,
                    snapshot.PointsOfSale.Select(pointOfSale => pointOfSale.PointOfSaleId).ToList(),
                    EnumValueDto.Of(FlightOfferShoppingContextMapper.Stage)),
                offers.Select(Offer).ToList(),
                blocked.Select(Offer).ToList(),
                BackofficeAncillaryOffersMapper.Diagnostics(snapshot),
                BackofficeAncillaryOffersMapper.RatesOfExchange(snapshot));
        }

        public static BackofficeSeatOfferDetailDto Detail(AncillaryOfferDetailSnapshot detail)
            => new(
                detail.Shopping.Offer.OfferId,
                Offer(detail.Item),
                ServiceSpecificationMapper.ToDto(detail.Specification.Seat!),
                BackofficeOfferDetailSectionsMapper.SeatRule(detail.SeatRule),
                BackofficeOfferDetailSectionsMapper.Restrictions(detail.Restrictions),
                BackofficeOfferDetailSectionsMapper.Terms(detail.Terms),
                detail.PublishedRates.Select(BackofficeOfferDetailSectionsMapper.Rate).ToList());

        public static BackofficeSeatSelectionResultDto Selection(AncillarySelectionSnapshot selection)
            => new(
                selection.Shopping.Offer.OfferId,
                EnumValueDto.Of(selection.Evaluation.Status),
                selection.Item.OfferItemRef,
                selection.CurrentOfferItemRef,
                selection.Evaluation.ProvisionChangedBySelection,
                Offer(selection.CurrentOfferItemRef, selection.Item.PointOfSaleId, selection.Evaluation.Candidate),
                selection.Evaluation.Candidate.Price.RequestedQuantityTotal,
                selection.Evaluation.FieldIssues.Select(issue => new BackofficeFieldIssueDto(issue.Field, issue.ReasonCode)).ToList(),
                false);

        public static BackofficeSeatOfferDto Offer(AncillaryOfferItem item) => Offer(item.OfferItemRef, item.PointOfSaleId, item.Candidate);

        public static BackofficeSeatOfferDto Offer(string offerItemRef, long pointOfSaleId, CanonicalAncillaryOfferCandidate candidate)
            => new(
                offerItemRef,
                pointOfSaleId,
                new OfferCandidateIdentityDto(
                    candidate.CandidateIdentity.DefinitionVersionId,
                    candidate.CandidateIdentity.ProvisionId,
                    EnumValueDto.Of(candidate.CandidateIdentity.CoverageScope),
                    candidate.CandidateIdentity.TravellerRef,
                    candidate.CandidateIdentity.CoverageRef,
                    candidate.CandidateIdentity.PricingRevisionId),
                new OfferServiceDto(
                    candidate.ServiceDefinitionRef,
                    candidate.DefinitionVersion,
                    EnumValueDto.Of(candidate.Profile),
                    candidate.VariantCode,
                    candidate.CommercialName,
                    candidate.Description,
                    candidate.ServiceTypeCode,
                    candidate.ServiceSubCode,
                    candidate.Document.Rfic,
                    candidate.Document.Rfisc),
                new OfferCoverageDto(
                    EnumValueDto.Of(candidate.Scope.CoverageScope),
                    candidate.Scope.TravellerRefs,
                    candidate.Scope.FlightRefs,
                    candidate.Scope.PortionRefs),
                new OfferEligibilityDto(EnumValueDto.Of(candidate.Eligibility.Status), candidate.Eligibility.ReasonCodes, candidate.Eligibility.MissingContextFields),
                new CustomerSelectionContractDto(
                    EnumValueDto.Of(candidate.Selection.Kind),
                    candidate.Selection.ZeroQuantityMeansNoSelection,
                    candidate.Selection.Fields.Select(field => new CustomerSelectionFieldDto(field.Name, EnumValueDto.Of(field.Type), field.Required)).ToList()),
                BackofficeAncillaryOfferMapper.Quantity(candidate.Quantity),
                BackofficeAncillaryOfferMapper.Pricing(candidate.Price),
                Terms(candidate.Seat!),
                new OfferConfirmationDto(
                    EnumValueDto.Of(candidate.Confirmation.BookingMethod),
                    EnumValueDto.Of(candidate.Confirmation.ConfirmationRequirement),
                    EnumValueDto.Of(candidate.Confirmation.ProviderConfirmationStatus)),
                new OfferDocumentDto(
                    EnumValueDto.Of(candidate.Document.Routing),
                    EnumValueDto.Of(candidate.Document.DocumentType),
                    candidate.Document.Rfic,
                    candidate.Document.Rfisc,
                    candidate.Document.DocumentRequired),
                EnumValueDto.Of(candidate.OfferReadiness),
                EnumValueDto.Of(BackofficeAncillaryOfferMapper.DisplayStateOf(candidate.OfferReadiness)),
                false,
                candidate.ReasonCodes);

        private static BackofficeSeatTermsDto Terms(SeatOfferTerms seat)
            => new(
                EnumValueDto.Of(seat.Purpose),
                seat.CharacteristicCodes,
                seat.ApplicableCabinIds,
                seat.RequiresExitRowEligibility,
                seat.RequiresAdjacentSeat,
                EnumValueDto.OfNullable(seat.ExtraSeatPurpose),
                seat.ExtraOccupiedSeatCount,
                seat.SeatNumbers);
    }
}
