using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Specifications;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections.Service
{
    public static class ServiceSeatMapResolutionMapper
    {
        public static ServiceSeatMapResolutionDto Map(SeatMapResolutionSnapshot resolution)
            => new(
                resolution.OrderId,
                resolution.OrderCommercialVersion,
                resolution.Seats.Select(seat => Seat(seat, resolution.ExpiresAtUtc)).ToList());

        private static ServiceResolvedSeatDto Seat(ResolvedSeatSnapshot seat, DateTimeOffset expiresAtUtc)
            => new(
                seat.Id,
                seat.Selection.TravellerRef,
                seat.Selection.FlightId,
                seat.Selection.SeatNumber,
                seat.Selection.PointOfSaleId,
                expiresAtUtc,
                Definition(seat.Definition),
                Provision(seat.Provision),
                Pricing(seat.Pricing, seat.Rate));

        private static ServiceSeatDefinitionDto Definition(AncillaryServiceDefinition definition)
            => new(
                definition.Id,
                definition.ServiceDefinitionRef,
                definition.Version,
                definition.SupplierId,
                definition.CommercialName,
                definition.Description,
                EnumValueDto.OfNullable(definition.Profile),
                definition.VariantCode,
                definition.ServiceTypeCode,
                definition.ServiceSubCode,
                EnumValueDto.Of(definition.SubCodeSource),
                definition.GroupCode,
                definition.SubGroupCode,
                definition.Description1Code,
                definition.Description2Code,
                EnumValueDto.OfNullable(definition.PricingUnit),
                EnumValueDto.OfNullable(definition.ServiceDateBasis),
                new ServiceSeatBookingDto(
                    EnumValueDto.Of(definition.Booking.Method),
                    definition.Booking.SsrCode,
                    definition.Booking.SsimCode,
                    EnumValueDto.Of(definition.Booking.ConfirmationRequirement)),
                new ServiceSeatDocumentDto(
                    EnumValueDto.Of(definition.Document.Type),
                    EnumValueDto.OfNullable(definition.DocumentRouting),
                    definition.Document.Rfic,
                    definition.Document.Rfisc),
                definition.Seat is { } seat ? Specification(seat) : null);

        private static ServiceSeatSpecificationDto Specification(SeatSpecification seat)
            => new(
                EnumValueDto.Of(seat.SeatPurpose),
                seat.RequiresExitRowEligibility,
                seat.RequiresAdjacentSeat,
                EnumValueDto.OfNullable(seat.ExtraSeatPurpose),
                seat.ExtraOccupiedSeatCount,
                seat.RequiresExternalTicketAction);

        private static ServiceSeatProvisionDto Provision(AncillaryProvision provision)
            => new(
                provision.Id,
                provision.Sequence,
                EnumValueDto.Of(provision.CoverageScope),
                EnumValueDto.Of(provision.PurchaseStage),
                EnumValueDto.Of(provision.PriceOrigin),
                EnumValueDto.Of(provision.ApplicationType),
                new ServiceSeatQuantityDto(EnumValueDto.Of(provision.Quantity.Unit), provision.Quantity.MinQuantity, provision.Quantity.MaxQuantity),
                new ServiceSeatOutcomeDto(EnumValueDto.Of(provision.Outcome.Disposition), provision.Outcome.DocumentRequired, provision.Outcome.BookingRequired),
                new ServiceSeatSettlementDto(
                    EnumValueDto.Of(provision.Settlement.ReissueRefund),
                    EnumValueDto.OfNullable(provision.Settlement.FormOfRefund),
                    provision.Settlement.Commissionable,
                    provision.Settlement.InterlineSettlement),
                new ServiceSeatAvailabilityDto(provision.Availability.MustCheckAvailability),
                new ServiceSeatFulfillmentDto(provision.Fulfillment.FulfillmentProviderKey));

        private static ServiceSeatPricingDto Pricing(AncillaryPricing pricing, AncillaryPricingRate rate)
            => new(pricing.Id, pricing.Version, EnumValueDto.OfNullable(pricing.PricingUnit), Rate(rate));

        private static ServiceSeatRateDto Rate(AncillaryPricingRate rate)
            => new(
                rate.Id,
                EnumValueDto.OfNullable(rate.PassengerTypeCode),
                rate.AgeFromInclusive,
                rate.AgeToExclusive,
                rate.CurrencyId,
                rate.BaseAmount,
                rate.UnitTotal.Amount,
                rate.IsUnitTotalComplete,
                rate.Components.OrderBy(component => component.Id).Select(Component).ToList());

        private static ServiceSeatPriceComponentDto Component(AncillaryPriceComponent component)
            => new(
                component.Id,
                EnumValueDto.Of(component.Category),
                component.Code,
                component.Name,
                component.CountryId,
                component.StationAirportId,
                component.Amount.Amount,
                EnumValueDto.OfNullable(component.FeeApplicationUnit),
                EnumValueDto.OfNullable(component.TaxTreatment));
    }
}
