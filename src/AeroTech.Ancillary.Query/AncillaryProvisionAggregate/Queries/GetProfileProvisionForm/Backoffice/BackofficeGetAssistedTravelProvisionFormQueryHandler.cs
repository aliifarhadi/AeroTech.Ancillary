using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionForm.Backoffice
{
    public sealed class BackofficeGetAssistedTravelProvisionFormQueryHandler
        : IRequestHandler<BackofficeGetAssistedTravelProvisionFormQuery, BackofficeProfileProvisionFormDto<AssistedTravelProvisionFormDto>>
    {
        private readonly IGetProfileProvisionFormService _service;

        public BackofficeGetAssistedTravelProvisionFormQueryHandler(IGetProfileProvisionFormService service) => _service = service;

        public async Task<BackofficeProfileProvisionFormDto<AssistedTravelProvisionFormDto>> Handle(
            BackofficeGetAssistedTravelProvisionFormQuery query,
            CancellationToken cancellationToken)
            => ProfileProvisionFormMapper.Reshape(
                await _service.ExecuteAsync(AncillaryProfile.AssistedTravel, nameof(ProvisionFormDto.AssistedTravelRule), query.ServiceDefinitionId, query.ProvisionId, cancellationToken),
                form => new AssistedTravelProvisionFormDto(
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
                    form.AssistedTravelRule));
    }
}
