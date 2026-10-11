using AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOfferDetail.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetSeatMap.OtaPanel;
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
    public sealed class OtaPanelController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OtaPanelController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> GetByOfferId([FromBody] OtaPanelGetAncillaryOffersQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Details")]
        public async Task<IActionResult> GetDetail([FromBody] OtaPanelGetAncillaryOfferDetailQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Selections/Evaluate")]
        public async Task<IActionResult> EvaluateSelection([FromBody] OtaPanelEvaluateSelectionQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("SeatMaps")]
        public async Task<IActionResult> GetSeatMap([FromBody] OtaPanelGetSeatMapQuery query, CancellationToken cancellationToken)
        {
            await _mediator.Send(query, cancellationToken);

            return NoContent();
        }
    }
}
