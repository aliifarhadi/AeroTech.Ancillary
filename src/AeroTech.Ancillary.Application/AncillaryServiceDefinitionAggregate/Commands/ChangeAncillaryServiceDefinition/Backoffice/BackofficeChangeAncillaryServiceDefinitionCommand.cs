using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ChangeAncillaryServiceDefinition.Backoffice
{
    public sealed record BackofficeChangeAncillaryServiceDefinitionCommand(
        long ServiceDefinitionId,
        long SupplierId,
        string ServiceSubCode,
        ServiceSubCodeSource SubCodeSource,
        string? ServiceTypeCode,
        string? GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code,
        PricingUnit PricingUnit,
        ServiceDateBasis ServiceDateBasis,
        string CommercialName,
        string? Description,
        ServiceDefinitionDocumentInput Document,
        ServiceDefinitionBookingInput Booking,
        DateOnly? SalesEffectiveFrom,
        DateOnly? SalesDiscontinueOn) : IRequest<ServiceDefinitionResult>, IChangeAncillaryServiceDefinitionCommand;
}
