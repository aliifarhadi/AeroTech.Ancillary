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
            });

            builder.OwnsOne(provision => provision.Outcome, outcome =>
            {
                outcome.Property(value => value.Disposition).HasColumnName("Disposition");
                outcome.Property(value => value.DocumentRequired).HasColumnName("DocumentRequired");
                outcome.Property(value => value.BookingRequired).HasColumnName("BookingRequired");
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

            builder.Navigation(provision => provision.Quantity).IsRequired();
            builder.Navigation(provision => provision.Application).IsRequired();
            builder.Navigation(provision => provision.Outcome).IsRequired();
            builder.Navigation(provision => provision.Settlement).IsRequired();
            builder.Navigation(provision => provision.Availability).IsRequired();
            builder.Navigation(provision => provision.Fulfillment).IsRequired();

            builder.HasMany(provision => provision.PassengerTypes)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.PassengerTypes).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.PointsOfSale)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.PointsOfSale).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.Customers)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.Customers).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.CustomerTypes)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.CustomerTypes).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.OriginAirports)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.OriginAirports).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.DestinationAirports)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.DestinationAirports).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.ViaAirports)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.ViaAirports).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.RoutePairs)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.RoutePairs).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.MarketingAirlines)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.MarketingAirlines).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.OperatingAirlines)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.OperatingAirlines).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.FlightNumbers)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.FlightNumbers).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.Flights)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.Flights).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.Aircraft)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.Aircraft).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.AirFares)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.AirFares).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.AirFareTypes)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.AirFareTypes).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.FareFamilies)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.FareFamilies).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.FareBases)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.FareBases).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.CabinClasses)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.CabinClasses).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.Rbds)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.Rbds).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.TravelDates)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.TravelDates).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.SeasonalPeriods)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.SeasonalPeriods).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.BlackoutPeriods)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.BlackoutPeriods).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.DayTimeRestrictions)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.DayTimeRestrictions).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.SeatNumbers)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.SeatNumbers).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(provision => provision.SeatCharacteristics)
                .WithOne()
                .HasForeignKey(row => row.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(provision => provision.SeatCharacteristics).UsePropertyAccessMode(PropertyAccessMode.Field);

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
