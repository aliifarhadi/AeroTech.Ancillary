using AeroTech.Ancillary.Query.SeatOffers.Queries.EvaluateSeatSelection.Backoffice;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOfferDetail.Backoffice;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.Backoffice;
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
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/SeatOffers")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> GetByOfferId([FromBody] BackofficeGetSeatOffersQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Details")]
        public async Task<IActionResult> GetDetail([FromBody] BackofficeGetSeatOfferDetailQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Selections/Evaluate")]
        public async Task<IActionResult> EvaluateSelection([FromBody] BackofficeEvaluateSeatSelectionQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));
    }
}
