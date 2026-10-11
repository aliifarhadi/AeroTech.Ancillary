using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.Backoffice
{
    public sealed class BackofficeEvaluateSelectionQueryHandler : IRequestHandler<BackofficeEvaluateSelectionQuery, BackofficeSelectionResultDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IEvaluateSelectionService _service;

        public BackofficeEvaluateSelectionQueryHandler(IBackofficeSalesScopeResolver salesScope, IEvaluateSelectionService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<BackofficeSelectionResultDto> Handle(BackofficeEvaluateSelectionQuery query, CancellationToken cancellationToken)
            => BackofficeSelectionResultMapper.Map(
                await _service.ExecuteAsync(
                    query,
                    BackofficeSelectionInputMapper.ToSelection(query.Selection),
                    await _salesScope.ResolveAsync(query.Options, cancellationToken),
                    cancellationToken));
    }
}
