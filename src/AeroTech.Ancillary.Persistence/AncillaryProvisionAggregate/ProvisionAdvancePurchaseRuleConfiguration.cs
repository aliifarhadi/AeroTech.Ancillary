using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionAdvancePurchaseRuleConfiguration : IEntityTypeConfiguration<ProvisionAdvancePurchaseRule>
    {
        public void Configure(EntityTypeBuilder<ProvisionAdvancePurchaseRule> builder)
        {
            builder.ToTable("ProvisionAdvancePurchaseRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();
        }
    }
}
