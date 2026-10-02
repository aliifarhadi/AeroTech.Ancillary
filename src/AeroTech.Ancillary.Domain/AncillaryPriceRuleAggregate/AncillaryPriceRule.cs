using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.ValueObjects;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate
{
    public sealed class AncillaryPriceRule : AggregateRoot<long>
    {
        private readonly List<PriceLine> _lines = new();

        private AncillaryPriceRule()
        {
        }

        private AncillaryPriceRule(long id, int ownerAirlineId, string productRef, DateTimeOffset createdAt)
        {
            Id = id;
            OwnerAirlineId = ownerAirlineId;
            ProductRef = productRef;
            Status = AncillaryPriceRuleStatus.Draft;
            CreatedAt = createdAt;
        }

        public int OwnerAirlineId { get; private set; }

        public string ProductRef { get; private set; } = default!;

        public int Priority { get; private set; }

        public int CurrencyId { get; private set; }

        public IReadOnlyCollection<PriceLine> Lines => _lines.AsReadOnly();

        public DateTimeOffset? SalesFrom { get; private set; }

        public DateTimeOffset? SalesTo { get; private set; }

        public DateOnly? TravelFrom { get; private set; }

        public DateOnly? TravelTo { get; private set; }

        public PriceRuleConditions Conditions { get; private set; } = default!;

        public AncillaryPriceRuleStatus Status { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public static AncillaryPriceRule Define(
            long id,
            int ownerAirlineId,
            string productRef,
            int priority,
            int currencyId,
            IReadOnlyList<PriceLineArgs> lines,
            DateTimeOffset? salesFrom,
            DateTimeOffset? salesTo,
            DateOnly? travelFrom,
            DateOnly? travelTo,
            PriceRuleConditions conditions,
            IIdGenerator idGenerator,
            DateTimeOffset createdAt)
        {
            Require(ownerAirlineId > 0, nameof(OwnerAirlineId));

            var rule = new AncillaryPriceRule(id, ownerAirlineId, productRef, createdAt);

            rule.Apply(priority, currencyId, lines, salesFrom, salesTo, travelFrom, travelTo, conditions, idGenerator);

            return rule;
        }

        public void Change(
            int priority,
            int currencyId,
            IReadOnlyList<PriceLineArgs> lines,
            DateTimeOffset? salesFrom,
            DateTimeOffset? salesTo,
            DateOnly? travelFrom,
            DateOnly? travelTo,
            PriceRuleConditions conditions,
            IIdGenerator idGenerator)
        {
            if (Status != AncillaryPriceRuleStatus.Draft)
                throw ExceptionFactory.AncillaryPriceRuleIsNotDraft();

            Apply(priority, currencyId, lines, salesFrom, salesTo, travelFrom, travelTo, conditions, idGenerator);
        }

        public void Activate()
        {
            if (Status is not (AncillaryPriceRuleStatus.Draft or AncillaryPriceRuleStatus.Suspended))
                throw ExceptionFactory.AncillaryPriceRuleStatusChangeNotAllowed();

            Status = AncillaryPriceRuleStatus.Active;
        }

        public void Suspend()
        {
            if (Status != AncillaryPriceRuleStatus.Active)
                throw ExceptionFactory.AncillaryPriceRuleStatusChangeNotAllowed();

            Status = AncillaryPriceRuleStatus.Suspended;
        }

        public void Retire()
        {
            if (Status == AncillaryPriceRuleStatus.Retired)
                throw ExceptionFactory.AncillaryPriceRuleStatusChangeNotAllowed();

            Status = AncillaryPriceRuleStatus.Retired;
        }

        private void Apply(
            int priority,
            int currencyId,
            IReadOnlyList<PriceLineArgs> lines,
            DateTimeOffset? salesFrom,
            DateTimeOffset? salesTo,
            DateOnly? travelFrom,
            DateOnly? travelTo,
            PriceRuleConditions conditions,
            IIdGenerator idGenerator)
        {
            Require(priority >= 1, nameof(Priority));
            Require(currencyId > 0, nameof(CurrencyId));
            Require(lines.Count(line => line.Category == AncillaryPriceLineCategory.Ancillary) == 1, nameof(Lines));
            Require(
                lines.Where(line => line.Category == AncillaryPriceLineCategory.Tax)
                    .GroupBy(line => line.Code, StringComparer.Ordinal)
                    .All(group => group.Count() == 1),
                nameof(Lines));
            Require(salesFrom is null || salesTo is null || salesFrom < salesTo, nameof(SalesTo));
            Require(travelFrom is null || travelTo is null || travelFrom <= travelTo, nameof(TravelTo));

            var priceLines = lines.Select(line => new PriceLine(idGenerator.NewId(), Id, line)).ToList();

            Priority = priority;
            CurrencyId = currencyId;
            SalesFrom = salesFrom;
            SalesTo = salesTo;
            TravelFrom = travelFrom;
            TravelTo = travelTo;
            Conditions = conditions;

            _lines.Clear();
            _lines.AddRange(priceLines);
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.AncillaryPriceRuleIsInvalid(field);
        }
    }
}
