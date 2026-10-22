using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers.Backoffice
{
    public static class BackofficeGroupedAncillaryOffersMapper
    {
        private const string Surface = GroupedOfferRoutes.Backoffice;

        public static BackofficeGroupedAncillaryOffersDto Map(GroupedAncillaryOffersSnapshot snapshot)
            => new(
                new BackofficeGroupedOffersMetaDto(snapshot.OfferId, snapshot.OrderId, snapshot.Shopping.CurrencyId, snapshot.Shopping.EvaluatedAtUtc, EnumValueDto.Of(snapshot.ShoppingStage)),
                snapshot.FlightIds.Select(flightId => new BackofficeGroupedSeatLinkDto(flightId, GroupedOfferRoutes.SeatMap(Surface))).ToList(),
                Of(snapshot, AncillaryProfile.Baggage, Baggage),
                Of(snapshot, AncillaryProfile.Meal, Meal),
                Of(snapshot, AncillaryProfile.Pet, Pet),
                Of(snapshot, AncillaryProfile.AssistedTravel, AssistedTravel),
                Of(snapshot, AncillaryProfile.AirportService, AirportService),
                Of(snapshot, AncillaryProfile.Priority, Priority),
                Of(snapshot, AncillaryProfile.Connectivity, Connectivity),
                Of(snapshot, AncillaryProfile.Upgrade, Upgrade),
                Of(snapshot, AncillaryProfile.Seat, ExtraSeat));

        private static IReadOnlyList<TDto> Of<TDto>(GroupedAncillaryOffersSnapshot snapshot, AncillaryProfile profile, Func<GroupedOfferItem, Common, TDto> map)
            => snapshot.Items
                .Where(row => row.Item.Candidate.Profile == profile)
                .Select(row => map(row, CommonOf(row)))
                .ToList();

        private static Common CommonOf(GroupedOfferItem row)
        {
            var candidate = row.Candidate;

            return new Common(
                row.Item.OfferItemRef,
                candidate.VariantCode,
                candidate.ServiceDefinitionRef,
                candidate.CommercialName,
                candidate.Scope.TravellerRefs,
                candidate.Scope.FlightRefs,
                candidate.Scope.PortionRefs,
                EnumValueDto.Of(candidate.Scope.CoverageScope),
                EnumValueDto.Of(candidate.OfferReadiness),
                new BackofficeGroupedPriceDto(candidate.Price.CurrencyId, candidate.Price.CompleteUnitTotal, OfferPurchase.IsPriceComplete(candidate.Price)),
                GroupedQuantity.Of(candidate) is { } quantity ? new BackofficeGroupedQuantityDto(quantity.MinPerSelection, quantity.MaxPerSelection, EnumValueDto.Of(quantity.Unit)) : null,
                row.SelectionId,
                row.RequiresDetails,
                row.SelectionId is null ? GroupedOfferRoutes.Details(Surface, candidate.Profile) : null);
        }

        private static BackofficeGroupedBaggageOfferDto Baggage(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.Baggage);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.Quantity, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                EnumValueDto.Of(spec.ChargeKind),
                EnumValueDto.OfNullable(spec.AllowanceConcept),
                spec.PackageWeightKg,
                spec.MaxKgPerPiece,
                spec.WeightFromExclusiveKg,
                spec.WeightToInclusiveKg,
                spec.EquipmentKind);
        }

        private static BackofficeGroupedMealOfferDto Meal(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.Meal);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.Quantity, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                EnumValueDto.Of(spec.MealKind),
                spec.MealCode,
                spec.MenuItemRef,
                spec.CateringLeadTimeMinutes);
        }

        private static BackofficeGroupedPetOfferDto Pet(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.Pet);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.Quantity, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                EnumValueDto.Of(spec.TransportMode),
                spec.MaxCombinedWeightKg,
                spec.MinAnimalAgeWeeks,
                spec.RequiredDocumentCodes.Select(code => code.Code).ToList());
        }

        private static BackofficeGroupedAssistedTravelOfferDto AssistedTravel(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.AssistedTravel);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.Quantity, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                EnumValueDto.Of(spec.AssistanceKind),
                GroupedOfferSpecifications.AllowedSsrCodes(spec));
        }

        private static BackofficeGroupedAirportServiceOfferDto AirportService(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.AirportService);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.Quantity, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                EnumValueDto.Of(spec.Kind),
                spec.AirportId,
                spec.FacilityId,
                spec.RequiresSpecificAppointment,
                spec.MaxGuestsPerPrimary);
        }

        private static BackofficeGroupedPriorityOfferDto Priority(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.Priority);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.Quantity, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                EnumValueDto.Of(spec.Kind),
                spec.PriorityZoneCode,
                spec.PriorityGroupCode);
        }

        private static BackofficeGroupedConnectivityOfferDto Connectivity(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.Connectivity);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.Quantity, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                EnumValueDto.Of(spec.PlanKind),
                spec.DurationMinutes,
                spec.IncludedDataMb,
                spec.MaxDevices);
        }

        private static BackofficeGroupedUpgradeOfferDto Upgrade(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.Upgrade);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.Quantity, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                spec.FromCabinId,
                spec.ToCabinId,
                EnumValueDto.Of(spec.AllowedUpgradeKind),
                spec.RequiresTicketExchange);
        }

        private static BackofficeGroupedExtraSeatOfferDto ExtraSeat(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.Seat);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.Quantity, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                EnumValueDto.OfNullable(spec.ExtraSeatPurpose),
                spec.ExtraOccupiedSeatCount,
                spec.RequiresAdjacentSeat,
                spec.RequiresExternalTicketAction);
        }

        private static TSpecification Specification<TSpecification>(TSpecification? specification) where TSpecification : class
            => specification ?? throw ExceptionFactory.OfferItemWasNotFound();

        private sealed record Common(
            string OfferItemRef,
            string VariantCode,
            string ServiceDefinitionRef,
            string Name,
            IReadOnlyList<string> TravellerRefs,
            IReadOnlyList<string> FlightRefs,
            IReadOnlyList<string> PortionRefs,
            EnumValueDto CoverageScope,
            EnumValueDto Readiness,
            BackofficeGroupedPriceDto Price,
            BackofficeGroupedQuantityDto? Quantity,
            string? SelectionId,
            bool RequiresDetails,
            string? DetailsEndpoint);
    }
}
