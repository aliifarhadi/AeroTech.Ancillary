using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionCreateForm.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionForm.Backoffice;
using AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using AeroTech.Framework.Presentation.Responses;
using AeroTech.Messages.Ancillary.Enums;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Backoffice Priority")]
    [Route($"Backoffice/v{{version:apiVersion}}/AncillaryServiceDefinitions")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class PriorityBackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PriorityBackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpGet("Priority/{serviceDefinitionId:long}/ProvisionForm")]
        [EndpointSummary("Read what a new priority provision of this product type may hold")]
        [EndpointDescription(
            "Returns the variant, the fixed application type, the rule sections a priority provision may carry and the limits the server enforces. " +
            "It holds no provision and no default value. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<BackofficeProfileProvisionCreateFormDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProvisionForm(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetProfileProvisionCreateFormQuery(AncillaryProfile.Priority, serviceDefinitionId), cancellationToken));

        [HttpPost("Priority/{serviceDefinitionId:long}/Provisions")]
        [EndpointSummary("Define a priority provision of this product type")]
        [EndpointDescription(
            "The product type comes from the route and the application type from the variant. The body holds only the shared fields; " +
            "any other member is refused with 400. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<ProvisionResult>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Define(long serviceDefinitionId, [FromBody] StandardProvisionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeDefineStandardProvisionCommand(
                AncillaryProfile.Priority,
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
                request.AdvancePurchase), cancellationToken));

        [HttpGet("Priority/{serviceDefinitionId:long}/Provisions/{provisionId:long}/Form")]
        [EndpointSummary("Read a priority provision for editing")]
        [EndpointDescription(
            "Returns the state of the provision and its form. The form has exactly the members of the PUT body and can be sent back as it was read. " +
            "A provision of another product type answers 404 with code 16301.")]
        [ProducesResponseType<ApiResult<BackofficeProfileProvisionFormDto<StandardProvisionFormDto>>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetForm(long serviceDefinitionId, long provisionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetStandardProvisionFormQuery(AncillaryProfile.Priority, serviceDefinitionId, provisionId), cancellationToken));

        [HttpPut("Priority/{serviceDefinitionId:long}/Provisions/{provisionId:long}")]
        [EndpointSummary("Change a draft priority provision")]
        [EndpointDescription(
            "Replaces the draft with the body, which holds only the shared fields. Only a draft is changed. " +
            "Settlement.CancellationRules left out or null keeps the stored rules and an empty list clears them. " +
            "A provision of another product type answers 404 with code 16301.")]
        [ProducesResponseType<ApiResult<ProvisionResult>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Change(
            long serviceDefinitionId,
            long provisionId,
            [FromBody] StandardProvisionRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangeStandardProvisionCommand(
                AncillaryProfile.Priority,
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
                request.AdvancePurchase), cancellationToken));
    }
}
