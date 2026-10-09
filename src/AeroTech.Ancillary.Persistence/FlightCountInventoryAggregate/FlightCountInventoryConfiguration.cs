using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.FlightCountInventoryAggregate
{
    public sealed class FlightCountInventoryConfiguration : IEntityTypeConfiguration<FlightCountInventory>
    {
        public const string CurrentPhysicalKeyIndex = "IX_FlightCountInventories_PhysicalKey_Current";

        public void Configure(EntityTypeBuilder<FlightCountInventory> builder)
        {
            builder.ToTable("FlightCountInventories");
            builder.HasKey(inventory => inventory.Id);
            builder.Property(inventory => inventory.Id).ValueGeneratedNever();
            builder.Property(inventory => inventory.Version).IsConcurrencyToken();
            builder.HasMany(inventory => inventory.Adjustments)
                .WithOne()
                .HasForeignKey(adjustment => adjustment.FlightCountInventoryId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(inventory => inventory.Adjustments).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasIndex(inventory => new { inventory.OwnerAirlineId, inventory.FlightId, inventory.ResourceId }, CurrentPhysicalKeyIndex)
                .IsUnique()
                .HasFilter($"[Status] <> {(int)InventoryRecordStatus.Retired}");
            builder.HasIndex(inventory => new { inventory.OwnerAirlineId, inventory.ResourceId });
            builder.HasIndex(inventory => inventory.Status);
        }
    }
}
