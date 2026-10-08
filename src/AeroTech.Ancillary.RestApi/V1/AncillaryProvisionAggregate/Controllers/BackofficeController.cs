using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SwitchActiveAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ActivateAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionBlackoutPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeRestriction.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionSeasonalPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBlackoutPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeRestriction.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeasonalPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionTravelDate.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ReactivateAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionBlackoutPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionDayTimeRestriction.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionSeasonalPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionTravelDate.Backoffice;
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
                request.Settlement,
                request.Availability,
                request.Fulfillment), cancellationToken));

        [HttpPost("{provisionId:long}/Activate")]
        public async Task<IActionResult> Activate(long provisionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeActivateAncillaryProvisionCommand(provisionId), cancellationToken));

        [HttpPost("{provisionId:long}/Publish")]
        public async Task<IActionResult> Publish(long provisionId, [FromBody] PublishProvisionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficePublishAncillaryProvisionCommand(provisionId, request.PricingId), cancellationToken));

        [HttpPost("{provisionId:long}/SwitchActivePricing")]
        public async Task<IActionResult> SwitchActivePricing(
            long provisionId,
            [FromBody] SwitchActivePricingRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeSwitchActiveAncillaryPricingCommand(provisionId, request.NewPricingId, request.ExpectedOldPricingId),
                cancellationToken));

        [HttpPost("{provisionId:long}/Suspend")]
        public async Task<IActionResult> Suspend(long provisionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeSuspendAncillaryProvisionCommand(provisionId), cancellationToken));

        [HttpPost("{provisionId:long}/Reactivate")]
        public async Task<IActionResult> Reactivate(long provisionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeReactivateAncillaryProvisionCommand(provisionId), cancellationToken));

        [HttpPost("{provisionId:long}/Retire")]
        public async Task<IActionResult> Retire(long provisionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRetireAncillaryProvisionCommand(provisionId), cancellationToken));

        [HttpPost("{provisionId:long}/TravelDates")]
        public async Task<IActionResult> AddTravelDate(long provisionId, [FromBody] ProvisionTravelDateRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeAddProvisionTravelDateCommand(provisionId, request.TravelDate), cancellationToken));

        [HttpPut("{provisionId:long}/TravelDates/{rowId:long}")]
        public async Task<IActionResult> ChangeTravelDate(
            long provisionId,
            long rowId,
            [FromBody] ProvisionTravelDateRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangeProvisionTravelDateCommand(provisionId, rowId, request.TravelDate), cancellationToken));

        [HttpDelete("{provisionId:long}/TravelDates/{rowId:long}")]
        public async Task<IActionResult> RemoveTravelDate(long provisionId, long rowId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRemoveProvisionTravelDateCommand(provisionId, rowId), cancellationToken));

        [HttpPost("{provisionId:long}/SeasonalPeriods")]
        public async Task<IActionResult> AddSeasonalPeriod(long provisionId, [FromBody] ProvisionDatePeriodRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeAddProvisionSeasonalPeriodCommand(provisionId, request.StartDate, request.EndDate), cancellationToken));

        [HttpPut("{provisionId:long}/SeasonalPeriods/{rowId:long}")]
        public async Task<IActionResult> ChangeSeasonalPeriod(
            long provisionId,
            long rowId,
            [FromBody] ProvisionDatePeriodRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangeProvisionSeasonalPeriodCommand(provisionId, rowId, request.StartDate, request.EndDate), cancellationToken));

        [HttpDelete("{provisionId:long}/SeasonalPeriods/{rowId:long}")]
        public async Task<IActionResult> RemoveSeasonalPeriod(long provisionId, long rowId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRemoveProvisionSeasonalPeriodCommand(provisionId, rowId), cancellationToken));

        [HttpPost("{provisionId:long}/BlackoutPeriods")]
        public async Task<IActionResult> AddBlackoutPeriod(long provisionId, [FromBody] ProvisionDatePeriodRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeAddProvisionBlackoutPeriodCommand(provisionId, request.StartDate, request.EndDate), cancellationToken));

        [HttpPut("{provisionId:long}/BlackoutPeriods/{rowId:long}")]
        public async Task<IActionResult> ChangeBlackoutPeriod(
            long provisionId,
            long rowId,
            [FromBody] ProvisionDatePeriodRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangeProvisionBlackoutPeriodCommand(provisionId, rowId, request.StartDate, request.EndDate), cancellationToken));

        [HttpDelete("{provisionId:long}/BlackoutPeriods/{rowId:long}")]
        public async Task<IActionResult> RemoveBlackoutPeriod(long provisionId, long rowId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRemoveProvisionBlackoutPeriodCommand(provisionId, rowId), cancellationToken));

        [HttpPost("{provisionId:long}/DayTimeRestrictions")]
        public async Task<IActionResult> AddDayTimeRestriction(long provisionId, [FromBody] ProvisionDayTimeRestrictionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeAddProvisionDayTimeRestrictionCommand(provisionId, request.DayOfWeek, request.StartTime, request.EndTime, request.Effect), cancellationToken));

        [HttpPut("{provisionId:long}/DayTimeRestrictions/{rowId:long}")]
        public async Task<IActionResult> ChangeDayTimeRestriction(
            long provisionId,
            long rowId,
            [FromBody] ProvisionDayTimeRestrictionRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangeProvisionDayTimeRestrictionCommand(provisionId, rowId, request.DayOfWeek, request.StartTime, request.EndTime, request.Effect), cancellationToken));

        [HttpDelete("{provisionId:long}/DayTimeRestrictions/{rowId:long}")]
        public async Task<IActionResult> RemoveDayTimeRestriction(long provisionId, long rowId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRemoveProvisionDayTimeRestrictionCommand(provisionId, rowId), cancellationToken));

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
