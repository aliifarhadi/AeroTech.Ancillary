using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ActivateAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ChangeAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReactivateAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.RetireAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReviseAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SuspendAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingById.Backoffice;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated.Backoffice;
using AeroTech.Ancillary.RestApi.V1.AncillaryPricingAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryPricingAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/AncillaryPricings")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Define([FromBody] DefinePricingRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeDefineAncillaryPricingCommand(
                request.AncillaryProvisionId,
                request.CurrencyId,
                request.FeeApplicationUnit,
                request.PriceLines), cancellationToken));

        [HttpPut("{pricingId:long}")]
        public async Task<IActionResult> Change(long pricingId, [FromBody] ChangePricingRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangeAncillaryPricingCommand(
                pricingId,
                request.CurrencyId,
                request.FeeApplicationUnit,
                request.PriceLines), cancellationToken));

        [HttpPost("{pricingId:long}/Activate")]
        public async Task<IActionResult> Activate(long pricingId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeActivateAncillaryPricingCommand(pricingId), cancellationToken));

        [HttpPost("{pricingId:long}/Suspend")]
        public async Task<IActionResult> Suspend(long pricingId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeSuspendAncillaryPricingCommand(pricingId), cancellationToken));

        [HttpPost("{pricingId:long}/Reactivate")]
        public async Task<IActionResult> Reactivate(long pricingId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeReactivateAncillaryPricingCommand(pricingId), cancellationToken));

        [HttpPost("{pricingId:long}/Retire")]
        public async Task<IActionResult> Retire(long pricingId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRetireAncillaryPricingCommand(pricingId), cancellationToken));

        [HttpPost("{pricingId:long}/Revise")]
        public async Task<IActionResult> Revise(long pricingId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeReviseAncillaryPricingCommand(pricingId), cancellationToken));

        [HttpGet("Paginated")]
        public async Task<IActionResult> Paginated(
            [FromQuery] BackofficeGetAncillaryPricingsPaginatedQuery query,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("{pricingId:long}")]
        public async Task<IActionResult> GetById(long pricingId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetAncillaryPricingByIdQuery(pricingId), cancellationToken));
    }
}
