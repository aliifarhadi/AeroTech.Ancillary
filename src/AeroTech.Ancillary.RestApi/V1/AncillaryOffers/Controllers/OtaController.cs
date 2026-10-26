using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedAncillaryOffers.Api;
using AeroTech.Ancillary.RestApi.V1.AncillaryOffers.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
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

        [HttpPost("Price")]
        public async Task<IActionResult> Price([FromBody] PriceAncillaryOffersRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ApiPriceAncillaryOffersCommand(request.OfferId, request.Selections), cancellationToken));

        [HttpGet("Price/{selectionId}")]
        public async Task<IActionResult> GetBySelectionId(string selectionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new ApiGetPricedAncillaryOffersQuery(selectionId), cancellationToken));
    }
}
