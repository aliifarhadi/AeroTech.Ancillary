using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections.Service
{
    public static class ServiceSelectionResolutionMapper
    {
        public static ServiceSelectionResolutionDto Map(SelectionResolutionSnapshot resolution, string? correlationId)
            => new(resolution.OrderId, resolution.OrderCommercialVersion, correlationId, resolution.AllValid, resolution.Results.Select(Result).ToList());

        private static ServiceResolvedSelectionDto Result(ResolvedSelectionSnapshot result)
            => new(
                result.PricedSelectionRef,
                result.Status.ToString(),
                result.ReasonCodes,
                result is { Line: { Candidate: { } candidate } line, Context: { } context } ? Service(line, candidate, context) : null,
                result is { Line.Seat: { IsIncludedSeat: true } seat, Context: { } seatContext } ? IncludedSeat(seat, seatContext) : null);

        private static ServiceResolvedServiceDto Service(SelectionLineSnapshot line, CanonicalAncillaryOfferCandidate candidate, AncillaryShoppingContext context)
            => new(
                line.Kind.ToString(),
                candidate.ServiceDefinitionRef,
                candidate.DefinitionVersionId,
                candidate.ProvisionId,
                candidate.Price.PricingRevisionId,
                candidate.Document.Rfic,
                candidate.Document.Rfisc,
                candidate.Booking.SsrCode,
                candidate.Scope.CoverageScope.ToString(),
                context.Travellers.Where(traveller => candidate.Travellers.Contains(traveller.TravellerRef)).Select(traveller => traveller.OrderTravellerId).OfType<long>().ToList(),
                candidate.Travellers,
                context.Flights.Where(flight => candidate.FlightRefs.Contains(flight.FlightRef)).Select(flight => flight.FlightId).ToList(),
                candidate.PortionRefs,
                line.Seat?.Offer.SeatNumber,
                line.Seat?.Flight.Flight.FlightCapacityId,
                line.Ancillary is { } ancillary ? OfferSelectionResultsMapper.SelectedOptions(ancillary) : [],
                OfferSelectionResultsMapper.ExactPrice(candidate.Price, line.CanPurchase),
                candidate.Availability.RequiresCheckAtReserve,
                candidate.VariantCode,
                candidate.ServiceTypeCode,
                candidate.ServiceSubCode,
                candidate.Booking.Method.ToString(),
                candidate.Booking.BookingRequired,
                candidate.Document.Routing.ToString(),
                candidate.Document.DocumentType.ToString(),
                candidate.Document.DocumentRequired,
                candidate.Confirmation.ConfirmationRequirement.ToString(),
                candidate.Price.PricingRateId,
                Selection(line.Ancillary?.Selection ?? line.Seat!.Selection));

        private static ServiceAncillarySelectionDto Selection(AncillarySelection selection)
            => new(
                selection.Quantity,
                selection.PackageProductRef,
                selection.BagRef,
                selection.MeasuredWeightKg,
                Dimensions(selection.Dimensions),
                selection.EquipmentKind,
                selection.WeightKg,
                selection.SeatNumber,
                selection.ExitRowTermsAccepted,
                selection.Purpose,
                selection.TargetCabinId,
                selection.QuoteRef,
                selection.MealCode,
                selection.MenuItemRef,
                selection.AnimalType,
                selection.OtherAnimalCode,
                selection.AnimalAgeWeeks,
                selection.CombinedWeightKg,
                Dimensions(selection.CarrierDimensions),
                selection.DocumentAcknowledgements,
                selection.SizeBracket,
                selection.AssistanceSsrCode,
                selection.EquipmentCode,
                selection.EvidenceDocumentRefs,
                selection.OxygenUnits,
                selection.InfantRef,
                selection.GuardianRef,
                selection.ChildRef,
                selection.GuardianHandoffContactProvided,
                selection.GuardianPickupContactProvided,
                selection.AirportId,
                selection.FacilityRef,
                selection.TimeWithOffset,
                selection.GuestCount,
                selection.OptIn,
                selection.PlanCode,
                selection.DeviceCount);

        private static ServiceSelectedDimensionsDto? Dimensions(SelectedDimensions? dimensions)
            => dimensions is null ? null : new ServiceSelectedDimensionsDto(dimensions.LengthCm, dimensions.WidthCm, dimensions.HeightCm);

        private static ServiceIncludedSeatDto IncludedSeat(SeatSelectionSnapshot seat, AncillaryShoppingContext context)
            => new(
                context.Travellers.FirstOrDefault(traveller => traveller.TravellerRef == seat.Offer.TravellerRef)?.OrderTravellerId,
                seat.Offer.TravellerRef,
                seat.Flight.Flight.FlightId,
                seat.Flight.Flight.FlightCapacityId,
                seat.Offer.SeatNumber,
                seat.CurrencyId,
                decimal.Zero);
    }
}
