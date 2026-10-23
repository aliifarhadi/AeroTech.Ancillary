using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections.Service;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryOffers.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Service")]
    [Route($"Service/v{{version:apiVersion}}/Ancillaries/Selections")]
    public sealed class ServiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServiceController(IMediator mediator) => _mediator = mediator;

        [HttpPost("ResolveGrouped")]
        public async Task<IActionResult> ResolveGrouped([FromBody] ServiceResolveGroupedSelectionsQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));
    }
}
