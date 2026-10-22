using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeGroupedAssistedTravelSelectedDto(
        string? AssistanceSsrCode,
        string? EquipmentCode,
        IReadOnlyList<string>? EvidenceDocumentRefs,
        decimal? OxygenUnits,
        string? InfantRef,
        string? GuardianRef,
        string? ChildRef,
        bool GuardianHandoffContactProvided,
        bool GuardianPickupContactProvided) : BackofficeGroupedSelectedDto;
}
