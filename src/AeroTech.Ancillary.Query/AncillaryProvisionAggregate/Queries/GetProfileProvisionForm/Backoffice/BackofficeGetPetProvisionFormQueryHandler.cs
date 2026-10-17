using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionForm.Backoffice
{
    public sealed class BackofficeGetPetProvisionFormQueryHandler
        : IRequestHandler<BackofficeGetPetProvisionFormQuery, BackofficeProfileProvisionFormDto<PetProvisionFormDto>>
    {
        private readonly IGetProfileProvisionFormService _service;

        public BackofficeGetPetProvisionFormQueryHandler(IGetProfileProvisionFormService service) => _service = service;

        public async Task<BackofficeProfileProvisionFormDto<PetProvisionFormDto>> Handle(
            BackofficeGetPetProvisionFormQuery query,
            CancellationToken cancellationToken)
            => ProfileProvisionFormMapper.Reshape(
                await _service.ExecuteAsync(AncillaryProfile.Pet, nameof(ProvisionFormDto.PetRule), query.ServiceDefinitionId, query.ProvisionId, cancellationToken),
                form => new PetProvisionFormDto(
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
                    form.PetRule));
    }
}
