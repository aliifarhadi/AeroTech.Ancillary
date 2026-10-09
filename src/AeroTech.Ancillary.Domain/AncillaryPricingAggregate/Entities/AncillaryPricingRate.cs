using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.ValueObjects;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities
{
    public sealed class AncillaryPricingRate : Entity<long>
    {
        private readonly List<AncillaryPriceComponent> _components = new();

        private AncillaryPricingRate()
        {
        }

        internal AncillaryPricingRate(long id, long ancillaryPricingId, AncillaryPricingRateArgs args, IIdGenerator idGenerator)
        {
            Require(args.PassengerTypeCode is null || Enum.IsDefined(args.PassengerTypeCode.Value), nameof(PassengerTypeCode));
            Require(args.AgeFromInclusive is null or >= 0, nameof(AgeFromInclusive));
            Require(
                args.AgeToExclusive is null || (args.AgeFromInclusive is not null && args.AgeToExclusive > args.AgeFromInclusive),
                nameof(AgeToExclusive));

            var basePrice = Money.Of(args.BaseAmount, args.CurrencyId);

            Require(basePrice.Amount > 0m, nameof(BaseAmount));

            Id = id;
            AncillaryPricingId = ancillaryPricingId;
            PassengerTypeCode = args.PassengerTypeCode;
            AgeFromInclusive = args.AgeFromInclusive;
            AgeToExclusive = args.AgeToExclusive;
            CurrencyId = basePrice.CurrencyId;
            BaseAmount = basePrice.Amount;

            _components.AddRange((args.Components ?? []).Select(component => new AncillaryPriceComponent(idGenerator.NewId(), id, CurrencyId, component)));

            if (_components
                .GroupBy(component => (component.Category, component.Code, component.CountryId, component.StationAirportId, component.FeeApplicationUnit))
                .Any(identity => identity.Count() > 1))
                throw ExceptionFactory.PricingSelectorConflict(nameof(AncillaryPriceComponent.Code));

            if (IncludedTaxes.Amount > BaseAmount)
                throw ExceptionFactory.PricingIncludedTaxExceedsBase();
        }

        public long AncillaryPricingId { get; private set; }

        public PassengerTypeCode? PassengerTypeCode { get; private set; }

        public int? AgeFromInclusive { get; private set; }

        public int? AgeToExclusive { get; private set; }

        public int CurrencyId { get; private set; }

        public decimal BaseAmount { get; private set; }

        public IReadOnlyCollection<AncillaryPriceComponent> Components => _components.AsReadOnly();

        public Money BasePrice => Money.Of(BaseAmount, CurrencyId);

        public Money UnitTotal => _components
            .Where(component => !IsUnapplied(component) && component.TaxTreatment != TaxTreatment.IncludedInBase)
            .Aggregate(BasePrice, (total, component) => total.Add(component.Amount));

        public Money IncludedTaxes => _components
            .Where(component => component.TaxTreatment == TaxTreatment.IncludedInBase)
            .Aggregate(Money.Of(0m, CurrencyId), (total, component) => total.Add(component.Amount));

        public IReadOnlyList<AncillaryPriceComponent> UnappliedFees => _components.Where(IsUnapplied).ToList();

        public bool IsUnitTotalComplete => !_components.Any(component => IsUnapplied(component) || HasUnknownTreatment(component));

        internal AncillaryPricingRate CopyTo(long id, long ancillaryPricingId, IIdGenerator idGenerator)
        {
            var copy = new AncillaryPricingRate
            {
                Id = id,
                AncillaryPricingId = ancillaryPricingId,
                PassengerTypeCode = PassengerTypeCode,
                AgeFromInclusive = AgeFromInclusive,
                AgeToExclusive = AgeToExclusive,
                CurrencyId = CurrencyId,
                BaseAmount = BaseAmount
            };

            copy._components.AddRange(_components.Select(component => component.CopyTo(idGenerator.NewId(), id)));

            return copy;
        }

        internal void EnsureScale(IReadOnlyDictionary<int, int> currencyDecimalPlaces)
        {
            BasePrice.EnsureScale(currencyDecimalPlaces);

            foreach (var component in _components)
                component.Amount.EnsureScale(currencyDecimalPlaces);
        }

        internal static bool HasUnknownTreatment(AncillaryPriceComponent component)
            => component.Category == AncillaryPriceLineCategory.Tax
               && component.TaxTreatment is not (TaxTreatment.AddedToBase or TaxTreatment.IncludedInBase);

        private static bool IsUnapplied(AncillaryPriceComponent component)
            => component.Category == AncillaryPriceLineCategory.Fee && component.FeeApplicationUnit != FeeApplicationUnit.Item;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.PricingIsInvalid($"{nameof(AncillaryPricingRate)}.{field}");
        }
    }
}
