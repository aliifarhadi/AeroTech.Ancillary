using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.ActivateFlightWeightInventory.Backoffice;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.AdjustFlightWeightInventory.Backoffice;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.CloseFlightWeightInventoryForSale.Backoffice;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory.Backoffice;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.OpenFlightWeightInventoryForSale.Backoffice;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.RetireFlightWeightInventory.Backoffice;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.SuspendFlightWeightInventory.Backoffice;
using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoriesPaginated.Backoffice;
using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoryById.Backoffice;
using AeroTech.Ancillary.RestApi.V1.FlightWeightInventoryAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.FlightWeightInventoryAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/FlightWeightInventories")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Define([FromBody] DefineFlightWeightInventoryRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeDefineFlightWeightInventoryCommand(
                request.OwnerAirlineId,
                request.FlightId,
                request.WeightResourceId,
                request.CapacityKg), cancellationToken));

        [HttpPost("{inventoryId:long}/Adjust")]
        public async Task<IActionResult> Adjust(long inventoryId, [FromBody] AdjustFlightWeightInventoryRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeAdjustFlightWeightInventoryCommand(
                inventoryId,
                request.NewKg,
                request.ReasonCode,
                request.CorrelationId,
                request.ExpectedVersion), cancellationToken));

        [HttpPost("{inventoryId:long}/Activate")]
        public async Task<IActionResult> Activate(long inventoryId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeActivateFlightWeightInventoryCommand(inventoryId, request.ExpectedVersion), cancellationToken));

        [HttpPost("{inventoryId:long}/CloseForSale")]
        public async Task<IActionResult> CloseForSale(long inventoryId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeCloseFlightWeightInventoryForSaleCommand(inventoryId, request.ExpectedVersion), cancellationToken));

        [HttpPost("{inventoryId:long}/OpenForSale")]
        public async Task<IActionResult> OpenForSale(long inventoryId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeOpenFlightWeightInventoryForSaleCommand(inventoryId, request.ExpectedVersion), cancellationToken));

        [HttpPost("{inventoryId:long}/Suspend")]
        public async Task<IActionResult> Suspend(long inventoryId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeSuspendFlightWeightInventoryCommand(inventoryId, request.ExpectedVersion), cancellationToken));

        [HttpPost("{inventoryId:long}/Retire")]
        public async Task<IActionResult> Retire(long inventoryId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRetireFlightWeightInventoryCommand(inventoryId, request.ExpectedVersion), cancellationToken));

        [HttpGet("Paginated")]
        public async Task<IActionResult> Paginated(
            [FromQuery] BackofficeGetFlightWeightInventoriesPaginatedQuery query,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("{inventoryId:long}")]
        public async Task<IActionResult> GetById(long inventoryId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetFlightWeightInventoryByIdQuery(inventoryId), cancellationToken));
    }
}
