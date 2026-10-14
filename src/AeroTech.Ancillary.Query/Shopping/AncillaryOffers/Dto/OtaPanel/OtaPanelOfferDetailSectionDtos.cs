namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelIncludedAllowanceDto(
        string TravellerRef,
        string FlightRef,
        int? CheckedPieces,
        decimal? CheckedWeight,
        string? CheckedWeightUnit,
        int? CabinPieces,
        decimal? CabinWeight,
        string? CabinWeightUnit,
        bool Verified);

    public sealed record OtaPanelBaggageRuleDto(
        int? FreePieces,
        int? FirstExcessPiece,
        int? LastExcessPiece,
        decimal? Weight,
        string WeightUnit,
        string PurchaseApplication,
        string? ChargeKind,
        string? AllowanceConcept);

    public sealed record OtaPanelSeatRuleDto(IReadOnlyList<string> SeatNumbers, IReadOnlyList<string> SeatCharacteristicCodes);

    public sealed record OtaPanelRestrictionsDto(
        IReadOnlyList<string> PassengerTypes,
        DateTimeOffset? SalesFrom,
        DateTimeOffset? SalesUntil,
        DateOnly? ServiceSalesFrom,
        DateOnly? ServiceSalesUntil,
        IReadOnlyList<OtaPanelDatePeriodDto> PermittedTravelPeriods,
        IReadOnlyList<OtaPanelDatePeriodDto> BlackoutPeriods,
        OtaPanelAdvancePurchaseDto? AdvancePurchase,
        string PurchaseStage);

    public sealed record OtaPanelDatePeriodDto(DateOnly StartDate, DateOnly EndDate);

    public sealed record OtaPanelAdvancePurchaseDto(int MinimumPeriod, int? MaximumPeriod, string Unit, bool SameTimeAsTicketed);

    public sealed record OtaPanelTermsDto(
        string Disposition,
        string ReissueRefund,
        string? FormOfRefund,
        string ConfirmationRequirement);

    public sealed record OtaPanelRateDto(
        string? PassengerTypeCode,
        int? AgeFromInclusive,
        int? AgeToExclusive,
        int CurrencyId,
        decimal BaseAmount,
        decimal UnitTotal,
        bool UnitTotalComplete,
        bool Applied);
}
