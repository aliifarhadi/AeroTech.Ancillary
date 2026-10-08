namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision
{
    public interface IPublishAncillaryProvisionCommand
    {
        long ProvisionId { get; }

        long PricingId { get; }
    }
}
