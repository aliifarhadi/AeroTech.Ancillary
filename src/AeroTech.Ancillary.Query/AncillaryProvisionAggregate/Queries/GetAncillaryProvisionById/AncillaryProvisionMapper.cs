using System.Globalization;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById
{
    public static class AncillaryProvisionMapper
    {
        public static BackofficeProvisionDto ToBackofficeProvision(
            AncillaryProvisionReadModel provision,
            IReadOnlyList<AncillaryProvisionRoutePairReadModel> routePairs,
            IReadOnlyList<AncillaryProvisionPriceLineReadModel> priceLines)
            => new(
                provision.Id,
                provision.ServiceDefinitionId,
                provision.Sequence,
                EnumValueDto.Of(provision.Status),
                provision.SalesEffectiveFrom,
                provision.SalesDiscontinueAt,
                EnumValueDto.Of(provision.CoverageScope),
                new BackofficeProvisionPassengerCriteriaDto(provision.PassengerTypeCodes.Select(EnumValueDto.Of).ToList()),
                new BackofficeProvisionSalesCriteriaDto(
                    provision.PointOfSaleIds,
                    provision.CustomerIds,
                    provision.CustomerTypes.Select(EnumValueDto.Of).ToList()),
                ToTravel(provision, routePairs),
                new BackofficeProvisionFareCriteriaDto(
                    provision.AirFareIds,
                    provision.AirFareTypes.Select(EnumValueDto.Of).ToList(),
                    provision.FareFamilyIds,
                    provision.FareBasisCodes,
                    provision.CabinClassIds,
                    provision.RbdIds),
                provision.AdvancePurchasePeriod is { } period && provision.AdvancePurchaseUnit is { } unit
                    ? new BackofficeProvisionAdvancePurchaseDto(period, EnumValueDto.Of(unit))
                    : null,
                EnumValueDto.Of(provision.QuantityUnit),
                provision.MinQuantity,
                provision.MaxQuantity,
                EnumValueDto.Of(provision.ApplicationType),
                ToBaggage(provision),
                provision.ApplicationType == ProvisionApplicationType.Seat
                    ? new BackofficeProvisionSeatApplicationDto(provision.SeatNumbers, provision.SeatCharacteristicCodes)
                    : null,
                EnumValueDto.Of(provision.Disposition),
                provision.DocumentRequired,
                provision.BookingRequired,
                provision.FeeCurrencyId,
                provision.FeeApplicationUnit is { } feeApplicationUnit ? EnumValueDto.Of(feeApplicationUnit) : null,
                EnumValueDto.Of(provision.ReissueRefund),
                provision.FormOfRefund is { } formOfRefund ? EnumValueDto.Of(formOfRefund) : null,
                provision.Commissionable,
                provision.InterlineSettlement,
                provision.MustCheckAvailability,
                provision.FulfillmentProviderKey,
                provision.CreatedAt,
                provision.ActivatedAt,
                provision.SuspendedAt,
                provision.RetiredAt,
                priceLines
                    .OrderBy(line => line.Id)
                    .Select(line => new BackofficeProvisionPriceLineDto(
                        line.Id,
                        EnumValueDto.Of(line.Category),
                        line.Code,
                        line.Name,
                        line.CountryId,
                        line.StationAirportId,
                        line.UnitAmount))
                    .ToList());

        public static ProvisionPaginatedRowDto ToPaginatedRow(
            AncillaryProvisionReadModel provision,
            IReadOnlyList<AncillaryProvisionPriceLineReadModel> priceLines,
            string? currency)
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
                FiledAmount = priceLines.Count == 0
                    ? null
                    : priceLines.Sum(line => line.UnitAmount).ToString("n2", CultureInfo.InvariantCulture),
                Currency = currency,
                SalesEffectiveFrom = provision.SalesEffectiveFrom,
                SalesDiscontinueAt = provision.SalesDiscontinueAt,
                Status = EnumValueDto.Of(provision.Status),
                CreatedAt = provision.CreatedAt
            };

        private static BackofficeProvisionTravelCriteriaDto ToTravel(
            AncillaryProvisionReadModel provision,
            IReadOnlyList<AncillaryProvisionRoutePairReadModel> routePairs)
            => new(
                provision.OriginAirportIds,
                provision.DestinationAirportIds,
                provision.ViaAirportIds,
                routePairs
                    .OrderBy(pair => pair.Id)
                    .Select(pair => new BackofficeProvisionRoutePairDto(
                        pair.Id,
                        pair.OriginAirportId,
                        pair.DestinationAirportId,
                        EnumValueDto.Of(pair.Direction)))
                    .ToList(),
                provision.TravelFrom,
                provision.TravelTo,
                provision.DaysOfWeek.Select(EnumValueDto.Of).ToList(),
                provision.TimeFrom,
                provision.TimeTo,
                provision.MarketingAirlineIds,
                provision.OperatingAirlineIds,
                provision.FlightNumbers,
                provision.FlightIds,
                provision.AircraftIds);

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
