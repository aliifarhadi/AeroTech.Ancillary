using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ChangeAncillaryPricing
{
    public interface IChangeAncillaryPricingCommand
    {
        long PricingId { get; }

        int CurrencyId { get; }

        FeeApplicationUnit? FeeApplicationUnit { get; }

        IReadOnlyList<PricingLineInput> PriceLines { get; }
    }
}
