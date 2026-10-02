using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct
{
    public sealed record AncillaryProductResult(
        long Id,
        int OwnerAirlineId,
        string ProductRef,
        int Version,
        AncillaryProductStatus Status);
}
