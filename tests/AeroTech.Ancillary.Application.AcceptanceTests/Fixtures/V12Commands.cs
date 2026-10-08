using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ActivateAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ChangeAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReactivateAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.RetireAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReviseAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SuspendAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SwitchActiveAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionPricingUnit;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed record TestDefinePricingCommand(
    long AncillaryProvisionId,
    int CurrencyId,
    FeeApplicationUnit? FeeApplicationUnit,
    IReadOnlyList<PricingLineInput> PriceLines) : IDefineAncillaryPricingCommand;

public sealed record TestChangePricingCommand(
    long PricingId,
    int CurrencyId,
    FeeApplicationUnit? FeeApplicationUnit,
    IReadOnlyList<PricingLineInput> PriceLines) : IChangeAncillaryPricingCommand;

public sealed record TestPricingLifecycleCommand(long PricingId)
    : IActivateAncillaryPricingCommand,
        ISuspendAncillaryPricingCommand,
        IReactivateAncillaryPricingCommand,
        IRetireAncillaryPricingCommand,
        IReviseAncillaryPricingCommand;

public sealed record TestSwitchActivePricingCommand(
    long ProvisionId,
    long NewPricingId,
    long? ExpectedOldPricingId = null) : ISwitchActiveAncillaryPricingCommand;

public sealed record TestPublishProvisionCommand(long ProvisionId, long PricingId) : IPublishAncillaryProvisionCommand;

public sealed record TestAssignPricingUnitCommand(long ServiceDefinitionId, PricingUnit PricingUnit)
    : IAssignAncillaryServiceDefinitionPricingUnitCommand;

public static class V12Commands
{
    public const int Usd = 155;

    public static TestDefinePricingCommand Pricing(long provisionId, int currencyId, params PricingLineInput[] priceLines)
        => new(provisionId, currencyId, FeeApplicationUnit.Item, priceLines);

    public static TestChangePricingCommand Change(long pricingId, TestDefinePricingCommand source)
        => new(pricingId, source.CurrencyId, source.FeeApplicationUnit, source.PriceLines);

    public static PricingLineInput Base(
        decimal amount,
        PassengerTypeCode? passengerTypeCode = null,
        int? ageFromInclusive = null,
        int? ageToExclusive = null,
        string? name = "Service")
        => new(passengerTypeCode, ageFromInclusive, ageToExclusive, AncillaryPriceLineCategory.Ancillary, null, name, null, null, amount);

    public static PricingLineInput Tax(
        string? code,
        decimal amount,
        PassengerTypeCode? passengerTypeCode = null,
        int? ageFromInclusive = null,
        int? ageToExclusive = null,
        int? countryId = null,
        int? stationAirportId = null,
        string? name = null)
        => new(passengerTypeCode, ageFromInclusive, ageToExclusive, AncillaryPriceLineCategory.Tax, code, name, countryId, stationAirportId, amount);

    public static PricingLineInput Fee(
        string? code,
        decimal amount,
        PassengerTypeCode? passengerTypeCode = null,
        int? ageFromInclusive = null,
        int? ageToExclusive = null)
        => new(passengerTypeCode, ageFromInclusive, ageToExclusive, AncillaryPriceLineCategory.Fee, code, null, null, null, amount);
}
