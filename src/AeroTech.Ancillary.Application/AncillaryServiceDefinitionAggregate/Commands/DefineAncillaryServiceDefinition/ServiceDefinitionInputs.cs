using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition
{
    public sealed record ServiceDefinitionDocumentInput(
        AncillaryDocumentType Type,
        string? Rfic,
        string? Rfisc);

    public sealed record ServiceDefinitionBookingInput(
        BookingMethod Method,
        string? SsrCode,
        string? SsimCode);
}
