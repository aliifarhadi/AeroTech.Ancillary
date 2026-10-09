using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Oracle;

public enum OracleRateOutcome
{
    Selected = 1,
    NoMatchingCurrency = 2,
    NoMatchingSelector = 3
}

public sealed record OracleRate(OracleRateOutcome Outcome, AncillaryPricingRate? Rate);
