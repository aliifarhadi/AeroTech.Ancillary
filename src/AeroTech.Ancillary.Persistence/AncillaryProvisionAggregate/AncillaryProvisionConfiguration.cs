using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
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
            builder.OwnsOne(provision => provision.Quantity, quantity =>
            {
                quantity.Property(value => value.Unit).HasColumnName("QuantityUnit");
                quantity.Property(value => value.MinQuantity).HasColumnName("MinQuantity");
                quantity.Property(value => value.MaxQuantity).HasColumnName("MaxQuantity");
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
            builder.Property(provision => provision.QuoteProviderKey).HasMaxLength(50);
            builder.Navigation(provision => provision.Quantity).IsRequired();
            builder.Navigation(provision => provision.Outcome).IsRequired();
            builder.Navigation(provision => provision.Settlement).IsRequired();
            builder.Navigation(provision => provision.Availability).IsRequired();
            builder.Navigation(provision => provision.Fulfillment).IsRequired();
            builder.HasOne(provision => provision.PassengerEligibility)
                .WithOne()
                .HasForeignKey<ProvisionPassengerEligibilityRule>(rule => rule.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(provision => provision.SalesRestrictions)
                .WithOne()
                .HasForeignKey<ProvisionSalesRestrictionsRule>(rule => rule.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(provision => provision.Geography)
                .WithOne()
                .HasForeignKey<ProvisionGeographyRule>(rule => rule.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(provision => provision.FlightApplication)
                .WithOne()
                .HasForeignKey<ProvisionFlightApplicationRule>(rule => rule.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(provision => provision.FareApplication)
                .WithOne()
                .HasForeignKey<ProvisionFareApplicationRule>(rule => rule.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(provision => provision.TravelDate)
                .WithOne()
                .HasForeignKey<ProvisionTravelDateRule>(rule => rule.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(provision => provision.DayTimeApplication)
                .WithOne()
                .HasForeignKey<ProvisionDayTimeApplicationRule>(rule => rule.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(provision => provision.AdvancePurchase)
                .WithOne()
                .HasForeignKey<ProvisionAdvancePurchaseRule>(rule => rule.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(provision => provision.BaggageApplication)
                .WithOne()
                .HasForeignKey<ProvisionBaggageApplicationRule>(rule => rule.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(provision => provision.SeatApplication)
                .WithOne()
                .HasForeignKey<ProvisionSeatApplicationRule>(rule => rule.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(provision => provision.PetRule)
                .WithOne()
                .HasForeignKey<ProvisionPetRule>(rule => rule.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(provision => provision.AssistedTravelRule)
                .WithOne()
                .HasForeignKey<ProvisionAssistedTravelRule>(rule => rule.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(provision => provision.AirportServiceRule)
                .WithOne()
                .HasForeignKey<ProvisionAirportServiceRule>(rule => rule.AncillaryProvisionId)
                .OnDelete(DeleteBehavior.Cascade);
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
