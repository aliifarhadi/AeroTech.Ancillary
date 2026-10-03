using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.CancelServiceReservationUnits.Service;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ConfirmServiceReservation.Service;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReleaseServiceReservation.Service;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation.Service;
using AeroTech.Ancillary.Query.ServiceReservationAggregate.Queries.GetServiceReservationById.Service;
using AeroTech.Ancillary.RestApi.V1.ServiceReservationAggregate.Requests;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.ServiceReservationAggregate
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Service")]
    [Route($"Service/v{{version:apiVersion}}/ServiceReservations")]
    public sealed class ServiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServiceController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Reserve([FromBody] ReserveServiceReservationRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ServiceReserveServiceReservationCommand(
                request.IdempotencyKey,
                request.Reference,
                request.ExpiresAt,
                request.Context,
                request.Units), cancellationToken));

        [HttpGet("{serviceReservationId:long}")]
        public async Task<IActionResult> GetById(long serviceReservationId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ServiceGetServiceReservationByIdQuery(serviceReservationId), cancellationToken));

        [HttpPost("{serviceReservationId:long}/Confirmations")]
        public async Task<IActionResult> Confirm(long serviceReservationId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ServiceConfirmServiceReservationCommand(serviceReservationId), cancellationToken));

        [HttpPost("{serviceReservationId:long}/Releases")]
        public async Task<IActionResult> Release(long serviceReservationId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ServiceReleaseServiceReservationCommand(serviceReservationId), cancellationToken));

        [HttpPost("{serviceReservationId:long}/Cancellations")]
        public async Task<IActionResult> Cancel(
            long serviceReservationId,
            [FromBody] CancelServiceReservationUnitsRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ServiceCancelServiceReservationUnitsCommand(serviceReservationId, request.UnitRefs), cancellationToken));
    }
}
