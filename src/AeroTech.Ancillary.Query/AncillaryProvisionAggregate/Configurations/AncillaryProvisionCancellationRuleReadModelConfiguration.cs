using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Configurations
{
    public sealed class AncillaryProvisionCancellationRuleReadModelConfiguration : IEntityTypeConfiguration<AncillaryProvisionCancellationRuleReadModel>
    {
        public void Configure(EntityTypeBuilder<AncillaryProvisionCancellationRuleReadModel> builder)
        {
            builder.ToTable("AncillaryProvisionCancellationRules");
            builder.HasKey(row => row.Id);
            builder.Property(row => row.Id).ValueGeneratedNever();
            builder.Property(row => row.RuleRef).HasMaxLength(50).IsRequired();
            builder.Property(row => row.PenaltyFixedAmount).HasPrecision(19, 6);
            builder.Property(row => row.PenaltyPercentage).HasPrecision(9, 4);
            builder.Property(row => row.SourceReference).HasMaxLength(256);
            builder.HasIndex(row => row.AncillaryProvisionId);
        }
    }
}
