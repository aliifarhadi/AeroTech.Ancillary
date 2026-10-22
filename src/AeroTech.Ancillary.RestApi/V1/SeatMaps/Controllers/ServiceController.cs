using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections.Service;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections.Service;
using AeroTech.Ancillary.RestApi.V1.SeatMaps.Requests;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.SeatMaps.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Service")]
    [Route($"Service/v{{version:apiVersion}}/Ancillaries/SeatMaps")]
    public sealed class ServiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServiceController(IMediator mediator) => _mediator = mediator;

        [HttpPost("Resolve")]
        public async Task<IActionResult> Resolve([FromBody] ServiceResolveSeatMapSelectionsQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Holds")]
        public async Task<IActionResult> Hold([FromBody] HoldSeatSelectionsRequest request, CancellationToken cancellationToken)
            => Created(string.Empty, await _mediator.Send(new ServiceHoldSeatSelectionsCommand(
                request.IdempotencyKey,
                request.OrderId,
                request.OrderCommercialVersion,
                request.Reference,
                request.RequestedExpiresAt,
                request.PointOfSaleId,
                request.CurrencyId,
                request.TicketedAtUtc,
                request.Seats), cancellationToken));
    }
}
