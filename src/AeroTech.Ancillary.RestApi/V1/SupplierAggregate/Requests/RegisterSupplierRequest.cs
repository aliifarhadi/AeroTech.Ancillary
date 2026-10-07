using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.SupplierAggregate.Requests
{
    public sealed record RegisterSupplierRequest(
        int OwnerAirlineId,
        string Name,
        SupplierFulfillmentKind FulfillmentKind,
        string? FulfillmentProviderKey);
}
