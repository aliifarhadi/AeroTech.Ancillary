using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetSelectionOptions.Api;
using AeroTech.Ancillary.RestApi.V1._Shared;
using AeroTech.Messages.Ancillary.Enums;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryOffers.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Api")]
    [Route($"Api/v{{version:apiVersion}}/AncillaryOffers")]
    [Authorize(SurfaceAuthorization.Ota)]
    public sealed class OtaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OtaController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> GetByOfferId([FromBody] ApiGetAncillaryOffersQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("{family}/{reference}/Options")]
        public async Task<IActionResult> GetOptions(string family, string reference, [FromQuery] string? offerId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ApiGetSelectionOptionsQuery(family, reference, offerId), cancellationToken));

        [HttpPost("Baggages/{reference}/Options")]
        public async Task<IActionResult> SubmitBaggageOptions(string reference, [FromQuery] string? offerId, [FromBody] BaggageSelectionOptions options, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ApiSubmitSelectionOptionsCommand(reference, offerId, AncillaryProfile.Baggage, options), cancellationToken));

        [HttpPost("Pets/{reference}/Options")]
        public async Task<IActionResult> SubmitPetOptions(string reference, [FromQuery] string? offerId, [FromBody] PetSelectionOptions options, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ApiSubmitSelectionOptionsCommand(reference, offerId, AncillaryProfile.Pet, options), cancellationToken));

        [HttpPost("AssistedTravel/{reference}/Options")]
        public async Task<IActionResult> SubmitAssistedTravelOptions(string reference, [FromQuery] string? offerId, [FromBody] AssistedTravelSelectionOptions options, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ApiSubmitSelectionOptionsCommand(reference, offerId, AncillaryProfile.AssistedTravel, options), cancellationToken));

        [HttpPost("AirportServices/{reference}/Options")]
        public async Task<IActionResult> SubmitAirportServiceOptions(string reference, [FromQuery] string? offerId, [FromBody] AirportServiceSelectionOptions options, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ApiSubmitSelectionOptionsCommand(reference, offerId, AncillaryProfile.AirportService, options), cancellationToken));

        [HttpPost("PriorityServices/{reference}/Options")]
        public async Task<IActionResult> SubmitPriorityOptions(string reference, [FromQuery] string? offerId, [FromBody] PrioritySelectionOptions options, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ApiSubmitSelectionOptionsCommand(reference, offerId, AncillaryProfile.Priority, options), cancellationToken));

        [HttpPost("Connectivity/{reference}/Options")]
        public async Task<IActionResult> SubmitConnectivityOptions(string reference, [FromQuery] string? offerId, [FromBody] ConnectivitySelectionOptions options, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ApiSubmitSelectionOptionsCommand(reference, offerId, AncillaryProfile.Connectivity, options), cancellationToken));
    }
}
