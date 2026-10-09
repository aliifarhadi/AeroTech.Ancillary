using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition
{
    public interface IDefineAncillaryServiceDefinitionCommand
    {
        int OwnerAirlineId { get; }

        long SupplierId { get; }

        string ServiceDefinitionRef { get; }

        string ServiceSubCode { get; }

        ServiceSubCodeSource SubCodeSource { get; }

        string? ServiceTypeCode { get; }

        string? GroupCode { get; }

        string? SubGroupCode { get; }

        string? Description1Code { get; }

        string? Description2Code { get; }

        PricingUnit PricingUnit { get; }

        ServiceDateBasis ServiceDateBasis { get; }

        AncillaryProfile Profile { get; }

        string VariantCode { get; }

        DocumentRouting DocumentRouting { get; }

        ServiceSpecificationInput Specification { get; }

        string CommercialName { get; }

        string? Description { get; }

        ServiceDefinitionDocumentInput Document { get; }

        ServiceDefinitionBookingInput Booking { get; }

        DateOnly? SalesEffectiveFrom { get; }

        DateOnly? SalesDiscontinueOn { get; }
    }
}
