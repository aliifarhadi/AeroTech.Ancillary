using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryVariants.Backoffice
{
    public sealed class BackofficeGetAncillaryVariantsQuery : IRequest<IReadOnlyList<AncillaryVariantDto>>, IAncillaryVariantsQuery
    {
        public AncillaryProfile? Profile { get; set; }
    }
}
