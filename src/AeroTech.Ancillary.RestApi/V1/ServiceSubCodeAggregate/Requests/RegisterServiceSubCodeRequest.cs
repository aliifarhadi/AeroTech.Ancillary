namespace AeroTech.Ancillary.RestApi.V1.ServiceSubCodeAggregate.Requests
{
    public sealed record RegisterServiceSubCodeRequest(
        int OwnerAirlineId,
        string Code,
        string? Rfic,
        string? GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code,
        string? CommercialName);
}
