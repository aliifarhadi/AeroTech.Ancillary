using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.ValueObjects;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities
{
    public sealed class AncillaryPriceComponent : Entity<long>
    {
        private const int CodeMaxLength = 10;
        private const int NameMaxLength = 100;

        private AncillaryPriceComponent()
        {
        }

        internal AncillaryPriceComponent(long id, long ancillaryPricingRateId, int rateCurrencyId, AncillaryPriceComponentArgs args)
        {
            if (args.Category == AncillaryPriceLineCategory.Ancillary)
                throw ExceptionFactory.PricingFlatLineNotSupported();

            Require(args.Category is AncillaryPriceLineCategory.Tax or AncillaryPriceLineCategory.Fee, nameof(Category));
            Require(args.Code is { Length: >= 1 and <= CodeMaxLength }, nameof(Code));
            Require(args.Name is null or { Length: >= 1 and <= NameMaxLength }, nameof(Name));
            Require(args.CountryId is null or > 0, nameof(CountryId));
            Require(args.StationAirportId is null or > 0, nameof(StationAirportId));
            Require(args.FeeApplicationUnit is null || Enum.IsDefined(args.FeeApplicationUnit.Value), nameof(FeeApplicationUnit));
            Require(args.Category == AncillaryPriceLineCategory.Fee || args.FeeApplicationUnit is null, nameof(FeeApplicationUnit));
            Require(args.Category == AncillaryPriceLineCategory.Tax || args.TaxIncludedInSource is null, nameof(TaxIncludedInSource));
            Require(args.Category == AncillaryPriceLineCategory.Tax || args.TaxTreatment is null, nameof(TaxTreatment));

            if (args.Category == AncillaryPriceLineCategory.Tax
                && args.TaxTreatment is not (Messages.Ancillary.Enums.TaxTreatment.AddedToBase or Messages.Ancillary.Enums.TaxTreatment.IncludedInBase))
                throw ExceptionFactory.PricingTaxTreatmentRequired(args.Code);

            var amount = Money.Of(args.Amount, args.CurrencyId);

            if (amount.CurrencyId != rateCurrencyId)
                throw ExceptionFactory.PricingCurrencyMismatch(rateCurrencyId, amount.CurrencyId);

            Id = id;
            AncillaryPricingRateId = ancillaryPricingRateId;
            Category = args.Category;
            Code = args.Code;
            Name = args.Name;
            CountryId = args.CountryId;
            StationAirportId = args.StationAirportId;
            Amount = amount;
            FeeApplicationUnit = args.FeeApplicationUnit;
            TaxIncludedInSource = args.TaxIncludedInSource;
            TaxTreatment = args.TaxTreatment;
        }

        public long AncillaryPricingRateId { get; private set; }

        public AncillaryPriceLineCategory Category { get; private set; }

        public string? Code { get; private set; }

        public string? Name { get; private set; }

        public int? CountryId { get; private set; }

        public int? StationAirportId { get; private set; }

        public Money Amount { get; private set; } = null!;

        public FeeApplicationUnit? FeeApplicationUnit { get; private set; }

        public bool? TaxIncludedInSource { get; private set; }

        public TaxTreatment? TaxTreatment { get; private set; }

        internal AncillaryPriceComponent CopyTo(long id, long ancillaryPricingRateId)
            => new()
            {
                Id = id,
                AncillaryPricingRateId = ancillaryPricingRateId,
                Category = Category,
                Code = Code,
                Name = Name,
                CountryId = CountryId,
                StationAirportId = StationAirportId,
                Amount = Amount.Copy(),
                FeeApplicationUnit = FeeApplicationUnit,
                TaxIncludedInSource = TaxIncludedInSource,
                TaxTreatment = TaxTreatment
            };

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.PricingIsInvalid($"{nameof(AncillaryPriceComponent)}.{field}");
        }
    }
}
