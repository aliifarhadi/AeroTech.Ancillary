using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats.Api;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetOrderSeatMap.Api;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Api;
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
    [Tags("Api")]
    [Route($"Api/v{{version:apiVersion}}/SeatMaps")]
    [Authorize(SurfaceAuthorization.Ota)]
    public sealed class OtaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OtaController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> GetByOfferId([FromBody] ApiGetSeatMapQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Orders")]
        public async Task<IActionResult> GetByOrderId([FromBody] ApiGetOrderSeatMapQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Price")]
        public async Task<IActionResult> Price([FromBody] PriceOfferSeatsRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ApiPriceOfferSeatsCommand(request.OfferId, request.Ids), cancellationToken));

        [HttpPost("Orders/Price")]
        public async Task<IActionResult> PriceOrder([FromBody] PriceOrderSeatsRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ApiPriceOrderSeatsCommand(request.OrderId, request.Ids), cancellationToken));
    }
}
