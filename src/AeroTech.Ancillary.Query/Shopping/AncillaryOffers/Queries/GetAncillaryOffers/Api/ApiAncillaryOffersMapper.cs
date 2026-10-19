using System.Text.RegularExpressions;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api
{
    public static partial class ApiAncillaryOffersMapper
    {
        public const string AncillaryOffers = "ancillary-offers";
        public const string NeedsSelectionOrRecheck = "NeedsSelectionOrRecheck";
        public const string NoApplicableOffer = "NoApplicableOffer";

        private const string Kilogram = "KG";

        public static bool IsOffered(OfferReadiness readiness)
            => readiness is OfferReadiness.Selectable or OfferReadiness.NeedsSelection or OfferReadiness.NeedsQuote;

        public static bool IsBag(CanonicalAncillaryOfferCandidate candidate) => candidate.Profile == AncillaryProfile.Baggage;

        public static ApiAncillaryOffersDto Map(AncillaryShoppingSnapshot snapshot, ApiOfferReferences references)
        {
            var items = snapshot.Items.Where(item => IsOffered(item.Candidate.OfferReadiness)).ToList();

            return new ApiAncillaryOffersDto(
                new ApiMetaDto(items.Count, snapshot.Items.Count() - items.Count, items.Count > 0 ? NeedsSelectionOrRecheck : NoApplicableOffer),
                new ApiAncillaryOffersDataDto(AncillaryOffers, snapshot.Offer.OfferId, references.Travelers, references.Segments),
                new ApiIncludedDto(
                    items
                        .Where(item => IsBag(item.Candidate))
                        .ToDictionary(item => item.OfferItemRef, item => Bag(item.Candidate, references), StringComparer.Ordinal),
                    items
                        .Where(item => !IsBag(item.Candidate))
                        .ToDictionary(item => item.OfferItemRef, item => OtherService(item.Candidate, references), StringComparer.Ordinal)));
        }

        public static ApiBagDto Bag(CanonicalAncillaryOfferCandidate candidate, ApiOfferReferences references)
        {
            var specification = references.Baggage(candidate.DefinitionVersionId);
            var weight = specification?.PackageWeightKg ?? specification?.WeightToInclusiveKg ?? specification?.MaxKgPerPiece;

            return new ApiBagDto(
                specification is null || specification.ChargeKind == BaggageChargeKind.WeightPackage ? null : 1,
                weight,
                weight is null ? null : Kilogram,
                Name(candidate),
                Price(candidate.Price.CompleteUnitTotal, candidate.Price.CurrencyId, references),
                candidate.Scope.CoverageScope == ServiceCoverageScope.Journey,
                references.SegmentIds(candidate.FlightRefs),
                references.TravelerIds(candidate.Travellers),
                Ancillary(candidate, references));
        }

        public static ApiOtherServiceDto OtherService(CanonicalAncillaryOfferCandidate candidate, ApiOfferReferences references)
            => new(
                Name(candidate),
                Price(candidate.Price.CompleteUnitTotal, candidate.Price.CurrencyId, references),
                candidate.Travellers.Count == 1,
                candidate.Scope.CoverageScope == ServiceCoverageScope.Journey,
                references.SegmentIds(candidate.FlightRefs),
                references.TravelerIds(candidate.Travellers),
                Ancillary(candidate, references));

        public static ApiElementaryPriceDto? Price(decimal? amount, int? currencyId, ApiOfferReferences references)
            => amount is { } value ? new ApiElementaryPriceDto(ApiOfferReferences.Amount(value), references.CurrencyCode(currencyId)) : null;

        private static string Name(CanonicalAncillaryOfferCandidate candidate)
            => Separators().Replace(AncillaryVariant.Find(candidate.VariantCode)!.Name, "_").Trim('_').ToUpperInvariant();

        private static ApiAncillaryDto Ancillary(CanonicalAncillaryOfferCandidate candidate, ApiOfferReferences references)
            => new(
                candidate.Profile switch
                {
                    AncillaryProfile.Baggage => "CHECKED_BAGS",
                    AncillaryProfile.Meal => "MEALS",
                    AncillaryProfile.Seat => "SEATS",
                    _ => "OTHER_SERVICES"
                },
                Service(candidate),
                Pricing(candidate.Price, references),
                Quantity(candidate),
                SelectionRequirements(candidate),
                new ApiAvailabilityDto(candidate.Availability.CheckedState.ToString(), candidate.Availability.IsGuaranteed, candidate.Availability.RequiresCheckAtReserve),
                candidate.Confirmation.ConfirmationRequirement.ToString(),
                candidate.OfferReadiness.ToString(),
                OfferPurchase.CanPurchase(candidate),
                candidate.ReasonCodes,
                candidate.Selection.Kind.ToString(),
                candidate.Scope.CoverageScope.ToString());

        private static ApiServiceDto Service(CanonicalAncillaryOfferCandidate candidate)
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

        private static ApiQuantityDto Quantity(CanonicalAncillaryOfferCandidate candidate)
            => new(candidate.Quantity.Unit.ToString(), candidate.Quantity.MinPerSelection, candidate.Quantity.MaxPerSelection);

        private static IReadOnlyList<ApiSelectionRequirementDto> SelectionRequirements(CanonicalAncillaryOfferCandidate candidate)
            => candidate.Selection.Fields.Select(field => new ApiSelectionRequirementDto(field.Name, field.Type.ToString(), field.Required)).ToList();

        private static ApiPricingDto Pricing(PriceAssessment price, ApiOfferReferences references)
            => new(
                price.Origin.ToString(),
                price.Status.ToString(),
                price.PricingUnit?.ToString(),
                references.CurrencyCode(price.CurrencyId),
                price.BaseAmount is { } baseAmount ? ApiOfferReferences.Amount(baseAmount) : null,
                price.CompleteUnitTotal is { } total ? ApiOfferReferences.Amount(total) : null,
                price.AddedTaxLines.Select(line => Tax(line, false, references))
                    .Concat(price.IncludedTaxLines.Select(line => Tax(line, true, references)))
                    .ToList(),
                price.AppliedUnitFeeLines.Select(line => Fee(line, false, references))
                    .Concat(price.UnappliedFeeLines.Select(line => Fee(line, true, references)))
                    .ToList(),
                price.RequestedQuantityTotal is { } requested ? ApiOfferReferences.Amount(requested) : null,
                price.IsOrderLevelTotalComplete);

        private static ApiTaxDto Tax(PriceComponentLine line, bool included, ApiOfferReferences references)
            => new(ApiOfferReferences.Amount(line.Amount), line.Code, references.CurrencyCode(line.CurrencyId), included);

        private static ApiFeeDto Fee(PriceComponentLine line, bool orderLevel, ApiOfferReferences references)
            => new(ApiOfferReferences.Amount(line.Amount), line.Code, references.CurrencyCode(line.CurrencyId), line.FeeApplicationUnit?.ToString(), orderLevel);

        [GeneratedRegex("[^A-Za-z0-9]+")]
        private static partial Regex Separators();
    }
}
