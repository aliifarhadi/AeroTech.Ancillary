using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeIncludedAllowanceDto(
        string TravellerRef,
        string FlightRef,
        int? CheckedPieces,
        decimal? CheckedWeight,
        EnumValueDto? CheckedWeightUnit,
        int? CabinPieces,
        decimal? CabinWeight,
        EnumValueDto? CabinWeightUnit,
        bool Verified);

    public sealed record BackofficeBaggageRuleDto(
        int? FreePieces,
        int? FirstExcessPiece,
        int? LastExcessPiece,
        decimal? Weight,
        EnumValueDto WeightUnit,
        EnumValueDto PurchaseApplication,
        EnumValueDto? ChargeKind,
        EnumValueDto? AllowanceConcept);

    public sealed record BackofficeSeatRuleDto(IReadOnlyList<string> SeatNumbers, IReadOnlyList<string> SeatCharacteristicCodes);

    public sealed record BackofficeRestrictionsDto(
        IReadOnlyList<EnumValueDto> PassengerTypes,
        DateTimeOffset? SalesFrom,
        DateTimeOffset? SalesUntil,
        DateOnly? ServiceSalesFrom,
        DateOnly? ServiceSalesUntil,
        IReadOnlyList<BackofficeDatePeriodDto> PermittedTravelPeriods,
        IReadOnlyList<BackofficeDatePeriodDto> BlackoutPeriods,
        BackofficeAdvancePurchaseDto? AdvancePurchase,
        EnumValueDto PurchaseStage);

    public sealed record BackofficeDatePeriodDto(DateOnly StartDate, DateOnly EndDate);

    public sealed record BackofficeAdvancePurchaseDto(int MinimumPeriod, int? MaximumPeriod, EnumValueDto Unit, bool SameTimeAsTicketed);

    public sealed record BackofficeTermsDto(
        EnumValueDto Disposition,
        EnumValueDto ReissueRefund,
        EnumValueDto? FormOfRefund,
        EnumValueDto ConfirmationRequirement,
        bool Commissionable,
        bool InterlineSettlement);

    public sealed record BackofficeRateDto(
        EnumValueDto? PassengerTypeCode,
        int? AgeFromInclusive,
        int? AgeToExclusive,
        int CurrencyId,
        decimal BaseAmount,
        decimal UnitTotal,
        bool UnitTotalComplete,
        bool Applied);
}
