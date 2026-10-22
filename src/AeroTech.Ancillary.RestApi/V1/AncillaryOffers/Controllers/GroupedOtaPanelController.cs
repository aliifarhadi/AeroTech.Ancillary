using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers.OtaPanel;
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
    [Tags("OtaPanel")]
    [Route($"OtaPanel/v{{version:apiVersion}}/AncillaryOffers")]
    [Authorize(SurfaceAuthorization.OtaPanel)]
    public sealed class GroupedOtaPanelController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GroupedOtaPanelController(IMediator mediator) => _mediator = mediator;

        [HttpPost("Grouped")]
        public async Task<IActionResult> GetGrouped([FromBody] OtaPanelGetGroupedAncillaryOffersQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));
    }
}
