using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Configurations
{
    public sealed class AirportSlotInventoryReadModelConfiguration : IEntityTypeConfiguration<AirportSlotInventoryReadModel>
    {
        public void Configure(EntityTypeBuilder<AirportSlotInventoryReadModel> builder)
        {
            builder.ToTable("AirportSlotInventories");
            builder.HasKey(inventory => inventory.Id);
            builder.Property(inventory => inventory.Id).ValueGeneratedNever();
            builder.HasIndex(inventory => new { inventory.OwnerAirlineId, inventory.FacilityId, inventory.StartUtc });
        }
    }
}
