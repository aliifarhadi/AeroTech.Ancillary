using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class AncillaryProvisionConfiguration : IEntityTypeConfiguration<AncillaryProvision>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvision> builder)
        {
            builder.ToTable("AncillaryProvisions");
            builder.HasKey(provision => provision.Id);
            builder.Property(provision => provision.Id).ValueGeneratedNever();

            builder.OwnsOne(provision => provision.Passenger, passenger =>
            {
                passenger.PrimitiveCollection(value => value.PassengerTypeCodes)
                    .HasField("_passengerTypeCodes")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("PassengerTypeCodes")
                    .IsRequired();
            });

            builder.OwnsOne(provision => provision.Sales, sales =>
            {
                sales.PrimitiveCollection(value => value.PointOfSaleIds)
                    .HasField("_pointOfSaleIds")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("PointOfSaleIds")
                    .IsRequired();
                sales.PrimitiveCollection(value => value.CustomerIds)
                    .HasField("_customerIds")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("CustomerIds")
                    .IsRequired();
                sales.PrimitiveCollection(value => value.CustomerTypes)
                    .HasField("_customerTypes")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("CustomerTypes")
                    .IsRequired();
            });

            builder.OwnsOne(provision => provision.Travel, travel =>
            {
                travel.PrimitiveCollection(value => value.OriginAirportIds)
                    .HasField("_originAirportIds")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("OriginAirportIds")
                    .IsRequired();
                travel.PrimitiveCollection(value => value.DestinationAirportIds)
                    .HasField("_destinationAirportIds")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("DestinationAirportIds")
                    .IsRequired();
                travel.PrimitiveCollection(value => value.ViaAirportIds)
                    .HasField("_viaAirportIds")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("ViaAirportIds")
                    .IsRequired();
                travel.Property(value => value.TravelFrom).HasColumnName("TravelFrom");
                travel.Property(value => value.TravelTo).HasColumnName("TravelTo");
                travel.PrimitiveCollection(value => value.DaysOfWeek)
                    .HasField("_daysOfWeek")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("DaysOfWeek")
                    .IsRequired();
                travel.Property(value => value.TimeFrom).HasColumnName("TimeFrom");
                travel.Property(value => value.TimeTo).HasColumnName("TimeTo");
                travel.PrimitiveCollection(value => value.MarketingAirlineIds)
                    .HasField("_marketingAirlineIds")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("MarketingAirlineIds")
                    .IsRequired();
                travel.PrimitiveCollection(value => value.OperatingAirlineIds)
                    .HasField("_operatingAirlineIds")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("OperatingAirlineIds")
                    .IsRequired();
                travel.PrimitiveCollection(value => value.FlightNumbers)
                    .HasField("_flightNumbers")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("FlightNumbers")
                    .HasColumnType("nvarchar(max)")
                    .IsRequired();
                travel.PrimitiveCollection(value => value.FlightIds)
                    .HasField("_flightIds")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("FlightIds")
                    .IsRequired();
                travel.PrimitiveCollection(value => value.AircraftIds)
                    .HasField("_aircraftIds")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("AircraftIds")
                    .IsRequired();
            });

            builder.OwnsOne(provision => provision.Fare, fare =>
            {
                fare.PrimitiveCollection(value => value.AirFareIds)
                    .HasField("_airFareIds")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("AirFareIds")
                    .IsRequired();
                fare.PrimitiveCollection(value => value.AirFareTypes)
                    .HasField("_airFareTypes")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("AirFareTypes")
                    .IsRequired();
                fare.PrimitiveCollection(value => value.FareFamilyIds)
                    .HasField("_fareFamilyIds")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("FareFamilyIds")
                    .IsRequired();
                fare.PrimitiveCollection(value => value.FareBasisCodes)
                    .HasField("_fareBasisCodes")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("FareBasisCodes")
                    .HasColumnType("nvarchar(max)")
                    .IsRequired();
                fare.PrimitiveCollection(value => value.CabinClassIds)
                    .HasField("_cabinClassIds")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("CabinClassIds")
                    .IsRequired();
                fare.PrimitiveCollection(value => value.RbdIds)
                    .HasField("_rbdIds")
                    .UsePropertyAccessMode(PropertyAccessMode.Field)
                    .HasColumnName("RbdIds")
                    .IsRequired();
            });

            builder.OwnsOne(provision => provision.AdvancePurchase, advancePurchase =>
            {
                advancePurchase.Property(value => value.Period).HasColumnName("AdvancePurchasePeriod");
                advancePurchase.Property(value => value.Unit).HasColumnName("AdvancePurchaseUnit");
            });

            builder.OwnsOne(provision => provision.Quantity, quantity =>
            {
                quantity.Property(value => value.Unit).HasColumnName("QuantityUnit");
                quantity.Property(value => value.MinQuantity).HasColumnName("MinQuantity");
                quantity.Property(value => value.MaxQuantity).HasColumnName("MaxQuantity");
            });

            builder.OwnsOne(provision => provision.Application, application =>
            {
                application.Property(value => value.Type).HasColumnName("ApplicationType");

                application.OwnsOne(value => value.Baggage, baggage =>
                {
                    baggage.Property(value => value.FreePieces).HasColumnName("BaggageFreePieces");
                    baggage.Property(value => value.FirstExcessPiece).HasColumnName("BaggageFirstExcessPiece");
                    baggage.Property(value => value.LastExcessPiece).HasColumnName("BaggageLastExcessPiece");
                    baggage.Property(value => value.Weight).HasColumnName("BaggageWeight").HasPrecision(9, 2);
                    baggage.Property(value => value.WeightUnit).HasColumnName("BaggageWeightUnit");
                    baggage.Property(value => value.TravelApplication).HasColumnName("BaggageTravelApplication");
                    baggage.Property(value => value.PurchaseApplication).HasColumnName("BaggagePurchaseApplication");
                    baggage.Property(value => value.RuleDeference).HasColumnName("BaggageRuleDeference");
                });

                application.OwnsOne(value => value.Seat, seat =>
                {
                    seat.PrimitiveCollection(value => value.SeatNumbers)
                        .HasField("_seatNumbers")
                        .UsePropertyAccessMode(PropertyAccessMode.Field)
                        .HasColumnName("SeatNumbers")
                        .HasColumnType("nvarchar(max)")
                        .IsRequired();
                    seat.PrimitiveCollection(value => value.SeatCharacteristicCodes)
                        .HasField("_seatCharacteristicCodes")
                        .UsePropertyAccessMode(PropertyAccessMode.Field)
                        .HasColumnName("SeatCharacteristicCodes")
                        .HasColumnType("nvarchar(max)")
                        .IsRequired();
                });
            });

            builder.OwnsOne(provision => provision.Outcome, outcome =>
            {
                outcome.Property(value => value.Disposition).HasColumnName("Disposition");
                outcome.Property(value => value.DocumentRequired).HasColumnName("DocumentRequired");
                outcome.Property(value => value.BookingRequired).HasColumnName("BookingRequired");
            });

            builder.OwnsOne(provision => provision.Fee, fee =>
            {
                fee.Property(value => value.CurrencyId).HasColumnName("FeeCurrencyId");
                fee.Property(value => value.ApplicationUnit).HasColumnName("FeeApplicationUnit");
            });

            builder.OwnsOne(provision => provision.Settlement, settlement =>
            {
                settlement.Property(value => value.ReissueRefund).HasColumnName("ReissueRefund");
                settlement.Property(value => value.FormOfRefund).HasColumnName("FormOfRefund");
                settlement.Property(value => value.Commissionable).HasColumnName("Commissionable");
                settlement.Property(value => value.InterlineSettlement).HasColumnName("InterlineSettlement");
            });

            builder.OwnsOne(provision => provision.Availability, availability =>
            {
                availability.Property(value => value.MustCheckAvailability).HasColumnName("MustCheckAvailability");
            });

            builder.OwnsOne(provision => provision.Fulfillment, fulfillment =>
            {
                fulfillment.Property(value => value.FulfillmentProviderKey).HasColumnName("FulfillmentProviderKey").HasMaxLength(50).IsRequired();
            });

            builder.Navigation(provision => provision.Passenger).IsRequired();
            builder.Navigation(provision => provision.Sales).IsRequired();
            builder.Navigation(provision => provision.Travel).IsRequired();
            builder.Navigation(provision => provision.Fare).IsRequired();
            builder.Navigation(provision => provision.Quantity).IsRequired();
            builder.Navigation(provision => provision.Application).IsRequired();
            builder.Navigation(provision => provision.Outcome).IsRequired();
            builder.Navigation(provision => provision.Settlement).IsRequired();
            builder.Navigation(provision => provision.Availability).IsRequired();
            builder.Navigation(provision => provision.Fulfillment).IsRequired();

            builder.HasMany(provision => provision.RoutePairs)
                .WithOne()
                .HasForeignKey(pair => pair.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(provision => provision.RoutePairs).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.PriceLines)
                .WithOne()
                .HasForeignKey(line => line.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(provision => provision.PriceLines).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(
                    provision => new { provision.ServiceDefinitionId, provision.Sequence },
                    "IX_AncillaryProvisions_ServiceDefinitionId_Sequence_Active")
                .IsUnique()
                .HasFilter($"[Status] = {(int)ProvisionStatus.Active}");
            builder.HasIndex(provision => provision.ServiceDefinitionId);
            builder.HasIndex(provision => provision.Status);
        }
    }
}
