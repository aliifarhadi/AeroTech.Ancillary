using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SwitchActiveAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ActivateAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ReactivateAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RetireAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.SuspendAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetProvisionPricingsPaginated.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionForm.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated.Backoffice;
using AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using AeroTech.Framework.Core.Domain.Queries;
using AeroTech.Framework.Presentation.Responses;
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
                request.PriceOrigin,
                request.QuoteProviderKey,
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
                request.SeatApplication,
                request.PetRule,
                request.AssistedTravelRule,
                request.AirportServiceRule), cancellationToken));

        [HttpPut("{provisionId:long}")]
        public async Task<IActionResult> Change(long provisionId, [FromBody] ChangeProvisionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangeAncillaryProvisionCommand(
                provisionId,
                request.Sequence,
                request.CoverageScope,
                request.PurchaseStage,
                request.PriceOrigin,
                request.QuoteProviderKey,
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
                request.SeatApplication,
                request.PetRule,
                request.AssistedTravelRule,
                request.AirportServiceRule), cancellationToken));

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

        [HttpGet("Paginated")]
        public async Task<IActionResult> Paginated(
            [FromQuery] BackofficeGetAncillaryProvisionsPaginatedQuery query,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("{provisionId:long}/Form")]
        [EndpointSummary("Read a sale rule for editing")]
        [EndpointDescription(
            "Returns the state of the provision and its form. The form has exactly the members of the PUT body of the provision, enums as names " +
            "and rule lists as plain values, and can be sent back as it was read.")]
        [ProducesResponseType<ApiResult<BackofficeProvisionFormDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetForm(long provisionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetAncillaryProvisionFormQuery(provisionId), cancellationToken));

        [HttpGet("{provisionId:long}/Pricings")]
        [EndpointSummary("List the pricings of a provision with their rates and price components")]
        [EndpointDescription(
            "One row per pricing version of the provision, newest first. Each row holds its rates and each rate holds its price components, " +
            "so no further call is needed. An unknown provision answers 404 with code 16301.")]
        [ProducesResponseType<ApiResult<GridData<ProvisionPricingRowDto>>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPricings(
            long provisionId,
            [FromQuery] BackofficeGetProvisionPricingsPaginatedQuery query,
            CancellationToken cancellationToken)
        {
            query.AncillaryProvisionId = provisionId;

            return Ok(await _mediator.Send(query, cancellationToken));
        }

        [HttpGet("{provisionId:long}")]
        public async Task<IActionResult> GetById(long provisionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetAncillaryProvisionByIdQuery(provisionId), cancellationToken));
    }
}
