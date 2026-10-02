using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.ValueObjects;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProductAggregate.ValueObjects
{
    public sealed class DocumentPolicy : ValueObject
    {
        private DocumentPolicy()
        {
        }

        public DocumentPolicy(AncillaryDocumentType type, string? rfisc, string? rfic)
        {
            if (!Enum.IsDefined(type))
                throw ExceptionFactory.AncillaryProductIsInvalid($"{nameof(DocumentPolicy)}.{nameof(Type)}");

            if (IsEmd(type) && rfisc is null)
                throw ExceptionFactory.AncillaryProductIsInvalid($"{nameof(DocumentPolicy)}.{nameof(Rfisc)}");

            Type = type;
            Rfisc = rfisc;
            Rfic = rfic;
        }

        public AncillaryDocumentType Type { get; private set; }

        public string? Rfisc { get; private set; }

        public string? Rfic { get; private set; }

        public DocumentPolicy Copy() => new(Type, Rfisc, Rfic);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Type;
            yield return Rfisc;
            yield return Rfic;
        }

        private static bool IsEmd(AncillaryDocumentType type) => type == AncillaryDocumentType.EmdAssociated;
    }
}
