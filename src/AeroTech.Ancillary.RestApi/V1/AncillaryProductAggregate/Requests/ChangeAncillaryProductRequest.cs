using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryProductAggregate.Requests
{
    public sealed record ChangeAncillaryProductRequest(
        string Name,
        string? Description,
        AncillarySalesScope SalesScope,
        ProductQuantity Quantity,
        ProductDocument Document,
        ProductCodes Codes,
        ProductTerms Terms,
        AncillaryInventoryControl InventoryControl,
        ProductBaggage? Baggage);
}
