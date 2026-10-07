using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold.Service;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices.Service;
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
                request.Services), cancellationToken));

        [HttpGet("Service-Holds/{holdId:long}")]
        public async Task<IActionResult> GetById(long holdId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ServiceGetAncillaryHoldByIdQuery(holdId), cancellationToken));

        [HttpPost("Service-Holds/{holdId:long}/Confirmations")]
        public async Task<IActionResult> Confirm(long holdId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ServiceConfirmAncillaryHoldCommand(holdId), cancellationToken));
    }
}
