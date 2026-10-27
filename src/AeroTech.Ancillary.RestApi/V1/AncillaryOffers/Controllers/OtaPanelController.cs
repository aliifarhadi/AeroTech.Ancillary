using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetOrderAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetPricedOrderAncillaryOffers.OtaPanel;
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

        [HttpPost("Orders")]
        public async Task<IActionResult> GetByOrderId([FromBody] OtaPanelGetOrderAncillaryOffersQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Price")]
        public async Task<IActionResult> Price([FromBody] PriceAncillaryOffersRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new OtaPanelPriceAncillaryOffersCommand(request.OfferId, request.Selections), cancellationToken));

        [HttpPost("Orders/Price")]
        public async Task<IActionResult> PriceOrder([FromBody] PriceOrderAncillaryOffersRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new OtaPanelPriceOrderAncillaryOffersCommand(request.OrderId, request.Selections), cancellationToken));

        [HttpGet("Price/{selectionId}")]
        public async Task<IActionResult> GetBySelectionId(string selectionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new OtaPanelGetPricedAncillaryOffersQuery(selectionId), cancellationToken));

        [HttpGet("Orders/Price/{selectionId}")]
        public async Task<IActionResult> GetByOrderSelectionId(string selectionId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new OtaPanelGetPricedOrderAncillaryOffersQuery(selectionId), cancellationToken));
    }
}
