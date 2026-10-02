using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Models
{
    public sealed class ServiceSubCodeReadModel
    {
        public long Id { get; set; }

        public int OwnerAirlineId { get; set; }

        public string Code { get; set; } = default!;

        public ServiceSubCodeSource Source { get; set; }

        public string Rfic { get; set; } = default!;

        public string GroupCode { get; set; } = default!;

        public string? SubGroupCode { get; set; }

        public string? Description1Code { get; set; }

        public string? Description2Code { get; set; }

        public string CommercialName { get; set; } = default!;

        public ServiceSubCodeStatus Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
