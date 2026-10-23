using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Options;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.VerifyTypedSelection
{
    public sealed class TypedSelectionVerifier : ITypedSelectionVerifier
    {
        private readonly IBackofficeSalesScopeResolver _backoffice;
        private readonly IOtaPanelSalesScopeResolver _otaPanel;
        private readonly IApiSalesScopeResolver _api;
        private readonly IFlightOfferShoppingSource _source;
        private readonly ITypedSelectionService _typed;

        public TypedSelectionVerifier(
            IBackofficeSalesScopeResolver backoffice,
            IOtaPanelSalesScopeResolver otaPanel,
            IApiSalesScopeResolver api,
            IFlightOfferShoppingSource source,
            ITypedSelectionService typed)
        {
            _backoffice = backoffice;
            _otaPanel = otaPanel;
            _api = api;
            _source = source;
            _typed = typed;
        }

        public async Task<VerifiedTypedSelection> VerifyAsync(TypedSelectionVerification verification, CancellationToken cancellationToken = default)
        {
            var reference = await _typed.ReadReferenceAsync(verification.Reference);
            var scope = await ScopeAsync(verification.Surface, cancellationToken);
            var shopping = await _source.LoadAsync(reference.OfferId, scope, cancellationToken);

            PricedOfferScopeGuard.EnsureSellable(shopping.Offer, scope);

            var proof = await _typed.ProveAsync(
                reference,
                verification.Family,
                Selection(verification.Options),
                shopping.PointsOfSale.Select(pointOfSale => pointOfSale.Context).ToList(),
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

        private Task<AncillarySalesScope> ScopeAsync(AuthorizationSurface surface, CancellationToken cancellationToken)
            => surface switch
            {
                AuthorizationSurface.Backoffice => _backoffice.ResolveAsync(null, cancellationToken),
                AuthorizationSurface.OtaPanel => _otaPanel.ResolveAsync(cancellationToken),
                AuthorizationSurface.Api => _api.ResolveAsync(cancellationToken),
                _ => throw ExceptionFactory.ShoppingCallerNotAuthorized()
            };

        private static AncillarySelection Selection(ISelectionOptions options)
            => new()
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
            };

        private static SelectedDimensions? Dimensions(SelectedDimensionsInput? dimensions)
            => dimensions is null ? null : new SelectedDimensions(dimensions.LengthCm, dimensions.WidthCm, dimensions.HeightCm);
    }
}
