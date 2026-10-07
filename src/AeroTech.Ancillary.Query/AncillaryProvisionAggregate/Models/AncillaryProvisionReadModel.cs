using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionReadModel
    {
        public long Id { get; set; }

        public long ServiceDefinitionId { get; set; }

        public int Sequence { get; set; }

        public ProvisionStatus Status { get; set; }

        public DateTimeOffset? SalesEffectiveFrom { get; set; }

        public DateTimeOffset? SalesDiscontinueAt { get; set; }

        public ServiceCoverageScope CoverageScope { get; set; }

        public List<PassengerTypeCode> PassengerTypeCodes { get; set; } = new();

        public List<long> PointOfSaleIds { get; set; } = new();

        public List<long> CustomerIds { get; set; } = new();

        public List<CustomerType> CustomerTypes { get; set; } = new();

        public List<int> OriginAirportIds { get; set; } = new();

        public List<int> DestinationAirportIds { get; set; } = new();

        public List<int> ViaAirportIds { get; set; } = new();

        public DateOnly? TravelFrom { get; set; }

        public DateOnly? TravelTo { get; set; }

        public List<DayOfWeek> DaysOfWeek { get; set; } = new();

        public TimeOnly? TimeFrom { get; set; }

        public TimeOnly? TimeTo { get; set; }

        public List<int> MarketingAirlineIds { get; set; } = new();

        public List<int> OperatingAirlineIds { get; set; } = new();

        public List<string> FlightNumbers { get; set; } = new();

        public List<long> FlightIds { get; set; } = new();

        public List<int> AircraftIds { get; set; } = new();

        public List<long> AirFareIds { get; set; } = new();

        public List<AirFareType> AirFareTypes { get; set; } = new();

        public List<long> FareFamilyIds { get; set; } = new();

        public List<string> FareBasisCodes { get; set; } = new();

        public List<int> CabinClassIds { get; set; } = new();

        public List<long> RbdIds { get; set; } = new();

        public int? AdvancePurchasePeriod { get; set; }

        public TimeUnit? AdvancePurchaseUnit { get; set; }

        public AncillaryQuantityUnit QuantityUnit { get; set; }

        public int MinQuantity { get; set; }

        public int MaxQuantity { get; set; }

        public ProvisionApplicationType ApplicationType { get; set; }

        public int? BaggageFreePieces { get; set; }

        public int? BaggageFirstExcessPiece { get; set; }

        public int? BaggageLastExcessPiece { get; set; }

        public decimal? BaggageWeight { get; set; }

        public WeightUnit? BaggageWeightUnit { get; set; }

        public BaggageTravelApplication? BaggageTravelApplication { get; set; }

        public BaggagePurchaseApplication? BaggagePurchaseApplication { get; set; }

        public BaggageRuleDeference? BaggageRuleDeference { get; set; }

        public List<string> SeatNumbers { get; set; } = new();

        public List<string> SeatCharacteristicCodes { get; set; } = new();

        public CommercialDisposition Disposition { get; set; }

        public bool DocumentRequired { get; set; }

        public bool BookingRequired { get; set; }

        public int? FeeCurrencyId { get; set; }

        public FeeApplicationUnit? FeeApplicationUnit { get; set; }

        public ReissueRefundPolicy ReissueRefund { get; set; }

        public FormOfRefund? FormOfRefund { get; set; }

        public bool Commissionable { get; set; }

        public bool InterlineSettlement { get; set; }

        public bool MustCheckAvailability { get; set; }

        public string FulfillmentProviderKey { get; set; } = default!;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? ActivatedAt { get; set; }

        public DateTimeOffset? SuspendedAt { get; set; }

        public DateTimeOffset? RetiredAt { get; set; }

        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
