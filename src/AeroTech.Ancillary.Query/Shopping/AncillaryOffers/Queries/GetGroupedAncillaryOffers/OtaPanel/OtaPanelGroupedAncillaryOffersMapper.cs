using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers.OtaPanel
{
    public static class OtaPanelGroupedAncillaryOffersMapper
    {
        private const string Surface = GroupedOfferRoutes.OtaPanel;

        public static OtaPanelGroupedAncillaryOffersDto Map(GroupedAncillaryOffersSnapshot snapshot)
            => new(
                new OtaPanelGroupedOffersMetaDto(snapshot.OfferId, snapshot.OrderId, snapshot.Shopping.CurrencyId, snapshot.Shopping.EvaluatedAtUtc, snapshot.ShoppingStage.ToString()),
                snapshot.FlightIds.Select(flightId => new OtaPanelGroupedSeatLinkDto(flightId, GroupedOfferRoutes.SeatMap(Surface))).ToList(),
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
            var candidate = row.Item.Candidate;

            return new Common(
                row.Item.OfferItemRef,
                candidate.VariantCode,
                candidate.ServiceDefinitionRef,
                candidate.CommercialName,
                candidate.Scope.TravellerRefs,
                candidate.Scope.FlightRefs,
                candidate.Scope.PortionRefs,
                candidate.Scope.CoverageScope.ToString(),
                candidate.OfferReadiness.ToString(),
                new OtaPanelGroupedPriceDto(candidate.Price.CurrencyId, candidate.Price.CompleteUnitTotal, OfferPurchase.IsPriceComplete(candidate.Price)),
                null,
                row.RequiresDetails,
                GroupedOfferRoutes.Details(Surface, candidate.Profile, row.RequiresDetails));
        }

        private static OtaPanelGroupedBaggageOfferDto Baggage(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.Baggage);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                spec.ChargeKind.ToString(),
                spec.AllowanceConcept?.ToString(),
                spec.PackageWeightKg,
                spec.MaxKgPerPiece,
                spec.WeightFromExclusiveKg,
                spec.WeightToInclusiveKg,
                spec.EquipmentKind);
        }

        private static OtaPanelGroupedMealOfferDto Meal(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.Meal);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                spec.MealKind.ToString(),
                spec.MealCode,
                spec.MenuItemRef,
                spec.CateringLeadTimeMinutes);
        }

        private static OtaPanelGroupedPetOfferDto Pet(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.Pet);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                spec.TransportMode.ToString(),
                spec.MaxCombinedWeightKg,
                spec.MinAnimalAgeWeeks,
                spec.RequiredDocumentCodes.Select(code => code.Code).ToList());
        }

        private static OtaPanelGroupedAssistedTravelOfferDto AssistedTravel(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.AssistedTravel);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                spec.AssistanceKind.ToString(),
                GroupedOfferSpecifications.AllowedSsrCodes(spec));
        }

        private static OtaPanelGroupedAirportServiceOfferDto AirportService(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.AirportService);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                spec.Kind.ToString(),
                spec.AirportId,
                spec.FacilityId,
                spec.RequiresSpecificAppointment,
                spec.MaxGuestsPerPrimary);
        }

        private static OtaPanelGroupedPriorityOfferDto Priority(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.Priority);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                spec.Kind.ToString(),
                spec.PriorityZoneCode,
                spec.PriorityGroupCode);
        }

        private static OtaPanelGroupedConnectivityOfferDto Connectivity(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.Connectivity);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                spec.PlanKind.ToString(),
                spec.DurationMinutes,
                spec.IncludedDataMb,
                spec.MaxDevices);
        }

        private static OtaPanelGroupedUpgradeOfferDto Upgrade(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.Upgrade);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                spec.FromCabinId,
                spec.ToCabinId,
                spec.AllowedUpgradeKind.ToString(),
                spec.RequiresTicketExchange);
        }

        private static OtaPanelGroupedExtraSeatOfferDto ExtraSeat(GroupedOfferItem row, Common c)
        {
            var spec = Specification(row.Definition.Seat);

            return new(
                c.OfferItemRef, c.VariantCode, c.ServiceDefinitionRef, c.Name, c.TravellerRefs, c.FlightRefs, c.PortionRefs, c.CoverageScope, c.Readiness, c.Price, c.SelectionId, c.RequiresDetails, c.DetailsEndpoint,
                spec.ExtraSeatPurpose?.ToString(),
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
            string CoverageScope,
            string Readiness,
            OtaPanelGroupedPriceDto Price,
            string? SelectionId,
            bool RequiresDetails,
            string? DetailsEndpoint);
    }
}
