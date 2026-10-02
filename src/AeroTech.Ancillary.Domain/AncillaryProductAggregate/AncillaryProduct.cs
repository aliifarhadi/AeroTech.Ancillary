using AeroTech.Ancillary.Domain.AncillaryProductAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProductAggregate
{
    public sealed class AncillaryProduct : AggregateRoot<long>
    {
        private const int ProductRefMinLength = 2;
        private const int ProductRefMaxLength = 20;
        private const int NameMaxLength = 100;
        private const int DescriptionMaxLength = 500;

        private const string BaggageRfic = "C";
        private const string BaggageGroupCode = "BG";

        private static readonly string[] BaggageServiceTypeCodes = ["C", "P"];

        private AncillaryProduct()
        {
        }

        private AncillaryProduct(
            long id,
            int ownerAirlineId,
            string productRef,
            int version,
            AncillaryProductType type,
            DateTimeOffset createdAt)
        {
            Id = id;
            OwnerAirlineId = ownerAirlineId;
            ProductRef = productRef;
            Version = version;
            Type = type;
            Status = AncillaryProductStatus.Draft;
            CreatedAt = createdAt;
        }

        public int OwnerAirlineId { get; private set; }

        public string ProductRef { get; private set; } = default!;

        public int Version { get; private set; }

        public AncillaryProductType Type { get; private set; }

        public string Name { get; private set; } = default!;

        public string? Description { get; private set; }

        public AncillarySalesScope SalesScope { get; private set; }

        public QuantityPolicy Quantity { get; private set; } = default!;

        public DocumentPolicy Document { get; private set; } = default!;

        public IndustryCodes Codes { get; private set; } = default!;

        public SalesTerms Terms { get; private set; } = default!;

        public AncillaryInventoryControl InventoryControl { get; private set; }

        public BaggageDetail? Baggage { get; private set; }

        public AncillaryProductStatus Status { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? ActivatedAt { get; private set; }

        public DateTimeOffset? RetiredAt { get; private set; }

        public static AncillaryProduct Define(
            long id,
            int ownerAirlineId,
            string productRef,
            AncillaryProductType type,
            string name,
            string? description,
            AncillarySalesScope salesScope,
            QuantityPolicy quantity,
            AncillaryDocumentType documentType,
            string? rfisc,
            string serviceTypeCode,
            SalesTerms terms,
            AncillaryInventoryControl inventoryControl,
            BaggageDetail? baggage,
            ServiceSubCode? subCode,
            DateTimeOffset createdAt)
        {
            Require(ownerAirlineId > 0, nameof(OwnerAirlineId));
            Require(IsProductRef(productRef), nameof(ProductRef));
            Require(Enum.IsDefined(type), nameof(Type));

            var product = new AncillaryProduct(id, ownerAirlineId, productRef, 1, type, createdAt);

            product.Apply(name, description, salesScope, quantity, documentType, rfisc, serviceTypeCode, terms, inventoryControl, baggage, subCode);

            return product;
        }

        public void Change(
            string name,
            string? description,
            AncillarySalesScope salesScope,
            QuantityPolicy quantity,
            AncillaryDocumentType documentType,
            string? rfisc,
            string serviceTypeCode,
            SalesTerms terms,
            AncillaryInventoryControl inventoryControl,
            BaggageDetail? baggage,
            ServiceSubCode? subCode)
        {
            if (Status != AncillaryProductStatus.Draft)
                throw ExceptionFactory.AncillaryProductIsNotDraft();

            Apply(name, description, salesScope, quantity, documentType, rfisc, serviceTypeCode, terms, inventoryControl, baggage, subCode);
        }

        public void Activate(ServiceSubCode? subCode, DateTimeOffset activatedAt)
        {
            if (Status is not (AncillaryProductStatus.Draft or AncillaryProductStatus.Suspended))
                throw ExceptionFactory.AncillaryProductStatusChangeNotAllowed();

            Apply(Name, Description, SalesScope, Quantity.Copy(), Document.Type, Document.Rfisc, Codes.ServiceTypeCode, Terms.Copy(), InventoryControl, Baggage?.Copy(), subCode);

            Status = AncillaryProductStatus.Active;
            ActivatedAt ??= activatedAt;
        }

        public void Suspend()
        {
            if (Status != AncillaryProductStatus.Active)
                throw ExceptionFactory.AncillaryProductStatusChangeNotAllowed();

            Status = AncillaryProductStatus.Suspended;
        }

        public void Retire(DateTimeOffset retiredAt)
        {
            if (Status == AncillaryProductStatus.Retired)
                throw ExceptionFactory.AncillaryProductStatusChangeNotAllowed();

            Status = AncillaryProductStatus.Retired;
            RetiredAt = retiredAt;
        }

        public AncillaryProduct Revise(long id, int version, DateTimeOffset createdAt)
        {
            if (Status is not (AncillaryProductStatus.Active or AncillaryProductStatus.Suspended))
                throw ExceptionFactory.AncillaryProductVersionCannotBeRevised();

            return new AncillaryProduct(id, OwnerAirlineId, ProductRef, version, Type, createdAt)
            {
                Name = Name,
                Description = Description,
                SalesScope = SalesScope,
                Quantity = Quantity.Copy(),
                Document = Document.Copy(),
                Codes = Codes.Copy(),
                Terms = Terms.Copy(),
                InventoryControl = InventoryControl,
                Baggage = Baggage?.Copy()
            };
        }

        private void Apply(
            string name,
            string? description,
            AncillarySalesScope salesScope,
            QuantityPolicy quantity,
            AncillaryDocumentType documentType,
            string? rfisc,
            string serviceTypeCode,
            SalesTerms terms,
            AncillaryInventoryControl inventoryControl,
            BaggageDetail? baggage,
            ServiceSubCode? subCode)
        {
            Require(name is { Length: >= 1 and <= NameMaxLength }, nameof(Name));
            Require(description is null or { Length: <= DescriptionMaxLength }, nameof(Description));
            Require(Enum.IsDefined(salesScope), nameof(SalesScope));
            Require(Enum.IsDefined(inventoryControl), nameof(InventoryControl));
            Require((baggage is not null) == (Type == AncillaryProductType.ExtraBaggage), nameof(Baggage));

            var requested = new DocumentPolicy(documentType, rfisc, null);

            if (subCode is null
                || subCode.OwnerAirlineId != OwnerAirlineId
                || subCode.Code != requested.Rfisc
                || subCode.Status != ServiceSubCodeStatus.Active)
                throw ExceptionFactory.AncillaryProductSubCodeIsNotActive();

            var document = new DocumentPolicy(documentType, subCode.Code, subCode.Rfic);
            var codes = new IndustryCodes(
                serviceTypeCode,
                subCode.GroupCode,
                subCode.SubGroupCode,
                subCode.Description1Code,
                subCode.Description2Code);

            if (Type == AncillaryProductType.ExtraBaggage)
            {
                Require(subCode.Rfic == BaggageRfic, nameof(DocumentPolicy.Rfic));
                Require(subCode.GroupCode == BaggageGroupCode, nameof(IndustryCodes.GroupCode));
                Require(BaggageServiceTypeCodes.Contains(serviceTypeCode), nameof(IndustryCodes.ServiceTypeCode));
            }

            if (subCode.Source == ServiceSubCodeSource.Industry)
            {
                var entry = IndustrySubCodeReference.Find(subCode.Code) ?? throw ExceptionFactory.IndustrySubCodeNotInReference();

                Require(documentType == entry.DocumentType, nameof(DocumentPolicy));
                Require(quantity.Min == entry.QuantityPerOccurrence && quantity.Max == entry.QuantityPerOccurrence, nameof(QuantityPolicy));
            }

            Require(
                !(subCode.Source == ServiceSubCodeSource.CarrierDefined
                  && Type == AncillaryProductType.ExtraBaggage
                  && terms.InterlineSettlementAllowed == true),
                nameof(SalesTerms.InterlineSettlementAllowed));

            Name = name;
            Description = description;
            SalesScope = salesScope;
            Quantity = quantity;
            Document = document;
            Codes = codes;
            Terms = terms;
            InventoryControl = inventoryControl;
            Baggage = baggage;
        }

        private static bool IsProductRef(string? value)
            => value is { Length: >= ProductRefMinLength and <= ProductRefMaxLength }
               && value.All(character => char.IsAsciiLetterUpper(character) || char.IsAsciiDigit(character));

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.AncillaryProductIsInvalid(field);
        }
    }
}
