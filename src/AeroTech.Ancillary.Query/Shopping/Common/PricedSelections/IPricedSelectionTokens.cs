namespace AeroTech.Ancillary.Query.Shopping.Common.PricedSelections
{
    public interface IPricedSelectionTokens
    {
        PricedSelectionToken Issue(PricedSelection selection, DateTimeOffset expiresAtUtc);

        Task<PricedSelectionReading> ReadAsync(string? token);
    }
}
