using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryServiceDefinitionAggregate
{
    public sealed class AncillaryServiceDefinitionConfiguration : IEntityTypeConfiguration<AncillaryServiceDefinition>
    {
        public void Configure(EntityTypeBuilder<AncillaryServiceDefinition> builder)
        {
            builder.ToTable("AncillaryServiceDefinitions");
            builder.HasKey(definition => definition.Id);
            builder.Property(definition => definition.Id).ValueGeneratedNever();

            builder.Property(definition => definition.ServiceDefinitionRef).HasMaxLength(30).IsRequired();
            builder.Property(definition => definition.ServiceTypeCode).HasMaxLength(1).IsRequired();
            builder.Property(definition => definition.ServiceSubCode).HasMaxLength(3).IsRequired();
            builder.Property(definition => definition.GroupCode).HasMaxLength(2).IsRequired();
            builder.Property(definition => definition.SubGroupCode).HasMaxLength(2);
            builder.Property(definition => definition.Description1Code).HasMaxLength(2);
            builder.Property(definition => definition.Description2Code).HasMaxLength(2);
            builder.Property(definition => definition.CommercialName).HasMaxLength(100).IsRequired();
            builder.Property(definition => definition.Description).HasMaxLength(500);

            builder.OwnsOne(definition => definition.Document, document =>
            {
                document.Property(value => value.Type).HasColumnName("DocumentType");
                document.Property(value => value.Rfic).HasColumnName("DocumentRfic").HasMaxLength(1);
                document.Property(value => value.Rfisc).HasColumnName("DocumentRfisc").HasMaxLength(3);
            });

            builder.OwnsOne(definition => definition.Booking, booking =>
            {
                booking.Property(value => value.Method).HasColumnName("BookingMethod");
                booking.Property(value => value.SsrCode).HasColumnName("BookingSsrCode").HasMaxLength(4);
                booking.Property(value => value.SsimCode).HasColumnName("BookingSsimCode").HasMaxLength(4);
                booking.Property(value => value.ConfirmationRequirement).HasColumnName("BookingConfirmationRequirement");
            });

            builder.Navigation(definition => definition.Document).IsRequired();
            builder.Navigation(definition => definition.Booking).IsRequired();

            builder.HasIndex(definition => new { definition.OwnerAirlineId, definition.ServiceDefinitionRef, definition.Version }).IsUnique();
            builder.HasIndex(
                    definition => new { definition.OwnerAirlineId, definition.ServiceDefinitionRef },
                    "IX_AncillaryServiceDefinitions_OwnerAirlineId_Ref_Active")
                .IsUnique()
                .HasFilter($"[Status] = {(int)ServiceDefinitionStatus.Active}");
            builder.HasIndex(definition => definition.SupplierId);
            builder.HasIndex(definition => definition.Status);
        }
    }
}
