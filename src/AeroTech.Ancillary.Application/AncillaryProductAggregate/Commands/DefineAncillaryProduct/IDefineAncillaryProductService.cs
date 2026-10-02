namespace AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct
{
    public interface IDefineAncillaryProductService
    {
        Task<AncillaryProductResult> DefineAsync(IDefineAncillaryProductCommand command, CancellationToken cancellationToken = default);
    }
}
