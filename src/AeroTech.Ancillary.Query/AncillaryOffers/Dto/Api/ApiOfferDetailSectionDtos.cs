namespace AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api
{
    public sealed record ApiIncludedAllowanceDto(
        string TravellerRef,
        string FlightRef,
        int? CheckedPieces,
        decimal? CheckedWeight,
        string? CheckedWeightUnit,
        int? CabinPieces,
        decimal? CabinWeight,
        string? CabinWeightUnit,
        bool Verified);

    public sealed record ApiBaggageRuleDto(
        int? FreePieces,
        int? FirstExcessPiece,
        int? LastExcessPiece,
        decimal? Weight,
        string WeightUnit,
        string PurchaseApplication,
        string? ChargeKind,
        string? AllowanceConcept);

    public sealed record ApiSeatRuleDto(IReadOnlyList<string> SeatNumbers, IReadOnlyList<string> SeatCharacteristicCodes);

    public sealed record ApiRestrictionsDto(
        IReadOnlyList<string> PassengerTypes,
        DateTimeOffset? SalesFrom,
        DateTimeOffset? SalesUntil,
        DateOnly? ServiceSalesFrom,
        DateOnly? ServiceSalesUntil,
        IReadOnlyList<ApiDatePeriodDto> PermittedTravelPeriods,
        IReadOnlyList<ApiDatePeriodDto> BlackoutPeriods,
        ApiAdvancePurchaseDto? AdvancePurchase,
        string PurchaseStage);

    public sealed record ApiDatePeriodDto(DateOnly StartDate, DateOnly EndDate);

    public sealed record ApiAdvancePurchaseDto(int MinimumPeriod, int? MaximumPeriod, string Unit, bool SameTimeAsTicketed);

    public sealed record ApiTermsDto(
        string Disposition,
        string ReissueRefund,
        string? FormOfRefund,
        string ConfirmationRequirement);

    public sealed record ApiRateDto(
        string? PassengerTypeCode,
        int? AgeFromInclusive,
        int? AgeToExclusive,
        int CurrencyId,
        decimal BaseAmount,
        decimal UnitTotal,
        bool UnitTotalComplete,
        bool Applied);
}
