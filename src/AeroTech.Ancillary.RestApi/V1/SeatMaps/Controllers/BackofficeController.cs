using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats.Backoffice;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetOrderSeatMap.Backoffice;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Backoffice;
using AeroTech.Ancillary.RestApi.V1.SeatMaps.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.SeatMaps.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/SeatMaps")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> GetByOfferId([FromBody] BackofficeGetSeatMapQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Orders")]
        public async Task<IActionResult> GetByOrderId([FromBody] BackofficeGetOrderSeatMapQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Price")]
        public async Task<IActionResult> Price([FromBody] PriceOfferSeatsRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficePriceOfferSeatsCommand(request.OfferId, request.Ids), cancellationToken));

        [HttpPost("Orders/Price")]
        public async Task<IActionResult> PriceOrder([FromBody] PriceOrderSeatsRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficePriceOrderSeatsCommand(request.OrderId, request.Ids), cancellationToken));
    }
}
