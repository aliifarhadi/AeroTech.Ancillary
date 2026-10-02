using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Dto
{
    public sealed class ServiceSubCodePaginatedRowDto
    {
        public string Id { get; set; } = null!;

        [Grid("Airline")] public int OwnerAirlineId { get; set; }

        [Grid("Code")] public string Code { get; set; } = null!;

        [Grid("Source")] public EnumValueDto Source { get; set; } = null!;

        [Grid("RFIC")] public string Rfic { get; set; } = null!;

        [Grid("Group")] public string GroupCode { get; set; } = null!;

        [Grid("Sub Group")] public string? SubGroupCode { get; set; }

        [Grid("Description 1")] public string? Description1Code { get; set; }

        [Grid("Description 2")] public string? Description2Code { get; set; }

        [Grid("Commercial Name")] public string CommercialName { get; set; } = null!;

        [Grid("Status")] public EnumValueDto Status { get; set; } = null!;

        [Grid("Created")] public DateTimeOffset CreatedAt { get; set; }
    }
}
