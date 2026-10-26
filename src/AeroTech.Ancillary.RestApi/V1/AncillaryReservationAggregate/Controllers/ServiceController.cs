using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.CancelAncillaryHold.Service;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold.Service;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices.Service;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ReleaseAncillaryHold.Service;
using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Queries.GetAncillaryHoldById.Service;
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
    public sealed class ServiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServiceController(IMediator mediator) => _mediator = mediator;

        [HttpPost("Service-Holds")]
        public async Task<IActionResult> Hold([FromBody] HoldAncillaryServicesRequest request, CancellationToken cancellationToken)
            => Created(string.Empty, await _mediator.Send(new ServiceHoldAncillaryServicesCommand(
                request.IdempotencyKey,
                request.OrderId,
                request.Reference,
                request.RequestedExpiresAt,
                request.Baggages,
                request.Meals,
                request.AssistedTravel,
                request.AirportServices,
                request.PriorityServices,
                request.Connectivity,
                request.Upgrades), cancellationToken));

        [HttpGet("Service-Holds/{holdId:long}")]
        public async Task<IActionResult> GetById(long holdId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ServiceGetAncillaryHoldByIdQuery(holdId), cancellationToken));

        [HttpPost("Service-Holds/{holdId:long}/Confirmations")]
        public async Task<IActionResult> Confirm(long holdId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ServiceConfirmAncillaryHoldCommand(holdId), cancellationToken));

        [HttpDelete("Service-Holds/{holdId:long}")]
        public async Task<IActionResult> Release(long holdId, [FromQuery] long[]? unitIds, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ServiceReleaseAncillaryHoldCommand(holdId, unitIds is { Length: > 0 } ? unitIds : null), cancellationToken));

        [HttpPost("Service-Holds/{holdId:long}/Cancellations")]
        public async Task<IActionResult> Cancel(long holdId, [FromBody] CancelAncillaryHoldRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ServiceCancelAncillaryHoldCommand(holdId, request.ReasonCode, request.UnitIds), cancellationToken));
    }
}
