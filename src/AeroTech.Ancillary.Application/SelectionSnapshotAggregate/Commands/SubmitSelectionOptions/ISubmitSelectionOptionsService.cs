namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions
{
    public interface ISubmitSelectionOptionsService
    {
        Task<SelectionSnapshotResult> SubmitAsync(ISubmitSelectionOptionsCommand command, CancellationToken cancellationToken = default);
    }
}
