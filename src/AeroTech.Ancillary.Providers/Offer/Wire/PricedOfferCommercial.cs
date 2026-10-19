using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Providers.Offer.Wire
{
    public sealed record PricedOfferCommercial(Channel? Channel, long? CustomerId, long? CountryId, int CurrencyId);
}
