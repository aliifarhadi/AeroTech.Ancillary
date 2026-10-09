using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Configurations
{
    public sealed class InventoryPolicyReadModelConfiguration : IEntityTypeConfiguration<InventoryPolicyReadModel>
    {
        public void Configure(EntityTypeBuilder<InventoryPolicyReadModel> builder)
        {
            builder.ToTable("AncillaryInventoryPolicies");
            builder.HasKey(policy => policy.Id);
            builder.Property(policy => policy.Id).ValueGeneratedNever();
            builder.Property(policy => policy.ServiceDefinitionRef).HasMaxLength(30).IsRequired();
            builder.Property(policy => policy.ProviderKey).HasMaxLength(50);
            builder.Property(policy => policy.WeightFixedKgPerUnit).HasPrecision(18, 3);
            builder.HasIndex(policy => new { policy.OwnerAirlineId, policy.ServiceDefinitionRef, policy.Status });
        }
    }
}
