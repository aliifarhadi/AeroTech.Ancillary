using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ActivateAncillaryServiceDefinition.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionPricingUnit.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionServiceDateBasis.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ChangeAncillaryServiceDefinition.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReactivateAncillaryServiceDefinition.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.RetireAncillaryServiceDefinition.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReviseAncillaryServiceDefinition.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.SuspendAncillaryServiceDefinition.Backoffice;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById.Backoffice;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated.Backoffice;
using AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/AncillaryServiceDefinitions")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Define([FromBody] DefineServiceDefinitionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeDefineAncillaryServiceDefinitionCommand(
                request.OwnerAirlineId,
                request.SupplierId,
                request.ServiceDefinitionRef,
                request.ServiceSubCode,
                request.SubCodeSource,
                request.ServiceTypeCode,
                request.GroupCode,
                request.SubGroupCode,
                request.Description1Code,
                request.Description2Code,
                request.PricingUnit,
                request.ServiceDateBasis,
                request.CommercialName,
                request.Description,
                request.Document,
                request.Booking,
                request.SalesEffectiveFrom,
                request.SalesDiscontinueOn), cancellationToken));

        [HttpPut("{serviceDefinitionId:long}")]
        public async Task<IActionResult> Change(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangeAncillaryServiceDefinitionCommand(
                serviceDefinitionId,
                request.SupplierId,
                request.ServiceSubCode,
                request.SubCodeSource,
                request.ServiceTypeCode,
                request.GroupCode,
                request.SubGroupCode,
                request.Description1Code,
                request.Description2Code,
                request.PricingUnit,
                request.ServiceDateBasis,
                request.CommercialName,
                request.Description,
                request.Document,
                request.Booking,
                request.SalesEffectiveFrom,
                request.SalesDiscontinueOn), cancellationToken));

        [HttpPost("{serviceDefinitionId:long}/Activate")]
        public async Task<IActionResult> Activate(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeActivateAncillaryServiceDefinitionCommand(serviceDefinitionId), cancellationToken));

        [HttpPost("{serviceDefinitionId:long}/Suspend")]
        public async Task<IActionResult> Suspend(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeSuspendAncillaryServiceDefinitionCommand(serviceDefinitionId), cancellationToken));

        [HttpPost("{serviceDefinitionId:long}/Reactivate")]
        public async Task<IActionResult> Reactivate(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeReactivateAncillaryServiceDefinitionCommand(serviceDefinitionId), cancellationToken));

        [HttpPost("{serviceDefinitionId:long}/Retire")]
        public async Task<IActionResult> Retire(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRetireAncillaryServiceDefinitionCommand(serviceDefinitionId), cancellationToken));

        [HttpPost("{serviceDefinitionId:long}/Revise")]
        public async Task<IActionResult> Revise(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeReviseAncillaryServiceDefinitionCommand(serviceDefinitionId), cancellationToken));

        [HttpPost("{serviceDefinitionId:long}/AssignPricingUnit")]
        public async Task<IActionResult> AssignPricingUnit(
            long serviceDefinitionId,
            [FromBody] AssignPricingUnitRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeAssignAncillaryServiceDefinitionPricingUnitCommand(serviceDefinitionId, request.PricingUnit),
                cancellationToken));

        [HttpPost("{serviceDefinitionId:long}/AssignServiceDateBasis")]
        public async Task<IActionResult> AssignServiceDateBasis(
            long serviceDefinitionId,
            [FromBody] AssignServiceDateBasisRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeAssignAncillaryServiceDefinitionServiceDateBasisCommand(serviceDefinitionId, request.ServiceDateBasis),
                cancellationToken));

        [HttpGet("Paginated")]
        public async Task<IActionResult> Paginated(
            [FromQuery] BackofficeGetAncillaryServiceDefinitionsPaginatedQuery query,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("{serviceDefinitionId:long}")]
        public async Task<IActionResult> GetById(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetAncillaryServiceDefinitionByIdQuery(serviceDefinitionId), cancellationToken));
    }
}
