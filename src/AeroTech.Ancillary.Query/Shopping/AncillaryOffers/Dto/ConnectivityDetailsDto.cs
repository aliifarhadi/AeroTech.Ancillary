using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record ConnectivityDetailsDto(
        ConnectivityPlanKind PlanKind,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? DurationMinutes,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? IncludedDataMb,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? MaxDevices,
        PurchaseStage DeliveryStage,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? FulfillmentProviderRef,
        IReadOnlyList<int> EligibleAircraftIds);
}
