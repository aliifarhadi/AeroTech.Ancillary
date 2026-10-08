using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing
{
    public interface IDefineAncillaryPricingCommand
    {
        long AncillaryProvisionId { get; }

        int CurrencyId { get; }

        FeeApplicationUnit? FeeApplicationUnit { get; }

        IReadOnlyList<PricingLineInput> PriceLines { get; }
    }
}
