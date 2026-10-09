using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionAssistedTravelRuleConfiguration : IEntityTypeConfiguration<ProvisionAssistedTravelRule>
    {
        public void Configure(EntityTypeBuilder<ProvisionAssistedTravelRule> builder)
        {
            builder.ToTable("ProvisionAssistedTravelRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();
        }
    }
}
