using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryReservationAggregate
{
    public sealed class AncillaryReservationUnitConfiguration : IEntityTypeConfiguration<AncillaryReservationUnit>
    {
        public void Configure(EntityTypeBuilder<AncillaryReservationUnit> builder)
        {
            builder.ToTable("AncillaryReservationUnits");
            builder.HasKey(unit => unit.Id);
            builder.Property(unit => unit.Id).ValueGeneratedNever();

            builder.Property(unit => unit.ProviderUnitRef).HasMaxLength(100);
            builder.Property(unit => unit.CancellationReasonCode).HasMaxLength(50);

            builder.PrimitiveCollection(unit => unit.CoveredFlightIds)
                .HasField("_coveredFlightIds")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .IsRequired();

            builder.HasIndex(unit => unit.AncillaryReservationId);
            builder.HasIndex(unit => unit.OrderServiceId);
        }
    }
}
