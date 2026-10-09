using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Domain._Shared.Rules;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Entities
{
    public sealed class PassengerUsageLimit : Entity<long>
    {
        internal const int CountingFamilyCodeMaxLength = 30;

        private PassengerUsageLimit()
        {
        }

        internal PassengerUsageLimit(long id, long inventoryPolicyId, PassengerUsageLimitArgs args)
        {
            Id = id;
            InventoryPolicyId = inventoryPolicyId;
            LimitScope = args.LimitScope;
            Change(args);
        }

        public long InventoryPolicyId { get; private set; }

        public PassengerUsageLimitScope LimitScope { get; private set; }

        public int MaxUnits { get; private set; }

        public string CountingFamilyCode { get; private set; } = default!;

        public PassengerUsageKey KeyFor(PassengerUsageSubject subject)
        {
            var traveller = string.IsNullOrWhiteSpace(subject.StableTravellerIdentity) ? null : subject.StableTravellerIdentity.Trim();

            switch (LimitScope)
            {
                case PassengerUsageLimitScope.PerOrder:
                    Require(subject.OrderId is > 0, nameof(subject.OrderId));
                    Require(subject.OrderTravellerId is > 0, nameof(subject.OrderTravellerId));

                    return new PassengerUsageKey(CountingFamilyCode, LimitScope, null, subject.OrderId, subject.OrderTravellerId, null, null);
                case PassengerUsageLimitScope.PerFlightOccurrence:
                    Require(traveller is not null, nameof(subject.StableTravellerIdentity));
                    Require(subject.FlightId is > 0, nameof(subject.FlightId));

                    return new PassengerUsageKey(CountingFamilyCode, LimitScope, traveller, null, null, subject.FlightId, null);
                default:
                    Require(traveller is not null, nameof(subject.StableTravellerIdentity));
                    Require(subject.ServiceDate is not null, nameof(subject.ServiceDate));

                    return new PassengerUsageKey(CountingFamilyCode, LimitScope, traveller, null, null, null, subject.ServiceDate);
            }
        }

        internal static string Normalize(string? countingFamilyCode) => (countingFamilyCode ?? string.Empty).Trim().ToUpperInvariant();

        internal void Change(PassengerUsageLimitArgs args)
        {
            var code = Normalize(args.CountingFamilyCode);

            Require(Enum.IsDefined(args.LimitScope) && args.LimitScope == LimitScope, nameof(LimitScope));
            Require(args.MaxUnits > 0, nameof(MaxUnits));
            Require(InventoryRules.IsCode(code, CountingFamilyCodeMaxLength), nameof(CountingFamilyCode));

            MaxUnits = args.MaxUnits;
            CountingFamilyCode = code;
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.InventoryPolicyIsInvalid($"{nameof(PassengerUsageLimit)}.{field}");
        }
    }
}
