namespace AeroTech.Ancillary.RestApi.V1.ServiceReservationAggregate.Requests
{
    public sealed record CancelServiceReservationUnitsRequest(IReadOnlyList<long> UnitRefs);
}
