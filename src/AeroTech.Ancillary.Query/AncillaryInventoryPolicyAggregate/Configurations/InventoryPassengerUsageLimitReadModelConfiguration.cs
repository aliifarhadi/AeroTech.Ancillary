using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Configurations
{
    public sealed class InventoryPassengerUsageLimitReadModelConfiguration : IEntityTypeConfiguration<InventoryPassengerUsageLimitReadModel>
    {
        public void Configure(EntityTypeBuilder<InventoryPassengerUsageLimitReadModel> builder)
        {
            builder.ToTable("AncillaryInventoryPassengerUsageLimits");
            builder.HasKey(limit => limit.Id);
            builder.Property(limit => limit.Id).ValueGeneratedNever();
            builder.Property(limit => limit.CountingFamilyCode).HasMaxLength(30).IsRequired();
            builder.HasIndex(limit => limit.InventoryPolicyId);
        }
    }
}
