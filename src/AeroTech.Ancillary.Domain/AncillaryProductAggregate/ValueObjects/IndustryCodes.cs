using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;

namespace AeroTech.Ancillary.Domain.AncillaryProductAggregate.ValueObjects
{
    public sealed class IndustryCodes : ValueObject
    {
        private const int ServiceTypeCodeLength = 1;

        private IndustryCodes()
        {
        }

        public IndustryCodes(
            string serviceTypeCode,
            string groupCode,
            string? subGroupCode,
            string? description1Code,
            string? description2Code)
        {
            if (serviceTypeCode is not { Length: ServiceTypeCodeLength } || !char.IsAsciiLetterUpper(serviceTypeCode[0]))
                throw ExceptionFactory.AncillaryProductIsInvalid($"{nameof(IndustryCodes)}.{nameof(ServiceTypeCode)}");

            ServiceTypeCode = serviceTypeCode;
            GroupCode = groupCode;
            SubGroupCode = subGroupCode;
            Description1Code = description1Code;
            Description2Code = description2Code;
        }

        public string ServiceTypeCode { get; private set; } = default!;

        public string GroupCode { get; private set; } = default!;

        public string? SubGroupCode { get; private set; }

        public string? Description1Code { get; private set; }

        public string? Description2Code { get; private set; }

        public IndustryCodes Copy() => new(ServiceTypeCode, GroupCode, SubGroupCode, Description1Code, Description2Code);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return ServiceTypeCode;
            yield return GroupCode;
            yield return SubGroupCode;
            yield return Description1Code;
            yield return Description2Code;
        }
    }
}
