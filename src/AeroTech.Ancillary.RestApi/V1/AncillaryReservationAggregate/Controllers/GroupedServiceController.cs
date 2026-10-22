using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections.Service;
using AeroTech.Ancillary.RestApi.V1.AncillaryReservationAggregate.Requests;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryReservationAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Service")]
    [Route($"Service/v{{version:apiVersion}}/Ancillaries")]
    public sealed class GroupedServiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GroupedServiceController(IMediator mediator) => _mediator = mediator;

        [HttpPost("Service-Holds/Grouped")]
        public async Task<IActionResult> HoldGrouped([FromBody] HoldGroupedSelectionsRequest request, CancellationToken cancellationToken)
            => Created(string.Empty, await _mediator.Send(new ServiceHoldGroupedSelectionsCommand(
                request.IdempotencyKey,
                request.OrderId,
                request.OrderCommercialVersion,
                request.Reference,
                request.RequestedExpiresAt,
                request.PointOfSaleId,
                request.CurrencyId,
                request.TicketedAtUtc,
                request.Seats ?? [],
                request.Baggages ?? [],
                request.Meals ?? [],
                request.Pets ?? [],
                request.AssistedTravel ?? [],
                request.AirportServices ?? [],
                request.PriorityServices ?? [],
                request.Connectivity ?? [],
                request.Upgrades ?? [],
                request.ExtraSeats ?? []), cancellationToken));
    }
}
