using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRuleById
{
    public interface IGetAncillaryPriceRuleByIdService
    {
        Task<BackofficeAncillaryPriceRuleDto> ExecuteAsync(long ancillaryPriceRuleId, CancellationToken cancellationToken = default);
    }
}
