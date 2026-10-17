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
    [Tags("Backoffice AssistedTravel")]
    [Route($"Backoffice/v{{version:apiVersion}}/AncillaryServiceDefinitions")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class AssistedTravelBackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AssistedTravelBackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpGet("AssistedTravel/{serviceDefinitionId:long}/ProvisionForm")]
        [EndpointSummary("Read what a new assisted travel provision of this product type may hold")]
        [EndpointDescription(
            "Returns the variant, the fixed application type, the rule sections an assisted travel provision may carry and the limits the server enforces. " +
            "It holds no provision and no default value. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<BackofficeProfileProvisionCreateFormDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProvisionForm(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetProfileProvisionCreateFormQuery(AncillaryProfile.AssistedTravel, serviceDefinitionId), cancellationToken));

        [HttpPost("AssistedTravel/{serviceDefinitionId:long}/Provisions")]
        [EndpointSummary("Define an assisted travel provision of this product type")]
        [EndpointDescription(
            "The product type comes from the route and the application type from the variant. The body holds the shared fields and the AssistedTravelRule section; " +
            "any other member is refused with 400. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<ProvisionResult>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Define(long serviceDefinitionId, [FromBody] AssistedTravelProvisionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeDefineAssistedTravelProvisionCommand(
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
                request.AssistedTravelRule), cancellationToken));

        [HttpGet("AssistedTravel/{serviceDefinitionId:long}/Provisions/{provisionId:long}/Form")]
        [EndpointSummary("Read an assisted travel provision for editing")]
        [EndpointDescription(
            "Returns the state of the provision and its form. The form has exactly the members of the PUT body and can be sent back as it was read. " +
            "A provision of another product type answers 404 with code 16301.")]
        [ProducesResponseType<ApiResult<BackofficeProfileProvisionFormDto<AssistedTravelProvisionFormDto>>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetForm(long serviceDefinitionId, long provisionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetAssistedTravelProvisionFormQuery(serviceDefinitionId, provisionId), cancellationToken));

        [HttpPut("AssistedTravel/{serviceDefinitionId:long}/Provisions/{provisionId:long}")]
        [EndpointSummary("Change a draft assisted travel provision")]
        [EndpointDescription(
            "Replaces the draft with the body, which holds the shared fields and the AssistedTravelRule section. Only a draft is changed. " +
            "Settlement.CancellationRules left out or null keeps the stored rules and an empty list clears them. " +
            "A provision of another product type answers 404 with code 16301.")]
        [ProducesResponseType<ApiResult<ProvisionResult>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Change(
            long serviceDefinitionId,
            long provisionId,
            [FromBody] AssistedTravelProvisionRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangeAssistedTravelProvisionCommand(
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
                request.AssistedTravelRule), cancellationToken));
    }
}
