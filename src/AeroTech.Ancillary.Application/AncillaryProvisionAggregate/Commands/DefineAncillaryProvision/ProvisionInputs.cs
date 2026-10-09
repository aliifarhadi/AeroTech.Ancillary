using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision
{
    public sealed record ProvisionPassengerEligibilityInput(
        IReadOnlyList<PassengerTypeCode>? AllowedPassengerTypes = null,
        IReadOnlyList<ProvisionAgeBandInput>? AllowedAgeBands = null);

    public sealed record ProvisionSalesRestrictionsInput(
        DateTimeOffset? SalesEffectiveFrom = null,
        DateTimeOffset? SalesDiscontinueAt = null,
        IReadOnlyList<long>? AllowedPointOfSaleIds = null,
        IReadOnlyList<long>? AllowedCustomerIds = null,
        IReadOnlyList<CustomerType>? AllowedCustomerTypes = null);

    public sealed record ProvisionGeographyInput(
        IReadOnlyList<int>? AllowedOriginAirportIds = null,
        IReadOnlyList<int>? AllowedDestinationAirportIds = null,
        IReadOnlyList<int>? AllowedViaAirportIds = null,
        IReadOnlyList<ProvisionRoutePairInput>? AllowedRoutePairs = null,
        IReadOnlyList<ProvisionServiceLocationInput>? ServiceLocations = null,
        IReadOnlyList<int>? CoverageCountryIds = null);

    public sealed record ProvisionFlightApplicationInput(
        IReadOnlyList<int>? AllowedMarketingAirlineIds = null,
        IReadOnlyList<int>? AllowedOperatingAirlineIds = null,
        IReadOnlyList<string>? AllowedFlightNumbers = null,
        IReadOnlyList<long>? AllowedFlightIds = null,
        IReadOnlyList<int>? AllowedAircraftIds = null);

    public sealed record ProvisionFareApplicationInput(
        IReadOnlyList<long>? AllowedAirFareIds = null,
        IReadOnlyList<AirFareType>? AllowedAirFareTypes = null,
        IReadOnlyList<long>? AllowedFareFamilyIds = null,
        IReadOnlyList<string>? AllowedFareBasisCodes = null,
        IReadOnlyList<int>? AllowedCabinClassIds = null,
        IReadOnlyList<long>? AllowedRbdIds = null);

    public sealed record ProvisionTravelDateInput(
        IReadOnlyList<ProvisionDatePeriodInput>? PermittedPeriods = null,
        IReadOnlyList<ProvisionDatePeriodInput>? BlackoutPeriods = null);

    public sealed record ProvisionDayTimeApplicationInput(
        IReadOnlyList<ProvisionDayTimeWindowInput>? Windows = null);

    public sealed record ProvisionAdvancePurchaseInput(
        int MinimumPeriod,
        TimeUnit Unit,
        bool SameTimeAsTicketed = false,
        int? MaximumPeriod = null);

    public sealed record ProvisionBaggageApplicationInput(
        int? FreePieces,
        int? FirstExcessPiece,
        int? LastExcessPiece,
        decimal? Weight,
        WeightUnit WeightUnit,
        BaggageTravelApplication? TravelApplication,
        BaggagePurchaseApplication PurchaseApplication,
        BaggageRuleDeference? RuleDeference,
        BaggageChargeKind? ChargeKind = null,
        BaggageAllowanceConcept? AllowanceConcept = null);

    public sealed record ProvisionSeatApplicationInput(
        IReadOnlyList<string>? SeatNumbers = null,
        IReadOnlyList<string>? SeatCharacteristicCodes = null);

    public sealed record ProvisionAgeBandInput(
        int AgeFromInclusive,
        int? AgeToExclusive);

    public sealed record ProvisionRoutePairInput(
        int OriginAirportId,
        int DestinationAirportId,
        RoutePairDirection Direction);

    public sealed record ProvisionServiceLocationInput(
        ServiceLocationType LocationType,
        int LocationId);

    public sealed record ProvisionDatePeriodInput(
        DateOnly StartDate,
        DateOnly EndDate);

    public sealed record ProvisionDayTimeWindowInput(
        byte DaysOfWeekMask,
        TimeOnly? StartLocalTime,
        TimeOnly? EndLocalTime,
        DayTimeRestrictionEffect Effect);

    public sealed record ProvisionQuantityInput(
        AncillaryQuantityUnit Unit,
        int MinQuantity,
        int MaxQuantity);

    public sealed record ProvisionOutcomeInput(
        CommercialDisposition Disposition,
        bool DocumentRequired,
        bool BookingRequired);

    public sealed record ProvisionSettlementInput(
        ReissueRefundPolicy ReissueRefund,
        FormOfRefund? FormOfRefund,
        bool Commissionable,
        bool InterlineSettlement);

    public sealed record ProvisionAvailabilityInput(
        bool MustCheckAvailability);

    public sealed record ProvisionFulfillmentInput(
        string FulfillmentProviderKey);
}
