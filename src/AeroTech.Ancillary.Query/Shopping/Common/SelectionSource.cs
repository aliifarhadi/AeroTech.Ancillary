using System.Globalization;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.Common
{
    public static class SelectionSource
    {
        public static string ReferenceOf(SelectionSnapshot snapshot)
            => snapshot.SourceKind == ShoppingSourceKind.Offer
                ? snapshot.OfferId!
                : snapshot.OrderId!.Value.ToString(CultureInfo.InvariantCulture);
    }
}
