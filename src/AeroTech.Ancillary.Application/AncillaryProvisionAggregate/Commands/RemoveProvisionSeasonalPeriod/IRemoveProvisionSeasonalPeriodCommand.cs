namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionSeasonalPeriod
{
    public interface IRemoveProvisionSeasonalPeriodCommand
    {
        long ProvisionId { get; }

        long RowId { get; }
    }
}
