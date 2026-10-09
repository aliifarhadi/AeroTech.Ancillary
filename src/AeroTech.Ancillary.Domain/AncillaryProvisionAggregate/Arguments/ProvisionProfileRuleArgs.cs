using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionPetRuleArgs(
        string? CountryExceptionCode,
        int? MinAnimalAgeWeeksOverride,
        decimal? MaxCombinedKgOverride,
        ConfirmationRequirement AcceptanceMode);

    public sealed record ProvisionAssistedTravelRuleArgs(
        int? MinimumLeadTimeMinutes,
        MinorConnectionPolicy? ConnectionPolicy,
        bool? MedicalApprovalRequired);

    public sealed record ProvisionAirportServiceRuleArgs(
        string? TerminalRef,
        AirportServiceDirection? Direction,
        TimeOnly? ServiceWindowStart,
        TimeOnly? ServiceWindowEnd,
        long? FacilityId,
        int? MaxGuestsPerPrimary);
}
