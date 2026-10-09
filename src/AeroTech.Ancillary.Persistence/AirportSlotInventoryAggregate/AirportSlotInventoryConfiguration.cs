using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AirportSlotInventoryAggregate
{
    public sealed class AirportSlotInventoryConfiguration : IEntityTypeConfiguration<AirportSlotInventory>
    {
        public const string CurrentPhysicalKeyIndex = "IX_AirportSlotInventories_PhysicalKey_Current";

        public void Configure(EntityTypeBuilder<AirportSlotInventory> builder)
        {
            builder.ToTable("AirportSlotInventories");
            builder.HasKey(inventory => inventory.Id);
            builder.Property(inventory => inventory.Id).ValueGeneratedNever();
            builder.Property(inventory => inventory.Version).IsConcurrencyToken();
            builder.HasMany(inventory => inventory.Adjustments)
                .WithOne()
                .HasForeignKey(adjustment => adjustment.AirportSlotInventoryId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(inventory => inventory.Adjustments).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasIndex(inventory => new { inventory.OwnerAirlineId, inventory.FacilityId, inventory.StartUtc, inventory.EndUtc }, CurrentPhysicalKeyIndex)
                .IsUnique()
                .HasFilter($"[Status] <> {(int)InventoryRecordStatus.Retired}");
            builder.HasIndex(inventory => new { inventory.OwnerAirlineId, inventory.FacilityId, inventory.EndUtc });
            builder.HasIndex(inventory => inventory.Status);
        }
    }
}
