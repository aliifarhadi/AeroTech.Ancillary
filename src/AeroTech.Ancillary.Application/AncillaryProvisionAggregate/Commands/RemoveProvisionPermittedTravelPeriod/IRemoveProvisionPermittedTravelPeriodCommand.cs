namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionPermittedTravelPeriod
{
    public interface IRemoveProvisionPermittedTravelPeriodCommand
    {
        long ProvisionId { get; }

        long RowId { get; }
    }
}
