using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.Common.PricedSelections
{
    public sealed record OrderBinding(long OrderId, int OrderCommercialVersion, string SourceVersion, DateTimeOffset ValidUntilUtc)
    {
        public static OrderBinding? Of(SourceIdentityContext? identity)
            => identity is
            {
                SourceKind: ShoppingSourceKind.Order,
                OrderId: { } orderId,
                OrderCommercialVersion: { } version,
                TrustedSourceVersion: { } sourceVersion,
                ValidUntilUtc: { } validUntilUtc
            }
                ? new OrderBinding(orderId, version, sourceVersion, validUntilUtc)
                : null;
    }
}
