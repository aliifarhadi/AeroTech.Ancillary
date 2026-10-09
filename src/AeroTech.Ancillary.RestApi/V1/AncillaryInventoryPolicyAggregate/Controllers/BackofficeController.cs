using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ActivateInventoryPolicy.Backoffice;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ChangeInventoryPolicy.Backoffice;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy.Backoffice;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.RetireInventoryPolicy.Backoffice;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.SuspendInventoryPolicy.Backoffice;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPoliciesPaginated.Backoffice;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyById.Backoffice;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyByServiceIdentity.Backoffice;
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
    [Route($"Backoffice/v{{version:apiVersion}}/AncillaryInventoryPolicies")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Define([FromBody] DefineInventoryPolicyRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeDefineInventoryPolicyCommand(
                request.OwnerAirlineId,
                request.ServiceDefinitionRef,
                request.ServiceDefinitionId,
                request.Authority,
                request.LocalPattern,
                request.ProviderKey,
                request.CountConsumption,
                request.WeightConsumption,
                request.SlotConsumption,
                request.PassengerUsageLimits), cancellationToken));

        [HttpPut("{policyId:long}")]
        public async Task<IActionResult> Change(long policyId, [FromBody] ChangeInventoryPolicyRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangeInventoryPolicyCommand(
                policyId,
                request.ServiceDefinitionId,
                request.Authority,
                request.LocalPattern,
                request.ProviderKey,
                request.CountConsumption,
                request.WeightConsumption,
                request.SlotConsumption,
                request.PassengerUsageLimits,
                request.ExpectedVersion), cancellationToken));

        [HttpPost("{policyId:long}/Activate")]
        public async Task<IActionResult> Activate(long policyId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeActivateInventoryPolicyCommand(policyId, request.ExpectedVersion), cancellationToken));

        [HttpPost("{policyId:long}/Suspend")]
        public async Task<IActionResult> Suspend(long policyId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeSuspendInventoryPolicyCommand(policyId, request.ExpectedVersion), cancellationToken));

        [HttpPost("{policyId:long}/Retire")]
        public async Task<IActionResult> Retire(long policyId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRetireInventoryPolicyCommand(policyId, request.ExpectedVersion), cancellationToken));

        [HttpGet("Paginated")]
        public async Task<IActionResult> Paginated(
            [FromQuery] BackofficeGetInventoryPoliciesPaginatedQuery query,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("ByServiceIdentity")]
        public async Task<IActionResult> GetByServiceIdentity([FromQuery] string serviceDefinitionRef, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetInventoryPolicyByServiceIdentityQuery(serviceDefinitionRef), cancellationToken));

        [HttpGet("{policyId:long}")]
        public async Task<IActionResult> GetById(long policyId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetInventoryPolicyByIdQuery(policyId), cancellationToken));
    }
}
