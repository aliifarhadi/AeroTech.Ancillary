using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ActivateAncillaryServiceDefinition.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionPricingUnit.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionServiceDateBasis.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ChangeAncillaryServiceDefinition.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReactivateAncillaryServiceDefinition.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.RetireAncillaryServiceDefinition.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReviseAncillaryServiceDefinition.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.SuspendAncillaryServiceDefinition.Backoffice;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById.Backoffice;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm.Backoffice;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated.Backoffice;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryVariants.Backoffice;
using AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using AeroTech.Framework.Presentation.Responses;
using AeroTech.Messages.Ancillary.Enums;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/AncillaryServiceDefinitions")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost("Baggage")]
        [Tags("Backoffice Baggage")]
        [EndpointSummary("Define a baggage product type (catalog)")]
        [EndpointDescription(
            "Creates the Draft catalog definition of a baggage product: A01 Extra checked bag, A02 Extra weight package, A03 Overweight bag, A04 Oversize bag, A05 Cabin bag, A06 Sports and special equipment. " +
            "The definition carries no price, no point of sale and no stock: sale rules are AncillaryProvisions, rates are AncillaryPricings " +
            "and stock is configured in the inventory resources.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> DefineBaggage([FromBody] DefineServiceDefinitionRequest<BaggageSpecificationRequest> request, CancellationToken cancellationToken)
            => DefineAsync(
                AncillaryProfile.Baggage,
                request,
                new ServiceSpecificationInput(Baggage: request.Specification?.ToInput(request.VariantCode)),
                cancellationToken);

        [HttpPut("Baggage/{serviceDefinitionId:long}")]
        [Tags("Backoffice Baggage")]
        [EndpointSummary("Change a Draft baggage product type (catalog)")]
        [EndpointDescription(
            "Replaces the editable fields of a Draft baggage definition. The body is the form member of GET Baggage/{serviceDefinitionId} " +
            "and can be sent back as it was read.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> ChangeBaggage(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<BaggageSpecificationRequest> request,
            CancellationToken cancellationToken)
            => ChangeAsync(
                serviceDefinitionId,
                AncillaryProfile.Baggage,
                request,
                new ServiceSpecificationInput(Baggage: request.Specification?.ToInput(request.VariantCode)),
                cancellationToken);

        [HttpGet("Baggage/{serviceDefinitionId:long}")]
        [Tags("Backoffice Baggage")]
        [EndpointSummary("Read a baggage product type for editing (catalog)")]
        [EndpointDescription(
            "Returns the state of the definition and its form. The form has exactly the members of the PUT body and its specification holds " +
            "only the fields of the variant. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<BackofficeServiceDefinitionFormDto<BaggageVariantSpecificationDto>>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBaggage(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetBaggageServiceDefinitionFormQuery(serviceDefinitionId), cancellationToken));

        [HttpPost("Seat")]
        [Tags("Backoffice Seat")]
        [EndpointSummary("Define a seat product type (catalog)")]
        [EndpointDescription(
            "Creates the Draft catalog definition of a seat product: A07 Standard seat, A08 Preferred seat, A09 Extra seat. " +
            "The definition carries no price, no point of sale and no stock: sale rules are AncillaryProvisions, rates are AncillaryPricings " +
            "and stock is configured in the inventory resources. The seats of a flight are read through AncillaryOffers, not here.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> DefineSeat([FromBody] DefineServiceDefinitionRequest<SeatSpecificationRequest> request, CancellationToken cancellationToken)
            => DefineAsync(
                AncillaryProfile.Seat,
                request,
                new ServiceSpecificationInput(Seat: request.Specification?.ToInput(request.VariantCode)),
                cancellationToken);

        [HttpPut("Seat/{serviceDefinitionId:long}")]
        [Tags("Backoffice Seat")]
        [EndpointSummary("Change a Draft seat product type (catalog)")]
        [EndpointDescription(
            "Replaces the editable fields of a Draft seat definition. The body is the form member of GET Seat/{serviceDefinitionId} " +
            "and can be sent back as it was read.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> ChangeSeat(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<SeatSpecificationRequest> request,
            CancellationToken cancellationToken)
            => ChangeAsync(
                serviceDefinitionId,
                AncillaryProfile.Seat,
                request,
                new ServiceSpecificationInput(Seat: request.Specification?.ToInput(request.VariantCode)),
                cancellationToken);

        [HttpGet("Seat/{serviceDefinitionId:long}")]
        [Tags("Backoffice Seat")]
        [EndpointSummary("Read a seat product type for editing (catalog)")]
        [EndpointDescription(
            "Returns the state of the definition and its form. The form has exactly the members of the PUT body and its specification holds " +
            "only the fields of the variant. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<BackofficeServiceDefinitionFormDto<SeatVariantSpecificationDto>>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSeat(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetSeatServiceDefinitionFormQuery(serviceDefinitionId), cancellationToken));

        [HttpPost("Upgrade")]
        [Tags("Backoffice Upgrade")]
        [EndpointSummary("Define an upgrade product type (catalog)")]
        [EndpointDescription(
            "Creates the Draft catalog definition of an upgrade product: A10 Cabin upgrade. " +
            "The definition carries no price, no point of sale and no stock: sale rules are AncillaryProvisions, rates are AncillaryPricings " +
            "and stock is configured in the inventory resources.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> DefineUpgrade([FromBody] DefineServiceDefinitionRequest<UpgradeSpecificationInput> request, CancellationToken cancellationToken)
            => DefineAsync(
                AncillaryProfile.Upgrade,
                request,
                new ServiceSpecificationInput(Upgrade: request.Specification),
                cancellationToken);

        [HttpPut("Upgrade/{serviceDefinitionId:long}")]
        [Tags("Backoffice Upgrade")]
        [EndpointSummary("Change a Draft upgrade product type (catalog)")]
        [EndpointDescription(
            "Replaces the editable fields of a Draft upgrade definition. The body is the form member of GET Upgrade/{serviceDefinitionId} " +
            "and can be sent back as it was read.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> ChangeUpgrade(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<UpgradeSpecificationInput> request,
            CancellationToken cancellationToken)
            => ChangeAsync(
                serviceDefinitionId,
                AncillaryProfile.Upgrade,
                request,
                new ServiceSpecificationInput(Upgrade: request.Specification),
                cancellationToken);

        [HttpGet("Upgrade/{serviceDefinitionId:long}")]
        [Tags("Backoffice Upgrade")]
        [EndpointSummary("Read an upgrade product type for editing (catalog)")]
        [EndpointDescription(
            "Returns the state of the definition and its form. The form has exactly the members of the PUT body and its specification holds " +
            "only the fields of the variant. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<BackofficeServiceDefinitionFormDto<CabinUpgradeSpecificationDto>>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUpgrade(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetUpgradeServiceDefinitionFormQuery(serviceDefinitionId), cancellationToken));

        [HttpPost("Meal")]
        [Tags("Backoffice Meal")]
        [EndpointSummary("Define a meal product type (catalog)")]
        [EndpointDescription(
            "Creates the Draft catalog definition of a meal product: A11 Free special meal, A12 Paid pre-order meal. " +
            "The definition carries no price, no point of sale and no stock: sale rules are AncillaryProvisions, rates are AncillaryPricings " +
            "and stock is configured in the inventory resources.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> DefineMeal([FromBody] DefineServiceDefinitionRequest<MealSpecificationRequest> request, CancellationToken cancellationToken)
            => DefineAsync(
                AncillaryProfile.Meal,
                request,
                new ServiceSpecificationInput(Meal: request.Specification?.ToInput(request.VariantCode)),
                cancellationToken);

        [HttpPut("Meal/{serviceDefinitionId:long}")]
        [Tags("Backoffice Meal")]
        [EndpointSummary("Change a Draft meal product type (catalog)")]
        [EndpointDescription(
            "Replaces the editable fields of a Draft meal definition. The body is the form member of GET Meal/{serviceDefinitionId} " +
            "and can be sent back as it was read.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> ChangeMeal(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<MealSpecificationRequest> request,
            CancellationToken cancellationToken)
            => ChangeAsync(
                serviceDefinitionId,
                AncillaryProfile.Meal,
                request,
                new ServiceSpecificationInput(Meal: request.Specification?.ToInput(request.VariantCode)),
                cancellationToken);

        [HttpGet("Meal/{serviceDefinitionId:long}")]
        [Tags("Backoffice Meal")]
        [EndpointSummary("Read a meal product type for editing (catalog)")]
        [EndpointDescription(
            "Returns the state of the definition and its form. The form has exactly the members of the PUT body and its specification holds " +
            "only the fields of the variant. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<BackofficeServiceDefinitionFormDto<MealVariantSpecificationDto>>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMeal(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetMealServiceDefinitionFormQuery(serviceDefinitionId), cancellationToken));

        [HttpPost("Pet")]
        [Tags("Backoffice Pet")]
        [EndpointSummary("Define a pet product type (catalog)")]
        [EndpointDescription(
            "Creates the Draft catalog definition of a pet product: A13 Pet in cabin, A14 Pet in hold. " +
            "The definition carries no price, no point of sale and no stock: sale rules are AncillaryProvisions, rates are AncillaryPricings " +
            "and stock is configured in the inventory resources.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> DefinePet([FromBody] DefineServiceDefinitionRequest<PetSpecificationRequest> request, CancellationToken cancellationToken)
            => DefineAsync(
                AncillaryProfile.Pet,
                request,
                new ServiceSpecificationInput(Pet: request.Specification?.ToInput(request.VariantCode)),
                cancellationToken);

        [HttpPut("Pet/{serviceDefinitionId:long}")]
        [Tags("Backoffice Pet")]
        [EndpointSummary("Change a Draft pet product type (catalog)")]
        [EndpointDescription(
            "Replaces the editable fields of a Draft pet definition. The body is the form member of GET Pet/{serviceDefinitionId} " +
            "and can be sent back as it was read.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> ChangePet(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<PetSpecificationRequest> request,
            CancellationToken cancellationToken)
            => ChangeAsync(
                serviceDefinitionId,
                AncillaryProfile.Pet,
                request,
                new ServiceSpecificationInput(Pet: request.Specification?.ToInput(request.VariantCode)),
                cancellationToken);

        [HttpGet("Pet/{serviceDefinitionId:long}")]
        [Tags("Backoffice Pet")]
        [EndpointSummary("Read a pet product type for editing (catalog)")]
        [EndpointDescription(
            "Returns the state of the definition and its form. The form has exactly the members of the PUT body and its specification holds " +
            "only the fields of the variant. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<BackofficeServiceDefinitionFormDto<PetVariantSpecificationDto>>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPet(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetPetServiceDefinitionFormQuery(serviceDefinitionId), cancellationToken));

        [HttpPost("AssistedTravel")]
        [Tags("Backoffice AssistedTravel")]
        [EndpointSummary("Define an assisted travel product type (catalog)")]
        [EndpointDescription(
            "Creates the Draft catalog definition of an assisted travel product: A15 Wheelchair, A16 Disability assistance, A17 Medical equipment, A18 Bassinet, A19 Unaccompanied minor. " +
            "The definition carries no price, no point of sale and no stock: sale rules are AncillaryProvisions, rates are AncillaryPricings " +
            "and stock is configured in the inventory resources.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> DefineAssistedTravel([FromBody] DefineServiceDefinitionRequest<AssistedTravelSpecificationRequest> request, CancellationToken cancellationToken)
            => DefineAsync(
                AncillaryProfile.AssistedTravel,
                request,
                new ServiceSpecificationInput(AssistedTravel: request.Specification?.ToInput(request.VariantCode)),
                cancellationToken);

        [HttpPut("AssistedTravel/{serviceDefinitionId:long}")]
        [Tags("Backoffice AssistedTravel")]
        [EndpointSummary("Change a Draft assisted travel product type (catalog)")]
        [EndpointDescription(
            "Replaces the editable fields of a Draft assisted travel definition. The body is the form member of GET AssistedTravel/{serviceDefinitionId} " +
            "and can be sent back as it was read.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> ChangeAssistedTravel(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<AssistedTravelSpecificationRequest> request,
            CancellationToken cancellationToken)
            => ChangeAsync(
                serviceDefinitionId,
                AncillaryProfile.AssistedTravel,
                request,
                new ServiceSpecificationInput(AssistedTravel: request.Specification?.ToInput(request.VariantCode)),
                cancellationToken);

        [HttpGet("AssistedTravel/{serviceDefinitionId:long}")]
        [Tags("Backoffice AssistedTravel")]
        [EndpointSummary("Read an assisted travel product type for editing (catalog)")]
        [EndpointDescription(
            "Returns the state of the definition and its form. The form has exactly the members of the PUT body and its specification holds " +
            "only the fields of the variant. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<BackofficeServiceDefinitionFormDto<AssistedTravelVariantSpecificationDto>>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAssistedTravel(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetAssistedTravelServiceDefinitionFormQuery(serviceDefinitionId), cancellationToken));

        [HttpPost("AirportService")]
        [Tags("Backoffice AirportService")]
        [EndpointSummary("Define an airport service product type (catalog)")]
        [EndpointDescription(
            "Creates the Draft catalog definition of an airport service product: A20 Lounge, A21 Fast track, A22 CIP meet and assist. " +
            "The definition carries no price, no point of sale and no stock: sale rules are AncillaryProvisions, rates are AncillaryPricings " +
            "and stock is configured in the inventory resources.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> DefineAirportService([FromBody] DefineServiceDefinitionRequest<AirportServiceSpecificationRequest> request, CancellationToken cancellationToken)
            => DefineAsync(
                AncillaryProfile.AirportService,
                request,
                new ServiceSpecificationInput(AirportService: request.Specification?.ToInput(request.VariantCode)),
                cancellationToken);

        [HttpPut("AirportService/{serviceDefinitionId:long}")]
        [Tags("Backoffice AirportService")]
        [EndpointSummary("Change a Draft airport service product type (catalog)")]
        [EndpointDescription(
            "Replaces the editable fields of a Draft airport service definition. The body is the form member of GET AirportService/{serviceDefinitionId} " +
            "and can be sent back as it was read.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> ChangeAirportService(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<AirportServiceSpecificationRequest> request,
            CancellationToken cancellationToken)
            => ChangeAsync(
                serviceDefinitionId,
                AncillaryProfile.AirportService,
                request,
                new ServiceSpecificationInput(AirportService: request.Specification?.ToInput(request.VariantCode)),
                cancellationToken);

        [HttpGet("AirportService/{serviceDefinitionId:long}")]
        [Tags("Backoffice AirportService")]
        [EndpointSummary("Read an airport service product type for editing (catalog)")]
        [EndpointDescription(
            "Returns the state of the definition and its form. The form has exactly the members of the PUT body and its specification holds " +
            "only the fields of the variant. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<BackofficeServiceDefinitionFormDto<AirportServiceVariantSpecificationDto>>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAirportService(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetAirportServiceServiceDefinitionFormQuery(serviceDefinitionId), cancellationToken));

        [HttpPost("Priority")]
        [Tags("Backoffice Priority")]
        [EndpointSummary("Define a priority product type (catalog)")]
        [EndpointDescription(
            "Creates the Draft catalog definition of a priority product: A23 Priority boarding and check-in. " +
            "The definition carries no price, no point of sale and no stock: sale rules are AncillaryProvisions, rates are AncillaryPricings " +
            "and stock is configured in the inventory resources.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> DefinePriority([FromBody] DefineServiceDefinitionRequest<PrioritySpecificationInput> request, CancellationToken cancellationToken)
            => DefineAsync(
                AncillaryProfile.Priority,
                request,
                new ServiceSpecificationInput(Priority: request.Specification),
                cancellationToken);

        [HttpPut("Priority/{serviceDefinitionId:long}")]
        [Tags("Backoffice Priority")]
        [EndpointSummary("Change a Draft priority product type (catalog)")]
        [EndpointDescription(
            "Replaces the editable fields of a Draft priority definition. The body is the form member of GET Priority/{serviceDefinitionId} " +
            "and can be sent back as it was read.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> ChangePriority(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<PrioritySpecificationInput> request,
            CancellationToken cancellationToken)
            => ChangeAsync(
                serviceDefinitionId,
                AncillaryProfile.Priority,
                request,
                new ServiceSpecificationInput(Priority: request.Specification),
                cancellationToken);

        [HttpGet("Priority/{serviceDefinitionId:long}")]
        [Tags("Backoffice Priority")]
        [EndpointSummary("Read a priority product type for editing (catalog)")]
        [EndpointDescription(
            "Returns the state of the definition and its form. The form has exactly the members of the PUT body and its specification holds " +
            "only the fields of the variant. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<BackofficeServiceDefinitionFormDto<PriorityBoardingCheckinSpecificationDto>>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPriority(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetPriorityServiceDefinitionFormQuery(serviceDefinitionId), cancellationToken));

        [HttpPost("Connectivity")]
        [Tags("Backoffice Connectivity")]
        [EndpointSummary("Define a connectivity product type (catalog)")]
        [EndpointDescription(
            "Creates the Draft catalog definition of a connectivity product: A24 Onboard Wi-Fi. " +
            "The definition carries no price, no point of sale and no stock: sale rules are AncillaryProvisions, rates are AncillaryPricings " +
            "and stock is configured in the inventory resources.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> DefineConnectivity([FromBody] DefineServiceDefinitionRequest<ConnectivitySpecificationInput> request, CancellationToken cancellationToken)
            => DefineAsync(
                AncillaryProfile.Connectivity,
                request,
                new ServiceSpecificationInput(Connectivity: request.Specification),
                cancellationToken);

        [HttpPut("Connectivity/{serviceDefinitionId:long}")]
        [Tags("Backoffice Connectivity")]
        [EndpointSummary("Change a Draft connectivity product type (catalog)")]
        [EndpointDescription(
            "Replaces the editable fields of a Draft connectivity definition. The body is the form member of GET Connectivity/{serviceDefinitionId} " +
            "and can be sent back as it was read.")]
        [ProducesResponseType<ApiResult<ServiceDefinitionResult>>(StatusCodes.Status200OK)]
        public Task<IActionResult> ChangeConnectivity(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<ConnectivitySpecificationInput> request,
            CancellationToken cancellationToken)
            => ChangeAsync(
                serviceDefinitionId,
                AncillaryProfile.Connectivity,
                request,
                new ServiceSpecificationInput(Connectivity: request.Specification),
                cancellationToken);

        [HttpGet("Connectivity/{serviceDefinitionId:long}")]
        [Tags("Backoffice Connectivity")]
        [EndpointSummary("Read a connectivity product type for editing (catalog)")]
        [EndpointDescription(
            "Returns the state of the definition and its form. The form has exactly the members of the PUT body and its specification holds " +
            "only the fields of the variant. A definition of another profile answers 409 with code 16219.")]
        [ProducesResponseType<ApiResult<BackofficeServiceDefinitionFormDto<OnboardWifiSpecificationDto>>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConnectivity(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetConnectivityServiceDefinitionFormQuery(serviceDefinitionId), cancellationToken));

        [HttpPost("{serviceDefinitionId:long}/Activate")]
        public async Task<IActionResult> Activate(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeActivateAncillaryServiceDefinitionCommand(serviceDefinitionId), cancellationToken));

        [HttpPost("{serviceDefinitionId:long}/Suspend")]
        public async Task<IActionResult> Suspend(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeSuspendAncillaryServiceDefinitionCommand(serviceDefinitionId), cancellationToken));

        [HttpPost("{serviceDefinitionId:long}/Reactivate")]
        public async Task<IActionResult> Reactivate(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeReactivateAncillaryServiceDefinitionCommand(serviceDefinitionId), cancellationToken));

        [HttpPost("{serviceDefinitionId:long}/Retire")]
        public async Task<IActionResult> Retire(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRetireAncillaryServiceDefinitionCommand(serviceDefinitionId), cancellationToken));

        [HttpPost("{serviceDefinitionId:long}/Revise")]
        public async Task<IActionResult> Revise(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeReviseAncillaryServiceDefinitionCommand(serviceDefinitionId), cancellationToken));

        [HttpPost("{serviceDefinitionId:long}/AssignPricingUnit")]
        public async Task<IActionResult> AssignPricingUnit(
            long serviceDefinitionId,
            [FromBody] AssignPricingUnitRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeAssignAncillaryServiceDefinitionPricingUnitCommand(serviceDefinitionId, request.PricingUnit),
                cancellationToken));

        [HttpPost("{serviceDefinitionId:long}/AssignServiceDateBasis")]
        public async Task<IActionResult> AssignServiceDateBasis(
            long serviceDefinitionId,
            [FromBody] AssignServiceDateBasisRequest request,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(
                new BackofficeAssignAncillaryServiceDefinitionServiceDateBasisCommand(serviceDefinitionId, request.ServiceDateBasis),
                cancellationToken));

        [HttpGet("Paginated")]
        public async Task<IActionResult> Paginated(
            [FromQuery] BackofficeGetAncillaryServiceDefinitionsPaginatedQuery query,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("Variants")]
        public async Task<IActionResult> Variants([FromQuery] BackofficeGetAncillaryVariantsQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("{serviceDefinitionId:long}")]
        public async Task<IActionResult> GetById(long serviceDefinitionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetAncillaryServiceDefinitionByIdQuery(serviceDefinitionId), cancellationToken));

        private async Task<IActionResult> DefineAsync<TSpecification>(
            AncillaryProfile profile,
            DefineServiceDefinitionRequest<TSpecification> request,
            ServiceSpecificationInput specification,
            CancellationToken cancellationToken)
            where TSpecification : class
            => Ok(await _mediator.Send(new BackofficeDefineAncillaryServiceDefinitionCommand(
                request.OwnerAirlineId,
                request.SupplierId,
                request.ServiceDefinitionRef,
                request.ServiceSubCode,
                request.SubCodeSource,
                request.ServiceTypeCode,
                request.GroupCode,
                request.SubGroupCode,
                request.Description1Code,
                request.Description2Code,
                request.PricingUnit,
                request.ServiceDateBasis,
                profile,
                request.VariantCode,
                request.DocumentRouting,
                specification,
                request.CommercialName,
                request.Description,
                request.Document,
                request.Booking,
                request.SalesEffectiveFrom,
                request.SalesDiscontinueOn), cancellationToken));

        private async Task<IActionResult> ChangeAsync<TSpecification>(
            long serviceDefinitionId,
            AncillaryProfile profile,
            ChangeServiceDefinitionRequest<TSpecification> request,
            ServiceSpecificationInput specification,
            CancellationToken cancellationToken)
            where TSpecification : class
            => Ok(await _mediator.Send(new BackofficeChangeAncillaryServiceDefinitionCommand(
                serviceDefinitionId,
                request.SupplierId,
                request.ServiceSubCode,
                request.SubCodeSource,
                request.ServiceTypeCode,
                request.GroupCode,
                request.SubGroupCode,
                request.Description1Code,
                request.Description2Code,
                request.PricingUnit,
                request.ServiceDateBasis,
                profile,
                request.VariantCode,
                request.DocumentRouting,
                specification,
                request.CommercialName,
                request.Description,
                request.Document,
                request.Booking,
                request.SalesEffectiveFrom,
                request.SalesDiscontinueOn), cancellationToken));
    }
}
