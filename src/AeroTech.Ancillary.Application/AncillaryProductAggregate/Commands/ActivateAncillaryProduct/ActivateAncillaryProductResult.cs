using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ActivateAncillaryProduct
{
    public sealed record ActivateAncillaryProductResult(
        long Id,
        int OwnerAirlineId,
        string ProductRef,
        int Version,
        AncillaryProductStatus Status,
        long? RetiredVersionId);
}
