using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Requests
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record DefineServiceDefinitionRequest<TSpecification>(
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
        string VariantCode,
        DocumentRouting DocumentRouting,
        TSpecification Specification,
        string CommercialName,
        string? Description,
        ServiceDefinitionDocumentInput Document,
        ServiceDefinitionBookingInput Booking,
        DateOnly? SalesEffectiveFrom,
        DateOnly? SalesDiscontinueOn)
        where TSpecification : class;
}
