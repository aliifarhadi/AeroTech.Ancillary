using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier
{
    public interface IRegisterSupplierCommand
    {
        int OwnerAirlineId { get; }

        string Name { get; }

        SupplierFulfillmentKind FulfillmentKind { get; }

        string? FulfillmentProviderKey { get; }
    }
}
