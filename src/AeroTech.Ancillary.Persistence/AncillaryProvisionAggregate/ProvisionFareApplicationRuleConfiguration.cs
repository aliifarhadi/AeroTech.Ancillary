using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate
{
    public sealed class ProvisionFareApplicationRuleConfiguration : IEntityTypeConfiguration<ProvisionFareApplicationRule>
    {
        public void Configure(EntityTypeBuilder<ProvisionFareApplicationRule> builder)
        {
            builder.ToTable("ProvisionFareApplicationRules");
            builder.HasKey(rule => rule.Id);
            builder.Property(rule => rule.Id).ValueGeneratedNever();
            builder.HasMany(rule => rule.AirFares)
                .WithOne()
                .HasForeignKey(row => row.ProvisionFareApplicationRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.AirFares).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.AirFareTypes)
                .WithOne()
                .HasForeignKey(row => row.ProvisionFareApplicationRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.AirFareTypes).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.FareFamilies)
                .WithOne()
                .HasForeignKey(row => row.ProvisionFareApplicationRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.FareFamilies).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.FareBases)
                .WithOne()
                .HasForeignKey(row => row.ProvisionFareApplicationRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.FareBases).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.CabinClasses)
                .WithOne()
                .HasForeignKey(row => row.ProvisionFareApplicationRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.CabinClasses).UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasMany(rule => rule.Rbds)
                .WithOne()
                .HasForeignKey(row => row.ProvisionFareApplicationRuleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            builder.Navigation(rule => rule.Rbds).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
