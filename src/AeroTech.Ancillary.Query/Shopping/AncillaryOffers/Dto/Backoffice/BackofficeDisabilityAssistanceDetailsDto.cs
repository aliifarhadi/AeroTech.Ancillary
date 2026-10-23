using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeDisabilityAssistanceDetailsDto(
        IReadOnlyList<string> AllowedSsrCodes,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] EnumValueDto? RequiredCommunicationMethod);
}
