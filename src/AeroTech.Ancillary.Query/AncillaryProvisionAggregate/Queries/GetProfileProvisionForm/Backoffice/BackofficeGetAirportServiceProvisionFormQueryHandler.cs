using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionForm.Backoffice
{
    public sealed class BackofficeGetAirportServiceProvisionFormQueryHandler
        : IRequestHandler<BackofficeGetAirportServiceProvisionFormQuery, BackofficeProfileProvisionFormDto<AirportServiceProvisionFormDto>>
    {
        private readonly IGetProfileProvisionFormService _service;

        public BackofficeGetAirportServiceProvisionFormQueryHandler(IGetProfileProvisionFormService service) => _service = service;

        public async Task<BackofficeProfileProvisionFormDto<AirportServiceProvisionFormDto>> Handle(
            BackofficeGetAirportServiceProvisionFormQuery query,
            CancellationToken cancellationToken)
            => ProfileProvisionFormMapper.Reshape(
                await _service.ExecuteAsync(AncillaryProfile.AirportService, nameof(ProvisionFormDto.AirportServiceRule), query.ServiceDefinitionId, query.ProvisionId, cancellationToken),
                form => new AirportServiceProvisionFormDto(
                    form.Sequence,
                    form.CoverageScope,
                    form.PurchaseStage,
                    form.PriceOrigin,
                    form.QuoteProviderKey,
                    form.Quantity,
                    form.Outcome,
                    form.Settlement,
                    form.Availability,
                    form.Fulfillment,
                    form.PassengerEligibility,
                    form.SalesRestrictions,
                    form.Geography,
                    form.FlightApplication,
                    form.FareApplication,
                    form.TravelDate,
                    form.DayTimeApplication,
                    form.AdvancePurchase,
                    form.AirportServiceRule));
    }
}
