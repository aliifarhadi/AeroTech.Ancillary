using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision
{
    internal static class ProvisionResultFactory
    {
        public static ProvisionResult ToResult(this AncillaryProvision provision)
            => new(
                provision.Id,
                provision.ServiceDefinitionId,
                provision.Sequence,
                provision.CoverageScope,
                provision.PurchaseStage,
                provision.Outcome.Disposition,
                provision.Status);
    }
}
