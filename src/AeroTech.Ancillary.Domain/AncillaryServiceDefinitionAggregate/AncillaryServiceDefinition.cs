using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.SupplierAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Aggregates;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate
{
    public sealed class AncillaryServiceDefinition : AggregateRoot<long>
    {
        private const int RefMaxLength = 30;
        private const int CommercialNameMaxLength = 100;
        private const int DescriptionMaxLength = 500;

        private static readonly string[] ServiceTypeCodes = ["A", "B", "C", "E", "F", "M", "T", "R", "Z"];

        private AncillaryServiceDefinition()
        {
        }

        private AncillaryServiceDefinition(long id, int ownerAirlineId, long supplierId, string serviceDefinitionRef)
        {
            Id = id;
            OwnerAirlineId = ownerAirlineId;
            SupplierId = supplierId;
            ServiceDefinitionRef = serviceDefinitionRef;
        }

        public int OwnerAirlineId { get; private set; }

        public long SupplierId { get; private set; }

        public string ServiceDefinitionRef { get; private set; } = default!;

        public int Version { get; private set; }

        public string ServiceTypeCode { get; private set; } = default!;

        public string ServiceSubCode { get; private set; } = default!;

        public ServiceSubCodeSource SubCodeSource { get; private set; }

        public string GroupCode { get; private set; } = default!;

        public string? SubGroupCode { get; private set; }

        public string? Description1Code { get; private set; }

        public string? Description2Code { get; private set; }

        public string CommercialName { get; private set; } = default!;

        public string? Description { get; private set; }

        public DocumentDefinition Document { get; private set; } = default!;

        public BookingDefinition Booking { get; private set; } = default!;

        public DateOnly? SalesEffectiveFrom { get; private set; }

        public DateOnly? SalesDiscontinueOn { get; private set; }

        public ServiceDefinitionStatus Status { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? ActivatedAt { get; private set; }

        public DateTimeOffset? SuspendedAt { get; private set; }

        public DateTimeOffset? RetiredAt { get; private set; }

        public static AncillaryServiceDefinition Define(
            long id,
            int ownerAirlineId,
            long supplierId,
            string serviceDefinitionRef,
            int version,
            string serviceSubCode,
            ServiceSubCodeSource subCodeSource,
            ServiceDefinitionClassificationArgs classification,
            string commercialName,
            string? description,
            DocumentDefinition document,
            BookingDefinition booking,
            DateOnly? salesEffectiveFrom,
            DateOnly? salesDiscontinueOn,
            DateTimeOffset createdAt)
        {
            Require(ownerAirlineId > 0, nameof(OwnerAirlineId));
            Require(supplierId > 0, nameof(SupplierId));
            Require(IsRef(serviceDefinitionRef), nameof(ServiceDefinitionRef));
            Require(version >= 1, nameof(Version));
            Require(IsCode(serviceSubCode, 3), nameof(ServiceSubCode));
            Require(Enum.IsDefined(subCodeSource), nameof(SubCodeSource));
            Require(commercialName is { Length: >= 1 and <= CommercialNameMaxLength }, nameof(CommercialName));
            Require(description is null or { Length: >= 1 and <= DescriptionMaxLength }, nameof(Description));
            Require(
                salesEffectiveFrom is null || salesDiscontinueOn is null || salesEffectiveFrom <= salesDiscontinueOn,
                nameof(SalesDiscontinueOn));

            var definition = new AncillaryServiceDefinition(id, ownerAirlineId, supplierId, serviceDefinitionRef);

            definition.Version = version;
            definition.ServiceSubCode = serviceSubCode;
            definition.SubCodeSource = subCodeSource;
            definition.ApplyClassification(serviceSubCode, subCodeSource, classification, document);
            definition.CommercialName = commercialName;
            definition.Description = description;
            definition.Document = document;
            definition.Booking = booking;
            definition.SalesEffectiveFrom = salesEffectiveFrom;
            definition.SalesDiscontinueOn = salesDiscontinueOn;
            definition.Status = ServiceDefinitionStatus.Draft;
            definition.CreatedAt = createdAt;

            return definition;
        }

        public void Activate(Supplier supplier, DateTimeOffset now)
        {
            if (Status != ServiceDefinitionStatus.Draft)
                throw ExceptionFactory.ServiceDefinitionStatusChangeNotAllowed();

            if (supplier.Id != SupplierId)
                throw ExceptionFactory.ServiceDefinitionIsInvalid(nameof(SupplierId));

            if (supplier.Status != SupplierStatus.Active)
                throw ExceptionFactory.ServiceDefinitionSupplierNotActive();

            Status = ServiceDefinitionStatus.Active;
            ActivatedAt = now;
        }

        private void ApplyClassification(
            string serviceSubCode,
            ServiceSubCodeSource subCodeSource,
            ServiceDefinitionClassificationArgs classification,
            DocumentDefinition document)
        {
            if (subCodeSource == ServiceSubCodeSource.Industry)
            {
                var entry = IndustryServiceSubCodeReference.Find(serviceSubCode)
                            ?? throw ExceptionFactory.IndustryServiceSubCodeNotFound(serviceSubCode);

                RequireIndustryMatch(classification.ServiceTypeCode, entry.ServiceTypeCode, serviceSubCode, nameof(ServiceTypeCode));
                RequireIndustryMatch(classification.GroupCode, entry.GroupCode, serviceSubCode, nameof(GroupCode));
                RequireIndustryMatch(classification.SubGroupCode, entry.SubGroupCode, serviceSubCode, nameof(SubGroupCode));
                RequireIndustryMatch(classification.Description1Code, entry.Description1Code, serviceSubCode, nameof(Description1Code));
                RequireIndustryMatch(classification.Description2Code, entry.Description2Code, serviceSubCode, nameof(Description2Code));

                if (entry.DocumentType is { } documentType && document.Type != documentType)
                    throw ExceptionFactory.IndustryServiceSubCodeSemanticsConflict($"{nameof(Document)}.{nameof(DocumentDefinition.Type)}", serviceSubCode);

                if (entry.Rfic is { } rfic && document.Type != AncillaryDocumentType.None && document.Rfic != rfic)
                    throw ExceptionFactory.IndustryServiceSubCodeSemanticsConflict($"{nameof(Document)}.{nameof(DocumentDefinition.Rfic)}", serviceSubCode);

                if (document.Type != AncillaryDocumentType.None && document.Rfisc != entry.Code)
                    throw ExceptionFactory.IndustryServiceSubCodeSemanticsConflict($"{nameof(Document)}.{nameof(DocumentDefinition.Rfisc)}", serviceSubCode);

                ServiceTypeCode = entry.ServiceTypeCode;
                GroupCode = entry.GroupCode;
                SubGroupCode = entry.SubGroupCode;
                Description1Code = entry.Description1Code;
                Description2Code = entry.Description2Code;
            }
            else
            {
                Require(
                    classification.ServiceTypeCode is not null && ServiceTypeCodes.Contains(classification.ServiceTypeCode),
                    nameof(ServiceTypeCode));
                Require(IsCode(classification.GroupCode, 2), nameof(GroupCode));
                Require(classification.SubGroupCode is null || IsCode(classification.SubGroupCode, 2), nameof(SubGroupCode));
                Require(classification.Description1Code is null || IsCode(classification.Description1Code, 2), nameof(Description1Code));
                Require(classification.Description2Code is null || IsCode(classification.Description2Code, 2), nameof(Description2Code));

                ServiceTypeCode = classification.ServiceTypeCode!;
                GroupCode = classification.GroupCode!;
                SubGroupCode = classification.SubGroupCode;
                Description1Code = classification.Description1Code;
                Description2Code = classification.Description2Code;
            }
        }

        private static void RequireIndustryMatch(string? supplied, string? reference, string serviceSubCode, string field)
        {
            if (supplied is not null && supplied != reference)
                throw ExceptionFactory.IndustryServiceSubCodeSemanticsConflict(field, serviceSubCode);
        }

        private static bool IsRef(string? value)
            => value is { Length: >= 1 and <= RefMaxLength }
               && value.All(ch => ch is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_');

        private static bool IsCode(string? value, int length)
            => value is not null
               && value.Length == length
               && value.All(ch => ch is >= 'A' and <= 'Z' or >= '0' and <= '9');

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.ServiceDefinitionIsInvalid(field);
        }
    }
}
