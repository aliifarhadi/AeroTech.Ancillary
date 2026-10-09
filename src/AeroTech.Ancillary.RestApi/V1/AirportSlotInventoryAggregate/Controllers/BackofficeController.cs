using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.ActivateAirportSlotInventory.Backoffice;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.AdjustAirportSlotInventory.Backoffice;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.CloseAirportSlotInventoryForSale.Backoffice;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory.Backoffice;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.OpenAirportSlotInventoryForSale.Backoffice;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.RetireAirportSlotInventory.Backoffice;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.SuspendAirportSlotInventory.Backoffice;
using AeroTech.Ancillary.Application._Shared.Time;
using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoriesPaginated.Backoffice;
using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoryById.Backoffice;
using AeroTech.Ancillary.RestApi.V1.AirportSlotInventoryAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.AirportSlotInventoryAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/AirportSlotInventories")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Define([FromBody] DefineAirportSlotInventoryRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeDefineAirportSlotInventoryCommand(
                request.OwnerAirlineId,
                request.AirportId,
                request.FacilityId,
                UtcInstant.Parse(request.StartUtc, nameof(request.StartUtc)),
                UtcInstant.Parse(request.EndUtc, nameof(request.EndUtc)),
                request.CapacityPersons), cancellationToken));

        [HttpPost("{inventoryId:long}/Adjust")]
        public async Task<IActionResult> Adjust(long inventoryId, [FromBody] AdjustAirportSlotInventoryRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeAdjustAirportSlotInventoryCommand(
                inventoryId,
                request.NewTotal,
                request.ReasonCode,
                request.CorrelationId,
                request.ExpectedVersion), cancellationToken));

        [HttpPost("{inventoryId:long}/Activate")]
        public async Task<IActionResult> Activate(long inventoryId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeActivateAirportSlotInventoryCommand(inventoryId, request.ExpectedVersion), cancellationToken));

        [HttpPost("{inventoryId:long}/CloseForSale")]
        public async Task<IActionResult> CloseForSale(long inventoryId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeCloseAirportSlotInventoryForSaleCommand(inventoryId, request.ExpectedVersion), cancellationToken));

        [HttpPost("{inventoryId:long}/OpenForSale")]
        public async Task<IActionResult> OpenForSale(long inventoryId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeOpenAirportSlotInventoryForSaleCommand(inventoryId, request.ExpectedVersion), cancellationToken));

        [HttpPost("{inventoryId:long}/Suspend")]
        public async Task<IActionResult> Suspend(long inventoryId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeSuspendAirportSlotInventoryCommand(inventoryId, request.ExpectedVersion), cancellationToken));

        [HttpPost("{inventoryId:long}/Retire")]
        public async Task<IActionResult> Retire(long inventoryId, [FromBody] InventoryVersionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRetireAirportSlotInventoryCommand(inventoryId, request.ExpectedVersion), cancellationToken));

        [HttpGet("Paginated")]
        public async Task<IActionResult> Paginated(
            [FromQuery] BackofficeGetAirportSlotInventoriesPaginatedQuery query,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("{inventoryId:long}")]
        public async Task<IActionResult> GetById(long inventoryId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetAirportSlotInventoryByIdQuery(inventoryId), cancellationToken));
    }
}
