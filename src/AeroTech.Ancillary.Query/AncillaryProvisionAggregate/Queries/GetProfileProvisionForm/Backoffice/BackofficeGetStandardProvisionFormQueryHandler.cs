using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionForm.Backoffice
{
    public sealed class BackofficeGetStandardProvisionFormQueryHandler
        : IRequestHandler<BackofficeGetStandardProvisionFormQuery, BackofficeProfileProvisionFormDto<StandardProvisionFormDto>>
    {
        private readonly IGetProfileProvisionFormService _service;

        public BackofficeGetStandardProvisionFormQueryHandler(IGetProfileProvisionFormService service) => _service = service;

        public async Task<BackofficeProfileProvisionFormDto<StandardProvisionFormDto>> Handle(
            BackofficeGetStandardProvisionFormQuery query,
            CancellationToken cancellationToken)
            => ProfileProvisionFormMapper.Reshape(
                await _service.ExecuteAsync(query.Profile, null, query.ServiceDefinitionId, query.ProvisionId, cancellationToken),
                form => new StandardProvisionFormDto(
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
                    form.AdvancePurchase));
    }
}
