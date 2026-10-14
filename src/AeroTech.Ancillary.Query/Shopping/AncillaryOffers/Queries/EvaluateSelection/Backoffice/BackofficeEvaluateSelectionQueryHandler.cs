using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.Common;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Backoffice
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
                    OfferFamilyFilters.Services,
                    cancellationToken));
    }
}
