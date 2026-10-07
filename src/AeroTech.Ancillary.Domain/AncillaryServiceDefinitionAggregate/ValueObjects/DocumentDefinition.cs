using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.ValueObjects
{
    public sealed class DocumentDefinition : ValueObject
    {
        private DocumentDefinition()
        {
        }

        private DocumentDefinition(AncillaryDocumentType type, string? rfic, string? rfisc)
        {
            Type = type;
            Rfic = rfic;
            Rfisc = rfisc;
        }

        public AncillaryDocumentType Type { get; private set; }

        public string? Rfic { get; private set; }

        public string? Rfisc { get; private set; }

        public static DocumentDefinition Create(AncillaryDocumentType type, string? rfic, string? rfisc)
        {
            Require(Enum.IsDefined(type), nameof(Type));

            if (type == AncillaryDocumentType.None)
            {
                Require(rfic is null, nameof(Rfic));
                Require(rfisc is null, nameof(Rfisc));
            }
            else
            {
                Require(rfic is { Length: 1 } && IsUpperOrDigit(rfic), nameof(Rfic));
                Require(rfisc is { Length: 3 } && IsUpperOrDigit(rfisc), nameof(Rfisc));
            }

            return new DocumentDefinition(type, rfic, rfisc);
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Type;
            yield return Rfic;
            yield return Rfisc;
        }

        private static bool IsUpperOrDigit(string value)
            => value.All(ch => ch is >= 'A' and <= 'Z' or >= '0' and <= '9');

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ServiceDefinitionIsInvalid($"{nameof(DocumentDefinition)}.{field}");
        }
    }
}
