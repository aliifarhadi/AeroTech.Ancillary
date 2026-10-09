using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryVariants.Backoffice
{
    public sealed class BackofficeGetAncillaryVariantsQueryHandler
        : IRequestHandler<BackofficeGetAncillaryVariantsQuery, IReadOnlyList<AncillaryVariantDto>>
    {
        private readonly IGetAncillaryVariantsService _service;

        public BackofficeGetAncillaryVariantsQueryHandler(IGetAncillaryVariantsService service) => _service = service;

        public Task<IReadOnlyList<AncillaryVariantDto>> Handle(BackofficeGetAncillaryVariantsQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query, cancellationToken);
    }
}
