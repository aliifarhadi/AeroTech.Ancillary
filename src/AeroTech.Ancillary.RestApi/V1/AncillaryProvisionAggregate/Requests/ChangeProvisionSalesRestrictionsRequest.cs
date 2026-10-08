using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Requests
{
    public sealed record ChangeProvisionSalesRestrictionsRequest(
        ProvisionSalesRestrictionsInput? SalesRestrictions);
}
