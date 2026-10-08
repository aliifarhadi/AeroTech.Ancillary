using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision
{
    public sealed record ProvisionPassengerCriteriaInput(IReadOnlyList<PassengerTypeCode>? PassengerTypeCodes = null);

    public sealed record ProvisionSalesCriteriaInput(
        IReadOnlyList<long>? PointOfSaleIds = null,
        IReadOnlyList<long>? CustomerIds = null,
        IReadOnlyList<CustomerType>? CustomerTypes = null);

    public sealed record ProvisionTravelCriteriaInput(
        IReadOnlyList<int>? OriginAirportIds = null,
        IReadOnlyList<int>? DestinationAirportIds = null,
        IReadOnlyList<int>? ViaAirportIds = null,
        IReadOnlyList<ProvisionRoutePairInput>? RoutePairs = null,
        IReadOnlyList<DateOnly>? TravelDates = null,
        IReadOnlyList<ProvisionDatePeriodInput>? SeasonalPeriods = null,
        IReadOnlyList<ProvisionDatePeriodInput>? BlackoutPeriods = null,
        IReadOnlyList<ProvisionDayTimeRestrictionInput>? DayTimeRestrictions = null,
        IReadOnlyList<int>? MarketingAirlineIds = null,
        IReadOnlyList<int>? OperatingAirlineIds = null,
        IReadOnlyList<string>? FlightNumbers = null,
        IReadOnlyList<long>? FlightIds = null,
        IReadOnlyList<int>? AircraftIds = null);

    public sealed record ProvisionRoutePairInput(
        int OriginAirportId,
        int DestinationAirportId,
        RoutePairDirection Direction);

    public sealed record ProvisionDatePeriodInput(
        DateOnly StartDate,
        DateOnly EndDate);

    public sealed record ProvisionDayTimeRestrictionInput(
        DayOfWeek DayOfWeek,
        TimeOnly? StartTime,
        TimeOnly? EndTime,
        DayTimeRestrictionEffect Effect);

    public sealed record ProvisionFareCriteriaInput(
        IReadOnlyList<long>? AirFareIds = null,
        IReadOnlyList<AirFareType>? AirFareTypes = null,
        IReadOnlyList<long>? FareFamilyIds = null,
        IReadOnlyList<string>? FareBasisCodes = null,
        IReadOnlyList<int>? CabinClassIds = null,
        IReadOnlyList<long>? RbdIds = null);

    public sealed record ProvisionAdvancePurchaseInput(
        int Period,
        TimeUnit Unit);

    public sealed record ProvisionQuantityInput(
        AncillaryQuantityUnit Unit,
        int MinQuantity,
        int MaxQuantity);

    public sealed record ProvisionApplicationInput(
        ProvisionApplicationType Type,
        ProvisionBaggageApplicationInput? Baggage = null,
        ProvisionSeatApplicationInput? Seat = null);

    public sealed record ProvisionBaggageApplicationInput(
        int? FreePieces,
        int? FirstExcessPiece,
        int? LastExcessPiece,
        decimal? Weight,
        WeightUnit WeightUnit,
        BaggageTravelApplication? TravelApplication,
        BaggagePurchaseApplication PurchaseApplication,
        BaggageRuleDeference? RuleDeference);

    public sealed record ProvisionSeatApplicationInput(
        IReadOnlyList<string>? SeatNumbers,
        IReadOnlyList<string>? SeatCharacteristicCodes);

    public sealed record ProvisionOutcomeInput(
        CommercialDisposition Disposition,
        bool DocumentRequired,
        bool BookingRequired);

    public sealed record ProvisionSettlementInput(
        ReissueRefundPolicy ReissueRefund,
        FormOfRefund? FormOfRefund,
        bool Commissionable,
        bool InterlineSettlement);

    public sealed record ProvisionAvailabilityInput(bool MustCheckAvailability);

    public sealed record ProvisionFulfillmentInput(string FulfillmentProviderKey);
}
