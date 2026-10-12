namespace AeroTech.Ancillary.ReferenceData.ReadModels
{
    public sealed class PointOfSaleConditionReadModel
    {
        public long Id { get; set; }
        public long PointOfSaleId { get; set; }
        public bool? CountryIsInclude { get; set; }
        public List<int> CountryIds { get; set; } = [];
        public bool? AirlineOfficeIsInclude { get; set; }
        public List<long> AirlineOfficeIds { get; set; } = [];
        public bool? TravelAgencyOfficeIsInclude { get; set; }
        public List<long> TravelAgencyOfficeIds { get; set; } = [];
        public bool? ChannelIsInclude { get; set; }
        public List<string> Channels { get; set; } = [];
    }
}
