using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeConnectivityDetailsDto(
        EnumValueDto PlanKind,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? DurationMinutes,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? IncludedDataMb,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? MaxDevices,
        EnumValueDto DeliveryStage,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? FulfillmentProviderRef,
        IReadOnlyList<int> EligibleAircraftIds);
}
