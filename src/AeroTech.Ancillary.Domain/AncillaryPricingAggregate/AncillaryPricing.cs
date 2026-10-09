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

        private readonly List<AncillaryPricingRate> _rates = new();

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

        public PricingStatus Status { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? ActivatedAt { get; private set; }

        public DateTimeOffset? SuspendedAt { get; private set; }

        public DateTimeOffset? RetiredAt { get; private set; }

        public IReadOnlyCollection<AncillaryPricingRate> Rates => _rates.AsReadOnly();

        public static AncillaryPricing Define(
            long id,
            long ancillaryProvisionId,
            PricingUnit pricingUnit,
            int version,
            IReadOnlyList<AncillaryPricingRateArgs> rates,
            IReadOnlyDictionary<int, int> currencyDecimalPlaces,
            IIdGenerator idGenerator,
            DateTimeOffset createdAt)
        {
            Require(ancillaryProvisionId > 0, nameof(AncillaryProvisionId));
            Require(Enum.IsDefined(pricingUnit), nameof(PricingUnit));
            Require(version >= 1, nameof(Version));

            var pricing = new AncillaryPricing(id, ancillaryProvisionId, pricingUnit, version);

            pricing.Status = PricingStatus.Draft;
            pricing.CreatedAt = createdAt;
            pricing.Apply(rates, idGenerator);
            pricing.EnsureScale(currencyDecimalPlaces);

            return pricing;
        }

        public void Change(
            IReadOnlyList<AncillaryPricingRateArgs> rates,
            IReadOnlyDictionary<int, int> currencyDecimalPlaces,
            IIdGenerator idGenerator)
        {
            if (Status != PricingStatus.Draft)
                throw ExceptionFactory.PricingStatusChangeNotAllowed();

            Apply(rates, idGenerator);
            EnsureScale(currencyDecimalPlaces);
        }

        public void Activate(IReadOnlyDictionary<int, int> currencyDecimalPlaces, DateTimeOffset now)
        {
            if (Status != PricingStatus.Draft)
                throw ExceptionFactory.PricingStatusChangeNotAllowed();

            EnsurePublishable(currencyDecimalPlaces);

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

        public void Reactivate(IReadOnlyDictionary<int, int> currencyDecimalPlaces)
        {
            if (Status != PricingStatus.Suspended)
                throw ExceptionFactory.PricingStatusChangeNotAllowed();

            EnsurePublishable(currencyDecimalPlaces);

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
            revision._rates.AddRange(_rates.Select(rate => rate.CopyTo(idGenerator.NewId(), id, idGenerator)));

            return revision;
        }

        public void AssignPricingUnit(PricingUnit pricingUnit)
        {
            if (PricingUnit is not null)
                throw ExceptionFactory.ServiceDefinitionPricingUnitAlreadyAssigned();

            Require(Enum.IsDefined(pricingUnit), nameof(PricingUnit));
            EnsureCoherent(pricingUnit, _rates);

            PricingUnit = pricingUnit;
        }

        private void Apply(IReadOnlyList<AncillaryPricingRateArgs> rates, IIdGenerator idGenerator)
        {
            var applied = (rates ?? []).Select(rate => new AncillaryPricingRate(idGenerator.NewId(), Id, rate, idGenerator)).ToList();

            EnsureCoherent(PricingUnit, applied);

            _rates.Clear();
            _rates.AddRange(applied);
        }

        private void EnsureScale(IReadOnlyDictionary<int, int> currencyDecimalPlaces)
        {
            foreach (var rate in _rates)
                rate.EnsureScale(currencyDecimalPlaces);
        }

        private void EnsurePublishable(IReadOnlyDictionary<int, int> currencyDecimalPlaces)
        {
            if (PricingUnit is null)
                throw ExceptionFactory.ServiceDefinitionPricingUnitNotAssigned();

            EnsureCoherent(PricingUnit, _rates);

            if (_rates.SelectMany(rate => rate.Components).Any(component => component.Code is null))
                throw ExceptionFactory.PricingIsInvalid($"{nameof(AncillaryPriceComponent)}.{nameof(AncillaryPriceComponent.Code)}");

            foreach (var fee in _rates.SelectMany(rate => rate.Components).Where(component => component.Category == AncillaryPriceLineCategory.Fee))
            {
                if (fee.FeeApplicationUnit is not { } feeApplicationUnit)
                    throw ExceptionFactory.PricingFeeApplicationUnitRequired(fee.Code);

                if (!ImplementedFeeApplicationUnits.Contains(feeApplicationUnit))
                    throw ExceptionFactory.FeeApplicationUnitNotSupported(feeApplicationUnit);
            }

            EnsureScale(currencyDecimalPlaces);
        }

        private static void EnsureCoherent(PricingUnit? pricingUnit, IReadOnlyList<AncillaryPricingRate> rates)
        {
            RequireUnambiguous(rates.Count > 0, nameof(Rates));
            RequireUnambiguous(
                rates
                    .GroupBy(rate => (rate.CurrencyId, rate.PassengerTypeCode, rate.AgeFromInclusive, rate.AgeToExclusive))
                    .All(key => key.Count() == 1),
                nameof(AncillaryPricingRate.CurrencyId));
            RequireUnambiguous(
                pricingUnit == Unit.PerPassenger
                || rates.All(rate => rate.PassengerTypeCode is null && rate.AgeFromInclusive is null),
                nameof(PricingUnit));

            foreach (var currency in rates.GroupBy(rate => rate.CurrencyId))
            {
                RequireUnambiguous(
                    currency.All(rate => rate.PassengerTypeCode is null) || currency.All(rate => rate.PassengerTypeCode is not null),
                    nameof(AncillaryPricingRate.PassengerTypeCode));

                foreach (var passengerType in currency.GroupBy(rate => rate.PassengerTypeCode))
                {
                    var bands = passengerType
                        .Select(rate => (From: rate.AgeFromInclusive, To: rate.AgeToExclusive))
                        .OrderBy(band => band.From)
                        .ToList();

                    RequireUnambiguous(bands.Count == 1 || bands.All(band => band.From is not null), nameof(AncillaryPricingRate.AgeFromInclusive));

                    for (var index = 1; index < bands.Count; index++)
                        RequireUnambiguous(
                            bands[index - 1].To is not null && bands[index - 1].To <= bands[index].From,
                            nameof(AncillaryPricingRate.AgeToExclusive));
                }
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
