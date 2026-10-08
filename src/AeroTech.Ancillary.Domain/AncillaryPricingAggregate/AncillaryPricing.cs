using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;
using FeeUnit = AeroTech.Messages.Ancillary.Enums.FeeApplicationUnit;
using Unit = AeroTech.Messages.Ancillary.Enums.PricingUnit;

namespace AeroTech.Ancillary.Domain.AncillaryPricingAggregate
{
    public sealed class AncillaryPricing : AggregateRoot<long>
    {
        private static readonly FeeUnit[] ImplementedFeeApplicationUnits =
        [
            FeeUnit.OneWay,
            FeeUnit.RoundTrip,
            FeeUnit.Item,
            FeeUnit.SectorOrPortion,
            FeeUnit.Ticket
        ];

        private readonly List<AncillaryPricingLine> _priceLines = new();

        private AncillaryPricing()
        {
        }

        private AncillaryPricing(long id, long ancillaryProvisionId, PricingUnit? pricingUnit, int version)
        {
            Id = id;
            AncillaryProvisionId = ancillaryProvisionId;
            PricingUnit = pricingUnit;
            Version = version;
        }

        public long AncillaryProvisionId { get; private set; }

        public PricingUnit? PricingUnit { get; private set; }

        public int Version { get; private set; }

        public int CurrencyId { get; private set; }

        public FeeApplicationUnit? FeeApplicationUnit { get; private set; }

