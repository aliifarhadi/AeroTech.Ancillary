using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Context;

namespace AeroTech.Ancillary.Query.Shopping.Common
{
    public static class OfferFamilyFilters
    {
        private static readonly string[] SeatMapVariants = [AncillaryVariant.StandardSeat, AncillaryVariant.PreferredSeat];

        public static readonly ShoppingFilter SeatMap = new() { VariantCodes = SeatMapVariants };

        public static readonly ShoppingFilter Services = new()
        {
            VariantCodes = AncillaryVariant.All.Select(variant => variant.Code).Where(code => !SeatMapVariants.Contains(code, StringComparer.Ordinal)).ToList()
        };
    }
}
