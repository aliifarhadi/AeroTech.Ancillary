using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.ServiceSubCodeAggregate
{
    public sealed class ServiceSubCode : AggregateRoot<long>
    {
        private const int CodeLength = 3;
        private const int RficLength = 1;
        private const int GroupCodeLength = 2;
        private const int CommercialNameMaxLength = 30;

        private ServiceSubCode()
        {
        }

        private ServiceSubCode(
            long id,
            int ownerAirlineId,
            string code,
            ServiceSubCodeSource source,
            string rfic,
            string groupCode,
            string? subGroupCode,
            string? description1Code,
            string? description2Code,
            string commercialName,
            DateTimeOffset createdAt)
        {
            Id = id;
            OwnerAirlineId = ownerAirlineId;
            Code = code;
            Source = source;
            Rfic = rfic;
            GroupCode = groupCode;
            SubGroupCode = subGroupCode;
            Description1Code = description1Code;
            Description2Code = description2Code;
            CommercialName = commercialName;
            Status = ServiceSubCodeStatus.Active;
            CreatedAt = createdAt;
        }

        public int OwnerAirlineId { get; private set; }

        public string Code { get; private set; } = default!;

        public ServiceSubCodeSource Source { get; private set; }

        public string Rfic { get; private set; } = default!;

        public string GroupCode { get; private set; } = default!;

        public string? SubGroupCode { get; private set; }

        public string? Description1Code { get; private set; }

        public string? Description2Code { get; private set; }

        public string CommercialName { get; private set; } = default!;

        public ServiceSubCodeStatus Status { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public static ServiceSubCode Register(
            long id,
            int ownerAirlineId,
            string code,
            string? rfic,
            string? groupCode,
            string? subGroupCode,
            string? description1Code,
            string? description2Code,
            string? commercialName,
            DateTimeOffset createdAt)
        {
            Require(ownerAirlineId > 0, nameof(OwnerAirlineId));
            Require(IsCode(code, CodeLength) && !code.StartsWith("98", StringComparison.Ordinal) && !code.StartsWith("99", StringComparison.Ordinal), nameof(Code));

            if (!char.IsAsciiDigit(code[0]))
            {
                Require(rfic is { Length: RficLength } && char.IsAsciiLetterUpper(rfic[0]), nameof(Rfic));
                Require(groupCode is not null && IsCode(groupCode, GroupCodeLength), nameof(GroupCode));
                Require(subGroupCode is null || IsCode(subGroupCode, GroupCodeLength), nameof(SubGroupCode));
                Require(description1Code is null || IsCode(description1Code, GroupCodeLength), nameof(Description1Code));
                Require(description2Code is null || IsCode(description2Code, GroupCodeLength), nameof(Description2Code));
                Require(commercialName is not null && IsCommercialName(commercialName), nameof(CommercialName));

                return new ServiceSubCode(
                    id,
                    ownerAirlineId,
                    code,
                    ServiceSubCodeSource.CarrierDefined,
                    rfic!,
                    groupCode!,
                    subGroupCode,
                    description1Code,
                    description2Code,
                    commercialName!,
                    createdAt);
            }

            Require(rfic is null, nameof(Rfic));
            Require(groupCode is null, nameof(GroupCode));
            Require(subGroupCode is null, nameof(SubGroupCode));
            Require(description1Code is null, nameof(Description1Code));
            Require(description2Code is null, nameof(Description2Code));
            Require(commercialName is null, nameof(CommercialName));

            var entry = IndustrySubCodeReference.Find(code) ?? throw ExceptionFactory.IndustrySubCodeNotInReference();

            return new ServiceSubCode(
                id,
                ownerAirlineId,
                code,
                ServiceSubCodeSource.Industry,
                entry.Rfic,
                entry.GroupCode,
                entry.SubGroupCode,
                entry.Description1Code,
                entry.Description2Code,
                entry.CommercialName,
                createdAt);
        }

        public void Retire()
        {
            if (Status != ServiceSubCodeStatus.Active)
                throw ExceptionFactory.ServiceSubCodeStatusChangeNotAllowed();

            Status = ServiceSubCodeStatus.Retired;
        }

        public void Reactivate()
        {
            if (Status != ServiceSubCodeStatus.Retired)
                throw ExceptionFactory.ServiceSubCodeStatusChangeNotAllowed();

            Status = ServiceSubCodeStatus.Active;
        }

        private static bool IsCode(string value, int length)
            => value.Length == length && value.All(character => char.IsAsciiLetterUpper(character) || char.IsAsciiDigit(character));

        private static bool IsCommercialName(string value)
            => value.Length is >= 1 and <= CommercialNameMaxLength
               && value.All(character => char.IsAsciiLetter(character) || char.IsAsciiDigit(character) || character == ' ');

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ServiceSubCodeIsInvalid(field);
        }
    }
}
