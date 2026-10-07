using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects
{
    public sealed class FeeDefinition : ValueObject
    {
        private FeeDefinition()
        {
        }

        private FeeDefinition(int currencyId, FeeApplicationUnit applicationUnit)
        {
            CurrencyId = currencyId;
            ApplicationUnit = applicationUnit;
        }

        public int CurrencyId { get; private set; }

        public FeeApplicationUnit ApplicationUnit { get; private set; }

        public static FeeDefinition Create(int currencyId, FeeApplicationUnit applicationUnit)
        {
            Require(currencyId > 0, nameof(CurrencyId));
            Require(Enum.IsDefined(applicationUnit), nameof(ApplicationUnit));

            return new FeeDefinition(currencyId, applicationUnit);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return CurrencyId;
            yield return ApplicationUnit;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(FeeDefinition)}.{field}");
        }
    }
}
