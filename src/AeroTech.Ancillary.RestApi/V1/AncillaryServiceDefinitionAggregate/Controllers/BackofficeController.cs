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
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById.Backoffice;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated.Backoffice;
using AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
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
        public Task<IActionResult> DefineBaggage([FromBody] DefineServiceDefinitionRequest<BaggageSpecificationInput> request, CancellationToken cancellationToken)
            => DefineAsync(AncillaryProfile.Baggage, request, new ServiceSpecificationInput(Baggage: request.Specification), cancellationToken);

        [HttpPut("Baggage/{serviceDefinitionId:long}")]
        [Tags("Backoffice Baggage")]
        public Task<IActionResult> ChangeBaggage(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<BaggageSpecificationInput> request,
            CancellationToken cancellationToken)
            => ChangeAsync(serviceDefinitionId, AncillaryProfile.Baggage, request, new ServiceSpecificationInput(Baggage: request.Specification), cancellationToken);

        [HttpPost("Seat")]
        [Tags("Backoffice Seat")]
        public Task<IActionResult> DefineSeat([FromBody] DefineServiceDefinitionRequest<SeatSpecificationInput> request, CancellationToken cancellationToken)
            => DefineAsync(AncillaryProfile.Seat, request, new ServiceSpecificationInput(Seat: request.Specification), cancellationToken);

        [HttpPut("Seat/{serviceDefinitionId:long}")]
        [Tags("Backoffice Seat")]
        public Task<IActionResult> ChangeSeat(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<SeatSpecificationInput> request,
            CancellationToken cancellationToken)
            => ChangeAsync(serviceDefinitionId, AncillaryProfile.Seat, request, new ServiceSpecificationInput(Seat: request.Specification), cancellationToken);

        [HttpPost("Upgrade")]
        [Tags("Backoffice Upgrade")]
        public Task<IActionResult> DefineUpgrade([FromBody] DefineServiceDefinitionRequest<UpgradeSpecificationInput> request, CancellationToken cancellationToken)
            => DefineAsync(AncillaryProfile.Upgrade, request, new ServiceSpecificationInput(Upgrade: request.Specification), cancellationToken);

        [HttpPut("Upgrade/{serviceDefinitionId:long}")]
        [Tags("Backoffice Upgrade")]
        public Task<IActionResult> ChangeUpgrade(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<UpgradeSpecificationInput> request,
            CancellationToken cancellationToken)
            => ChangeAsync(serviceDefinitionId, AncillaryProfile.Upgrade, request, new ServiceSpecificationInput(Upgrade: request.Specification), cancellationToken);

        [HttpPost("Meal")]
        [Tags("Backoffice Meal")]
        public Task<IActionResult> DefineMeal([FromBody] DefineServiceDefinitionRequest<MealSpecificationInput> request, CancellationToken cancellationToken)
            => DefineAsync(AncillaryProfile.Meal, request, new ServiceSpecificationInput(Meal: request.Specification), cancellationToken);

        [HttpPut("Meal/{serviceDefinitionId:long}")]
        [Tags("Backoffice Meal")]
        public Task<IActionResult> ChangeMeal(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<MealSpecificationInput> request,
            CancellationToken cancellationToken)
            => ChangeAsync(serviceDefinitionId, AncillaryProfile.Meal, request, new ServiceSpecificationInput(Meal: request.Specification), cancellationToken);

        [HttpPost("Pet")]
        [Tags("Backoffice Pet")]
        public Task<IActionResult> DefinePet([FromBody] DefineServiceDefinitionRequest<PetSpecificationInput> request, CancellationToken cancellationToken)
            => DefineAsync(AncillaryProfile.Pet, request, new ServiceSpecificationInput(Pet: request.Specification), cancellationToken);

        [HttpPut("Pet/{serviceDefinitionId:long}")]
        [Tags("Backoffice Pet")]
        public Task<IActionResult> ChangePet(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<PetSpecificationInput> request,
            CancellationToken cancellationToken)
            => ChangeAsync(serviceDefinitionId, AncillaryProfile.Pet, request, new ServiceSpecificationInput(Pet: request.Specification), cancellationToken);

        [HttpPost("AssistedTravel")]
        [Tags("Backoffice AssistedTravel")]
        public Task<IActionResult> DefineAssistedTravel([FromBody] DefineServiceDefinitionRequest<AssistedTravelSpecificationInput> request, CancellationToken cancellationToken)
            => DefineAsync(AncillaryProfile.AssistedTravel, request, new ServiceSpecificationInput(AssistedTravel: request.Specification), cancellationToken);

        [HttpPut("AssistedTravel/{serviceDefinitionId:long}")]
        [Tags("Backoffice AssistedTravel")]
        public Task<IActionResult> ChangeAssistedTravel(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<AssistedTravelSpecificationInput> request,
            CancellationToken cancellationToken)
            => ChangeAsync(serviceDefinitionId, AncillaryProfile.AssistedTravel, request, new ServiceSpecificationInput(AssistedTravel: request.Specification), cancellationToken);

        [HttpPost("AirportService")]
        [Tags("Backoffice AirportService")]
        public Task<IActionResult> DefineAirportService([FromBody] DefineServiceDefinitionRequest<AirportServiceSpecificationInput> request, CancellationToken cancellationToken)
            => DefineAsync(AncillaryProfile.AirportService, request, new ServiceSpecificationInput(AirportService: request.Specification), cancellationToken);

        [HttpPut("AirportService/{serviceDefinitionId:long}")]
        [Tags("Backoffice AirportService")]
        public Task<IActionResult> ChangeAirportService(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<AirportServiceSpecificationInput> request,
            CancellationToken cancellationToken)
            => ChangeAsync(serviceDefinitionId, AncillaryProfile.AirportService, request, new ServiceSpecificationInput(AirportService: request.Specification), cancellationToken);

        [HttpPost("Priority")]
        [Tags("Backoffice Priority")]
        public Task<IActionResult> DefinePriority([FromBody] DefineServiceDefinitionRequest<PrioritySpecificationInput> request, CancellationToken cancellationToken)
            => DefineAsync(AncillaryProfile.Priority, request, new ServiceSpecificationInput(Priority: request.Specification), cancellationToken);

        [HttpPut("Priority/{serviceDefinitionId:long}")]
        [Tags("Backoffice Priority")]
        public Task<IActionResult> ChangePriority(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<PrioritySpecificationInput> request,
            CancellationToken cancellationToken)
            => ChangeAsync(serviceDefinitionId, AncillaryProfile.Priority, request, new ServiceSpecificationInput(Priority: request.Specification), cancellationToken);

        [HttpPost("Connectivity")]
        [Tags("Backoffice Connectivity")]
        public Task<IActionResult> DefineConnectivity([FromBody] DefineServiceDefinitionRequest<ConnectivitySpecificationInput> request, CancellationToken cancellationToken)
            => DefineAsync(AncillaryProfile.Connectivity, request, new ServiceSpecificationInput(Connectivity: request.Specification), cancellationToken);

        [HttpPut("Connectivity/{serviceDefinitionId:long}")]
        [Tags("Backoffice Connectivity")]
        public Task<IActionResult> ChangeConnectivity(
            long serviceDefinitionId,
            [FromBody] ChangeServiceDefinitionRequest<ConnectivitySpecificationInput> request,
            CancellationToken cancellationToken)
            => ChangeAsync(serviceDefinitionId, AncillaryProfile.Connectivity, request, new ServiceSpecificationInput(Connectivity: request.Specification), cancellationToken);

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
