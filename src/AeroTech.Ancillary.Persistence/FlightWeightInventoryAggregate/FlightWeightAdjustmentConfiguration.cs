using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.FlightWeightInventoryAggregate
{
    public sealed class FlightWeightAdjustmentConfiguration : IEntityTypeConfiguration<FlightWeightAdjustment>
    {
        public void Configure(EntityTypeBuilder<FlightWeightAdjustment> builder)
        {
            builder.ToTable("FlightWeightAdjustments");
            builder.HasKey(adjustment => adjustment.Id);
            builder.Property(adjustment => adjustment.Id).ValueGeneratedNever();
            builder.Property(adjustment => adjustment.ReasonCode).HasMaxLength(50).IsRequired();
            builder.Property(adjustment => adjustment.CorrelationId).HasMaxLength(64).IsRequired();
            builder.Property(adjustment => adjustment.PreviousKg).HasPrecision(18, 3);
            builder.Property(adjustment => adjustment.NewKg).HasPrecision(18, 3);
            builder.HasIndex(adjustment => new { adjustment.FlightWeightInventoryId, adjustment.CorrelationId }).IsUnique();
        }
    }
}
