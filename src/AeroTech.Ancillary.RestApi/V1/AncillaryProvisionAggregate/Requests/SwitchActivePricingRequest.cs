namespace AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Requests
{
    public sealed record SwitchActivePricingRequest(
        long NewPricingId,
        long? ExpectedOldPricingId);
}
