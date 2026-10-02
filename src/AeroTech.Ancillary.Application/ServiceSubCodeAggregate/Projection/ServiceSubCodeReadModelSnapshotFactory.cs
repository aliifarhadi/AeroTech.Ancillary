using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate.Contracts;

namespace AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Projection
{
    internal static class ServiceSubCodeReadModelSnapshotFactory
    {
        public static ServiceSubCodeReadModelSnapshot ToReadModelSnapshot(this ServiceSubCode subCode)
            => new(
                subCode.Id,
                subCode.OwnerAirlineId,
                subCode.Code,
                subCode.Source,
                subCode.Rfic,
                subCode.GroupCode,
                subCode.SubGroupCode,
                subCode.Description1Code,
                subCode.Description2Code,
                subCode.CommercialName,
                subCode.Status,
                subCode.CreatedAt);
    }
}
