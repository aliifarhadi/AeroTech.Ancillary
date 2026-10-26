using System.Text.Json.Serialization;

namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto
{
    public sealed record AncillaryProductHoldDto<TUnit>(
        long ProvisionId,
        string ServiceDefinitionRef,
        IReadOnlyList<TUnit> Units);

    public sealed record AncillaryProductHoldDto<TDetails, TUnit>(
        long ProvisionId,
        string ServiceDefinitionRef,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] TDetails? Details,
        IReadOnlyList<TUnit> Units)
        where TDetails : class;
}
