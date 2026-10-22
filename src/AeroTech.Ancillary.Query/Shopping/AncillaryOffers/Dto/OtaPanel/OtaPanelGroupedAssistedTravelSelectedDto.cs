namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedAssistedTravelSelectedDto(
        string? AssistanceSsrCode,
        string? EquipmentCode,
        IReadOnlyList<string>? EvidenceDocumentRefs,
        decimal? OxygenUnits,
        string? InfantRef,
        string? GuardianRef,
        string? ChildRef,
        bool GuardianHandoffContactProvided,
        bool GuardianPickupContactProvided) : OtaPanelGroupedSelectedDto;
}
