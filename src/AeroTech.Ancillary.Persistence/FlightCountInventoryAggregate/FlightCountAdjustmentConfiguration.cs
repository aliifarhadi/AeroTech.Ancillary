using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.FlightCountInventoryAggregate
{
    public sealed class FlightCountAdjustmentConfiguration : IEntityTypeConfiguration<FlightCountAdjustment>
    {
        public void Configure(EntityTypeBuilder<FlightCountAdjustment> builder)
        {
            builder.ToTable("FlightCountAdjustments");
            builder.HasKey(adjustment => adjustment.Id);
            builder.Property(adjustment => adjustment.Id).ValueGeneratedNever();
            builder.Property(adjustment => adjustment.ReasonCode).HasMaxLength(50).IsRequired();
            builder.Property(adjustment => adjustment.CorrelationId).HasMaxLength(64).IsRequired();
            builder.HasIndex(adjustment => new { adjustment.FlightCountInventoryId, adjustment.CorrelationId }).IsUnique();
        }
    }
}
