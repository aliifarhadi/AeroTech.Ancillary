using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Requests
{
    public sealed record ChangeServiceDefinitionRequest(
        long SupplierId,
        string ServiceSubCode,
        ServiceSubCodeSource SubCodeSource,
        string? ServiceTypeCode,
        string? GroupCode,
        string? SubGroupCode,
        string? Description1Code,
        string? Description2Code,
        string CommercialName,
        string? Description,
        ServiceDefinitionDocumentInput Document,
        ServiceDefinitionBookingInput Booking,
        DateOnly? SalesEffectiveFrom,
        DateOnly? SalesDiscontinueOn);
}
