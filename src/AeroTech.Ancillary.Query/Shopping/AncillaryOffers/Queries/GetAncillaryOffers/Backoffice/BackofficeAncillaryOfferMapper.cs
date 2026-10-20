using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice
{
    public static class BackofficeAncillaryOfferMapper
    {
        public static AncillaryOfferDisplayState DisplayStateOf(OfferReadiness readiness)
            => readiness switch
            {
                OfferReadiness.Selectable => AncillaryOfferDisplayState.AvailableForSelection,
                OfferReadiness.NeedsSelection => AncillaryOfferDisplayState.NeedsInput,
                OfferReadiness.NeedsQuote => AncillaryOfferDisplayState.QuoteRequired,
                OfferReadiness.NeedsVerification => AncillaryOfferDisplayState.NeedsVerification,
                OfferReadiness.Unavailable => AncillaryOfferDisplayState.Unavailable,
                OfferReadiness.NotEligible => AncillaryOfferDisplayState.NotEligible,
                _ => throw new ArgumentOutOfRangeException(nameof(readiness), readiness, null)
            };

        public static bool IsDiscoverable(OfferReadiness readiness)
            => readiness is OfferReadiness.Selectable or OfferReadiness.NeedsSelection or OfferReadiness.NeedsQuote;

        public static BackofficeAncillaryOfferDto Map(AncillaryOfferItem item) => Map(item.OfferItemRef, item.PointOfSaleId, item.Candidate, item.OptionGroups);

        public static BackofficeAncillaryOfferDto Map(string offerItemRef, long pointOfSaleId, CanonicalAncillaryOfferCandidate candidate, IReadOnlyList<OfferOptionGroup> optionGroups)
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
                Quantity(candidate.Quantity),
                Pricing(candidate.Price),
                Availability(candidate.Availability),
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
                EnumValueDto.Of(DisplayStateOf(candidate.OfferReadiness)),
                OfferPurchase.CanPurchase(candidate),
                candidate.ReasonCodes,
                OfferInlineOptions.Groups(offerItemRef, candidate, optionGroups, refs => refs, refs => refs),
                OfferInlineOptions.Interaction(candidate, optionGroups));

        private static OfferQuantityDto Quantity(QuantityAssessment quantity)
            => new(
                EnumValueDto.Of(quantity.Unit),
                quantity.MinPerSelection,
                quantity.MaxPerSelection,
                quantity.CumulativeLimits
                    .Select(limit => new OfferCumulativeLimitDto(
                        limit.CountingFamilyCode,
                        EnumValueDto.Of(limit.LimitScope),
                        limit.MaxUnits,
                        EnumValueDto.Of(limit.ConsumptionUnit),
                        limit.UnitsPerPurchase,
                        limit.FamilyVerified,
                        limit.VerifiedConsumed,
                        limit.VerifiedRemaining))
                    .ToList());

        private static OfferPricingDto Pricing(PriceAssessment price)
            => new(
                EnumValueDto.Of(price.Origin),
                EnumValueDto.Of(price.Status),
                price.CurrencyId,
                price.FiledCurrencyId,
                RateOfExchange(price.RateOfExchange),
                EnumValueDto.OfNullable(price.PricingUnit),
                price.BaseAmount,
                price.CompleteUnitTotal,
                price.AddedTaxLines.Select(Line).ToList(),
                price.IncludedTaxLines.Select(Line).ToList(),
                price.AppliedUnitFeeLines.Select(Line).ToList(),
                price.UnappliedFeeLines.Select(Line).ToList(),
                price.IsOrderLevelTotalComplete,
                price.QuoteProviderKey,
                price.ReasonCodes);

        public static OfferRateOfExchangeDto? RateOfExchange(PublishedRateOfExchange? rate)
            => rate is null ? null : new OfferRateOfExchangeDto(rate.RateOfExchangeTableId, rate.FromCurrencyId, rate.ToCurrencyId, rate.Rate);

        private static OfferPriceLineDto Line(PriceComponentLine line)
            => new(line.Code, line.Amount, line.CurrencyId, line.CountryId, line.StationAirportId, EnumValueDto.OfNullable(line.FeeApplicationUnit));

        private static OfferAvailabilityDto Availability(AvailabilityAssessment availability)
            => new(
                EnumValueDto.OfNullable(availability.InventoryAuthority),
                EnumValueDto.Of(availability.ConfigState),
                EnumValueDto.Of(availability.CheckedState),
                availability.IsGuaranteed,
                availability.RequiresCheckAtReserve,
                availability.ConsumptionPreview is { } preview
                    ? new OfferConsumptionPreviewDto(
                        preview.CountUnits,
                        EnumValueDto.OfNullable(preview.CountUnit),
                        preview.WeightKg,
                        preview.OccupancyMinutes,
                        preview.OccupiedPersons)
                    : null,
                availability.ReasonCodes);
    }
}
