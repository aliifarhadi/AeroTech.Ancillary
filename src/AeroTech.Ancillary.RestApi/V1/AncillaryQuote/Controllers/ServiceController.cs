using AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote.Service;
using AeroTech.Ancillary.RestApi.V1.AncillaryQuote.Requests;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryQuote
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Service")]
    [Route($"Service/v{{version:apiVersion}}/AncillaryQuotes")]
    public sealed class ServiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServiceController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Quote([FromBody] GetAncillaryQuoteRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ServiceGetAncillaryQuoteQuery(
                request.CurrencyId,
                request.AsOf,
                request.SalesContext,
                request.Travellers,
                request.Bounds,
                request.Selections,
                request.Existing), cancellationToken));
    }
}
