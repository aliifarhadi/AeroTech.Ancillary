using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers.Backoffice;
using AeroTech.Ancillary.RestApi.V1._Shared;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryOffers.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/AncillaryOffers")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class GroupedBackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GroupedBackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost("Grouped")]
        public async Task<IActionResult> GetGrouped([FromBody] BackofficeGetGroupedAncillaryOffersQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));
    }
}
