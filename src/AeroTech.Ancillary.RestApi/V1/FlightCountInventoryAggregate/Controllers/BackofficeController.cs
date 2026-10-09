using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.ActivateFlightCountInventory.Backoffice;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.AdjustFlightCountInventory.Backoffice;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.CloseFlightCountInventoryForSale.Backoffice;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory.Backoffice;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.OpenFlightCountInventoryForSale.Backoffice;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.RetireFlightCountInventory.Backoffice;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.SuspendFlightCountInventory.Backoffice;
using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoriesPaginated.Backoffice;
using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoryById.Backoffice;
using AeroTech.Ancillary.RestApi.V1.FlightCountInventoryAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.FlightCountInventoryAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/FlightCountInventories")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Define([FromBody] DefineFlightCountInventoryRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeDefineFlightCountInventoryCommand(
                request.OwnerAirlineId,
                request.FlightId,
                request.ResourceId,
                request.CountUnit,
                request.TotalCapacity), cancellationToken));

        [HttpPost("{inventoryId:long}/Adjust")]
        public async Task<IActionResult> Adjust(long inventoryId, [FromBody] AdjustFlightCountInventoryRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeAdjustFlightCountInventoryCommand(
                inventoryId,
                request.NewTotal,
                request.ReasonCode,
                request.CorrelationId,
                request.ExpectedVersion), cancellationToken));

        [HttpPost("{inventoryId:long}/Activate")]
        public async Task<IActionResult> Activate(long inventoryId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeActivateFlightCountInventoryCommand(inventoryId, request.ExpectedVersion), cancellationToken));

        [HttpPost("{inventoryId:long}/CloseForSale")]
        public async Task<IActionResult> CloseForSale(long inventoryId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeCloseFlightCountInventoryForSaleCommand(inventoryId, request.ExpectedVersion), cancellationToken));

        [HttpPost("{inventoryId:long}/OpenForSale")]
        public async Task<IActionResult> OpenForSale(long inventoryId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeOpenFlightCountInventoryForSaleCommand(inventoryId, request.ExpectedVersion), cancellationToken));

        [HttpPost("{inventoryId:long}/Suspend")]
        public async Task<IActionResult> Suspend(long inventoryId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeSuspendFlightCountInventoryCommand(inventoryId, request.ExpectedVersion), cancellationToken));

        [HttpPost("{inventoryId:long}/Retire")]
        public async Task<IActionResult> Retire(long inventoryId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRetireFlightCountInventoryCommand(inventoryId, request.ExpectedVersion), cancellationToken));

        [HttpGet("Paginated")]
        public async Task<IActionResult> Paginated(
            [FromQuery] BackofficeGetFlightCountInventoriesPaginatedQuery query,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("{inventoryId:long}")]
        public async Task<IActionResult> GetById(long inventoryId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetFlightCountInventoryByIdQuery(inventoryId), cancellationToken));
    }
}
