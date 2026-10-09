using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Configurations
{
    public sealed class FlightCountInventoryReadModelConfiguration : IEntityTypeConfiguration<FlightCountInventoryReadModel>
    {
        public void Configure(EntityTypeBuilder<FlightCountInventoryReadModel> builder)
        {
            builder.ToTable("FlightCountInventories");
            builder.HasKey(inventory => inventory.Id);
            builder.Property(inventory => inventory.Id).ValueGeneratedNever();
            builder.HasIndex(inventory => new { inventory.OwnerAirlineId, inventory.FlightId, inventory.ResourceId });
        }
    }
}
