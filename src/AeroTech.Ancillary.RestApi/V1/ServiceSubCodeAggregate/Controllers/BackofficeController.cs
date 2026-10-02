using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.ReactivateServiceSubCode.Backoffice;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode.Backoffice;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RetireServiceSubCode.Backoffice;
using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Queries.GetServiceSubCodeById.Backoffice;
using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Queries.GetServiceSubCodesPaginated.Backoffice;
using AeroTech.Ancillary.RestApi.V1.ServiceSubCodeAggregate.Requests;
using AeroTech.Ancillary.RestApi.V1._Shared;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AeroTech.Ancillary.RestApi.V1.ServiceSubCodeAggregate
{
    [ApiController]
    [ApiVersion("1.0")]
    [Tags("Backoffice")]
    [Route($"Backoffice/v{{version:apiVersion}}/ServiceSubCodes")]
    [Authorize(SurfaceAuthorization.Backoffice)]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterServiceSubCodeRequest request, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRegisterServiceSubCodeCommand(
                request.OwnerAirlineId,
                request.Code,
                request.Rfic,
                request.GroupCode,
                request.SubGroupCode,
                request.Description1Code,
                request.Description2Code,
                request.CommercialName), cancellationToken));

        [HttpPost("{serviceSubCodeId:long}/Retire")]
        public async Task<IActionResult> Retire(long serviceSubCodeId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeRetireServiceSubCodeCommand(serviceSubCodeId), cancellationToken));

        [HttpPost("{serviceSubCodeId:long}/Reactivate")]
        public async Task<IActionResult> Reactivate(long serviceSubCodeId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeReactivateServiceSubCodeCommand(serviceSubCodeId), cancellationToken));

        [HttpGet("Paginated")]
        public async Task<IActionResult> Paginated(
            [FromQuery] BackofficeGetServiceSubCodesPaginatedQuery query,
            CancellationToken cancellationToken)
            => Ok(await _mediator.Send(query, cancellationToken));

        [HttpGet("{serviceSubCodeId:long}")]
        public async Task<IActionResult> GetById(long serviceSubCodeId, CancellationToken cancellationToken)
            => Ok(await _mediator.Send(new BackofficeGetServiceSubCodeByIdQuery(serviceSubCodeId), cancellationToken));
    }
}
