using AeroTech.Ancillary.Query.AncillaryQuote.Dto;

namespace AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote
{
    public interface IGetAncillaryQuoteService
    {
        Task<AncillaryQuoteDto> ExecuteAsync(IGetAncillaryQuoteQuery query, CancellationToken cancellationToken = default);
    }
}
