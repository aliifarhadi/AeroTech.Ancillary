using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service
{
    public sealed record ServiceResolvedProvisionDto(
        long ProvisionId,
        ServiceCoverageScope CoverageScope,
        PurchaseStage PurchaseStage,
        BookingMethod BookingMethod,
        bool BookingRequired,
        ConfirmationRequirement ConfirmationRequirement,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? FulfillmentProviderKey,
        DocumentRouting DocumentRouting,
        AncillaryDocumentType DocumentType,
        bool DocumentRequired,
        bool RequiresCheckAtReserve);
}
