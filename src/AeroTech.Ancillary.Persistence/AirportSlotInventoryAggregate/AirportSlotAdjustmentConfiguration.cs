using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AirportSlotInventoryAggregate
{
    public sealed class AirportSlotAdjustmentConfiguration : IEntityTypeConfiguration<AirportSlotAdjustment>
    {
        public void Configure(EntityTypeBuilder<AirportSlotAdjustment> builder)
        {
            builder.ToTable("AirportSlotAdjustments");
            builder.HasKey(adjustment => adjustment.Id);
            builder.Property(adjustment => adjustment.Id).ValueGeneratedNever();
            builder.Property(adjustment => adjustment.ReasonCode).HasMaxLength(50).IsRequired();
            builder.Property(adjustment => adjustment.CorrelationId).HasMaxLength(64).IsRequired();
            builder.HasIndex(adjustment => new { adjustment.AirportSlotInventoryId, adjustment.CorrelationId }).IsUnique();
        }
    }
}
