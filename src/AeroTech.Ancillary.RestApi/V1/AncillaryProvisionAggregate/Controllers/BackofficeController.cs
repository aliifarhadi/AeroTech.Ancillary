using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ActivateAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ReactivateAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RetireAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.SuspendAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated.Backoffice;
using AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/AncillaryProvisions")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Define([FromBody] DefineProvisionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeDefineAncillaryProvisionCommand(
                request.ServiceDefinitionId,
                request.Sequence,
                request.SalesEffectiveFrom,
                request.SalesDiscontinueAt,
                request.CoverageScope,
                request.Passenger,
                request.Sales,
                request.Travel,
                request.Fare,
                request.AdvancePurchase,
                request.Quantity,
                request.Application,
                request.Outcome,
                request.Fee,
                request.Settlement,
                request.Availability,
                request.Fulfillment), cancellationToken));

        [HttpPut("{provisionId:long}")]
        public async Task<IActionResult> Change(long provisionId, [FromBody] ChangeProvisionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangeAncillaryProvisionCommand(
                provisionId,
                request.Sequence,
                request.SalesEffectiveFrom,
                request.SalesDiscontinueAt,
                request.CoverageScope,
                request.Passenger,
                request.Sales,
                request.Travel,
                request.Fare,
                request.AdvancePurchase,
                request.Quantity,
                request.Application,
                request.Outcome,
                request.Fee,
                request.Settlement,
                request.Availability,
                request.Fulfillment), cancellationToken));

        [HttpPost("{provisionId:long}/Activate")]
        public async Task<IActionResult> Activate(long provisionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeActivateAncillaryProvisionCommand(provisionId), cancellationToken));

        [HttpPost("{provisionId:long}/Suspend")]
        public async Task<IActionResult> Suspend(long provisionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeSuspendAncillaryProvisionCommand(provisionId), cancellationToken));

        [HttpPost("{provisionId:long}/Reactivate")]
        public async Task<IActionResult> Reactivate(long provisionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeReactivateAncillaryProvisionCommand(provisionId), cancellationToken));

        [HttpPost("{provisionId:long}/Retire")]
        public async Task<IActionResult> Retire(long provisionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRetireAncillaryProvisionCommand(provisionId), cancellationToken));

        [HttpGet("Paginated")]
        public async Task<IActionResult> Paginated(
            [FromQuery] BackofficeGetAncillaryProvisionsPaginatedQuery query,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("{provisionId:long}")]
        public async Task<IActionResult> GetById(long provisionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetAncillaryProvisionByIdQuery(provisionId), cancellationToken));
    }
}
