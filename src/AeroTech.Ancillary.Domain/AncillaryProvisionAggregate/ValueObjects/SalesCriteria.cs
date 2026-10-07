using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects
{
    public sealed class SalesCriteria : ValueObject
    {
        private List<long> _pointOfSaleIds = new();
        private List<long> _customerIds = new();
        private List<CustomerType> _customerTypes = new();

        private SalesCriteria()
        {
        }

        private SalesCriteria(
            IReadOnlyList<long> pointOfSaleIds,
            IReadOnlyList<long> customerIds,
            IReadOnlyList<CustomerType> customerTypes)
        {
            _pointOfSaleIds = pointOfSaleIds.ToList();
            _customerIds = customerIds.ToList();
            _customerTypes = customerTypes.ToList();
        }

        public IReadOnlyList<long> PointOfSaleIds => _pointOfSaleIds.AsReadOnly();

        public IReadOnlyList<long> CustomerIds => _customerIds.AsReadOnly();

        public IReadOnlyList<CustomerType> CustomerTypes => _customerTypes.AsReadOnly();

        public static SalesCriteria Create(
            IReadOnlyList<long>? pointOfSaleIds,
            IReadOnlyList<long>? customerIds,
            IReadOnlyList<CustomerType>? customerTypes)
        {
            var pointsOfSale = pointOfSaleIds ?? [];
            var customers = customerIds ?? [];
            var types = customerTypes ?? [];

            Require(AreIds(pointsOfSale), nameof(PointOfSaleIds));
            Require(AreIds(customers), nameof(CustomerIds));
            Require(types.All(type => Enum.IsDefined(type)) && types.Distinct().Count() == types.Count, nameof(CustomerTypes));

            return new SalesCriteria(pointsOfSale, customers, types);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return string.Join(',', _pointOfSaleIds);
            yield return string.Join(',', _customerIds);
            yield return string.Join(',', _customerTypes);
        }

        private static bool AreIds(IReadOnlyList<long> ids)
            => ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(SalesCriteria)}.{field}");
        }
    }
}