        public PricingStatus Status { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? ActivatedAt { get; private set; }

        public DateTimeOffset? SuspendedAt { get; private set; }

        public DateTimeOffset? RetiredAt { get; private set; }

        public IReadOnlyCollection<AncillaryPricingLine> PriceLines => _priceLines.AsReadOnly();

        public static AncillaryPricing Define(
            long id,
            long ancillaryProvisionId,
            PricingUnit pricingUnit,
            int version,
            int currencyId,
            FeeApplicationUnit? feeApplicationUnit,
            IReadOnlyList<AncillaryPricingLineArgs> priceLines,
            IIdGenerator idGenerator,
            DateTimeOffset createdAt)
        {
            Require(ancillaryProvisionId > 0, nameof(AncillaryProvisionId));
            Require(Enum.IsDefined(pricingUnit), nameof(PricingUnit));
            Require(version >= 1, nameof(Version));

            var pricing = new AncillaryPricing(id, ancillaryProvisionId, pricingUnit, version);

            pricing.Status = PricingStatus.Draft;
            pricing.CreatedAt = createdAt;
            pricing.Apply(currencyId, feeApplicationUnit, priceLines, idGenerator);

            return pricing;
        }

        public void Change(
            int currencyId,
            FeeApplicationUnit? feeApplicationUnit,
            IReadOnlyList<AncillaryPricingLineArgs> priceLines,
            IIdGenerator idGenerator)
        {
            if (Status != PricingStatus.Draft)
                throw ExceptionFactory.PricingStatusChangeNotAllowed();

            Apply(currencyId, feeApplicationUnit, priceLines, idGenerator);
        }

        public void Activate(DateTimeOffset now)
        {
            if (Status != PricingStatus.Draft)
                throw ExceptionFactory.PricingStatusChangeNotAllowed();

            EnsurePublishable();

            Status = PricingStatus.Active;
            ActivatedAt = now;
        }

        public void Supersede(DateTimeOffset now)
        {
            if (Status != PricingStatus.Active)
                throw ExceptionFactory.PricingStatusChangeNotAllowed();

            Status = PricingStatus.Retired;
            RetiredAt = now;
        }

        public void Suspend(DateTimeOffset now)
        {
            if (Status != PricingStatus.Active)
                throw ExceptionFactory.PricingStatusChangeNotAllowed();

            Status = PricingStatus.Suspended;
            SuspendedAt = now;
        }

        public void Reactivate()
        {
            if (Status != PricingStatus.Suspended)
                throw ExceptionFactory.PricingStatusChangeNotAllowed();

            EnsurePublishable();

            Status = PricingStatus.Active;
            SuspendedAt = null;
        }

        public void Retire(DateTimeOffset now)
        {
            if (Status == PricingStatus.Retired)
                throw ExceptionFactory.PricingStatusChangeNotAllowed();

            Status = PricingStatus.Retired;
            RetiredAt = now;
        }

        public AncillaryPricing Revise(long id, int version, IIdGenerator idGenerator, DateTimeOffset createdAt)
        {
            if (Status == PricingStatus.Draft)
                throw ExceptionFactory.PricingStatusChangeNotAllowed();

            if (PricingUnit is null)
                throw ExceptionFactory.ServiceDefinitionPricingUnitNotAssigned();

            Require(version > Version, nameof(Version));

            var revision = new AncillaryPricing(id, AncillaryProvisionId, PricingUnit, version);

            revision.Status = PricingStatus.Draft;
            revision.CreatedAt = createdAt;
            revision.Apply(CurrencyId, FeeApplicationUnit, _priceLines.Select(line => line.ToArgs()).ToList(), idGenerator);

            return revision;
        }

        public void AssignPricingUnit(PricingUnit pricingUnit)
        {
            if (PricingUnit is not null)
                throw ExceptionFactory.ServiceDefinitionPricingUnitAlreadyAssigned();

            Require(Enum.IsDefined(pricingUnit), nameof(PricingUnit));
            EnsureCoherent(pricingUnit, _priceLines);

            PricingUnit = pricingUnit;
        }

        private void Apply(
            int currencyId,
            FeeApplicationUnit? feeApplicationUnit,
            IReadOnlyList<AncillaryPricingLineArgs> priceLines,
            IIdGenerator idGenerator)
        {
            Require(currencyId > 0, nameof(CurrencyId));
            Require(feeApplicationUnit is null || Enum.IsDefined(feeApplicationUnit.Value), nameof(FeeApplicationUnit));

            var lines = priceLines.Select(line => new AncillaryPricingLine(idGenerator.NewId(), Id, line)).ToList();

            EnsureCoherent(PricingUnit, lines);

            CurrencyId = currencyId;
            FeeApplicationUnit = feeApplicationUnit;

            _priceLines.Clear();
            _priceLines.AddRange(lines);
        }

        private void EnsurePublishable()
        {
            if (PricingUnit is null)
                throw ExceptionFactory.ServiceDefinitionPricingUnitNotAssigned();

            if (FeeApplicationUnit is { } feeApplicationUnit && !ImplementedFeeApplicationUnits.Contains(feeApplicationUnit))
                throw ExceptionFactory.FeeApplicationUnitNotSupported(feeApplicationUnit);

            EnsureCoherent(PricingUnit, _priceLines);
        }

        private static void EnsureCoherent(PricingUnit? pricingUnit, IReadOnlyList<AncillaryPricingLine> lines)
        {
            var selectors = lines
                .GroupBy(line => (line.PassengerTypeCode, line.AgeFromInclusive, line.AgeToExclusive))
                .ToList();

            RequireUnambiguous(
                selectors.Count > 0
                && selectors.All(selector => selector.Count(line => line.Category == AncillaryPriceLineCategory.Ancillary) == 1),
                nameof(AncillaryPriceLineCategory.Ancillary));
            RequireUnambiguous(
                lines
                    .Where(line => line.Category != AncillaryPriceLineCategory.Ancillary)
                    .GroupBy(line => (
                        line.PassengerTypeCode,
                        line.AgeFromInclusive,
                        line.AgeToExclusive,
                        line.Category,
                        line.Code,
                        line.CountryId,
                        line.StationAirportId))
                    .All(component => component.Count() == 1),
                nameof(AncillaryPricingLine.Code));
            RequireUnambiguous(
                pricingUnit == Unit.PerPassenger
                || selectors.All(selector => selector.Key.PassengerTypeCode is null && selector.Key.AgeFromInclusive is null),
                nameof(PricingUnit));
            RequireUnambiguous(
                selectors.All(selector => selector.Key.PassengerTypeCode is null)
                || selectors.All(selector => selector.Key.PassengerTypeCode is not null),
                nameof(AncillaryPricingLine.PassengerTypeCode));

            foreach (var passengerType in selectors.GroupBy(selector => selector.Key.PassengerTypeCode))
            {
                var bands = passengerType
                    .Select(selector => (From: selector.Key.AgeFromInclusive, To: selector.Key.AgeToExclusive))
                    .OrderBy(band => band.From)
                    .ToList();

                RequireUnambiguous(bands.Count == 1 || bands.All(band => band.From is not null), nameof(AncillaryPricingLine.AgeFromInclusive));

                for (var index = 1; index < bands.Count; index++)
                    RequireUnambiguous(
                        bands[index - 1].To is not null && bands[index - 1].To <= bands[index].From,
                        nameof(AncillaryPricingLine.AgeToExclusive));
            }
        }

        private static void RequireUnambiguous(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.PricingSelectorConflict(field);
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.PricingIsInvalid(field);
        }
    }
}
