using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRuleById.Backoffice
{
    public sealed class BackofficeGetAncillaryPriceRuleByIdQueryHandler : IRequestHandler<BackofficeGetAncillaryPriceRuleByIdQuery, BackofficeAncillaryPriceRuleDto>
    {
        private readonly IGetAncillaryPriceRuleByIdService _service;

        public BackofficeGetAncillaryPriceRuleByIdQueryHandler(IGetAncillaryPriceRuleByIdService service) => _service = service;

        public Task<BackofficeAncillaryPriceRuleDto> Handle(BackofficeGetAncillaryPriceRuleByIdQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.AncillaryPriceRuleId, cancellationToken);
    }
}
