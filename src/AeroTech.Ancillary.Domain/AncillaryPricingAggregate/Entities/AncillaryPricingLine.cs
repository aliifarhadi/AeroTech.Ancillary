using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities
{
    public sealed class AncillaryPricingLine : Entity<long>
    {
        private const int CodeMaxLength = 10;
        private const int NameMaxLength = 100;

        private AncillaryPricingLine()
        {
        }

        internal AncillaryPricingLine(long id, long ancillaryPricingId, AncillaryPricingLineArgs args)
        {
            Require(args.PassengerTypeCode is null || Enum.IsDefined(args.PassengerTypeCode.Value), nameof(PassengerTypeCode));
            Require(args.AgeFromInclusive is null or >= 0, nameof(AgeFromInclusive));
            Require(
                args.AgeToExclusive is null || (args.AgeFromInclusive is not null && args.AgeToExclusive > args.AgeFromInclusive),
                nameof(AgeToExclusive));
            Require(Enum.IsDefined(args.Category), nameof(Category));
            Require(args.Code is null or { Length: >= 1 and <= CodeMaxLength }, nameof(Code));
            Require(args.Category == AncillaryPriceLineCategory.Ancillary || args.Code is not null, nameof(Code));
            Require(args.Name is null or { Length: >= 1 and <= NameMaxLength }, nameof(Name));
            Require(args.CountryId is null or > 0, nameof(CountryId));
            Require(args.StationAirportId is null or > 0, nameof(StationAirportId));
            Require(args.Amount >= 0 && args.Amount == decimal.Round(args.Amount, 2), nameof(Amount));
            Require(args.Category != AncillaryPriceLineCategory.Ancillary || args.Amount > 0, nameof(Amount));

            Id = id;
            AncillaryPricingId = ancillaryPricingId;
            PassengerTypeCode = args.PassengerTypeCode;
            AgeFromInclusive = args.AgeFromInclusive;
            AgeToExclusive = args.AgeToExclusive;
            Category = args.Category;
            Code = args.Code;
            Name = args.Name;
            CountryId = args.CountryId;
            StationAirportId = args.StationAirportId;
            Amount = args.Amount;
        }

        public long AncillaryPricingId { get; private set; }

        public PassengerTypeCode? PassengerTypeCode { get; private set; }

        public int? AgeFromInclusive { get; private set; }

        public int? AgeToExclusive { get; private set; }

        public AncillaryPriceLineCategory Category { get; private set; }

        public string? Code { get; private set; }

        public string? Name { get; private set; }

        public int? CountryId { get; private set; }

        public int? StationAirportId { get; private set; }

        public decimal Amount { get; private set; }

        internal AncillaryPricingLineArgs ToArgs()
            => new(PassengerTypeCode, AgeFromInclusive, AgeToExclusive, Category, Code, Name, CountryId, StationAirportId, Amount);

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.PricingIsInvalid($"{nameof(AncillaryPricingLine)}.{field}");
        }
    }
}
