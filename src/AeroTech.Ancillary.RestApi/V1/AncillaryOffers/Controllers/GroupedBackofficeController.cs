using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedOfferDetail.Backoffice;
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
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/AncillaryOffers")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class GroupedBackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GroupedBackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost("Grouped")]
        public async Task<IActionResult> GetGrouped([FromBody] BackofficeGetGroupedAncillaryOffersQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Baggages/Details")]
        public Task<IActionResult> GetBaggageDetail([FromBody] BackofficeGetGroupedOfferDetailQuery query, CancellationToken cancellationToken)
            => DetailAsync(query, AncillaryProfile.Baggage, cancellationToken);

        [HttpPost("Meals/Details")]
        public Task<IActionResult> GetMealDetail([FromBody] BackofficeGetGroupedOfferDetailQuery query, CancellationToken cancellationToken)
            => DetailAsync(query, AncillaryProfile.Meal, cancellationToken);

        [HttpPost("Pets/Details")]
        public Task<IActionResult> GetPetDetail([FromBody] BackofficeGetGroupedOfferDetailQuery query, CancellationToken cancellationToken)
            => DetailAsync(query, AncillaryProfile.Pet, cancellationToken);

        [HttpPost("AssistedTravel/Details")]
        public Task<IActionResult> GetAssistedTravelDetail([FromBody] BackofficeGetGroupedOfferDetailQuery query, CancellationToken cancellationToken)
            => DetailAsync(query, AncillaryProfile.AssistedTravel, cancellationToken);

        [HttpPost("AirportServices/Details")]
        public Task<IActionResult> GetAirportServiceDetail([FromBody] BackofficeGetGroupedOfferDetailQuery query, CancellationToken cancellationToken)
            => DetailAsync(query, AncillaryProfile.AirportService, cancellationToken);

        [HttpPost("PriorityServices/Details")]
        public Task<IActionResult> GetPriorityServiceDetail([FromBody] BackofficeGetGroupedOfferDetailQuery query, CancellationToken cancellationToken)
            => DetailAsync(query, AncillaryProfile.Priority, cancellationToken);

        [HttpPost("Connectivity/Details")]
        public Task<IActionResult> GetConnectivityDetail([FromBody] BackofficeGetGroupedOfferDetailQuery query, CancellationToken cancellationToken)
            => DetailAsync(query, AncillaryProfile.Connectivity, cancellationToken);

        [HttpPost("Upgrades/Details")]
        public Task<IActionResult> GetUpgradeDetail([FromBody] BackofficeGetGroupedOfferDetailQuery query, CancellationToken cancellationToken)
            => DetailAsync(query, AncillaryProfile.Upgrade, cancellationToken);

        [HttpPost("ExtraSeats/Details")]
        public Task<IActionResult> GetExtraSeatDetail([FromBody] BackofficeGetGroupedOfferDetailQuery query, CancellationToken cancellationToken)
            => DetailAsync(query, AncillaryProfile.Seat, cancellationToken);

        private async Task<IActionResult> DetailAsync(BackofficeGetGroupedOfferDetailQuery query, AncillaryProfile family, CancellationToken cancellationToken)
        {
            query.Family = family;

            return Ok(await _mediator.Send(query, cancellationToken));
        }
    }
}
