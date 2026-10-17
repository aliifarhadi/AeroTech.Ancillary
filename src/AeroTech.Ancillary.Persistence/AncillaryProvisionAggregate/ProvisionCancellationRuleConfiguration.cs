using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionCancellationRuleConfiguration : IEntityTypeConfiguration<ProvisionCancellationRule>
    {
        public void Configure(EntityTypeBuilder<ProvisionCancellationRule> builder)
        {
            builder.ToTable("ProvisionCancellationRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();
            builder.Property(rule => rule.RuleRef).HasMaxLength(50).IsRequired();
            builder.Property(rule => rule.PenaltyFixedAmount).HasPrecision(19, 6);
            builder.Property(rule => rule.PenaltyPercentage).HasPrecision(9, 4);
            builder.Property(rule => rule.SourceReference).HasMaxLength(256);
            builder.HasIndex(rule => new { rule.AncillaryProvisionId, rule.RuleRef }).IsUnique();
        }
    }
}
