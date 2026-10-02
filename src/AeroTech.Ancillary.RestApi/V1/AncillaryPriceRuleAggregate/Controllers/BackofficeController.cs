using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ActivateAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ChangeAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.RetireAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.SuspendAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRuleById.Backoffice;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRulesPaginated.Backoffice;
using AeroTech.Ancillary.RestApi.V1.AncillaryPriceRuleAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryPriceRuleAggregate
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/AncillaryPriceRules")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Define([FromBody] DefineAncillaryPriceRuleRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeDefineAncillaryPriceRuleCommand(
                request.OwnerAirlineId,
                request.ProductRef,
                request.Priority,
                request.CurrencyId,
                request.Lines,
                request.SalesFrom,
                request.SalesTo,
                request.TravelFrom,
                request.TravelTo,
                request.Conditions), cancellationToken));

        [HttpPut("{ancillaryPriceRuleId:long}")]
        public async Task<IActionResult> Change(
            long ancillaryPriceRuleId,
            [FromBody] ChangeAncillaryPriceRuleRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangeAncillaryPriceRuleCommand(
                ancillaryPriceRuleId,
                request.Priority,
                request.CurrencyId,
                request.Lines,
                request.SalesFrom,
                request.SalesTo,
                request.TravelFrom,
                request.TravelTo,
                request.Conditions), cancellationToken));

        [HttpPost("{ancillaryPriceRuleId:long}/Activate")]
        public async Task<IActionResult> Activate(long ancillaryPriceRuleId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeActivateAncillaryPriceRuleCommand(ancillaryPriceRuleId), cancellationToken));

        [HttpPost("{ancillaryPriceRuleId:long}/Suspend")]
        public async Task<IActionResult> Suspend(long ancillaryPriceRuleId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeSuspendAncillaryPriceRuleCommand(ancillaryPriceRuleId), cancellationToken));

        [HttpPost("{ancillaryPriceRuleId:long}/Retire")]
        public async Task<IActionResult> Retire(long ancillaryPriceRuleId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRetireAncillaryPriceRuleCommand(ancillaryPriceRuleId), cancellationToken));

        [HttpGet("Paginated")]
        public async Task<IActionResult> Paginated(
            [FromQuery] BackofficeGetAncillaryPriceRulesPaginatedQuery query,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("{ancillaryPriceRuleId:long}")]
        public async Task<IActionResult> GetById(long ancillaryPriceRuleId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetAncillaryPriceRuleByIdQuery(ancillaryPriceRuleId), cancellationToken));
    }
}
