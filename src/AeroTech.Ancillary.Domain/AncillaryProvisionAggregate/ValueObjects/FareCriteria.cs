using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.AirPrice.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects
{
    public sealed class FareCriteria : ValueObject
    {
        private const int FareBasisMaxLength = 64;

        private List<long> _airFareIds = new();
        private List<AirFareType> _airFareTypes = new();
        private List<long> _fareFamilyIds = new();
        private List<string> _fareBasisCodes = new();
        private List<int> _cabinClassIds = new();
        private List<long> _rbdIds = new();

        private FareCriteria()
        {
        }

        public IReadOnlyList<long> AirFareIds => _airFareIds.AsReadOnly();

        public IReadOnlyList<AirFareType> AirFareTypes => _airFareTypes.AsReadOnly();

        public IReadOnlyList<long> FareFamilyIds => _fareFamilyIds.AsReadOnly();

        public IReadOnlyList<string> FareBasisCodes => _fareBasisCodes.AsReadOnly();

        public IReadOnlyList<int> CabinClassIds => _cabinClassIds.AsReadOnly();

        public IReadOnlyList<long> RbdIds => _rbdIds.AsReadOnly();

        public static FareCriteria Create(
            IReadOnlyList<long>? airFareIds,
            IReadOnlyList<AirFareType>? airFareTypes,
            IReadOnlyList<long>? fareFamilyIds,
            IReadOnlyList<string>? fareBasisCodes,
            IReadOnlyList<int>? cabinClassIds,
            IReadOnlyList<long>? rbdIds)
        {
            var airFares = airFareIds ?? [];
            var types = airFareTypes ?? [];
            var families = fareFamilyIds ?? [];
            var bases = (fareBasisCodes ?? []).Select(code => (code ?? string.Empty).Trim().ToUpperInvariant()).ToList();
            var cabins = cabinClassIds ?? [];
            var rbds = rbdIds ?? [];

            Require(AreIds(airFares), nameof(AirFareIds));
            Require(types.All(type => Enum.IsDefined(type)) && types.Distinct().Count() == types.Count, nameof(AirFareTypes));
            Require(AreIds(families), nameof(FareFamilyIds));
            Require(
                bases.All(code => code.Length is >= 1 and <= FareBasisMaxLength && !code.Any(char.IsWhiteSpace))
                && bases.Distinct(StringComparer.Ordinal).Count() == bases.Count,
                nameof(FareBasisCodes));
            Require(cabins.All(id => id > 0) && cabins.Distinct().Count() == cabins.Count, nameof(CabinClassIds));
            Require(AreIds(rbds), nameof(RbdIds));

            return new FareCriteria
            {
                _airFareIds = airFares.ToList(),
                _airFareTypes = types.ToList(),
                _fareFamilyIds = families.ToList(),
                _fareBasisCodes = bases,
                _cabinClassIds = cabins.ToList(),
                _rbdIds = rbds.ToList()
            };
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return string.Join(',', _airFareIds);
            yield return string.Join(',', _airFareTypes);
            yield return string.Join(',', _fareFamilyIds);
            yield return string.Join(',', _fareBasisCodes);
            yield return string.Join(',', _cabinClassIds);
            yield return string.Join(',', _rbdIds);
        }

        private static bool AreIds(IReadOnlyList<long> ids)
            => ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count;

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ProvisionIsInvalid($"{nameof(FareCriteria)}.{field}");
        }
    }
}
