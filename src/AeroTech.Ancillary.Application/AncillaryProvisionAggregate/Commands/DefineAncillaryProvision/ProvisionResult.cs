using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision
{
    public sealed record ProvisionResult(
        long Id,
        long ServiceDefinitionId,
        int Sequence,
        ServiceCoverageScope CoverageScope,
        PurchaseStage PurchaseStage,
        CommercialDisposition Disposition,
        ProvisionStatus Status);
}
