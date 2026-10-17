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
    [Tags("Backoffice Pet")]
    [Route($"Backoffice/v{{version:apiVersion}}/AncillaryServiceDefinitions")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class PetBackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PetBackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpGet("Pet/{serviceDefinitionId:long}/ProvisionForm")]
        [EndpointSummary("Read what a new pet provision of this product type may hold")]
        [EndpointDescription(
            "Returns the variant, the fixed application type, the rule sections a pet provision may carry and the limits the server enforces. " +
            "It holds no provision and no default value. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<BackofficeProfileProvisionCreateFormDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProvisionForm(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetProfileProvisionCreateFormQuery(AncillaryProfile.Pet, serviceDefinitionId), cancellationToken));

        [HttpPost("Pet/{serviceDefinitionId:long}/Provisions")]
        [EndpointSummary("Define a pet provision of this product type")]
        [EndpointDescription(
            "The product type comes from the route and the application type from the variant. The body holds the shared fields and the PetRule section; " +
            "any other member is refused with 400. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<ProvisionResult>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Define(long serviceDefinitionId, [FromBody] PetProvisionRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeDefinePetProvisionCommand(
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
                request.PetRule), cancellationToken));

        [HttpGet("Pet/{serviceDefinitionId:long}/Provisions/{provisionId:long}/Form")]
        [EndpointSummary("Read a pet provision for editing")]
        [EndpointDescription(
            "Returns the state of the provision and its form. The form has exactly the members of the PUT body and can be sent back as it was read. " +
            "A provision of another product type answers 404 with code 16301.")]
        [ProducesResponseType<ApiResult<BackofficeProfileProvisionFormDto<PetProvisionFormDto>>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetForm(long serviceDefinitionId, long provisionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetPetProvisionFormQuery(serviceDefinitionId, provisionId), cancellationToken));

        [HttpPut("Pet/{serviceDefinitionId:long}/Provisions/{provisionId:long}")]
        [EndpointSummary("Change a draft pet provision")]
        [EndpointDescription(
            "Replaces the draft with the body, which holds the shared fields and the PetRule section. Only a draft is changed. " +
            "Settlement.CancellationRules left out or null keeps the stored rules and an empty list clears them. " +
            "A provision of another product type answers 404 with code 16301.")]
        [ProducesResponseType<ApiResult<ProvisionResult>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Change(
            long serviceDefinitionId,
            long provisionId,
            [FromBody] PetProvisionRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeChangePetProvisionCommand(
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
                request.PetRule), cancellationToken));
    }
}
