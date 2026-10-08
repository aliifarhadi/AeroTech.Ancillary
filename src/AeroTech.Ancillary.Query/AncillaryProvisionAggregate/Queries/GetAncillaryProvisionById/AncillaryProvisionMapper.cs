using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Messages.Ancillary.Enums;
using System.Globalization;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById
{
    public static class AncillaryProvisionMapper
    {
        public static BackofficeProvisionDto ToBackofficeProvision(
            AncillaryProvisionReadModel provision,
            AncillaryProvisionConditionRows rows)
            => new(
                provision.Id,
                provision.ServiceDefinitionId,
                provision.Sequence,
                EnumValueDto.Of(provision.Status),
                provision.SalesEffectiveFrom,
                provision.SalesDiscontinueAt,
                EnumValueDto.Of(provision.CoverageScope),
                new BackofficeProvisionPassengerCriteriaDto(
                    rows.PassengerTypes.Select(row => new BackofficeProvisionConditionRowDto<EnumValueDto>(row.Id, EnumValueDto.Of(row.PassengerTypeCode))).ToList()),
                new BackofficeProvisionSalesCriteriaDto(
                    rows.PointsOfSale.Select(row => new BackofficeProvisionConditionRowDto<long>(row.Id, row.PointOfSaleId)).ToList(),
                    rows.Customers.Select(row => new BackofficeProvisionConditionRowDto<long>(row.Id, row.CustomerId)).ToList(),
                    rows.CustomerTypes.Select(row => new BackofficeProvisionConditionRowDto<EnumValueDto>(row.Id, EnumValueDto.Of(row.CustomerType))).ToList()),
                new BackofficeProvisionTravelCriteriaDto(
                    rows.OriginAirports.Select(row => new BackofficeProvisionConditionRowDto<int>(row.Id, row.AirportId)).ToList(),
                    rows.DestinationAirports.Select(row => new BackofficeProvisionConditionRowDto<int>(row.Id, row.AirportId)).ToList(),
                    rows.ViaAirports.Select(row => new BackofficeProvisionConditionRowDto<int>(row.Id, row.AirportId)).ToList(),
                    rows.RoutePairs
                    .Select(row => new BackofficeProvisionRoutePairDto(row.Id, row.OriginAirportId, row.DestinationAirportId, EnumValueDto.Of(row.Direction)))
                    .ToList(),
                    rows.TravelDates.Select(row => new BackofficeProvisionConditionRowDto<DateOnly>(row.Id, row.TravelDate)).ToList(),
                    rows.SeasonalPeriods.Select(row => new BackofficeProvisionDatePeriodDto(row.Id, row.StartDate, row.EndDate)).ToList(),
                    rows.BlackoutPeriods.Select(row => new BackofficeProvisionDatePeriodDto(row.Id, row.StartDate, row.EndDate)).ToList(),
                    rows.DayTimeRestrictions
                    .Select(row => new BackofficeProvisionDayTimeRestrictionDto(
                        row.Id,
                        EnumValueDto.Of(row.DayOfWeek),
                        row.StartTime,
                        row.EndTime,
                        EnumValueDto.Of(row.Effect)))
                    .ToList(),
                    rows.MarketingAirlines.Select(row => new BackofficeProvisionConditionRowDto<int>(row.Id, row.AirlineId)).ToList(),
                    rows.OperatingAirlines.Select(row => new BackofficeProvisionConditionRowDto<int>(row.Id, row.AirlineId)).ToList(),
                    rows.FlightNumbers.Select(row => new BackofficeProvisionConditionRowDto<string>(row.Id, row.FlightNumber)).ToList(),
                    rows.Flights.Select(row => new BackofficeProvisionConditionRowDto<long>(row.Id, row.FlightId)).ToList(),
                    rows.Aircraft.Select(row => new BackofficeProvisionConditionRowDto<int>(row.Id, row.AircraftId)).ToList()),
                new BackofficeProvisionFareCriteriaDto(
                    rows.AirFares.Select(row => new BackofficeProvisionConditionRowDto<long>(row.Id, row.AirFareId)).ToList(),
                    rows.AirFareTypes.Select(row => new BackofficeProvisionConditionRowDto<EnumValueDto>(row.Id, EnumValueDto.Of(row.AirFareType))).ToList(),
                    rows.FareFamilies.Select(row => new BackofficeProvisionConditionRowDto<long>(row.Id, row.FareFamilyId)).ToList(),
                    rows.FareBases.Select(row => new BackofficeProvisionConditionRowDto<string>(row.Id, row.FareBasisCode)).ToList(),
                    rows.CabinClasses.Select(row => new BackofficeProvisionConditionRowDto<int>(row.Id, row.CabinClassId)).ToList(),
                    rows.Rbds.Select(row => new BackofficeProvisionConditionRowDto<long>(row.Id, row.RbdId)).ToList()),
                provision.AdvancePurchasePeriod is { } period && provision.AdvancePurchaseUnit is { } unit
                    ? new BackofficeProvisionAdvancePurchaseDto(period, EnumValueDto.Of(unit))
                    : null,
                EnumValueDto.Of(provision.QuantityUnit),
                provision.MinQuantity,
                provision.MaxQuantity,
                EnumValueDto.Of(provision.ApplicationType),
                ToBaggage(provision),
                provision.ApplicationType == ProvisionApplicationType.Seat
                    ? new BackofficeProvisionSeatApplicationDto(
                    rows.SeatNumbers.Select(row => new BackofficeProvisionConditionRowDto<string>(row.Id, row.SeatNumber)).ToList(),
                    rows.SeatCharacteristics.Select(row => new BackofficeProvisionConditionRowDto<string>(row.Id, row.CharacteristicCode)).ToList())
                    : null,
                EnumValueDto.Of(provision.Disposition),
                provision.DocumentRequired,
                provision.BookingRequired,
                EnumValueDto.Of(provision.ReissueRefund),
                provision.FormOfRefund is { } formOfRefund ? EnumValueDto.Of(formOfRefund) : null,
                provision.Commissionable,
                provision.InterlineSettlement,
                provision.MustCheckAvailability,
                provision.FulfillmentProviderKey,
                provision.CreatedAt,
                provision.ActivatedAt,
                provision.SuspendedAt,
                provision.RetiredAt);

        public static ProvisionPaginatedRowDto ToPaginatedRow(
            AncillaryProvisionReadModel provision,
            int travelDateCount,
            int seasonalPeriodCount,
            int blackoutPeriodCount,
            int dayTimeRestrictionCount)
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
                TravelDateCount = travelDateCount,
                SeasonalPeriodCount = seasonalPeriodCount,
                BlackoutPeriodCount = blackoutPeriodCount,
                DayTimeRestrictionCount = dayTimeRestrictionCount,
                SalesEffectiveFrom = provision.SalesEffectiveFrom,
                SalesDiscontinueAt = provision.SalesDiscontinueAt,
                Status = EnumValueDto.Of(provision.Status),
                CreatedAt = provision.CreatedAt
            };

        private static BackofficeProvisionBaggageApplicationDto? ToBaggage(AncillaryProvisionReadModel provision)
            => provision.BaggageWeightUnit is { } weightUnit && provision.BaggagePurchaseApplication is { } purchaseApplication
                ? new BackofficeProvisionBaggageApplicationDto(
                    provision.BaggageFreePieces,
                    provision.BaggageFirstExcessPiece,
                    provision.BaggageLastExcessPiece,
                    provision.BaggageWeight,
                    EnumValueDto.Of(weightUnit),
                    EnumValueDto.OfNullable(provision.BaggageTravelApplication),
                    EnumValueDto.Of(purchaseApplication),
                    EnumValueDto.OfNullable(provision.BaggageRuleDeference))
                : null;
    }
}
