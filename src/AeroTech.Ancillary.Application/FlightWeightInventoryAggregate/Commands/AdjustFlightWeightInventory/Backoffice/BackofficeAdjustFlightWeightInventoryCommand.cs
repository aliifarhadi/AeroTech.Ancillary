using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using MediatR;

namespace AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.AdjustFlightWeightInventory.Backoffice
{
    public sealed record BackofficeAdjustFlightWeightInventoryCommand(
        long InventoryId,
        decimal NewKg,
        string ReasonCode,
        string CorrelationId,
        long ExpectedVersion) : IRequest<FlightWeightAdjustmentResult>, IAdjustFlightWeightInventoryCommand;
}
