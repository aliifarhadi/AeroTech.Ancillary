using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Entities
{
    public sealed class PriceLine : Entity<long>
    {
        private const int CodeMaxLength = 10;
        private const int NameMaxLength = 100;
        private const int AmountScale = 2;

        private const decimal AmountLimit = 10_000_000_000_000_000m;

        private PriceLine()
        {
        }

        internal PriceLine(long id, long ancillaryPriceRuleId, PriceLineArgs args)
        {
            if (!Enum.IsDefined(args.Category))
                throw ExceptionFactory.AncillaryPriceRuleIsInvalid($"{nameof(PriceLine)}.{nameof(Category)}");

            if (args.Code is { Length: > CodeMaxLength } || (args.Category == AncillaryPriceLineCategory.Tax && string.IsNullOrEmpty(args.Code)))
                throw ExceptionFactory.AncillaryPriceRuleIsInvalid($"{nameof(PriceLine)}.{nameof(Code)}");

            if (args.Name is { Length: > NameMaxLength })
                throw ExceptionFactory.AncillaryPriceRuleIsInvalid($"{nameof(PriceLine)}.{nameof(Name)}");

            if (args.Amount <= 0 || args.Amount >= AmountLimit || decimal.Round(args.Amount, AmountScale) != args.Amount)
                throw ExceptionFactory.AncillaryPriceRuleIsInvalid($"{nameof(PriceLine)}.{nameof(Amount)}");

            Id = id;
            AncillaryPriceRuleId = ancillaryPriceRuleId;
            Category = args.Category;
            Code = args.Code;
            Name = args.Name;
            Amount = args.Amount;
        }

        public long AncillaryPriceRuleId { get; private set; }

        public AncillaryPriceLineCategory Category { get; private set; }

        public string? Code { get; private set; }

        public string? Name { get; private set; }

        public decimal Amount { get; private set; }
    }
}
