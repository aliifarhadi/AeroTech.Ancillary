using AeroTech.Ancillary.Query.AncillaryQuote.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote.Service
{
    public sealed class ServiceGetAncillaryQuoteQueryHandler : IRequestHandler<ServiceGetAncillaryQuoteQuery, AncillaryQuoteDto>
    {
        private readonly IGetAncillaryQuoteService _service;

        public ServiceGetAncillaryQuoteQueryHandler(IGetAncillaryQuoteService service) => _service = service;

        public Task<AncillaryQuoteDto> Handle(ServiceGetAncillaryQuoteQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query, cancellationToken);
    }
}
