using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Shopping.Resolution;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveAncillarySelection.Service
{
    public static class ServiceResolvedSelectionsMapper
    {
        public static ServiceResolvedSelectionsDto Map(IReadOnlyList<ResolvedSelection> selections)
            => new(
                Family(selections, AncillaryProfile.Baggage, selection => AncillaryOffersMapper.Baggage(selection.Definition)),
                Family(selections, AncillaryProfile.Meal, selection => AncillaryOffersMapper.Meal(selection.Definition)),
                Family(selections, AncillaryProfile.Pet, selection => AncillaryOffersMapper.Pet(selection.Definition)),
                Family(selections, AncillaryProfile.AssistedTravel, selection => AncillaryOffersMapper.AssistedTravel(selection.Definition)),
                Family(selections, AncillaryProfile.AirportService, selection => AncillaryOffersMapper.AirportService(selection.Definition, selection.Provision)),
                Family(selections, AncillaryProfile.Priority, selection => AncillaryOffersMapper.Priority(selection.Definition)),
                Family(selections, AncillaryProfile.Connectivity, selection => AncillaryOffersMapper.Connectivity(selection.Definition)),
                Family(selections, AncillaryProfile.Upgrade, selection => AncillaryOffersMapper.Upgrade(selection.Definition)),
                [],
                []);

        private static IReadOnlyList<ServiceResolvedSelectionDto<TDetails>> Family<TDetails>(
            IReadOnlyList<ResolvedSelection> selections,
            AncillaryProfile profile,
            Func<ResolvedSelection, TDetails> details)
            => selections
                .Where(selection => selection.Definition.Profile == profile)
                .Select(selection => new ServiceResolvedSelectionDto<TDetails>(
                    selection.OfferItemId,
                    selection.Definition.VariantCode!,
                    selection.SelectionScope,
                    AncillaryOffersMapper.Unit(selection.SelectionScope, selection.TravellerRef, selection.CoverageRef),
                    Coverage(selection),
                    new ServiceSourceBindingDto(selection.OfferBinding, selection.SalesBinding),
                    Definition(selection),
                    Provision(selection),
                    Pricing(selection),
                    details(selection),
                    SelectionOptionsMapper.Selected(profile, selection.Quantity, selection.SelectedJson)))
                .ToList();

        private static OfferCoverageDto Coverage(ResolvedSelection selection)
            => selection.SelectionScope switch
            {
                SelectionScope.PerOrder => new OfferCoverageDto([], [], []),
                SelectionScope.PerTraveller => new OfferCoverageDto([selection.TravellerRef], [], []),
                SelectionScope.PerTravellerPerPortion => new OfferCoverageDto([selection.TravellerRef], [selection.CoverageRef], []),
                SelectionScope.PerTravellerPerFlight => new OfferCoverageDto([selection.TravellerRef], [], [selection.CoverageRef]),
                _ => throw new ArgumentOutOfRangeException(nameof(selection), selection.SelectionScope, null)
            };

        private static ServiceResolvedDefinitionDto Definition(ResolvedSelection selection)
        {
            var definition = selection.Definition;

            return new ServiceResolvedDefinitionDto(
                definition.Id,
                definition.ServiceDefinitionRef,
                definition.VariantCode!,
                definition.ServiceTypeCode,
                definition.ServiceSubCode,
                definition.Document.Rfic,
                definition.Document.Rfisc,
                definition.Booking.SsrCode);
        }

        private static ServiceResolvedProvisionDto Provision(ResolvedSelection selection)
        {
            var provision = selection.Provision;
            var definition = selection.Definition;

            return new ServiceResolvedProvisionDto(
                provision.Id,
                provision.CoverageScope,
                provision.PurchaseStage,
                definition.Booking.Method,
                provision.Outcome.BookingRequired,
                selection.ConfirmationRequirement,
                provision.Fulfillment.FulfillmentProviderKey,
                definition.DocumentRouting!.Value,
                definition.Document.Type,
                provision.Outcome.DocumentRequired,
                selection.RequiresCheckAtReserve);
        }

        private static ServiceResolvedPricingDto Pricing(ResolvedSelection selection)
            => new(
                selection.Pricing.Id,
                selection.Rate.Id,
                selection.Pricing.Version,
                selection.Rate.CurrencyId,
                selection.Quantity,
                selection.UnitAmount,
                selection.TotalAmount,
                selection.PricingUnit,
                selection.Rate.BaseAmount,
                Lines(selection.Rate, AncillaryPriceLineCategory.Tax),
                Lines(selection.Rate, AncillaryPriceLineCategory.Fee));

        private static IReadOnlyList<ServicePriceLineDto> Lines(AncillaryPricingRate rate, AncillaryPriceLineCategory category)
            => rate.Components
                .Where(component => component.Category == category)
                .Select(component => new ServicePriceLineDto(
                    component.Code,
                    component.Amount.Amount,
                    component.Amount.CurrencyId,
                    component.CountryId,
                    component.StationAirportId,
                    component.TaxTreatment,
                    component.FeeApplicationUnit))
                .ToList();
    }
}
