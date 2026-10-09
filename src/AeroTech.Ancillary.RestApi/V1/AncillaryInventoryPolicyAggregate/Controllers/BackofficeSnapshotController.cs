using AeroTech.Ancillary.Application._Shared.Time;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryConfigurationSnapshot.Backoffice;
using AeroTech.Ancillary.RestApi.V1.AncillaryInventoryPolicyAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryInventoryPolicyAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/AncillaryInventoryConfigurationSnapshots")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeSnapshotController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeSnapshotController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] InventoryConfigurationSnapshotRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeGetInventoryConfigurationSnapshotQuery
                {
                    ServiceDefinitionRef = request.ServiceDefinitionRef,
                    FlightId = request.FlightId,
                    AtUtc = UtcInstant.ParseOptional(request.AtUtc, nameof(request.AtUtc))
                },
                cancellationToken));
    }
}
