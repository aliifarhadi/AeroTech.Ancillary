using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetSelectionOptions.OtaPanel;
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
    [Tags("OtaPanel")]
    [Route($"OtaPanel/v{{version:apiVersion}}/AncillaryOffers")]
    [Authorize(SurfaceAuthorization.OtaPanel)]
    public sealed class OtaPanelController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OtaPanelController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> GetByOfferId([FromBody] OtaPanelGetAncillaryOffersQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("{family}/{reference}/Options")]
        public async Task<IActionResult> GetOptions(string family, string reference, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new OtaPanelGetSelectionOptionsQuery(family, reference), cancellationToken));

        [HttpPost("Baggages/{reference}/Options")]
        public async Task<IActionResult> SubmitBaggageOptions(string reference, [FromBody] BaggageSelectionOptions options, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new OtaPanelSubmitSelectionOptionsCommand(reference, AncillaryProfile.Baggage, options), cancellationToken));

        [HttpPost("Pets/{reference}/Options")]
        public async Task<IActionResult> SubmitPetOptions(string reference, [FromBody] PetSelectionOptions options, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new OtaPanelSubmitSelectionOptionsCommand(reference, AncillaryProfile.Pet, options), cancellationToken));

        [HttpPost("AssistedTravel/{reference}/Options")]
        public async Task<IActionResult> SubmitAssistedTravelOptions(string reference, [FromBody] AssistedTravelSelectionOptions options, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new OtaPanelSubmitSelectionOptionsCommand(reference, AncillaryProfile.AssistedTravel, options), cancellationToken));

        [HttpPost("AirportServices/{reference}/Options")]
        public async Task<IActionResult> SubmitAirportServiceOptions(string reference, [FromBody] AirportServiceSelectionOptions options, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new OtaPanelSubmitSelectionOptionsCommand(reference, AncillaryProfile.AirportService, options), cancellationToken));

        [HttpPost("PriorityServices/{reference}/Options")]
        public async Task<IActionResult> SubmitPriorityOptions(string reference, [FromBody] PrioritySelectionOptions options, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new OtaPanelSubmitSelectionOptionsCommand(reference, AncillaryProfile.Priority, options), cancellationToken));

        [HttpPost("Connectivity/{reference}/Options")]
        public async Task<IActionResult> SubmitConnectivityOptions(string reference, [FromBody] ConnectivitySelectionOptions options, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new OtaPanelSubmitSelectionOptionsCommand(reference, AncillaryProfile.Connectivity, options), cancellationToken));
    }
}
