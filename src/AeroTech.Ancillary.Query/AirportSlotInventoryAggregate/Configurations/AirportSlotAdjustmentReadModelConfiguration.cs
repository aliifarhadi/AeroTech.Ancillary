using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Configurations
{
    public sealed class AirportSlotAdjustmentReadModelConfiguration : IEntityTypeConfiguration<AirportSlotAdjustmentReadModel>
    {
        public void Configure(EntityTypeBuilder<AirportSlotAdjustmentReadModel> builder)
        {
            builder.ToTable("AirportSlotAdjustments");
            builder.HasKey(adjustment => adjustment.Id);
            builder.Property(adjustment => adjustment.Id).ValueGeneratedNever();
            builder.Property(adjustment => adjustment.ReasonCode).HasMaxLength(50).IsRequired();
            builder.Property(adjustment => adjustment.CorrelationId).HasMaxLength(64).IsRequired();
            builder.HasIndex(adjustment => adjustment.AirportSlotInventoryId);
        }
    }
}
