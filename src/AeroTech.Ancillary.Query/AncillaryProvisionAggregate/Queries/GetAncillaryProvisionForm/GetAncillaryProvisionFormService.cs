using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionForm
{
    public sealed class GetAncillaryProvisionFormService : IGetAncillaryProvisionFormService
    {
        private readonly IGetAncillaryProvisionByIdService _provisions;

        public GetAncillaryProvisionFormService(IGetAncillaryProvisionByIdService provisions) => _provisions = provisions;

        public async Task<BackofficeProvisionFormDto> ExecuteAsync(long provisionId, CancellationToken cancellationToken = default)
            => AncillaryProvisionFormMapper.ToBackofficeForm(await _provisions.ExecuteAsync(provisionId, cancellationToken));
    }
}
