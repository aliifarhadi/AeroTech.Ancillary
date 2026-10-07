using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Projection
{
    internal static class AncillaryProvisionReadModelSnapshotFactory
    {
        public static AncillaryProvisionReadModelSnapshot ToReadModelSnapshot(this AncillaryProvision provision)
            => new(
                provision.Id,
                provision.ServiceDefinitionId,
                provision.Sequence,
                provision.Status,
                provision.SalesEffectiveFrom,
                provision.SalesDiscontinueAt,
                provision.CoverageScope,
                provision.Quantity.Unit,
                provision.Quantity.MinQuantity,
                provision.Quantity.MaxQuantity,
                provision.ApplicationType,
                provision.Outcome.Disposition,
                provision.Outcome.DocumentRequired,
                provision.Outcome.BookingRequired,
                provision.Fee?.CurrencyId,
                provision.Fee?.ApplicationUnit,
                provision.Settlement.ReissueRefund,
                provision.Settlement.FormOfRefund,
                provision.Settlement.Commissionable,
                provision.Settlement.InterlineSettlement,
                provision.Availability.MustCheckAvailability,
                provision.Fulfillment.FulfillmentProviderKey,
                provision.CreatedAt,
                provision.PriceLines
                    .OrderBy(line => line.Id)
                    .Select(line => new ProvisionPriceLineReadModelSnapshot(
                        line.Id,
                        line.Category,
                        line.Code,
                        line.Name,
                        line.UnitAmount))
                    .ToList());
    }
}
