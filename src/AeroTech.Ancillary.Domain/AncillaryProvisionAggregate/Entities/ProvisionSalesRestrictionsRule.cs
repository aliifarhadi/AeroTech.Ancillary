using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities
{
    public sealed class ProvisionSalesRestrictionsRule : Entity<long>
    {
        private readonly List<ProvisionPointOfSale> _pointsOfSale = new();
        private readonly List<ProvisionCustomer> _customers = new();
        private readonly List<ProvisionCustomerType> _customerTypes = new();

        private ProvisionSalesRestrictionsRule()
        {
        }

        private ProvisionSalesRestrictionsRule(long id, long ancillaryProvisionId)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
        }

        public long AncillaryProvisionId { get; private set; }

        public DateTimeOffset? SalesEffectiveFrom { get; private set; }

        public DateTimeOffset? SalesDiscontinueAt { get; private set; }

        public IReadOnlyCollection<ProvisionPointOfSale> PointsOfSale => _pointsOfSale.AsReadOnly();

        public IReadOnlyCollection<ProvisionCustomer> Customers => _customers.AsReadOnly();

        public IReadOnlyCollection<ProvisionCustomerType> CustomerTypes => _customerTypes.AsReadOnly();

        internal static Func<ProvisionSalesRestrictionsRule?> Plan(
            ProvisionSalesRestrictionsRule? stored,
            long ancillaryProvisionId,
            ProvisionSalesRestrictionsArgs? args,
            IIdGenerator idGenerator)
        {
            if (args is null || args.IsEmpty)
                return () => null;

            if (args.SalesEffectiveFrom is not null && args.SalesDiscontinueAt is not null && args.SalesEffectiveFrom >= args.SalesDiscontinueAt)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(ProvisionSalesRestrictionsRule)}.{nameof(SalesDiscontinueAt)}");

            var rule = stored ?? new ProvisionSalesRestrictionsRule(idGenerator.NewId(), ancillaryProvisionId);
            var pointsOfSale = AncillaryProvision.MergeRows(
                rule._pointsOfSale,
                args.PointOfSaleIds.Select(value => new ProvisionPointOfSale(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(PointsOfSale));
            var customers = AncillaryProvision.MergeRows(
                rule._customers,
                args.CustomerIds.Select(value => new ProvisionCustomer(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(Customers));
            var customerTypes = AncillaryProvision.MergeRows(
                rule._customerTypes,
                args.CustomerTypes.Select(value => new ProvisionCustomerType(idGenerator.NewId(), ancillaryProvisionId, rule.Id, value)).ToList(),
                (left, right) => left.SameAs(right),
                (left, right) => left.SameAs(right),
                nameof(CustomerTypes));

            return () =>
            {
                rule.SalesEffectiveFrom = args.SalesEffectiveFrom;
                rule.SalesDiscontinueAt = args.SalesDiscontinueAt;
                AncillaryProvision.ReplaceRows(rule._pointsOfSale, pointsOfSale);
                AncillaryProvision.ReplaceRows(rule._customers, customers);
                AncillaryProvision.ReplaceRows(rule._customerTypes, customerTypes);

                return rule;
            };
        }
    }
}
