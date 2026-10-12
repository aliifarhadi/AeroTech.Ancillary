namespace AeroTech.Messages.AirPrice.IntegrationEvents.V1
{
    public sealed record PointOfSaleDefined(string Title, long Number) : BaseIntegrationEvent;
}
