using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Configurations
{
    public sealed class FlightCountAdjustmentReadModelConfiguration : IEntityTypeConfiguration<FlightCountAdjustmentReadModel>
    {
        public void Configure(EntityTypeBuilder<FlightCountAdjustmentReadModel> builder)
        {
            builder.ToTable("FlightCountAdjustments");
            builder.HasKey(adjustment => adjustment.Id);
            builder.Property(adjustment => adjustment.Id).ValueGeneratedNever();
            builder.Property(adjustment => adjustment.ReasonCode).HasMaxLength(50).IsRequired();
            builder.Property(adjustment => adjustment.CorrelationId).HasMaxLength(64).IsRequired();
            builder.HasIndex(adjustment => adjustment.FlightCountInventoryId);
        }
    }
}
