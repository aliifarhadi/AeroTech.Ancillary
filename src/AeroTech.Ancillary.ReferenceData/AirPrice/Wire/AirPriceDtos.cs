using System.Text.Json.Serialization;
using AeroTech.Ancillary.ReferenceData.Syncing;

namespace AeroTech.Ancillary.ReferenceData.AirPrice.Wire
{
    public sealed class AirPriceEnvelope<T>
    {
        public T? Data { get; set; }

        public AirPriceError[]? Errors { get; set; }
    }

    public sealed class AirPriceError
    {
        public int? Code { get; set; }

        public string? Title { get; set; }

        public string? Detail { get; set; }
    }

    public sealed class AirFareDto : ISyncSourceDto<long>
    {
        [JsonPropertyName("airFareId")]
        public long Id { get; set; }
        public long FareNumber { get; set; }
        public AirFareCabinClassDto? CabinClass { get; set; }
        public AirFareBookingClassDto? BookingClass { get; set; }
        public AirFareFareFamilyDto? FareFamily { get; set; }
        public string? FareBasis { get; set; }
        public DateTimeOffset LastUpdateTime { get; set; }
    }

    public sealed class AirFareCabinClassDto
    {
        public int CabinClassId { get; set; }
    }

    public sealed class AirFareBookingClassDto
    {
        public long RbdId { get; set; }
    }

    public sealed class AirFareFareFamilyDto
    {
        public long FareFamilyId { get; set; }
        public string? FareFamily { get; set; }
    }

    public sealed class PointOfSaleDto : ISyncSourceDto<long>
    {
        public long Id { get; set; }
        public long Number { get; set; }
        public string Title { get; set; } = default!;
        public DateTimeOffset LastUpdateTime { get; set; }
        public List<PointOfSaleConditionDto>? Conditions { get; set; }
    }

    public sealed class PointOfSaleConditionDto
    {
        public long Id { get; set; }
        public bool? CountryIsInclude { get; set; }
        public List<int>? CountryIds { get; set; }
        public bool? AirlineOfficeIsInclude { get; set; }
        public List<long>? AirlineOfficeIds { get; set; }
        public bool? TravelAgencyOfficeIsInclude { get; set; }
        public List<long>? TravelAgencyOfficeIds { get; set; }
        public bool? ChannelIsInclude { get; set; }
        public List<string>? Channels { get; set; }
    }
}
