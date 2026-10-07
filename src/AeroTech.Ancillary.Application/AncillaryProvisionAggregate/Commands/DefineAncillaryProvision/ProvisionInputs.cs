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
        DateOnly? TravelFrom = null,
        DateOnly? TravelTo = null,
        IReadOnlyList<DayOfWeek>? DaysOfWeek = null,
        TimeOnly? TimeFrom = null,
        TimeOnly? TimeTo = null,
        IReadOnlyList<int>? MarketingAirlineIds = null,
        IReadOnlyList<int>? OperatingAirlineIds = null,
        IReadOnlyList<string>? FlightNumbers = null,
        IReadOnlyList<long>? FlightIds = null,
        IReadOnlyList<int>? AircraftIds = null);

    public sealed record ProvisionRoutePairInput(
        int OriginAirportId,
        int DestinationAirportId,
        RoutePairDirection Direction);

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

    public sealed record ProvisionFeeInput(
        FeeApplicationUnit ApplicationUnit,
        int CurrencyId,
        IReadOnlyList<ProvisionPriceLineInput> PriceLines);

    public sealed record ProvisionPriceLineInput(
        AncillaryPriceLineCategory Category,
        string? Code,
        string? Name,
        decimal UnitAmount,
        int? CountryId = null,
        int? StationAirportId = null);

    public sealed record ProvisionSettlementInput(
        ReissueRefundPolicy ReissueRefund,
        FormOfRefund? FormOfRefund,
        bool Commissionable,
        bool InterlineSettlement);

    public sealed record ProvisionAvailabilityInput(bool MustCheckAvailability);

    public sealed record ProvisionFulfillmentInput(string FulfillmentProviderKey);
}
