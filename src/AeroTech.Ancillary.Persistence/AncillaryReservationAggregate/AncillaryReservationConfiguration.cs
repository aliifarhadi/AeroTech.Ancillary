using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryReservationAggregate
{
    public sealed class AncillaryReservationConfiguration : IEntityTypeConfiguration<AncillaryReservation>
    {
        public void Configure(EntityTypeBuilder<AncillaryReservation> builder)
        {
            builder.ToTable("AncillaryReservations");
            builder.HasKey(reservation => reservation.Id);
            builder.Property(reservation => reservation.Id).ValueGeneratedNever();

            builder.Property(reservation => reservation.IdempotencyKey).HasMaxLength(128).IsRequired();
            builder.Property(reservation => reservation.Reference).HasMaxLength(128).IsRequired();

            builder.HasMany(reservation => reservation.Units)
                .WithOne()
                .HasForeignKey(unit => unit.AncillaryReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(reservation => reservation.Units).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(reservation => reservation.IdempotencyKey).IsUnique();
            builder.HasIndex(reservation => reservation.OrderId);
        }
    }
}
