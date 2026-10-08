using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionDayTimeApplicationRuleConfiguration : IEntityTypeConfiguration<ProvisionDayTimeApplicationRule>
    {
        public void Configure(EntityTypeBuilder<ProvisionDayTimeApplicationRule> builder)
        {
            builder.ToTable("ProvisionDayTimeApplicationRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();
            builder.HasMany(rule => rule.Windows)
                .WithOne()
                .HasForeignKey(row => row.ProvisionDayTimeApplicationRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.Windows).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
