using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionForm.Backoffice
{
    public sealed class BackofficeGetBaggageProvisionFormQueryHandler
        : IRequestHandler<BackofficeGetBaggageProvisionFormQuery, BackofficeProfileProvisionFormDto<BaggageProvisionFormDto>>
    {
        private readonly IGetProfileProvisionFormService _service;

        public BackofficeGetBaggageProvisionFormQueryHandler(IGetProfileProvisionFormService service) => _service = service;

        public async Task<BackofficeProfileProvisionFormDto<BaggageProvisionFormDto>> Handle(
            BackofficeGetBaggageProvisionFormQuery query,
            CancellationToken cancellationToken)
            => ProfileProvisionFormMapper.Reshape(
                await _service.ExecuteAsync(AncillaryProfile.Baggage, nameof(ProvisionFormDto.BaggageApplication), query.ServiceDefinitionId, query.ProvisionId, cancellationToken),
                form => new BaggageProvisionFormDto(
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
                    form.BaggageApplication));
    }
}
