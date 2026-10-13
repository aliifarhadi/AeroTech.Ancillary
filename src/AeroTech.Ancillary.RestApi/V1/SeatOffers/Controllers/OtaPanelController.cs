using AeroTech.Ancillary.Query.SeatOffers.Queries.EvaluateSeatSelection.OtaPanel;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOfferDetail.OtaPanel;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.OtaPanel;
using AeroTech.Ancillary.RestApi.V1._Shared;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.SeatOffers.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("OtaPanel")]
    [Route($"OtaPanel/v{{version:apiVersion}}/SeatOffers")]
    [Authorize(SurfaceAuthorization.OtaPanel)]
    public sealed class OtaPanelController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OtaPanelController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> GetByOfferId([FromBody] OtaPanelGetSeatOffersQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Details")]
        public async Task<IActionResult> GetDetail([FromBody] OtaPanelGetSeatOfferDetailQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Selections/Evaluate")]
        public async Task<IActionResult> EvaluateSelection([FromBody] OtaPanelEvaluateSeatSelectionQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));
    }
}
