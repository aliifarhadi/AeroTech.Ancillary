using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Results
{
    public sealed record QuantityAssessment(
        AncillaryQuantityUnit Unit,
        int MinPerSelection,
        int MaxPerSelection,
        bool ZeroIsDeselect,
        int? RequestedQuantity,
        IReadOnlyList<CumulativeLimitAssessment> CumulativeLimits,
        IReadOnlyList<string> ReasonCodes);

    public sealed record CumulativeLimitAssessment(
        string CountingFamilyCode,
        PassengerUsageLimitScope LimitScope,
        int MaxUnits,
        UsageConsumptionUnit ConsumptionUnit,
        decimal? UnitsPerPurchase,
        bool FamilyVerified,
        decimal? VerifiedConsumed,
        decimal? VerifiedRemaining);
}
