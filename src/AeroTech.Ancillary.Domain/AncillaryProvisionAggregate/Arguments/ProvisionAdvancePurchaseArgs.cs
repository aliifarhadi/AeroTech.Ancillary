using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionAdvancePurchaseArgs(
        int MinimumPeriod,
        TimeUnit Unit,
        bool SameTimeAsTicketed);
}
