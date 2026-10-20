using Microsoft.Extensions.Configuration;

namespace AeroTech.Ancillary.Query.Shopping.Common.PricedSelections
{
    public sealed record PricedSelectionOptions(string? SigningKey)
    {
        public const string SigningKeyKey = "Shopping:SelectionSigningKey";

        public static PricedSelectionOptions From(IConfiguration configuration) => new(configuration[SigningKeyKey]);
    }
}
