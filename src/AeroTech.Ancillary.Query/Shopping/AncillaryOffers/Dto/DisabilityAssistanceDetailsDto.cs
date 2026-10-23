using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record DisabilityAssistanceDetailsDto(
        IReadOnlyList<string> AllowedSsrCodes,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] AssistanceCommunicationMethod? RequiredCommunicationMethod);
}
