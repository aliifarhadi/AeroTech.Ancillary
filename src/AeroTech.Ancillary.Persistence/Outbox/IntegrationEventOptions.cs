namespace AeroTech.Ancillary.Persistence.Outbox
{
    public sealed class IntegrationEventOptions
    {
        public long TenantId { get; set; } = 1;

        public string SourceSystem { get; set; } = "Ancillary";
    }
}
