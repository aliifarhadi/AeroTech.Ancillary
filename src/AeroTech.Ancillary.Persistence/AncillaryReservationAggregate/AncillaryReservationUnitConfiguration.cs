using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryReservationAggregate
{
    public sealed class AncillaryReservationUnitConfiguration : IEntityTypeConfiguration<AncillaryReservationUnit>
    {
        private const string UnitKey = "AncillaryReservationUnitId";

        public void Configure(EntityTypeBuilder<AncillaryReservationUnit> builder)
        {
            builder.ToTable("AncillaryReservationUnits");
            builder.HasKey(unit => unit.Id);
            builder.Property(unit => unit.Id).ValueGeneratedNever();

            builder.Property(unit => unit.VariantCode).HasMaxLength(8).IsRequired();
            builder.Property(unit => unit.ServiceDefinitionRef).HasMaxLength(30).IsRequired();
            builder.Property(unit => unit.SeatMapSelectionId).HasMaxLength(256);
            builder.Property(unit => unit.SeatNumber).HasMaxLength(16);
            builder.Property(unit => unit.AppendIdempotencyKey).HasMaxLength(128);
            builder.Property(unit => unit.ProviderUnitRef).HasMaxLength(100);
            builder.Property(unit => unit.CancellationReasonCode).HasMaxLength(50);
            builder.Ignore(unit => unit.CoveredFlightIds);

            builder.OwnsMany(unit => unit.Flights, flights =>
            {
                flights.ToTable("AncillaryReservationUnitFlights");
                flights.WithOwner().HasForeignKey(UnitKey);
                flights.Property(flight => flight.FlightId).ValueGeneratedNever();
                flights.HasKey(UnitKey, nameof(AncillaryReservationUnitFlight.FlightId));
            });
            builder.Navigation(unit => unit.Flights).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.OwnsOne(unit => unit.Baggage, selection =>
            {
                selection.ToTable("AncillaryReservationUnitBaggageSelections");
                selection.WithOwner().HasForeignKey(UnitKey);
                selection.Property(value => value.BagRef).HasMaxLength(64);
                selection.Property(value => value.MeasuredWeightKg).HasPrecision(18, 3);
                selection.Property(value => value.WeightKg).HasPrecision(18, 3);
                selection.Property(value => value.EquipmentKind).HasMaxLength(20);
                selection.OwnsOne(value => value.Dimensions, size =>
                {
                    size.Property(value => value.LengthCm).HasColumnName("LengthCm").HasPrecision(18, 3);
                    size.Property(value => value.WidthCm).HasColumnName("WidthCm").HasPrecision(18, 3);
                    size.Property(value => value.HeightCm).HasColumnName("HeightCm").HasPrecision(18, 3);
                });
            });

            builder.OwnsOne(unit => unit.Meal, selection =>
            {
                selection.ToTable("AncillaryReservationUnitMealSelections");
                selection.WithOwner().HasForeignKey(UnitKey);
                selection.Property(value => value.MealCode).HasMaxLength(4);
                selection.Property(value => value.MenuItemRef).HasMaxLength(40);
            });

            builder.OwnsOne(unit => unit.AssistedTravel, selection =>
            {
                selection.ToTable("AncillaryReservationUnitAssistedTravelSelections");
                selection.WithOwner().HasForeignKey(UnitKey);
                selection.Property(value => value.AssistanceSsrCode).HasMaxLength(4).IsRequired();
            });

            builder.OwnsOne(unit => unit.AirportService, selection =>
            {
                selection.ToTable("AncillaryReservationUnitAirportServiceSelections");
                selection.WithOwner().HasForeignKey(UnitKey);
            });

            builder.OwnsOne(unit => unit.Connectivity, selection =>
            {
                selection.ToTable("AncillaryReservationUnitConnectivitySelections");
                selection.WithOwner().HasForeignKey(UnitKey);
            });

            builder.OwnsOne(unit => unit.Upgrade, selection =>
            {
                selection.ToTable("AncillaryReservationUnitUpgradeSelections");
                selection.WithOwner().HasForeignKey(UnitKey);
            });

            builder.HasMany(unit => unit.Allocations)
                .WithOne()
                .HasForeignKey(allocation => allocation.AncillaryReservationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(unit => unit.Allocations).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(unit => unit.AncillaryReservationId);
            builder.HasIndex(unit => unit.OrderServiceId);
        }
    }
}
