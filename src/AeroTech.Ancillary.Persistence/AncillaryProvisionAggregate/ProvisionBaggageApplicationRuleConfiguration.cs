using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionBaggageApplicationRuleConfiguration : IEntityTypeConfiguration<ProvisionBaggageApplicationRule>
    {
        public void Configure(EntityTypeBuilder<ProvisionBaggageApplicationRule> builder)
        {
            builder.ToTable("ProvisionBaggageApplicationRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();
            builder.Property(rule => rule.Weight).HasPrecision(9, 2);
        }
    }
}
