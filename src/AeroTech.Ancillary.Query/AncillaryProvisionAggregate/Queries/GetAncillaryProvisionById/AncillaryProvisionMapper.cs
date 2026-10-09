using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Messages.Ancillary.Enums;
using System.Globalization;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById
{
    public static class AncillaryProvisionMapper
    {
        private static readonly DayOfWeek[] MaskOrder =
        [
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        ];

        public static BackofficeProvisionDto ToBackofficeProvision(
            AncillaryProvisionReadModel provision,
            ServiceDateBasis? serviceDateBasis,
            AncillaryProvisionRuleRows rows)
            => new(
                provision.Id,
                provision.ServiceDefinitionId,
                EnumValueDto.OfNullable(serviceDateBasis),
                provision.Sequence,
                EnumValueDto.Of(provision.Status),
                EnumValueDto.Of(provision.CoverageScope),
                EnumValueDto.Of(provision.PurchaseStage),
                EnumValueDto.Of(provision.QuantityUnit),
                provision.MinQuantity,
                provision.MaxQuantity,
                EnumValueDto.Of(provision.ApplicationType),
                EnumValueDto.Of(provision.Disposition),
                provision.DocumentRequired,
                provision.BookingRequired,
                EnumValueDto.Of(provision.ReissueRefund),
                EnumValueDto.OfNullable(provision.FormOfRefund),
                provision.Commissionable,
                provision.InterlineSettlement,
                provision.MustCheckAvailability,
                provision.FulfillmentProviderKey,
                provision.CreatedAt,
                provision.ActivatedAt,
                provision.SuspendedAt,
                provision.RetiredAt,
                provision.PassengerEligibilityRuleId is { } passengerEligibilityRuleId
                    ? new BackofficeProvisionPassengerEligibilityDto(
                        passengerEligibilityRuleId,
                        rows.PassengerTypes.Select(row => new BackofficeProvisionRuleRowDto<EnumValueDto>(row.Id, EnumValueDto.Of(row.PassengerTypeCode))).ToList(),
                        rows.AgeBands.Select(row => new BackofficeProvisionAgeBandDto(row.Id, row.AgeFromInclusive, row.AgeToExclusive)).ToList())
                    : null,
                provision.SalesRestrictionsRuleId is { } salesRestrictionsRuleId
                    ? new BackofficeProvisionSalesRestrictionsDto(
                        salesRestrictionsRuleId,
                        provision.SalesEffectiveFrom,
                        provision.SalesDiscontinueAt,
                        rows.PointsOfSale.Select(row => new BackofficeProvisionRuleRowDto<long>(row.Id, row.PointOfSaleId)).ToList(),
                        rows.Customers.Select(row => new BackofficeProvisionRuleRowDto<long>(row.Id, row.CustomerId)).ToList(),
                        rows.CustomerTypes.Select(row => new BackofficeProvisionRuleRowDto<EnumValueDto>(row.Id, EnumValueDto.Of(row.CustomerType))).ToList())
                    : null,
                provision.GeographyRuleId is { } geographyRuleId
                    ? new BackofficeProvisionGeographyDto(
                        geographyRuleId,
                        rows.OriginAirports.Select(row => new BackofficeProvisionRuleRowDto<int>(row.Id, row.AirportId)).ToList(),
                        rows.DestinationAirports.Select(row => new BackofficeProvisionRuleRowDto<int>(row.Id, row.AirportId)).ToList(),
                        rows.ViaAirports.Select(row => new BackofficeProvisionRuleRowDto<int>(row.Id, row.AirportId)).ToList(),
                        rows.CoverageCountries.Select(row => new BackofficeProvisionRuleRowDto<int>(row.Id, row.CountryId)).ToList(),
                        rows.RoutePairs.Select(row => new BackofficeProvisionRoutePairDto(row.Id, row.OriginAirportId, row.DestinationAirportId, EnumValueDto.Of(row.Direction))).ToList(),
                        rows.ServiceLocations.Select(row => new BackofficeProvisionServiceLocationDto(row.Id, EnumValueDto.Of(row.LocationType), row.LocationId)).ToList())
                    : null,
                provision.FlightApplicationRuleId is { } flightApplicationRuleId
                    ? new BackofficeProvisionFlightApplicationDto(
                        flightApplicationRuleId,
                        rows.MarketingAirlines.Select(row => new BackofficeProvisionRuleRowDto<int>(row.Id, row.AirlineId)).ToList(),
                        rows.OperatingAirlines.Select(row => new BackofficeProvisionRuleRowDto<int>(row.Id, row.AirlineId)).ToList(),
                        rows.FlightNumbers.Select(row => new BackofficeProvisionRuleRowDto<string>(row.Id, row.FlightNumber)).ToList(),
                        rows.Flights.Select(row => new BackofficeProvisionRuleRowDto<long>(row.Id, row.FlightId)).ToList(),
                        rows.Aircraft.Select(row => new BackofficeProvisionRuleRowDto<int>(row.Id, row.AircraftId)).ToList())
                    : null,
                provision.FareApplicationRuleId is { } fareApplicationRuleId
                    ? new BackofficeProvisionFareApplicationDto(
                        fareApplicationRuleId,
                        rows.AirFares.Select(row => new BackofficeProvisionRuleRowDto<long>(row.Id, row.AirFareId)).ToList(),
                        rows.AirFareTypes.Select(row => new BackofficeProvisionRuleRowDto<EnumValueDto>(row.Id, EnumValueDto.Of(row.AirFareType))).ToList(),
                        rows.FareFamilies.Select(row => new BackofficeProvisionRuleRowDto<long>(row.Id, row.FareFamilyId)).ToList(),
                        rows.FareBases.Select(row => new BackofficeProvisionRuleRowDto<string>(row.Id, row.FareBasisCode)).ToList(),
                        rows.CabinClasses.Select(row => new BackofficeProvisionRuleRowDto<int>(row.Id, row.CabinClassId)).ToList(),
                        rows.Rbds.Select(row => new BackofficeProvisionRuleRowDto<long>(row.Id, row.RbdId)).ToList())
                    : null,
                provision.TravelDateRuleId is { } travelDateRuleId
                    ? new BackofficeProvisionTravelDateDto(
                        travelDateRuleId,
                        rows.PermittedPeriods.Select(row => new BackofficeProvisionDatePeriodDto(row.Id, row.StartDate, row.EndDate)).ToList(),
                        rows.BlackoutPeriods.Select(row => new BackofficeProvisionDatePeriodDto(row.Id, row.StartDate, row.EndDate)).ToList())
                    : null,
                provision.DayTimeApplicationRuleId is { } dayTimeApplicationRuleId
                    ? new BackofficeProvisionDayTimeApplicationDto(
                        dayTimeApplicationRuleId,
                        rows.Windows.Select(row => new BackofficeProvisionDayTimeWindowDto(row.Id, row.DaysOfWeekMask, DaysOfWeek(row.DaysOfWeekMask), row.StartLocalTime, row.EndLocalTime, EnumValueDto.Of(row.Effect))).ToList())
                    : null,
                provision.AdvancePurchaseRuleId is { } advancePurchaseRuleId && provision.AdvancePurchasePeriod is { } minimumPeriod && provision.AdvancePurchaseUnit is { } unit
                    ? new BackofficeProvisionAdvancePurchaseDto(advancePurchaseRuleId, minimumPeriod, EnumValueDto.Of(unit), provision.AdvancePurchaseSameTimeAsTicketed, provision.AdvancePurchaseMaximumPeriod)
                    : null,
                provision.BaggageApplicationRuleId is { } baggageApplicationRuleId
                && provision.BaggageWeightUnit is { } weightUnit
                && provision.BaggagePurchaseApplication is { } purchaseApplication
                    ? new BackofficeProvisionBaggageApplicationDto(
                        baggageApplicationRuleId,
                        provision.BaggageFreePieces,
                        provision.BaggageFirstExcessPiece,
                        provision.BaggageLastExcessPiece,
                        provision.BaggageWeight,
                        EnumValueDto.Of(weightUnit),
                        EnumValueDto.OfNullable(provision.BaggageTravelApplication),
                        EnumValueDto.Of(purchaseApplication),
                        EnumValueDto.OfNullable(provision.BaggageRuleDeference),
                        EnumValueDto.OfNullable(provision.BaggageChargeKind),
                        EnumValueDto.OfNullable(provision.BaggageAllowanceConcept))
                    : null,
                provision.SeatApplicationRuleId is { } seatApplicationRuleId
                    ? new BackofficeProvisionSeatApplicationDto(
                        seatApplicationRuleId,
                        rows.SeatNumbers.Select(row => new BackofficeProvisionRuleRowDto<string>(row.Id, row.SeatNumber)).ToList(),
                        rows.SeatCharacteristics.Select(row => new BackofficeProvisionRuleRowDto<string>(row.Id, row.CharacteristicCode)).ToList())
                    : null);

        public static ProvisionPaginatedRowDto ToPaginatedRow(
            AncillaryProvisionReadModel provision,
            int permittedPeriodCount,
            int blackoutPeriodCount,
            int dayTimeWindowCount)
            => new()
            {
                Id = provision.Id.ToString(CultureInfo.InvariantCulture),
                ServiceDefinitionId = provision.ServiceDefinitionId.ToString(CultureInfo.InvariantCulture),
                Sequence = provision.Sequence,
                CoverageScope = EnumValueDto.Of(provision.CoverageScope),
                Disposition = EnumValueDto.Of(provision.Disposition),
                QuantityUnit = EnumValueDto.Of(provision.QuantityUnit),
                MinQuantity = provision.MinQuantity,
                MaxQuantity = provision.MaxQuantity,
                PermittedPeriodCount = permittedPeriodCount,
                BlackoutPeriodCount = blackoutPeriodCount,
                DayTimeWindowCount = dayTimeWindowCount,
                SalesEffectiveFrom = provision.SalesEffectiveFrom,
                SalesDiscontinueAt = provision.SalesDiscontinueAt,
                Status = EnumValueDto.Of(provision.Status),
                CreatedAt = provision.CreatedAt
            };

        private static IReadOnlyList<string> DaysOfWeek(byte mask)
            => MaskOrder.Where((_, index) => (mask & (1 << index)) != 0).Select(day => day.ToString()).ToList();
    }
}
