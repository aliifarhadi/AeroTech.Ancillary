using AeroTech.Ancillary.Domain.ServiceReservationAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.ServiceReservationAggregate
{
    public sealed class ServiceReservationConfiguration : IEntityTypeConfiguration<ServiceReservation>
    {
        public void Configure(EntityTypeBuilder<ServiceReservation> builder)
        {
            builder.ToTable("ServiceReservations");
            builder.HasKey(reservation => reservation.Id);
            builder.Property(reservation => reservation.Id).ValueGeneratedNever();

            builder.Property(reservation => reservation.IdempotencyKey).HasMaxLength(128).IsRequired();
            builder.Property(reservation => reservation.Reference).HasMaxLength(128).IsRequired();

            builder.HasMany(reservation => reservation.Units)
                .WithOne()
                .HasForeignKey(unit => unit.ServiceReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(reservation => reservation.Units).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(reservation => reservation.IdempotencyKey).IsUnique();
        }
    }
}
