using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.FlightWeightInventoryAggregate
{
    public sealed class FlightWeightInventoryConfiguration : IEntityTypeConfiguration<FlightWeightInventory>
    {
        public const string CurrentPhysicalKeyIndex = "IX_FlightWeightInventories_PhysicalKey_Current";

        public void Configure(EntityTypeBuilder<FlightWeightInventory> builder)
        {
            builder.ToTable("FlightWeightInventories");
            builder.HasKey(inventory => inventory.Id);
            builder.Property(inventory => inventory.Id).ValueGeneratedNever();
            builder.Property(inventory => inventory.Version).IsConcurrencyToken();
            builder.Property(inventory => inventory.CapacityKg).HasPrecision(18, 3);
            builder.HasMany(inventory => inventory.Adjustments)
                .WithOne()
                .HasForeignKey(adjustment => adjustment.FlightWeightInventoryId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(inventory => inventory.Adjustments).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasIndex(inventory => new { inventory.OwnerAirlineId, inventory.FlightId, inventory.WeightResourceId }, CurrentPhysicalKeyIndex)
                .IsUnique()
                .HasFilter($"[Status] <> {(int)InventoryRecordStatus.Retired}");
            builder.HasIndex(inventory => inventory.Status);
        }
    }
}
