using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ActivateAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ChangeAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.RetireAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ReviseAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.SuspendAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductById.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductsPaginated.Backoffice;
using AeroTech.Ancillary.RestApi.V1.AncillaryProductAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryProductAggregate
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/AncillaryProducts")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Define([FromBody] DefineAncillaryProductRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeDefineAncillaryProductCommand(
                request.OwnerAirlineId,
                request.ProductRef,
                request.Type,
                request.Name,
                request.Description,
                request.SalesScope,
                request.Quantity,
                request.Document,
                request.Codes,
                request.Terms,
                request.InventoryControl,
                request.Baggage), cancellationToken));

        [HttpPut("{ancillaryProductId:long}")]
        public async Task<IActionResult> Change(
            long ancillaryProductId,
            [FromBody] ChangeAncillaryProductRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangeAncillaryProductCommand(
                ancillaryProductId,
                request.Name,
                request.Description,
                request.SalesScope,
                request.Quantity,
                request.Document,
                request.Codes,
                request.Terms,
                request.InventoryControl,
                request.Baggage), cancellationToken));

        [HttpPost("{ancillaryProductId:long}/Activate")]
        public async Task<IActionResult> Activate(long ancillaryProductId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeActivateAncillaryProductCommand(ancillaryProductId), cancellationToken));

        [HttpPost("{ancillaryProductId:long}/Suspend")]
        public async Task<IActionResult> Suspend(long ancillaryProductId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeSuspendAncillaryProductCommand(ancillaryProductId), cancellationToken));

        [HttpPost("{ancillaryProductId:long}/Retire")]
        public async Task<IActionResult> Retire(long ancillaryProductId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRetireAncillaryProductCommand(ancillaryProductId), cancellationToken));

        [HttpPost("{ancillaryProductId:long}/Revise")]
        public async Task<IActionResult> Revise(long ancillaryProductId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeReviseAncillaryProductCommand(ancillaryProductId), cancellationToken));

        [HttpGet("Paginated")]
        public async Task<IActionResult> Paginated(
            [FromQuery] BackofficeGetAncillaryProductsPaginatedQuery query,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("{ancillaryProductId:long}")]
        public async Task<IActionResult> GetById(long ancillaryProductId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetAncillaryProductByIdQuery(ancillaryProductId), cancellationToken));
    }
}
