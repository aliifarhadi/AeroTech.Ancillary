namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ActivateAncillaryProduct
{
    public interface IActivateAncillaryProductService
    {
        Task<ActivateAncillaryProductResult> ActivateAsync(IActivateAncillaryProductCommand command, CancellationToken cancellationToken = default);
    }
}
