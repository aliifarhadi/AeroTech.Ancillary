using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service
{
    public sealed record ServicePriceLineDto(
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Code,
        decimal Amount,
        int CurrencyId,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? CountryId,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? StationAirportId,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] TaxTreatment? TaxTreatment,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] FeeApplicationUnit? FeeApplicationUnit);
}
