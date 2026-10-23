using System.Globalization;
using System.Security.Claims;
using System.Text;
using AeroTech.Ancillary.Shopping.Engine;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AeroTech.Ancillary.Shopping.Selections
{
    public sealed class SelectionIdCodec : ISelectionIdCodec
    {
        public const int MaximumLength = 1024;

        private const int MinimumKeyBytes = 32;
        private const string KindClaim = "p";
        private const string RateClaim = "rate";
        private const string ScopeClaim = "sc";
        private const string TravellerClaim = "tr";
        private const string CoverageClaim = "cv";
        private const string QuantityClaim = "q";
        private const string OfferBindingClaim = "ob";
        private const string SalesBindingClaim = "sb";

        private static readonly IReadOnlyDictionary<SelectionIdKind, string> Prefixes = new Dictionary<SelectionIdKind, string>
        {
            [SelectionIdKind.Reference] = "r_",
            [SelectionIdKind.Direct] = "d_"
        };

        private static readonly IReadOnlyDictionary<SelectionIdKind, string> Purposes = new Dictionary<SelectionIdKind, string>
        {
            [SelectionIdKind.Reference] = "r",
            [SelectionIdKind.Direct] = "d"
        };

        private readonly SelectionIdOptions _options;
        private readonly IClock _clock;
        private readonly JsonWebTokenHandler _handler = new() { SetDefaultTimesOnTokenCreation = false, MaximumTokenSizeInBytes = MaximumLength };

        public SelectionIdCodec(SelectionIdOptions options, IClock clock)
        {
            _options = options;
            _clock = clock;
        }

        public string Issue(SignedSelection selection)
        {
            if (!IsWellFormed(selection))
                throw new InvalidOperationException("The signed selection is not well formed.");

            var claims = new List<Claim>
            {
                new(KindClaim, Purposes[selection.Kind]),
                new(RateClaim, Number(selection.PricingRateId), ClaimValueTypes.Integer64),
                new(ScopeClaim, Number((int)selection.CoverageScope), ClaimValueTypes.Integer32),
                new(TravellerClaim, selection.TravellerRef),
                new(CoverageClaim, selection.CoverageRef),
                new(OfferBindingClaim, selection.OfferBinding),
                new(SalesBindingClaim, selection.SalesBinding)
            };

            if (selection.Quantity is { } quantity)
                claims.Add(new Claim(QuantityClaim, Number(quantity), ClaimValueTypes.Integer32));

            var token = _handler.CreateToken(new SecurityTokenDescriptor
            {
                Expires = selection.ExpiresAtUtc.UtcDateTime,
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = new SigningCredentials(SigningKey(), SecurityAlgorithms.HmacSha256)
            });
            var id = Prefixes[selection.Kind] + token;

            return id.Length <= MaximumLength ? id : throw new InvalidOperationException("The signed selection exceeds the maximum length.");
        }

        public async Task<SignedSelectionReading> ReadAsync(string? id, SelectionIdKind kind)
        {
            var key = SigningKey();
            var prefix = Prefixes[kind];

            if (string.IsNullOrEmpty(id) || id.Length > MaximumLength || !id.StartsWith(prefix, StringComparison.Ordinal))
                return SignedSelectionReading.Rejected;

            var result = await _handler.ValidateTokenAsync(id[prefix.Length..], new TokenValidationParameters
            {
                IssuerSigningKey = key,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false
            });

            if (!result.IsValid || result.SecurityToken is not JsonWebToken token || Parse(token, kind) is not { } selection)
                return SignedSelectionReading.Rejected;

            return _clock.GetDateTime() < selection.ExpiresAtUtc
                ? new SignedSelectionReading(SignedSelectionStatus.Valid, selection)
                : new SignedSelectionReading(SignedSelectionStatus.Expired, selection);
        }

        private static SignedSelection? Parse(JsonWebToken token, SelectionIdKind kind)
        {
            if (!token.TryGetPayloadValue<string>(KindClaim, out var purpose) || purpose != Purposes[kind]
                || !token.TryGetPayloadValue<long>(RateClaim, out var rate)
                || !token.TryGetPayloadValue<int>(ScopeClaim, out var scope)
                || !token.TryGetPayloadValue<string>(TravellerClaim, out var traveller)
                || !token.TryGetPayloadValue<string>(CoverageClaim, out var coverage)
                || !token.TryGetPayloadValue<string>(OfferBindingClaim, out var offerBinding)
                || !token.TryGetPayloadValue<string>(SalesBindingClaim, out var salesBinding))
                return null;

            int? quantity = token.TryGetPayloadValue<int>(QuantityClaim, out var count) ? count : null;
            var selection = new SignedSelection(
                kind,
                rate,
                (ServiceCoverageScope)scope,
                traveller,
                coverage,
                quantity,
                offerBinding,
                salesBinding,
                new DateTimeOffset(token.ValidTo, TimeSpan.Zero));

            return IsWellFormed(selection) ? selection : null;
        }

        private static bool IsWellFormed(SignedSelection selection)
            => selection.PricingRateId > 0
               && Enum.IsDefined(selection.CoverageScope)
               && IsUnit(selection.CoverageScope, selection.TravellerRef, selection.CoverageRef)
               && selection.OfferBinding is { Length: SelectionBindings.Length }
               && selection.SalesBinding is { Length: SelectionBindings.Length }
               && selection.Kind switch
               {
                   SelectionIdKind.Direct => selection is { Quantity: > 0 },
                   SelectionIdKind.Reference => selection is { Quantity: null },
                   _ => false
               };

        private static bool IsUnit(ServiceCoverageScope scope, string? travellerRef, string? coverageRef)
            => travellerRef is not null && coverageRef is { Length: > 0 } && scope switch
            {
                ServiceCoverageScope.Order => travellerRef.Length == 0 && coverageRef == CoveragePlanner.OrderRef,
                ServiceCoverageScope.Journey => travellerRef.Length > 0 && coverageRef == CoveragePlanner.JourneyRef,
                _ => travellerRef.Length > 0 && coverageRef is not (CoveragePlanner.OrderRef or CoveragePlanner.JourneyRef)
            };

        private static string Number(long value) => value.ToString(CultureInfo.InvariantCulture);

        private SymmetricSecurityKey SigningKey()
            => _options.SigningKey is { } key && Encoding.UTF8.GetByteCount(key) >= MinimumKeyBytes
                ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                : throw new InvalidOperationException($"'{SelectionIdOptions.SigningKeyKey}' must be configured with at least {MinimumKeyBytes} bytes.");
    }
}
