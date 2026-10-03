using AeroTech.Ancillary.Domain.ServiceReservationAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.ServiceReservationAggregate
{
    public sealed class ServiceReservationUnitConfiguration : IEntityTypeConfiguration<ServiceReservationUnit>
    {
        public void Configure(EntityTypeBuilder<ServiceReservationUnit> builder)
        {
            builder.ToTable("ServiceReservationUnits");
            builder.HasKey(unit => unit.Id);
            builder.Property(unit => unit.Id).ValueGeneratedNever();

            builder.Property(unit => unit.UnitReference).HasMaxLength(128).IsRequired();
            builder.Property(unit => unit.ProductRef).HasMaxLength(20).IsRequired();
            builder.Property(unit => unit.TravellerRef).HasMaxLength(50).IsRequired();
            builder.Property(unit => unit.BoundRef).HasMaxLength(50).IsRequired();
            builder.Property(unit => unit.FlightRef).HasMaxLength(50);

            builder.PrimitiveCollection(unit => unit.CoveredFlightIds)
                .HasField("_coveredFlightIds")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .IsRequired();

            builder.HasIndex(unit => new { unit.ServiceReservationId, unit.UnitReference }).IsUnique();
        }
    }
}
