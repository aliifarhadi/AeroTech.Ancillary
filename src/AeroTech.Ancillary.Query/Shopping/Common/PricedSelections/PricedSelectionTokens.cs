using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AeroTech.Ancillary.Query.Shopping.Common.PricedSelections
{
    public sealed class PricedSelectionTokens : IPricedSelectionTokens
    {
        private const string Issuer = "AeroTech.Ancillary";
        private const string SelectionClaim = "selection";
        private const int MinimumKeyBytes = 32;
        private const int MaximumTokenBytes = 8192;

        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
        {
            IgnoreReadOnlyProperties = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly PricedSelectionOptions _options;
        private readonly IClock _clock;
        private readonly JsonWebTokenHandler _handler = new() { MaximumTokenSizeInBytes = MaximumTokenBytes };

        public PricedSelectionTokens(PricedSelectionOptions options, IClock clock)
        {
            _options = options;
            _clock = clock;
        }

        public PricedSelectionToken Issue(PricedSelection selection, DateTimeOffset expiresAtUtc)
        {
            var issuedAtUtc = _clock.GetDateTime().UtcDateTime;
            var token = _handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = Issuer,
                Audience = Issuer,
                IssuedAt = issuedAtUtc,
                NotBefore = issuedAtUtc,
                Expires = expiresAtUtc.UtcDateTime,
                SigningCredentials = new SigningCredentials(SigningKey(), SecurityAlgorithms.HmacSha256),
                Claims = new Dictionary<string, object>
                {
                    [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString("N"),
                    [SelectionClaim] = JsonSerializer.Serialize(selection, Json)
                }
            });

            return new PricedSelectionToken(token, expiresAtUtc);
        }

        public async Task<PricedSelectionReading> ReadAsync(string? token)
        {
            var key = SigningKey();

            if (string.IsNullOrWhiteSpace(token) || Encoding.UTF8.GetByteCount(token) > MaximumTokenBytes)
                return PricedSelectionReading.Rejected;

            var result = await _handler.ValidateTokenAsync(token, new TokenValidationParameters
            {
                ValidIssuer = Issuer,
                ValidAudience = Issuer,
                IssuerSigningKey = key,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidateLifetime = false
            });

            if (!result.IsValid || result.SecurityToken is not JsonWebToken signed || !signed.TryGetPayloadValue<string>(SelectionClaim, out var payload))
                return PricedSelectionReading.Rejected;

            var selection = Deserialize(payload);
            var now = _clock.GetDateTime().UtcDateTime;

            if (selection is not { SchemaVersion: PricedSelection.CurrentSchemaVersion } || now < signed.ValidFrom)
                return PricedSelectionReading.Rejected;

            var expiresAtUtc = new DateTimeOffset(signed.ValidTo, TimeSpan.Zero);

            return new PricedSelectionReading(now < signed.ValidTo ? SelectionResolutionStatus.Valid : SelectionResolutionStatus.Expired, selection, expiresAtUtc);
        }

        private static PricedSelection? Deserialize(string payload)
        {
            try
            {
                return JsonSerializer.Deserialize<PricedSelection>(payload, Json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private SymmetricSecurityKey SigningKey()
            => _options.SigningKey is { } key && Encoding.UTF8.GetByteCount(key) >= MinimumKeyBytes
                ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                : throw new InvalidOperationException($"'{PricedSelectionOptions.SigningKeyKey}' must be configured with at least {MinimumKeyBytes} bytes.");
    }
}
