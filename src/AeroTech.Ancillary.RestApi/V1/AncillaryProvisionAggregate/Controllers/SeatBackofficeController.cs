using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionCreateForm.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionForm.Backoffice;
using AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
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
    [Tags("Backoffice Seat")]
    [Route($"Backoffice/v{{version:apiVersion}}/AncillaryServiceDefinitions")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class SeatBackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SeatBackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpGet("Seat/{serviceDefinitionId:long}/ProvisionForm")]
        [EndpointSummary("Read what a new seat provision of this product type may hold")]
        [EndpointDescription(
            "Returns the variant, the fixed application type, the rule sections a seat provision may carry and the limits the server enforces. " +
            "It holds no provision and no default value. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<BackofficeSeatProvisionCreateFormDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProvisionForm(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetSeatProvisionCreateFormQuery(serviceDefinitionId), cancellationToken));

        [HttpPost("Seat/{serviceDefinitionId:long}/Provisions")]
        [EndpointSummary("Define a seat provision of this product type")]
        [EndpointDescription(
            "The product type comes from the route and the application type is Seat. The body holds only the seat fields; " +
            "any other member is refused with 400. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<ProvisionResult>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Define(long serviceDefinitionId, [FromBody] SeatProvisionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeDefineSeatProvisionCommand(
                serviceDefinitionId,
                request.Sequence,
                request.CoverageScope,
                request.PurchaseStage,
                request.PriceOrigin,
                request.QuoteProviderKey,
                request.Quantity,
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
                request.SeatApplication), cancellationToken));

        [HttpGet("Seat/{serviceDefinitionId:long}/Provisions/{provisionId:long}/Form")]
        [EndpointSummary("Read a seat provision for editing")]
        [EndpointDescription(
            "Returns the state of the provision and its form. The form has exactly the members of the PUT body and can be sent back as it was read. " +
            "A provision of another product type answers 404 with code 16301.")]
        [ProducesResponseType<ApiResult<BackofficeSeatProvisionFormDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetForm(long serviceDefinitionId, long provisionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetSeatProvisionFormQuery(serviceDefinitionId, provisionId), cancellationToken));

        [HttpPut("Seat/{serviceDefinitionId:long}/Provisions/{provisionId:long}")]
        [EndpointSummary("Change a draft seat provision")]
        [EndpointDescription(
            "Replaces the draft with the body, which holds only the seat fields. Only a draft is changed. " +
            "A provision of another product type answers 404 with code 16301.")]
        [ProducesResponseType<ApiResult<ProvisionResult>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Change(
            long serviceDefinitionId,
            long provisionId,
            [FromBody] SeatProvisionRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangeSeatProvisionCommand(
                serviceDefinitionId,
                provisionId,
                request.Sequence,
                request.CoverageScope,
                request.PurchaseStage,
                request.PriceOrigin,
                request.QuoteProviderKey,
                request.Quantity,
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
                request.SeatApplication), cancellationToken));
    }
}
