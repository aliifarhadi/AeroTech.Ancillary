using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Arguments;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public sealed class PriceAncillaryOffersService : IPriceAncillaryOffersService
    {
        private const string IdPrefix = "s_";
        private const int IdRandomBytes = 16;

        private static readonly JsonSerializerOptions SelectedJsonOptions = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly ISelectionVerifier _verifier;
        private readonly ISeatPriceVerifier _seatPrices;
        private readonly ISelectionSnapshotRepository _snapshots;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public PriceAncillaryOffersService(
            ISelectionVerifier verifier,
            ISeatPriceVerifier seatPrices,
            ISelectionSnapshotRepository snapshots,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _verifier = verifier;
            _seatPrices = seatPrices;
            _snapshots = snapshots;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<SelectionSnapshotResult> SubmitAsync(IPriceAncillaryOffersCommand command, CancellationToken cancellationToken = default)
        {
            var createdAtUtc = _clock.GetDateTime();
            var selections = command.Selections.Select(Submitted).ToList();
            var verified = await _verifier.VerifyAsync(new SelectionVerification(command.Surface, command.OfferId, selections), cancellationToken);

            return await SaveAsync(
                SelectionSnapshot.CreateForOffer(NewId(), command.OfferId!, Lines(verified, selections), createdAtUtc, verified.Min(line => line.ExpiresAtUtc)),
                cancellationToken);
        }

        public async Task<SelectionSnapshotResult> SubmitAsync(IPriceOrderAncillaryOffersCommand command, CancellationToken cancellationToken = default)
        {
            var createdAtUtc = _clock.GetDateTime();
            var orderId = command.OrderId!.Value;
            var selections = command.Selections.Select(Submitted).ToList();
            var verified = await _verifier.VerifyAsync(new OrderSelectionVerification(command.Surface, orderId, selections), cancellationToken);

            return await SaveAsync(
                SelectionSnapshot.CreateForOrder(
                    NewId(),
                    orderId,
                    verified.OrderCommercialVersion,
                    Lines(verified.Selections, selections),
                    createdAtUtc,
                    verified.Selections.Min(line => line.ExpiresAtUtc)),
                cancellationToken);
        }

        public async Task<SelectionSnapshotResult> SubmitAsync(IPriceOfferSeatsCommand command, CancellationToken cancellationToken = default)
        {
            var createdAtUtc = _clock.GetDateTime();
            var verified = await _seatPrices.VerifyAsync(new SeatPriceVerification(command.Surface, command.OfferId, command.Ids), cancellationToken);

            return await SaveAsync(
                SelectionSnapshot.CreateForOffer(NewId(), command.OfferId!, Lines(verified, Seats(command)), createdAtUtc, verified.Min(line => line.ExpiresAtUtc)),
                cancellationToken);
        }

        public async Task<SelectionSnapshotResult> SubmitAsync(IPriceOrderSeatsCommand command, CancellationToken cancellationToken = default)
        {
            var createdAtUtc = _clock.GetDateTime();
            var orderId = command.OrderId!.Value;
            var verified = await _seatPrices.VerifyAsync(new OrderSeatPriceVerification(command.Surface, orderId, command.Ids), cancellationToken);

            return await SaveAsync(
                SelectionSnapshot.CreateForOrder(
                    NewId(),
                    orderId,
                    verified.OrderCommercialVersion,
                    Lines(verified.Selections, Seats(command)),
                    createdAtUtc,
                    verified.Selections.Min(line => line.ExpiresAtUtc)),
                cancellationToken);
        }

        private async Task<SelectionSnapshotResult> SaveAsync(SelectionSnapshot snapshot, CancellationToken cancellationToken)
        {
            await _snapshots.AddAsync(snapshot, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SelectionSnapshotResult(snapshot.Id, snapshot.ExpiresAtUtc);
        }

        private static string NewId() => IdPrefix + Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(IdRandomBytes));

        private static SubmittedSelection Submitted(AncillarySelectionLine line)
            => line switch
            {
                { OfferItemId: { } id, Options: null } => new DirectSelection(id, line.Quantity),
                { OfferItemId: { } id, Options.Baggage: { } options } => new TypedSelection(id, AncillaryProfile.Baggage, options),
                { OfferItemId: { } id, Options.Pet: { } options } => new TypedSelection(id, AncillaryProfile.Pet, options),
                { OfferItemId: { } id, Options.AssistedTravel: { } options } => new TypedSelection(id, AncillaryProfile.AssistedTravel, options),
                { OfferItemId: { } id, Options.AirportService: { } options } => new TypedSelection(id, AncillaryProfile.AirportService, options),
                { OfferItemId: { } id, Options.Priority: { } options } => new TypedSelection(id, AncillaryProfile.Priority, options),
                { OfferItemId: { } id, Options.Connectivity: { } options } => new TypedSelection(id, AncillaryProfile.Connectivity, options),
                _ => throw ExceptionFactory.SelectionOptionsAreInvalid("the options of one family are required")
            };

        private static IReadOnlyList<SubmittedSelection> Seats(IPriceSeatsCommand command)
            => command.Ids.Select(id => new DirectSelection(id, null)).ToList();

        private static IReadOnlyList<SelectionSnapshotLineArgs> Lines(IReadOnlyList<VerifiedSelection> verified, IReadOnlyList<SubmittedSelection> selections)
            => selections.Select((selection, index) => Line(verified[index], selection)).ToList();

        private static SelectionSnapshotLineArgs Line(VerifiedSelection verified, SubmittedSelection selection)
            => new(
                selection.Id,
                verified.PricingRateId,
                verified.CoverageScope,
                verified.CoverageScope == ServiceCoverageScope.Order ? null : verified.TravellerRef,
                verified.CoverageRef,
                verified.SourceBinding,
                verified.SalesBinding,
                verified.Quantity,
                selection is TypedSelection typed ? JsonSerializer.Serialize(typed.Options, typed.Options.GetType(), SelectedJsonOptions) : null);
    }
}
