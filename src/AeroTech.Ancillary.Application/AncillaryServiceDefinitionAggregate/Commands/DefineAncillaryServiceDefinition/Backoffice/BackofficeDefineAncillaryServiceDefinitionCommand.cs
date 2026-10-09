using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition.Backoffice
{
    public sealed record BackofficeDefineAncillaryServiceDefinitionCommand(
        int OwnerAirlineId,
        long SupplierId,
        string ServiceDefinitionRef,
        string ServiceSubCode,
        ServiceSubCodeSource SubCodeSource,
        string? ServiceTypeCode,
        string? GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code,
        PricingUnit PricingUnit,
        ServiceDateBasis ServiceDateBasis,
        AncillaryProfile Profile,
        string VariantCode,
        DocumentRouting DocumentRouting,
        ServiceSpecificationInput Specification,
        string CommercialName,
        string? Description,
        ServiceDefinitionDocumentInput Document,
        ServiceDefinitionBookingInput Booking,
        DateOnly? SalesEffectiveFrom,
        DateOnly? SalesDiscontinueOn) : IRequest<ServiceDefinitionResult>, IDefineAncillaryServiceDefinitionCommand;
}
