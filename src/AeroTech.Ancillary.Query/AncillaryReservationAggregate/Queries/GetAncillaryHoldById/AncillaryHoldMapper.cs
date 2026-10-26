using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Queries.GetAncillaryHoldById
{
    public static class AncillaryHoldMapper
    {
        public static AncillaryHoldDto ToAncillaryHold(AncillaryReservation reservation, DateTimeOffset now)
        {
            var units = reservation.Units.OrderBy(unit => unit.Id).ToList();
            Func<AncillaryReservationUnit, AncillaryReservationUnitStatus> status = unit => reservation.StatusOf(unit, now);

            return new AncillaryHoldDto(
                reservation.Id,
                reservation.OrderId,
                reservation.Reference,
                reservation.EffectiveStatus(now),
                reservation.ExpiresAt,
                Family(units, AncillaryProfile.Baggage, unit => Unit(unit, status(unit), Baggage(unit)), unit => unit.Baggage?.EquipmentKind is { } kind ? new HeldBaggageDetailsDto(kind) : null),
                Family(units, AncillaryProfile.Meal, unit => Unit(unit, status(unit)), unit => unit.Meal is { } meal ? new HeldMealDetailsDto(meal.MealCode, meal.MenuItemRef) : null),
                Family(units, AncillaryProfile.AssistedTravel, unit => Unit(unit, status(unit), AssistedTravel(unit))),
                Family(units, AncillaryProfile.AirportService, unit => Unit(unit, status(unit), AirportService(unit)), unit => unit.AirportService is { } airport ? new HeldAirportServiceDetailsDto(airport.AirportId, airport.FacilityId) : null),
                Family(units, AncillaryProfile.Priority, unit => Unit(unit, status(unit), Priority(unit))),
                Family(units, AncillaryProfile.Connectivity, unit => Unit(unit, status(unit), Connectivity(unit))),
                Family(units, AncillaryProfile.Upgrade, unit => Unit(unit, status(unit)), unit => unit.Upgrade is { } upgrade ? new HeldUpgradeDetailsDto(upgrade.ToCabinId) : null),
                Family(units, AncillaryProfile.Seat, unit => Seat(unit, status(unit))));
        }

        private static IReadOnlyList<AncillaryVariantHoldDto<TDetails, TUnit>>? Family<TDetails, TUnit>(
            IReadOnlyList<AncillaryReservationUnit> units,
            AncillaryProfile family,
            Func<AncillaryReservationUnit, TUnit> unit,
            Func<AncillaryReservationUnit, TDetails?> details)
            where TDetails : class
            => Variants(units, family) is { Count: > 0 } variants
                ? variants
                    .Select(variant => new AncillaryVariantHoldDto<TDetails, TUnit>(
                        variant.Key,
                        variant
                            .GroupBy(held => held.ProvisionId)
                            .Select(product => new AncillaryProductHoldDto<TDetails, TUnit>(
                                product.Key,
                                product.First().ServiceDefinitionRef,
                                details(product.First()),
                                product.Select(unit).ToList()))
                            .ToList()))
                    .ToList()
                : null;

        private static IReadOnlyList<AncillaryVariantHoldDto<TUnit>>? Family<TUnit>(
            IReadOnlyList<AncillaryReservationUnit> units,
            AncillaryProfile family,
            Func<AncillaryReservationUnit, TUnit> unit)
            => Variants(units, family) is { Count: > 0 } variants
                ? variants
                    .Select(variant => new AncillaryVariantHoldDto<TUnit>(
                        variant.Key,
                        variant
                            .GroupBy(held => held.ProvisionId)
                            .Select(product => new AncillaryProductHoldDto<TUnit>(product.Key, product.First().ServiceDefinitionRef, product.Select(unit).ToList()))
                            .ToList()))
                    .ToList()
                : null;

        private static IReadOnlyList<IGrouping<string, AncillaryReservationUnit>> Variants(IReadOnlyList<AncillaryReservationUnit> units, AncillaryProfile family)
            => units
                .Where(unit => FamilyOf(unit) == family)
                .GroupBy(unit => unit.VariantCode)
                .ToList();

        private static AncillaryProfile FamilyOf(AncillaryReservationUnit unit)
            => unit.SeatNumber is not null ? AncillaryProfile.Seat : AncillaryVariant.Find(unit.VariantCode)!.Profile;

        private static AncillaryUnitHoldDto Unit(AncillaryReservationUnit unit, AncillaryReservationUnitStatus status)
            => new(unit.Id, unit.OrderServiceId, status, unit.CancellationReasonCode, Traveller(unit), Coverage(unit), new HeldSelectedUnitDto(unit.Quantity));

        private static AncillaryUnitHoldDto<TOptions> Unit<TOptions>(AncillaryReservationUnit unit, AncillaryReservationUnitStatus status, TOptions? options)
            where TOptions : class
            => new(unit.Id, unit.OrderServiceId, status, unit.CancellationReasonCode, Traveller(unit), Coverage(unit), new HeldSelectedUnitDto<TOptions>(unit.Quantity, options));

        private static SeatUnitHoldDto Seat(AncillaryReservationUnit unit, AncillaryReservationUnitStatus status)
            => new(
                unit.Id,
                unit.OrderServiceId,
                status,
                unit.CancellationReasonCode,
                Traveller(unit),
                Coverage(unit),
                new HeldSelectedUnitDto(unit.Quantity),
                unit.SeatNumber!,
                unit.SeatMapSelectionId!);

        private static HoldUnitDto? Traveller(AncillaryReservationUnit unit)
            => unit.TravellerId is { } travellerId ? new HoldUnitDto(travellerId) : null;

        private static HoldCoverageDto? Coverage(AncillaryReservationUnit unit)
            => unit.CoveredFlightIds.Count > 0 ? new HoldCoverageDto(unit.CoveredFlightIds) : null;

        private static HeldBaggageOptionsDto? Baggage(AncillaryReservationUnit unit)
            => unit.Baggage is { } baggage and not { BagRef: null, MeasuredWeightKg: null, Dimensions: null, WeightKg: null }
                ? new HeldBaggageOptionsDto(new HeldBaggageSelectionDto(
                    baggage.BagRef,
                    baggage.MeasuredWeightKg,
                    baggage.Dimensions is { } size ? new OfferDimensionsDto(size.LengthCm, size.WidthCm, size.HeightCm) : null,
                    baggage.WeightKg))
                : null;

        private static HeldAssistedTravelOptionsDto? AssistedTravel(AncillaryReservationUnit unit)
            => unit.AssistedTravel is { } assistedTravel ? new HeldAssistedTravelOptionsDto(new HeldAssistedTravelSelectionDto(assistedTravel.AssistanceSsrCode)) : null;

        private static HeldAirportServiceOptionsDto? AirportService(AncillaryReservationUnit unit)
            => unit.AirportService is { } airportService and not { TimeWithOffset: null, GuestCount: null }
                ? new HeldAirportServiceOptionsDto(new HeldAirportServiceSelectionDto(airportService.TimeWithOffset, airportService.GuestCount))
                : null;

        private static HeldPriorityOptionsDto? Priority(AncillaryReservationUnit unit)
            => unit.VariantCode == AncillaryVariant.PriorityBoardingCheckin ? new HeldPriorityOptionsDto(new HeldPrioritySelectionDto(true)) : null;

        private static HeldConnectivityOptionsDto? Connectivity(AncillaryReservationUnit unit)
            => unit.Connectivity is { DeviceCount: { } deviceCount } ? new HeldConnectivityOptionsDto(new HeldConnectivitySelectionDto(deviceCount)) : null;
    }
}
