using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetOrderAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedOrderAncillaryOffers.Backoffice;
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
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/AncillaryOffers")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> GetByOfferId([FromBody] BackofficeGetAncillaryOffersQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Orders")]
        public async Task<IActionResult> GetByOrderId([FromBody] BackofficeGetOrderAncillaryOffersQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Price")]
        public async Task<IActionResult> Price([FromBody] PriceAncillaryOffersRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficePriceAncillaryOffersCommand(request.OfferId, request.Selections), cancellationToken));

        [HttpPost("Orders/Price")]
        public async Task<IActionResult> PriceOrder([FromBody] PriceOrderAncillaryOffersRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficePriceOrderAncillaryOffersCommand(request.OrderId, request.Selections), cancellationToken));

        [HttpGet("Price/{selectionId}")]
        public async Task<IActionResult> GetBySelectionId(string selectionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetPricedAncillaryOffersQuery(selectionId), cancellationToken));

        [HttpGet("Orders/Price/{selectionId}")]
        public async Task<IActionResult> GetByOrderSelectionId(string selectionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetPricedOrderAncillaryOffersQuery(selectionId), cancellationToken));
    }
}
