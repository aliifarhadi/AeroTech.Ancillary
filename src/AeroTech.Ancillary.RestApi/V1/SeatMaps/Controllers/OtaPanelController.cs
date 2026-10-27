using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetOrderSeatMap.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.OtaPanel;
using AeroTech.Ancillary.RestApi.V1._Shared;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.SeatMaps.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("OtaPanel")]
    [Route($"OtaPanel/v{{version:apiVersion}}/SeatMaps")]
    [Authorize(SurfaceAuthorization.OtaPanel)]
    public sealed class OtaPanelController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OtaPanelController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> GetByOfferId([FromBody] OtaPanelGetSeatMapQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpPost("Orders")]
        public async Task<IActionResult> GetByOrderId([FromBody] OtaPanelGetOrderSeatMapQuery query, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));
    }
}
