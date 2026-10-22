using System.Buffers.Text;
using System.Text;
using System.Text.Json;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.SelectionIds
{
    public static class NonSeatSelectionIdCodec
    {
        public const int Version = 1;
        public const int MaximumLength = 4096;

        private static readonly string[] Members = ["v", "source", "scope", "offerItemRef", "profile", "variantCode", "selection"];
        private static readonly string[] SourceMembers = ["kind", "ref", "orderCommercialVersion"];
        private static readonly string[] ScopeMembers = ["channel", "pointOfSaleId", "airlineOfficeId", "travelAgencyId", "customerId", "countryId"];

        public static string Encode(NonSeatSelectionId id)
        {
            using var buffer = new MemoryStream();
            using (var writer = new Utf8JsonWriter(buffer))
            {
                writer.WriteStartObject();
                writer.WriteNumber(Members[0], Version);
                writer.WriteStartObject(Members[1]);
                writer.WriteString(SourceMembers[0], id.SourceKind.ToString());
                writer.WriteString(SourceMembers[1], id.SourceRef);

                if (id.OrderCommercialVersion is { } version)
                    writer.WriteNumber(SourceMembers[2], version);
                else
                    writer.WriteNull(SourceMembers[2]);

                writer.WriteEndObject();
                writer.WriteStartObject(Members[2]);
                writer.WriteString(ScopeMembers[0], id.Scope.Channel.ToString());
                writer.WriteNumber(ScopeMembers[1], id.Scope.PointOfSaleId);
                WriteOptional(writer, ScopeMembers[2], id.Scope.AirlineOfficeId);
                WriteOptional(writer, ScopeMembers[3], id.Scope.TravelAgencyId);
                WriteOptional(writer, ScopeMembers[4], id.Scope.CustomerId);
                WriteOptional(writer, ScopeMembers[5], id.Scope.CountryId);
                writer.WriteEndObject();
                writer.WriteString(Members[3], id.OfferItemRef);
                writer.WriteString(Members[4], id.Profile.ToString());
                writer.WriteString(Members[5], id.VariantCode);
                writer.WritePropertyName(Members[6]);
                SelectionFields.Write(writer, id.Selection);
                writer.WriteEndObject();
            }

            var encoded = Base64Url.EncodeToString(buffer.ToArray());

            return encoded.Length <= MaximumLength ? encoded : throw new InvalidOperationException("The selection id exceeds the maximum length.");
        }

        public static NonSeatSelectionId? Decode(string? id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length > MaximumLength || !Base64Url.IsValid(id))
                return null;

            NonSeatSelectionId? decoded;

            try
            {
                using var document = JsonDocument.Parse(Base64Url.DecodeFromChars(id));

                decoded = Read(document.RootElement);
            }
            catch (Exception exception) when (exception is JsonException or FormatException or InvalidOperationException or OverflowException)
            {
                return null;
            }

            return decoded is not null && string.Equals(Encode(decoded), id, StringComparison.Ordinal) ? decoded : null;
        }

        private static NonSeatSelectionId? Read(JsonElement root)
        {
            if (!HasExactly(root, Members) || !root.GetProperty(Members[0]).TryGetInt32(out var version) || version != Version)
                return null;

            var source = root.GetProperty(Members[1]);
            var scope = root.GetProperty(Members[2]);

            if (!HasExactly(source, SourceMembers) || !HasExactly(scope, ScopeMembers))
                return null;

            if (!Enum.TryParse<ShoppingSourceKind>(Text(source, SourceMembers[0]), false, out var kind) || kind is not (ShoppingSourceKind.Offer or ShoppingSourceKind.Order))
                return null;

            var sourceRef = Text(source, SourceMembers[1]);
            var versionElement = source.GetProperty(SourceMembers[2]);
            int? orderVersion = versionElement.ValueKind == JsonValueKind.Number && versionElement.TryGetInt32(out var number) ? number : null;

            if (sourceRef is null || (kind == ShoppingSourceKind.Offer ? versionElement.ValueKind != JsonValueKind.Null : orderVersion is not > 0))
                return null;

            if (!Enum.TryParse<Channel>(Text(scope, ScopeMembers[0]), false, out var channel) || !Enum.IsDefined(channel))
                return null;

            var pointOfSale = scope.GetProperty(ScopeMembers[1]);

            if (pointOfSale.ValueKind != JsonValueKind.Number || !pointOfSale.TryGetInt64(out var pointOfSaleId) || pointOfSaleId <= 0)
                return null;

            if (!TryOptional(scope, ScopeMembers[2], out var office) || !TryOptional(scope, ScopeMembers[3], out var agency) || !TryOptional(scope, ScopeMembers[4], out var customer) || !TryOptional(scope, ScopeMembers[5], out var country))
                return null;

            var offerItemRef = Text(root, Members[3]);
            var variantCode = Text(root, Members[5]);
            var variant = AncillaryVariant.Find(variantCode);

            if (offerItemRef is null
                || variant is null
                || !Enum.TryParse<AncillaryProfile>(Text(root, Members[4]), false, out var profile)
                || variant.Profile != profile
                || (profile == AncillaryProfile.Seat && variantCode != AncillaryVariant.ExtraSeat))
                return null;

            var selection = SelectionFields.Read(root.GetProperty(Members[6]));

            return selection is null
                ? null
                : new NonSeatSelectionId(kind, sourceRef, orderVersion, new NonSeatSelectionScope(channel, pointOfSaleId, office, agency, customer, country), offerItemRef, profile, variantCode!, selection);
        }

        private static bool HasExactly(JsonElement element, string[] members)
            => element.ValueKind == JsonValueKind.Object && element.EnumerateObject().Select(member => member.Name).SequenceEqual(members, StringComparer.Ordinal);

        private static string? Text(JsonElement element, string member)
            => element.GetProperty(member) is { ValueKind: JsonValueKind.String } value && value.GetString() is { Length: > 0 } text ? text : null;

        private static bool TryOptional(JsonElement element, string member, out long? value)
        {
            value = null;
            var property = element.GetProperty(member);

            if (property.ValueKind == JsonValueKind.Null)
                return true;

            if (property.ValueKind != JsonValueKind.Number || !property.TryGetInt64(out var number) || number <= 0)
                return false;

            value = number;

            return true;
        }

        private static void WriteOptional(Utf8JsonWriter writer, string member, long? value)
        {
            if (value is { } number)
                writer.WriteNumber(member, number);
            else
                writer.WriteNull(member);
        }
    }
}
