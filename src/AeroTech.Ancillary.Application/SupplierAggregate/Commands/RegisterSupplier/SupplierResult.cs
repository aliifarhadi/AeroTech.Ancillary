using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier
{
    public sealed record SupplierResult(
        long Id,
        int OwnerAirlineId,
        string Name,
        SupplierFulfillmentKind FulfillmentKind,
        string? FulfillmentProviderKey,
        SupplierStatus Status);
}
