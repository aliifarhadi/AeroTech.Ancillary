using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Messages.AirPrice.IntegrationEvents.V1
{
    public sealed record RateOfExchangeTablePublished(
        string TableCode,
        RateApplication Application,
        RateSource Source,
        DateTimeOffset EffectiveFrom,
        DateTimeOffset? EffectiveTo,
        string? Description,
        long? EnteredBy,
        DateTimeOffset EnteredAt,
        long? ApprovedBy,
        DateTimeOffset? ApprovedAt,
        IReadOnlyList<RateOfExchangePublishedRate> Rates) : BaseIntegrationEvent;

    public sealed record RateOfExchangePublishedRate(
        long Id,
        int FromCurrencyId,
        int ToCurrencyId,
        decimal Rate);
}
