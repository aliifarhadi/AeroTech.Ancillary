using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Configurations
{
    public sealed class FlightWeightInventoryReadModelConfiguration : IEntityTypeConfiguration<FlightWeightInventoryReadModel>
    {
        public void Configure(EntityTypeBuilder<FlightWeightInventoryReadModel> builder)
        {
            builder.ToTable("FlightWeightInventories");
            builder.HasKey(inventory => inventory.Id);
            builder.Property(inventory => inventory.Id).ValueGeneratedNever();
            builder.Property(inventory => inventory.CapacityKg).HasPrecision(18, 3);
            builder.HasIndex(inventory => new { inventory.OwnerAirlineId, inventory.FlightId, inventory.WeightResourceId });
        }
    }
}
