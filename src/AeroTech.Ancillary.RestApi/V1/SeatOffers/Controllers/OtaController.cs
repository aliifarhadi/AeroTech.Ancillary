using AeroTech.Ancillary.Query.SeatOffers.Queries.EvaluateSeatSelection.Api;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOfferDetail.Api;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.Api;
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
    [Tags("Api")]
    [Route($"Api/v{{version:apiVersion}}/SeatOffers")]
    [Authorize(SurfaceAuthorization.Ota)]
    public sealed class OtaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OtaController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> GetByOfferId([FromBody] ApiGetSeatOffersQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Details")]
        public async Task<IActionResult> GetDetail([FromBody] ApiGetSeatOfferDetailQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Selections/Evaluate")]
        public async Task<IActionResult> EvaluateSelection([FromBody] ApiEvaluateSeatSelectionQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));
    }
}
