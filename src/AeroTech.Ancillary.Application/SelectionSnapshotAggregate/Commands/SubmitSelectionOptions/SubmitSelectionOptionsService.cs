using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Contracts;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.SubmitSelectionOptions
{
    public sealed class SubmitSelectionOptionsService : ISubmitSelectionOptionsService
    {
        private const string IdPrefix = "s_";
        private const int IdRandomBytes = 16;

        private static readonly JsonSerializerOptions SelectedJsonOptions = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly ITypedSelectionVerifier _verifier;
        private readonly ISelectionSnapshotRepository _snapshots;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;

        public SubmitSelectionOptionsService(
            ITypedSelectionVerifier verifier,
            ISelectionSnapshotRepository snapshots,
            IUnitOfWork unitOfWork,
            IClock clock)
        {
            _verifier = verifier;
            _snapshots = snapshots;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<SelectionSnapshotResult> SubmitAsync(ISubmitSelectionOptionsCommand command, CancellationToken cancellationToken = default)
        {
            var verified = await _verifier.VerifyAsync(
                new TypedSelectionVerification(command.Surface, command.Reference, command.OfferId, command.Family, command.Options),
                cancellationToken);
            var snapshot = SelectionSnapshot.Create(
                IdPrefix + Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(IdRandomBytes)),
                verified.PricingRateId,
                verified.CoverageScope,
                verified.CoverageScope == ServiceCoverageScope.Order ? null : verified.TravellerRef,
                verified.CoverageRef,
                verified.OfferBinding,
                verified.SalesBinding,
                verified.Quantity,
                JsonSerializer.Serialize(command.Options, command.Options.GetType(), SelectedJsonOptions),
                _clock.GetDateTime(),
                verified.ExpiresAtUtc);

            await _snapshots.AddAsync(snapshot, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SelectionSnapshotResult(snapshot.Id, snapshot.ExpiresAtUtc);
        }
    }
}
