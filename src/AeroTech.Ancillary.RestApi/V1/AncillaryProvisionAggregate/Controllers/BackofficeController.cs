using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SwitchActiveAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ActivateAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionBlackoutPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeWindow.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAdvancePurchase.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBaggageApplication.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBlackoutPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeApplication.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeWindow.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionFareApplication.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionFlightApplication.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionGeography.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPassengerEligibility.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPermittedTravelPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSalesRestrictions.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeatApplication.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionTravelDate.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ReactivateAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionBlackoutPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionDayTimeWindow.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionPermittedTravelPeriod.Backoffice;
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
                request.CoverageScope,
                request.PurchaseStage,
                request.Quantity,
                request.ApplicationType,
                request.Outcome,
                request.Settlement,
                request.Availability,
                request.Fulfillment,
                request.PassengerEligibility,
                request.SalesRestrictions,
                request.Geography,
                request.FlightApplication,
                request.FareApplication,
                request.TravelDate,
                request.DayTimeApplication,
                request.AdvancePurchase,
                request.BaggageApplication,
                request.SeatApplication), cancellationToken));

        [HttpPut("{provisionId:long}")]
        public async Task<IActionResult> Change(long provisionId, [FromBody] ChangeProvisionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangeAncillaryProvisionCommand(
                provisionId,
                request.Sequence,
                request.CoverageScope,
                request.PurchaseStage,
                request.Quantity,
                request.ApplicationType,
                request.Outcome,
                request.Settlement,
                request.Availability,
                request.Fulfillment,
                request.PassengerEligibility,
                request.SalesRestrictions,
                request.Geography,
                request.FlightApplication,
                request.FareApplication,
                request.TravelDate,
                request.DayTimeApplication,
                request.AdvancePurchase,
                request.BaggageApplication,
                request.SeatApplication), cancellationToken));

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

        [HttpPut("{provisionId:long}/PassengerEligibility")]
        public async Task<IActionResult> ChangePassengerEligibility(
            long provisionId,
            [FromBody] ChangeProvisionPassengerEligibilityRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeChangeProvisionPassengerEligibilityCommand(provisionId, request.PassengerEligibility),
                cancellationToken));

        [HttpPut("{provisionId:long}/SalesRestrictions")]
        public async Task<IActionResult> ChangeSalesRestrictions(
            long provisionId,
            [FromBody] ChangeProvisionSalesRestrictionsRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeChangeProvisionSalesRestrictionsCommand(provisionId, request.SalesRestrictions),
                cancellationToken));

        [HttpPut("{provisionId:long}/Geography")]
        public async Task<IActionResult> ChangeGeography(
            long provisionId,
            [FromBody] ChangeProvisionGeographyRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeChangeProvisionGeographyCommand(provisionId, request.Geography),
                cancellationToken));

        [HttpPut("{provisionId:long}/FlightApplication")]
        public async Task<IActionResult> ChangeFlightApplication(
            long provisionId,
            [FromBody] ChangeProvisionFlightApplicationRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeChangeProvisionFlightApplicationCommand(provisionId, request.FlightApplication),
                cancellationToken));

        [HttpPut("{provisionId:long}/FareApplication")]
        public async Task<IActionResult> ChangeFareApplication(
            long provisionId,
            [FromBody] ChangeProvisionFareApplicationRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeChangeProvisionFareApplicationCommand(provisionId, request.FareApplication),
                cancellationToken));

        [HttpPut("{provisionId:long}/TravelDate")]
        public async Task<IActionResult> ChangeTravelDate(
            long provisionId,
            [FromBody] ChangeProvisionTravelDateRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeChangeProvisionTravelDateCommand(provisionId, request.TravelDate),
                cancellationToken));

        [HttpPut("{provisionId:long}/DayTimeApplication")]
        public async Task<IActionResult> ChangeDayTimeApplication(
            long provisionId,
            [FromBody] ChangeProvisionDayTimeApplicationRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeChangeProvisionDayTimeApplicationCommand(provisionId, request.DayTimeApplication),
                cancellationToken));

        [HttpPut("{provisionId:long}/AdvancePurchase")]
        public async Task<IActionResult> ChangeAdvancePurchase(
            long provisionId,
            [FromBody] ChangeProvisionAdvancePurchaseRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeChangeProvisionAdvancePurchaseCommand(provisionId, request.AdvancePurchase),
                cancellationToken));

        [HttpPut("{provisionId:long}/BaggageApplication")]
        public async Task<IActionResult> ChangeBaggageApplication(
            long provisionId,
            [FromBody] ChangeProvisionBaggageApplicationRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeChangeProvisionBaggageApplicationCommand(provisionId, request.BaggageApplication),
                cancellationToken));

        [HttpPut("{provisionId:long}/SeatApplication")]
        public async Task<IActionResult> ChangeSeatApplication(
            long provisionId,
            [FromBody] ChangeProvisionSeatApplicationRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeChangeProvisionSeatApplicationCommand(provisionId, request.SeatApplication),
                cancellationToken));

        [HttpPost("{provisionId:long}/TravelDate/PermittedPeriods")]
        public async Task<IActionResult> AddPermittedTravelPeriod(long provisionId, [FromBody] ProvisionDatePeriodRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeAddProvisionPermittedTravelPeriodCommand(provisionId, request.StartDate, request.EndDate),
                cancellationToken));

        [HttpPut("{provisionId:long}/TravelDate/PermittedPeriods/{rowId:long}")]
        public async Task<IActionResult> ChangePermittedTravelPeriod(
            long provisionId,
            long rowId,
            [FromBody] ProvisionDatePeriodRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeChangeProvisionPermittedTravelPeriodCommand(provisionId, rowId, request.StartDate, request.EndDate),
                cancellationToken));

        [HttpDelete("{provisionId:long}/TravelDate/PermittedPeriods/{rowId:long}")]
        public async Task<IActionResult> RemovePermittedTravelPeriod(long provisionId, long rowId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRemoveProvisionPermittedTravelPeriodCommand(provisionId, rowId), cancellationToken));

        [HttpPost("{provisionId:long}/TravelDate/BlackoutPeriods")]
        public async Task<IActionResult> AddBlackoutPeriod(long provisionId, [FromBody] ProvisionDatePeriodRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeAddProvisionBlackoutPeriodCommand(provisionId, request.StartDate, request.EndDate),
                cancellationToken));

        [HttpPut("{provisionId:long}/TravelDate/BlackoutPeriods/{rowId:long}")]
        public async Task<IActionResult> ChangeBlackoutPeriod(
            long provisionId,
            long rowId,
            [FromBody] ProvisionDatePeriodRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeChangeProvisionBlackoutPeriodCommand(provisionId, rowId, request.StartDate, request.EndDate),
                cancellationToken));

        [HttpDelete("{provisionId:long}/TravelDate/BlackoutPeriods/{rowId:long}")]
        public async Task<IActionResult> RemoveBlackoutPeriod(long provisionId, long rowId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRemoveProvisionBlackoutPeriodCommand(provisionId, rowId), cancellationToken));

        [HttpPost("{provisionId:long}/DayTimeApplication/Windows")]
        public async Task<IActionResult> AddDayTimeWindow(long provisionId, [FromBody] ProvisionDayTimeWindowRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeAddProvisionDayTimeWindowCommand(provisionId, request.DaysOfWeekMask, request.StartLocalTime, request.EndLocalTime, request.Effect),
                cancellationToken));

        [HttpPut("{provisionId:long}/DayTimeApplication/Windows/{rowId:long}")]
        public async Task<IActionResult> ChangeDayTimeWindow(
            long provisionId,
            long rowId,
            [FromBody] ProvisionDayTimeWindowRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeChangeProvisionDayTimeWindowCommand(provisionId, rowId, request.DaysOfWeekMask, request.StartLocalTime, request.EndLocalTime, request.Effect),
                cancellationToken));

        [HttpDelete("{provisionId:long}/DayTimeApplication/Windows/{rowId:long}")]
        public async Task<IActionResult> RemoveDayTimeWindow(long provisionId, long rowId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRemoveProvisionDayTimeWindowCommand(provisionId, rowId), cancellationToken));

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
