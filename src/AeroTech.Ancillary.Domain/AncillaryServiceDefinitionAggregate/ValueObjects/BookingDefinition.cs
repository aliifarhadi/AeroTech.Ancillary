using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.ValueObjects
{
    public sealed class BookingDefinition : ValueObject
    {
        private const int CodeLength = 4;

        private BookingDefinition()
        {
        }

        private BookingDefinition(BookingMethod method, string? ssrCode, string? ssimCode, ConfirmationRequirement confirmationRequirement)
        {
            Method = method;
            SsrCode = ssrCode;
            SsimCode = ssimCode;
            ConfirmationRequirement = confirmationRequirement;
        }

        public BookingMethod Method { get; private set; }

        public string? SsrCode { get; private set; }

        public string? SsimCode { get; private set; }

        public ConfirmationRequirement ConfirmationRequirement { get; private set; }

        public static BookingDefinition Create(
            BookingMethod method,
            string? ssrCode,
            string? ssimCode,
            ConfirmationRequirement confirmationRequirement = ConfirmationRequirement.Immediate)
        {
            Require(Enum.IsDefined(method), nameof(Method));
            Require(method != BookingMethod.Ssr || IsCode(ssrCode), nameof(SsrCode));
            Require(ssrCode is null || IsCode(ssrCode), nameof(SsrCode));
            Require(ssimCode is null || IsCode(ssimCode), nameof(SsimCode));
            Require(Enum.IsDefined(confirmationRequirement), nameof(ConfirmationRequirement));

            return new BookingDefinition(method, ssrCode, ssimCode, confirmationRequirement);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Method;
            yield return SsrCode;
            yield return SsimCode;
            yield return ConfirmationRequirement;
        }

        private static bool IsCode(string? value)
            => value is { Length: >= 1 and <= CodeLength }
               && value.All(ch => ch is >= 'A' and <= 'Z' or >= '0' and <= '9');

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ServiceDefinitionIsInvalid($"{nameof(BookingDefinition)}.{field}");
        }
    }
}
