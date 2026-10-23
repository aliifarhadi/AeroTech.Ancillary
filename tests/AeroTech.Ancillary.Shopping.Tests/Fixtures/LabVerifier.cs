using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Options;
using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public sealed class LabVerifier : ITypedSelectionVerifier
{
    private readonly ITypedSelectionService _typed;
    private readonly IReadOnlyList<AncillaryShoppingContext> _contexts;

    public LabVerifier(ITypedSelectionService typed, IReadOnlyList<AncillaryShoppingContext> contexts)
    {
        _typed = typed;
        _contexts = contexts;
    }

    public async Task<VerifiedTypedSelection> VerifyAsync(TypedSelectionVerification verification, CancellationToken cancellationToken = default)
    {
        var reference = await _typed.ReadReferenceAsync(verification.Reference);
        var options = verification.Options;
        var proof = await _typed.ProveAsync(
            reference,
            verification.Family,
            new AncillarySelection
            {
                BagRef = options.BagRef,
                MeasuredWeightKg = options.MeasuredWeightKg,
                Dimensions = Dimensions(options.Dimensions),
                WeightKg = options.WeightKg,
                AnimalType = options.AnimalType,
                OtherAnimalCode = options.OtherAnimalCode,
                AnimalAgeWeeks = options.AnimalAgeWeeks,
                CombinedWeightKg = options.CombinedWeightKg,
                CarrierDimensions = Dimensions(options.CarrierDimensions),
                DocumentAcknowledgements = options.DocumentAcknowledgements,
                SizeBracket = options.SizeBracket,
                AssistanceSsrCode = options.AssistanceSsrCode,
                InfantRef = options.InfantRef,
                GuardianRef = options.GuardianRef,
                TimeWithOffset = options.TimeWithOffset,
                GuestCount = options.GuestCount,
                OptIn = options.OptIn,
                DeviceCount = options.DeviceCount
            },
            _contexts,
            cancellationToken);

        return new VerifiedTypedSelection(
            proof.PricingRateId,
            proof.CoverageScope,
            proof.TravellerRef,
            proof.CoverageRef,
            proof.OfferBinding,
            proof.SalesBinding,
            proof.Quantity,
            proof.ExpiresAtUtc);
    }

    private static SelectedDimensions? Dimensions(SelectedDimensionsInput? dimensions)
        => dimensions is null ? null : new SelectedDimensions(dimensions.LengthCm, dimensions.WidthCm, dimensions.HeightCm);
}
