using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryInventoryPolicyAggregate
{
    public sealed class PassengerUsageLimitConfiguration : IEntityTypeConfiguration<PassengerUsageLimit>
    {
        public void Configure(EntityTypeBuilder<PassengerUsageLimit> builder)
        {
            builder.ToTable("InventoryPassengerUsageLimits");
            builder.HasKey(limit => limit.Id);
            builder.Property(limit => limit.Id).ValueGeneratedNever();
            builder.Property(limit => limit.CountingFamilyCode).HasMaxLength(30).IsRequired();
            builder.HasIndex(limit => new { limit.InventoryPolicyId, limit.LimitScope }).IsUnique();
        }
    }
}
