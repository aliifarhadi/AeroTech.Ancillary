using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier.Backoffice;
using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RetireSupplier.Backoffice;
using AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSupplierById.Backoffice;
using AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSuppliersPaginated.Backoffice;
using AeroTech.Ancillary.RestApi.V1.SupplierAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.SupplierAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/Suppliers")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterSupplierRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRegisterSupplierCommand(
                request.OwnerAirlineId,
                request.Name,
                request.FulfillmentKind,
                request.FulfillmentProviderKey), cancellationToken));

        [HttpPost("{supplierId:long}/Retire")]
        public async Task<IActionResult> Retire(long supplierId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRetireSupplierCommand(supplierId), cancellationToken));

        [HttpGet("Paginated")]
        public async Task<IActionResult> Paginated(
            [FromQuery] BackofficeGetSuppliersPaginatedQuery query,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("{supplierId:long}")]
        public async Task<IActionResult> GetById(long supplierId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetSupplierByIdQuery(supplierId), cancellationToken));
    }
}
