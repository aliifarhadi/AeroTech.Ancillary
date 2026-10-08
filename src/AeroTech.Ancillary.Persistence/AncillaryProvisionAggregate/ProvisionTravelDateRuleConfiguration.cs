using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionTravelDateRuleConfiguration : IEntityTypeConfiguration<ProvisionTravelDateRule>
    {
        public void Configure(EntityTypeBuilder<ProvisionTravelDateRule> builder)
        {
            builder.ToTable("ProvisionTravelDateRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();
            builder.HasMany(rule => rule.PermittedPeriods)
                .WithOne()
                .HasForeignKey(row => row.ProvisionTravelDateRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.PermittedPeriods).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.BlackoutPeriods)
                .WithOne()
                .HasForeignKey(row => row.ProvisionTravelDateRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.BlackoutPeriods).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
